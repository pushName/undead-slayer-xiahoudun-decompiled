using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UndeadSlayer.Migration
{
    /// Read-only probe: do the 12 shader names stored in the Unity 4 build data still
    /// resolve in Unity 2022.3, and if not, which current built-in name replaces each one?
    ///
    /// Every Material we have to recreate references one of these names, so this single
    /// answer decides whether the material step is a 12-row remap table or a rewrite job.
    /// Nothing here mutates any asset.
    ///
    /// Menu: Undead Slayer > Probe Shader Names (Read-only)
    public static class ShaderProbe
    {
        // The distinct Shader.m_Name values that materials ACTUALLY reference, resolved by
        // walking every Material's m_Shader pointer through its file's externals table.
        // 355 materials, 0 unresolved, 15 distinct names. (An earlier pass reported only 12
        // because it scanned levels+sharedassets and missed `unity default resources` and
        // the AssetBundles -- same under-scoping mistake that produced the wrong "31".)
        // The trailing number is how many materials use it.
        private static readonly string[] Shipped =
        {
            "Mobile-Particle-Add",                        // 104
            "Mobile-VertexLit-OnlyDirectionalLights",     // 100
            "Unlit-Alpha",                                //  43
            "Unlit-Normal",                               //  30
            "Mobile-Particle-Alpha",                      //  21
            "Mobile-Particle-Multiply",                   //  16
            "Mobile Lightmap Only",                       //  10
            "Mobile Alpha Blend plus",                    //   8
            "Mobile Lightmap Only2",                      //   8
            "Font",                                       //   6
            "Mobile-Diffuse",                             //   4
            "Particle Add",                               //   2
            "Mobile-BumpSpec",                            //   1
            "Mobile-VertexLit",                           //   1
            "Sprites-Default",                            //   1
        };

        // Also worth knowing whether these still exist at all, since the game uses them.
        private static readonly string[] Extra =
        {
            "Standard", "Unlit/Color", "Unlit/Texture", "Unlit/Transparent",
            "Mobile/Diffuse", "Mobile/Vertex Lit", "Mobile/Bumped Specular",
            "Legacy Shaders/Diffuse", "Legacy Shaders/VertexLit",
            "Legacy Shaders/Bumped Diffuse", "Legacy Shaders/Transparent/Diffuse",
            "Legacy Shaders/Particles/Additive", "Legacy Shaders/Particles/Alpha Blended",
            "Legacy Shaders/Particles/Multiply",
            "Particles/Standard Unlit", "Particles/Unlit", "Particles/Added",
            "Particles/Alpha Blended", "Particles/Multiply", "Particles/Additive",
            "Sprites/Default", "UI/Default", "Diffuse", "VertexLit",
        };

        // Candidate 2022 built-in names, keyed by the shipped name they would replace.
        private static readonly Dictionary<string, string[]> Candidates =
            new Dictionary<string, string[]>
            {
                { "Mobile-Diffuse", new[] { "Mobile/Diffuse", "Legacy Shaders/Diffuse", "Diffuse" } },
                { "Mobile-BumpSpec", new[] { "Mobile/Bumped Specular", "Legacy Shaders/Bumped Diffuse", "Bumped Specular" } },
                { "Mobile-VertexLit", new[] { "Mobile/Vertex Lit", "Legacy Shaders/VertexLit", "VertexLit" } },
                { "Mobile-VertexLit-OnlyDirectionalLights", new[] { "Mobile/Vertex Lit", "Legacy Shaders/VertexLit" } },
                { "Mobile Lightmap Only", new[] { "Mobile/Diffuse", "Unlit/Texture" } },
                { "Mobile Lightmap Only2", new[] { "Mobile/Diffuse", "Unlit/Texture" } },
                { "Mobile Alpha Blend plus", new[] { "Mobile/Diffuse", "Transparent/Diffuse", "Legacy Shaders/Transparent/Diffuse" } },
                { "Mobile-Particle-Add", new[] { "Particles/Additive", "Legacy Shaders/Particles/Additive", "Particles/Standard Unlit" } },
                { "Mobile-Particle-Alpha", new[] { "Particles/Alpha Blended", "Legacy Shaders/Particles/Alpha Blended", "Particles/Standard Unlit" } },
                { "Mobile-Particle-Multiply", new[] { "Particles/Multiply", "Legacy Shaders/Particles/Multiply" } },
                { "Particle Add", new[] { "Particles/Additive", "Legacy Shaders/Particles/Additive" } },
                { "Unlit-Alpha", new[] { "Unlit/Transparent", "Unlit/Texture", "Legacy Shaders/Diffuse" } },
                { "Unlit-Normal", new[] { "Unlit/Texture", "Unlit/Color", "Legacy Shaders/Diffuse" } },
                // New in this pass -- the three the 12-name probe never covered.
                { "Sprites-Default", new[] { "Sprites/Default", "UI/Default" } },
                { "Font", new[] { "UI/Default", "Sprites/Default", "Legacy Shaders/Diffuse" } },
            };

        [MenuItem("Undead Slayer/Probe Shader Names (Read-only)")]
        private static void Probe()
        {
            var sb = new StringBuilder();
            sb.AppendLine("===== SHADER PROBE (read-only) =====");

            int alive = 0;
            sb.AppendLine("-- the 12 names stored in the shipped data --");
            foreach (var n in Shipped)
            {
                var s = Shader.Find(n);
                if (s != null)
                {
                    alive++;
                }
                sb.AppendLine("  Shader.Find(\"" + n + "\") -> " + (s != null ? "FOUND" : "null"));
            }
            sb.AppendLine("  still resolvable by their original name: " + alive + " / " + Shipped.Length);

            sb.AppendLine();
            sb.AppendLine("-- replacement candidates --");
            foreach (var kv in Shipped)
            {
                string[] cands;
                if (!Candidates.TryGetValue(kv, out cands))
                {
                    continue;
                }
                var hits = new List<string>();
                foreach (var c in cands)
                {
                    if (Shader.Find(c) != null)
                    {
                        hits.Add(c);
                    }
                }
                sb.AppendLine("  " + kv + "  =>  " + (hits.Count > 0 ? string.Join(" | ", hits) : "NO CANDIDATE FOUND"));
            }

            sb.AppendLine();
            sb.AppendLine("-- general built-in names --");
            foreach (var n in Extra)
            {
                sb.AppendLine("  " + n + " -> " + (Shader.Find(n) != null ? "exists" : "null"));
            }

            var report = sb.ToString();
            UnityEngine.Debug.Log(report);
            EditorUtility.DisplayDialog(
                "Shader probe",
                "resolvable by original name: " + alive + " / " + Shipped.Length +
                "\n\nFull report is in the Console (copy the block that starts with '===== SHADER PROBE').",
                "OK");
        }
    }
}
