"""Read-only: what did extract_scene.py drop, and which export would satisfy it?

extract_scene.py now records every cross-file reference it cannot express yet in `dropped_refs`
(file, path_id, the field, the object that holds it). Counting them as "cross-file ref dropped: 20"
was what made task #17 un-plannable -- the number did not say whether the targets are prefabs (an
exporter can produce them), built-in assets (a different exporter), or something not in the build.

This resolves each entry against the container it names and answers three things:
  * the target's real type, and for a Transform the GameObject that owns it and the root of its
    hierarchy -- a Transform's own m_Name is empty in Unity 4, so reading it would report "" and
    look like an unnamed object.
  * whether that root is a Resources prefab, i.e. reachable by one of the script's Load keys or a
    numbered sibling of a concatenation head (prefabs-survey.json).
  * which existing export channel, if any, already covers the demanded type.

Run:  <unitypy venv python> tools/migration/scan_dropped_refs.py  [RECON_DIR=<windows path>]
"""
import collections
import json
import os
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCENES = os.path.join(_REPO, "tools", "migration", "scenes")
SURVEY = os.path.join(_REPO, "tools", "migration", "prefabs-survey.json")
OUT = os.path.join(_REPO, "tools", "migration", "dropped-refs.json")

# What the 2022 side already has a producer for. Anything else is a named gap, not a guess.
CHANNEL = {"Texture2D": "export_textures.py", "Material": "export_materials.py",
           "Mesh": "export_meshes.py", "AnimationClip": "export_animation_descriptors.py",
           "AudioClip": "NOT EXPORTED YET (B4/#14 audio)",
           "Font": "NOT EXPORTED YET", "GUISkin": "NOT EXPORTED YET",
           "GameObject": "task #17 (prefab export)", "Transform": "task #17 (prefab export)"}


def pid_of(v):
    """UnityPy returns m_Component entries as (PPtr, classID) tuples and PPtrs bare elsewhere."""
    while isinstance(v, (tuple, list)):
        v = v[0]
    return int(getattr(v, "m_PathID", v) or 0)


