"""Read-only probe: which builtin m_Tag index is which tag name?

Why this needs a probe instead of documentation
------------------------------------------------
Unity 4 stores GameObject.m_Tag as an int. User tags are 20000 + index into TagManager.tags,
so those names are directly readable from ProjectSettings. Builtin tags occupy small ints and
NO file serialises their names -- TagManager.asset deliberately omits them. So "1 means
Respawn" has to come from evidence; a wrong guess silently retags objects.

The evidence is the referring side
----------------------------------
The shipped scripts always pair a tag with the component they take from it:

    GameObject.FindWithTag("Respawn").GetComponent<Spawn>()          Cha_Control.cs:460
    GameObject.FindWithTag("Finish").GetComponent<LoopArchive>()     UI_map.cs:374

so in the original build the "Respawn"-tagged GameObjects are the ones carrying Spawn /
Spawn_story / DB_Monster / DB_Boss, and the "Finish"-tagged ones carry LoopArchive / Tower_base.
Two independent axes have to agree before this prints a conclusion:
  * component signature -- the type the code asks for,
  * scene membership   -- the scene that hosts the calling script must host the tagged object.

Scoring is validated on known truth first
----------------------------------------
The 10 user tag names are known for certain, so the same scoring function is run against them.
If it does not recover those, it says nothing about the builtins and this probe prints a
refusal instead of a mapping.

Run:  <unitypy venv python> tools/migration/scan_builtin_tags.py
"""
import collections
import json
import os
import re
import struct
import sys

import UnityPy

import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCRIPTS = os.path.join(_REPO, "unity", "Assets", "Scripts")
BINDINGS = os.path.join(_REPO, "tools", "migration", "monoscript-bindings.json")
SCENES_JSON = os.path.join(_REPO, "tools", "migration", "scenes")

# Unity 4 TagManager user tags, declaration order (index == m_Tag - 20000). Restored from
# mainData and written to unity/ProjectSettings/TagManager.asset by restore_projectsettings.py.
USER_TAGS = ["efs_mon", "ui", "p_plan", "skill", "icon_skill", "sound", "dummy", "general",
             "ranking", "cashshop"]


# The names a small m_Tag index can possibly mean. Deliberately a SUPERSET of what Unity 4's
# Inspector dropdown offers, unioned at run time with every string the scripts pass to
# FindWithTag that is not a user tag -- the conclusion is only as wide as the candidates tried,
# so nothing that could occupy index 1..6 is left out just because it was not guessed up front.
UNITY4_BUILTIN_NAMES = ["Respawn", "Finish", "EditorOnly", "ScriptOnly", "Player", "MainCamera",
                        "PropectorOnly", "DestroyOnly"]


def requested_by_tag():
    """tag name -> {component type name -> number of call sites}.

    Two shapes are accepted, both of which are unambiguous in the shipped sources:
      FindWithTag("X").GetComponent<Y>()            (chained)
      GameObject go = FindWithTag("X"); go.GetComponent<Y>()   (same method, next few lines)
    `.transform` / SendMessage uses are recorded as markers so the reader can see the site
    exists without pretending it names a component.
    """
    got = collections.defaultdict(collections.Counter)
    sites = []
    chained = re.compile(r'FindWithTag\s*\(\s*"([^"]+)"\s*\)\s*\.\s*'
                         r'(?:GetComponent(?:<([A-Za-z_][\w]*)>)?|SendMessage\s*\(\s*"([^"]+)")')
    assigned = re.compile(r'(\w+)\s*=\s*(?:GameObject\.)?FindWithTag\s*\(\s*"([^"]+)"\s*\)')
    anycall = re.compile(r'FindWithTag\s*\(\s*"([^"]+)"')
    for root, _dirs, files in os.walk(SCRIPTS):
        for name in sorted(files):
            if not name.endswith(".cs"):
                continue
            full = os.path.join(root, name)
            with open(full, encoding="utf-8", errors="replace") as fh:
                lines = fh.readlines()
            text = "".join(lines)
            for m in anycall.finditer(text):
                sites.append((m.group(1), os.path.relpath(full, _REPO)))
            for i, line in enumerate(lines):
                for m in chained.finditer(line):
                    tag, comp, msg = m.group(1), m.group(2), m.group(3)
                    got[tag][comp or ("<SendMessage:%s>" % msg if msg else "<GetComponent>()")] += 1
                m = assigned.search(line)
                if not m:
                    continue
                var, tag = m.group(1), m.group(2)
                hit = None
                for follow in lines[i + 1:i + 6]:
                    fm = re.search(re.escape(var) + r'\s*\.\s*GetComponent(?:<([A-Za-z_][\w]*)>)?',
                                   follow)
                    if fm:
                        hit = fm.group(1) or "<GetComponent>()"
                        break
                    nm = re.search(re.escape(var) + r'\s*\.\s*(transform|gameObject)\b', follow)
                    if nm:
                        hit = "<%s>" % nm.group(1)
                        break
                got[tag][hit or "<not resolved within 5 lines>"] += 1
    return got, sites


