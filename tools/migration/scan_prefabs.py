"""Read-only: are the Resources.Load keys recoverable from the APK at all?

Task #17 is the upstream blocker for several things already documented -- Monster_efs.shadow and
Spawn_story.mon_destroy reference objects inside prefabs in sharedassets, and the 8 ambiguous
animation states live in Cha_Control.attack_blade[] which is prefab data. Before writing an
exporter, measure whether the data supports one:

  1. every Resources.Load / Resources.LoadAll literal key in the shipped scripts, and the type the
     call site demands (Instantiate -> GameObject, `as Texture2D` -> Texture2D, Load<AudioClip>).
     A key whose closing quote is immediately followed by `+` is only the HEAD of a family
     ("bgm_stage" + num), so it must be resolved against numbered siblings, not reported missing.
  2. whether a GameObject of that name exists in a NON-level container (that is what a prefab root
     looks like in a Unity 4 serialized file), and how big its subtree is -- the subtree size says
     whether the exporter has to walk a hierarchy or copy one object.
  3. for a key with no GameObject, whether the name exists under another object type at all
     (Texture2D, AudioClip, Material, TextAsset, ...), because then it belongs to an exporter that
     already exists rather than to #17.

A key with nothing behind it is a gap to report, not something to name-match into existence.
Conversely, the 2026-10-10 first pass called 14 keys "not in the build" and every one of them was a
concatenation head whose numbered siblings are all present -- a self-inflicted false negative,
which is why the head/type split is the core of this probe rather than a footnote.

Run:  <unitypy venv python> tools/migration/scan_prefabs.py   [RECON_DIR=<windows path to recon>]
"""
import collections
import json
import os
import re
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCRIPTS = os.path.join(_REPO, "unity", "Assets", "Scripts")
OUT = os.path.join(_REPO, "tools", "migration", "prefabs-survey.json")

LOAD = re.compile(r'Resources\.Load(?:All)?(?:<[A-Za-z_][\w.]*>)?\s*\(\s*"([^"]+)"')
# The C# type a call site asks for, mapped to the Unity 4 object type that can satisfy it.
TYPE_OF = {"GameObject": "GameObject", "Texture": "Texture2D", "Texture2D": "Texture2D",
           "AudioClip": "AudioClip", "Material": "Material", "TextAsset": "TextAsset",
           "Mesh": "Mesh", "Animation": "Animation", "Sprite": "Sprite"}


def call_sites():
    """key -> {wanted: [PPtr types], head: bool, sites: int, examples: [str]}"""
    out = {}
    for root, _d, files in os.walk(SCRIPTS):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            with open(os.path.join(root, fn), encoding="utf-8", errors="replace") as fh:
                text = fh.read()
            for m in LOAD.finditer(text):
                key = m.group(1)
                line_start = text.rfind("\n", 0, m.start()) + 1
                line_end = text.find("\n", m.end())
                line = text[line_start:line_end if line_end > 0 else len(text)].strip()
                # What follows the closing quote decides whether the literal IS the whole key.
                after = text[m.end():m.end() + 3].lstrip()
                g = re.search(r'Resources\.Load<(\w+)>', m.group(0))
                a = re.search(r'\bas\s+([A-Za-z_][\w.]*)\b', line)
                wanted = set()
                for name in (g.group(1) if g else None, a.group(1) if a else None):
                    if name in TYPE_OF:
                        wanted.add(TYPE_OF[name])
                if "Instantiate" in line:
                    wanted.add("GameObject")
                rec = out.setdefault(key, {"wanted": [], "head": False, "sites": 0,
                                           "examples": []})
                rec["wanted"] = sorted(set(rec["wanted"]) | wanted)
                rec["sites"] += 1
                if after.startswith("+") or key.endswith("_"):
                    rec["head"] = True
                if len(rec["examples"]) < 3:
                    rec["examples"].append("%s: %s" % (fn[:-3], line[:150]))
    return out


def ppid(val):
    return int(getattr(val, "m_PathID", val) or 0)


