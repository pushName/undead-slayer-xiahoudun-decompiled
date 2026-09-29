using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UndeadSlayer.Migration
{
    /// Builds real Mesh assets from tools/migration/meshes.json (produced by
    /// export_meshes.py out of the Unity 4 build data).
    ///
    /// Unity writes the .asset, so no hand-authored mesh YAML.
    ///
    /// After building each mesh its bounds are compared against the AABB that was stored in
    /// the source data. A mismatch beyond 1% means the vertex streams were decoded wrong --
    /// that is reported loudly instead of shipping a silently mangled mesh.
    ///
    /// Menu: Undead Slayer > Build Meshes From JSON
    public static class MeshRebuilder
    {
        [Serializable]
        private class BoundsRec
        {
            public float[] center;
            public float[] extent;
        }

        [Serializable]
        private class MeshDesc
        {
            public string key;
            public string asset_path;
            public string name;
            public int vertex_count;
            public int index_width;
            public bool skinned;
            public float[] vertices;
            public float[] normals;
            public float[] tangents;
            public float[] uv0;
            public float[] uv1;
            public float[] weights;
            public float[] bindposes;
            public int[] bones;
            public int[] triangles;
            public int[] submesh_index_counts;
            public int[] bone_name_hashes;
            public string[] bone_names;
            public BoundsRec bounds;
        }

        [Serializable]
        private class MeshFile
        {
            public int mesh_count;
            public int total_vertices;
            public int skinned_meshes;
            public MeshDesc[] meshes;
        }

        [MenuItem("Undead Slayer/Build Meshes From JSON")]
        private static void Build()
        {
            var jsonPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "tools", "migration", "meshes.json"));
            if (!File.Exists(jsonPath))
            {
                Debug.LogError("[MeshRebuilder] missing " + jsonPath +
                               " -- run tools/migration/export_meshes.py first");
                return;
            }

            var data = JsonUtility.FromJson<MeshFile>(File.ReadAllText(jsonPath));
            if (data == null || data.meshes == null || data.meshes.Length == 0)
            {
                Debug.LogError("[MeshRebuilder] " + jsonPath + " has no meshes");
                return;
            }

            EnsureFolder("Assets/Imported/Meshes");

            int built = 0, failed = 0, boundsMismatch = 0, replaced = 0;
            foreach (var m in data.meshes)
            {
                Mesh mesh;
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(m.asset_path);
                if (existing != null)
                {
                    mesh = existing;
                    mesh.Clear();
                    replaced++;
                }
                else
                {
                    mesh = new Mesh { name = m.name };
                }

                if (m.vertex_count > 65000)
                {
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }

                try
                {
                    Fill(mesh, m);
                }
                catch (Exception e)
                {
                    failed++;
                    Debug.LogError("[MeshRebuilder] " + m.key + " (" + m.name + "): " + e.Message);
                    continue;
                }

                mesh.RecalculateBounds();
                if (m.bounds != null && m.bounds.extent != null && m.bounds.extent.Length >= 3)
                {
                    var want = m.bounds.extent;
                    var got = mesh.bounds.extents;
                    float scale = Mathf.Max(1e-4f, Mathf.Max(want[0], Mathf.Max(want[1], want[2])));
                    float err = Mathf.Max(
                        Mathf.Abs(want[0] - got.x), Mathf.Max(
                        Mathf.Abs(want[1] - got.y), Mathf.Abs(want[2] - got.z)));
                    if (err / scale > 0.01f)
                    {
                        boundsMismatch++;
                        if (boundsMismatch <= 10)
                        {
                            Debug.LogWarning(
                                "[MeshRebuilder] " + m.key + " (" + m.name + ") bounds off by " +
                                (100f * err / scale).ToString("F1") + "% -- source " +
                                want[0] + "," + want[1] + "," + want[2] + " vs rebuilt " + got);
                        }
                    }
                }

                if (existing == null)
                {
                    AssetDatabase.CreateAsset(mesh, m.asset_path);
                }
                else
                {
                    EditorUtility.SetDirty(mesh);
                }
                built++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "[MeshRebuilder] done\n  in json        : " + data.meshes.Length +
                "\n  built/updated  : " + built + "  (" + replaced + " overwritten)" +
                "\n  failed         : " + failed +
                "\n  bounds mismatch: " + boundsMismatch +
                "\n  skinned        : " + data.skinned_meshes +
                "\n\nSkin binding does not depend on the mesh's bone names: the shipped meshes" +
                " store only 32-bit name hashes, but SkinnedMeshRenderer.m_Bones points" +
                " straight at the scene's own Transforms, so SceneRebuilder re-binds by id.");

            EditorUtility.DisplayDialog("Mesh rebuild",
                "built " + built + " of " + data.meshes.Length +
                "\nfailed: " + failed + "\nbounds mismatches: " + boundsMismatch +
                "\n\nDetails in the Console.", "OK");
        }

        private static void Fill(Mesh mesh, MeshDesc m)
        {
            int vc = m.vertex_count;
            if (m.vertices == null || m.vertices.Length < vc * 3)
            {
                throw new Exception("vertex stream shorter than vertex_count");
            }

            var verts = new List<Vector3>(vc);
            for (int i = 0; i < vc; i++)
            {
                verts.Add(new Vector3(m.vertices[i * 3], m.vertices[i * 3 + 1], m.vertices[i * 3 + 2]));
            }
            mesh.SetVertices(verts);

            if (m.normals != null && m.normals.Length >= vc * 3)
            {
                var n = new List<Vector3>(vc);
                for (int i = 0; i < vc; i++)
                {
                    n.Add(new Vector3(m.normals[i * 3], m.normals[i * 3 + 1], m.normals[i * 3 + 2]));
                }
                mesh.SetNormals(n);
            }

            if (m.tangents != null && m.tangents.Length >= vc * 4)
            {
                var t = new List<Vector4>(vc);
                for (int i = 0; i < vc; i++)
                {
                    t.Add(new Vector4(m.tangents[i * 4], m.tangents[i * 4 + 1],
                                      m.tangents[i * 4 + 2], m.tangents[i * 4 + 3]));
                }
                mesh.SetTangents(t);
            }

            if (m.uv0 != null && m.uv0.Length >= vc * 2)
            {
                var u = new List<Vector2>(vc);
                for (int i = 0; i < vc; i++)
                {
                    u.Add(new Vector2(m.uv0[i * 2], m.uv0[i * 2 + 1]));
                }
                mesh.SetUVs(0, u);
            }

            if (m.uv1 != null && m.uv1.Length >= vc * 2)
            {
                var u = new List<Vector2>(vc);
                for (int i = 0; i < vc; i++)
                {
                    u.Add(new Vector2(m.uv1[i * 2], m.uv1[i * 2 + 1]));
                }
                mesh.SetUVs(1, u);
            }

            if (m.skinned && m.bones != null && m.weights != null &&
                m.bones.Length >= vc * 4 && m.weights.Length >= vc * 4)
            {
                var bw = new List<BoneWeight>(vc);
                for (int i = 0; i < vc; i++)
                {
                    bw.Add(new BoneWeight
                    {
                        boneIndex0 = m.bones[i * 4],
                        boneIndex1 = m.bones[i * 4 + 1],
                        boneIndex2 = m.bones[i * 4 + 2],
                        boneIndex3 = m.bones[i * 4 + 3],
                        weight0 = m.weights[i * 4],
                        weight1 = m.weights[i * 4 + 1],
                        weight2 = m.weights[i * 4 + 2],
                        weight3 = m.weights[i * 4 + 3],
                    });
                }
                mesh.boneWeights = bw.ToArray();

                if (m.bindposes != null && m.bindposes.Length >= 16)
                {
                    int nb = m.bindposes.Length / 16;
                    var bp = new List<Matrix4x4>(nb);
                    for (int b = 0; b < nb; b++)
                    {
                        var mat = new Matrix4x4();
                        for (int r = 0; r < 4; r++)
                        {
                            for (int c = 0; c < 4; c++)
                            {
                                mat[r, c] = m.bindposes[b * 16 + r * 4 + c];
                            }
                        }
                        bp.Add(mat);
                    }
                    mesh.bindposes = bp.ToArray();
                }
            }

            if (m.triangles == null || m.submesh_index_counts == null)
            {
                throw new Exception("no triangles");
            }
            int offset = 0;
            // SetTriangles does NOT grow subMeshCount -- assigning submesh index >= the
            // current count fails with "Submesh index is out of bounds", silently leaving
            // all but the first submesh empty. Declare the count up front.
            mesh.subMeshCount = m.submesh_index_counts.Length;
            for (int s = 0; s < m.submesh_index_counts.Length; s++)
            {
                int count = m.submesh_index_counts[s];
                var tri = new int[count];
                Array.Copy(m.triangles, offset, tri, 0, count);
                mesh.SetTriangles(tri, s);
                offset += count;
            }
            if (offset != m.triangles.Length)
            {
                throw new Exception("submesh index counts (" + offset +
                                    ") do not cover the triangle array (" + m.triangles.Length + ")");
            }

            // Post-condition. Unity reports a bad submesh index through Debug.LogError
            // rather than an exception, so without this a mesh missing every submesh after
            // the first still counts as "built".
            if (mesh.subMeshCount != m.submesh_index_counts.Length)
            {
                throw new Exception("subMeshCount is " + mesh.subMeshCount + ", expected "
                                    + m.submesh_index_counts.Length);
            }
            for (int s = 0; s < m.submesh_index_counts.Length; s++)
            {
                int got = (int)mesh.GetIndexCount(s);
                if (got != m.submesh_index_counts[s])
                {
                    throw new Exception("submesh " + s + " holds " + got + " indices, expected "
                                        + m.submesh_index_counts[s]);
                }
            }
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }
            var parts = assetFolder.Split('/');
            var cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(cur, parts[i]);
                }
                cur = next;
            }
        }
    }
}
