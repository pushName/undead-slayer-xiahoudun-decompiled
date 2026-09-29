"""Survey the shipped Unity 4 build data to size up task #9 before writing any converter.

Answers, in one pass:
  * which component types actually appear, and how often  -> how much of a
    Unity4->Unity22 component mapper is even required
  * every MonoBehaviour's script pointer -> MonoScript name -> does a .cs exist
  * texture formats / sizes -> what the export step has to decode
  * per-level object histograms -> what each scene rebuild will cost

Read-only. Run:  <unitypy venv python> tools/migration/survey_scene_data.py
"""
import collections
import glob
import json
import os
import sys

import UnityPy

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DUMP_ASSETS = os.path.join(_REPO, "dump", "assets")
RECON = os.environ.get("RECON_DIR") or os.path.join(os.environ.get("TEMP", "/tmp"), "recon")
SCRIPTS = os.path.join(_REPO, "unity", "Assets", "Scripts")
OUT = os.environ.get("SURVEY_OUT") or os.path.join(os.environ.get("TEMP", "/tmp"), "survey9.json")


def data_files():
    levels = sorted(glob.glob(os.path.join(DUMP_ASSETS, "level*")))
    shared = sorted(glob.glob(os.path.join(DUMP_ASSETS, "sharedassets*.assets")))
    recon = sorted(glob.glob(os.path.join(RECON, "sharedassets*.assets")))
    return levels, shared + recon


def script_class_names():
    """Every public type name defined under unity/Assets/Scripts, best effort."""
    names = set()
    for root, _d, files in os.walk(SCRIPTS):
        for fn in files:
            if fn.endswith(".cs"):
                names.add(fn[:-3])
    return names


def main():
    levels, shared = data_files()
    comp_types = collections.Counter()
    mono_count = collections.Counter()
    tex_fmt = collections.Counter()
    tex_size = collections.Counter()
    type_hist_global = collections.Counter()
    per_level = {}
    monobehaviour_scripts = collections.Counter()   # path_id of MonoScript -> uses
    monoscript_names = {}                            # path_id -> name
    audio_ext = collections.Counter()
    mesh_count = 0
    clip_count = 0
    mat_count = 0
    shader_names = []
    errors = []

    for path in levels + shared:
        base = os.path.basename(path)
        is_level = base.startswith("level")
        try:
            env = UnityPy.load(path)
        except Exception as exc:
            errors.append((base, repr(exc)))
            continue
        types = collections.Counter(o.type.name for o in env.objects)
        type_hist_global.update(types)
        if is_level:
            per_level[base] = dict(types)

        # MonoScript table lives in sharedassets; collect wherever seen.
        for obj in env.objects:
            t = obj.type.name
            if t == "MonoScript":
                try:
                    d = obj.read()
                    monoscript_names[obj.path_id] = getattr(d, "m_Name", "?")
                except Exception:
                    pass
            elif t == "Texture2D":
                try:
                    d = obj.read()
                    tex_fmt[str(getattr(d, "m_TextureFormat", "?"))] += 1
                    tex_size["%dx%d" % (getattr(d, "m_Width", 0), getattr(d, "m_Height", 0))] += 1
                except Exception:
                    tex_fmt["<unreadable>"] += 1
            elif t == "MonoBehaviour":
                try:
                    d = obj.read()
                    ps = getattr(d, "m_Script", None)
                    if ps is not None:
                        key = (ps.m_FileID, ps.m_PathID)
                        monobehaviour_scripts[key] += 1
                except Exception:
                    pass
            elif t == "AudioClip":
                try:
                    d = obj.read()
                    for c in (d.completed_sample_count,):
                        pass
                    audio_ext["<clip>"] += 1
                except Exception:
                    pass
            elif t == "Mesh":
                mesh_count += 1
            elif t == "AnimationClip":
                clip_count += 1
            elif t == "Material":
                mat_count += 1
            elif t == "Shader":
                try:
                    shader_names.append(getattr(obj.read(), "m_Name", "?"))
                except Exception:
                    shader_names.append("<unreadable>")

    # Component types inside levels only (that is what a scene rebuild must emit).
    for lvl, hist in per_level.items():
        for t, n in hist.items():
            if t.endswith("Collider") or t in (
                "Transform", "Rigidbody", "AudioSource", "Camera", "Light",
                "Animation", "MeshRenderer", "SkinnedMeshRenderer", "ParticleSystem",
                "MonoBehaviour", "GameObject",
            ):
                comp_types[t] += n

    have = script_class_names()
    named = set(monoscript_names.values())
    missing = sorted(n for n in named if n not in have)

    report = {
        "levels": len(per_level),
        "sharedassets": len(shared),
        "load_errors": errors,
        "global_type_histogram": dict(type_hist_global.most_common()),
        "level_component_histogram": dict(comp_types.most_common()),
        "per_level": per_level,
        "monoscript_count": len(monoscript_names),
        "monoscript_distinct_names": len(named),
        "monoscript_names_without_matching_cs": missing,
        "monobehaviour_script_pointers": len(monobehaviour_scripts),
        "texture_formats": dict(tex_fmt.most_common()),
        "texture_top_sizes": dict(tex_size.most_common(25)),
        "mesh_total": mesh_count,
        "animationclip_total": clip_count,
        "material_total": mat_count,
        "shaders": sorted(set(shader_names)),
    }
    with open(OUT, "w", encoding="utf-8") as fh:
        json.dump(report, fh, indent=1, ensure_ascii=False, default=str)

    print("survey written to", OUT)
    print()
    print("levels: %d   sharedassets: %d   load failures: %d"
          % (len(per_level), len(shared), len(errors)))
    print()
    print("=== component types appearing inside level files ===")
    for t, n in comp_types.most_common():
        print("  %6d  %s" % (n, t))
    print()
    print("=== global type histogram (top 30) ===")
    for t, n in type_hist_global.most_common(30):
        print("  %6d  %s" % (n, t))
    print()
    print("MonoScript objects: %d, distinct names: %d" % (len(monoscript_names), len(named)))
    print("MonoBehaviour script pointers seen: %d" % len(monobehaviour_scripts))
    print("MonoScript names with NO matching .cs in unity/Assets/Scripts: %d" % len(missing))
    for m in missing[:40]:
        print("   -", m)
    print()
    print("=== Texture2D formats ===")
    for f, n in tex_fmt.most_common():
        print("  %6d  %s" % (n, f))
    print()
    print("Mesh %d / AnimationClip %d / Material %d" % (mesh_count, clip_count, mat_count))
    print()
    print("=== %d distinct Shaders ===" % len(set(shader_names)))
    for s in sorted(set(shader_names)):
        print("   -", s)


if __name__ == "__main__":
    main()
