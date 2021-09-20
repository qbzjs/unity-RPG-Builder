using System;
using BLINK.RPGBuilder;
using BLINK.RPGBuilder.World;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BLINK.RPGBuilder.Managers;
using UnityEditor.SceneManagement;
using UnityEngine.Events;

[CanEditMultipleObjects]
[CustomEditor(typeof(InteractiveNode))]
public class InteractiveNodeEditor : Editor
{
    private InteractiveNode nodeREF;
    
    private SerializedProperty isClick;
    private SerializedProperty isTrigger;
    private SerializedProperty useCount;
    private SerializedProperty cooldown;
    private SerializedProperty interactionTime;
    private SerializedProperty useDistanceMax;
    private SerializedProperty nodeUseAnimation;
    private SerializedProperty nodeInteractableYOffset;
    private SerializedProperty nodeInteractableName;
    private SerializedProperty weaponEquippedRequired;
    
    private void InitSerializedData()
    {
        isClick = serializedObject.FindProperty("isClick");
        isTrigger = serializedObject.FindProperty("isTrigger");
        useCount = serializedObject.FindProperty("useCount");
        cooldown = serializedObject.FindProperty("cooldown");
        interactionTime = serializedObject.FindProperty("interactionTime");
        useDistanceMax = serializedObject.FindProperty("useDistanceMax");
        nodeUseAnimation = serializedObject.FindProperty("nodeUseAnimation");
        nodeInteractableYOffset = serializedObject.FindProperty("interactableUIOffsetY");
        nodeInteractableName = serializedObject.FindProperty("interactableName");
        weaponEquippedRequired = serializedObject.FindProperty("weaponEquippedRequired");
    }
    private void OnEnable()
    {
        nodeREF = (InteractiveNode) target;
        InitSerializedData();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((InteractiveNode) target),
            typeof(InteractiveNode), false);
        GUI.enabled = true;

        var SubTitleStyle = new GUIStyle();
        SubTitleStyle.alignment = TextAnchor.UpperLeft;
        SubTitleStyle.fontSize = 17;
        SubTitleStyle.fontStyle = FontStyle.Bold;
        SubTitleStyle.normal.textColor = Color.white;

        
        GUILayout.Space(5);
        GUILayout.Label("Node Type", SubTitleStyle);
        GUILayout.Space(5);
        nodeREF.nodeType = (InteractiveNode.InteractiveNodeType) EditorGUILayout.EnumPopup("Type", nodeREF.nodeType);

        switch (nodeREF.nodeType)
        {
            case InteractiveNode.InteractiveNodeType.resourceNode:
            {
                GUILayout.Space(5);
                GUILayout.Label("Resource Node Specific", SubTitleStyle);
                GUILayout.Space(5);

                nodeREF.resourceNodeData = (RPGResourceNode) EditorGUILayout.ObjectField("Resource Node",
                    nodeREF.resourceNodeData, typeof(RPGResourceNode), false);
                break;
            }
            case InteractiveNode.InteractiveNodeType.effectNode:
            {
                GUILayout.Space(5);
                GUILayout.Label("Effects Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Effect"))
                    nodeREF.effectsData.Add(new InteractiveNode.effectsDATA());

                var ThisList = serializedObject.FindProperty("effectsData");
                nodeREF.effectsData = GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.effectsDATA>;

                for (var a = 0; a < nodeREF.effectsData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        nodeREF.effectsData.RemoveAt(a);
                        serializedObject.Update();
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (nodeREF.effectsData[a].effect != null) effectName = nodeREF.effectsData[a].effect._name;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    nodeREF.effectsData[a].effect = (RPGEffect) EditorGUILayout.ObjectField("Effect",
                        nodeREF.effectsData[a].effect, typeof(RPGEffect), false);
                    nodeREF.effectsData[a].chance =
                        EditorGUILayout.Slider("Chance", nodeREF.effectsData[a].chance, 0f, 100f);



                    GUILayout.Space(10);
                }
            }

