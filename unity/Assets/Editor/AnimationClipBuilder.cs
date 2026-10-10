using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UndeadSlayer.EditorTools
{
    /// <summary>
    /// Builds the 2022 .anim assets that tools/migration/anim_clips/*.json describe.
    ///
    /// Why this exists as an Editor script instead of written YAML
    /// Unity owns the .anim serialization, so a hand-written file can import while silently
    /// dropping curves. Here AnimationClip.SetCurve + AssetDatabase.CreateAsset produce the bytes
    /// and the script then reads them BACK and asserts -- "no exception" is not proof.
    ///
    /// What it asserts per clip
    ///   * every source curve family landed: one channel per axis, with the same keyframe count
    ///     as the source curve, verified through AnimationUtility.GetCurveBindings;
    ///   * rotation fidelity: Unity 4 stored rotation as quaternion curves with quaternion
    ///     slopes, and Animation.SetCurve only takes float channels, so the values become euler
    ///     channels. That conversion can wrap. This samples the BUILT clip at ten points per
    ///     segment and compares against the source quaternion evaluated with Unity's own Hermite
    ///     math, then reports the worst angular error. Without that measurement a wrong
    ///     conversion would look identical to a right one until a character twisted oddly.
    ///   * animation events survived (27 across the whole build, and they drive hit detection).
    ///
    /// What it refuses to fake
    /// 15 float curves across 8 clips are bound to a component through (classID, attribute); the
    /// concrete 2022 type is not something to guess, so they are counted and reported instead of
    /// being attached to a plausible-sounding property.
    /// </summary>
    public static class AnimationClipBuilder
    {
        private const string MenuRoot = "Undead Slayer/";
        private const string ClipDir = "Assets/Imported/Animations";
        // Application.dataPath is <repo>/unity/Assets, so the migration tools live two levels up
        // -- the same resolution SceneRebuilder / MeshRebuilder / MaterialRebuilder use. With
        // only ".." here the index lookup lands in <repo>/unity/tools and the menu reports the
        // committed file as missing.
        private const string RepoRoot = "../../";

        [Serializable] private class Key
        {
            public float t;
            public float[] value;
            public float[] in_slope;
            public float[] out_slope;
        }

        [Serializable] private class BoneCurve
        {
            public string path;
            public Key[] keys;
        }

        [Serializable] private class FloatCurveDesc
        {
            public string path;
            public string attribute;
            public int class_id;
            public Key[] keys;
        }

        [Serializable] private class CurveSet
        {
            public BoneCurve[] position;
            public BoneCurve[] rotation;
            public BoneCurve[] scale;
            public FloatCurveDesc[] float_curves;
        }

        [Serializable] private class Counts
        {
            public int position;
            public int rotation;
            public int scale;
            public int float_curves;
            public int events;
            public int keyframes;
        }

        [Serializable] private class EventDesc
        {
            public float time;
            public string function;
            public string data;
            public float @float;
            public int @int;
            public int options;
        }

        [Serializable] private class Desc
        {
            public string id;
            public string container;
            public string name;
            public string asset_path;
            public int path_id;
            public float sample_rate;
            public float length;
            public int wrap_mode;
            public bool export_legacy;
            public Counts counts;
            public CurveSet curves;
            public EventDesc[] events;
            public string[] unresolved;
        }

        /// <summary>
        /// One animation state an object needs, and where its .anim lives. Mirrors
        /// node.animations written by tools/migration/extract_scene.py.
        /// </summary>
        [Serializable] private class Binding
        {
            public string name;
            public string clip_id;
            public string picked_by;
            public string source_descriptor;
            public string asset;
        }

        [Serializable] private class NodeAnim
        {
            public int id;
            public string name;
            public Binding[] animations;
        }

        [Serializable] private class SceneDoc
        {
            public string scene_name;
            public NodeAnim[] nodes;
        }

        /// <summary>
        /// One row of tools/migration/animation-index.json. Only the fields needed to turn a clip
        /// into an asset path and back to its descriptor.
        /// </summary>
        [Serializable] private class IndexEntry
        {
            public string id;
            public string name;
            public string container;
            public int path_id;
            public string asset_path;
            public string descriptor;
        }

        [Serializable] private class IndexDoc
        {
            public IndexEntry[] clips;
        }

        private class Stat
        {
            public int clips;
            public int failed;
            public int channels;
            public int builtKeys;
            public int sourceKeys;
            public int floatCurvesSkipped;
            public int eventsBuilt;
            public float worstErrorDeg;
            public float worstAtKeyErrorDeg;
            public string worstClip;
            public string worstPath;
        }

        [MenuItem(MenuRoot + "Build AnimationClips From Descriptors")]
        private static void Build()
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, RepoRoot));
            string scenesDir = Path.Combine(root, "tools", "migration", "scenes");
            if (!Directory.Exists(scenesDir))
            {
                // Name the path that was tried: a missing-file report once hid the fact that this
                // script resolved the repo root as .. instead of ../.. and looked in the wrong
                // directory entirely.
                EditorUtility.DisplayDialog("Animation clips", "No scene descriptions at:\n"
                    + scenesDir + "\n\nRun tools/migration/extract_scene.py first.", "OK");
                return;
            }

            // One asset per (state name, source clip). The file name is the state name, because a
            // legacy Animation state IS the clip's object name and CreateAsset pins object name to
            // file name -- 15 clips are called "idle", so a single shared asset cannot serve two
            // names, and the folder carries the clip identity. Measured alternative: AddClip cloned
            // 119 clips into Story.unity, 320 KB -> 47.9 MB, duplicating every curve per scene.
            int sceneFiles = 0;
            var work = new Dictionary<string, Binding>();
            var ambiguous = new List<string>();
            var scenesText = new StringBuilder();
            foreach (string file in Directory.GetFiles(scenesDir, "level*.json"))
            {
                string raw = File.ReadAllText(file);
                scenesText.Append(raw);
                SceneDoc doc = JsonUtility.FromJson<SceneDoc>(raw);
                sceneFiles++;
                if (doc == null || doc.nodes == null)
                {
                    continue;
                }
                foreach (NodeAnim n in doc.nodes)
                {
                    if (n.animations == null)
                    {
                        continue;
                    }
                    foreach (Binding b in n.animations)
                    {
                        if (b.picked_by != null && b.picked_by.StartsWith("lowest-path-id"))
                        {
                            ambiguous.Add(doc.scene_name + "/" + n.name + ":" + b.name);
                        }
                        work[b.asset] = b;
                    }
                }
            }

            // The prefab descriptions reference the same kind of asset, and until now this builder
            // only looked at the 23 scenes -- which is exactly why 6 bindings inside built prefabs
            // logged "clip asset not found": nothing had ever built them. Both feeds count now, the
            // node bindings for their picked_by evidence and the raw text for the scan below.
            string prefabDir = Path.Combine(root, "tools", "migration", "prefabs");
            int prefabDocs = 0;
            if (Directory.Exists(prefabDir))
            {
                foreach (string file in Directory.GetFiles(prefabDir, "*.json"))
                {
                    string raw = File.ReadAllText(file);
                    scenesText.Append(raw);
                    prefabDocs++;
                    SceneDoc doc = JsonUtility.FromJson<SceneDoc>(raw);
                    if (doc == null || doc.nodes == null)
                    {
                        continue;
                    }
                    string label = Path.GetFileNameWithoutExtension(file);
                    foreach (NodeAnim n in doc.nodes)
                    {
                        if (n.animations == null)
                        {
                            continue;
                        }
                        foreach (Binding b in n.animations)
                        {
                            if (b.picked_by != null && b.picked_by.StartsWith("lowest-path-id"))
                            {
                                ambiguous.Add("prefab " + label + ":" + b.name);
                            }
                            work[b.asset] = b;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("[AnimationClipBuilder] no prefab descriptions at " + prefabDir
                          + " -- building only what the scenes reference");
            }

            // A MonoBehaviour field of type AnimationClip (Cha_Control.begrab and friends, 51
            // reference slots in Story and Stage) names the same kind of asset but arrives through
            // components[].fields instead of nodes[].animations, so JsonUtility would have to model
            // nested arrays of a self-referential type to see it -- which it cannot do. Scanning the
            // scene text for the paths this project generates needs no schema and cannot miss a field
            // hidden inside an array or struct value.
            string indexPath = Path.Combine(root, "tools", "migration", "animation-index.json");
            if (!File.Exists(indexPath))
            {
                EditorUtility.DisplayDialog("Animation clips", "No clip index at:\n" + indexPath
                    + "\n\nRun tools/migration/export_animation_descriptors.py first.", "OK");
                return;
            }
            IndexDoc index = JsonUtility.FromJson<IndexDoc>(File.ReadAllText(indexPath));
            if (index == null || index.clips == null || index.clips.Length == 0)
            {
                EditorUtility.DisplayDialog("Animation clips",
                    "animation-index.json parsed to no clips.", "OK");
                return;
            }
            var mentioned = new HashSet<string>();
            foreach (Match m in Regex.Matches(scenesText.ToString(),
                         "\"(Assets/Imported/Animations/[^\"]+\\.anim)\""))
            {
                mentioned.Add(m.Groups[1].Value);
            }
            var known = new HashSet<string>();
            int fromFields = 0;
            foreach (IndexEntry e in index.clips)
            {
                known.Add(e.asset_path);
                if (work.ContainsKey(e.asset_path) || !mentioned.Contains(e.asset_path))
                {
                    continue;
                }
                work[e.asset_path] = new Binding
                {
                    name = e.name,
                    clip_id = e.id,
                    picked_by = "referenced by a MonoBehaviour field",
                    source_descriptor = e.descriptor,
                    asset = e.asset_path,
                };
                fromFields++;
            }
            var orphanMentions = new List<string>();
            foreach (string p in mentioned)
            {
                if (!known.Contains(p))
                {
                    orphanMentions.Add(p);
                }
            }
            EnsureFolder();

            var stat = new Stat();
            var problems = new List<string>();
            int done = 0;
            int total = work.Count;

            foreach (Binding binding in work.Values)
            {
                done++;
                string descPath = Path.Combine(root,
                    binding.source_descriptor.Replace('/', Path.DirectorySeparatorChar));
                if (EditorUtility.DisplayCancelableProgressBar("Building animation clips",
                        done + "/" + total + "  " + binding.asset, (float)done / total))
                {
                    break;
                }
                if (!File.Exists(descPath))
                {
                    stat.failed++;
                    problems.Add(binding.asset + ": descriptor absent: " + binding.source_descriptor);
                    continue;
                }

                Desc d = JsonUtility.FromJson<Desc>(File.ReadAllText(descPath));
                AnimationClip clip = BuildClip(d, stat, problems);
                if (clip == null)
                {
                    stat.failed++;
                    continue;
                }

                string assetPath = binding.asset;
                EnsureFolderFor(assetPath);
                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath) != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
                AssetDatabase.CreateAsset(clip, assetPath);

                // Post-condition on the FILE, not on the object still in memory: re-read it and
                // confirm the channel and keyframe counts survived serialisation.
                AssetDatabase.SaveAssets();
                var back = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                if (back == null)
                {
                    stat.failed++;
                    problems.Add(d.id + ": asset not loadable after CreateAsset");
                    continue;
                }
                // Count the RUNTIME bindings only. SetCurve writes the runtime curve and Unity
                // mirrors it into the editor copy, so summing both would report double the
                // channels and the assertion below would fail on a correct clip.
                int bindings = AnimationUtility.GetCurveBindings(back).Length;
                int keys = 0;
                foreach (var b in AnimationUtility.GetCurveBindings(back))
                {
                    AnimationCurve cur = AnimationUtility.GetEditorCurve(back, b);
                    keys += cur == null ? 0 : cur.length;
                }
                // Each source position/rotation/scale curve becomes exactly 3 float channels; the
                // skipped float curves are not expected to appear. A shortfall means SetCurve was
                // given a property name this Unity does not accept and dropped it.
                int expectedChannels = 3 * (d.counts.position + d.counts.rotation + d.counts.scale);
                if (bindings != expectedChannels)
                {
                    stat.failed++;
                    problems.Add(string.Format("{0}: channels {1} but expected {2} (property "
                        + "names rejected?)", d.id, bindings, expectedChannels));
                }
                if (back.name != binding.name)
                {
                    // A real failure now, not a curiosity: the state name a script plays IS this
                    // object's name, and the file is named after it on purpose. If Unity reports a
                    // different name, Play(binding.name) will not resolve.
                    stat.failed++;
                    problems.Add(string.Format("{0}: object name '{1}' but the state is '{2}'",
                        binding.asset, back.name, binding.name));
                }
                if (back.events.Length != d.counts.events)
                {
                    stat.failed++;
                    problems.Add(string.Format("{0}: events survived {1} of {2}", d.id,
                        back.events.Length, d.counts.events));
                }
                stat.sourceKeys += d.counts.keyframes;
                stat.builtKeys += keys;
                stat.channels += bindings;
                stat.clips++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            var sb = new StringBuilder();
            sb.AppendLine("===== ANIMATION CLIP BUILD =====");
            sb.AppendLine(string.Format("clips built        : {0}  ({1} (state,clip) assets "
                + "requested), failed {2}", stat.clips, total, stat.failed));
            sb.AppendLine(string.Format("requested by       : {0} Animation state bindings + {1} "
                + "MonoBehaviour field references; the index lists {2} clips and only what a scene "
                + "actually mentions is built", total - fromFields, fromFields, index.clips.Length));
            if (orphanMentions.Count > 0)
            {
                // A scene names a clip path the index does not have. That is the two-path-naming bug
                // coming back (extract_scene and the index once disagreed on all 427), so it is an
                // error rather than a footnote.
                sb.AppendLine(string.Format("!! {0} referenced clip path(s) are not in "
                    + "animation-index.json -- the path rule has forked again: {1}",
                    orphanMentions.Count, string.Join(", ", orphanMentions.GetRange(
                        0, Mathf.Min(3, orphanMentions.Count)).ToArray())));
            }
            sb.AppendLine(string.Format("float channels     : {0}", stat.channels));
            sb.AppendLine(string.Format("curve keys         : built {0} for {1} source vector keys"
                + "  (each vector/quaternion curve becomes 3-4 float channels)", stat.builtKeys,
                stat.sourceKeys));
            sb.AppendLine(string.Format("animation events   : {0} persisted via AnimationUtility",
                stat.eventsBuilt));
            sb.AppendLine(string.Format("descriptions scanned  : {0} scene file(s) + {1} prefab file(s)",
                sceneFiles, prefabDocs));
            sb.AppendLine(string.Format("float curves skipped: {0} (classID+attribute, not guessed)",
                stat.floatCurvesSkipped));
            sb.AppendLine(string.Format("ambiguous bindings  : {0} (same state name, several "
                + "candidate clips -- picked by lowest path_id, NOT proven)", ambiguous.Count));
            foreach (string a in ambiguous)
            {
                sb.AppendLine("   ambiguous: " + a);
            }
            sb.AppendLine(string.Format("rotation error AT KEYS   : {0:0.000} deg  <- assumption "
                + "free, this is the euler round trip", stat.worstAtKeyErrorDeg));
            sb.AppendLine(string.Format("rotation error BETWEEN   : {0:0.000} deg  [{1} / {2}] "
                + "<- vs this file's Hermite model of Unity 4, not ground truth",
                stat.worstErrorDeg, stat.worstClip, stat.worstPath));
            if (stat.worstAtKeyErrorDeg > 2f)
            {
                sb.AppendLine("  -- the quaternion->euler conversion itself is wrong at the keys.");
                sb.AppendLine("     Do not use these clips for gameplay until that is fixed.");
            }
            if (problems.Count > 0)
            {
                sb.AppendLine("problems (" + problems.Count + "):");
                foreach (string p in problems)
                {
                    sb.AppendLine("  " + p);
                }
            }
            string report = sb.ToString();
            File.WriteAllText(Path.Combine(root, "tools", "migration", "animation-build-report.txt"),
                report);
            Debug.Log(report);
            EditorUtility.DisplayDialog("Animation clips",
                stat.clips + " clips built, " + stat.failed + " failed\n" +
                "worst rotation error " + stat.worstErrorDeg.ToString("0.00", CultureInfo.InvariantCulture) +
                " deg\nReport: tools/migration/animation-build-report.txt",
                "OK");
        }

        private static AnimationClip BuildClip(Desc d, Stat stat, List<string> problems)
        {
            var clip = new AnimationClip
            {
                name = d.name,
                frameRate = d.sample_rate,
                legacy = d.export_legacy,
                wrapMode = (WrapMode)d.wrap_mode
            };

            if (d.curves == null)
            {
                problems.Add(d.id + ": descriptor has no curve set");
                return null;
            }

            foreach (BoneCurve c in d.curves.position ?? new BoneCurve[0])
            {
                SetVectorChannels(clip, c, "m_LocalPosition", d, 3, stat, problems);
            }
            foreach (BoneCurve c in d.curves.scale ?? new BoneCurve[0])
            {
                SetVectorChannels(clip, c, "m_LocalScale", d, 3, stat, problems);
            }
            foreach (BoneCurve c in d.curves.rotation ?? new BoneCurve[0])
            {
                SetRotationChannels(clip, c, d, stat, problems);
            }

            stat.floatCurvesSkipped += (d.curves.float_curves ?? new FloatCurveDesc[0]).Length;

            var ev = new List<AnimationEvent>();
            foreach (EventDesc e in d.events ?? new EventDesc[0])
            {
                ev.Add(new AnimationEvent
                {
                    time = e.time,
                    functionName = e.function,
                    stringParameter = e.data,
                    floatParameter = e.@float,
                    intParameter = e.@int,
                    messageOptions = (SendMessageOptions)e.options
                });
            }
            if (ev.Count > 0)
            {
                // AnimationClip.events is a getter on a clip that is not yet an asset; setting it
                // logs "Please use Editor.AnimationUtility to add persistent animation events" and
                // drops them. AnimationUtility.SetAnimationEvents is the API that persists them.
                AnimationUtility.SetAnimationEvents(clip, ev.ToArray());
                stat.eventsBuilt += ev.Count;
            }
            return clip;
        }

        private static void SetVectorChannels(AnimationClip clip, BoneCurve c, string property,
            Desc d, int components, Stat stat, List<string> problems)
        {
            for (int axis = 0; axis < components; axis++)
            {
                var curve = new AnimationCurve();
                foreach (Key k in c.keys)
                {
                    if (k.value == null || k.value.Length <= axis)
                    {
                        problems.Add(d.id + ": " + property + " key missing component " + axis);
                        return;
                    }
                    curve.AddKey(new Keyframe(k.t, k.value[axis], Slope(k, axis), Slope(k, axis)));
                }
                clip.SetCurve(c.path, typeof(Transform), property + "." + AxisName(axis), curve);
            }
        }

        private static void SetRotationChannels(AnimationClip clip, BoneCurve c, Desc d,
            Stat stat, List<string> problems)
        {
            if (c.keys == null || c.keys.Length == 0)
            {
                return;
            }

            // Unity's Quaternion.eulerAngles returns 0..360, which jumps across a segment and
            // makes the interpolated path spin the long way round. Shift each axis by whole turns
            // to stay closest to the previous key -- Quaternion.Euler is periodic in 360, so the
            // rotation at the keys stays exact while the curve between them stays continuous.
            var srcQuat = new Quaternion[c.keys.Length];
            var eulers = new Vector3[c.keys.Length];
            for (int i = 0; i < c.keys.Length; i++)
            {
                float[] v = c.keys[i].value;
                if (v == null || v.Length < 4)
                {
                    problems.Add(d.id + ": rotation key without 4 components on " + c.path);
                    return;
                }
                srcQuat[i] = new Quaternion(v[0], v[1], v[2], v[3]);
                Vector3 e = srcQuat[i].eulerAngles;
                if (i > 0)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        e[a] = NearestTo(eulers[i - 1][a], e[a]);
                    }
                }
                eulers[i] = e;
            }

            var ch = new[] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve() };
            for (int i = 0; i < c.keys.Length; i++)
            {
                Key k = c.keys[i];
                Vector3 rate = AngularRateDeg(k.in_slope, k.out_slope, srcQuat[i]);
                for (int a = 0; a < 3; a++)
                {
                    ch[a].AddKey(new Keyframe(k.t, eulers[i][a], rate[a], rate[a]));
                }
            }

            for (int a = 0; a < 3; a++)
            {
                clip.SetCurve(c.path, typeof(Transform),
                    "localEulerAnglesRaw." + AxisName(a), ch[a]);
            }

            MeasureRotation(clip, c, d, stat, problems);
        }

        /// <summary>
        /// Compare the built euler channels against the source quaternion curves, evaluated with
        /// the same cubic Hermite math Unity uses, at ten points per segment.
        /// </summary>
        private static void MeasureRotation(AnimationClip clip, BoneCurve c,
            Desc d, Stat stat, List<string> problems)
        {
            if (c.keys == null || c.keys.Length < 2)
            {
                return;
            }
            var curves = new AnimationCurve[3];
            for (int a = 0; a < 3; a++)
            {
                curves[a] = AnimationUtility.GetEditorCurve(clip, c.path, typeof(Transform),
                    "localEulerAnglesRaw." + AxisName(a));
                if (curves[a] == null || curves[a].length == 0)
                {
                    problems.Add(d.id + ": rotation channel " + a + " unreadable on " + c.path);
                    return;
                }
            }

            float maxBetween = 0f;
            float maxAtKey = 0f;
            for (int i = 0; i < c.keys.Length; i++)
            {
                // AT A KEY the source value is the key itself, so any error here is purely the
                // quaternion -> euler -> quaternion round trip. This is the assumption-free
                // number.
                Vector3 ek = new Vector3(curves[0].Evaluate(c.keys[i].t),
                    curves[1].Evaluate(c.keys[i].t), curves[2].Evaluate(c.keys[i].t));
                float errKey = Quaternion.Angle(Quaternion.Euler(ek), ToQuatSafe(c.keys[i].value));
                if (errKey > maxAtKey)
                {
                    maxAtKey = errKey;
                }
            }
            for (int i = 0; i < c.keys.Length - 1; i++)
            {
                Key a = c.keys[i], b = c.keys[i + 1];
                for (int s = 1; s < 10; s++)
                {
                    float u = s / 10f;
                    float t = a.t + (b.t - a.t) * u;
                    Vector3 e = new Vector3(curves[0].Evaluate(t), curves[1].Evaluate(t),
                        curves[2].Evaluate(t));
                    Quaternion built = Quaternion.Euler(e);
                    Quaternion want = HermiteQuat(a, b, u);
                    float err = Quaternion.Angle(built, want);
                    if (err > maxBetween)
                    {
                        maxBetween = err;
                    }
                }
            }
            // The between-keys figure compares against THIS FILE's model of how Unity 4
            // interpolated a quaternion curve (component-wise cubic Hermite, renormalised). That
            // model is an assumption -- Unity's legacy animation may have slerped instead -- so a
            // large value here says "my model and the built curve disagree", not "the clip is
            // wrong". The at-keys figure has no such dependency and is the one that gates use.
            if (maxBetween > stat.worstErrorDeg)
            {
                stat.worstErrorDeg = maxBetween;
                stat.worstClip = d.id;
                stat.worstPath = c.path;
            }
            if (maxAtKey > stat.worstAtKeyErrorDeg)
            {
                stat.worstAtKeyErrorDeg = maxAtKey;
            }
        }

        private static Quaternion HermiteQuat(Key a, Key b, float u)
        {
            // Cubic Hermite on the segment, per quaternion component.
            float dt = b.t - a.t;
            if (dt <= 0f)
            {
                return ToQuatSafe(a.value);
            }
            var res = new float[4];
            for (int i = 0; i < 4; i++)
            {
                float p0 = Comp(a.value, i), p1 = Comp(b.value, i);
                float m0 = Comp(a.out_slope, i) * dt, m1 = Comp(b.in_slope, i) * dt;
                float h00 = (1 + 2 * u) * (1 - u) * (1 - u);
                float h10 = u * (1 - u) * (1 - u);
                float h01 = u * u * (3 - 2 * u);
                float h11 = u * u * (u - 1);
                res[i] = h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
            }
            var r = new Quaternion(res[0], res[1], res[2], res[3]);
            return r.normalized;
        }

        /// <summary>
        /// dq/dt = 1/2 * omega * q  =>  omega = 2 * dq/dt * q^-1.  The vector part of omega is the
        /// angular velocity, so an euler channel's tangent is that axis rate in degrees/second.
        /// Averaging the in and out slopes keeps a key whose two slopes disagree (a broken key in
        /// the source) from producing an infinite-looking handle.
        /// </summary>
        private static Vector3 AngularRateDeg(float[] inSlope, float[] outSlope, Quaternion q)
        {
            var dq = new Quaternion(
                Mid(inSlope, outSlope, 0), Mid(inSlope, outSlope, 1),
                Mid(inSlope, outSlope, 2), Mid(inSlope, outSlope, 3));
            Quaternion prod = dq * Quaternion.Inverse(q);
            // Quaternion has no * float operator in 2022, so scale the vector part directly.
            return new Vector3(prod.x, prod.y, prod.z) * (2f * Mathf.Rad2Deg);
        }

        private static float Mid(float[] a, float[] b, int i)
        {
            float x = a == null ? 0f : Comp(a, i);
            float y = b == null ? 0f : Comp(b, i);
            return (x + y) * 0.5f;
        }

        private static Quaternion ToQuatSafe(float[] v)
        {
            return v != null && v.Length >= 4 ? new Quaternion(v[0], v[1], v[2], v[3])
                                              : Quaternion.identity;
        }

        private static float Comp(float[] v, int i)
        {
            return v != null && v.Length > i ? v[i] : 0f;
        }

        private static float Slope(Key k, int axis)
        {
            float a = Comp(k.in_slope, axis), b = Comp(k.out_slope, axis);
            return float.IsNaN(a) || float.IsNaN(b) ? 0f : (a + b) * 0.5f;
        }

        private static float NearestTo(float target, float candidate)
        {
            float best = candidate;
            float bestDist = Mathf.Abs(candidate - target);
            for (int turns = -2; turns <= 2; turns++)
            {
                float alt = candidate + 360f * turns;
                float dist = Mathf.Abs(alt - target);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = alt;
                }
            }
            return best;
        }

        private static string AxisName(int i)
        {
            return i == 0 ? "x" : (i == 1 ? "y" : "z");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Imported"))
            {
                AssetDatabase.CreateFolder("Assets", "Imported");
            }
            if (!AssetDatabase.IsValidFolder(ClipDir))
            {
                AssetDatabase.CreateFolder("Assets/Imported", "Animations");
            }
        }

        /// <summary>
        /// Create every folder a per-consumer asset path needs. AssetDatabase.CreateAsset does NOT
        /// create intermediate folders, and these paths are now nested two deep
        /// (Animations/&lt;scene&gt;/&lt;object&gt;/&lt;state&gt;.anim).
        /// </summary>
        private static void EnsureFolderFor(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length - 1; i++)
            {
                string child = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(child))
                {
                    AssetDatabase.CreateFolder(cur, parts[i]);
                }
                cur = child;
            }
        }
    }
}
