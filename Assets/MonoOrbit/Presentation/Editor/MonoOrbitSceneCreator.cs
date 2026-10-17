using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonoOrbit.Presentation.Editor
{
    /// <summary>
    /// Explicit authoring command. Never silently replaces existing scenes.
    /// The generated scene is a real Unity asset and should be committed after QA.
    /// </summary>
    public static class MonoOrbitSceneCreator
    {
        private const string ScenePath = "Assets/MonoOrbit/Scenes/MonoOrbitMatch.unity";

        [MenuItem("Tools/Mono Orbit/Create Match Scene")]
        public static void CreateMatchScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!AssetDatabase.IsValidFolder("Assets/MonoOrbit/Scenes"))
                AssetDatabase.CreateFolder("Assets/MonoOrbit", "Scenes");

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                if (!EditorUtility.DisplayDialog("Mono Orbit",
                    "A match scene already exists. Replace it with a newly generated one?",
                    "Replace scene", "Cancel"))
                    return;
            }

            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.009f, 0.019f, 0.047f);
            cameraObject.AddComponent<AudioListener>();

            var screenObject = new GameObject("Mono Orbit Match Screen");
            screenObject.AddComponent<MonoOrbitMatchScreen>();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError("Mono Orbit: Could not save match scene.");
                return;
            }

            var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!buildScenes.Exists(entry => entry.path == ScenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Mono Orbit: Match scene created at " + ScenePath);
            Selection.activeGameObject = screenObject;
        }
    }
}
