using AirportAR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AirportAR.Editor
{
    public static class DemoSceneSetup
    {
        [MenuItem("Airport AR/Create Main Scene")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Object.DestroyImmediate(Camera.main.gameObject);

            var bootstrap = new GameObject("DemoApp");
            bootstrap.AddComponent<DemoAppBootstrap>();

            const string scenePath = "Assets/Scenes/MainScene.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);

            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            Debug.Log("[DemoSceneSetup] MainScene created at Assets/Scenes/MainScene.unity");
        }
    }
}
