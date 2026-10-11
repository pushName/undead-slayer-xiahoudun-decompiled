"""Verify rebuilt .unity files against the level JSONs they were generated from. Read-only.

WHY THIS EXISTS

The Editor logs a per-scene summary, but that summary counts what the tool INTENDED to add.
This reads the file Unity actually wrote and compares it with what tools/migration/extract_scene.py
claimed the source scene contained. The two agreeing is the acceptance criterion; "the menu item
ran without throwing" is not.

Unity owns scene serialisation (see the README's "let the owning tool serialise" rule), so this
script deliberately parses 2022 YAML by class-id header rather than assuming any field layout.

Run:  python tools/migration/verify_scenes.py [--verbose]
Exit: 0 only if every rebuilt scene matches; 1 otherwise.
"""
import collections
import glob
import json
import os
import re
import sys

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCENE_DIR = os.path.join(REPO, "unity", "Assets", "Scene")
JSON_DIR = os.path.join(REPO, "tools", "migration", "scenes")

# Unity class IDs that appear as "--- !u!<id>" in a scene file.
CLASS = {
    1: "GameObject", 4: "Transform", 20: "Camera", 23: "MeshRenderer", 33: "MeshFilter",
    54: "Rigidbody", 64: "MeshCollider", 65: "BoxCollider", 81: "AudioListener",
    82: "AudioSource", 108: "Light", 111: "Animation", 114: "MonoBehaviour",
    124: "FlareLayer", 135: "SphereCollider", 136: "CapsuleCollider",
    137: "SkinnedMeshRenderer", 14: "CharacterController", 59: "HingeJoint",
    138: "ConfigurableJoint", 145: "WheelCollider", 92: "GUILayer",
    12: "ParticleAnimator", 15: "EllipsoidParticleEmitter", 26: "ParticleRenderer",
}
# Unity 4 native components that do not exist in 2022. extract_scene.py marks these "unsupported"
# and SceneRebuilder skips them, so finding one in a rebuilt file would mean it was invented.
LEGACY_FORBIDDEN = {12: "ParticleAnimator", 15: "EllipsoidParticleEmitter", 26: "ParticleRenderer",
                    92: "GUILayer", 124: "FlareLayer"}
# MonoBehaviour bindings SceneRebuilder legitimately cannot attach. Verified against the shipped
# assembly, not inferred: `ilspycmd -t ChannelMgr dump/assets/Managed/Assembly-CSharp.dll` prints
# `public class ChannelMgr` with no base type, so it is not a Component in the original game either.
KNOWN_NON_MONO = {"ChannelMgr"}
# PPtrs to these always resolve inside the file, so they are not asset references.
NOT_AN_ASSET = {
    "0000000000000000e000000000000000": "builtin extra (unity default extra resources)",
    "0000000000000000f000000000000000": "builtin resources (unity default resources)",
    "00000000000000004000000000000000": "builtin resource manager",
    "00000000000000005100000000000000": "prefab layout (empty)",
}


def guid_index():
    """guid -> project-relative asset path, from every .meta under unity/Assets."""
    out = {}
    skipped_meta = []
    assets_root = os.path.join(REPO, "unity")
    for root, _dirs, files in os.walk(assets_root):
        for fn in files:
            if not fn.endswith(".meta"):
                continue
            try:
                with open(os.path.join(root, fn), "r", encoding="utf-8", errors="replace") as fh:
                    m = re.search(r"^guid: ([0-9a-f]{32})\s*$", fh.read(), re.M)
            except OSError:
                # The Editor regenerates .meta files in place, so a walk can race an import and
                # find the entry gone between listing and opening it. Skip it and say so -- the
                # alternative is a traceback that looks like a scene defect.
                skipped_meta.append(os.path.relpath(os.path.join(root, fn), REPO))
                continue
            if m:
                rel = os.path.relpath(os.path.join(root, fn)[:-5], assets_root)
                out[m.group(1)] = rel.replace(os.sep, "/")
    if skipped_meta:
        print("note: %d .meta unreadable during the guid sweep (Editor import race), e.g. %s"
              % (len(skipped_meta), skipped_meta[:2]))
    return out


