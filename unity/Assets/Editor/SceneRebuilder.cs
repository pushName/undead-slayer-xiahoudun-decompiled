using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UndeadSlayer.Migration
{
    /// Rebuilds a Unity 2022 scene from the JSON that tools/migration/extract_scene.py
    /// produces out of the Unity 4 build data.
    ///
    /// The point of going through the Editor rather than writing .unity YAML directly is
    /// that Unity serialises the file itself, so every required field, serializedVersion
    /// number and the trailing SceneRoots block is guaranteed to be whatever 2022.3
    /// actually expects. A hand-written scene file would either fail to import or, worse,
    /// import while quietly dropping objects.
    ///
    /// v1 restores STRUCTURE, not STATE: hierarchy, names, layers, tags, active flags,
    /// local TRS, the component set, and MonoBehaviour script bindings. MonoBehaviour field
    /// values are not decoded (Unity 4 builds carry no TypeTree, so they can only be read
    /// per-class from Assembly-CSharp), and Mesh/Material/AudioClip references are still
    /// empty because those assets are not exported yet.
    public static class SceneRebuilder
    {
        private const string MenuRoot = "Undead Slayer/Rebuild Scene From Level JSON...";

        [Serializable]
        private class Trs
        {
            public float[] pos;
            public float[] rot;
            public float[] scale;
        }

        [Serializable]
        private class Comp
        {
            public string kind;      // MonoBehaviour | renderer | component | unsupported
            public string script;    // class name, for kind == MonoBehaviour
            public string guid;      // .cs guid, for kind == MonoBehaviour
            public string cs;        // path to the .cs, for reporting only
            public string type;      // UnityEngine type name
            public bool enabled = true;
            // kind == renderer
            public string mesh;
            public string[] materials;
            public int sorting_order;
            public bool cast_shadows;
            public bool receive_shadows;
            public int[] bones;
            public int root_bone;
        }

        [Serializable]
        private class Node
        {
            public int id;
            public int go_id;
            public string name;
            public int layer;
            public string tag;
            public bool active = true;
            public Trs trs;
            public int parent;
            public int[] children;
            public Comp[] components;
        }

        [Serializable]
        private class SceneFile
        {
            public string source_level;
            public string scene_name;
            public string scene_path;
            public int build_index;
            public int node_count;
            public int[] root_ids;
            public string[] warnings;
            public Node[] nodes;
        }

        [MenuItem(MenuRoot)]
        private static void RebuildInteractive()
        {
            var defaultDir = FindJsonDir();
            var file = EditorUtility.OpenFilePanel(
                "选择要重建的 level JSON", defaultDir ?? "Assets", "json");
            if (string.IsNullOrEmpty(file))
            {
                return;
            }
            Rebuild(file);
        }

        /// Rebuilds one JSON without any UI. Handy from the command line:
        ///   Unity -batchmode -executeMethod UndeadSlayer.Migration.SceneRebuilder.RebuildFromArgs +json <path>
        public static void Rebuild(string jsonPath)
        {
            var text = File.ReadAllText(jsonPath);
            var data = JsonUtility.FromJson<SceneFile>(text);
            if (data == null || data.nodes == null || data.nodes.Length == 0)
            {
                Debug.LogError("[SceneRebuilder] %s has no nodes".Fmt(jsonPath));
                return;
            }

            var dropped = data.nodes
                .SelectMany(n => n.components ?? new Comp[0])
                .Where(c => c.kind == "unsupported")
                .GroupBy(c => c.type)
                .Select(g => "%s x%d".Fmt(g.Key, g.Count()));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var byId = new Dictionary<int, GameObject>();
            int addedScripts = 0, missingScripts = 0, addedComps = 0, unknownComps = 0;
            int meshesSet = 0, matsSet = 0, missingAssets = 0;
            var skinJobs = new List<SkinJob>();

            foreach (var n in data.nodes)
            {
                var go = new GameObject(n.name);
                byId[n.id] = go;
            }

            // Parent first, then local TRS: reparenting rewrites the local transform.
            foreach (var n in data.nodes)
            {
                var go = byId[n.id];
                Transform t = go.transform;

                if (n.parent != 0 && byId.TryGetValue(n.parent, out var pObj))
                {
                    t.SetParent(pObj.transform, false);
                }

                if (n.trs != null)
                {
                    t.localPosition = V3(n.trs.pos);
                    t.localScale = V3(n.trs.scale);
                    if (n.trs.rot != null && n.trs.rot.Length >= 4)
                    {
                        t.localRotation = new Quaternion(n.trs.rot[0], n.trs.rot[1],
                                                         n.trs.rot[2], n.trs.rot[3]);
                    }
                }

                if (n.layer >= 0 && n.layer <= 31)
                {
                    go.layer = n.layer;
                }

                if (!string.IsNullOrEmpty(n.tag))
                {
                    try
                    {
                        go.tag = n.tag;
                    }
                    catch (UnityException)
                    {
                        Debug.LogWarning(
                            "[SceneRebuilder] tag '%s' is not defined in TagManager; %s left Untagged"
                            .Fmt(n.tag, n.name));
                    }
                }

                foreach (var c in n.components ?? new Comp[0])
                {
                    if (c.kind == "MonoBehaviour")
                    {
                        var type = FindType(c.script);
                        if (type == null)
                        {
                            missingScripts++;
                            // Say WHY. FindType filters on IsAssignableFrom(MonoBehaviour), so a null
                            // here has two different causes, and blaming "not found" would point at a
                            // missing file when the file is actually present.
                            // One template per branch, assigned before formatting: "a + b".Fmt(...)
                            // must be parenthesised or the call binds to b alone and %s leaks through.
                            var why = FindAnyType(c.script) != null
                                ? "[SceneRebuilder] %s: script type '%s' EXISTS but is not a MonoBehaviour (%s) -- cannot be attached; this is a property of the original build data, not a lost file"
                                : "[SceneRebuilder] %s: script type '%s' not found (%s)";
                            Debug.LogError(why.Fmt(n.name, c.script, c.cs));
                            continue;
                        }
                        if (go.GetComponent(type) != null)
                        {
                            continue; // same class twice on one GameObject is not allowed
                        }
                        var mb = go.AddComponent(type);
                        ((MonoBehaviour)mb).enabled = c.enabled;
                        addedScripts++;
                    }
                    else if (c.kind == "component")
                    {
                        var type = FindUnityType(c.type);
                        if (type == null)
                        {
                            unknownComps++;
                            Debug.LogWarning(
                                "[SceneRebuilder] %s: component type '%s' not resolvable".Fmt(n.name, c.type));
                            continue;
                        }
                        if (go.GetComponent(type) != null)
                        {
                            continue;
                        }
                        go.AddComponent(type);
                        addedComps++;
                    }
                    else if (c.kind == "renderer")
                    {
                        ApplyRenderer(go, c, byId, skinJobs, ref meshesSet, ref matsSet, ref missingAssets);
                    }
                    // kind == "unsupported" is counted up front and reported, not added.
                }
            }

            // Bones point at Transforms elsewhere in the scene, so they can only be wired
            // once every GameObject exists.
            foreach (var job in skinJobs)
            {
                var found = new List<Transform>(job.Comp.bones.Length);
                foreach (var id in job.Comp.bones)
                {
                    GameObject target;
                    if (byId.TryGetValue(id, out target)) found.Add(target.transform);
                }
                if (found.Count == job.Comp.bones.Length && found.Count > 0)
                {
                    job.Renderer.bones = found.ToArray();
                    if (job.Comp.root_bone != 0)
                    {
                        GameObject rb;
                        if (byId.TryGetValue(job.Comp.root_bone, out rb))
                        {
                            job.Renderer.rootBone = rb.transform;
                        }
                    }
                }
                else
                {
                    missingAssets++;
                    Debug.LogWarning("[SceneRebuilder] " + job.Name +
                        ": skeleton unresolved (" + found.Count + "/" + job.Comp.bones.Length +
                        " bones found) -- mesh will not deform");
                }
            }

            // Restore sibling order per parent and per root list.
            foreach (var n in data.nodes)
            {
                if (n.children == null)
                {
                    continue;
                }
                var t = byId[n.id].transform;
                int index = 0;
                foreach (var childId in n.children)
                {
                    if (byId.TryGetValue(childId, out var child) && child.transform.parent == t)
                    {
                        child.transform.SetSiblingIndex(index++);
                    }
                }
            }
            if (data.root_ids != null)
            {
                for (int i = 0; i < data.root_ids.Length; i++)
                {
                    if (byId.TryGetValue(data.root_ids[i], out var root) && root.transform.parent == null)
                    {
                        root.transform.SetSiblingIndex(i);
                    }
                }
            }

            // Deactivate last, so AddComponent ran while everything was awake-able.
            foreach (var n in data.nodes)
            {
                if (!n.active)
                {
                    byId[n.id].SetActive(false);
                }
            }

            EnsureFolderFor(data.scene_path);
            if (!EditorSceneManager.SaveScene(scene, data.scene_path))
            {
                Debug.LogError("[SceneRebuilder] SaveScene failed for " + data.scene_path);
                return;
            }

            Debug.Log(
                "[SceneRebuilder] %s -> %s\n  GameObjects : %d\n  scripts     : %d added, %d unresolved type\n  components  : %d added, %d unresolvable\n  dropped     : %s\n  NOT restored: MonoBehaviour field values (default), AudioClip references (not exported yet), scene settings (Unity defaults)"
                .Fmt(data.source_level, data.scene_path, data.nodes.Length,
                     addedScripts, missingScripts, addedComps, unknownComps,
                     meshesSet, matsSet, missingAssets,
                     dropped.Any() ? string.Join(", ", dropped) : "none"));

            AssetDatabase.Refresh();
        }

        [MenuItem("Undead Slayer/Rebuild ALL Extracted Levels")]
        private static void RebuildAll()
        {
            var dir = FindJsonDir();
            if (dir == null)
            {
                Debug.LogError("[SceneRebuilder] no tools/migration/scenes directory found");
                return;
            }
            var files = Directory.GetFiles(dir, "level*.json").OrderBy(f => f).ToArray();
            foreach (var f in files)
            {
                Rebuild(f);
            }
            Debug.Log("[SceneRebuilder] rebuilt %d scene(s)".Fmt(files.Length));
        }

        // There IS deliberately no "sync build settings" menu item here.
        //
        // EditorBuildSettings.asset was restored from mainData with guid = md5(path) (an APK ships no
        // source guids), but Unity allocates its own guid when it imports each .unity, so the md5
        // values became a contradicting record and Application.LoadLevel("Intro") could not resolve.
        // Assigning EditorBuildSettings.scenes from a menu does NOT fix it: the in-memory list
        // accepts it, but ProjectSettings/EditorBuildSettings.asset is never written -- AssetDatabase
        // .SaveAssets() only covers Assets/, and re-reading the file showed it byte-unchanged with
        // all 24 guids still md5 (that is what the post-condition in verify_scenes.py now checks).
        // The working route is offline: restore_projectsettings.py's scene_guid() reads each file's
        // real guid from its .meta and regenerates the asset, which verified 24/24 and is idempotent.

        private class SkinJob
        {
            public string Name;
            public Comp Comp;
            public SkinnedMeshRenderer Renderer;
        }

        private static void ApplyRenderer(GameObject go, Comp c, Dictionary<int, GameObject> byId,
                                          List<SkinJob> skinJobs, ref int meshesSet, ref int matsSet,
                                          ref int missingAssets)
        {
            var type = FindUnityType(c.type);
            if (type == null)
            {
                Debug.LogWarning("[SceneRebuilder] " + go.name + ": renderer type " + c.type + " unresolvable");
                return;
            }
            var comp = go.GetComponent(type);
            if (comp == null) comp = go.AddComponent(type);

            if (!string.IsNullOrEmpty(c.mesh))
            {
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(c.mesh);
                if (mesh == null)
                {
                    missingAssets++;
                    Debug.LogWarning("[SceneRebuilder] " + go.name + ": mesh not found at " + c.mesh);
                }
                else if (comp is MeshFilter)
                {
                    ((MeshFilter)comp).sharedMesh = mesh;
                    meshesSet++;
                }
                else if (comp is SkinnedMeshRenderer)
                {
                    ((SkinnedMeshRenderer)comp).sharedMesh = mesh;
                    meshesSet++;
                }
            }

            if (c.materials == null) return;

            var mats = new List<Material>();
            foreach (var path in c.materials)
            {
                if (string.IsNullOrEmpty(path)) { mats.Add(null); continue; }
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    missingAssets++;
                    Debug.LogWarning("[SceneRebuilder] " + go.name + ": material not found at " + path);
                    mats.Add(null);
                }
                else
                {
                    mats.Add(m);
                    matsSet++;
                }
            }
            if (mats.Count > 0)
            {
                var r = comp as Renderer;
                if (r != null)
                {
                    r.sharedMaterials = mats.ToArray();
                    r.sortingOrder = c.sorting_order;
                    r.shadowCastingMode = c.cast_shadows
                        ? UnityEngine.Rendering.ShadowCastingMode.On
                        : UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = c.receive_shadows;
                }
            }

            var smr = comp as SkinnedMeshRenderer;
            if (smr != null && c.bones != null && c.bones.Length > 0)
            {
                skinJobs.Add(new SkinJob { Name = go.name, Comp = c, Renderer = smr });
            }
        }

        private static string FindJsonDir()
        {
            // Application.dataPath is <repo>/unity/Assets, so the extractor's output is
            // exactly two levels up. Relative paths would depend on the Editor's CWD,
            // which is not the project folder when Unity is launched from a shortcut.
            var fromDataPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "..", "tools", "migration", "scenes"));
            if (Directory.Exists(fromDataPath))
            {
                return fromDataPath;
            }
            foreach (var candidate in new[] { "tools/migration/scenes", "../tools/migration/scenes" })
            {
                var full = Path.GetFullPath(candidate);
                if (Directory.Exists(full))
                {
                    return full;
                }
            }
            return null;
        }

        private static void EnsureFolderFor(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(dir) || AssetDatabase.IsValidFolder(dir))
            {
                return;
            }
            var parent = Path.GetDirectoryName(dir)?.Replace('\\', '/');
            var leaf = Path.GetFileName(dir);
            if (string.IsNullOrEmpty(parent) || parent == "." || parent == "Assets")
            {
                AssetDatabase.CreateFolder("Assets", leaf);
            }
            else
            {
                EnsureFolderFor(parent + "/x");
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        private static Type FindType(string simpleName)
        {
            if (string.IsNullOrEmpty(simpleName))
            {
                return null;
            }
            if (TypeCache.TryGetValue(simpleName, out var cached))
            {
                return cached;
            }
            Type found = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in SafeTypes(asm))
                {
                    if (t.Name == simpleName && typeof(MonoBehaviour).IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        found = t;
                        break;
                    }
                }
                if (found != null)
                {
                    break;
                }
            }
            TypeCache[simpleName] = found;
            return found;
        }

        private static Type FindUnityType(string name)
        {
            var key = "UnityEngine." + name;
            if (TypeCache.TryGetValue(key, out var cached))
            {
                return cached;
            }
            Type found = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in SafeTypes(asm))
                {
                    if (t.FullName == key && typeof(Component).IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        found = t;
                        break;
                    }
                }
                if (found != null)
                {
                    break;
                }
            }
            TypeCache[key] = found;
            return found;
        }

        /// <summary>
        /// Same search as FindType but WITHOUT the MonoBehaviour filter, used only to explain a
        /// failure: a name that resolves to a real-but-plain class (ChannelMgr on PayMgr) is a
        /// property of the original build data, while a name that resolves to nothing means the
        /// regenerated sources are incomplete. Reporting both as "not found" would blame the wrong
        /// thing. Not cached -- it runs on the error path only.
        /// </summary>
        private static Type FindAnyType(string simpleName)
        {
            if (string.IsNullOrEmpty(simpleName))
            {
                return null;
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in SafeTypes(asm))
                {
                    if (t.Name == simpleName)
                    {
                        return t;
                    }
                }
            }
            return null;
        }

        private static IEnumerable<Type> SafeTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
            catch (Exception)
            {
                return Enumerable.Empty<Type>();
            }
        }

        private static Vector3 V3(float[] a)
        {
            if (a == null || a.Length < 3)
            {
                return Vector3.zero;
            }
            return new Vector3(a[0], a[1], a[2]);
        }
    }

    internal static class StrFmt
    {
        /// Sequential printf-style substitution.
        ///
        /// This used to be a thin wrapper over string.Format, but every call site here is
        /// written with %s / %d -- which string.Format does not recognise, so it returned the
        /// template untouched and the whole summary logged as literal "%s -> %s". Implemented
        /// explicitly instead of rewriting the call sites to {0}/{1}, because a mis-ordered
        /// {n} fails silently in exactly the same way this bug did.
        public static string Fmt(this string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format) || args == null || args.Length == 0)
            {
                return format;
            }
            var sb = new StringBuilder(format.Length + 32);
            int next = 0;
            bool leftover = false;
            for (int i = 0; i < format.Length; i++)
            {
                if (format[i] == '%' && i + 1 < format.Length &&
                    (format[i + 1] == 's' || format[i + 1] == 'd'))
                {
                    if (next >= args.Length) { leftover = true; }
                    else
                    {
                        sb.Append(System.Convert.ToString(args[next], CultureInfo.InvariantCulture));
                        next++;
                        i++;
                        continue;
                    }
                }
                sb.Append(format[i]);
            }
            // Fail loudly instead of quietly: "a + b".Fmt(...) binds the call to b alone, so the
            // placeholders in a survive and the args vanish. Both shapes below mean the caller is
            // broken, not the data -- exactly the trap that made the rebuild summary unreadable.
            if (next == 0 || leftover)
            {
                sb.Append(" [Fmt: ").Append(next).Append('/')
                  .Append(args.Length).Append(" arg(s) used").Append(leftover ? ", placeholders left over" : "")
                  .Append("]");
            }
            return sb.ToString();
        }
    }
}
