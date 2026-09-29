"""Turn the 355 Unity 4 Material objects into a description the Editor can recreate.

Output is JSON, not .mat files -- same reason as the scene rebuild: let Unity serialise.
Assets/Editor/MaterialRebuilder.cs reads this, calls new Material(Shader.Find(target)) and
AssetDatabase.CreateAsset, so the .mat YAML is always whatever 2022.3 actually expects.

Shader names go through tools/migration/shader-remap.json. Anything still listed there as
"pending" is reported and SKIPPED rather than guessed at.

Colour handling: the project ships with m_ActiveColorSpace = 0 (Gamma, read from
mainData's PlayerSettings), so Unity 4's stored colour values are used verbatim. If the
project is ever switched to Linear this has to be revisited -- the conversion is not
something to do silently per-material.

Run:  <unitypy venv python> tools/migration/export_materials.py
"""
import collections
import json
import os
import re
import sys

import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
REMAP = os.path.join(_REPO, "tools", "migration", "shader-remap.json")
TEX_INDEX = os.path.join(_REPO, "tools", "migration", "texture-index.json")
OUT = os.path.join(_REPO, "tools", "migration", "materials.json")

BAD = re.compile(r'[<>:"/\\|?*\x00-\x1f]')


def safe(name, key):
    n = BAD.sub("_", (name or "").strip()).strip(". ")
    return (n[:50] or "material") + "_" + re.sub(r"\W+", "_", key)


def main():
    remap_doc = json.load(open(REMAP, encoding="utf-8"))
    remap = remap_doc["remap"]
    # Anything not in `remap` is skipped. `deferred` entries are skipped on purpose with a
    # recorded reason; `pending` ones still need the shader probe.
    pending = set(remap_doc.get("pending") or []) | set(remap_doc.get("deferred") or {})
    textures = json.load(open(TEX_INDEX, encoding="utf-8")) if os.path.exists(TEX_INDEX) else {}
    if not textures:
        sys.exit("no texture-index.json -- run export_textures.py first")

    # Pass 1: index every Shader object so a Material's m_Shader pointer can be named.
    shaders = {}
    per_file = {}
    for name, path in u4.sources():
        try:
            env = u4.load(path)
        except Exception:
            continue
        per_file[name] = (path, u4.externals_of(env))
        for o in env.objects:
            if o.type.name == "Shader":
                try:
                    shaders["%s!%d" % (name, o.path_id)] = str(o.read().m_Name)
                except Exception:
                    pass

    # Pass 2: read the materials.
    out = []
    index = {}
    used_names = collections.Counter()
    skipped_pending = collections.Counter()
    no_shader = 0
    tex_hit = tex_miss = 0
    taken = set()

    for name, path in u4.sources():
        entry = per_file.get(name)
        if not entry:
            continue
        _p, ext = entry
        try:
            env = u4.load(path)
        except Exception:
            continue
        for o in env.objects:
            if o.type.name != "Material":
                continue
            key = "%s!%d" % (name, o.path_id)
            try:
                m = o.read()
            except Exception:
                no_shader += 1
                continue

            skey = None
            tgt = u4.resolve_target(name, ext, m.m_Shader.m_FileID)
            if tgt:
                skey = "%s!%d" % (tgt, int(m.m_Shader.m_PathID))
            shader_name = shaders.get(skey or "")
            if shader_name is None:
                no_shader += 1
                continue
            used_names[shader_name] += 1
            target = remap.get(shader_name)
            if target is None:
                skipped_pending[shader_name] += 1
                continue

            mname = str(m.m_Name)
            fname = safe(mname, key)
            asset = "Assets/Imported/Materials/%s.mat" % fname
            while asset in taken:
                asset = asset[:-4] + "b.mat"
            taken.add(asset)

            sp = m.m_SavedProperties
            texs, cols, flts = {}, {}, {}
            for pname, env_entry in (getattr(sp, "m_TexEnvs", None) or []):
                pn = getattr(pname, "name", str(pname))
                tp = getattr(env_entry, "m_Texture", None)
                if tp is None or int(tp.m_PathID) == 0:
                    continue
                ttgt = u4.resolve_target(name, ext, tp.m_FileID)
                tkey = "%s!%d" % (ttgt, int(tp.m_PathID)) if ttgt else None
                tpath = textures.get(tkey)
                if tpath:
                    tex_hit += 1
                else:
                    tex_miss += 1
                    continue
                sc = getattr(env_entry, "m_Scale", None)
                of = getattr(env_entry, "m_Offset", None)
                texs[pn] = {
                    "path": tpath,
                    "scale": [float(sc.x), float(sc.y)] if sc else [1.0, 1.0],
                    "offset": [float(of.x), float(of.y)] if of else [0.0, 0.0],
                }
            for pname, c in (getattr(sp, "m_Colors", None) or []):
                pn = getattr(pname, "name", str(pname))
                cols[pn] = [float(c.r), float(c.g), float(c.b), float(c.a)]
            for pname, v in (getattr(sp, "m_Floats", None) or []):
                pn = getattr(pname, "name", str(pname))
                flts[pn] = float(v)

            # Arrays, not objects: Unity's JsonUtility cannot deserialise a dictionary and
            # would silently leave the field null, so every property would be skipped
            # without any error. Key/value pairs are emitted as one object each instead.
            rec = {
                "key": key,
                "asset_path": asset,
                "name": mname,
                "shader_src_name": shader_name,
                "shader_target": target,
                "render_queue": int(getattr(m, "m_CustomRenderQueue", -1) or -1),
                "textures": [{"name": k, **v} for k, v in texs.items()],
                "colors": [{"name": k, "rgba": v} for k, v in cols.items()],
                "floats": [{"name": k, "value": v} for k, v in flts.items()],
            }
            out.append(rec)
            index[key] = asset

    payload = {
        "color_space": "gamma",
        "material_count": len(out),
        "index": index,
        "shader_usage": dict(used_names.most_common()),
        "skipped_pending_shader": dict(skipped_pending),
        "texture_refs_resolved": tex_hit,
        "texture_refs_missing": tex_miss,
        "materials_unreadable": no_shader,
        "materials": out,
    }
    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(payload, fh, indent=1, ensure_ascii=False)

    print("materials written : %d" % len(out))
    print("texture refs      : %d resolved, %d missing" % (tex_hit, tex_miss))
    if no_shader:
        print("unreadable / shader pointer unresolvable: %d" % no_shader)
    if skipped_pending:
        print("SKIPPED, shader still pending in shader-remap.json:")
        for k, v in skipped_pending.most_common():
            print("   %5d  %s" % (v, k))
    print("shader usage (all materials seen):")
    for k, v in used_names.most_common():
        mark = "" if k in remap else "   <-- NOT MAPPED"
        print("   %5d  %-42s -> %s%s" % (v, k, remap.get(k, "?"), mark))
    print("wrote %s" % os.path.relpath(OUT, _REPO).replace(os.sep, "/"))


if __name__ == "__main__":
    main()
