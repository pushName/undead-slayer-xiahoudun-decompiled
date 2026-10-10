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
  tools/migration/prefabs/<name>.json      one description per exportable root
  tools/migration/prefabs-export.json      tally, the pending list, and per-root counts

Run:  <unitypy venv python> tools/migration/export_prefabs.py [--dry-run]  RECON_DIR=<path>
"""
import argparse
import collections
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

BAD = re.compile(r'[<>:"/\\|?*\x00-\x1f]')


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
    args = ap.parse_args()

    roots = wanted_roots()
    unique = {k: v[0] for k, v in roots.items() if len(v) == 1}
    pending = {k: v for k, v in roots.items() if len(v) > 1}
    print("GameObject keys: %d, single candidate: %d, ambiguous (not guessed): %d"
          % (len(roots), len(unique), len(pending)))
    for k, v in sorted(pending.items()):
        print("   pending %-16s candidates %s" % (k, v))

    names = {k: BAD.sub("_", k).strip(". ") for k in unique}
    lower = collections.Counter(n.lower() for n in names.values())
    dup = [n for n, c in lower.items() if c > 1]
    if dup:
        print("REFUSING: two roots would be written as the same Resources name: %s" % dup[:6])
        return 2
    res_dir = os.path.join(_REPO, "unity", "Assets", "Resources")
    on_disk = {os.path.splitext(f)[0].lower() for f in os.listdir(res_dir)
               if not f.endswith(".meta")}
    hit = sorted(n for n in names.values() if n.lower() in on_disk)
    if hit:
        print("REFUSING: %d prefab name(s) collide with an existing Resources asset, which would "
              "make Resources.Load('%s') ambiguous" % (len(hit), hit[0]))
        return 2

    by_container = collections.defaultdict(list)
    for key, (cont, pid) in unique.items():
        by_container[cont].append((key, pid))
    print("containers to open: %d" % len(by_container))

    contents = json.load(open(CONTENTS, encoding="utf-8")) if os.path.exists(CONTENTS) else None
    srcs = dict(u4.sources())
    stats = collections.Counter()
    report, missing = {}, []
    missing_clips = set()
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
            keep = subtree_ids(nodes, root["id"])
            kept = cut_to_subtree(nodes, keep, stats)
            hist = histogram(kept)
            if contents:
                claim = contents.get("per_root", {}).get("%s!%d" % (cont, pid), {}).get("components")
                if claim is not None and dict(hist) != claim:
                    print("MISMATCH %s: exporter sees %s, prefab-contents.json measured %s"
                          % (key, dict(hist), claim))
                    return 2
            payload = {
                "source_container": cont,
                "root_path_id": pid,
                "prefab_name": key,
                "asset_path": "Assets/Resources/%s.prefab" % names[key],
                "node_count": len(kept),
                "root_ids": [root["id"]],
                "unsupported_components": dict(unsupported),
                "field_stats": dict(ctx.field_stats),
                "container_dropped_refs": list(ctx.dropped_refs),
                "container_warnings": list(ctx.warnings),
                "nodes": kept,
            }
            report[key] = {"container": cont, "path_id": pid,
                           "assets": payload["asset_path"], "objects": len(kept),
                           "components": dict(hist)}
            if not args.dry_run:
                os.makedirs(OUT_DIR, exist_ok=True)
                safe = names[key]
                tmp = os.path.join(OUT_DIR, safe + ".json.tmp")
                with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
                    json.dump(payload, fh, indent=1, ensure_ascii=False)
                os.replace(tmp, os.path.join(OUT_DIR, safe + ".json"))
            for n in kept:
                for b in n.get("animations") or []:
                    if not os.path.exists(os.path.join(_REPO, "unity",
                                                       b["asset"].replace("/", os.sep))):
                        missing_clips.add("%s -> %s" % (key, b["asset"]))
            stats["exported"] += 1

    if missing:
        print("REFUSING to call this a run: %d root GameObject(s) produced no node at all, e.g. %s"
              % (len(missing), missing[:3]))
        return 2
    # Some animation bindings reference a .anim the Editor has not built yet: the index lists 427
    # clips and 140 have been turned into assets. Counted rather than hidden -- the fix is the build
    # pass, not the description.
    print("animation bindings pointing at a .anim that is not built yet: %d "
          "(the clip-to-asset pass has produced 140 of the build's 427)" % len(missing_clips))
    print("exported %d prefabs, %d objects total (%s cut for references outside a subtree)"
          % (stats["exported"], sum(r["objects"] for r in report.values()),
             {k: v for k, v in stats.items() if k != "exported"} or "nothing"))
    doc = {"tally": {"exported": stats["exported"], "pending_ambiguous": len(pending)},
           "pending": {k: ["%s!%d" % c for c in v] for k, v in sorted(pending.items())},
           "cut_stats": {k: v for k, v in stats.items() if k != "exported"},
           "animation_bindings_missing_on_disk": sorted(missing_clips),
           "prefabs": report}
    if args.dry_run:
        print("dry run -- nothing written")
        return 0
    tmp = OUT_REPORT + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(doc, fh, indent=1, ensure_ascii=False, sort_keys=True)
    os.replace(tmp, OUT_REPORT)
    back = json.load(open(OUT_REPORT, encoding="utf-8"))
    if len(back["prefabs"]) != stats["exported"]:
        print("MISMATCH: report lists %d prefabs, exported %d" % (len(back["prefabs"]),
                                                                  stats["exported"]))
        return 2
    on_disk = [f for f in os.listdir(OUT_DIR) if f.endswith(".json")]
    if len(on_disk) != stats["exported"]:
        print("MISMATCH: %d descriptions asked for, %d on disk" % (stats["exported"], len(on_disk)))
        return 2
    print("wrote %d descriptions into %s and %s (re-read ok)"
          % (len(on_disk), os.path.relpath(OUT_DIR, _REPO).replace(os.sep, "/"),
             os.path.relpath(OUT_REPORT, _REPO).replace(os.sep, "/")))
    if args.verbose:
        for k in sorted(report):
            print("   %-24s %3d objects  %s" % (k, report[k]["objects"],
                                                report[k]["components"]))
    return 0


if __name__ == "__main__":
    sys.exit(main())
