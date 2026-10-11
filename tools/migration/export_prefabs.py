"""Export the prefabs the shipped code loads by name, as hierarchy descriptions the Editor can build.

Which roots, decided by the referring side: `scan_prefabs.py` found the 63 Resources.Load keys, and
38 of them want a GameObject. Those keys resolve to 102 root GameObjects (33 whole-key hits plus 65
numbered family members like Stage_dun1). For each key the candidate roots are matched by
GameObject.m_Name inside a non-level container; 99 keys have exactly one candidate and are exported,
3 do not (`angel_splash5`, `angel_splash8`, `wp_1` each have two or more DISTINCT containers/objects
carrying that name) and are written to the report as pending, not guessed. mainData's shipped
resource index would be the right oracle for those three and is not readable offline far enough --
see tools/migration/dump_maindata_resource_index.py and the notes in README gap 8.

The node shape is extract_scene.py's, unchanged: the same FileCtx resolves components, decoded
MonoBehaviour fields, meshes, materials and clip paths, because a second copy of that resolution is
how the scene pass and the ledger resolver once disagreed about 63 dropped references.

What a prefab adds, and what is therefore done here and not in extract_scene:
  * the subtree under the root Transform is selected; the root's parent is forced to 0;
  * a `node`-typed field whose target is outside the subtree is dropped and counted -- a scene can
    hold a reference to any object in its own file, an asset cannot;
  * SkinnedMeshRenderer bone lists are cut to the subtree for the same reason.

Emits:
  tools/migration/prefabs/<name>.json      one description per load-key root (Assets/Resources)
  tools/migration/prefab-templates/<name>.json  ... per template root (Assets/Imported/Prefabs)
  tools/migration/prefabs-export.json      tally, the pending list, and per-root counts
  tools/migration/prefab-templates.json    container!path_id -> the asset that now holds it, which
                                           is how extract_scene turns a dropped cross-file
                                           reference into a reference into a prefab

Run:  <unitypy venv python> tools/migration/export_prefabs.py [--dry-run] [--with-templates]
                             [--only NAME[,NAME]]   RECON_DIR=<path>
`--only` is a pilot switch: it exports just those template roots, leaves the aggregate report and
the other descriptions alone, and merges rather than rewrites the index.
"""
import argparse
import collections
import glob
import json
import os
import re
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import extract_scene as ES
import unity4_files as u4

_REPO = ES._REPO
OUT_DIR = os.path.join(_REPO, "tools", "migration", "prefabs")
OUT_REPORT = os.path.join(_REPO, "tools", "migration", "prefabs-export.json")
SURVEY = os.path.join(_REPO, "tools", "migration", "prefabs-survey.json")
CONTENTS = os.path.join(_REPO, "tools", "migration", "prefab-contents.json")
MIG = os.path.join(_REPO, "tools", "migration")
load_json = ES.load_json
# Scratch scenes the Editor builds into, then deletes. Under Assets so SaveScene accepts it, but
# never committed -- see the docstring's flow.
BUILD_SCENE_DIR = "Assets/Imported/_PrefabBuild"

BAD = re.compile(r'[<>:"/\\|?*\x00-\x1f]')


def asset_refs(o):
    """Yield (asset_path, declared_type) for every "asset" path anywhere in a document.

    A plain recursive walk rather than a typed parse, because the references are not all at one
    depth or one shape: an Animation component's m_Clips binding, a scalar field like
    Cha_Weapon.bowgun, and an array element like Cha_Control.attack_dual[0] sit three levels apart,
    and only the latter two carry kind == "asset".
    """
    if isinstance(o, dict):
        p = o.get("asset")
        if isinstance(p, str) and p.startswith("Assets/"):
            # A binding carries no declared type but is by definition a clip; a field carries "t".
            yield p, o.get("t") or ("AnimationClip" if "clip_id" in o else "?")
        for v in o.values():
            for r in asset_refs(v):
                yield r
    elif isinstance(o, list):
        for v in o:
            for r in asset_refs(v):
                yield r

