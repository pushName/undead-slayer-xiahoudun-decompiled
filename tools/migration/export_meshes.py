"""Export the 256 Unity 4 meshes into a JSON description the Editor can rebuild.

UnityPy 1.25.3 has no Mesh.Export(), and .obj could not carry UV1/tangents/skin weights
anyway, so this emits the vertex streams and lets Assets/Editor/MeshRebuilder.cs build real
Mesh assets -- same "let Unity serialise" principle as the scene and material steps.

CHANNEL LAYOUT (derived from the data, not assumed -- see README):
    index 0  dim 3 float32  position   range -5..17.3, only 1% unit-length
    index 1  dim 3 float32  normal     -1..1, 100% unit-length over sampled verts
    index 2  dim 1 byte     vertex colour -- 255 on every vertex of the 12 meshes that
                             have it, i.e. pure white, so it is dropped as a no-op
    index 3  dim 2 float32  UV0        -3.375..4.375 (tiling beyond [0,1] is expected)
    index 4  dim 2 float32  UV1        0..0.9999 (lightmap UVs)
    index 5  dim 4 float32  tangent    -1..1, 100% unit-length on xyz
Any channel index outside these is reported, never guessed at.

INDEX BUFFER: Unity 4 stores it as a byte array and each SubMesh says where in it to start
(firstByte) and how many indices to take. 16- vs 32-bit is not flagged in this data, so both
widths are tried and the one that consumes the buffer exactly, with every index < vertexCount,
wins. If neither does, the mesh is reported instead of being silently mis-triangulated.

Run:  <unitypy venv python> tools/migration/export_meshes.py
"""
import collections
import json
import os
import re
import struct
import sys

import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(_REPO, "tools", "migration", "meshes.json")

BAD = re.compile(r'[<>:"/\\|?*\x00-\x1f]')

# channel index -> (target list name, dimension we expect)
CHANNELS = {0: ("vertices", 3), 1: ("normals", 3), 3: ("uv0", 2), 4: ("uv1", 2), 5: ("tangents", 4)}
DROPPED_CHANNELS = {2: "vertex colour, constant 255 (white) in this data"}

MATRIX_ELEMENTS = ["e00","e01","e02","e03","e10","e11","e12","e13","e20","e21","e22","e23","e30","e31","e32","e33"]


def safe(name, key):
    n = BAD.sub("_", (name or "").strip()).strip(". ")
    return (n[:50] or "mesh") + "_" + re.sub(r"\W+", "_", key)


def read_channel(vd, idx, count):
    """-> (values flattened list, dim) or (None, 0)."""
    ch = vd.m_Channels[idx]
    dim = int(ch.dimension)
    fmt = int(ch.format)
    if fmt != 0:
        return None, dim, "format %d (only float32 handled)" % fmt
    st = vd.m_Streams[ch.stream]
    stride, base = int(st.stride), int(st.offset)
    data = vd.m_DataSize
    out = []
    for v in range(count):
        off = base + v * stride + int(ch.offset)
        if off + dim * 4 > len(data):
            return None, dim, "channel data runs past the buffer"
        out.extend(struct.unpack_from("<%df" % dim, data, off))
    return out, dim, None


def decode_indices(index_bytes, submeshes, vertex_count):
    """Try 16- then 32-bit. Return (list-of-triangle-index-lists, width) or (None, None)."""
    total = sum(int(s.indexCount) for s in submeshes)
    for width, code in ((2, "H"), (4, "I")):
        if total * width != len(index_bytes):
            continue
        tris = []
        ok = True
        for s in submeshes:
            cnt = int(s.indexCount)
            chunk = struct.unpack_from("<%d%s" % (cnt, code), index_bytes, int(s.firstByte))
            if any(i >= vertex_count for i in chunk):
                ok = False
                break
            tris.append(list(chunk))
        if ok:
            return tris, width
    return None, None


