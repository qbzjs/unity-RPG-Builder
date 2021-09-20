using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Managers;
using UnityEditor;
using UnityEngine;

public class RPGBAdvancedDialogueOptionsWindow : EditorWindow
{
    
    private static RPGBAdvancedDialogueOptionsWindow Instance;
    private GUISkin skin;
    private RPGBuilderEditorDATA editorDATA;
    private RPGBuilderEditorDATA.ThemeTypes cachedTheme;
    private Vector2 viewScrollPosition;

    public static RPGDialogueTextNode currentNode;
    private static int requirementIndex;
    private static int gameActionIndex;


    public static void AssignTextNodeRequirement(RPGDialogueTextNode textNode)
    {
        if (currentNode.RequirementList.Count >= requirementIndex + 1)
        {
            currentNode.RequirementList[requirementIndex].textNodeREF = textNode;
        }
        Instance.Repaint();
    }
    public static void AssignTextNodeGameAction(RPGDialogueTextNode textNode)
    {
        if (currentNode.GameActionsList.Count >= gameActionIndex + 1)
        {
            currentNode.GameActionsList[gameActionIndex].textNodeREF = textNode;
        }
        Instance.Repaint();
    }
    private void OnEnable()
    {
        Instance = this;
        editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        skin = Resources.Load<GUISkin>(editorDATA.RPGBEditorDataPath + "RPGBuilderSkin");
        cachedTheme = editorDATA.curEditorTheme;
    }

    private void OnDestroy()
    {
        currentNode = null;
        Instance = null;
    }

    private void OnDisable()
    {
        currentNode = null;
        Instance = null;
    }
    public static bool IsOpen => Instance != null;

    public void AssignCurrentDialogueNode(RPGDialogueTextNode dialogueNode)
    {
        currentNode = dialogueNode;
    }
    
    private void OnGUI()
    {
        DrawView();
    }

    private void DrawView()
    {

        if (currentNode == null)
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
        GUILayout.Space(13);
        GUILayout.BeginVertical();
        viewScrollPosition = GUILayout.BeginScrollView(viewScrollPosition, false, false, GUILayout.Width(width),
            GUILayout.Height(height));

        GUILayout.Space(10);
        GUILayout.Label(currentNode.message, skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));

        currentNode.hasRequirements =
            EditorGUILayout.ToggleLeft("Show Requirements", currentNode.hasRequirements, GUILayout.Width(300));
        if (currentNode.hasRequirements)
        {
            GUILayout.Space(10);
            GUILayout.Label("Requirements", skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));

            if (GUILayout.Button("+ Add Requirement", skin.GetStyle("AddButton"), GUILayout.Width(325),
                GUILayout.Height(25)))
                currentNode.RequirementList.Add(new RequirementsManager.RequirementDATA());

            for (var a = 0; a < currentNode.RequirementList.Count; a++)
            {
                GUILayout.Space(10);
                var requirementNumber = a + 1;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("X", skin.GetStyle("RemoveButton"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    currentNode.RequirementList.RemoveAt(a);
                    return;
                }

                var effectName = currentNode.RequirementList[a].requirementType.ToString();
                EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                    GUILayout.Width(300));
                EditorGUILayout.EndHorizontal();

                currentNode.RequirementList[a].requirementType =
                    (RequirementsManager.RequirementType) EditorGUILayout.EnumPopup("Type",
                        currentNode.RequirementList[a].requirementType, GUILayout.Width(300));

                switch (currentNode.RequirementList[a].requirementType)
                {
                    case RequirementsManager.RequirementType.pointSpent:
                        break;
                    case RequirementsManager.RequirementType.classLevel:
                        currentNode.RequirementList[a].classLevelValue = EditorGUILayout.IntField("Level",
                            currentNode.RequirementList[a].classLevelValue, GUILayout.Width(300));
                        break;
                    case RequirementsManager.RequirementType.skillLevel:
                        currentNode.RequirementList[a].skillRequiredREF =
                            (RPGSkill) EditorGUILayout.ObjectField(
                                "Skill", currentNode.RequirementList[a].skillRequiredREF,
                                typeof(RPGSkill), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].skillRequiredREF != null)
                        {
                            currentNode.RequirementList[a].skillRequiredID =
                                currentNode.RequirementList[a].skillRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].skillRequiredID = -1;
                        }

