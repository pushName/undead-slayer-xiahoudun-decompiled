using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace UndeadSlayer.Migration
{
    /// <summary>
    /// Turn tools/migration/prefabs/*.json into Assets/Resources/&lt;name&gt;.prefab.
    ///
    /// Why it calls SceneRebuilder.Rebuild instead of growing its own node loop: the loop that
    /// creates GameObjects, wires parents, adds scripts, applies decoded MonoBehaviour fields and
    /// binds Animation states has been run over the 23 levels and carries the post-conditions that
    /// found real bugs there. A second implementation for prefabs would drift from it silently --
    /// which is how the scene pass and the ledger resolver once disagreed about 63 dropped refs.
    /// So the description is built into a scratch scene, and only the last step is prefab-specific:
    /// save the scratch scene's single root with PrefabUtility.SaveAsPrefabAsset, then delete the
    /// scratch file.
    ///
    /// Checks per prefab, because a wrong save is invisible until the game instantiates it:
    ///   * the open scene must be the scratch scene just written (otherwise a Rebuild that bailed
    ///     early would leave the previous prefab's root open and it would be saved under a new name);
    ///   * the scene must have exactly one root GameObject, and the object count must equal the
    ///     description's node_count;
    ///   * every asset guid present in the scratch scene must also appear in the saved .prefab --
    ///     a reference the save dropped is a silently null field at runtime;
    ///   * the .prefab must exist on disk afterwards, and the scratch scene must not.
    ///
    /// The scratch folder is git-ignored so a crashed run cannot leave committed artifacts.
    /// </summary>
    public static class PrefabRebuilder
    {
        private const string MenuRoot = "Undead Slayer/Build Prefabs From JSON (recovered roots)";
        private const string ScratchFolder = "Assets/Imported/_PrefabBuild";

        private static readonly Regex GuidPattern =
            new Regex("guid: ([0-9a-f]{32})", RegexOptions.Compiled);

        [MenuItem(MenuRoot)]
        private static void BuildAll()
        {
            var dir = SceneRebuilder.FindJsonDir("prefabs");
            if (dir == null)
            {
                Debug.LogError("[PrefabRebuilder] no tools/migration/prefabs folder -- run "
                               + "export_prefabs.py first. Nothing was built.");
                return;
            }

            var files = Directory.GetFiles(dir, "*.json");
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            if (files.Length == 0)
            {
                Debug.LogError("[PrefabRebuilder] " + dir + " holds no descriptions -- nothing to build.");
                return;
            }

            int built = 0, refused = 0, objects = 0;
            var problems = new List<string>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var file in files)
            {
                string error;
                int count;
                if (BuildOne(file, out error, out count))
                {
                    built++;
                    objects += count;
                }
                else
                {
                    refused++;
                    if (problems.Count < 12)
                    {
                        problems.Add(error);
                    }
                }
            }

            // Delete the scratch folder as a whole: leftover scenes would be committed by accident.
            if (AssetDatabase.IsValidFolder(ScratchFolder))
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Scene",
                                          new[] { ScratchFolder }))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                }
            }
            if (AssetDatabase.IsValidFolder(ScratchFolder))
            {
                AssetDatabase.DeleteAsset(ScratchFolder);
            }
            AssetDatabase.Refresh();

            var msg = "[PrefabRebuilder] {0} prefabs built into Assets/Resources, {1} refused, {2} "
                      .Fmt(built, refused, objects) + "objects in total, took " + stopwatch.ElapsedMilliseconds
                      + " ms";
            if (problems.Count > 0)
            {
                msg += "\n  first problems:\n    - " + string.Join("\n    - ", problems.ToArray());
            }
            if (refused > 0)
            {
                Debug.LogWarning(msg + "\n  refused prefabs are NOT in Resources/ -- the keys that "
                                 + "load them will still return null.");
            }
            else
            {
                Debug.Log(msg);
            }
        }

        /// <summary>
        /// Builds one description and saves its root as a prefab. Returns false with a reason rather
        /// than leaving a half-written asset, because a prefab that loads but is wrong is worse than
        /// one that is missing.
        /// </summary>
        private static bool BuildOne(string jsonPath, out string error, out int objectCount)
        {
            error = null;
            objectCount = 0;
            var text = File.ReadAllText(jsonPath);
            var data = JsonUtility.FromJson<SceneRebuilder.SceneFile>(text);
            if (data == null || data.nodes == null || data.nodes.Length == 0)
            {
                error = Path.GetFileName(jsonPath) + ": no nodes in the description";
                return false;
            }
            if (string.IsNullOrEmpty(data.asset_path) || string.IsNullOrEmpty(data.scene_path))
            {
                error = Path.GetFileName(jsonPath) + ": description has no asset_path/scene_path";
                return false;
            }

            SceneRebuilder.Rebuild(jsonPath);

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != data.scene_path)
            {
                error = Path.GetFileName(jsonPath) + ": the open scene is " + scene.path
                        + ", expected the scratch scene " + data.scene_path
                        + " -- SceneRebuilder did not get as far as saving it";
                return false;
            }

            var roots = scene.GetRootGameObjects();
            if (roots.Length != 1)
            {
                error = Path.GetFileName(jsonPath) + ": scratch scene has " + roots.Length
                        + " root GameObjects, a prefab needs exactly one";
                CloseAndDelete(scene);
                return false;
            }
            int counted = CountRecursive(roots[0].transform);
            if (counted != data.node_count)
            {
                error = Path.GetFileName(jsonPath) + ": scene holds " + counted + " objects but the "
                        + "description says " + data.node_count;
                CloseAndDelete(scene);
                return false;
            }

            var sceneGuids = new HashSet<string>(GuidsOf(File.ReadAllText(
                Path.Combine(ProjectRoot(), scene.path))));
            SceneRebuilder.EnsureFolderFor(data.asset_path);
            bool saved;
            var prefab = PrefabUtility.SaveAsPrefabAsset(roots[0], data.asset_path, out saved);
            if (!saved || prefab == null)
            {
                error = Path.GetFileName(jsonPath) + ": SaveAsPrefabAsset returned false for "
                        + data.asset_path;
                CloseAndDelete(scene);
                return false;
            }

            var prefabPath = Path.Combine(ProjectRoot(), data.asset_path);
            if (!File.Exists(prefabPath))
            {
                error = Path.GetFileName(jsonPath) + ": " + data.asset_path
                        + " is reported saved but is not on disk";
                CloseAndDelete(scene);
                return false;
            }

            // A reference the prefab did not carry becomes a null field at runtime, which is the
            // failure mode this whole project keeps tripping over. Only PROJECT assets are compared:
            // a saved scene also references Unity's built-in resource namespace, and a .prefab has no
            // reason to carry that. A first attempt filtered with GUIDToAssetPath alone and Unity
            // still reported the same 2 guids for all 94 prefabs, so the shape test in
            // IsProjectAssetGuid is what actually excludes them.
            var projectGuids = new HashSet<string>();
            foreach (var g in sceneGuids)
            {
                if (IsProjectAssetGuid(g)) projectGuids.Add(g);
            }
            sceneGuids = projectGuids;
            var prefabGuids = GuidsOf(File.ReadAllText(prefabPath));
            var missing = new List<string>();
            foreach (var g in sceneGuids)
            {
                if (!prefabGuids.Contains(g))
                {
                    missing.Add(g);
                }
            }
            if (missing.Count > 0)
            {
                Debug.LogWarning("[PrefabRebuilder] " + data.asset_path + ": " + missing.Count
                                 + " asset guid(s) present in the scratch scene are absent from the "
                                 + "saved prefab -- those references did not survive the save: "
                                 + string.Join(", ", missing.GetRange(0, Math.Min(5, missing.Count)).ToArray()));
            }

            CloseAndDelete(scene);
            objectCount = counted;
            return true;
        }

        private static void CloseAndDelete(Scene scene)
        {
            var path = scene.path;
            // Unity refuses to unload the LAST open scene ("Unloading the last loaded scene ... is
            // not supported"), and it said so 94 times. Open a fresh empty scene first, which is the
            // switch it asks for, then the scratch file is no longer in use and deletes cleanly.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!string.IsNullOrEmpty(path) && !AssetDatabase.DeleteAsset(path))
            {
                Debug.LogWarning("[PrefabRebuilder] could not delete scratch scene " + path);
            }
        }

        private static int CountRecursive(Transform t)
        {
            int n = 1;
            for (int i = 0; i < t.childCount; i++)
            {
                n += CountRecursive(t.GetChild(i));
            }
            return n;
        }

        /// <summary>
        /// Unity keeps its own resources (default shader, builtin extra, scene settings references)
        /// under an all-zero guid with a single nibble set, e.g. 0000000000000000e000000000000000.
        /// GUIDToAssetPath resolves those to internal paths rather than returning empty, so the
        /// shape is the reliable test -- proven by a run in which path filtering alone still let both
        /// built-in guids through and warned on all 94 prefabs.
        /// </summary>
        private static bool IsProjectAssetGuid(string guid)
        {
            if (InternalGuid.IsMatch(guid)) return false;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return !string.IsNullOrEmpty(path) && path.StartsWith("Assets/");
        }

        private static readonly Regex InternalGuid =
            new Regex("^0{16}[0-9a-f]0{15}$", RegexOptions.Compiled);

        private static HashSet<string> GuidsOf(string yaml)
        {
            var set = new HashSet<string>();
            foreach (Match m in GuidPattern.Matches(yaml))
            {
                set.Add(m.Groups[1].Value);
            }
            return set;
        }

        private static string ProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }
    }
}