def main():
    out = []
    index = {}
    problems = collections.Counter()
    unknown_channels = collections.Counter()
    taken = set()
    total_verts = 0
    skinned = 0
    has_bone_names = 0

    for name, path in u4.sources():
        try:
            env = u4.load(path)
        except Exception:
            continue
        for o in env.objects:
            if o.type.name != "Mesh":
                continue
            key = "%s!%d" % (name, o.path_id)
            try:
                d = o.read()
                vd = d.m_VertexData
            except Exception:
                problems["mesh read failed"] += 1
                continue
            if vd is None:
                problems["no vertex data"] += 1
                continue
            vc = int(vd.m_VertexCount)
            if not vc:
                problems["zero vertices"] += 1
                continue

            streams = {}
            bad = False
            for idx, ch in enumerate(vd.m_Channels):
                if not int(ch.dimension):
                    continue
                if idx in DROPPED_CHANNELS:
                    continue
                if idx not in CHANNELS:
                    unknown_channels["index %d dim %d" % (idx, int(ch.dimension))] += 1
                    problems["unknown channel index"] += 1
                    bad = True
                    continue
                vals, dim, err = read_channel(vd, idx, vc)
                if err:
                    problems["%s: %s" % (CHANNELS[idx][0], err)] += 1
                    bad = True
                    continue
                if dim != CHANNELS[idx][1]:
                    problems["%s has dim %d not %d" % (CHANNELS[idx][0], dim, CHANNELS[idx][1])] += 1
                    bad = True
                    continue
                streams[CHANNELS[idx][0]] = [round(x, 5) for x in vals]
            if bad or "vertices" not in streams:
                continue

            ib = bytes(bytearray(d.m_IndexBuffer or []))
            tris, width = decode_indices(ib, d.m_SubMeshes or [], vc)
            if tris is None:
                problems["index buffer undecodable"] += 1
                continue

            skin = getattr(d, "m_Skin", None) or []
            bindpose = getattr(d, "m_BindPose", None) or []
            hashes = getattr(d, "m_BoneNameHashes", None) or []
            bone_names = getattr(d, "m_BoneNames", None) or []
            is_skinned = bool(skin)
            if is_skinned:
                skinned += 1
                if bone_names:
                    has_bone_names += 1
                if len(skin) != vc:
                    problems["skin count != vertex count"] += 1
                    continue
                bones = []
                weights = []
                for s in skin:
                    bones.extend([int(s.boneIndex_0_), int(s.boneIndex_1_),
                                  int(s.boneIndex_2_), int(s.boneIndex_3_)])
                    weights.extend([round(float(s.weight_0_), 5), round(float(s.weight_1_), 5),
                                    round(float(s.weight_2_), 5), round(float(s.weight_3_), 5)])
                streams["bones"] = bones
                streams["weights"] = weights
                rec_extra = {
                    "bindposes": [round(float(getattr(m, e)), 6)
                                  for m in bindpose for e in MATRIX_ELEMENTS],
                    "bone_names": [str(b) for b in bone_names],
                    "bone_name_hashes": [int(h) for h in hashes],
                }

            mname = str(d.m_Name)
            asset = "Assets/Imported/Meshes/%s.asset" % safe(mname, key)
            while asset in taken:
                asset = asset[:-6] + "b.asset"
            taken.add(asset)

            out.append({
                "key": key,
                "asset_path": asset,
                "name": mname,
                "vertex_count": vc,
                "index_width": width,
                # Flattened: JsonUtility cannot deserialise a jagged int[][], so the
                # submesh split is carried as per-submesh index counts instead.
                "triangles": [i for sub in tris for i in sub],
                "submesh_index_counts": [len(sub) for sub in tris],
                "skinned": is_skinned,
                **(rec_extra if is_skinned else {}),
                "bounds": {
                    "center": [round(float(x), 5) for x in
                               (d.m_LocalAABB.m_Center.x, d.m_LocalAABB.m_Center.y, d.m_LocalAABB.m_Center.z)],
                    "extent": [round(float(x), 5) for x in
                               (d.m_LocalAABB.m_Extent.x, d.m_LocalAABB.m_Extent.y, d.m_LocalAABB.m_Extent.z)],
                },
                **streams,
            })
            index[key] = asset
            total_verts += vc

    payload = {
        "mesh_count": len(out),
        "total_vertices": total_verts,
        "skinned_meshes": skinned,
        "index": index,
        "problems": dict(problems.most_common()),
        "unknown_channels": dict(unknown_channels.most_common()),
        "dropped_channels": DROPPED_CHANNELS,
        "meshes": out,
    }
    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(payload, fh, indent=0, separators=(",", ":"), ensure_ascii=False)

    size = os.path.getsize(OUT)
    print("meshes exported : %d of 256   (%.1f MB json)" % (len(out), size / 1048576.0))
    print("total vertices  : %d" % total_verts)
    print("skinned meshes  : %d   of which carrying real bone NAMES: %d"%(skinned, has_bone_names))
    if problems:
        print("skipped / problems:")
        for k, v in problems.most_common():
            print("   %5d  %s" % (v, k))
    print("wrote %s" % os.path.relpath(OUT, _REPO).replace(os.sep, "/"))


if __name__ == "__main__":
    main()
