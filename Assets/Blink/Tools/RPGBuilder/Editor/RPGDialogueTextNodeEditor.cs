using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(RPGDialogueTextNode))]
public class RPGDialogueTextNodeEditor : NodeEditor
{

    private GUISkin skin;
    private bool isInitialized;
    public RPGBuilderEditorDATA editorDATA;

    private void InitData()
    {
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        skin = Resources.Load<GUISkin>(editorDATA.RPGBEditorDataPath + "RPGBuilderSkin");
        isInitialized = true;
    }
    
    public override void OnHeaderGUI() {
        
        if (!isInitialized)
        {
            InitData();
        }
        
        GUI.color = Color.white;
        RPGDialogueTextNode rpgDialogueTextNode = target as RPGDialogueTextNode;
        
        string title = rpgDialogueTextNode.message != "" ? rpgDialogueTextNode.message : "New Text Node";
        GUIStyle headerStyle = NodeEditorResources.styles.nodeHeader;
        headerStyle.clipping = TextClipping.Clip;
        
        if (title.Length > 40)
        {
            title = title.Remove(39);
            title += "...";
        }

        GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Width(300),GUILayout.Height(25));
        
        GUI.color = Color.white;
    }

    public override void OnBodyGUI()
    {

        if (!isInitialized)
        {
            InitData();
        }
        RPGDialogueTextNode dialogueTextNode = target as RPGDialogueTextNode;
        
        if (dialogueTextNode is null) return;
            
        GUILayout.BeginHorizontal();
        GUILayout.Space(9);
        GUILayout.Label("Identity:", GUILayout.Width(60));
        dialogueTextNode.identityType =
            (RPGDialogueTextNode.IdentityType) EditorGUILayout.EnumPopup(dialogueTextNode.identityType,
                GUILayout.Width(245));
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.BeginHorizontal();
        NodePort input = dialogueTextNode.GetInputPort("previousNode");
        GUIStyle portStyle = NodeEditorWindow.current.graphEditor.GetPortStyle(input);
        NodeEditorGUILayout.PortField(GUIContent.none, input, GUILayout.Width(0));
        Rect inpuRect = RPGDialogueGraphUtilities.getInputRect(input, new Rect());
        NodeEditor inputEditor = GetEditor(input.node, NodeEditorWindow.current);
        NodeEditorGUILayout.DrawPortHandle(inpuRect, RPGDialogueGraphUtilities.getBackgroundColor(inputEditor),
            RPGDialogueGraphUtilities.getTypeColor(input), portStyle.normal.background,
            portStyle.active.background);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Message:", GUILayout.Width(60));
        dialogueTextNode.message = GUILayout.TextArea(dialogueTextNode.message, GUILayout.Width(245), GUILayout.Height(75));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        NodePort output = dialogueTextNode.GetOutputPort("nextNodes");
        NodeEditorGUILayout.PortField(GUIContent.none, output, GUILayout.Width(0));
        Rect outputRect = RPGDialogueGraphUtilities.getOuputRect(output, new Rect());
        NodeEditor outputEditor = GetEditor(output.node, NodeEditorWindow.current);
        NodeEditorGUILayout.DrawPortHandle(outputRect, RPGDialogueGraphUtilities.getBackgroundColor(outputEditor),
            RPGDialogueGraphUtilities.getTypeColor(output), portStyle.normal.background,
            portStyle.active.background);
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();

        
        
        GUILayout.BeginHorizontal();
        GUILayout.Space(315);
        if (GUILayout.Button("", skin.GetStyle("SettingsButton"), GUILayout.Width(20), GUILayout.Height(20)))
        {
            if (RPGBAdvancedDialogueOptionsWindow.currentNode == dialogueTextNode)
            {
                return;
            }
            if (RPGBAdvancedDialogueOptionsWindow.IsOpen)
            {
                EditorWindow.GetWindow(typeof(RPGBAdvancedDialogueOptionsWindow)).Close();
                    
                var window = (RPGBAdvancedDialogueOptionsWindow) EditorWindow.GetWindow(
                    typeof(RPGBAdvancedDialogueOptionsWindow), false, "Advanced Dialogue Node Options");
                window.minSize = new Vector2(350, 500);
                GUI.contentColor = Color.white;
                window.Show();
                window.AssignCurrentDialogueNode(dialogueTextNode);
            }
            else
            {
                var window = (RPGBAdvancedDialogueOptionsWindow) EditorWindow.GetWindow(
                    typeof(RPGBAdvancedDialogueOptionsWindow), false, "Advanced Dialogue Node Options");
                window.minSize = new Vector2(350, 500);
                GUI.contentColor = Color.white;
                window.Show();
                window.AssignCurrentDialogueNode(dialogueTextNode);
            }
        }
        GUILayout.EndHorizontal();
        
    }

}
