using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UndeadSlayer.Migration
{
    /// Creates the project's .mat assets from tools/migration/materials.json, which
    /// export_materials.py produced out of the Unity 4 build data.
    ///
    /// Unity creates the file, so the .mat YAML is guaranteed to be whatever 2022.3 expects
    /// -- same principle as SceneRebuilder.
    ///
    /// Shader names were already remapped in tools/migration/shader-remap.json (Unity 4's
    /// "Mobile-Diffuse" etc. do not exist in 2022). If Shader.Find returns null here, that
    /// table is wrong or the built-in shader was stripped from the build -- it is reported,
    /// never silently substituted.
    ///
    /// Menu: Undead Slayer > Build Materials From JSON
    public static class MaterialRebuilder
    {
        [Serializable]
        private class TexRef
        {
            public string name;
            public string path;
            public float[] scale;
            public float[] offset;
        }

        [Serializable]
        private class ColorRef
        {
            public string name;
            public float[] rgba;
        }

        [Serializable]
        private class FloatRef
        {
            public string name;
            public float value;
        }

        [Serializable]
        private class Mat
        {
            public string key;
            public string asset_path;
            public string name;
            public string shader_src_name;
            public string shader_target;
            public int render_queue = -1;
            public TexRef[] textures;
            public ColorRef[] colors;
            public FloatRef[] floats;
        }

        [Serializable]
        private class MatFile
        {
            public string color_space;
            public int material_count;
            public Mat[] materials;
        }

        [MenuItem("Undead Slayer/Build Materials From JSON")]
        private static void Build()
        {
            var jsonPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "tools", "migration", "materials.json"));
            if (!File.Exists(jsonPath))
            {
                Debug.LogError("[MaterialRebuilder] missing " + jsonPath +
                               " -- run tools/migration/export_materials.py first");
                return;
            }

            var data = JsonUtility.FromJson<MatFile>(File.ReadAllText(jsonPath));
            if (data == null || data.materials == null || data.materials.Length == 0)
            {
                Debug.LogError("[MaterialRebuilder] " + jsonPath + " has no materials");
                return;
            }
            if (data.color_space == "linear")
            {
                Debug.LogError("[MaterialRebuilder] materials.json claims linear colour space, " +
                               "but the shipped project is Gamma (m_ActiveColorSpace = 0). " +
                               "Refusing to import unconverted colour values.");
                return;
            }

            EnsureFolder("Assets/Imported/Materials");

            int created = 0, skippedShader = 0, skippedTexture = 0, replaced = 0;
            var missingShaders = new Dictionary<string, int>();

            foreach (var m in data.materials)
            {
                var shader = Shader.Find(m.shader_target);
                if (shader == null)
                {
                    skippedShader++;
                    int prev;
                    missingShaders.TryGetValue(m.shader_target, out prev);
                    missingShaders[m.shader_target] = prev + 1;
                    continue;
                }

                var existing = AssetDatabase.LoadAssetAtPath<Material>(m.asset_path);
                Material mat;
                if (existing != null)
                {
                    mat = existing;
                    mat.shader = shader;
                    replaced++;
                }
                else
                {
                    mat = new Material(shader);
                }

                // .mat files persist m_Name, and Unity warns "Main Object Name ... does not
                // match filename" whenever that stored value differs from the file name. An
                // earlier run wrote the bare source name (e.g. "ef_energygather") into all 349
                // files, so merely *stopping* that assignment does not repair them -- the
                // stored value has to be rewritten to match. Assigning the file-name stem also
                // makes the step self-healing on an already-populated project.
                mat.name = Path.GetFileNameWithoutExtension(m.asset_path);

                if (m.textures != null)
                {
                    foreach (var t in m.textures)
                    {
                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(t.path);
                        if (tex == null)
                        {
                            skippedTexture++;
                            continue;
                        }
                        mat.SetTexture(t.name, tex);
                        // A texture bound to a bump/normal slot must be imported as a normal
                        // map, or Unity pops "NormalMap settings" on every material that uses
                        // it. Driven by the SLOT NAME from the shipped data, never by the
                        // filename -- e.g. bg_normalcolor_68.png has "normal" in its name but
                        // is referenced by no material at all.
                        if (IsNormalSlot(t.name))
                        {
                            MarkNormalMap(t.path);
                        }
                        if (t.scale != null && t.scale.Length >= 2)
                        {
                            mat.SetTextureScale(t.name, new Vector2(t.scale[0], t.scale[1]));
                        }
                        if (t.offset != null && t.offset.Length >= 2)
                        {
                            mat.SetTextureOffset(t.name, new Vector2(t.offset[0], t.offset[1]));
                        }
                    }
                }

                if (m.colors != null)
                {
                    foreach (var c in m.colors)
                    {
                        if (c.rgba != null && c.rgba.Length >= 4)
                        {
                            mat.SetColor(c.name, new Color(c.rgba[0], c.rgba[1], c.rgba[2], c.rgba[3]));
                        }
                    }
                }

                if (m.floats != null)
                {
                    foreach (var f in m.floats)
                    {
                        mat.SetFloat(f.name, f.value);
                    }
                }

                if (m.render_queue >= 0)
                {
                    mat.renderQueue = m.render_queue;
                }

                if (existing == null)
                {
                    AssetDatabase.CreateAsset(mat, m.asset_path);
                }
                else
                {
                    EditorUtility.SetDirty(mat);
                }
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[MaterialRebuilder] done");
            sb.AppendLine("  materials in json     : " + data.materials.Length);
            sb.AppendLine("  created/updated       : " + created + "  (" + replaced + " overwritten)");
            sb.AppendLine("  skipped, Shader.Find null : " + skippedShader);
            foreach (var kv in missingShaders)
            {
                sb.AppendLine("      " + kv.Key + " x" + kv.Value);
            }
            sb.AppendLine("  texture slots pointing at a missing png : " + skippedTexture);
            sb.AppendLine("  textures switched to NormalMap import   : " + normalMapsMarked);
            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog("Material rebuild",
                "created/updated " + created + " of " + data.materials.Length +
                "\nShader.Find failures: " + skippedShader +
                "\nmissing textures: " + skippedTexture +
                "\n\nDetails are in the Console.", "OK");
        }

        private static readonly HashSet<string> NormalSlots =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "_BumpMap", "_NormalMap", "_Normal", "_Bump",
            };

        private static bool IsNormalSlot(string slot)
        {
            return !string.IsNullOrEmpty(slot) && NormalSlots.Contains(slot);
        }

        private static readonly HashSet<string> MarkedNormalMaps = new HashSet<string>();

        private static void MarkNormalMap(string assetPath)
        {
            if (!MarkedNormalMaps.Add(assetPath))
            {
                return;
            }
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("[MaterialRebuilder] no TextureImporter for " + assetPath);
                return;
            }
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
                normalMapsMarked++;
            }
        }

        private static int normalMapsMarked;

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