# The second kind of prefab: a hierarchy root that lives in a container and is referenced by a
# scene's MonoBehaviour field as an Instantiate template, but is NOT named by any Resources.Load key.
# Their names and root ids come from dropped-refs.json (scan_dropped_refs.py resolves the roots).
# Measured at first: 97 candidate roots, zero name collisions among themselves, one name (wp_horse)
# already exported as a load-key prefab, so 96 were exported. The ledger grew to 99 when the
# MonoBehaviour field decode found three more (ef_stepfog, ef_thrustfog, num_level) -- every wider
# decoder uncovers references this list had never seen. The hierarchies are NOT all single objects --
# sizes run from 1 to 42 objects -- so the index covers each subtree's own ids, see the note in the
# loop.
DROPPED = os.path.join(_REPO, "tools", "migration", "dropped-refs.json")
TPL_OUT_DIR = os.path.join(_REPO, "tools", "migration", "prefab-templates")
# Deliberately NOT under Resources/: not one of these roots appears among the 299 rows of the
# shipped resource index, so no call site can load them by name, and giving two assets the same
# resource path is an ambiguous load rather than a fix.
TPL_ASSET_DIR = "Assets/Imported/Prefabs"
TPL_BUILD_SCENE_DIR = "Assets/Imported/_TemplateBuild"
TEMPLATE_INDEX = os.path.join(_REPO, "tools", "migration", "prefab-templates.json")


def template_roots(only=None):
    """{name: (container, root GameObject path_id)} for the Instantiate-template roots.

    Two sources, unioned. The ledger (dropped-refs.json) is where a root is DISCOVERED, but it is
    not a list to export from: once a run publishes the index, the next extract resolves those
    fields and they stop being dropped, so the ledger empties itself out from under this tool -- and
    a second run would then report zero template roots and the stale sweep would delete the 96
    descriptions the first run wrote. The published index is therefore the durable list, and the
    ledger only adds roots not yet in it.

    Returns None to refuse: a name claimed by two different objects would mean two prefabs wanting
    one file, which is the same ambiguity the texture pass already got burned by.
    """
    out = {}
    # Seed from the descriptions this tool already wrote: each one names its own container and root
    # object, so the list survives the ledger emptying out above.
    if os.path.isdir(TPL_OUT_DIR):
        for fn in sorted(os.listdir(TPL_OUT_DIR)):
            if not fn.endswith(".json"):
                continue
            d = load_json(os.path.join(TPL_OUT_DIR, fn)) or {}
            if d.get("source_container") and d.get("root_path_id") is not None:
                out[d.get("prefab_name") or fn[:-5]] = (d["source_container"], d["root_path_id"])
    seeded = len(out)
    doc = load_json(DROPPED)
    if doc is None and not seeded:
        print("no dropped-refs.json and no %s -- nothing can name the template roots; run "
              "scan_dropped_refs.py first" % os.path.basename(TEMPLATE_INDEX))
        return None
    for r in (doc or {}).get("resolved", []):
        if r.get("tgt_type") != "Transform" or r.get("root_object") is None or not r.get("root"):
            continue
        name, key = r["root"], (r["container"], r["root_object"])
        if name in out and out[name] != key:
            print("REFUSING: template root name %s is claimed by %s and %s" % (name, out[name], key))
            return None
        out[name] = key
    if only:
        want = [n.strip() for n in only.split(",") if n.strip()]
        miss = sorted(n for n in want if n.lower() not in {k.lower() for k in out})
        if miss:
            print("REFUSING: --only names that are not template roots: %s" % miss)
            return None
        out = {k: v for k, v in out.items() if k.lower() in {n.lower() for n in want}}
    if seeded:
        print("template roots: %d carried by the published index, plus any new ones in the ledger"
              % seeded)
    return out


def wanted_roots():
    """{name: [(container, path_id)]} for the GameObject assets the shipped code asks for.

    A whole key contributes under its own name; a concatenation head contributes each of its
    numbered members, because those are the names the runtime can actually produce.
    """
    doc = json.load(open(SURVEY, encoding="utf-8"))
    out = {}

    def add(name, pairs):
        seen = out.setdefault(name, [])
        for p in pairs:
            if tuple(p) not in seen:
                seen.append(tuple(p))

    for key, v in doc["verdicts"].items():
        if v.get("kind") == "prefab":
            add(key, v.get("objects") or [])
        elif v.get("kind") == "family":
            for member, objs in (v.get("prefab_objects") or {}).items():
                add(member, objs)
    return out