                break;
            case InteractiveNode.InteractiveNodeType.abilityNode:
            {


            }
                break;
            case InteractiveNode.InteractiveNodeType.questNode:
            {
                GUILayout.Space(5);
                GUILayout.Label("Quest Specific", SubTitleStyle);
                GUILayout.Space(5);

                nodeREF.questsData.quest = (RPGQuest) EditorGUILayout.ObjectField("Quest",
                    nodeREF.questsData.quest, typeof(RPGQuest), false);
                nodeREF.questsData.chance =
                    EditorGUILayout.Slider("Chance", nodeREF.questsData.chance, 0f, 100f);

                break;
            }
            case InteractiveNode.InteractiveNodeType.giveTreePoint:
            {
                GUILayout.Space(5);
                GUILayout.Label("Tree Points Specific", SubTitleStyle);
                GUILayout.Space(5);
                if (GUILayout.Button("+ Add Tree Point"))
                    nodeREF.treePointsData.Add(new InteractiveNode.treePointsDATA());

                var ThisList = serializedObject.FindProperty("treePointsData");
                nodeREF.treePointsData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.treePointsDATA>;

                for (var a = 0; a < nodeREF.treePointsData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        nodeREF.treePointsData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (nodeREF.treePointsData[a].treePoint != null)
                        effectName = nodeREF.treePointsData[a].treePoint._name;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    nodeREF.treePointsData[a].treePoint = (RPGTreePoint) EditorGUILayout.ObjectField(
                        "Tree Point", nodeREF.treePointsData[a].treePoint, typeof(RPGTreePoint), false);
                    nodeREF.treePointsData[a].amount =
                        EditorGUILayout.IntField("Amount", nodeREF.treePointsData[a].amount);
                    nodeREF.treePointsData[a].chance =
                        EditorGUILayout.Slider("Chance", nodeREF.treePointsData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.teachSkill:
            {
                GUILayout.Space(5);
                GUILayout.Label("Skills Specific", SubTitleStyle);
                GUILayout.Space(5);
                break;
            }
            case InteractiveNode.InteractiveNodeType.giveClassEXP:
            {

                GUILayout.Space(5);
                GUILayout.Label("Class EXP Specific", SubTitleStyle);
                GUILayout.Space(5);

                nodeREF.classExpData.expAmount =
                    EditorGUILayout.IntField("Amount", nodeREF.classExpData.expAmount);
                nodeREF.classExpData.chance =
                    EditorGUILayout.Slider("Chance", nodeREF.classExpData.chance, 0f, 100f);

                break;
            }
            case InteractiveNode.InteractiveNodeType.giveSkillEXP:
            {
                GUILayout.Space(5);
                GUILayout.Label("Skills EXP Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Skill"))
                    nodeREF.skillExpData.Add(new InteractiveNode.skillExpDATA());

                var ThisList = serializedObject.FindProperty("skillExpData");
                nodeREF.skillExpData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.skillExpDATA>;

                for (var a = 0; a < nodeREF.skillExpData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        nodeREF.skillExpData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (nodeREF.skillExpData[a].skill != null) effectName = nodeREF.skillExpData[a].skill._name;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    nodeREF.skillExpData[a].skill = (RPGSkill) EditorGUILayout.ObjectField("Skill",
                        nodeREF.skillExpData[a].skill, typeof(RPGSkill), false);
                    nodeREF.skillExpData[a].expAmount =
                        EditorGUILayout.IntField("Amount", nodeREF.skillExpData[a].expAmount);
                    nodeREF.skillExpData[a].chance =
                        EditorGUILayout.Slider("Chance", nodeREF.skillExpData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.completeTask:
            {
                GUILayout.Space(5);
                GUILayout.Label("Tasks Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Task"))
                    nodeREF.taskData.Add(new InteractiveNode.taskDATA());

                var ThisList = serializedObject.FindProperty("taskData");
                nodeREF.taskData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.taskDATA>;

                for (var a = 0; a < nodeREF.taskData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        nodeREF.taskData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (nodeREF.taskData[a].task != null) effectName = nodeREF.taskData[a].task._name;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    nodeREF.taskData[a].task = (RPGTask) EditorGUILayout.ObjectField("Task",
                        nodeREF.taskData[a].task, typeof(RPGTask), false);
                    nodeREF.taskData[a].chance =
                        EditorGUILayout.Slider("Chance", nodeREF.taskData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.container:
            {
                GUILayout.Space(5);
                GUILayout.Label("Container Specific", SubTitleStyle);
                GUILayout.Space(5);

                if (GUILayout.Button("+ Add Loot Table"))
                    nodeREF.containerTablesData.Add(new InteractiveNode.containerLootTablesDATA());

                var ThisList = serializedObject.FindProperty("containerTablesData");
                nodeREF.containerTablesData =
                    GetTargetObjectOfProperty(ThisList) as List<InteractiveNode.containerLootTablesDATA>;

                for (var a = 0; a < nodeREF.containerTablesData.Count; a++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle xButtonStyle = new GUIStyle();
                    xButtonStyle.normal.textColor = Color.red;
                    xButtonStyle.fontSize = 18;

                    if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        nodeREF.containerTablesData.RemoveAt(a);
                        return;
                    }

                    var requirementNumber = a + 1;
                    string effectName = "";
                    if (nodeREF.containerTablesData[a].lootTable != null)
                        effectName = nodeREF.containerTablesData[a].lootTable._name;
                    EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                        GUILayout.Width(150));
                    EditorGUILayout.EndHorizontal();

                    nodeREF.containerTablesData[a].lootTable =
                        (RPGLootTable) EditorGUILayout.ObjectField("Loot Table",
                            nodeREF.containerTablesData[a].lootTable, typeof(RPGLootTable), false);
                    nodeREF.containerTablesData[a].chance = EditorGUILayout.Slider("Chance",
                        nodeREF.containerTablesData[a].chance, 0f, 100f);

                    GUILayout.Space(10);
                }

                break;
            }
            case InteractiveNode.InteractiveNodeType.UnityEvent:
            {
                GUILayout.Space(5);
                GUILayout.Label("Unity Event Specific", SubTitleStyle);
                GUILayout.Space(5);

                var ThisList = serializedObject.FindProperty("unityEvent");
                serializedObject.Update();
                EditorGUILayout.PropertyField(ThisList, true);
                serializedObject.ApplyModifiedProperties();

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        // REQUIREMENT FIELDS
        GUILayout.Space(5);
        GUILayout.Label("Requirements", SubTitleStyle);
        GUILayout.Space(5);
        isClick.boolValue = EditorGUILayout.Toggle("Is Click?", isClick.boolValue);
        isTrigger.boolValue = EditorGUILayout.Toggle("Is Trigger?", isTrigger.boolValue);
        weaponEquippedRequired.stringValue = EditorGUILayout.TextField("Require Weapon/Tool?", weaponEquippedRequired.stringValue);

        if (GUILayout.Button("+ Add Requirement"))
            nodeREF.useRequirement.Add(new RequirementsManager.RequirementDATA());

        var ThisList2 = serializedObject.FindProperty("useRequirement");
        nodeREF.useRequirement = GetTargetObjectOfProperty(ThisList2) as List<RequirementsManager.RequirementDATA>;

        for (var a = 0; a < nodeREF.useRequirement.Count; a++)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUIStyle xButtonStyle = new GUIStyle();
            xButtonStyle.normal.textColor = Color.red;
            xButtonStyle.fontSize = 18;

            if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(18), GUILayout.Height(18)))
            {
                nodeREF.useRequirement.RemoveAt(a);
                return;
            }

            var requirementNumber = a + 1;
            string effectName = nodeREF.useRequirement[a].requirementType.ToString();
            EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();


            nodeREF.useRequirement[a].requirementType =
                (RequirementsManager.RequirementType) EditorGUILayout.EnumPopup("Type",
                    nodeREF.useRequirement[a].requirementType);

            switch (nodeREF.useRequirement[a].requirementType)
            {
                case RequirementsManager.RequirementType._class:
                    nodeREF.useRequirement[a].classRequiredREF =
                        (RPGClass) EditorGUILayout.ObjectField("Class",
                            nodeREF.useRequirement[a].classRequiredREF, typeof(RPGClass), false);
                    if (nodeREF.useRequirement[a].classRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].classRequiredID = nodeREF.useRequirement[a].classRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].classRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.classLevel:
                    nodeREF.useRequirement[a].classRequiredREF =
                        (RPGClass) EditorGUILayout.ObjectField("Class",
                            nodeREF.useRequirement[a].classRequiredREF, typeof(RPGClass), false);
                    nodeREF.useRequirement[a].classLevelValue = EditorGUILayout.IntField("Level",
                        nodeREF.useRequirement[a].classLevelValue);
                    if (nodeREF.useRequirement[a].classRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].classRequiredID = nodeREF.useRequirement[a].classRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].classRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.race:
                    nodeREF.useRequirement[a].raceRequiredREF =
                        (RPGRace) EditorGUILayout.ObjectField("Race",
                            nodeREF.useRequirement[a].raceRequiredREF, typeof(RPGRace), false);
                    if (nodeREF.useRequirement[a].raceRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].raceRequiredID = nodeREF.useRequirement[a].raceRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].raceRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.abilityKnown:
                case RequirementsManager.RequirementType.abilityNotKnown:
                    nodeREF.useRequirement[a].abilityRequiredREF =
                        (RPGAbility) EditorGUILayout.ObjectField("Ability",
                            nodeREF.useRequirement[a].abilityRequiredREF, typeof(RPGAbility), false);
                    if (nodeREF.useRequirement[a].abilityRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].abilityRequiredID = nodeREF.useRequirement[a].abilityRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].abilityRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.recipeKnown:
                case RequirementsManager.RequirementType.recipeNotKnown:
                    nodeREF.useRequirement[a].recipeRequiredREF =
                        (RPGCraftingRecipe) EditorGUILayout.ObjectField("Recipe",
                            nodeREF.useRequirement[a].recipeRequiredREF, typeof(RPGCraftingRecipe),
                            false);
                    if (nodeREF.useRequirement[a].recipeRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].craftingRecipeRequiredID = nodeREF.useRequirement[a].recipeRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].craftingRecipeRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.resourceNodeKnown:
                case RequirementsManager.RequirementType.resourceNodeNotKnown:
                    nodeREF.useRequirement[a].resourceNodeRequiredREF =
                        (RPGResourceNode) EditorGUILayout.ObjectField("Resource Node",
                            nodeREF.useRequirement[a].resourceNodeRequiredREF, typeof(RPGResourceNode),
                            false);
                    if (nodeREF.useRequirement[a].resourceNodeRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].resourceNodeRequiredID = nodeREF.useRequirement[a].resourceNodeRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].resourceNodeRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.bonusKnown:
                case RequirementsManager.RequirementType.bonusNotKnown:
                    nodeREF.useRequirement[a].bonusRequiredREF =
                        (RPGBonus) EditorGUILayout.ObjectField("Bonus",
                            nodeREF.useRequirement[a].bonusRequiredREF, typeof(RPGBonus), false);
                    if (nodeREF.useRequirement[a].bonusRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].bonusRequiredID = nodeREF.useRequirement[a].bonusRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].bonusRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.itemOwned:
                    nodeREF.useRequirement[a].itemRequiredREF =
                        (RPGItem) EditorGUILayout.ObjectField("Item",
                            nodeREF.useRequirement[a].itemRequiredREF, typeof(RPGItem), false);
                    nodeREF.useRequirement[a].itemRequiredCount = EditorGUILayout.IntField("Count",
                        nodeREF.useRequirement[a].itemRequiredCount);
                    nodeREF.useRequirement[a].consumeItem =
                        EditorGUILayout.Toggle("Consume", nodeREF.useRequirement[a].consumeItem);
                    if (nodeREF.useRequirement[a].itemRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].itemRequiredID = nodeREF.useRequirement[a].itemRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].itemRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.npcKilled:
                    nodeREF.useRequirement[a].npcRequiredREF =
                        (RPGNpc) EditorGUILayout.ObjectField("NPC",
                            nodeREF.useRequirement[a].npcRequiredREF, typeof(RPGNpc), false);
                    nodeREF.useRequirement[a].npcKillsRequired = EditorGUILayout.IntField("Kills",
                        nodeREF.useRequirement[a].npcKillsRequired);
                    if (nodeREF.useRequirement[a].npcRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].npcRequiredID = nodeREF.useRequirement[a].npcRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].npcRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.questState:
                    nodeREF.useRequirement[a].questRequiredREF =
                        (RPGQuest) EditorGUILayout.ObjectField("Quest",
                            nodeREF.useRequirement[a].questRequiredREF, typeof(RPGQuest), false);
                    nodeREF.useRequirement[a].questStateRequired =
                        (QuestManager.questState) EditorGUILayout.EnumPopup("State",
                            nodeREF.useRequirement[a].questStateRequired);
                    if (nodeREF.useRequirement[a].questRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].questRequiredID = nodeREF.useRequirement[a].questRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].questRequiredID = -1;
                    }
                    break;
                case RequirementsManager.RequirementType.skillLevel:
                    nodeREF.useRequirement[a].skillRequiredREF =
                        (RPGSkill) EditorGUILayout.ObjectField("Skill",
                            nodeREF.useRequirement[a].skillRequiredREF, typeof(RPGSkill), false);
                    nodeREF.useRequirement[a].skillLevelValue = EditorGUILayout.IntField("Level",
                        nodeREF.useRequirement[a].skillLevelValue);
                    if (nodeREF.useRequirement[a].skillRequiredREF != null)
                    {
                        nodeREF.useRequirement[a].skillRequiredID = nodeREF.useRequirement[a].skillRequiredREF.ID;
                    }
                    else
                    {
                        nodeREF.useRequirement[a].skillRequiredID = -1;
                    }
                    break;
            }

            GUILayout.Space(5);
        }

        // GENERAL FIELDS
        GUILayout.Space(5);
        GUILayout.Label("State", SubTitleStyle);
        GUILayout.Space(5);
        nodeREF.nodeState = (InteractiveNode.InteractiveNodeState) EditorGUILayout.EnumPopup("State", nodeREF.nodeState);
        if (nodeREF.nodeType != InteractiveNode.InteractiveNodeType.resourceNode)
        {
            useCount.intValue = EditorGUILayout.IntField("Use Count", useCount.intValue);
            cooldown.floatValue = EditorGUILayout.FloatField("Cooldown", cooldown.floatValue);
            interactionTime.floatValue = EditorGUILayout.FloatField("Interaction Time", interactionTime.floatValue);
            useDistanceMax.floatValue = EditorGUILayout.FloatField("Use Distance Max", useDistanceMax.floatValue);
        }

        GUILayout.Space(5);
        GUILayout.Label("Node Animation", SubTitleStyle);
        GUILayout.Space(5);
        //animationName.stringValue = EditorGUILayout.TextField("Player Animation", animationName.stringValue);
        nodeUseAnimation.stringValue = EditorGUILayout.TextField("Node Animation", nodeUseAnimation.stringValue);
        

        GUILayout.Space(5);
        GUILayout.Label("Player Animation", SubTitleStyle);
        GUILayout.Space(5);
        nodeREF.visualAnimations = DrawVisualAnimationsList(nodeREF.visualAnimations);


        GUILayout.Space(5);
        GUILayout.Label("Sound", SubTitleStyle);
        GUILayout.Space(5);
        nodeREF.nodeUseSound = (AudioClip) EditorGUILayout.ObjectField("Use Sound",
            nodeREF.nodeUseSound, typeof(AudioClip), false);
        
        GUILayout.Space(5);
        GUILayout.Label("Visuals", SubTitleStyle);
        GUILayout.Space(5);
        nodeREF.readyVisual =
            (GameObject) EditorGUILayout.ObjectField("Ready Visual", nodeREF.readyVisual, typeof(GameObject), true);
        nodeREF.onCooldownVisual = (GameObject) EditorGUILayout.ObjectField("On Cooldown Visual",
            nodeREF.onCooldownVisual, typeof(GameObject), true);
        nodeREF.disabledVisual =
            (GameObject) EditorGUILayout.ObjectField("Disabled Visual", nodeREF.disabledVisual, typeof(GameObject),
                true);
        
        GUILayout.Space(5);
        GUILayout.Label("Interactable UI", SubTitleStyle);
        GUILayout.Space(5);
        if (nodeREF.nodeType != InteractiveNode.InteractiveNodeType.resourceNode)
        {
            nodeInteractableName.stringValue =
                EditorGUILayout.TextField("Interactable Name", nodeInteractableName.stringValue);
        }
        nodeInteractableYOffset.floatValue = EditorGUILayout.FloatField("Y Offset", nodeInteractableYOffset.floatValue);
        
        PrefabUtility.RecordPrefabInstancePropertyModifications(nodeREF);
        serializedObject.ApplyModifiedProperties();
        Undo.RecordObject(nodeREF, "Modified Interactive Node");
        
        if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(nodeREF);
    }

    private object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }

        return obj;
    }
    private object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        //while (index-- >= 0)
        //    enm.MoveNext();
        //return enm.Current;

        for (var i = 0; i <= index; i++)
            if (!enm.MoveNext()) return null;
        return enm.Current;
    }
    
    private List<RPGCombatDATA.CombatVisualAnimation> DrawVisualAnimationsList(List<RPGCombatDATA.CombatVisualAnimation> visualAnimations)
    {
        if (GUILayout.Button("+ Add Visual Animation", GUILayout.ExpandWidth(true),
            GUILayout.Height(25)))
        {
            visualAnimations.Add(new RPGCombatDATA.CombatVisualAnimation());
        }

        GUIStyle xButtonStyle = new GUIStyle();
        xButtonStyle.normal.textColor = Color.red;
        xButtonStyle.fontSize = 18;
        
        for (var a = 0; a < visualAnimations.Count; a++)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            var requirementNumber = a + 1;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X", xButtonStyle, GUILayout.Width(20),
                GUILayout.Height(20)))
            {
                visualAnimations.RemoveAt(a);
                return visualAnimations;
            }

            EditorGUILayout.LabelField("Visual:" + requirementNumber + ":");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Activate On", GUILayout.Width(147.5f));
            visualAnimations[a].activationType =
                (RPGCombatDATA.CombatVisualActivationType) EditorGUILayout.EnumPopup(visualAnimations[a].activationType);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(147.5f));
            visualAnimations[a].parameterType =
                (RPGCombatDATA.CombatVisualAnimationParameterType) EditorGUILayout.EnumPopup(
                    visualAnimations[a].parameterType);
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();

            visualAnimations[a].animationParameter = EditorGUILayout.TextField("Animation Parameter", visualAnimations[a].animationParameter);

            switch (visualAnimations[a].parameterType)
            {
                case RPGCombatDATA.CombatVisualAnimationParameterType.Bool:
                    visualAnimations[a].boolValue =
                        EditorGUILayout.Toggle("Set True?", visualAnimations[a].boolValue);
                    visualAnimations[a].duration = EditorGUILayout.FloatField("Duration", visualAnimations[a].duration);
                    break;
                case RPGCombatDATA.CombatVisualAnimationParameterType.Int:
                    visualAnimations[a].intValue = EditorGUILayout.IntField("Value", visualAnimations[a].intValue);
                    break;
                case RPGCombatDATA.CombatVisualAnimationParameterType.Float:
                        visualAnimations[a].floatValue = EditorGUILayout.FloatField("Value", visualAnimations[a].floatValue);
                    break;
            }
            
            visualAnimations[a].delay = EditorGUILayout.FloatField("Delay", visualAnimations[a].delay);
            
            visualAnimations[a].showWeapons =
                EditorGUILayout.Toggle("Show Weapon?", visualAnimations[a].showWeapons);
            if (visualAnimations[a].showWeapons)
            {
                visualAnimations[a].showWeaponDuration =
                    EditorGUILayout.FloatField("Weapon Duration", visualAnimations[a].showWeaponDuration);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        return visualAnimations;
    }
    
}