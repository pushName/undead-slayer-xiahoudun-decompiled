using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UndeadSlayer.Migration
{
    /// Applies the Unity 4 PhysicsManager values recorded in tools/migration/maindata-physics.tsv.
    ///
    /// The collision matrix is the reason this file exists. 27 of the 32 layer masks are
    /// non-default, and they decide which attack layer connects with which target layer, so
    /// the shipped DynamicsManager.asset (all-f = everything collides) is wrong for combat.
    /// It was deliberately never written by hand: Unity 2022 spells m_LayerCollisionMatrix as
    /// 256 hex characters, and whether each 8-character chunk is a uint32's little-endian bytes
    /// or its big-endian digits was not confirmable offline. Both spellings parse without an
    /// error, so guessing wrong permutes every mask's bits and silently changes hit detection.
    ///
    /// So nothing here writes that field. Physics.IgnoreLayerCollision is the only public setter
    /// for a layer pair (there is no Physics.SetLayerCollision), and Unity serialises the result
    /// itself in whatever spelling it uses. The decode below then runs in the OTHER direction --
    /// read the 256 characters Unity wrote and find which reading reproduces the 32 recorded
    /// masks -- which is what turns the open question into a measured answer.
    ///
    /// Two independent read-backs, because they answer different questions: Physics.GetIgnoreLayerCollision
    /// is Unity's own decoded state, and the file text is what survives a restart. The first run
    /// settled both -- the written 256 characters decode back to all 32 recorded masks under the
    /// little-endian-bytes reading, which also means IgnoreLayerCollision does reach the diagonal
    /// (26 of 32 layers do not collide with themselves in this game).
    ///
    /// Menu: Undead Slayer > Apply MainData Physics (Writes DynamicsManager)
    public static class PhysicsRebuilder
    {
        private const int Layers = 32;
        private const int Pairs = Layers * (Layers + 1) / 2;   // 528, including each layer against itself
        private const string SettingsAssetPath = "ProjectSettings/DynamicsManager.asset";
        private const string TsvSubPath = "tools/migration/maindata-physics.tsv";

        private sealed class Expected
        {
            public readonly uint[] Mask = new uint[Layers];
            public readonly string[] LayerName = new string[Layers];
            public bool HaveSolver;
            public bool HaveAngular;
            public int SolverIterations;
            public float MaxAngularSpeed;
        }

        [MenuItem("Undead Slayer/Apply MainData Physics (Writes DynamicsManager)")]
        public static void Apply()
        {
            var want = Read();
            if (want == null)
            {
                return;
            }

            // The masks index layers by number, so a TSV row whose name does not match the
            // project's own layer table means the matrix would land on the wrong layers --
            // the same silent-breakage class this whole tool is trying to avoid. Refuse first.
            var nameProblems = new List<string>();
            for (int i = 0; i < Layers; i++)
            {
                var actual = LayerMask.LayerToName(i) ?? "";
                var recorded = want.LayerName[i] ?? "";
                if (recorded == "(unnamed)") recorded = "";
                if (actual != recorded)
                {
                    nameProblems.Add("%d: tsv '%s' vs project '%s'".Fmt(i, recorded, actual));
                }
            }
            if (nameProblems.Count > 0)
            {
                Debug.LogError(("[PhysicsRebuilder] REFUSING to apply: %d layer name(s) disagree with " +
                                "the project's layer table, so the masks would land on the wrong layers: %s")
                    .Fmt(nameProblems.Count, string.Join(", ", nameProblems)));
                return;
            }

            var before = Snapshot();

            int set = 0, selfRejected = 0;
            for (int i = 0; i < Layers; i++)
            {
                for (int j = i; j < Layers; j++)
                {
                    bool collide = ((want.Mask[i] >> j) & 1u) != 0;
                    // Cross-check the recorded symmetry instead of trusting it: entry i bit j
                    // and entry j bit i must say the same thing about the pair.
                    bool mirror = ((want.Mask[j] >> i) & 1u) != 0;
                    if (collide != mirror)
                    {
                        Debug.LogError("[PhysicsRebuilder] pair (%d,%d) is not symmetric in the tsv".Fmt(i, j));
                        return;
                    }
                    if (Physics.GetIgnoreLayerCollision(i, j) == !collide)
                    {
                        continue;
                    }
                    Physics.IgnoreLayerCollision(i, j, !collide);
                    set++;
                    if (i == j && Physics.GetIgnoreLayerCollision(i, j) != !collide)
                    {
                        selfRejected++;
                    }
                }
            }

            var mgr = AssetDatabase.LoadAllAssetsAtPath(SettingsAssetPath);
            if (mgr == null || mgr.Length == 0)
            {
                Debug.LogError("[PhysicsRebuilder] cannot load the object behind " + SettingsAssetPath +
                               ", so the two scalar fields and the file write are out of reach");
                return;
            }

            var so = new SerializedObject(mgr[0]);
            var iterations = so.FindProperty("m_DefaultSolverIterations");
            var angular = so.FindProperty("m_DefaultMaxAngularSpeed");
            if (iterations == null || angular == null)
            {
                Debug.LogError("[PhysicsRebuilder] m_DefaultSolverIterations / m_DefaultMaxAngularSpeed " +
                               "not found on the serialized PhysicsManager");
                return;
            }
            iterations.intValue = want.SolverIterations;
            angular.floatValue = want.MaxAngularSpeed;
            bool applied = so.ApplyModifiedProperties();

            // The ProjectSettings write is confirmed -- the first run left DynamicsManager.asset
            // carrying the new matrix and both scalars. Which of these calls is load-bearing was
            // not distinguished, so all three stay; the only reason they are in doubt at all is that
            // EditorBuildSettings.asset turned out to be unreachable through SaveAssets(), and that
            // object is not loadable from AssetDatabase in the first place, so the comparison does
            // not transfer.
            EditorUtility.SetDirty(mgr[0]);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var after = Snapshot();
            int wrong = 0, firstWrong = -1;
            var mismatch = new List<string>();
            for (int i = 0; i < Layers; i++)
            {
                for (int j = i; j < Layers; j++)
                {
                    bool collide = ((want.Mask[i] >> j) & 1u) != 0;
                    if (Physics.GetIgnoreLayerCollision(i, j) != !collide)
                    {
                        if (wrong == 0) firstWrong = i * Layers + j;
                        wrong++;
                        if (mismatch.Count < 8) mismatch.Add("(%d,%d)".Fmt(i, j));
                    }
                }
            }

            // There is deliberately no in-memory read-back. SerializedObject does expose
            // m_LayerCollisionMatrix, but as propertyType Generic rather than a string (measured on
            // the first run), so it can never hand us the spelling we are trying to learn. The file
            // text is the only serialized view -- and it is also the view that has to survive a
            // restart, so it is the one worth asserting on.
            var diskText = ReadFileText();
            var diskHex = ExtractHex(diskText, "m_LayerCollisionMatrix");

            Debug.Log(("[PhysicsRebuilder] pairs now ignoring collisions: %d of %d (before: %d), " +
                       "IgnoreLayerCollision calls made: %d, self-pairs rejected by Unity: %d\n" +
                       "  runtime read-back via Physics.GetIgnoreLayerCollision: %d/%d pairs agree with the tsv%s\n" +
                       "  ApplyModifiedProperties(scalars): %s, Physics.defaultSolverIterations now %d, " +
                       "Physics.defaultMaxAngularSpeed now %s\n"
                       + "  ---- encoding answer (this is the part README gap 6 needs) ----").Fmt(
                after.Count(v => v), Pairs, before.Count(v => v), set, selfRejected,
                Pairs - wrong, Pairs,
                wrong == 0 ? "" : " -- MISMATCH " + string.Join(" ", mismatch),
                applied ? "true" : "false", Physics.defaultSolverIterations,
                Physics.defaultMaxAngularSpeed.ToString("0.####", CultureInfo.InvariantCulture)));

            Report("file on disk", diskHex, want.Mask);
            ReportScalar(diskText, "m_DefaultSolverIterations", want.SolverIterations.ToString());
            ReportScalar(diskText, "m_DefaultMaxAngularSpeed",
                         want.MaxAngularSpeed.ToString("0.####", CultureInfo.InvariantCulture));

            if (wrong > 0)
            {
                Debug.LogError(("[PhysicsRebuilder] NOT applied: %d pair(s) still disagree with the tsv " +
                                "(first one is pair index %d). The runtime state is what the game would use.")
                    .Fmt(wrong, firstWrong));
            }
            else if (diskHex == null || diskHex.Length != Layers * 8)
            {
                Debug.LogError("[PhysicsRebuilder] runtime state is correct but " + SettingsAssetPath +
                               " does not carry a 256-character m_LayerCollisionMatrix, so the change may " +
                               "not survive a restart. Do not call this done until the file agrees.");
            }
        }

        // Reads the recorded masks and the two scalars we have decided to carry over. Rows whose
        // action is "already equal" or "NO MAPPING" are left alone -- those are statements about
        // the current file, not instructions.
        private static Expected Read()
        {
            var tsv = FindPath(TsvSubPath);
            if (tsv == null)
            {
                Debug.LogError("[PhysicsRebuilder] " + TsvSubPath + " not found");
                return null;
            }

            var want = new Expected();
            var seen = new HashSet<int>();
            var problems = new List<string>();

            foreach (var raw in File.ReadAllLines(tsv))
            {
                if (raw.StartsWith("#") || !raw.Contains("\t"))
                {
                    continue;
                }
                var f = raw.Split('\t');
                if (f[0] == "matrix")
                {
                    if (f.Length < 5 || !int.TryParse(f[1], out int index) || index < 0 || index >= Layers)
                    {
                        problems.Add("unparsable matrix row: " + raw);
                        continue;
                    }
                    uint dec, hex;
                    try
                    {
                        dec = Convert.ToUInt32(f[3], CultureInfo.InvariantCulture);
                        hex = Convert.ToUInt32(f[4].StartsWith("0x") ? f[4].Substring(2) : f[4], 16);
                    }
                    catch (Exception e)
                    {
                        problems.Add("mask %s does not parse (%s)".Fmt(raw, e.GetType().Name));
                        continue;
                    }
                    if (dec != hex)
                    {
                        problems.Add("layer %d: mask_dec %d != mask_hex %s".Fmt(index, dec, f[4]));
                        continue;
                    }
                    if (!seen.Add(index))
                    {
                        problems.Add("layer %d appears twice".Fmt(index));
                        continue;
                    }
                    want.Mask[index] = dec;
                    want.LayerName[index] = f[2];
                }
                else if (f[0] == "scalar" && f.Length >= 4)
                {
                    if (f[1] == "m_SolverIterationCount" && int.TryParse(f[2], out int iters))
                    {
                        want.SolverIterations = iters;
                        want.HaveSolver = true;
                    }
                    else if (f[1] == "m_MaxAngularVelocity" &&
                             float.TryParse(f[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float mas))
                    {
                        want.MaxAngularSpeed = mas;
                        want.HaveAngular = true;
                    }
                }
            }

            if (seen.Count != Layers)
            {
                problems.Add("expected 32 matrix rows, got " + seen.Count);
            }
            if (!want.HaveSolver || !want.HaveAngular)
            {
                problems.Add("missing the m_SolverIterationCount and/or m_MaxAngularVelocity row");
            }
            if (problems.Count > 0)
            {
                Debug.LogError("[PhysicsRebuilder] REFUSING: " + string.Join("; ", problems));
                return null;
            }
            return want;
        }

        private static List<bool> Snapshot()
        {
            var list = new List<bool>(Layers * (Layers + 1) / 2);
            for (int i = 0; i < Layers; i++)
            {
                for (int j = i; j < Layers; j++)
                {
                    list.Add(Physics.GetIgnoreLayerCollision(i, j));
                }
            }
            return list;
        }

        // Four candidate spellings, so the answer is a measurement rather than a guess: byte order
        // inside each 8-character chunk, crossed with whether chunk k describes layer k or 31-k.
        private static void Report(string where, string hex, uint[] want)
        {
            if (string.IsNullOrEmpty(hex))
            {
                Debug.LogWarning("[PhysicsRebuilder] %s: no m_LayerCollisionMatrix string to decode".Fmt(where));
                return;
            }
            if (hex.Length != Layers * 8)
            {
                Debug.LogWarning("[PhysicsRebuilder] %s: matrix string is %d chars, not %d -- %s"
                    .Fmt(where, hex.Length, Layers * 8, hex));
                return;
            }
            if (hex.Any(c => !Uri.IsHexDigit(c)))
            {
                Debug.LogWarning("[PhysicsRebuilder] %s: matrix string is not hex -- %s".Fmt(where, hex));
                return;
            }

            var cands = new[]
            {
                "big-endian digits, layer k at chunk k",
                "little-endian bytes, layer k at chunk k",
                "big-endian digits, chunk order reversed",
                "little-endian bytes, chunk order reversed",
            };
            var hits = new int[4];
            for (int c = 0; c < 4; c++)
            {
                bool byteSwap = c % 2 == 1;
                bool reverse = c >= 2;
                for (int i = 0; i < Layers; i++)
                {
                    var chunk = hex.Substring(reverse ? (Layers - 1 - i) * 8 : i * 8, 8);
                    if (byteSwap) chunk = SwapBytes(chunk);
                    if (Convert.ToUInt32(chunk, 16) == want[i]) hits[c]++;
                }
            }

            var matched = Enumerable.Range(0, 4).Where(c => hits[c] == Layers).ToArray();
            var detail = string.Join(", ", Enumerable.Range(0, 4)
                .Select(c => "%s -> %d/%d".Fmt(cands[c], hits[c], Layers)));
            if (matched.Length == 1)
            {
                Debug.Log(("[PhysicsRebuilder] %s: CONFIRMED spelling is '%s' (all 32 masks reproduce). " +
                           "Other readings: %s").Fmt(where, cands[matched[0]], detail));
            }
            else
            {
                Debug.LogError(("[PhysicsRebuilder] %s: no single spelling reproduces all 32 masks (%s). " +
                                "chunk0='%s' layer0=%s(%s), chunk1='%s' layer1=%s(%s)")
                    .Fmt(where, detail,
                         hex.Substring(0, 8), want[0].ToString("x8"), LayerMask.LayerToName(0),
                         hex.Substring(8, 8), want[1].ToString("x8"), LayerMask.LayerToName(1)));
            }
        }

        private static void ReportScalar(string text, string field, string wantValue)
        {
            var line = text == null ? null
                : text.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith(field + ":"));
            if (line == null)
            {
                Debug.LogWarning("[PhysicsRebuilder] %s not found in the file on disk".Fmt(field));
                return;
            }
            var got = line.Substring(line.IndexOf(':') + 1).Trim();
            if (got == wantValue)
            {
                Debug.Log("[PhysicsRebuilder] file on disk: %s = %s (as recorded)".Fmt(field, got));
            }
            else
            {
                Debug.LogWarning(("[PhysicsRebuilder] file on disk: %s = %s, expected %s -- the runtime value " +
                                  "may not have been written out")
                    .Fmt(field, got, wantValue));
            }
        }

        // Reverse the four two-character groups, i.e. read the chunk's bytes back in memory order.
        // Swapping the two characters INSIDE each group instead (which is what this first did) is a
        // nibble swap, not a byte swap: it still scores 5/32, because a mask that is palindromic per
        // nibble, above all 0xFFFFFFFF, survives either reading, so the bug does not show up as "no
        // candidate matched" -- it shows up as the right candidate scoring 5/32 and a confusing
        // "no single spelling" error. That is exactly what the first run logged.
        private static string SwapBytes(string chunk)
        {
            var sb = new StringBuilder(chunk.Length);
            for (int i = chunk.Length - 2; i >= 0; i -= 2)
            {
                sb.Append(chunk, i, 2);
            }
            return sb.ToString();
        }

        private static string ExtractHex(string text, string field)
        {
            if (text == null)
            {
                return null;
            }
            var line = text.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith(field + ":"));
            return line?.Substring(line.IndexOf(':') + 1).Trim();
        }

        private static string ReadFileText()
        {
            var path = FindPath(SettingsAssetPath);
            return path == null ? null : File.ReadAllText(path);
        }

        // Application.dataPath is <repo>/unity/Assets, so ProjectSettings is one level up and the
        // extractor output two. Absolute, because the Editor's CWD is not the project folder when
        // Unity is launched from a shortcut.
        private static string FindPath(string relativePath)
        {
            var unityRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            foreach (var candidate in new[]
                     {
                         Path.Combine(unityRoot, "..", relativePath),
                         Path.Combine(unityRoot, relativePath),
                         relativePath,
                     })
            {
                var full = Path.GetFullPath(candidate);
                if (File.Exists(full))
                {
                    return full;
                }
            }
            return null;
        }
    }
}
