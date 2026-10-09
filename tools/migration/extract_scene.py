"""Extract one Unity 4 level file into a JSON description that the Editor can rebuild from.

WHY JSON + an Editor script, instead of writing .unity YAML directly
Unity 4 player builds carry no TypeTree, and Unity 2022's scene YAML has many required
fields per object class (serializedVersion numbers, m_CorrespondingSourceObject, the
SceneRoots block, scene-manager objects...). Writing that from memory produces files that
either fail to import or, worse, import while silently dropping data. So this tool emits a
description and Assets/Editor/SceneRebuilder.cs lets Unity serialise the real file.

WHAT THIS CAPTURES (v1)
  hierarchy (Transform father/children), name, layer, tag, active flag, local TRS,
  the component *set* of each GameObject, and for MonoBehaviour the bound script class
  + the .cs guid from monoscript-bindings.json.

WHAT IT DELIBERATELY OMITS
  * MonoBehaviour field values. They live at offset 24 of each MonoBehaviour's raw bytes
    and can only be decoded per-class from Assembly-CSharp (no TypeTree). Components are
    therefore created with default field values -- structure, not state.
  * Mesh / Material / Texture / AudioClip references -- those assets do not exist in the
    project until task #14 exports them.
  * Component types that no longer exist in 2022 (ParticleAnimator, ParticleRenderer,
    EllipsoidParticleEmitter, FlareLayer, GUILayer). These are counted under
    "unsupported_components" so the gap stays visible instead of vanishing.

Run:  <unitypy venv python> tools/migration/extract_scene.py level7
"""
import collections
import json
import os
import re
import struct
import sys

import UnityPy

import unity4_files as u4
import monobehaviour_fields as MF

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DUMP = os.path.join(_REPO, "dump", "assets")
BINDINGS = os.path.join(_REPO, "tools", "migration", "monoscript-bindings.json")
OUT_DIR = os.environ.get("SCENE_OUT") or os.path.join(_REPO, "tools", "migration", "scenes")

# levelN == BuildSettings.levels[N+1]; proven by which controller script each scene binds
# (level22 -> Ending, level7 -> UI_general, level19 -> UI_result, ...). See README gap 5.
SCENE_ORDER = ["Ready", "Intro", "Story", "Story_2", "Story_3", "Story_4", "Story_5", "Map",
               "General", "Status", "Forge", "Skill", "Pet", "Archive", "Extreme", "Friends",
               "Loading", "Loading2", "Stage", "Stage_ride", "Result", "Result_ride",
               "Result_Extreme", "Ending"]

# Unity 4 TagManager user tags, in declaration order (index == m_Tag - 20000).
USER_TAGS = ["efs_mon", "ui", "p_plan", "skill", "icon_skill", "sound", "dummy", "general",
             "ranking", "cashshop"]

# Builtin tag indices. Their NAMES are never serialised (TagManager.asset lists user tags only),
# so each one below is pinned by two independent axes -- see tools/migration/scan_builtin_tags.py,
# which calibrates the method on the 10 known user tags first (7/10 uniquely recovered, the other
# 3 reported as uncheckable rather than counted as failures) and then:
#   1 Respawn     the only index carrying Spawn / Spawn_story / DB_Monster, which is exactly what
#                 FindWithTag("Respawn").GetComponent<Spawn>() (Cha_Control.cs:460) asks for; and
#                 it appears in level1+level17, the two scenes hosting the callers.
#   2 Finish      the only index carrying LoopArchive, asked for by FindWithTag("Finish") in
#                 UI_map.cs:374 / UI_Extreme.cs:374; appears in exactly level6(Map)+level13(Extreme).
#   5 MainCamera  no script uses the name, Camera.main does: every object at 5 is a camera named
#                 "Main Camera"/"MainCamera" (14 of 14), while the 21 untagged cameras are named
#                 Main_Camera / Camera_UI / Camera_2dbg ... so 0 cannot be it.
#   6 Player      the only index carrying Cha_Control / Cha_Skill / Cha_Control_ride_cha in every
#                 scene that calls FindWithTag("Player").
# 3 and 4 are deliberately left unmapped: they do not occur in any of the 23 level files, so
# nothing here claims to know them, and a warning still fires if one ever appears.
BUILTIN_TAGS = {0: None, 1: "Respawn", 2: "Finish", 5: "MainCamera", 6: "Player"}