def collect(path):
    """-> (gameobjects, transform_parentage, typed_names) for one serialized file.

    gameobjects    {path_id: (name, component_count)}
    parentage      {go path_id: father go path_id or None}   (same-file PPtrs only)
    typed_names    {(type, name): [path_id]}

    m_Father is a PPtr to the parent TRANSFORM, not to a GameObject, so the transform->GameObject
    map has to be applied before the id can be used as a hierarchy key. Treating the raw value as
    a GameObject id silently made every prefab look like a single object.
    """
    env = UnityPy.load(path)
    gos, typed = {}, collections.defaultdict(list)
    t_of_go, father_of_t = {}, {}
    for o in env.objects:
        t = o.type.name
        try:
            d = o.read()
        except Exception:
            continue
        nm = getattr(d, "m_Name", None)
        if nm is not None:
            typed[(t, str(nm))].append(o.path_id)
        if t == "GameObject":
            gos[o.path_id] = (str(nm), len(list(d.m_Component)))
        elif t == "Transform":
            go = ppid(d.m_GameObject)
            if go:
                t_of_go[o.path_id] = go
            father = d.m_Father
            fp = ppid(father) if father is not None else 0
            # A cross-file m_Father would be a nested prefab; recording it as "no father" would
            # silently split a hierarchy in two, so the FileID is checked, not assumed to be 0.
            fid = int(getattr(father, "m_FileID", 0) or 0) if father is not None else 0
            if fp and fid == 0:
                father_of_t[o.path_id] = fp

    parentage, t_for_go = {}, {}
    for tp, go in t_of_go.items():
        t_for_go.setdefault(go, tp)
    for go in gos:
        tp = t_for_go.get(go)
        ftp = father_of_t.get(tp) if tp is not None else None
        parentage[go] = t_of_go.get(ftp) if ftp else None
    return gos, parentage, dict(typed)


def subtree(key, children):
    """Object count under `key` -- a (file, path_id) pair -- including itself, cycle-guarded."""
    seen, stack = set(), [key]
    while stack:
        cur = stack.pop()
        if cur in seen:
            continue
        seen.add(cur)
        stack.extend(children.get(cur, []))
    return len(seen)


def calibrate():
    """Known-positive control for the extraction path itself.

    Without this the probe fails silently: read the UnityPy API wrongly and every container yields
    zero GameObjects, so the honest-looking line below becomes "no prefab exists for any key" -- a
    verdict about the game's data that is really a verdict about this script. The 2026-10-10 run
    did exactly that: a half-deleted venv (every site-packages *.py gone) made all 449 loads raise
    and the scan still printed "matched 0 / 61".

    The level file with the MOST GameObjects is read through the same code path and must yield both
    GameObjects and parent links. Picking the largest is not a nicety: the first version calibrated
    on level5, which really does hold 2 flat GameObjects, and "something must have a father"
    reported a broken Transform read that was not there.
    """
    levels = [(b, p) for b, p in u4.sources() if b.startswith("level")]
    if not levels:
        return None, "no level file available to calibrate against"
    best = None
    for base, path in levels:
        try:
            gos, parentage, typed = collect(path)
        except Exception as e:
            return None, "%s: load raised: %s" % (base, str(e)[:80])
        if best is None or len(gos) > best[1]:
            best = (base, len(gos), sum(1 for v in parentage.values() if v), len(typed))
    base, n, parented, ntyped = best
    if n == 0:
        return None, "%s yielded 0 GameObjects -- extraction is broken" % base
    if parented == 0:
        return None, ("%s yielded %d GameObjects and none has a father -- either the Transform "
                      "read is broken or no scene in this build has a hierarchy, which "
                      "contradicts the 23 rebuilt .unity files" % (base, n))
    return ("%s -> %d GameObjects, %d with a parent, %d typed names"
            % (base, n, parented, ntyped), None)


