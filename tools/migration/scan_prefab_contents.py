"""Read-only: what is actually inside the prefabs task #17 would have to export?

scan_prefabs.py answered "which Resources.Load keys have an object behind them";
scan_dropped_refs.py answered "which objects the scenes point at by fileID". Neither says what an
exporter has to carry, and that answer is the design:

  * the subtree of every root, with each object's path inside the prefab. A scene reference to a
    Transform INSIDE a prefab has to be re-addressed by that path once the prefab is a 2022 .prefab
    asset -- AssetDatabase.LoadAllAssetsAtPath gives the objects, and the name path is the only
    stable handle (the fileIDs inside a prefab are assigned by Unity, not by us).
  * the component types present, split with the SAME lists extract_scene.py uses for scenes: what
    2022 still has, what was removed in 5.x (legacy particles and friends), what is unsupported.
  * for renderers and MonoBehaviours, whether cross-file dependencies resolve through the indexes the
    existing exports produced (meshes/materials/textures/clips/audio). An unresolvable dependency
    inside a prefab is worse than in a scene: nothing downstream can fill it.
  * the MonoBehaviour field decode rate for prefab objects -- the first time that decoder runs on
    data that was never in a level file.

Writes tools/migration/prefab-contents.json. Reads the build, changes nothing.

Run:  <unitypy venv python> tools/migration/scan_prefab_contents.py   RECON_DIR=<path to recon>
"""
import collections
import json
import os
import struct
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4
import monobehaviour_fields as MF
import extract_scene as ES          # SUPPORTED / REMOVED_IN_5 / the decode contract
from scan_dropped_refs import pid_of  # same PPtr-shape handling as the ledger resolver

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MIG = os.path.join(_REPO, "tools", "migration")
OUT = os.path.join(MIG, "prefab-contents.json")


def key_index(fname, transform=None):
    p = os.path.join(MIG, fname)
    if not os.path.exists(p):
        return None
    d = json.load(open(p, encoding="utf-8"))
    idx = d.get("index", d)
    return set(idx if transform is None else transform(idx))


