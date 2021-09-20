using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RPGBAdvancedDialogueOptionsNodeSelector : EditorWindow
{
    private static RPGBAdvancedDialogueOptionsNodeSelector Instance;
    private GUISkin skin;
    private RPGBuilderEditorDATA editorDATA;
    private RPGBuilderEditorDATA.ThemeTypes cachedTheme;
    private Vector2 viewScrollPosition;

    //public static RPGDialogueTextNode currentNode;
    public static RPGDialogueGraph currentGraph;

    public enum selectorType
    {
        requirement,
        gameAction
    }

    public selectorType thisSelectorType;
    
    private void OnEnable()
    {
        Instance = this;
        skin = Resources.Load<GUISkin>("THMSV/RPGBuilderEditor/GUIStyles/RPGBuilderSkin");
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("THMSV/RPGBuilderEditor/Data/RPGBuilderEditorData");
        cachedTheme = editorDATA.curEditorTheme;
    }

    private void OnDestroy()
    {
        //currentNode = null;
        currentGraph = null;
        Instance = null;
    }

    private void OnDisable()
    {
        //currentNode = null;
        currentGraph = null;
        Instance = null;
    }
    public static bool IsOpen => Instance != null;

    public void InitSelector(RPGDialogueGraph graph, selectorType selector)
    {
        thisSelectorType = selector;
        currentGraph = graph;
    }
    
    private void OnGUI()
    {
        DrawView();
    }

    private void DrawView()
    {
        if (currentGraph == null)
        {
            Close();
            return;
        }
        
        Color guiColor = GUI.color;

        int width = Screen.width;
        if (width < 350) width = 350;
        float height = Screen.height;
        if (height < 500) height = 500;

        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        
        viewScrollPosition = GUILayout.BeginScrollView(viewScrollPosition, false, false, GUILayout.Width(width), GUILayout.Height(height));

        GUILayout.Space(10);
        string graphName = currentGraph.name;
        graphName = graphName.Remove(0, 13);
        graphName = graphName.Replace("_GRAPH", "");
        GUILayout.Label(graphName, skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));

        foreach (var node in currentGraph.nodes)
        {
            RPGDialogueTextNode textNodeREF = (RPGDialogueTextNode)node;

            if (!GUILayout.Button(textNodeREF.message, GUILayout.Width(325), GUILayout.Height(25))) continue;
            if(thisSelectorType == selectorType.requirement)
                RPGBAdvancedDialogueOptionsWindow.AssignTextNodeRequirement(textNodeREF);
            else
                RPGBAdvancedDialogueOptionsWindow.AssignTextNodeGameAction(textNodeREF);
            Instance.Close();
        }
        
        GUI.color = guiColor;
        
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
    }
}
