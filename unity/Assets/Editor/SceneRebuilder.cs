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
            // kind == MonoBehaviour: decoded field values (see tools/migration/monobehaviour_fields.py).
            // Absent when the class's layout could not be proven, which is deliberate -- the
            // decoder refuses a class rather than half-fill it.
            public FieldVal[] fields;
        }

        /// <summary>
        /// One decoded MonoBehaviour field. Flat rather than typed because JsonUtility cannot
        /// deserialise a union or a dictionary, so kind selects which member is meaningful.
        /// </summary>
        [Serializable]
        private class FieldVal
        {
            public string n;         // field name
            public string kind;      // node | int | float | bool | vec | str | asset | arr | obj | opaque
            public string t;         // declared type name (Transform, GameObject, Vector3, ...)
            public int node;         // kind == node: source object id, resolved to a scene node
            public int i;
            public float f;
            public int b;
            public float[] v;
            public string s;         // kind == str
            public string asset;     // kind == asset: project-relative path
            public FieldItem[] items;  // kind == arr: one entry per element
        }

        /// <summary>
        /// One element of an array field. A separate type rather than a recursive FieldVal because
        /// JsonUtility binds by exact field name and does not do recursion; the element shapes seen
        /// in this build are asset / int / float / node, which is all this covers.
        /// </summary>
        [Serializable]
        private class FieldItem
        {
            public string kind;
            public string t;
            public int node;
            public int i;
            public float f;
            public int b;
            public float[] v;
            public string asset;
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
            public AnimBinding[] animations;
        }

        /// <summary>
        /// One animation state an object's own scripts will ask for, and which .anim asset was
        /// chosen for it. tools/migration/extract_scene.py derives this from the scripts, because
        /// the APK carries no AnimationContainer and every Animation.m_Clip is null.
        /// </summary>
        [Serializable]
        private class AnimBinding
        {
            public string name;
            public string asset;
            public string clip_id;
            public string picked_by;
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
            var fieldJobs = new List<FieldJob>();
            int fieldsRefs = 0, fieldsVals = 0, fieldsMissing = 0, fieldsLanded = 0, fieldsDropped = 0;
            int fieldsNotApplied = 0, fieldsArrayElems = 0, fieldsArrayPartial = 0;
            int fieldsLandedArray = 0;

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
                            // Two different failures, deliberately different severities. FindType
                            // filters on IsAssignableFrom(MonoBehaviour), so a null means either
                            // "no such class at all" (our corpus is incomplete -> error) or "the
                            // class exists but is not a component" (a proven property of the shipped
                            // data -> warning). The latter happened once in 394 bindings: PayMgr
                            // binds to ChannelMgr, whose MonoScript names class ChannelMgr in
                            // Assembly-CSharp.dll, and that class has no base type -- verified with
                            // ilspycmd on the shipped DLL, and Assembly-CSharp-firstpass.dll has no
                            // ChannelMgr at all, so there is no second same-named MonoBehaviour.
                            // Keeping it as an error only buried the real ones under repeat noise.
                            var nonMono = FindAnyType(c.script);
                            if (nonMono != null)
                            {
                                Debug.LogWarning(
                                    "[SceneRebuilder] %s: script type '%s' EXISTS but is not a MonoBehaviour (%s) -- cannot be attached; this is a property of the original build data, not a lost file"
                                    .Fmt(n.name, c.script, c.cs));
                            }
                            else
                            {
                                Debug.LogError(
                                    "[SceneRebuilder] %s: script type '%s' not found (%s)"
                                    .Fmt(n.name, c.script, c.cs));
                            }
                            continue;
                        }
                        if (go.GetComponent(type) != null)
                        {
                            continue; // same class twice on one GameObject is not allowed
                        }
                        var mb = go.AddComponent(type);
                        ((MonoBehaviour)mb).enabled = c.enabled;
                        addedScripts++;
                        if (c.fields != null && c.fields.Length > 0)
                        {
                            fieldJobs.Add(new FieldJob { Mb = (MonoBehaviour)mb, Comp = c });
                        }
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

            // Decoded MonoBehaviour field values, applied through SerializedObject so Unity
            // validates the property and serialises it -- same reason the whole tool exists.
            // Deferred for the same reason as bones: references point at other scene objects.
            foreach (var job in fieldJobs)
            {
                var so = new SerializedObject(job.Mb);
                // Field names whose reference values must still be non-null after Apply. Scoped to
                // this one component: the first cut kept them in a list shared by every job, which
                // would have charged component B for component A's dropped reference.
                var wantLanded = new List<string>();
                bool dirty = false;
                foreach (var fv in job.Comp.fields)
                {
                    var prop = so.FindProperty(fv.n);
                    if (prop == null)
                    {
                        fieldsMissing++;
                        Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                            ": no such serialised property on " + job.Mb.GetType().Name);
                        continue;
                    }
                    if (fv.kind == "node")
                    {
                        GameObject target;
                        if (!byId.TryGetValue(fv.node, out target))
                        {
                            fieldsMissing++;
                            Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                                ": source object " + fv.node + " is not in this scene");
                            continue;
                        }
                        // Decide what to hand over from the field's DECLARED CLR TYPE. The first
                        // cut here matched SerializedProperty.type against the strings "Transform"
                        // and "GameObject"; that is Unity's own naming, and trusting it is why
                        // every reference came back null while int/float/bool applied fine.
                        var fi = FindFieldType(job.Mb.GetType(), fv.n);
                        var ft = fi == null ? null : fi.FieldType;
                        object value = null;
                        if (ft != null)
                        {
                            if (ft.IsAssignableFrom(typeof(Transform))) value = target.transform;
                            else if (ft.IsAssignableFrom(typeof(GameObject))) value = target;
                            else if (typeof(Component).IsAssignableFrom(ft)) value = target.GetComponent(ft);
                        }
                        if (value == null)
                        {
                            fieldsMissing++;
                            Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                                ": cannot resolve on " + target.name +
                                " (field type=" + (ft == null ? "<none>" : ft.Name) +
                                ", prop.type=" + prop.type + ", prop.class=" + prop.propertyType +
                                ") -- left unset");
                            continue;
                        }
                        prop.objectReferenceValue = (UnityEngine.Object)value;
                        fieldsRefs++;
                    }
                    else if (fv.kind == "int") { prop.intValue = fv.i; fieldsVals++; }
                    else if (fv.kind == "float") { prop.floatValue = fv.f; fieldsVals++; }
                    else if (fv.kind == "bool") { prop.boolValue = fv.b != 0; fieldsVals++; }
                    else if (fv.kind == "vec" && fv.v != null)
                    {
                        if (prop.propertyType == SerializedPropertyType.Vector2 && fv.v.Length >= 2)
                            prop.vector2Value = new Vector2(fv.v[0], fv.v[1]);
                        else if (prop.propertyType == SerializedPropertyType.Vector3 && fv.v.Length >= 3)
                            prop.vector3Value = new Vector3(fv.v[0], fv.v[1], fv.v[2]);
                        else if (prop.propertyType == SerializedPropertyType.Vector4 && fv.v.Length >= 4)
                            prop.vector4Value = new Vector4(fv.v[0], fv.v[1], fv.v[2], fv.v[3]);
                        else if (prop.propertyType == SerializedPropertyType.Color && fv.v.Length >= 4)
                            prop.colorValue = new Color(fv.v[0], fv.v[1], fv.v[2], fv.v[3]);
                        else { fieldsMissing++; continue; }
                        fieldsVals++;
                    }
                    else if (fv.kind == "str")
                    {
                        if (prop.propertyType != SerializedPropertyType.String)
                        {
                            fieldsMissing++;
                            Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                                ": JSON says string, property is " + prop.propertyType);
                            continue;
                        }
                        prop.stringValue = fv.s;
                        fieldsVals++;
                    }
                    else if (fv.kind == "asset")
                    {
                        var a = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fv.asset);
                        if (a == null)
                        {
                            fieldsMissing++;
                            Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                                ": asset '" + fv.asset + "' is not in the project");
                            continue;
                        }
                        prop.objectReferenceValue = a;
                        fieldsRefs++;
                        wantLanded.Add(fv.n);
                    }
                    else if (fv.kind == "arr" && fv.items != null)
                    {
                        int landed = 0;
                        prop.arraySize = fv.items.Length;
                        for (int ei = 0; ei < fv.items.Length; ei++)
                        {
                            var item = fv.items[ei];
                            var ep = prop.GetArrayElementAtIndex(ei);
                            if (ep == null) continue;
                            if (item.kind == "asset")
                            {
                                var a = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.asset);
                                if (a == null) continue;
                                ep.objectReferenceValue = a;
                                landed++;
                            }
                            else if (item.kind == "node")
                            {
                                GameObject target;
                                if (!byId.TryGetValue(item.node, out target)) continue;
                                // An array element's declared element type is not reachable from
                                // the SerializedProperty, so hand over the GameObject unless the
                                // property says it wants a component.
                                if (ep.propertyType == SerializedPropertyType.ObjectReference &&
                                    ep.type == "GameObject") ep.objectReferenceValue = target;
                                else ep.objectReferenceValue = target.transform;
                                landed++;
                            }
                            else if (item.kind == "int") { ep.intValue = item.i; landed++; }
                            else if (item.kind == "float") { ep.floatValue = item.f; landed++; }
                            else if (item.kind == "bool") { ep.boolValue = item.b != 0; landed++; }
                            else if (item.kind == "vec" && item.v != null
                                     && ep.propertyType == SerializedPropertyType.Vector3
                                     && item.v.Length >= 3)
                            {
                                ep.vector3Value = new Vector3(item.v[0], item.v[1], item.v[2]);
                                landed++;
                            }
                        }
                        if (landed == 0)
                        {
                            fieldsMissing++;
                            Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                                ": array of " + fv.items.Length + " applied nothing");
                            continue;
                        }
                        fieldsArrayElems += landed;
                        if (landed != fv.items.Length)
                        {
                            fieldsArrayPartial++;
                            Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                                ": only " + landed + " of " + fv.items.Length +
                                " array elements applied");
                        }
                        wantLanded.Add(fv.n);
                    }
                    else if (fv.kind == "obj" || fv.kind == "opaque")
                    {
                        // Decoded and present in the JSON, but this pass cannot apply them yet.
                        // Counted separately so "refused" keeps meaning "something went wrong"
                        // instead of absorbing work that is simply not implemented.
                        fieldsNotApplied++;
                        continue;
                    }
                    else { fieldsMissing++; continue; }
                    dirty = true;
                }
                if (!dirty) continue;
                so.ApplyModifiedPropertiesWithoutUndo();
                // Post-condition. Setting objectReferenceValue on a property can be accepted and
                // then discarded at apply time (wrong type, object not yet serialisable), and the
                // first version of this pass reported "applied" from the assignment alone -- which
                // is how five Cam_intro references came back as {fileID: 0} in the written file
                // while the summary claimed they were set. Re-read and count what actually stuck.
                so.Update();
                foreach (var fv in job.Comp.fields)
                {
                    bool wantRef = fv.kind == "node" || fv.kind == "asset";
                    if (!wantRef && fv.kind != "arr") continue;
                    var p2 = so.FindProperty(fv.n);
                    if (p2 == null)
                    {
                        fieldsDropped++;
                        continue;
                    }
                    if (fv.kind == "arr")
                    {
                        // An array of references counts per element; a scalar counts once.
                        int expected = fv.items == null ? 0 : fv.items.Length;
                        int got = 0;
                        for (int ei = 0; ei < Math.Min(expected, p2.arraySize); ei++)
                        {
                            var ep = p2.GetArrayElementAtIndex(ei);
                            if (ep == null) continue;
                            if (ep.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                if (ep.objectReferenceValue != null) got++;
                            }
                            else if (ep.propertyType == SerializedPropertyType.Integer
                                     || ep.propertyType == SerializedPropertyType.Float
                                     || ep.propertyType == SerializedPropertyType.Boolean)
                            {
                                got++;
                            }
                        }
                        fieldsLandedArray += got;
                        if (got < expected)
                        {
                            fieldsDropped++;
                            Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                                ": array holds " + got + " of " + expected +
                                " values after apply");
                        }
                        continue;
                    }
                    if (p2.objectReferenceValue != null) fieldsLanded++;
                    else
                    {
                        fieldsDropped++;
                        Debug.LogWarning("[SceneRebuilder] " + job.Mb.name + "." + fv.n +
                            ": assignment did not survive apply");
                    }
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

            // Register the animation states the object's own scripts play, by assigning the
            // Animation component's clip list through SerializedObject. NOT Animation.AddClip:
            // AddClip renames the clip to the state name, and since a clip asset's object name is
            // fixed to its file name, Unity resolved the conflict by CLONING each clip into the
            // scene -- measured at 119 embedded copies and 47.9 MB for Story.unity, against 320 KB
            // before. The assets are named after their state now (one folder per consumer), so a
            // plain reference already carries the right name and nothing has to be renamed.
            int animStates = 0, animObjects = 0, animMissing = 0, animAmbiguous = 0;
            var clipGuids = new HashSet<string>();
            foreach (var n in data.nodes)
            {
                if (n.animations == null || n.animations.Length == 0)
                {
                    continue;
                }
                GameObject goNode;
                if (!byId.TryGetValue(n.id, out goNode))
                {
                    continue;
                }
                var anim = goNode.GetComponent<Animation>();
                if (anim == null)
                {
                    animMissing++;
                    Debug.LogWarning(
                        "[SceneRebuilder] %s has %d animation states to register but no "
                        + "Animation component".Fmt(n.name, n.animations.Length));
                    continue;
                }
                var clips = new List<AnimationClip>();
                foreach (var ab in n.animations)
                {
                    if (ab.picked_by != null && ab.picked_by.StartsWith("lowest-path-id"))
                    {
                        animAmbiguous++;
                    }
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ab.asset);
                    if (clip == null)
                    {
                        animMissing++;
                        Debug.LogWarning(
                            "[SceneRebuilder] %s: clip asset '%s' not found -- run 'Build "
                            + "AnimationClips From Descriptors' first".Fmt(n.name, ab.asset));
                        continue;
                    }
                    if (clip.name != ab.name)
                    {
                        // The state name IS the clip object name for legacy Animation, so a
                        // mismatch means Play(ab.name) would fail even though the reference is set.
                        animMissing++;
                        Debug.LogWarning(
                            "[SceneRebuilder] %s: '%s' object name is '%s' -- Play(\"%s\") would "
                            + "not resolve".Fmt(n.name, ab.asset, clip.name, ab.name));
                        continue;
                    }
                    clips.Add(clip);
                    clipGuids.Add(AssetDatabase.AssetPathToGUID(ab.asset));
                }
                if (clips.Count == 0)
                {
                    continue;
                }
                var animSo = new SerializedObject(anim);
                var list = animSo.FindProperty("m_Animations");
                if (list == null)
                {
                    Debug.LogWarning("[SceneRebuilder] Animation.m_Animations not serialised in "
                        + "this Unity version -- states cannot be bound");
                    continue;
                }
                list.arraySize = clips.Count;
                for (int i = 0; i < clips.Count; i++)
                {
                    list.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
                }
                var def = animSo.FindProperty("m_Animation");
                if (def != null)
                {
                    def.objectReferenceValue = clips[0];
                }
                animSo.ApplyModifiedPropertiesWithoutUndo();
                animSo.Update();
                int landed = 0;
                var check = animSo.FindProperty("m_Animations");
                for (int i = 0; i < check.arraySize; i++)
                {
                    if (check.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    {
                        landed++;
                    }
                }
                if (landed != clips.Count)
                {
                    Debug.LogWarning(
                        "[SceneRebuilder] %s: %d of %d clip references did not land on the "
                        + "Animation component".Fmt(n.name, clips.Count - landed, clips.Count));
                }
                animStates += landed;
                animObjects++;
            }

            EnsureFolderFor(data.scene_path);
            if (!EditorSceneManager.SaveScene(scene, data.scene_path))
            {
                Debug.LogError("[SceneRebuilder] SaveScene failed for " + data.scene_path);
                return;
            }

            // "AddClip succeeded" only proves the in-memory component. Whether the clip list
            // actually serialised into the scene file is a separate question, so count the clip
            // guids in the bytes that were just written.
            int guidsInFile = 0;
            if (clipGuids.Count > 0)
            {
                string saved = Path.Combine(Application.dataPath, "..", data.scene_path);
                string sceneText = File.Exists(saved) ? File.ReadAllText(saved) : "";
                foreach (string g in clipGuids)
                {
                    if (g.Length > 0 && sceneText.Contains(g))
                    {
                        guidsInFile++;
                    }
                }
                if (guidsInFile == 0)
                {
                    Debug.LogWarning(
                        "[SceneRebuilder] %d clip(s) were AddClip'd but NO clip guid appears in "
                        + "the saved scene file -- Animation's clip list did not persist."
                        .Fmt(clipGuids.Count));
                }
            }

            Debug.Log(
                "[SceneRebuilder] %s -> %s\n  GameObjects : %d\n  scripts     : %d added, %d unresolved type\n  components  : %d added, %d unresolvable\n  assets      : %d meshes, %d materials, %d missing\n  fields      : %d refs assigned + %d values applied, %d refused, %d landed in the file, %d did not survive, %d decoded but not applicable yet\n  arrays      : %d elements written, %d landed in the file, %d arrays partial\n  animations  : %d states on %d objects, %d/%d clip guids in the saved file, %d unresolved asset, %d picked by lowest-path-id\n  dropped     : %s\n  NOT restored: AudioClip references (not exported yet), scene settings (Unity defaults), and every MonoBehaviour class whose field layout could not be proven byte-exactly"
                .Fmt(data.source_level, data.scene_path, data.nodes.Length,
                     addedScripts, missingScripts, addedComps, unknownComps,
                     meshesSet, matsSet, missingAssets,
                     fieldsRefs, fieldsVals, fieldsMissing, fieldsLanded, fieldsDropped,
                     fieldsNotApplied, fieldsArrayElems, fieldsLandedArray, fieldsArrayPartial,
                     animStates, animObjects, guidsInFile, clipGuids.Count, animMissing,
                     animAmbiguous,
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

        /// <summary>
        /// Declared type of a serialisable field, searching base classes. Unity's default
        /// BindingFlags do not walk the hierarchy for instance fields, so a field inherited from a
        /// parent class reads as missing unless DeclaredOnly is stepped up by hand.
        /// </summary>
        private static FieldInfo FindFieldType(Type t, string name)
        {
            for (var c = t; c != null && c != typeof(object); c = c.BaseType)
            {
                var f = c.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (f != null) return f;
            }
            return null;
        }

        private class FieldJob
        {
            public MonoBehaviour Mb;
            public Comp Comp;
        }

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
