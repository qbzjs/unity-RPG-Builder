using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
#if UNITY_EDITOR
using XNodeEditor;

public static class RPGDialogueGraphUtilities
{
    public static Color getBackgroundColor(NodeEditor editor)
    {
        return editor.GetTint();
    }
    public static Color getTypeColor(NodePort port)
    {
        return NodeEditorWindow.current.graphEditor.GetPortColor(port);
    }

    public static Rect getInputRect(XNode.NodePort port, Rect rect)
    {
        rect = GUILayoutUtility.GetLastRect();
        rect.position = rect.position - new Vector2(16, 0);

        rect.size = new Vector2(16, 16);
        return rect;
    }

    public static Rect getOuputRect(XNode.NodePort port, Rect rect)
    {
        rect = GUILayoutUtility.GetLastRect();
        rect.position = rect.position + new Vector2(rect.width, 0);
        rect.size = new Vector2(16, 16);
        return rect;
    }
}
#endif