def expected(json_path):
    """What extract_scene.py says the source scene contains."""
    with open(json_path, "r", encoding="utf-8") as fh:
        d = json.load(fh)
    kinds = collections.Counter()
    scripts = collections.Counter()
    mat_slots = mesh_slots = 0
    mat_paths = set()
    mesh_paths = set()
    clip_paths = set()
    audio_clips = set()
    audio_missing = set()
    audio_slots = 0
    for n in d["nodes"]:
        for a in n.get("animations") or []:
            p = a.get("asset")
            if p and os.path.exists(os.path.join(REPO, "unity", p)):
                clip_paths.add(p)
        for c in n.get("components") or []:
            kinds[c.get("kind")] += 1
            if c.get("kind") == "MonoBehaviour":
                scripts[c.get("script")] += 1
            if c.get("kind") == "audio":
                # Counted on kind, not on the presence of a clip: 40 of the 67 sources have a null
                # clip in the build, and keying the component count to the clip would make the
                # "a component was dropped" check fire on every one of them.
                audio_slots += 1
                if c.get("clip"):
                    p = c["clip"]
                    if os.path.exists(os.path.join(REPO, "unity", p)):
                        audio_clips.add(p)
                    else:
                        audio_missing.add(p)
                elif "clip" in c:
                    # The key present but null: the build pointed at a clip that audio.json does not
                    # name, so nothing can be written for it. Reported rather than left quiet --
                    # without this the witness would only see "no clip expected here".
                    audio_missing.add("<not in audio.json>")
            if c.get("type") in ("MeshRenderer", "SkinnedMeshRenderer"):
                for p in c.get("materials") or []:
                    mat_slots += 1
                    if p and os.path.exists(os.path.join(REPO, "unity", p)):
                        mat_paths.add(p)
            # Any component that carries its own mesh PPtr: MeshFilter AND SkinnedMeshRenderer do,
            # plain MeshRenderer does not.
            if "mesh" in c:
                p = c.get("mesh")
                if p:
                    mesh_slots += 1
                    if os.path.exists(os.path.join(REPO, "unity", p)):
                        mesh_paths.add(p)
    return {
        "scene_name": d["scene_name"],
        "source_level": d["source_level"],
        "nodes": len(d["nodes"]),
        "meshfilter": sum(1 for n in d["nodes"] for c in (n.get("components") or [])
                          if c.get("type") == "MeshFilter"),
        "renderer": sum(1 for n in d["nodes"] for c in n.get("components") or []
                        if c.get("type") in ("MeshRenderer", "SkinnedMeshRenderer")),
        "attachable": sum(v for k, v in scripts.items() if k not in KNOWN_NON_MONO),
        "unattachable": sum(v for k, v in scripts.items() if k in KNOWN_NON_MONO),
        "unsupported": dict(d.get("unsupported_components") or {}),
        "scripts": dict(scripts),
        "component_kinds": dict(kinds),
        "mat_slots": mat_slots,
        "mat_paths_here": mat_paths,
        "mesh_slots": mesh_slots,
        "mesh_paths_here": mesh_paths,
        "clip_paths_here": clip_paths,
        # The clip an AudioSource component carries is a native field, not a MonoBehaviour one, so
        # no field_refs witness above can see it. Counted here and compared with !u!82 below.
        "audio_slots": audio_slots,
        "audio_clips_here": audio_clips,
        "audio_clips_missing": audio_missing,
    }


def expected_refs(json_path):
    """The reference-valued fields extract_scene.py decoded, as (field, want) pairs.

    want is an asset path for kind "asset" and "local" for a same-scene object reference. This is
    a scene-level set, not per component: the YAML does not tie a field back to the MonoBehaviour
    that declared it without re-parsing m_Script per doc, and the question asked here is "did the
    value get written at all", which the field name answers. Instance-level binding is what
    SceneRebuilder's own post-check already does.
    """
    d = json.load(open(json_path, encoding="utf-8"))
    want = []
    for n in d["nodes"]:
        for c in n.get("components") or []:
            if c.get("kind") != "MonoBehaviour":
                continue
            for f in c.get("fields") or []:
                if f.get("kind") == "asset":
                    want.append((f["n"], f["asset"]))
                elif f.get("kind") == "node":
                    want.append((f["n"], "local"))
                elif f.get("kind") == "arr":
                    for it in f.get("items") or []:
                        if it.get("kind") == "asset":
                            want.append((f["n"], it["asset"]))
                        elif it.get("kind") == "node":
                            want.append((f["n"], "local"))
    return want