def main():
    for need in ("prefabs-survey.json", "dropped-refs.json", "monoscript-bindings.json"):
        if not os.path.exists(os.path.join(MIG, need)):
            sys.exit("%s is missing -- this scan reads those artifacts, it does not rebuild them"
                     % need)
    survey = json.load(open(os.path.join(MIG, "prefabs-survey.json"), encoding="utf-8"))
    ledger = json.load(open(os.path.join(MIG, "dropped-refs.json"), encoding="utf-8"))
    bindings = json.load(open(os.path.join(MIG, "monoscript-bindings.json"), encoding="utf-8"))
    monoscripts = bindings["monoscripts"]

    RESOLVABLE = {
        "Mesh": key_index("meshes.json"),
        "Material": key_index("materials.json"),
        "Texture2D": key_index("texture-index.json"),
        "AudioClip": set(key_index("audio.json") or []),
        "AnimationClip": key_index("animation-index.json",
                                   lambda ix: ["%s!%d" % (e["container"], e["path_id"])
                                               for e in ix["clips"]]),
    }
    missing_indexes = sorted(k for k, v in RESOLVABLE.items() if v is None)
    if missing_indexes:
        print("WARNING: no index for %s -- dependencies of that type will be reported as "
              "unresolvable because there is nothing to check them against, not because the "
              "export failed" % missing_indexes)

    src = {b: p for b, p in u4.sources()}
    cache = {}

    def doc(f):
        """-> ({path_id: object}, externals) for one container, loaded once."""
        if f not in cache:
            if f not in src:
                cache[f] = (None, [])
            else:
                env = UnityPy.load(src[f])
                cache[f] = ({o.path_id: o for o in env.objects}, u4.externals_of(env))
        return cache[f]

    def read(f, pid):
        objs, _ext = doc(f)
        if not objs or pid not in objs:
            return None, None
        o = objs[pid]
        try:
            return o.type.name, o.read()
        except Exception:
            return o.type.name, None

    def go_index(f):
        """{GameObject name: [path_id]} for one container, built once and reused."""
        k = ("go", f)
        if k not in cache:
            objs, _ext = doc(f)
            out = collections.defaultdict(list)
            if objs:
                for pid, o in objs.items():
                    if o.type.name != "GameObject":
                        continue
                    try:
                        out[str(o.read().m_Name)].append(pid)
                    except Exception:
                        continue
            cache[k] = dict(out)
        return cache[k]

    # ---- 1. the roots: Load keys and numbered family members, from the survey
    roots = {}
    for key, v in survey["verdicts"].items():
        if v["kind"] == "prefab":
            for f, p in v["objects"]:
                roots[(f, p)] = key
        for name, locs in (v.get("prefab_objects") or {}).items():
            for f, p in locs:
                roots[(f, p)] = name
    print("roots named by a Resources.Load key: %d" % len(roots))

    # ---- 2. the roots the scenes address by fileID: climb to the top, keep the child path
    ref_paths = {}
    for r in ledger["resolved"]:
        f, pid = r["container"], r["path_id"]
        if r["tgt_type"] not in ("Transform", "GameObject") or not f or f not in src:
            continue
        chain, name = [], None
        if r["tgt_type"] == "GameObject":
            chain, name = [r["object"]], r["object"]
        else:
            t, d = read(f, pid)
            go_pid = pid_of(d.m_GameObject)
            _gt, gd = read(f, go_pid)
            name = str(gd.m_Name)
            chain = [name]
            cur = pid
            guard = 0
            while cur and guard < 64:
                _tt, td = read(f, cur)
                fa = td.m_Father
                fp = pid_of(fa)
                if not fp or int(getattr(fa, "m_FileID", 0) or 0) != 0:
                    break
                _nt, fd = read(f, fp)
                _x, ogd = read(f, pid_of(fd.m_GameObject))
                chain.append(str(ogd.m_Name))
                cur = fp
                guard += 1
        root_name = chain[-1]
        # the root's path_id, from the per-file GameObject index. A name occurring twice at root
        # level in one container is recorded as ambiguous rather than guessed at.
        cands = go_index(f).get(root_name, [])
        if not cands:
            ref_paths["%s!%d" % (f, pid)] = {"root": root_name, "path": list(reversed(chain)),
                                              "root_pid": None, "why": "root GameObject not found"}
            continue
        root_pid = cands[0]
        roots.setdefault((f, root_pid), root_name)
        ref_paths["%s!%d" % (f, pid)] = {"root": root_name, "path": list(reversed(chain)),
                                         "root_pid": root_pid,
                                         "ambiguous_root": len(cands) > 1}
    print("roots after adding the fileID-referenced ones: %d" % len(roots))
    print("ledger references mapped to a prefab path: %d" % len(ref_paths))

    # ---- 3. walk every root's subtree
    comp_hist = collections.Counter()
    unres = []
    mono = collections.Counter()
    dep = collections.Counter()
    per_root = {}
    files_by_root = collections.Counter()

    for (f, root_pid), root_name in sorted(roots.items()):
        objs, externals = doc(f)
        # children index for this file (built once per file, not per root)
        if ("kids", f) not in cache:
            kids = collections.defaultdict(list)
            go_of = {}
            for pid, o in objs.items():
                if o.type.name != "Transform":
                    continue
                try:
                    d = o.read()
                except Exception:
                    continue
                gop = pid_of(d.m_GameObject)
                go_of[pid] = gop
                fa = d.m_Father
                fp = pid_of(fa)
                if fp and int(getattr(fa, "m_FileID", 0) or 0) == 0:
                    ft, fd = read(f, fp)
                    if ft == "Transform":
                        kids[pid_of(fd.m_GameObject)].append(gop)
            cache[("kids", f)] = dict(kids)
        kids = cache[("kids", f)]

        seen, stack = set(), [root_pid]
        while stack:
            cur = stack.pop()
            if cur in seen:
                continue
            seen.add(cur)
            stack.extend(kids.get(cur, []))
        root_key = "%s!%d" % (f, root_pid)
        per_root[root_key] = {"name": root_name, "objects": len(seen), "components": {}}
        root_comps = collections.Counter()
        files_by_root[f] += 1
        for go_pid in seen:
            t, gd = read(f, go_pid)
            if t != "GameObject":
                continue
            for _tid, pp in u4.component_pairs(gd):
                cp = int(pp.m_PathID)
                ct, cd = read(f, cp)
                if ct is None:
                    comp_hist["unreadable"] += 1
                    continue
                comp_hist[ct] += 1
                root_comps[ct] += 1
                if ct in ES.REMOVED_IN_5:
                    continue
                if ct == "MonoBehaviour":
                    # The typed read is not needed and often not possible here: the fields live in
                    # the raw instance data, which get_raw_data() gives regardless of whether UnityPy
                    # can parse the class. Counting a read() failure as "this prefab's script data is
                    # unreadable" would have understated the decode rate by 81 components.
                    if cp not in objs:
                        mono["component object absent from the file"] += 1
                        continue
                    raw = bytes(objs[cp].get_raw_data())
                    if len(raw) < MF.HEADER_LEN:
                        mono["record shorter than the header"] += 1
                        continue
                    _gof, _gop, _en, scf, scp = struct.unpack_from("<iiiii", raw, 0)
                    tgt = u4.resolve_target(f, externals, scf) if scf else f
                    sname = monoscripts.get("%s!%d" % (tgt, scp)) if tgt else None
                    if sname is None:
                        mono["script unresolved"] += 1
                        continue
                    vals, why, truncated = MF.decode_for_class(sname, raw)
                    if vals is None:
                        mono["refused: " + MF.classify_refusal(why)[:40]] += 1
                        continue
                    mono["decoded"] += 1
                    if truncated:
                        mono["decoded, trailing fields absent"] += 1
                    for fname, v in vals:
                        items = [v] + [x for x in (v.get("arr") or []) if isinstance(x, dict)] \
                            if isinstance(v, dict) else [v]
                        for it in items:
                            if not isinstance(it, dict) or "ref" not in it:
                                continue
                            if int(it["file"]) == 0:
                                continue
                            rt = str(it.get("type") or "")
                            tgt2 = u4.resolve_target(f, externals, int(it["file"]))
                            key = "%s!%d" % (tgt2, int(it["ref"]))
                            pool = RESOLVABLE.get(rt)
                            if pool is None:
                                dep["type not indexed: " + (rt or "?")] += 1
                            elif key in pool:
                                dep["resolvable: " + rt] += 1
                            else:
                                dep["UNRESOLVED: " + rt] += 1
                                unres.append({"prefab": root_key, "name": root_name,
                                              "script": sname, "field": fname, "type": rt,
                                              "target": key})
                elif cd is not None and ct in ("MeshFilter", "SkinnedMeshRenderer", "MeshRenderer"):
                    m = getattr(cd, "m_Mesh", None)
                    if m is not None and pid_of(m) and int(getattr(m, "m_FileID", 0) or 0):
                        mt = u4.resolve_target(f, externals, int(m.m_FileID))
                        k = "%s!%d" % (mt, pid_of(m))
                        dep[("resolvable: Mesh" if k in (RESOLVABLE["Mesh"] or set())
                             else "UNRESOLVED: Mesh")] += 1
                    mats = getattr(cd, "m_Materials", None) or []
                    for mm in mats:
                        if pid_of(mm) and int(getattr(mm, "m_FileID", 0) or 0):
                            mt = u4.resolve_target(f, externals, int(mm.m_FileID))
                            k = "%s!%d" % (mt, pid_of(mm))
                            ok = k in (RESOLVABLE["Material"] or set())
                        dep[("resolvable: Material" if ok else "UNRESOLVED: Material")] += 1
                        if not ok:
                            unres.append({"prefab": root_key, "name": root_name,
                                          "component": ct, "field": "m_Materials",
                                          "type": "Material", "target": k})

        per_root[root_key]["components"] = dict(root_comps)

    mono_total = comp_hist.get("MonoBehaviour", 0)
    mono_sum = sum(mono.values())
    print("\n=== MonoBehaviour accounting ===")
    print("components %d, bucketed %d, unaccounted %d" % (mono_total, mono_sum, mono_total-mono_sum))
    if mono_total != mono_sum:
        print("COVERAGE HOLE: %d MonoBehaviour components fell through the buckets -- the decode"
              " rate below would be a partial number" % (mono_total - mono_sum))
        return 2
    total_objs = sum(v["objects"] for v in per_root.values())
    print("\n=== volume ===")
    print("prefabs: %d roots, %d GameObjects in their subtrees (max %d, %s)"
          % (len(per_root), total_objs,
             max(v["objects"] for v in per_root.values()),
             [k for k, v in sorted(per_root.items(), key=lambda kv: -kv[1]["objects"])[:3]]))
    print("containers holding them: %s" % dict(files_by_root.most_common()))
    print("\n=== component types across those subtrees ===")
    for t, n in comp_hist.most_common():
        tag = ("REMOVED in Unity 5 (#1 territory)" if t in ES.REMOVED_IN_5
               else "ok" if t in ES.SUPPORTED or t in ("MonoBehaviour", "Transform", "Canvas")
               else "not classified -- check")
        print("  %-28s %5d   %s" % (t, n, tag))
    print("\n=== MonoBehaviour fields inside prefabs (first run on non-level data) ===")
    for k, v in mono.most_common():
        print("  %-46s %d" % (k, v))
    print("\n=== cross-file dependencies inside prefabs ===")
    for k, v in dep.most_common():
        print("  %-28s %d" % (k, v))

    sizes = sorted(v["objects"] for v in per_root.values())
    with open(OUT, "w", encoding="utf-8") as fh:
        json.dump({"roots": {"%s!%d" % (f, p): n for (f, p), n in roots.items()},
                   "per_root": per_root,
                   "subtree_sizes": {"min": sizes[0], "median": sizes[len(sizes) // 2],
                                     "max": sizes[-1], "total": sum(sizes)},
                   "components": dict(comp_hist),
                   "mono": dict(mono),
                   "dependencies": dict(dep),
                   "unresolved_dependencies": unres,
                   "ledger_paths": ref_paths}, fh, indent=1, ensure_ascii=False)
    print("\nwrote %s" % OUT)
    return 0


if __name__ == "__main__":
    sys.exit(main())