                        currentNode.RequirementList[a].classLevelValue = EditorGUILayout.IntField("Level",
                            currentNode.RequirementList[a].classLevelValue, GUILayout.Width(300));
                        break;
                    case RequirementsManager.RequirementType.itemOwned:
                        currentNode.RequirementList[a].itemRequiredREF = (RPGItem) EditorGUILayout.ObjectField(
                            "Item",
                            currentNode.RequirementList[a].itemRequiredREF,
                            typeof(RPGItem), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].itemRequiredREF != null)
                        {
                            currentNode.RequirementList[a].itemRequiredID =
                                currentNode.RequirementList[a].itemRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].itemRequiredID = -1;
                        }

                        currentNode.RequirementList[a].itemRequiredCount = EditorGUILayout.IntField("Count",
                            currentNode.RequirementList[a].itemRequiredCount, GUILayout.Width(300));
                        break;
                    case RequirementsManager.RequirementType.abilityKnown:
                    case RequirementsManager.RequirementType.abilityNotKnown:

                        currentNode.RequirementList[a].abilityRequiredREF =
                            (RPGAbility) EditorGUILayout.ObjectField(
                                "Ability", currentNode.RequirementList[a].abilityRequiredREF,
                                typeof(RPGAbility), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].abilityRequiredREF != null)
                        {
                            currentNode.RequirementList[a].abilityRequiredID =
                                currentNode.RequirementList[a].abilityRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].abilityRequiredID = -1;
                        }

                        break;
                    case RequirementsManager.RequirementType.recipeKnown:
                    case RequirementsManager.RequirementType.recipeNotKnown:
                        currentNode.RequirementList[a].recipeRequiredREF =
                            (RPGCraftingRecipe) EditorGUILayout.ObjectField("Recipe",
                                currentNode.RequirementList[a].recipeRequiredREF,
                                typeof(RPGCraftingRecipe), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].recipeRequiredREF != null)
                        {
                            currentNode.RequirementList[a].craftingRecipeRequiredID =
                                currentNode.RequirementList[a].recipeRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].craftingRecipeRequiredID = -1;
                        }

                        break;
                    case RequirementsManager.RequirementType.resourceNodeKnown:
                    case RequirementsManager.RequirementType.resourceNodeNotKnown:
                        currentNode.RequirementList[a].resourceNodeRequiredREF =
                            (RPGResourceNode) EditorGUILayout.ObjectField("Resource Node",
                                currentNode.RequirementList[a].resourceNodeRequiredREF,
                                typeof(RPGResourceNode), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].resourceNodeRequiredREF != null)
                        {
                            currentNode.RequirementList[a].resourceNodeRequiredID =
                                currentNode.RequirementList[a].resourceNodeRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].resourceNodeRequiredID = -1;
                        }

                        break;
                    case RequirementsManager.RequirementType.race:
                        currentNode.RequirementList[a].raceRequiredREF =
                            (RPGRace) EditorGUILayout.ObjectField("Race",
                                currentNode.RequirementList[a].raceRequiredREF,
                                typeof(RPGRace), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].raceRequiredREF != null)
                        {
                            currentNode.RequirementList[a].raceRequiredID =
                                currentNode.RequirementList[a].raceRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].raceRequiredID = -1;
                        }

                        break;
                    case RequirementsManager.RequirementType.questState:
                        currentNode.RequirementList[a].questRequiredREF =
                            (RPGQuest) EditorGUILayout.ObjectField(
                                "Quest", currentNode.RequirementList[a].questRequiredREF,
                                typeof(RPGQuest), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].questRequiredREF != null)
                        {
                            currentNode.RequirementList[a].questRequiredID =
                                currentNode.RequirementList[a].questRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].questRequiredID = -1;
                        }

                        currentNode.RequirementList[a].questStateRequired =
                            (QuestManager.questState) EditorGUILayout.EnumPopup(
                                new GUIContent("State", "The required state of the quest"),
                                currentNode.RequirementList[a].questStateRequired, GUILayout.Width(300));
                        break;
                    case RequirementsManager.RequirementType.npcKilled:
                        currentNode.RequirementList[a].npcRequiredREF = (RPGNpc) EditorGUILayout.ObjectField(
                            new GUIContent("NPC", "The NPC required to be killed"),
                            currentNode.RequirementList[a].npcRequiredREF, typeof(RPGNpc), false,
                            GUILayout.Width(300));
                        currentNode.RequirementList[a].npcKillsRequired = EditorGUILayout.IntField(
                            new GUIContent("Kills",
                                "How many times this NPC should have been killed for the bonus to be active"),
                            currentNode.RequirementList[a].npcKillsRequired, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].npcRequiredREF != null)
                            currentNode.RequirementList[a].npcRequiredID =
                                currentNode.RequirementList[a].npcRequiredREF.ID;
                        else
                            currentNode.RequirementList[a].npcRequiredID = -1;
                        break;
                    case RequirementsManager.RequirementType.bonusKnown:
                    case RequirementsManager.RequirementType.bonusNotKnown:
                        currentNode.RequirementList[a].bonusRequiredREF = (RPGBonus) EditorGUILayout.ObjectField(
                            "Bonus", currentNode.RequirementList[a].bonusRequiredREF,
                            typeof(RPGBonus), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].bonusRequiredREF != null)
                        {
                            currentNode.RequirementList[a].bonusRequiredID =
                                currentNode.RequirementList[a].bonusRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].bonusRequiredID = -1;
                        }

                        break;
                    case RequirementsManager.RequirementType._class:
                        currentNode.RequirementList[a].classRequiredREF =
                            (RPGClass) EditorGUILayout.ObjectField("Class",
                                currentNode.RequirementList[a].classRequiredREF,
                                typeof(RPGClass), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].classRequiredREF != null)
                        {
                            currentNode.RequirementList[a].classRequiredID =
                                currentNode.RequirementList[a].classRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].classRequiredID = -1;
                        }

                        break;
                    case RequirementsManager.RequirementType.weaponTemplateLevel:
                        currentNode.RequirementList[a].weaponTemplateRequiredREF =
                            (RPGWeaponTemplate) EditorGUILayout.ObjectField(
                                "Weapon Template", currentNode.RequirementList[a].weaponTemplateRequiredREF,
                                typeof(RPGWeaponTemplate), false, GUILayout.Width(300));
                        if (currentNode.RequirementList[a].weaponTemplateRequiredREF != null)
                        {
                            currentNode.RequirementList[a].weaponTemplateRequiredID =
                                currentNode.RequirementList[a].weaponTemplateRequiredREF.ID;
                        }
                        else
                        {
                            currentNode.RequirementList[a].weaponTemplateRequiredID = -1;
                        }

                        currentNode.RequirementList[a].weaponTemplateLevelValue = EditorGUILayout.IntField("Level",
                            currentNode.RequirementList[a].weaponTemplateLevelValue, GUILayout.Width(300));
                        break;
                    case RequirementsManager.RequirementType.dialogueLineCompleted:
                    case RequirementsManager.RequirementType.dialogueLineNotCompleted:
                        EditorGUILayout.BeginVertical();
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.LabelField(currentNode.RequirementList[a].textNodeREF != null ? currentNode.RequirementList[a].textNodeREF.message : "- Assign a Node -");
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Node", GUILayout.Width(50), GUILayout.Height(15));
                        currentNode.RequirementList[a].textNodeREF =
                            (RPGDialogueTextNode) EditorGUILayout.ObjectField(currentNode.RequirementList[a].textNodeREF,
                                typeof(RPGDialogueTextNode), false, GUILayout.Width(125));
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUI.EndDisabledGroup();
                        
                        if (GUILayout.Button("Select", skin.GetStyle("AddButton"), GUILayout.Width(125),
                            GUILayout.Height(18)))
                        {
                            if (RPGBAdvancedDialogueOptionsNodeSelector.currentGraph == currentNode.graph)
                            {
                                return;
                            }

                            requirementIndex = a;

                            if (RPGBAdvancedDialogueOptionsNodeSelector.IsOpen)
                            {
                                GetWindow(typeof(RPGBAdvancedDialogueOptionsNodeSelector)).Close();

                                var window = (RPGBAdvancedDialogueOptionsNodeSelector) EditorWindow.GetWindow(
                                    typeof(RPGBAdvancedDialogueOptionsNodeSelector), false, "Selector");
                                window.minSize = new Vector2(350, 500);
                                GUI.contentColor = Color.white;
                                window.Show();
                                window.InitSelector((RPGDialogueGraph) currentNode.graph, RPGBAdvancedDialogueOptionsNodeSelector.selectorType.requirement);
                            }
                            else
                            {
                                var window = (RPGBAdvancedDialogueOptionsNodeSelector) EditorWindow.GetWindow(
                                    typeof(RPGBAdvancedDialogueOptionsNodeSelector), false,
                                    "Advanced Dialogue Node Options");
                                window.minSize = new Vector2(350, 500);
                                GUI.contentColor = Color.white;
                                window.Show();
                                window.InitSelector((RPGDialogueGraph) currentNode.graph, RPGBAdvancedDialogueOptionsNodeSelector.selectorType.requirement);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                }
            }

            GUILayout.Space(10);
        }

        GUILayout.Box(editorDATA.gearSetsSeparator, skin.GetStyle("CustomImage"), GUILayout.Width(450),
            GUILayout.Height(10));
        currentNode.hasGameActions =
            EditorGUILayout.ToggleLeft("Show Actions", currentNode.hasGameActions, GUILayout.Width(300));

        if (currentNode.hasGameActions)
        {
            GUILayout.Space(10);
            GUILayout.Label("Game Actions", skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));

            if (GUILayout.Button("+ Add Game Action", skin.GetStyle("AddButton"), GUILayout.Width(325),
                GUILayout.Height(25)))
                currentNode.GameActionsList.Add(new RPGBGameActions());

            for (var a = 0; a < currentNode.GameActionsList.Count; a++)
            {
                GUILayout.Space(10);
                var requirementNumber = a + 1;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("X", skin.GetStyle("RemoveButton"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    currentNode.GameActionsList.RemoveAt(a);
                    return;
                }

                var effectName = currentNode.GameActionsList[a].actionType.ToString();
                EditorGUILayout.LabelField("" + requirementNumber + ": " + effectName,
                    GUILayout.Width(300));
                EditorGUILayout.EndHorizontal();

                currentNode.GameActionsList[a].actionType = (ActionType) EditorGUILayout.EnumPopup("Type",
                    currentNode.GameActionsList[a].actionType, GUILayout.Width(300));

                switch (currentNode.GameActionsList[a].actionType)
                {
                    case ActionType.UseAbility:
                    case ActionType.LearnAbility:
                        currentNode.GameActionsList[a].abilityREF = (RPGAbility) EditorGUILayout.ObjectField(
                            "Ability", currentNode.GameActionsList[a].abilityREF,
                            typeof(RPGAbility), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].abilityREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].abilityREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        break;
                    case ActionType.ApplyEffect:
                        currentNode.GameActionsList[a].effectREF = (RPGEffect) EditorGUILayout.ObjectField(
                            "Effect", currentNode.GameActionsList[a].effectREF,
                            typeof(RPGEffect), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].effectREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].effectREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        int effectRankField = currentNode.GameActionsList[a].effectRank + 1;
                        if (effectRankField == 0) effectRankField = 1;

                        if (currentNode.GameActionsList[a].effectREF != null)
                        {
                            if (effectRankField > currentNode.GameActionsList[a].effectREF.ranks.Count)
                            {
                                effectRankField = currentNode.GameActionsList[a].effectREF.ranks.Count;
                            }
                        }

                        effectRankField = EditorGUILayout.IntField("Effect Rank",
                            effectRankField, GUILayout.Width(300));
                        currentNode.GameActionsList[a].effectRank = effectRankField - 1;

                        currentNode.GameActionsList[a].target =
                            (RPGCombatDATA.TARGET_TYPE) EditorGUILayout.EnumPopup("Apply On",
                                currentNode.GameActionsList[a].target,
                                GUILayout.Width(300));
                        break;
                    case ActionType.GainItem:
                    case ActionType.RemoveItem:
                        currentNode.GameActionsList[a].itemREF = (RPGItem) EditorGUILayout.ObjectField("Item",
                            currentNode.GameActionsList[a].itemREF,
                            typeof(RPGItem), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].itemREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].itemREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Count",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.LearnRecipe:
                        currentNode.GameActionsList[a].craftingRecipeREF =
                            (RPGCraftingRecipe) EditorGUILayout.ObjectField("Recipe",
                                currentNode.GameActionsList[a].craftingRecipeREF,
                                typeof(RPGCraftingRecipe), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].craftingRecipeREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].craftingRecipeREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        break;
                    case ActionType.LearnResourceNode:
                        currentNode.GameActionsList[a].resourceNodeREF =
                            (RPGResourceNode) EditorGUILayout.ObjectField("Resource Node",
                                currentNode.GameActionsList[a].resourceNodeREF,
                                typeof(RPGResourceNode), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].resourceNodeREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].resourceNodeREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        break;
                    case ActionType.LearnBonus:
                        currentNode.GameActionsList[a].bonusREF = (RPGBonus) EditorGUILayout.ObjectField(
                            "Bonus", currentNode.GameActionsList[a].bonusREF,
                            typeof(RPGBonus), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].bonusREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].bonusREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        break;
                    case ActionType.LearnSkill:
                        currentNode.GameActionsList[a].skillREF = (RPGSkill) EditorGUILayout.ObjectField(
                            "Skill", currentNode.GameActionsList[a].skillREF,
                            typeof(RPGSkill), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].skillREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].skillREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        break;
                    case ActionType.LearnTalentTree:
                        currentNode.GameActionsList[a].talentTreeREF =
                            (RPGTalentTree) EditorGUILayout.ObjectField("Talent Tree",
                                currentNode.GameActionsList[a].talentTreeREF,
                                typeof(RPGTalentTree), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].talentTreeREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].talentTreeREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        break;
                    case ActionType.GainTreePoint:
                    case ActionType.LoseTreePoint:
                        currentNode.GameActionsList[a].treePointREF =
                            (RPGTreePoint) EditorGUILayout.ObjectField("Tree Point",
                                currentNode.GameActionsList[a].treePointREF,
                                typeof(RPGTreePoint), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].treePointREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].treePointREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Amount",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.GainEXP:
                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Amount",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.GainSkillLevel:
                    case ActionType.GainSkillEXP:
                        currentNode.GameActionsList[a].skillREF = (RPGSkill) EditorGUILayout.ObjectField(
                            "Skill", currentNode.GameActionsList[a].skillREF,
                            typeof(RPGSkill), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].skillREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].skillREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Amount",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.GainWeaponTemplateEXP:
                        currentNode.GameActionsList[a].weaponTemplateREF =
                            (RPGWeaponTemplate) EditorGUILayout.ObjectField("Weapon Template",
                                currentNode.GameActionsList[a].weaponTemplateREF,
                                typeof(RPGWeaponTemplate), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].weaponTemplateREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].weaponTemplateREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Amount",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.GainLevel:
                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Amount",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.ProposeQuest:
                        currentNode.GameActionsList[a].questREF = (RPGQuest) EditorGUILayout.ObjectField(
                            "Quest", currentNode.GameActionsList[a].questREF,
                            typeof(RPGQuest), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].questREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].questREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        break;
                    case ActionType.GainCurrency:
                    case ActionType.LoseCurrency:
                        currentNode.GameActionsList[a].currencyREF = (RPGCurrency) EditorGUILayout.ObjectField(
                            "Currency", currentNode.GameActionsList[a].currencyREF,
                            typeof(RPGCurrency), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].currencyREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].currencyREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Amount",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.SpawnGameObject:
                        currentNode.GameActionsList[a].prefab = (GameObject) EditorGUILayout.ObjectField(
                            "Game Object Prefab", currentNode.GameActionsList[a].prefab,
                            typeof(GameObject), false, GUILayout.Width(300));
                        currentNode.GameActionsList[a].spawnPosition = EditorGUILayout.Vector3Field("Position",
                            currentNode.GameActionsList[a].spawnPosition, GUILayout.Width(300));
                        break;
                    case ActionType.DestroyGameObject:
                        currentNode.GameActionsList[a].GameObjectName = EditorGUILayout.TextField(
                            "Game Object Name",
                            currentNode.GameActionsList[a].GameObjectName, GUILayout.Width(300));
                        break;
                    case ActionType.ToggleWorldNode:
                        currentNode.GameActionsList[a].GameObjectName = EditorGUILayout.TextField(
                            "World Node Name",
                            currentNode.GameActionsList[a].GameObjectName, GUILayout.Width(300));
                        break;
                    case ActionType.PlayAnimation:
                        currentNode.GameActionsList[a].animationName = EditorGUILayout.TextField(
                            "Animation Parameter Name",
                            currentNode.GameActionsList[a].animationName, GUILayout.Width(300));
                        break;
                    case ActionType.ActivateGameObject:
                        currentNode.GameActionsList[a].GameObjectName = EditorGUILayout.TextField(
                            "Game Object Name",
                            currentNode.GameActionsList[a].GameObjectName, GUILayout.Width(300));
                        break;
                    case ActionType.DeactiveGameObject:
                        currentNode.GameActionsList[a].GameObjectName = EditorGUILayout.TextField(
                            "Game Object Name",
                            currentNode.GameActionsList[a].GameObjectName, GUILayout.Width(300));
                        break;
                    case ActionType.GainFactionpoints:
                    case ActionType.LoseFactionPoints:
                        currentNode.GameActionsList[a].factionREF = (RPGFaction) EditorGUILayout.ObjectField(
                            "Faction", currentNode.GameActionsList[a].factionREF,
                            typeof(RPGFaction), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].factionREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].factionREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }

                        currentNode.GameActionsList[a].count = EditorGUILayout.IntField("Amount",
                            currentNode.GameActionsList[a].count, GUILayout.Width(300));
                        break;
                    case ActionType.PlaySound:
                        currentNode.GameActionsList[a].audioClip = (AudioClip) EditorGUILayout.ObjectField(
                            "Audio Clip", currentNode.GameActionsList[a].audioClip,
                            typeof(AudioClip), false, GUILayout.Width(300));
                        break;
                    case ActionType.Teleport:
                        currentNode.GameActionsList[a].teleportType =
                                        (TELEPORT_TYPE) EditorGUILayout.EnumPopup(
                                            new GUIContent("Type", "What type of teleport is it?"),
                                            currentNode.GameActionsList[a].teleportType);
                        switch (currentNode.GameActionsList[a].teleportType)
                        {
                            case TELEPORT_TYPE.gameScene:
                            {
                                currentNode.GameActionsList[a].gameSceneREF =
                                    (RPGGameScene) EditorGUILayout.ObjectField(
                                        new GUIContent("Game Scene", "The game scene to teleport to"),
                                        currentNode.GameActionsList[a].gameSceneREF, typeof(RPGGameScene), false);
                                if (currentNode.GameActionsList[a].gameSceneREF != null)
                                    currentNode.GameActionsList[a].assetID =
                                        currentNode.GameActionsList[a].gameSceneREF.ID;
                                else
                                    currentNode.GameActionsList[a].assetID = -1;
                                currentNode.GameActionsList[a].spawnPosition = EditorGUILayout.Vector3Field(
                                    new GUIContent("Location", "Position coordinates to teleport to"),
                                    currentNode.GameActionsList[a].spawnPosition);
                                break;
                            }
                            case TELEPORT_TYPE.position:
                                currentNode.GameActionsList[a].spawnPosition = EditorGUILayout.Vector3Field(
                                    new GUIContent("Location", "Position coordinates to teleport to"),
                                    currentNode.GameActionsList[a].spawnPosition);
                                break;
                        }

                        break;
                    case ActionType.SaveCharacterData:
                        break;
                    case ActionType.RemoveEffect:
                        currentNode.GameActionsList[a].effectREF = (RPGEffect) EditorGUILayout.ObjectField(
                            "Effect", currentNode.GameActionsList[a].effectREF,
                            typeof(RPGEffect), false, GUILayout.Width(300));
                        if (currentNode.GameActionsList[a].effectREF != null)
                        {
                            currentNode.GameActionsList[a].assetID =
                                currentNode.GameActionsList[a].effectREF.ID;
                        }
                        else
                        {
                            currentNode.GameActionsList[a].assetID = -1;
                        }
                        break;
                    
                    case ActionType.CompleteDialogueLine:
                        
                        EditorGUILayout.BeginVertical();
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.LabelField(currentNode.GameActionsList[a].textNodeREF != null ? currentNode.GameActionsList[a].textNodeREF.message : "- Assign a Node -");
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Node", GUILayout.Width(50), GUILayout.Height(15));
                        currentNode.GameActionsList[a].textNodeREF =
                            (RPGDialogueTextNode) EditorGUILayout.ObjectField(currentNode.GameActionsList[a].textNodeREF,
                                typeof(RPGDialogueTextNode), false, GUILayout.Width(125));
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUI.EndDisabledGroup();
                        
                        if (GUILayout.Button("Select", skin.GetStyle("AddButton"), GUILayout.Width(125),
                            GUILayout.Height(18)))
                        {
                            if (RPGBAdvancedDialogueOptionsNodeSelector.currentGraph == currentNode.graph)
                            {
                                return;
                            }

                            gameActionIndex = a;

                            if (RPGBAdvancedDialogueOptionsNodeSelector.IsOpen)
                            {
                                GetWindow(typeof(RPGBAdvancedDialogueOptionsNodeSelector)).Close();

                                var window = (RPGBAdvancedDialogueOptionsNodeSelector) EditorWindow.GetWindow(
                                    typeof(RPGBAdvancedDialogueOptionsNodeSelector), false, "Selector");
                                window.minSize = new Vector2(350, 500);
                                GUI.contentColor = Color.white;
                                window.Show();
                                window.InitSelector((RPGDialogueGraph) currentNode.graph, RPGBAdvancedDialogueOptionsNodeSelector.selectorType.gameAction);
                            }
                            else
                            {
                                var window = (RPGBAdvancedDialogueOptionsNodeSelector) EditorWindow.GetWindow(
                                    typeof(RPGBAdvancedDialogueOptionsNodeSelector), false,
                                    "Advanced Dialogue Node Options");
                                window.minSize = new Vector2(350, 500);
                                GUI.contentColor = Color.white;
                                window.Show();
                                window.InitSelector((RPGDialogueGraph) currentNode.graph, RPGBAdvancedDialogueOptionsNodeSelector.selectorType.gameAction);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                currentNode.GameActionsList[a].chance =
                    EditorGUILayout.Slider("Chance", currentNode.GameActionsList[a].chance,
                        0f, 100f, GUILayout.Width(300));

                GUILayout.Space(10);
            }
        }

        GUILayout.Box(editorDATA.gearSetsSeparator, skin.GetStyle("CustomImage"), GUILayout.Width(450),
            GUILayout.Height(10));
        currentNode.showSettings =
            EditorGUILayout.ToggleLeft("Show Settings?", currentNode.showSettings, GUILayout.Width(300));
        if (currentNode.showSettings)
        {
            
            GUILayout.Space(10);
            GUILayout.Label("Interaction Settings", skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));
            GUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Viewed Endlessly?", GUILayout.Width(150));
            GUILayout.Space(5);
            currentNode.viewedEndless = EditorGUILayout.Toggle(currentNode.viewedEndless, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            if (!currentNode.viewedEndless)
            {
                currentNode.viewCountMax = EditorGUILayout.IntField("View Times",
                    currentNode.viewCountMax, GUILayout.Width(300));
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Clicked Endlessly?", GUILayout.Width(150));
            GUILayout.Space(5);
            currentNode.clickedEndless = EditorGUILayout.Toggle(currentNode.clickedEndless, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            if (!currentNode.clickedEndless)
            {
                currentNode.clickCountMax = EditorGUILayout.IntField("Click Times",
                    currentNode.clickCountMax, GUILayout.Width(300));
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Visual Settings", skin.GetStyle("ViewTitle"), GUILayout.Width(325), GUILayout.Height(40));
            GUILayout.Space(5);
            currentNode.nodeImage = (Sprite)EditorGUILayout.ObjectField("Image", currentNode.nodeImage, typeof(Sprite), false, GUILayout.Width(300), GUILayout.Height(300));
        }
        GUILayout.Box(editorDATA.gearSetsSeparator, skin.GetStyle("CustomImage"), GUILayout.Width(450), GUILayout.Height(10));


        GUI.color = guiColor;

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
}
