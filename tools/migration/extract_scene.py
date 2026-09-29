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
import struct
import sys

import UnityPy

import unity4_files as u4

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

# Builtin tag indices. 5 and 6 are not inferred from the objects' display names -- they are
# forced by shipped code: Camera.main is called 28 times across the scripts, and the only
# camera-bearing GameObjects carry m_Tag 5, so 5 must be MainCamera for the shipped game to
# work at all. The same argument makes 6 = Player (the cha1 / cha_ui protagonist objects).
# Indices 1..4 are deliberately unmapped; guessing them would silently retag objects.
BUILTIN_TAGS = {0: None, 5: "MainCamera", 6: "Player"}

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
                comps.append({"kind": "MonoBehaviour", "script": name,
                              "guid": script_guid.get(name, ""),
                              "cs": script_cs.get(name, ""),
                              "enabled": bool(en)})
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

    payload = {
        "source_level": target,
        "scene_name": scene_name,
        "scene_path": "Assets/Scene/%s.unity" % scene_name,
        "build_index": idx + 1,
        "node_count": len(nodes),
        "root_ids": roots,
        "unsupported_components": dict(unsupported),
        "warnings": warnings,
        "nodes": nodes,
    }
    os.makedirs(OUT_DIR, exist_ok=True)
    out = os.path.join(OUT_DIR, "%s.json" % target)
    with open(out, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(payload, fh, indent=1, ensure_ascii=False)

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
    print("wrote %s" % os.path.relpath(out, _REPO).replace(os.sep, "/"))


if __name__ == "__main__":
    main()
