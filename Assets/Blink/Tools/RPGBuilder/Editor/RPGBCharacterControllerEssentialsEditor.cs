using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Character;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RPGBCharacterControllerEssentials))]
public class RPGBCharacterControllerEssentialsEditor : Editor
{
    private RPGBCharacterControllerEssentials REF;
    
    public RPGBuilderEditorDATA editorDATA;
    private RPGGeneralDATA generalSettings;
    
    private void OnEnable()
    {
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        generalSettings = Resources.Load<RPGGeneralDATA>(  editorDATA.RPGBDatabasePath +"Settings/GeneralSettings");
        REF = (RPGBCharacterControllerEssentials) target;
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((RPGBCharacterControllerEssentials) target),
            typeof(RPGBCharacterControllerEssentials),
            false);
        GUI.enabled = true;
        EditorGUI.BeginChangeCheck();
        var SubTitleStyle = new GUIStyle();
        SubTitleStyle.alignment = TextAnchor.UpperLeft;
        SubTitleStyle.fontSize = 17;
        SubTitleStyle.fontStyle = FontStyle.Bold;
        SubTitleStyle.normal.textColor = Color.white;

        if (generalSettings.useOldController)
        {
            GUILayout.Space(5);
            GUILayout.Label("Using: Built in controller", SubTitleStyle);
            GUILayout.Space(5);
        }

        if (!EditorGUI.EndChangeCheck()) return;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(REF);
    }

}