# Native component classes that still exist in 2022 and can simply be AddComponent'd.
SUPPORTED = {
    "MeshRenderer", "SkinnedMeshRenderer", "MeshFilter", "Camera", "Light", "AudioSource",
    "Rigidbody", "Animation", "SphereCollider", "BoxCollider", "CapsuleCollider",
    "AudioListener", "ParticleSystem", "ParticleSystemRenderer",
    "TrailRenderer", "LineRenderer", "SpriteRenderer",
    "TextMesh", "CharacterController", "ConfigurableJoint", "HingeJoint", "ConstantForce",
    "WheelCollider", "Joint", "SpringJoint",
}
REMOVED_IN_5 = {"ParticleAnimator", "ParticleRenderer", "EllipsoidParticleEmitter",
                "ParticleEmitter", "FlareLayer", "GUILayer", "GUIText", "GUITexture",
                "Interactable", "NetworkView"}


def tag_name(value):
    """-> (name or None, warning or None). None means Untagged or deliberately unmapped."""
    if value >= 20000:
        i = value - 20000
        if i < len(USER_TAGS):
            return USER_TAGS[i], None
        return None, "user tag index %d out of range" % i
    if value in BUILTIN_TAGS:
        return BUILTIN_TAGS[value], None
    return None, "unmapped builtin m_Tag=%d" % value


def load_json(path):
    if not os.path.exists(path):
        return None
    with open(path, encoding="utf-8") as fh:
        return json.load(fh)


def _safe(s):
    return re.sub(r"[^0-9A-Za-z_-]", "_", str(s))


def clip_asset_path(name, container, path_id):
    """The one rule for where a clip's .anim lives: Animations/<state>__<container>_<id>/<state>.anim

    Both the scene binding table and MonoBehaviour AnimationClip[] references go through this, so
    the two can never disagree about where a clip is. The file name is the state name because a
    legacy Animation state IS the clip's object name and CreateAsset pins that to the file name.
    """
    key = _safe(container).replace("_assets", "") + "_" + str(path_id)
    return "Assets/Imported/Animations/%s/%s.anim" % (_safe(name) + "__" + key, _safe(name))


def flatten_value(fname, v, obj2node, asset_lookup, stats=None):
    """One decoded value -> (JSON field dict or None, stat key or None).

    Every branch is explicit. The previous version ended in `else: treat as Vector`, which would
    have raised on the string/array/struct values this pass adds -- better than silently writing
    nonsense, but still a hole. Anything that cannot be represented is counted, never guessed.
    """
    if stats is None:
        stats = {}
    if "ref" in v:
        if v["file"] != 0:
            # Cross-file: a scene object cannot host it, but an exported asset can. Resolve it
            # against the material / mesh / texture / clip indexes when the type has one, and drop
            # it (counted) when it points at something not exported yet.
            path = asset_lookup(v["type"], v["file"], v["ref"])
            if path:
                return {"n": fname, "kind": "asset", "asset": path, "t": v["type"]}, \
                       "cross-file -> asset"
            return None, "cross-file ref dropped"
        if v["ref"] == 0 and v["file"] == 0:
            # A null PPtr: the field was unassigned in the shipped build. Counted as faithful
            # absence, NOT as a dropped reference -- 70 fields were being reported as "dangling"
            # until that was checked, which made a correct result look like a leak.
            return None, "null in source"
        node = obj2node.get(v["ref"])
        if node is None:
            return None, "dangling ref dropped"
        return {"n": fname, "kind": "node", "node": node, "t": v["type"]}, "ref set"
    if "bool" in v:
        return {"n": fname, "kind": "bool", "b": 1 if v["bool"] else 0}, None
    if "float" in v:
        return {"n": fname, "kind": "float", "f": v["float"]}, None
    if "int" in v:
        return {"n": fname, "kind": "int", "i": v["int"]}, None
    if "opaque" in v:
        return {"n": fname, "kind": "opaque", "t": v["opaque"], "size": v["size"]}, \
               "opaque skipped (%s)" % v["opaque"]
    if "str" in v:
        return {"n": fname, "kind": "str", "s": v["str"]}, "string set"
    if "arr" in v:
        items = []
        dropped = 0
        for e in v["arr"]:
            fv, st = flatten_value(fname, e, obj2node, asset_lookup)
            if st:
                # Element outcomes used to be thrown away, so a half-empty array looked identical
                # to a fully resolved one in the tally.
                stats[st + " (element)"] = stats.get(st + " (element)", 0) + 1
            if fv is None:
                dropped += 1
            else:
                items.append({k: val for k, val in fv.items() if k != "n"})
        if not items:
            return None, "array fully dropped"
        out = {"n": fname, "kind": "arr", "t": v["elem"], "items": items}
        stat = "array set" if not dropped else "array partial (%d dropped)" % dropped
        return out, stat
    if "obj" in v:
        # Sub-field names are kept: a struct's meaning is its field names, and dropping them
        # would leave the Editor unable to say which value goes where.
        items = []
        for sub, e in v["obj"].items():
            fv, st = flatten_value(sub, e, obj2node, asset_lookup)
            if st:
                stats[st + " (struct field)"] = stats.get(st + " (struct field)", 0) + 1
            if fv is not None:
                items.append(fv)
        return ({"n": fname, "kind": "obj", "t": v["struct"], "fields": items}, "struct set") \
            if items else (None, "struct fully dropped")
    if "floats" in v:
        return {"n": fname, "kind": "vec", "v": v["floats"], "t": v.get("vec", "")}, None
    return None, "unknown decoded kind"


