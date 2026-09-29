using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UndeadSlayer.Migration
{
    /// Builds the one scene the APK does not contain: Assets/Scene/Ready.unity (build index 0).
    ///
    /// This is a HAND-MADE STAND-IN, not recovered data. dump/assets only carries level0..level22,
    /// which map to build indices 1..23 (dump levelN == levels[N+1]), so index 0 has nothing to
    /// rebuild from. It still has to exist, because it is where the shipped game seeds the save
    /// decryption key: Crypto.Init() is called from UI_Ready.Start (UI_Ready.cs:48) and nowhere
    /// else, and Crypto.KEY2 is only ever assigned inside Init(). Without it KEY2 stays empty, the
    /// catch-fallback in Load_int_key (Crypto.cs:101) builds an 8-byte Rijndael key, and
    /// set_Key (Crypto.cs:150) throws out of the Awake/Start of ~24 scene scripts.
    ///
    /// AndroidScript must live in the SAME scene as UI_Ready: AndroidScript.i is assigned in its
    /// own Awake, Crypto.Init() dereferences it, and nothing anywhere AddComponent's it.
    ///
    /// Field values UI_Ready would have had (the per-channel splash textures and basicSkin) are
    /// unknowable, so they stay null; OnGUI then simply draws no splash image.
    public static class ReadySceneBuilder
    {
        private const string ScenePath = "Assets/Scene/Ready.unity";

        [MenuItem("Undead Slayer/Build Stand-in Ready Scene (NOT recovered data)")]
        public static void Build()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Ready.unity already exists",
                    "This tool writes a hand-made stand-in for the missing build-index-0 scene. " +
                    "Overwrite " + ScenePath + "?", "Overwrite", "Cancel"))
            {
                Debug.Log("[ReadySceneBuilder] cancelled, left " + ScenePath + " untouched");
                return;
            }

            EnsureFolderFor(ScenePath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            camGo.AddComponent<AudioListener>();

            // Order is irrelevant: every Awake runs before any Start, so AndroidScript.i is set by
            // the time UI_Ready.Start calls Crypto.Init().
            var bridgeGo = new GameObject("AndroidScript");
            bridgeGo.AddComponent<AndroidScript>();

            var readyGo = new GameObject("UI_Ready");
            readyGo.AddComponent<UI_Ready>();

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError("[ReadySceneBuilder] SaveScene failed for " + ScenePath);
                return;
            }

            Debug.Log("[ReadySceneBuilder] wrote " + ScenePath + " -- 3 GameObjects " +
                      "(Main Camera/MainCamera, AndroidScript, UI_Ready). Stand-in for build " +
                      "index 0, which has no data in the APK. Enter playmode here so " +
                      "UI_Ready.Start runs Crypto.Init(); on desktop KEY and KEY2 both derive " +
                      "from SystemInfo.deviceUniqueIdentifier, so they are identical by design.");
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
    }
}