def _mark_pptr(field_refs, name, file_id, guid, guids):
    """Credit a MonoBehaviour field only when its PPtr actually holds something."""
    if int(file_id) == 0 and not guid:
        return
    if guid:
        field_refs[name].add(guids.get(guid) or ("guid:" + guid[:12]))
    else:
        field_refs[name].add("local")


def actual(scene_path, guids):
    with open(scene_path, "r", encoding="utf-8", errors="replace") as fh:
        text = fh.read()
    counts = collections.Counter()
    for tid in re.findall(r"^--- !u!(\d+)", text, re.M):
        counts[CLASS.get(int(tid), "type" + tid)] += 1
    refs = collections.Counter(re.findall(r"guid: ([0-9a-f]{32})", text))
    unresolved = {g: c for g, c in refs.items() if g not in guids and g not in NOT_AN_ASSET}
    # Which asset paths the file actually references, per field kind. Counting NULL slots is the
    # wrong test: a slot can be legitimately null for reasons that have nothing to do with
    # staleness. Two false positives proved it -- a MeshFilter whose mesh extract_scene.py could
    # not resolve is written with NO mesh key at all (so nothing was expected, yet it serialises
    # m_Mesh: {fileID: 0}), and ParticleSystemRenderer (!u!199) has its own m_Mesh field too.
    # The precise question is containment: does every expected, now-existing asset path appear as a
    # non-null reference of the right kind?
    mesh_guids = set(re.findall(r"^  m_Mesh: \{fileID: (?!0\})[^}]*guid: ([0-9a-f]{32})", text, re.M))
    mat_guids = set()
    empty_by_field = collections.Counter()
    owner = "?"
    for line in text.split("\n"):
        owned = re.match(r"^  (\w+):", line)
        if owned:
            owner = owned.group(1)
            continue
        if re.match(r"^\s*-\s*\{fileID:\s*0\}\s*$", line):
            empty_by_field[owner] += 1
        elif owner == "m_Materials":
            g = re.search(r"guid: ([0-9a-f]{32})", line)
            if g:
                mat_guids.add(g.group(1))
    tags = collections.Counter(re.findall(r"^  m_TagString: (.*)$", text, re.M))
    active = collections.Counter(re.findall(r"^  m_IsActive: (\d)$", text, re.M))
    # Null references held by MonoBehaviour fields -- the size of the "field values not decoded"
    # job. Counting only LIST entries ("- {fileID: 0}") understated it badly: Cam_intro's public
    # Transform fields serialise as SCALAR lines (cha: {fileID: 0}), so Intro read as nullRef=2
    # while cha/gate/UI_intro/mon1/mon2 were all null. Walk !u!114 docs and take both shapes,
    # skipping the PPtr fields Unity itself writes on every object.
    reserved = {"m_CorrespondingSourceObject", "m_PrefabInstance", "m_PrefabAsset",
                "m_GameObject", "m_Script", "m_Mesh", "m_UpdateWhenInvisible"}
    null_refs = collections.Counter()
    null_by_script = collections.Counter()
    for doc in re.split(r"(?m)^--- ", text)[1:]:
        if not doc.startswith("!u!114"):
            continue
        script = re.search(r"^  m_Script: \{fileID: 11500000, guid: ([0-9a-f]{32})", doc, re.M)
        who = os.path.basename(guids[script.group(1)]) if script and script.group(1) in guids else "?"
        mine = 0
        for key in re.findall(r"^  (\w+): \{fileID: 0\}", doc, re.M):
            if key not in reserved:
                null_refs[key] += 1
                mine += 1
        owner = "?"
        for line in doc.split("\n"):
            owned = re.match(r"^  (\w+):", line)
            if owned:
                owner = owned.group(1)
            elif re.match(r"^\s*-\s*\{fileID:\s*0\}\s*$", line) and owner not in reserved:
                null_refs[owner] += 1
                mine += 1
        if mine:
            null_by_script[who] += mine
    null_ref_total = sum(null_by_script.values())
    # What the reference-valued fields actually hold. Every !u!114 field that carries a PPtr is
    # recorded as field -> {resolved asset path or "local"}; a field is only credited when the
    # PPtr is non-null, so "the Editor accepted the assignment" cannot count as landed here.
    field_refs = collections.defaultdict(set)
    pp_re = re.compile(r"^  (\w+): \{fileID: (\d+)(?:, guid: ([0-9a-f]{32}))?")
    item_re = re.compile(r"^\s*-\s*\{fileID: (\d+)(?:, guid: ([0-9a-f]{32}))?")
    for doc in re.split(r"(?m)^--- ", text)[1:]:
        if not doc.startswith("!u!114"):
            continue
        owner = None
        for line in doc.split("\n"):
            owned = re.match(r"^  (\w+):", line)
            m = pp_re.match(line)
            mi = item_re.match(line)
            if owned:
                # A list-valued field owns the "- {fileID: ...}" lines under it, and its own line
                # carries no PPtr at all, so the owner has to be tracked separately from the match.
                owner = None if (owned.group(1) in reserved
                                 or owned.group(1) == "m_Script") else owned.group(1)
            if owner is None:
                continue
            if m:
                _mark_pptr(field_refs, owner, m.group(2), m.group(3), guids)
            elif mi:
                _mark_pptr(field_refs, owner, mi.group(1), mi.group(2), guids)
    # AudioSource's clip is a field of a NATIVE component (!u!82), so the !u!114 walk above cannot
    # see it, and neither can any field-level witness. Read it straight off the component docs:
    # a non-zero fileID+guid is a clip that landed, whatever the writer logged. The key is
    # m_audioClip in Unity 2022's YAML -- searching for plain "clip:" matches nothing and made a
    # successful build look like a failed one.
    audio_clips = set()
    for doc in re.split(r"(?m)^--- ", text)[1:]:
        if not doc.startswith("!u!82"):
            continue
        for g in re.findall(r"^  m_audioClip: \{fileID: [1-9]\d*(?:, guid: ([0-9a-f]{32}))?",
                            doc, re.M):
            if guids.get(g):
                audio_clips.add(guids[g])
    audio_sources = len(re.findall(r"(?m)^--- !u!82 ", text))
    # Calibration guard: count the key itself, null or not. If it never appears while the scene is
    # expected to hold AudioSources, the YAML key changed under us -- and reporting "clips missing"
    # for that would be the same false verdict the line above produced once already.
    audio_clip_lines = len(re.findall(r"^  m_audioClip:", text, re.M))
    return {
        "counts": dict(counts),
        "field_refs": {k: sorted(v) for k, v in field_refs.items()},
        "distinct_guid_refs": len(refs),
        "unresolved": unresolved,
        "mesh_guids": {guids.get(g) for g in mesh_guids if guids.get(g)},
        "mat_guids": {guids.get(g) for g in mat_guids if guids.get(g)},
        # Animation clips the scene's Animation components reference. Animation's clip list is
        # rebuilt by SceneRebuilder through Animation.AddClip, and whether that survives
        # serialisation was an open question -- so it is checked here instead of trusted.
        "clip_guids": {g for g in refs
                       if guids.get(g, "").startswith("Assets/Imported/Animations")},
        "empty_materials": empty_by_field.get("m_Materials", 0),
        "null_script_refs": null_ref_total,
        "null_ref_fields": dict(null_refs),
        "null_by_script": dict(null_by_script),
        "null_mesh": len(re.findall(r"^  m_Mesh: \{fileID: 0\}", text, re.M)),
        "audio_clips": audio_clips,
        "audio_sources": audio_sources,
        "audio_clip_lines": audio_clip_lines,
        "tags": dict(tags),
        "active": dict(active),
        "bytes": len(text),
    }