def subtree_ids(nodes, root_id):
    """Transform ids under (and including) root_id, following the children links."""
    by_id = {n["id"]: n for n in nodes}
    seen, stack = set(), [root_id]
    while stack:
        i = stack.pop()
        if i in seen or i not in by_id:
            continue
        seen.add(i)
        stack.extend(by_id[i]["children"])
    return seen


def cut_to_subtree(nodes, keep, stats):
    """Keep only subtree nodes, and drop the references that cannot live inside an asset."""
    out = []
    for n in nodes:
        if n["id"] not in keep:
            continue
        rec = dict(n)
        comps = []
        for c in n["components"]:
            c = dict(c)
            if "bones" in c:
                inside = [b for b in c["bones"] if b in keep]
                if len(inside) != len(c["bones"]):
                    stats["bone outside subtree dropped"] += len(c["bones"]) - len(inside)
                c["bones"] = inside
                if c.get("root_bone") and c["root_bone"] not in keep:
                    c["root_bone"] = 0
                    stats["root bone outside subtree cleared"] += 1
            if c.get("fields"):
                keep_fields = []
                for f in c["fields"]:
                    if f.get("kind") == "node" and f.get("node") not in keep:
                        stats["node ref outside subtree dropped"] += 1
                        continue
                    if f.get("kind") == "arr":
                        items = []
                        for e in f.get("items") or []:
                            if e.get("kind") == "node" and e.get("node") not in keep:
                                stats["node ref outside subtree dropped"] += 1
                                continue
                            items.append(e)
                        if not items:
                            stats["array fully outside subtree"] += 1
                            continue
                        f = dict(f, items=items)
                    if f.get("kind") == "obj":
                        subs = [s for s in (f.get("fields") or [])
                                if not (s.get("kind") == "node" and s.get("node") not in keep)]
                        if len(subs) != len(f.get("fields") or []):
                            stats["node ref outside subtree dropped"] += 1
                        f = dict(f, fields=subs)
                    keep_fields.append(f)
                if keep_fields:
                    c["fields"] = keep_fields
                else:
                    c.pop("fields", None)
            comps.append(c)
        rec["components"] = comps
        if rec["parent"] not in keep:
            rec["parent"] = 0
        out.append(rec)
    return out