def main():
    sites = call_sites()
    keys = sorted(sites)
    heads = [k for k in keys if sites[k]["head"]]
    exact = [k for k in keys if not sites[k]["head"]]
    print("literal Resources.Load keys: %d  (%d whole keys, %d concatenation heads: %s)"
          % (len(keys), len(exact), len(heads), heads))

    ok, why = calibrate()
    if ok is None:
        print("CALIBRATION FAILED -- refusing to report any match counts: %s" % why)
        return 2
    print("calibration (extraction path proven on known data): %s" % ok)

    roots = collections.defaultdict(list)          # name -> [(file, path_id)]
    parentage, components = {}, {}
    nonlevel_t, level_t = collections.defaultdict(list), collections.defaultdict(list)
    loaded, failed = 0, []
    src = u4.sources()
    for base, path in src:
        in_levels = base.startswith("level")
        try:
            gos, par, typed = collect(path)
        except Exception as e:
            if not in_levels:
                failed.append((base, str(e)[:40]))
            continue
        for pid, (name, ncomp) in gos.items():
            if in_levels:
                continue
            roots[name].append((base, pid))
            parentage[(base, pid)] = par.get(pid)
            components[(base, pid)] = ncomp
        for (t, n), pids in typed.items():
            for pid in pids:
                (level_t if in_levels else nonlevel_t)[(t, n)].append((base, pid))
        if not in_levels:
            loaded += 1

    n_nonlevel = len(src) - len([1 for b, _p in src if b.startswith("level")])
    print("containers: %d total, %d non-level scanned, %d failed" % (len(src), loaded, len(failed)))
    if failed:
        print("  failures: %s" % failed[:6])
    # A scan that could not open most of its inputs has measured nothing, whatever it prints next.
    if loaded == 0 or len(failed) * 4 > n_nonlevel:
        print("COVERAGE TOO LOW (%d of %d non-level containers opened) -- NO MEASUREMENT, "
              "not reporting match counts" % (loaded, n_nonlevel))
        return 2
    parented = sum(1 for v in parentage.values() if v)
    print("distinct GameObject names outside levels: %d (total %d objects, %d with a father)"
          % (len(roots), sum(len(v) for v in roots.values()), parented))
    if not roots:
        print("0 GameObject names outside levels -- genuinely none, or this scan is reading the "
              "wrong containers; NO MEASUREMENT")
        return 2

    children = collections.defaultdict(list)
    for key, fa in parentage.items():
        if fa:
            children[(key[0], fa)].append(key)

    def names_of(types, pred):
        got = collections.defaultdict(set)
        for (t, n) in nonlevel_t:
            if (not types or t in types) and pred(n):
                got[t].add(n)
        for n in roots:
            if (not types or "GameObject" in types) and pred(n):
                got["GameObject"].add(n)
        return got

    verdicts = {}
    print("\n=== whole keys ===")
    for k in exact:
        wanted = sites[k]["wanted"]
        got = names_of(wanted, lambda n: n == k)
        go_hits = roots.get(k, [])
        if go_hits:
            sz = [subtree((f, p), children) for f, p in go_hits]
            verdicts[k] = {"kind": "prefab", "objects": go_hits, "subtree": sz,
                           "components": sorted({components[(f, p)] for f, p in go_hits}),
                           "types": sorted(t for t in got if t != "GameObject")}
            print("  PREFAB  %-22s subtree=%-8s comps=%-6s files=%s other types=%s sites=%d"
                  % (k, sz, verdicts[k]["components"], sorted({f for f, _p in go_hits}),
                     verdicts[k]["types"], sites[k]["sites"]))
            continue
        types = sorted({t for t, ns in got.items() if ns})
        lvl = sorted({t for (t, n) in level_t if n == k})
        if types:
            verdicts[k] = {"kind": "asset", "types": types,
                           "objects": {t: nonlevel_t[(t, k)] for t in types}}
            print("  ASSET   %-22s types=%-30s wanted=%s sites=%d"
                  % (k, types, wanted, sites[k]["sites"]))
        elif lvl:
            verdicts[k] = {"kind": "only in levels", "level_types": lvl}
            print("  INLEVEL %-22s name exists inside a level file as %s -- Resources.Load cannot "
                  "reach a scene object, so this key was dead in the original build too"
                  % (k, lvl))
        else:
            ci = sorted(n for n in roots if n.lower() == k.lower())
            verdicts[k] = {"kind": "NOT FOUND", "case_only": ci}
            print("  GAP     %-22s nothing behind it (case-only matches: %s)  | %s"
                  % (k, ci, sites[k]["examples"][0][:100] if sites[k]["examples"] else ""))

    print("\n=== concatenation heads ===")
    for k in heads:
        wanted = sites[k]["wanted"]
        members = names_of(wanted, lambda n, h=k: n.startswith(h))
        members = {t: sorted(v) for t, v in sorted(members.items()) if v}
        total = sum(len(v) for v in members.values())
        if total:
            # GameObject members are recorded with their location, not just their name: an
            # exporter has to find the object again, and a name is ambiguous across containers.
            locs = {n: roots[n] for n in members.get("GameObject", []) if n in roots}
            verdicts[k] = {"kind": "family", "members": members, "prefab_objects": locs}
            print("  FAMILY  %-22s wanted=%-14s members=%d %s"
                  % (k, wanted or "any", total,
                     {t: (len(v), v[:4]) for t, v in members.items()}))
        else:
            verdicts[k] = {"kind": "NOT FOUND"}
            print("  GAP     %-22s no object of any type starts with this head | %s"
                  % (k, sites[k]["examples"][0][:100] if sites[k]["examples"] else ""))

    prefabs = sorted(k for k, v in verdicts.items() if v["kind"] == "prefab")
    sizes = sorted(x for k in prefabs for x in verdicts[k]["subtree"])
    # The hierarchy join is keyed by (file, path_id); passing a bare path_id silently made every
    # prefab look like one object. If the containers demonstrably hold parented objects but NO
    # matched prefab has children, that is this scan contradicting itself -- a per-key version of
    # this check fired on RankingManager, which really is a single object, so it is aggregate only.
    if sizes and max(sizes) == 1 and parented:
        print("  INCONSISTENT: %d non-level GameObjects have a father yet all %d matched prefabs "
              "resolve to 1 object -- the hierarchy join is wrong, not the data"
              % (parented, len(prefabs)))
        return 2
    fam_prefab = sorted(k for k, v in verdicts.items() if v["kind"] == "family"
                        and "GameObject" in v["members"])
    fam_objs = sorted({n for k in fam_prefab for n in verdicts[k]["members"]["GameObject"]})
    gaps = sorted(k for k, v in verdicts.items() if v["kind"] == "NOT FOUND")
    print("\n=== what this buys the exporter ===")
    print("  whole keys that are prefabs      : %d (subtree min %s, median %s, max %s, %d objects)"
          % (len(prefabs), sizes[0] if sizes else "-", sizes[len(sizes) // 2] if sizes else "-",
             sizes[-1] if sizes else "-", sum(sizes)))
    print("  heads whose members are prefabs  : %d heads -> %d GameObject roots (%s...)"
          % (len(fam_prefab), len(fam_objs), fam_objs[:10]))
    print("  keys that are a plain asset      : %d"
          % sum(1 for v in verdicts.values() if v["kind"] == "asset"))
    print("  keys only reachable inside levels: %d"
          % sum(1 for v in verdicts.values() if v["kind"] == "only in levels"))
    print("  keys with nothing behind them    : %d %s" % (len(gaps), gaps))
    print("  keys with something behind them  : %d / %d"
          % (sum(1 for k in keys if verdicts[k]["kind"] != "NOT FOUND"), len(keys)))

    with open(OUT, "w", encoding="utf-8") as fh:
        json.dump({"keys": keys, "sites": sites, "verdicts": verdicts,
                   "prefab_keys": prefabs, "family_prefab_objects": fam_objs, "gaps": gaps,
                   "components": {("%s!%d" % f): c for f, c in components.items()}}, fh, indent=1)
    print("\nwrote %s" % OUT)
    return 0


if __name__ == "__main__":
    sys.exit(main())
