using BLINK.RPGBuilder;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(CombatNode))]
public class CombatNodeEditor : Editor
{
    private CombatNode nodeCbtREF;

    private int curTab;
    private void OnEnable()
    {
        nodeCbtREF = (CombatNode) target;
        switch (nodeCbtREF.nodeType)
        {
            case CombatNode.COMBAT_NODE_TYPE.player:
                curTab = 1;
                break;
            case CombatNode.COMBAT_NODE_TYPE.objectAction:
                curTab = 2;
                break;
            case CombatNode.COMBAT_NODE_TYPE.pet:
                curTab = 3;
                break;
        }
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((CombatNode) target), typeof(CombatNode),
            false);
        GUI.enabled = true;
        EditorGUI.BeginChangeCheck();
        var SubTitleStyle = new GUIStyle();
        SubTitleStyle.alignment = TextAnchor.UpperLeft;
        SubTitleStyle.fontSize = 17;
        SubTitleStyle.fontStyle = FontStyle.Bold;
        SubTitleStyle.normal.textColor = Color.white;

        curTab = GUILayout.Toolbar(curTab, new[] {"MOB", "PLAYER", "OBJECT", "PET"});
        switch (curTab)
        {
            case 0:
                nodeCbtREF.nodeType = CombatNode.COMBAT_NODE_TYPE.mob;

                GUILayout.Space(5);
                GUILayout.Label("Mob Specific", SubTitleStyle);
                GUILayout.Space(5);
                nodeCbtREF.npcDATA =
                    (RPGNpc) EditorGUILayout.ObjectField("NPC DATA:", nodeCbtREF.npcDATA, typeof(RPGNpc), false);
                nodeCbtREF.agentREF =
                    (RPGBAIAgent) EditorGUILayout.ObjectField("AI_Controller", nodeCbtREF.agentREF, typeof(RPGBAIAgent),
                        true);
                break;
            case 1:
                nodeCbtREF.nodeType = CombatNode.COMBAT_NODE_TYPE.player;

                GUILayout.Space(5);
                GUILayout.Label("Player Specific", SubTitleStyle);
                GUILayout.Space(5);

                nodeCbtREF.playerControllerEssentials = (RPGBCharacterControllerEssentials) EditorGUILayout.ObjectField("Controller Essentials:",
                    nodeCbtREF.playerControllerEssentials, typeof(RPGBCharacterControllerEssentials), true);
                
                nodeCbtREF.appearanceREF = (PlayerAppearanceHandler) EditorGUILayout.ObjectField("Player Appearance:",
                    nodeCbtREF.appearanceREF, typeof(PlayerAppearanceHandler), true);
                nodeCbtREF.indicatorManagerREF = (GroundIndicatorManager) EditorGUILayout.ObjectField(
                    "Ground Indicator Manager:", nodeCbtREF.indicatorManagerREF, typeof(GroundIndicatorManager), true);
                break;
            case 2:
                nodeCbtREF.nodeType = CombatNode.COMBAT_NODE_TYPE.objectAction;

                GUILayout.Space(5);
                GUILayout.Label("Object Specific", SubTitleStyle);
                GUILayout.Space(5);

                nodeCbtREF.npcDATA = (RPGNpc) EditorGUILayout.ObjectField("NPC DATA:", nodeCbtREF.npcDATA, typeof(RPGNpc), false);
                break;
            case 3:
                nodeCbtREF.nodeType = CombatNode.COMBAT_NODE_TYPE.pet;

                GUILayout.Space(5);
                GUILayout.Label("Pet Specific", SubTitleStyle);
                GUILayout.Space(5);

                nodeCbtREF.npcDATA =
                    (RPGNpc) EditorGUILayout.ObjectField("NPC DATA:", nodeCbtREF.npcDATA, typeof(RPGNpc), false);
                nodeCbtREF.agentREF =
                    (RPGBAIAgent) EditorGUILayout.ObjectField("AI_Controller", nodeCbtREF.agentREF, typeof(RPGBAIAgent),
                        true);
                break;
        }

        // GENERAL FIELDS
        GUILayout.Space(5);
        GUILayout.Label("General Fields", SubTitleStyle);
        GUILayout.Space(5);

        nodeCbtREF.thisRendererREF = (Renderer) EditorGUILayout.ObjectField("Mesh Renderer Target",
            nodeCbtREF.thisRendererREF, typeof(Renderer), true);
        nodeCbtREF.nameplateYOffset = EditorGUILayout.FloatField("Nameplate Y Offset", nodeCbtREF.nameplateYOffset);

        GUILayout.Space(5);
        GUILayout.Label("STATS", SubTitleStyle);
        GUILayout.Space(5);
        var tps2 = serializedObject.FindProperty("nodeStats");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps2, true);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        GUILayout.Space(5);
        GUILayout.Label("EFFECTS", SubTitleStyle);
        GUILayout.Space(5);
        var tps3 = serializedObject.FindProperty("nodeStateData");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps3, true);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();


        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(nodeCbtREF);
    }
}