def scan_levels(bindings):
    """Walk every level file and return the per-m_Tag component tallies described in main()."""
    monoscripts = bindings["monoscripts"]
    carriers = collections.defaultdict(collections.Counter)
    scenes_of = collections.defaultdict(set)
    names_of = collections.defaultdict(collections.Counter)
    camera_at = collections.Counter()
    cam_names = collections.defaultdict(collections.Counter)  # tag value -> camera object names
    per_script = collections.defaultdict(collections.Counter)   # component -> tag values seen
    comp_scenes = collections.defaultdict(collections.Counter)  # component -> level files
    cam_scenes = collections.Counter()                          # level files with a Camera
    in_scene = collections.defaultdict(collections.Counter)     # (level, m_Tag) -> components
    n_go = 0
    missing = []
    for i in range(23):
        target = "level%d" % i
        path = os.path.join(u4.DUMP, target)
        if not os.path.exists(path):
            missing.append(target)
            continue
        externals = bindings["level_externals"].get(target, [])
        env = UnityPy.load(path)

        def script_name(sc_file_id, sc_path_id, _ext=externals, _t=target):
            if sc_file_id == 0:
                key = "%s!%d" % (_t, sc_path_id)
            else:
                if sc_file_id - 1 >= len(_ext):
                    return None
                key = "%s!%d" % (_ext[sc_file_id - 1], sc_path_id)
            return monoscripts.get(key)

        comps = collections.defaultdict(set)
        for o in env.objects:
            tn = o.type.name
            if tn in ("GameObject", "Transform"):
                continue
            if tn == "MonoBehaviour":
                raw = bytes(o.get_raw_data())
                _gof, go, _en, scf, scp = struct.unpack_from("<iiiii", raw, 0)
                if go:
                    comps[go].add(script_name(scf, scp) or "<unresolved script>")
                continue
            try:
                d = o.read()
            except Exception:
                continue
            go = int(getattr(getattr(d, "m_GameObject", None), "m_PathID", 0) or 0)
            if go:
                comps[go].add(tn)

        for o in env.objects:
            if o.type.name != "GameObject":
                continue
            d = o.read()
            n_go += 1
            v = int(d.m_Tag)
            cs = comps.get(o.path_id, set())
            for c in cs:
                carriers[v][c] += 1
                per_script[c][v] += 1
                comp_scenes[c][target] += 1
                in_scene[(target, v)][c] += 1
            if not cs:
                carriers[v]["<no component scanned>"] += 1
            names_of[v][str(d.m_Name)] += 1
            scenes_of[v].add(target)
            if "Camera" in cs:
                camera_at[v] += 1
                cam_names[v][str(d.m_Name)] += 1
                cam_scenes[target] += 1
    return (carriers, scenes_of, names_of, camera_at, cam_names, per_script, comp_scenes,
            cam_scenes, in_scene, n_go, missing)


def label(v):
    if v >= 20000:
        return "user[%d]=%s" % (v - 20000, USER_TAGS[v - 20000])
    if v == 0:
        return "Untagged"
    return "builtin %d" % v


def carried(tag, v, carriers, req, script_names):
    """Script class names that BOTH: the scripts request from FindWithTag("tag"), AND actually sit
    on an object with m_Tag=v. Empty means no identifying evidence for this (tag, index) pair.

    Restricting to MonoBehaviour names is deliberate: a native component such as AudioSource sits
    on objects of every tag value, so it identifies nothing and would manufacture false matches.
    """
    want = {c for c in req.get(tag, ()) if not c.startswith("<")} & script_names
    have = {c for c in carriers.get(v, ()) if not c.startswith("<")}
    return sorted(want & have)


def caller_scenes_for(tag, sites, mb_scene):
    """level files hosting a script that calls FindWithTag("tag")."""
    out = set()
    for t, relpath in sites:
        if t == tag:
            out |= mb_scene.get(os.path.basename(relpath)[:-3], set())
    return sorted(out)