def bind_animations(nodes, clip_index, script_states, warnings, stats):
    """Attach to each Animation-carrying node the clips its own scripts will ask for by name.

    Why names and not geometry: the APK has no AnimationContainer objects and every Animation
    component's m_Clip is null, so there is no pointer to follow. Bone paths cannot decide it
    either -- every humanoid reuses the Bip01/... names, which made a path-containment rule bind
    224 clips to the protagonist. The scripts are the authority: Pet_eagle.cs calls
    Play("fly_flap"), so the object carrying Pet_eagle needs a state named fly_flap. Registering
    an extra state is harmless for legacy Animation; a missing one throws.

    Duplicate clip names (idle exists 15 times) are resolved by container-of-the-node's-mesh,
    then by bone-path containment, then by lowest path_id -- and the rule that won is recorded,
    so an arbitrary pick is visible instead of silent.

    The asset path is per CONSUMER, and that is load-bearing. A legacy Animation state's name IS
    the clip's object name, and AssetDatabase.CreateAsset forces object name = file name. One
    shared asset per clip therefore cannot serve two different state names: measured, Animation.
    AddClip renamed what it needed and Unity cloned 119 clips INTO Story.unity, taking the file
    from 320 KB to 47.9 MB and duplicating every curve per scene. Naming the asset exactly after
    the state, inside a folder per object, lets the scene reference the asset instead.
    """
    by_id = {n["id"]: n for n in nodes}
    by_name = collections.defaultdict(list)
    for entry in clip_index:
        by_name[entry["name"]].append(entry)

    def descendants(nid, seen):
        for cid in by_id[nid].get("children") or []:
            if cid in by_id and cid not in seen:
                seen.add(cid)
                descendants(cid, seen)

    for n in nodes:
        if not any(c.get("type") == "Animation" for c in n["components"]):
            continue
        scripts = [c.get("script") for c in n["components"] if c.get("kind") == "MonoBehaviour"]
        want = sorted({name for s in scripts for name in script_states.get(s or "", [])})
        if not want:
            continue
        sub = set()
        descendants(n["id"], sub)
        sub_names = {by_id[i]["name"] for i in sub}
        containers = set()
        for i in sub | {n["id"]}:
            for c in by_id[i]["components"]:
                m = re.search(r"_(sharedassets\d+)_assets_", str(c.get("mesh") or ""))
                if m:
                    containers.add(m.group(1) + ".assets")

        bound = []
        for name in want:
            cands = by_name.get(name)
            if not cands:
                stats["missing_name"] += 1
                warnings.append("%s: script asks for animation state '%s' but no clip in the "
                                "whole APK has that name" % (n["name"], name))
                continue
            pick, how = None, ""
            if len(cands) == 1:
                pick, how = cands[0], "only"
            else:
                same = [c for c in cands if c["container"] in containers]
                if len(same) == 1:
                    pick, how = same[0], "same-container"
                else:
                    fits = [c for c in (same or cands)
                            if c["paths"] and all(p in sub_names for p in c["paths"])]
                    if len(fits) == 1:
                        pick, how = fits[0], "path-containment"
                    else:
                        pick = sorted(fits or cands, key=lambda c: c["path_id"])[0]
                        how = "lowest-path-id (ambiguous among %d)" % len(fits or cands)
                        stats["ambiguous"] += 1
            stats[how.split(" ")[0]] += 1
            # One asset per (state name, source clip), not per consumer: 273 (consumer,state)
            # bindings come from 114 distinct clips, so keying on the consumer stored every shared
            # clip 2.4x over -- 119.8 MB of .anim for ~50 MB of animation. If #18 later resolves an
            # ambiguous state to different clips per consumer, that just adds another folder here.
            bound.append({"name": name, "clip_id": pick["id"], "picked_by": how,
                          "source_descriptor": pick["descriptor"],
                          "asset": clip_asset_path(name, pick["container"], pick["path_id"])})
        if bound:
            n["animations"] = bound


