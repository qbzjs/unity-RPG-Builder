using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGDialogue : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;

    public RPGDialogueGraph dialogueGraph;

    public bool hasExitNode;
    public string exitNodeText = "Goodbye";
    
    public void updateThis(RPGDialogue newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        description = newData.description;
        displayName = newData.displayName;
        dialogueGraph = newData.dialogueGraph;
        hasExitNode = newData.hasExitNode;
        exitNodeText = newData.exitNodeText;
    }
}