def main():
    bindings = json.load(open(BINDINGS, encoding="utf-8"))
    req, sites = requested_by_tag()
    carriers, scenes_of, names_of, camera_at, cam_names, per_script, comp_scenes, cam_scenes, \
        in_scene, n_go, missing = scan_levels(bindings)
    script_names = set(bindings["monoscripts"].values())
    values = sorted(carriers)

    mb_scene = collections.defaultdict(set)
    for fn in sorted(os.listdir(SCENES_JSON)) if os.path.isdir(SCENES_JSON) else []:
        if not fn.endswith(".json"):
            continue
        with open(os.path.join(SCENES_JSON, fn), encoding="utf-8") as fh:
            doc = json.load(fh)
        for node in doc.get("nodes", []):
            for c in node.get("components", []):
                if c.get("kind") == "MonoBehaviour" and c.get("script"):
                    mb_scene[c["script"]].add(fn[:-5])

    print("=== scanned %d GameObjects across %d level files%s ==="
          % (n_go, 23 - len(missing), (" (missing: %s)" % ",".join(missing)) if missing else ""))
    print("FindWithTag call sites in the whole script tree: %d, over %d distinct tag string(s): %s"
          % (len(sites), len({t for t, _ in sites}), sorted({t for t, _ in sites})))
    for v in values:
        print("\n%s  (m_Tag=%d)  scenes=%s" % (label(v), v, ",".join(sorted(scenes_of[v]))))
        print("   components: %s" % ", ".join("%s x%d" % kv for kv in carriers[v].most_common(12)))
        print("   names:      %s" % ", ".join("%s x%d" % kv for kv in names_of[v].most_common(6)))

    print("\n=== referring side: components each tag name is asked for ===")
    for tag in sorted(req):
        print("  %-12s %s" % (tag, req[tag].most_common(6)))

    print("\n=== exhaustive attribution: where does each requested component actually sit? ===")
    # "Respawn is index 1" also claims "those components sit at NO other index", so the complete
    # observed index set per component has to be shown, not just the winning one.
    for tag in sorted(req):
        for c in sorted(x for x in req[tag] if not x.startswith("<")):
            print("  %-11s %-22s m_Tag %-26s levels %s"
                  % (tag, c, str(dict(per_script.get(c, {"<in no level file>": 0}))),
                     ",".join(sorted(comp_scenes.get(c, ()))) or "-"))

    def identify(tag):
        """-> (status, winner_or_None, detail) using only MonoBehaviour evidence, over every
        observed m_Tag value."""
        hits = {v: carried(tag, v, carriers, req, script_names) for v in values}
        scored = {v: h for v, h in hits.items() if h}
        want_scripts = sorted({c for c in req.get(tag, ()) if not c.startswith("<")} & script_names)
        if not want_scripts:
            return "no-evidence", None, ("asks only for %s -- native components sit at every index "
                                         "and identify nothing"
                                         % sorted({c for c in req.get(tag, ())
                                                   if not c.startswith("<")}))
        if not scored:
            return "absent", None, "none of %s sits on any object in these 23 level files" \
                % want_scripts
        best = max(len(h) for h in scored.values())
        winners = sorted(v for v, h in scored.items() if len(h) == best)
        detail = "requested %s; %s" % (want_scripts,
                                       "; ".join("m_Tag=%d carries %s" % (v, scored[v])
                                                 for v in sorted(scored)))
        if len(winners) == 1 and len(scored) == 1:
            return "unique", winners[0], detail
        return "ambiguous", (winners[0] if len(winners) == 1 else None), detail

    print("\n=== calibration against the 10 KNOWN user tags ===")
    validated = 0
    for idx, tag in enumerate(USER_TAGS):
        status, win, detail = identify(tag)
        true_v = 20000 + idx
        if status == "unique" and win == true_v:
            validated += 1
            print("  %-12s OK       recovered m_Tag=%d" % (tag, true_v))
        elif status == "no-evidence":
            print("  %-12s no data   %s" % (tag, detail))
        elif status == "absent":
            print("  %-12s absent    %s (true index is %d; the component must live in a prefab)"
                  % (tag, detail, true_v))
        else:
            print("  %-12s MISMATCH  got %s, true m_Tag=%d -- %s" % (tag, win, true_v, detail))
    print("  uniquely recovered: %d / %d" % (validated, len(USER_TAGS)))
    if validated == 0:
        print("\nMETHOD NOT VALIDATED -- this run says nothing about the builtin indices.")
        return 1

    print("\n=== full matrix: every candidate name x every observed index ===")
    # Small indices can only mean builtin names, but any tag string the scripts look up that is
    # not a user tag joins the candidate set so it cannot be missed.
    candidates = sorted(set(UNITY4_BUILTIN_NAMES)
                        | ({t for t, _s in sites} - set(USER_TAGS))
                        - {"Untagged"})
    for name in candidates:
        status, win, detail = identify(name)
        print("  %-15s %-11s %s" % (name, status, detail))

    print("\n=== consequence check: for each observed index, which code could look it up? ===")
    for v in values:
        if v == 0 or v >= 20000:
            continue
        claims = [n for n in candidates if identify(n)[1] == v and identify(n)[0] == "unique"]
        print("  m_Tag=%d : pinned as %s" % (v, claims or "NOTHING -- no FindWithTag argument "
                                                           "matches these objects"))

    print("\n=== camera-bearing objects per index (the MainCamera axis; no script uses the name) ===")
    for v in sorted(camera_at):
        print("  m_Tag=%-6d %2d camera object(s): %s"
              % (v, camera_at[v], ", ".join("%s x%d" % kv for kv in cam_names[v].most_common(6))))
    print("  %d of 23 levels contain a Camera at all" % len(cam_scenes))

    print("\n=== scene-scoped identification ===")
    # FindWithTag searches the loaded scene, not the whole APK, so cross-scene noise is not
    # ambiguity. Per caller scene: which m_Tag values carry a component the code asks for.
    scoped = {}
    for name in candidates:
        want_scripts = sorted({c for c in req.get(name, ()) if not c.startswith("<")}
                              & script_names)
        cs = caller_scenes_for(name, sites, mb_scene)
        if not want_scripts or not cs:
            continue
        per_scene = {}
        for x in cs:
            vals = sorted(vv for (xx, vv) in in_scene if xx == x)
            hits = {vv: sorted(c for c in in_scene[(x, vv)] if c in want_scripts) for vv in vals}
            per_scene[x] = {vv: h for vv, h in hits.items() if h}
        one = {v for h in per_scene.values() for v in h}
        empty = sorted(x for x, h in per_scene.items() if not h)
        scoped[name] = (sorted(one), empty)
        verdict = ("UNIQUE m_Tag=%s in every scene that has one" % sorted(one)
                   if len(one) == 1 else "not settled: %s" % sorted(one))
        print("  %-12s wants %-46s %s" % (name, str(want_scripts), verdict))
        if empty:
            print("        no candidate object in %s -- the sought object must be spawned at "
                  "runtime or persist from another scene" % ",".join(empty))
        for x in sorted(per_scene):
            if per_scene[x]:
                print("        %-8s %s" % (x, per_scene[x]))

    print("\n=== scene correspondence: caller scene must host the tagged object ===")
    for name, (one, empty) in sorted(scoped.items()):
        if len(one) != 1:
            continue
        v = one[0]
        cs = caller_scenes_for(name, sites, mb_scene)
        obj = sorted(scenes_of[v])
        extra = sorted(set(cs) - set(obj))
        print("  %-12s m_Tag=%d objects in %-24s callers in %-24s %s"
              % (name, v, ",".join(obj), ",".join(cs) or "-",
                 ("gap in %s (see above)" % ",".join(extra)) if extra else "OK"))

    print("\n=== what to write into extract_scene.py ===")
    mapping = {}
    for name, (one, _e) in sorted(scoped.items()):
        if len(one) == 1:
            mapping.setdefault(one[0], name)
    # MainCamera is settled by a different axis: Camera.main is the only way the scripts reach it,
    # so the objects it must return are the camera-bearing ones. The candidate index is the one
    # whose objects are ALL cameras named exactly "Main Camera"/"MainCamera" -- index 0 cannot be
    # it, because its 21 cameras are named Main_Camera / Camera_UI / Camera_2dbg ... and untagged.
    main_cam = [v for v in values
                if camera_at[v] and camera_at[v] == sum(names_of[v].values())
                and set(names_of[v]) <= {"Main Camera", "MainCamera"}]
    print("  MainCamera axis: indices whose every object is a Main Camera-named camera: %s"
          % main_cam)
    if len(main_cam) == 1:
        mapping.setdefault(main_cam[0], "MainCamera")
    for v in values:
        if v == 0 or v >= 20000:
            continue
        print("  %d: %-14s  (observed in: %s)"
              % (v, '"%s"' % mapping[v] if v in mapping else "None  # undetermined",
                 ",".join(sorted(scenes_of[v]))))
    print("  observed small indices are exactly %s; Unity's other builtin names never occur in "
          "these 23 files, so they stay unmapped and would still warn."
          % sorted(v for v in values if 0 < v < 20000))
    return 0


if __name__ == "__main__":
    sys.exit(main())