def main():
    target = sys.argv[1] if len(sys.argv) > 1 else "level7"
    if not target.startswith("level"):
        target = "level" + target
    path = os.path.join(DUMP, target)
    if not os.path.exists(path):
        sys.exit("no such level: %s" % path)

    bindings = json.load(open(BINDINGS, encoding="utf-8"))
    script_guid = bindings["script_to_guid"]
    script_cs = bindings["script_to_cs"]
    externals = bindings["level_externals"].get(target, [])
    monoscripts = bindings["monoscripts"]

    # Asset indexes produced by the earlier export steps. Missing ones are not fatal --
    # the renderer is then emitted with a null reference and reported, so the scene still
    # rebuilds and the gap stays visible instead of the tool refusing to run.
    mig = os.path.join(_REPO, "tools", "migration")
    meshes_doc = load_json(os.path.join(mig, "meshes.json"))
    mats_doc = load_json(os.path.join(mig, "materials.json"))
    mesh_index = (meshes_doc or {}).get("index", {})
    mat_index = (mats_doc or {}).get("index", {})
    if not mesh_index or not mat_index:
        print("WARNING: mesh and/or material index missing -- renderers will have null "
              "references. Run export_meshes.py / export_materials.py first.")

    def asset_for(index, file_id, path_id):
        tgt = u4.resolve_target(target, externals, file_id)
        if tgt is None:
            return None, "<bad FileID %d>" % file_id
        key = "%s!%d" % (tgt, path_id)
        return index.get(key), key

    # Cross-file object references. A scene cannot host them, but an exported asset can: the
    # material / mesh / texture indexes are keyed the same way (container!path_id), and clip paths
    # are a pure function of (name, container, path_id). Anything else stays dropped and counted.
    tex_index = (load_json(os.path.join(_REPO, "tools", "migration", "texture-index.json")) or {})
    clip_index_for_refs = {}
    _ci = load_json(os.path.join(_REPO, "tools", "migration", "animation-index.json"))
    if _ci:
        for e in _ci["clips"]:
            clip_index_for_refs["%s!%d" % (e["container"], e["path_id"])] = clip_asset_path(
                e["name"], e["container"], e["path_id"])
    BY_TYPE = {"Material": mat_index, "Mesh": mesh_index, "Texture2D": tex_index,
               "Texture": tex_index, "AnimationClip": clip_index_for_refs}

    def asset_lookup(typ, file_id, path_id):
        idx = BY_TYPE.get(typ)
        if idx is None:
            return None
        tgt = u4.resolve_target(target, externals, file_id)
        return idx.get("%s!%d" % (tgt, path_id)) if tgt else None

    def translate_renderer(tn, obj, warnings, go_id):
        rec = {"kind": "renderer", "type": tn}
        try:
            d = obj.read()
        except Exception as exc:
            warnings.append("%s %d (GameObject %s): unreadable (%s)" % (tn, obj.path_id, go_id, exc))
            rec["unavailable"] = True
            return rec
        rec["enabled"] = bool(getattr(d, "m_Enabled", True))
        if tn == "MeshFilter":
            pm = getattr(d, "m_Mesh", None)
            if pm is not None and int(pm.m_PathID):
                asset, key = asset_for(mesh_index, int(pm.m_FileID), int(pm.m_PathID))
                rec["mesh"] = asset
                if asset is None:
                    warnings.append("MeshFilter %d: mesh %s not in meshes.json" % (obj.path_id, key))
            return rec
        mats = []
        for i, pp in enumerate(getattr(d, "m_Materials", None) or []):
            if int(pp.m_PathID) == 0:
                mats.append(None)
                continue
            asset, key = asset_for(mat_index, int(pp.m_FileID), int(pp.m_PathID))
            mats.append(asset)
            if asset is None:
                warnings.append("%s %d: material[%d] %s not buildable (deferred shader?)"
                                % (tn, obj.path_id, i, key))
        rec["materials"] = mats
        rec["sorting_order"] = int(getattr(d, "m_SortingOrder", 0) or 0)
        rec["cast_shadows"] = bool(getattr(d, "m_CastShadows", False))
        rec["receive_shadows"] = bool(getattr(d, "m_ReceiveShadows", False))
        if tn == "SkinnedMeshRenderer":
            # m_Bones holds direct Transform pointers inside this same level file, so the
            # skeleton can be re-bound by id -- the missing bone NAMES in the mesh only
            # matter for meshes shared across prefabs.
            rec["bones"] = [int(pp.m_PathID) for pp in (getattr(d, "m_Bones", None) or [])
                            if int(pp.m_FileID) == 0]
            rec["root_bone"] = int(getattr(d, "m_RootBone", None).m_PathID or 0) \
                if getattr(d, "m_RootBone", None) is not None else 0
            pm = getattr(d, "m_Mesh", None)
            if pm is not None and int(pm.m_PathID):
                rec["mesh"], _k = asset_for(mesh_index, int(pm.m_FileID), int(pm.m_PathID))
        return rec

    env = UnityPy.load(path)
    objs = {o.path_id: o for o in env.objects}

    def script_name(sc_file_id, sc_path_id):
        if sc_file_id == 0:
            key = "%s!%d" % (target, sc_path_id)
        else:
            if sc_file_id - 1 >= len(externals):
                return None
            key = "%s!%d" % (externals[sc_file_id - 1], sc_path_id)
        return monoscripts.get(key)

    transforms = {}
    for o in env.objects:
        if o.type.name != "Transform":
            continue
        d = o.read()
        transforms[o.path_id] = {
            "father": d.m_Father.m_PathID,
            "children": [c.m_PathID for c in (d.m_Children or [])],
            "pos": [round(float(d.m_LocalPosition.x), 6), round(float(d.m_LocalPosition.y), 6),
                    round(float(d.m_LocalPosition.z), 6)],
            "rot": [round(float(d.m_LocalRotation.x), 6), round(float(d.m_LocalRotation.y), 6),
                    round(float(d.m_LocalRotation.z), 6), round(float(d.m_LocalRotation.w), 6)],
            "scale": [round(float(d.m_LocalScale.x), 6), round(float(d.m_LocalScale.y), 6),
                      round(float(d.m_LocalScale.z), 6)],
        }

    nodes = []
    warnings = []
    unsupported = collections.Counter()

    # Any object id -> the node id (its GameObject's Transform path_id) it belongs to. Needed
    # because a serialised reference may point at the Transform, the GameObject, or any other
    # component of the same object, and the scene JSON is keyed by Transform id.
    obj2node = {}
    for o in env.objects:
        if o.type.name != "GameObject":
            continue
        d = o.read()
        tf = [pp.m_PathID for _tid, pp in d.m_Component if int(_tid) == 4]
        if not tf:
            continue
        obj2node[o.path_id] = tf[0]
        for _tid, pp in d.m_Component:
            obj2node[pp.m_PathID] = tf[0]

    field_stats = collections.Counter()

    for o in env.objects:
        if o.type.name != "GameObject":
            continue
        d = o.read()
        tpath = None
        comps = []
        for _tid, pp in d.m_Component:
            t = objs.get(pp.m_PathID)
            if t is None:
                warnings.append("GameObject %d: component pointer %d dangles" % (o.path_id, pp.m_PathID))
                continue
            tn = t.type.name
            if tn == "Transform":
                tpath = pp.m_PathID
                continue
            if tn == "MonoBehaviour":
                raw = bytes(t.get_raw_data())
                _gof, _gop, en, scf, scp = struct.unpack_from("<iiiii", raw, 0)
                name = script_name(scf, scp)
                if name is None:
                    warnings.append("MonoBehaviour %d: script %d!%d unresolved" % (t.path_id, scf, scp))
                    continue
                entry = {"kind": "MonoBehaviour", "script": name,
                         "guid": script_guid.get(name, ""),
                         "cs": script_cs.get(name, ""),
                         "enabled": bool(en)}
                vals, why = MF.decode_for_class(name, raw)
                if vals is None:
                    field_stats["refused[" + MF.classify_refusal(why)[:38] + "]"] += 1
                else:
                    out_fields = []
                    for fname, v in vals:
                        fv, stat = flatten_value(fname, v, obj2node, asset_lookup)
                        if stat:
                            field_stats[stat] += 1
                        if fv is not None:
                            out_fields.append(fv)
                    if out_fields:
                        entry["fields"] = out_fields
                    field_stats["instances decoded"] += 1
                comps.append(entry)
            elif tn in ("MeshFilter", "MeshRenderer", "SkinnedMeshRenderer"):
                comps.append(translate_renderer(tn, t, warnings, o.path_id))
            elif tn in REMOVED_IN_5:
                unsupported[tn] += 1
                comps.append({"kind": "unsupported", "type": tn})
            elif tn in SUPPORTED:
                comps.append({"kind": "component", "type": tn})
            else:
                unsupported[tn] += 1
                warnings.append("GameObject %d: unhandled component type %s" % (o.path_id, tn))

        tname, twarn = tag_name(int(d.m_Tag))
        if twarn:
            warnings.append("GameObject %s: %s" % (o.path_id, twarn))
        if tpath is None:
            warnings.append("GameObject %s has no Transform -- skipped" % o.path_id)
            continue
        tr = transforms[tpath]
        nodes.append({
            "id": tpath,
            "go_id": o.path_id,
            "name": str(d.m_Name),
            "layer": int(d.m_Layer),
            "tag": tname,
            "active": bool(d.m_IsActive),
            "trs": {"pos": tr["pos"], "rot": tr["rot"], "scale": tr["scale"]},
            "parent": tr["father"],
            "children": tr["children"],
            "components": comps,
        })

    ids = {n["id"] for n in nodes}
    roots = [n["id"] for n in nodes if n["parent"] == 0 or n["parent"] not in ids]
    idx = int(target[5:])
    scene_name = SCENE_ORDER[idx + 1] if idx + 1 < len(SCENE_ORDER) else "?"

    # Animation binding. Both inputs are produced by the animation tools; a missing one is not
    # fatal -- the scene still rebuilds, just with no states registered, and the gap is reported.
    clip_index = load_json(os.path.join(mig, "animation-index.json"))
    survey = load_json(os.path.join(mig, "animations-survey.json"))
    anim_stats = collections.Counter()
    if clip_index and survey:
        bind_animations(nodes, clip_index["clips"], survey.get("script_states", {}),
                        warnings, anim_stats)
    else:
        warnings.append("animation-index.json / animations-survey.json missing -- no Animation "
                        "component will have states registered")

    payload = {
        "source_level": target,
        "scene_name": scene_name,
        "scene_path": "Assets/Scene/%s.unity" % scene_name,
        "build_index": idx + 1,
        "node_count": len(nodes),
        "root_ids": roots,
        "unsupported_components": dict(unsupported),
        "field_stats": dict(field_stats),
        "animation_bindings": dict(anim_stats),
        "warnings": warnings,
        "nodes": nodes,
    }
    os.makedirs(OUT_DIR, exist_ok=True)
    out = os.path.join(OUT_DIR, "%s.json" % target)
    # Write via a temp file and rename. Truncating the destination in place hit intermittent
    # OSError EINVAL on this machine for whichever level some other process (Editor, indexer)
    # happened to be reading, which loses a level silently if the caller ignores the exit code.
    tmp = out + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(payload, fh, indent=1, ensure_ascii=False)
    for attempt in range(5):
        try:
            os.replace(tmp, out)
            break
        except OSError:
            if attempt == 4:
                raise
            import time
            time.sleep(0.4 * (attempt + 1))

    kinds = collections.Counter(
        c["type"] if c["kind"] == "component" else c["kind"]
        for n in nodes for c in n["components"])
    print("level  : %s  ->  scene %r (build index %d)" % (target, scene_name, idx + 1))
    print("nodes  : %d, roots: %d" % (len(nodes), len(roots)))
    print("components:")
    for k, v in kinds.most_common():
        print("   %5d  %s" % (v, k))
    if unsupported:
        print("DROPPED (no such component in 2022): %s" % dict(unsupported))
    if warnings:
        print("warnings: %d" % len(warnings))
        for w in warnings[:8]:
            print("   ", w)
    print("mono fields: %s" % (dict(field_stats) or "none"))
    print("wrote %s" % os.path.relpath(out, _REPO).replace(os.sep, "/"))


if __name__ == "__main__":
    main()
