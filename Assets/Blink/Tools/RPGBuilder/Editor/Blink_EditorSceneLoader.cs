using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Blink_EditorSceneLoader : EditorWindow
{
    private RPGBuilderEditorDATA editorDATA;
    
    [MenuItem("BLINK/Scene Loader")]
    private static void Init()
    {
        // Get existing open window or if none, make a new one:
        var window = (Blink_EditorSceneLoader) GetWindow(typeof(Blink_EditorSceneLoader));
        window.Show();
    }

    public void OnEnable()
    {
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
    }

    private Vector2 scrollPos;
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUILayout.BeginHorizontal();
        GUILayout.Space(Screen.width / 4);
        GUILayout.BeginVertical();

        foreach (var scene in editorDATA.sceneLoaderList.Where(scene => GUILayout.Button(scene.sceneName, GUILayout.Height(22))))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scene.scene));
        }

        GUILayout.EndVertical();
        GUILayout.Space(Screen.width / 4);
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
    }
}