using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Character;
using UnityEditor;
using UnityEngine;

namespace BLINK.Controller
{
    [CustomEditor(typeof(RPGBThirdPersonCharacterControllerEssentials))]
    public class RPGBuilderThirdPersonControllerEssentialsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:",
                MonoScript.FromMonoBehaviour((RPGBCharacterControllerEssentials) target),
                typeof(RPGBCharacterControllerEssentials),
                false);
            GUI.enabled = true;

            var SubTitleStyle = new GUIStyle();
            SubTitleStyle.alignment = TextAnchor.UpperLeft;
            SubTitleStyle.fontSize = 17;
            SubTitleStyle.fontStyle = FontStyle.Bold;
            SubTitleStyle.normal.textColor = Color.white;

            GUILayout.Space(5);
            GUILayout.Label("RPG Builder Action RPG Controller", SubTitleStyle);
            GUILayout.Space(5);
        }
    }
}