def histogram(nodes):
    """Component counts including Transform, so the number is comparable with the independent
    measurement in prefab-contents.json (extract_scene keys nodes by Transform and does not carry
    the Transform as a component). A MonoBehaviour is counted under its class of component, not
    under its script name, because the measurement pass labelled it by type."""
    c = collections.Counter({"Transform": len(nodes)})
    for n in nodes:
        for comp in n["components"]:
            if comp["kind"] == "MonoBehaviour":
                c["MonoBehaviour"] += 1
            else:
                c[comp["type"]] += 1
    return c


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--verbose", action="store_true")
    ap.add_argument("--with-templates", action="store_true",
                    help="also export the Instantiate-template roots from dropped-refs.json")
    ap.add_argument("--only", metavar="NAME[,NAME]",
                    help="templates: export only these root names (pilot runs); implies "
                         "--with-templates and skips deleting stale descriptions")
    args = ap.parse_args()

    roots = wanted_roots()
    if args.only:
        # A pilot run: the load-key roots are already exported and committed, and re-running them
        # here would rewrite 97 descriptions plus the aggregate report to prove nothing. Drop them
        # and keep only the named template roots.
        roots = {}
        print("PILOT: --only given, so the load-key roots are skipped for this run")
    # mainData's shipped resource index decides a root when it claims one object under that exact
    # key: a row gives (container, path_id), and the test here is that the row's own verdict is
    # "exact" (the object there really carries the name) or that no other row claims the rival
    # candidates. Measured on this build: angel_splash8 and wp_1 pass the name test, and
    # angel_splash5's four same-named siblings are pointed at by NO row at all, while pid 1 is.
    # Name matching stays as the fallback, and every root records which of the two decided it.
    index_roots, index_any, claimed = {}, {}, collections.defaultdict(list)
    rpath = os.path.join(_REPO, "tools", "migration", "maindata-resource-index.json")
    if os.path.exists(rpath):
        rdoc = json.load(open(rpath, encoding="utf-8"))
        for r in rdoc["records"]:
            if r.get("listed_container"):
                claimed[(r["listed_container"], r["b"])].append(r["path"])
                index_any.setdefault(r["path"].lower(), (r["listed_container"], r["b"]))
                if r["verdict"] == "exact":
                    index_roots.setdefault(r["path"].lower(),
                                           (r["listed_container"], r["b"]))
    unique, pending, decided = {}, {}, {}
    for k, v in roots.items():
        if len(v) == 1:
            # A unique name hit stands on its own. The index is not allowed to override it: it has
            # already been seen pointing at a child object (wp_b01 -> sharedassets18!1639, whose
            # Transform has a father), so where the name match is unambiguous it is the safer judge.
            unique[k], decided[k] = v[0], "only one candidate by name"
            continue
        pick = index_roots.get(k.lower())
        if pick and pick in v:
            unique[k], decided[k] = pick, "mainData index, self-agrees on the name"
            continue
        row = index_any.get(k.lower())
        if row and all(claimed.get(c, []) == [] for c in v):
            # The row names this asset at an object the name search never proposed, and none of the
            # same-named candidates is claimed by any row at all -- angel_splash5's four siblings.
            unique[k], decided[k] = row, "mainData index; every same-named candidate is unclaimed"
        else:
            pending[k] = v
    print("GameObject keys: %d, roots decided: %d, still ambiguous (not guessed): %d"
          % (len(roots), len(unique), len(pending)))
    for k in sorted(decided):
        if decided[k].startswith("mainData index"):
            print("   by index  %-16s %s!%d  -- %s" % (k, decided[k] and unique[k][0][:14],
                                                       unique[k][1], decided[k]))
    for k, v in sorted(pending.items()):
        print("   pending %-16s candidates %s" % (k, v))

    names = {k: BAD.sub("_", k).strip(". ") for k in unique}
    lower = collections.Counter(n.lower() for n in names.values())
    dup = [n for n, c in lower.items() if c > 1]
    if dup:
        print("REFUSING: two roots would be written as the same Resources name: %s" % dup[:6])
        return 2
    res_dir = os.path.join(_REPO, "unity", "Assets", "Resources")
    # Clash detection is about two DIFFERENT kinds of asset ending up on one resource path -- that is
    # what made 18 textures and 94 prefabs fight over the same names. A .prefab already sitting there
    # is this exporter's own output from a previous Editor run, and refusing it would make the
    # description impossible to regenerate after building once.
    on_disk = {os.path.splitext(f)[0].lower() for f in os.listdir(res_dir)
               if not f.endswith(".meta") and not f.endswith(".prefab")}
    hit = sorted(n for n in names.values() if n.lower() in on_disk)
    if hit:
        print("REFUSING: %d prefab name(s) collide with a non-prefab Resources asset, which would "
              "make Resources.Load('%s') ambiguous" % (len(hit), hit[0]))
        return 2

    # Where each root's description and asset go. Load keys are Resources prefabs; template roots are
    # Imported/Prefabs, and the two kinds never share a directory or a name space.
    folders = {k: ("Assets/Resources", OUT_DIR, BUILD_SCENE_DIR) for k in unique}
    if args.with_templates or args.only:
        tpl = template_roots(args.only)
        if tpl is None:
            return 2
        exported_as_keys = set(unique.values())
        added, already = 0, 0
        for name in sorted(tpl):
            key = tpl[name]
            if key in exported_as_keys:
                # wp_horse: a load-key export already produces exactly this root, so the scene
                # reference resolves to that asset instead of a duplicate copy of it.
                already += 1
                continue
            if name in unique:
                print("REFUSING: template root %s would collide with a load-key prefab of the "
                      "same name but a different object" % name)
                return 2
            unique[name] = key
            decided[name] = "hierarchy root named by dropped-refs (no Resources.Load key)"
            names[name] = BAD.sub("_", name).strip(". ")
            folders[name] = (TPL_ASSET_DIR, TPL_OUT_DIR, TPL_BUILD_SCENE_DIR)
            added += 1
        dup2 = [n for n, c in collections.Counter(
            names[k].lower() for k in unique if folders[k][1] == TPL_OUT_DIR).items() if c > 1]
        if dup2:
            print("REFUSING: two template roots would be written to the same file: %s" % dup2[:4])
            return 2
        print("template roots asked %d, added %d under %s, %d already exported as a load key"
              % (len(tpl), added, TPL_ASSET_DIR, already))

    by_container = collections.defaultdict(list)
    for key, (cont, pid) in unique.items():
        by_container[cont].append((key, pid))
    print("containers to open: %d" % len(by_container))

    contents = json.load(open(CONTENTS, encoding="utf-8")) if os.path.exists(CONTENTS) else None
    srcs = dict(u4.sources())
    stats = collections.Counter()
    report, missing, not_roots = {}, {}, {}
    for cont in sorted(by_container):
        path = srcs.get(cont)
        if path is None:
            print("REFUSING: container %s is not among u4.sources(), so nothing can be exported "
                  "for the keys stored in it" % cont)
            return 2
        env = UnityPy.load(path)
        ctx = ES.FileCtx(cont, env)
        nodes, unsupported = ctx.build_nodes()
        idx = ES.indexes()
        if idx["clip_index"] and idx["survey"]:
            ES.bind_animations(nodes, idx["clip_index"]["clips"],
                               idx["survey"].get("script_states", {}), ctx.warnings,
                               collections.Counter())
        by_go = {n["go_id"]: n for n in nodes}
        for key, pid in sorted(by_container[cont]):
            root = by_go.get(pid)
            if root is None:
                missing.append((key, "%s!%d" % (cont, pid)))
                continue
            if root["parent"] not in (0, None):
                # The registered object sits inside another hierarchy (7 of the wp_b* weapon keys
                # and angel_splash5 look like this). That is not an error to refuse: the shipped
                # build hands Resources.Load exactly this object, and instantiating it yields just
                # this subtree, which is what cut_to_subtree + parent=0 reproduces. Recorded so the
                # oddity stays visible instead of being smoothed away.
                not_roots[key] = "%s!%d father=%s" % (cont, pid, root["parent"])
            keep = subtree_ids(nodes, root["id"])
            kept = cut_to_subtree(nodes, keep, stats)
            hist = histogram(kept)
            if contents:
                claim = contents.get("per_root", {}).get("%s!%d" % (cont, pid), {}).get("components")
                if claim is not None and dict(hist) != claim:
                    print("MISMATCH %s: exporter sees %s, prefab-contents.json measured %s"
                          % (key, dict(hist), claim))
                    return 2
            # Every object id a scene PPtr could name INSIDE this prefab: each node's GameObject, its
            # Transform, and every component on that GameObject. The shipped references point at
            # whatever the field actually held -- bowgun01 names Transform 437, not the root 332 --
            # and these hierarchies run to 42 objects (bosssp), so indexing only the root would leave
            # every reference into a child dropping. (An earlier census of mine reported all 97 roots
            # as single objects; that walk only counted GameObjects while m_Children yields Transform
            # ids, so it skipped every child it met. subtree_ids is the tested walk, and this uses it.)
            ids = set()
            for n in kept:
                ids.add(n["id"])
                go = ctx.objs.get(n.get("go_id"))
                if n.get("go_id"):
                    ids.add(n["go_id"])
                if go is not None:
                    for _cid, pp in u4.component_pairs(go.read()):
                        p2 = getattr(pp, "m_PathID", None)
                        if p2:
                            ids.add(p2)
            payload = {
                "registered_object_parent": not_roots.get(key),
                "source_container": cont,
                "root_path_id": pid,
                "prefab_name": key,
                "asset_path": "%s/%s.prefab" % (folders[key][0], names[key]),
                # The Editor builds a prefab by running the ONE proven node builder
                # (SceneRebuilder.Rebuild) into a throwaway scene and saving its root. scene_path is
                # that scratch file; PrefabRebuilder deletes it after SaveAsPrefabAsset. Naming it
                # here instead of in the C# keeps one definition of where build artifacts live.
                "scene_path": folders[key][2] + "/%s.unity" % names[key],
                "node_count": len(kept),
                "root_ids": [root["id"]],
                "unsupported_components": dict(unsupported),
                "field_stats": dict(ctx.field_stats),
                "container_dropped_refs": list(ctx.dropped_refs),
                "container_warnings": list(ctx.warnings),
                "nodes": kept,
            }
            report[key] = {"container": cont, "path_id": pid, "decided_by": decided[key],
                           "assets": payload["asset_path"], "objects": len(kept),
                           "object_ids": sorted(ids), "components": dict(hist)}
            if not args.dry_run:
                out_dir = folders[key][1]
                os.makedirs(out_dir, exist_ok=True)
                safe = names[key]
                tmp = os.path.join(out_dir, safe + ".json.tmp")
                with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
                    json.dump(payload, fh, indent=1, ensure_ascii=False)
                os.replace(tmp, os.path.join(out_dir, safe + ".json"))
            stats["exported"] += 1

    if missing:
        print("REFUSING to call this a run: %d root GameObject(s) produced no node at all, e.g. %s"
              % (len(missing), missing[:3]))
        return 2
    # Every asset path the scenes and descriptions now reference has to exist under unity/ by the
    # time the Editor builds, because a reference to a missing asset lands as null and the only
    # trace is a warning buried in a Console dump. The first version of this guard looked only at
    # nodes[].animations of the descriptions it had just written, so it printed "0" the whole way
    # through the run that put 65 Cha_Control.attack_* AnimationClip array elements into Stage --
    # the referring side is not only the Animation component, and array elements are a third shape.
    built = len(glob.glob(os.path.join(_REPO, "unity", "Assets", "Imported", "Animations",
                                       "*", "*.anim")))
    total = len((load_json(os.path.join(MIG, "animation-index.json")) or {}).get("clips", []))
    absent = collections.defaultdict(set)
    scanned = 0
    for d in [os.path.join(MIG, "scenes"), os.path.join(MIG, "prefabs"), TPL_OUT_DIR]:
        for fn in sorted(glob.glob(os.path.join(d, "*.json"))):
            scanned += 1
            for p, t in asset_refs(load_json(fn) or {}):
                if not os.path.exists(os.path.join(_REPO, "unity", p.replace("/", os.sep))):
                    absent[t].add(p)
    if absent:
        print("MISSING ASSET: %d referenced asset path(s) do not exist on disk, from "
              "%d scene/prefab description file(s) scanned"
              % (sum(len(v) for v in absent.values()), scanned))
        for t, ps in sorted(absent.items(), key=lambda x: -len(x[1])):
            print("   %-16s %d   e.g. %s" % (t, len(ps), sorted(ps)[0]))
        print("   the .anim ones are the clip pass's job: run Undead Slayer/Build AnimationClips "
              "From Descriptors, then build prefabs and rebuild the scenes again")
    else:
        print("asset references: every path in %d description file(s) exists under unity/ "
              "(the clip pass has produced %d of the build's %d AnimationClips)"
              % (scanned, built, total))
    print("exported %d prefabs, %d objects total (%s cut for references outside a subtree)"
          % (stats["exported"], sum(r["objects"] for r in report.values()),
             {k: v for k, v in stats.items() if k != "exported"} or "nothing"))
    if not_roots:
        print("registered objects that are not hierarchy roots (exported as their own subtree, "
              "parent cleared): %d" % len(not_roots))
        for k, why in sorted(not_roots.items()):
            print("   %-16s %s" % (k, why))
    doc = {"tally": {"exported": stats["exported"], "pending_ambiguous": len(pending)},
           "registered_object_not_a_root": not_roots,
           "pending": {k: ["%s!%d" % c for c in v] for k, v in sorted(pending.items())},
           "cut_stats": {k: v for k, v in stats.items() if k != "exported"},
           # Recomputed at the end of the run, over the scenes as well as the descriptions, so the
           # report cannot list "no gaps" while the scene files still point at unbuilt assets.
           "referenced_assets_missing_on_disk": {t: sorted(ps)
                                                 for t, ps in sorted(absent.items())},
           "prefabs": report,
           "decided_by": {k: decided[k] for k in sorted(decided)}}
    if args.dry_run:
        print("dry run -- nothing written")
        return 0
    if not args.only:
        tmp = OUT_REPORT + ".tmp"
        with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
            json.dump(doc, fh, indent=1, ensure_ascii=False, sort_keys=True)
        os.replace(tmp, OUT_REPORT)
        back = json.load(open(OUT_REPORT, encoding="utf-8"))
        if len(back["prefabs"]) != stats["exported"]:
            print("MISMATCH: report lists %d prefabs, exported %d" % (len(back["prefabs"]),
                                                                      stats["exported"]))
            return 2
    else:
        print("PILOT run: %s is left untouched (it lists the full export)"
              % os.path.relpath(OUT_REPORT, _REPO).replace(os.sep, "/"))
    stale = []
    # A filtered pilot run writes one description, so cleaning "stale" files against that would
    # delete every other description on disk. Skip the sweep whenever a filter is active.
    if not args.dry_run and not args.only:
        for out_dir in (OUT_DIR, TPL_OUT_DIR):
            if not os.path.isdir(out_dir):
                continue
            keep_files = {names[k] + ".json" for k in report if folders[k][1] == out_dir}
            for f in sorted(os.listdir(out_dir)):
                if f.endswith(".json") and f not in keep_files:
                    os.remove(os.path.join(out_dir, f))
                    stale.append(os.path.join(os.path.basename(out_dir), f))
        if stale:
            print("removed %d stale description(s) whose root is no longer exported: %s"
                  % (len(stale), ", ".join(stale[:6])))
    counted = {}
    for out_dir in (OUT_DIR, TPL_OUT_DIR):
        want = sum(1 for k in report if folders[k][1] == out_dir)
        if not os.path.isdir(out_dir):
            if want:
                print("MISMATCH: %d descriptions asked for in %s, directory absent"
                      % (want, os.path.basename(out_dir)))
                return 2
            continue
        have = len([f for f in os.listdir(out_dir) if f.endswith(".json")])
        counted[out_dir] = have
        if not args.only and have != want:
            print("MISMATCH: %d descriptions asked for in %s, %d on disk"
                  % (want, os.path.basename(out_dir), have))
            return 2
    # The index extract_scene.py consults to turn a cross-file reference into a reference INTO a
    # prefab. Keyed by the object the shipped PPtr named, so it covers both kinds of prefab --
    # a template root that also happens to be a load key (wp_horse) resolves to the Resources asset.
    index = {"%s!%d" % (r["container"], oid): {"asset": r["assets"], "prefab": k,
                                               "root": "%s!%d" % (r["container"], r["path_id"])}
             for k, r in sorted(report.items()) for oid in r["object_ids"]}
    if args.only:
        # A partial run must not narrow the index the extractor reads, so merge this run's entries
        # into what is already published instead of rewriting it from one root.
        merged = load_json(TEMPLATE_INDEX) or {}
        merged.update(index)
        index = merged
    tmp = TEMPLATE_INDEX + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(index, fh, indent=1, ensure_ascii=False, sort_keys=True)
    os.replace(tmp, TEMPLATE_INDEX)
    print("prefab-root index: %d container!path_id -> asset path(s) written to %s"
          % (len(index), os.path.relpath(TEMPLATE_INDEX, _REPO).replace(os.sep, "/")))
    print("this run exported %d prefab(s); descriptions on disk: %d in %s, %d in %s"
          % (stats["exported"], counted.get(OUT_DIR, 0),
             os.path.relpath(OUT_DIR, _REPO).replace(os.sep, "/"),
             counted.get(TPL_OUT_DIR, 0),
             os.path.relpath(TPL_OUT_DIR, _REPO).replace(os.sep, "/")))
    if args.verbose:
        for k in sorted(report):
            print("   %-24s %3d objects  %s" % (k, report[k]["objects"],
                                                report[k]["components"]))
    return 0


if __name__ == "__main__":
    sys.exit(main())