def build_settings():
    """EditorBuildSettings entries must carry the guid Unity actually assigned to each .unity.

    Application.LoadLevel("<name>") -- the only way this game changes scene -- resolves through
    this list, so a stale guid means a scene that silently cannot load. This is a permanent check
    because the obvious fix from inside the Editor does not persist: assigning
    EditorBuildSettings.scenes leaves ProjectSettings/EditorBuildSettings.asset byte-unchanged
    (observed: 24/24 entries still md5(path), file mtime unmoved), which is why the supported
    route is restore_projectsettings.py reading the real guids out of the .meta files.
    """
    p = os.path.join(REPO, "unity", "ProjectSettings", "EditorBuildSettings.asset")
    with open(p, "r", encoding="utf-8", errors="replace") as fh:
        text = fh.read()
    entries = re.findall(r"^    path: (.+)$\n^    guid: ([0-9a-f]{32})$", text, re.M)
    good, stale, absent = [], [], []
    for path, guid in entries:
        meta = os.path.join(REPO, "unity", path + ".meta")
        if not os.path.exists(meta):
            absent.append(path)
            continue
        with open(meta, "r", encoding="utf-8", errors="replace") as fh:
            m = re.search(r"^guid: ([0-9a-f]{32})\s*$", fh.read(), re.M)
        (good if (m and m.group(1) == guid) else stale).append(path)
    return len(entries), good, stale, absent