def main():
    if not os.path.exists(SURVEY):
        sys.exit("prefabs-survey.json missing -- run scan_prefabs.py first; without it every "
                 "Transform target would be reported as unknown rather than checked")
    survey = json.load(open(SURVEY, encoding="utf-8"))
    prefab_names = set(survey["prefab_keys"])
    family_members = set(survey["family_prefab_objects"])

    src = {b: p for b, p in u4.sources()}
    cache = {}

    def objects_of(fname):
        if fname not in cache:
            try:
                cache[fname] = {o.path_id: o for o in UnityPy.load(src[fname]).objects}
            except Exception:
                cache[fname] = None
        return cache[fname]

    def read(fname, pid):
        """-> (type_name, data or None, None) | (None, None, why).

        Absent and unreadable are different problems and must not share a message: a GUISkin asset
        that UnityPy types as MonoBehaviour and then fails to parse is still ONE asset sitting in a
        file we can open, while a missing path_id would mean the reference itself is wrong.
        """
        objs = objects_of(fname)
        if objs is None:
            return None, None, "container could not be opened"
        if pid not in objs:
            return None, None, "path_id not present in %s" % fname
        o = objs[pid]
        try:
            return o.type.name, o.read(), None
        except Exception as e:
            return o.type.name, None, "present as %s but UnityPy cannot parse it (%s)" % (
                o.type.name, str(e)[:48])

    def data_of(fname, pid):
        t, d, _why = read(fname, pid)
        return t, d

    def owner_and_root(fname, t, d):
        """Transform/Component -> its GameObject name, and the root of that hierarchy."""
        if t == "GameObject":
            return str(d.m_Name), str(d.m_Name)
        go = getattr(d, "m_GameObject", None)
        if go is None:
            return None, None
        gpid = pid_of(go)
        gt, gd = data_of(fname, gpid)
        if gt != "GameObject":
            return None, None
        own = str(gd.m_Name)
        root = own
        # Walk up through Transform.m_Father to reach the root; a Unity 4 m_Component entry comes
        # back as a (PPtr, classID) tuple in this UnityPy version, so both shapes must be handled.
        tp, tdata = None, None
        for c in gd.m_Component:
            cpid = pid_of(c)
            ct, cd = data_of(fname, cpid)
            if ct == "Transform":
                tp, tdata = cpid, cd
                break
        guard = 0
        while tp is not None and tdata is not None and guard < 64:
            fa = tdata.m_Father
            fpid = pid_of(fa)
            if not fpid or int(getattr(fa, "m_FileID", 0) or 0) != 0:
                break
            nt, nd = data_of(fname, fpid)
            if nt != "Transform":
                break
            _gt, gd2 = data_of(fname, pid_of(nd.m_GameObject))
            if gd2 is not None:
                root = str(gd2.m_Name)
            tp, tdata = fpid, nd
            guard += 1
        return own, root

    rows, unresolvable = [], []
    for fn in sorted(os.listdir(SCENES)):
        if not fn.startswith("level") or not fn.endswith(".json"):
            continue
        doc = json.load(open(os.path.join(SCENES, fn), encoding="utf-8"))
        for r in doc.get("dropped_refs", []):
            r["level"] = doc["source_level"]
            fname = r.get("file")
            if not fname or fname not in src:
                r["status"] = "target file not in sources"
                unresolvable.append(r)
                continue
            t, d, err = read(fname, r["path_id"])
            if d is None:
                r["status"] = err or "unreadable"
                r["tgt_type"] = t
                unresolvable.append(r)
                continue
            own, root = owner_and_root(fname, t, d)
            r.update({"tgt_type": t, "object": own, "root": root, "container": fname})
            if fname.startswith("level"):
                reach = "object inside ANOTHER level (cross-scene reference)"
            elif root in prefab_names:
                reach = "Load key"
            elif root in family_members:
                reach = "numbered sibling of a Load key"
            else:
                reach = "Resources root no script names"
            r["reachable_by"] = reach
            r["channel"] = CHANNEL.get(t, CHANNEL.get(r["t"], "unclassified"))
            rows.append(r)

    print("dropped references resolved: %d, unresolvable: %d" % (len(rows), len(unresolvable)))
    # Known-positive control, independent of everything above: Monster_efs.shadow was resolved by
    # hand to sharedassets2.assets!446, the Transform of a root GameObject named "shadow". If this
    # tool does not reproduce that exact reading, its "reachable_by" column is not trustworthy and
    # the whole report is noise -- a report that only ever prints counts has no way to notice.
    ctl = [r for r in rows if r["script"] == "Monster_efs" and r["field"] == "shadow"]
    if not ctl:
        print("CONTROL FAILED: no Monster_efs.shadow entry in the ledger -- the ledger itself is "
              "not being read")
        return 2
    c = ctl[0]
    if not (c["container"] == "sharedassets2.assets" and c["path_id"] == 446
            and c["tgt_type"] == "Transform" and c["object"] == "shadow"):
        print("CONTROL FAILED: Monster_efs.shadow resolved to %s -- resolution is wrong" % c)
        return 2
    print("control: Monster_efs.shadow -> %s!%d %s of GameObject %r (as verified by hand)"
          % (c["container"], c["path_id"], c["tgt_type"], c["object"]))
    print("\nby target type x how the root can be reached:")
    tab = collections.Counter((r["tgt_type"], r["reachable_by"]) for r in rows)
    for (t, reach), n in sorted(tab.items(), key=lambda kv: -kv[1]):
        print("  %-14s %-40s %3d   -> %s" % (t, reach, n, CHANNEL.get(t, "see field type")))
    print("\nby the type the field demands:")
    for t, n in collections.Counter(r["t"] for r in rows).most_common():
        print("  %-12s %3d   -> %s" % (t, n, CHANNEL.get(t, "unclassified")))

    print("\n=== distinct things that would have to be produced ===")
    tr = [r for r in rows if r["tgt_type"] == "Transform"]
    print("  #17 prefabs : %d distinct root objects (in %d containers), covering %d references"
          % (len({r["root"] for r in tr}), len({r["container"] for r in tr}), len(tr)))
    au = [r for r in rows if r["tgt_type"] == "AudioClip"]
    print("  audio clips : %d distinct objects, covering %d references"
          % (len({(r["container"], r["path_id"]) for r in au}), len(au)))
    fo = [r for r in rows if r["tgt_type"] == "Font"]
    print("  fonts       : %d distinct objects, covering %d references"
          % (len({(r["container"], r["path_id"]) for r in fo}), len(fo)))
    print("  unparseable : %d distinct objects, covering %d references (%s)"
          % (len({(r.get("file"), r.get("path_id")) for r in unresolvable}), len(unresolvable),
             {r["t"] for r in unresolvable}))
    levels_per_root = collections.defaultdict(set)
    for r in tr:
        levels_per_root[r["root"]].add(r["level"])
    shared = {k: sorted(v) for k, v in levels_per_root.items() if len(v) > 1}
    print("  prefabs referenced from more than one level: %d %s"
          % (len(shared), dict(list(shared.items())[:8])))

    hierarchy = {}

    def hier(fname):
        """Cached (go path_id -> name, go -> child gos, go -> father go) for one container."""
        if fname in hierarchy:
            return hierarchy[fname]
        objs = objects_of(fname)
        if not objs:
            hierarchy[fname] = None
            return None
        go_names, kids, dad = {}, collections.defaultdict(list), {}
        for pid, o in objs.items():
            if o.type.name not in ("GameObject", "Transform"):
                continue
            try:
                d = o.read()
            except Exception:
                continue
            if o.type.name == "GameObject":
                go_names[pid] = str(d.m_Name)
                continue
            gop = pid_of(d.m_GameObject)
            kids[gop]  # touch so a root with no children still has an entry
            f = d.m_Father
            if f is not None and int(getattr(f, "m_FileID", 0) or 0) == 0 and pid_of(f):
                ft, fd = data_of(fname, pid_of(f))
                if ft == "Transform":
                    father_go = pid_of(fd.m_GameObject)
                    kids[father_go].append(gop)
                    dad[gop] = father_go
        hierarchy[fname] = (go_names, kids, dad)
        return hierarchy[fname]

    # Subtree size, because "54 prefabs" and "54 objects" are different amounts of work. Only the
    # containers that actually hold a target are walked.
    def subtree_size(fname, root_name):
        h = hier(fname)
        if not h:
            return None
        go_names, kids, _dad = h
        best = 0
        for pid, n in go_names.items():
            if n != root_name:
                continue
            seen, stack = set(), [pid]
            while stack:
                cur = stack.pop()
                if cur in seen:
                    continue
                seen.add(cur)
                stack.extend(kids.get(cur, []))
            best = max(best, len(seen))
        return best

    per = {}
    for r in tr:
        key = (r["container"], r["root"])
        if key not in per:
            per[key] = subtree_size(*key)
    tot = sum(v for v in per.values() if v)
    unknown = sum(1 for v in per.values() if not v)
    dist = sorted(v for v in per.values() if v)
    print("  objects inside those prefabs: %d across %d (root,container) pairs"
          " (not measured: %d)" % (tot, len(per), unknown))
    if dist:
        print("  per-prefab object count: min %d, median %d, max %d -- %s"
              % (dist[0], dist[len(dist) // 2], dist[-1],
                 sorted(per.items(), key=lambda kv: -(kv[1] or 0))[:4]))

    print("\n=== the Transform targets, by root object ===")
    byroot = collections.Counter(r["root"] for r in rows if r["tgt_type"] == "Transform")
    for name, n in byroot.most_common():
        sel = [r for r in rows if r["root"] == name and r["tgt_type"] == "Transform"]
        reach = {r["reachable_by"] for r in sel}
        where = sorted({r["container"] for r in sel})
        who = sorted({"%s.%s" % (r["script"], r["field"]) for r in sel})
        objs = sorted({r["object"] for r in sel})
        print("  %-22s refs=%-3d %s\n      objects %s\n      referenced by %s\n      in %s"
              % (name, n, sorted(reach), objs[:6], who[:8], where))
    print("  distinct roots: %d" % len(byroot))

    if unresolvable:
        print("\n=== could not resolve (%d) ===" % len(unresolvable))
        for s, n in collections.Counter(r["status"] for r in unresolvable).most_common():
            print("  %-46s %d" % (s, n))
        for r in unresolvable[:12]:
            print("  %s %s.%s -> %s!%s : %s" % (r.get("level"), r.get("script"), r.get("field"),
                                                r.get("file"), r.get("path_id"), r["status"]))

    with open(OUT, "w", encoding="utf-8") as fh:
        json.dump({"resolved": rows, "unresolvable": unresolvable,
                   "prefab_subtrees": {"%s!%s" % k: v for k, v in per.items()}},
                  fh, indent=1, ensure_ascii=False)
    print("\nwrote %s" % OUT)
    return 0


if __name__ == "__main__":
    sys.exit(main())