def main():
    verbose = "--verbose" in sys.argv
    guids = guid_index()
    # reverse map: an expected asset path -> the guid the scene has to reference
    path_of = {v: k for k, v in guids.items()}
    rows = []
    failures = []
    # A bare total is a snapshot, and a snapshot rots: the README carried "1319 null references"
    # long after re-exports and rebuilds had brought it down to 543, because nothing re-derived it
    # and nothing said WHO holds them. Print the by-class attribution on every run so the number and
    # its owner move together, and so the next stale claim is one glance away from being caught.
    null_by_script_all = collections.Counter()
    for jp in sorted(glob.glob(os.path.join(JSON_DIR, "level*.json")),
                     key=lambda p: int(re.search(r"level(\d+)", p).group(1))):
        exp = expected(jp)
        name = exp["scene_name"]
        sp = os.path.join(SCENE_DIR, name + ".unity")
        if not os.path.exists(sp):
            rows.append((exp["source_level"], name, "-", "-", "-", "-", "-", "-", "-", "-",
                         "pending"))
            continue
        act = actual(sp, guids)
        go = act["counts"].get("GameObject", 0)
        mf = act["counts"].get("MeshFilter", 0)
        rd = act["counts"].get("MeshRenderer", 0) + act["counts"].get("SkinnedMeshRenderer", 0)
        mb = act["counts"].get("MonoBehaviour", 0)

        problems = []
        if go != exp["nodes"]:
            problems.append("GameObject %d != json nodes %d" % (go, exp["nodes"]))
        if mf != exp["meshfilter"]:
            problems.append("MeshFilter %d != json %d" % (mf, exp["meshfilter"]))
        if rd != exp["renderer"]:
            problems.append("renderers %d != json %d" % (rd, exp["renderer"]))
        # MonoBehaviour bindings to non-MonoBehaviour classes are skipped on purpose (see
        # KNOWN_NON_MONO), so compare against the attachable subset only.
        if mb != exp["attachable"]:
            problems.append("MonoBehaviour %d != json attachable %d (%d bound to "
                            "non-MonoBehaviour class(es) skipped)"
                            % (mb, exp["attachable"], exp["unattachable"]))
        if act["unresolved"]:
            problems.append("%d unresolved asset ref(s): %s"
                            % (sum(act["unresolved"].values()), sorted(act["unresolved"])[:3]))
        # STALENESS, stated as containment. A null slot carries no guid, so "0 unresolved refs"
        # says nothing about it -- Loading.unity proved that (rebuilt at 11:19, before the mesh and
        # material exports, every one of its 20 refs serialised as {fileID: 0} while "unresolved"
        # stayed at 0). But count-of-nulls is ALSO the wrong test, because nulls are legitimate for
        # reasons unrelated to staleness. So ask the precise question instead.
        missing_mesh = exp["mesh_paths_here"] - act["mesh_guids"]
        if missing_mesh:
            problems.append("%d mesh asset(s) exist now but no m_Mesh here points at them -- this "
                            "scene predates the mesh export, rebuild it: %s"
                            % (len(missing_mesh), ", ".join(sorted(missing_mesh)[:3])))
        missing_mat = exp["mat_paths_here"] - act["mat_guids"]
        if missing_mat:
            problems.append("%d .mat exist now but no m_Materials entry points at them -- rebuild: %s"
                            % (len(missing_mat), ", ".join(sorted(missing_mat)[:3])))
        want_clips = {path_of.get(p) for p in exp["clip_paths_here"]} - {None}
        missing_clip = want_clips - act["clip_guids"]
        if missing_clip:
            problems.append("%d bound animation clip(s) are not referenced by this scene -- the "
                            "clip list did not serialise, rebuild it: %s"
                            % (len(missing_clip), ", ".join(sorted(missing_clip)[:2])))
        # The reference-valued fields are checked the same way: what the decoder says a field
        # should point at, against what the written file holds in that field. SceneRebuilder
        # already re-reads after Apply, but that is the same process that did the writing -- this
        # is the independent witness, and it is what catches "the export appeared after the
        # scene was built".
        wrefs = expected_refs(jp)
        absent = [(f, w) for f, w in wrefs if w not in act["field_refs"].get(f, ())]
        if absent:
            problems.append("%d/%d decoded field references are not in the file -- the scene "
                            "predates the export they point at, rebuild it. First: %s"
                            % (len(absent), len(wrefs),
                               ", ".join("%s->%s" % (f, os.path.basename(w))
                                         for f, w in absent[:3])))
        missing_audio = exp["audio_clips_here"] - act["audio_clips"]
        if missing_audio:
            problems.append("%d AudioSource clip(s) the description names are not in the file -- "
                            "rebuild this scene. First: %s"
                            % (len(missing_audio), sorted(missing_audio)[:2]))
        if exp["audio_slots"] and not act["audio_clip_lines"]:
            problems.append("%d AudioSource(s) expected but the file has no m_audioClip key at all "
                            "-- the YAML field name this check searches for has changed, so the clip "
                            "verdicts here are unreadable, not failed" % exp["audio_slots"])
        if exp["audio_clips_missing"]:
            problems.append("%d AudioSource clip(s) the build points at cannot be resolved from "
                            "audio.json: %s" % (len(exp["audio_clips_missing"]),
                                                 sorted(exp["audio_clips_missing"])[:2]))
        if act["audio_sources"] != exp["audio_slots"]:
            problems.append("%d AudioSource components expected, %d in the file -- a component was "
                            "dropped, not just its clip" % (exp["audio_slots"], act["audio_sources"]))
        for tid, label in LEGACY_FORBIDDEN.items():
            if act["counts"].get(label, 0):
                problems.append("legacy component %s survived (it does not exist in 2022)" % label)
        if problems:
            failures.append((name, problems))
        null_by_script_all.update(act.get("null_by_script") or {})
        rows.append((exp["source_level"], name, go, mf, rd, mb, act["null_mesh"],
                     act["empty_materials"], len(exp["clip_paths_here"]),
                     act["null_script_refs"], "OK" if not problems else "FAIL"))
        for p in problems:
            print("   !! %s: %s" % (name, p))
        if verbose:
            print("   -- %s tags=%s active=%s refs=%d bytes=%d"
                  % (name, act["tags"], act["active"], act["distinct_guid_refs"], act["bytes"]))

    print()
    print("%-9s %-14s %6s %5s %5s %5s %6s %5s %5s %8s %6s"
          % ("dump", "scene", "GO", "MeshF", "Rend", "MonoB", "noMesh", "noMat", "clips",
             "nullRef", "result"))
    for r in rows:
        print("%-9s %-14s %6s %5s %5s %5s %6s %5s %5s %8s %6s" % r)
    built = sum(1 for r in rows if r[2] != "-")
    print("null object references by script: %d total over %d class(s); %s"
          % (sum(null_by_script_all.values()), len(null_by_script_all),
             ", ".join("%s %d" % kv for kv in null_by_script_all.most_common(6))))
    total, good, stale, absent = build_settings()
    print("\nbuild settings: %d entries, %d guid(s) match the .meta, %d stale, %d scene file absent"
          % (total, len(good), len(stale), len(absent)))
    if stale:
        print("   stale (LoadLevel cannot resolve these): " + ", ".join(os.path.basename(p) for p in stale[:6]))
    if absent:
        print("   missing file: " + ", ".join(os.path.basename(p) for p in absent[:6]))
    if stale or absent:
        failures.append(("EditorBuildSettings", ["%d stale, %d absent" % (len(stale), len(absent))]))

    print("scenes rebuilt: %d/23   failing: %d" % (built, len(failures)))
    if failures:
        print("FAIL")
        return 1
    print("PASS" if built == 23 else "PASS for the %d scene(s) present; %d still missing"
          % (built, 23 - built))
    return 0


if __name__ == "__main__":
    sys.exit(main())
