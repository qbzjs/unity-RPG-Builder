using System;
using System.Collections.Generic;
using BLINK.RPGBuilder;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class RPGBuilderUtilities
{
    public static void EnableCG(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public static void DisableCG(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
    
    public static void PlaySound (GameObject casterGO, GameObject effectGO, AudioClip clip, bool attachToEffect)
    {
        if (clip == null) return;
        GameObject goTarget = attachToEffect ? effectGO : casterGO;
        var aSource = goTarget.GetComponent<AudioSource>();
        if (aSource == null)
        {
            aSource = goTarget.AddComponent<AudioSource>();
        }
        aSource.spatialBlend = 1;
        aSource.volume = 0.55f;
        aSource.maxDistance = 20;
        aSource.PlayOneShot(clip);
    }

    public static float StatAffectsBodyScale(RPGStat stat)
    {
        float totalBodyScaleModifier = 0;
        foreach (var bonus in stat.statBonuses)
        {
            if (bonus.statType == RPGStat.STAT_TYPE.BODY_SCALE)
            {
                totalBodyScaleModifier += bonus.modifyValue;
            }
        }
        
        return totalBodyScaleModifier;
    }
    public static bool IsPointerOverUIObject()
    {
        var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        var results = new List<RaycastResult>();
        if (EventSystem.current == null) return false;
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public static float[] vector3ToFloatArray(Vector3 vector3)
    {
        return new float[] {vector3.x, vector3.y, vector3.z};
    }

    public static Vector3 floatArrayToVector3(float[] floatArray)
    {
        return new Vector3(floatArray[0], floatArray[1], floatArray[2]);
    }
    
    public static string GetKeybindText(KeyCode key)
    {
        var KeyBindString = key.ToString();
        if (KeyBindString.Contains("Alpha"))
        {
            return KeyBindString.Remove(0, 5);
        }
        if (KeyBindString.Contains("Mouse"))
        {
            return "M" + KeyBindString.Remove(0, 5);
        }

        return KeyBindString;
    }

    public static KeyCode GetAbilityKey(int abilityID)
    {
        for (var index = 0; index < ActionBarManager.Instance.actionBarSlots.Count; index++)
        {
            if(ActionBarManager.Instance.actionBarSlots[index].contentType != CharacterData.ActionBarSlotContentType.Ability) continue;
            var actionAb = ActionBarManager.Instance.actionBarSlots[index];
            if (actionAb.ThisAbility == null) continue;
            if (actionAb.ThisAbility.ID != abilityID) continue;
            int abilitySlotNumber = index + 1;
            return GetCurrentKeyByActionKeyName("ACTION_BAR_SLOT_" + abilitySlotNumber);
        }

        return KeyCode.None;
    }

    public static int IsComboActive(CombatNode cbtNode, int comboID, int index)
    {
        for (var i = 0; i < cbtNode.activeCombos.Count; i++)
        {
            var combo = cbtNode.activeCombos[i];
            if (combo.combo.ID != comboID) continue;
            if (combo.comboIndex != index) continue;
            return i;
        }

        return -1;
    }

    public static void SetAbilityComboActive(int abilityID, bool active)
    {
        foreach (var ab in CharacterData.Instance.abilitiesData)
        {
            if(ab.ID != abilityID) continue;
            ab.comboActive = active;
        }
    }

    public static bool IsAbilityInCombo(int abilityID)
    {
        foreach (var ab in CharacterData.Instance.abilitiesData)
        {
            if(ab.ID != abilityID) continue;
            return ab.comboActive;
        }

        return false;
    }

    public static KeyCode GetActionKeyCodeByName(string actionKeyName)
    {
        foreach (var actionKey in RPGBuilderEssentials.Instance.generalSettings.actionKeys)
        {
            if(actionKey.actionName != actionKeyName) continue;
            return actionKey.defaultKey;
        }

        return KeyCode.None;
    }
    
    public static KeyCode GetCurrentKeyByActionKeyName(string actionKeyName)
    {
        foreach (var actionKey in CharacterData.Instance.actionKeys)
        {
            if(actionKey.actionKeyName != actionKeyName) continue;
            return actionKey.currentKey;
        }

        return KeyCode.None;
    }

    public static RPGCurrency getCurrencyByName(string name)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allCurrencies)
            if (t._name == name)
                return t;

        return null;
    }

    public static RPGTreePoint getTreePointByName(string name)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
            if (t._name == name)
                return t;

        return null;
    }

    public static RPGFaction getFactionByName(string name)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allFactions)
            if (t._name == name)
                return t;

        return null;
    }

    public static RPGSkill getSkillByName(string name)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allSkills)
            if (t._name == name)
                return t;

        return null;
    }

    public static RPGWeaponTemplate getWeaponTemplateByName(string name)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allWeaponTemplates)
            if (t._name == name)
                return t;

        return null;
    }

    
    public static int getWeaponTemplateLevel(int ID, RPGSpellbook spellbook)
    {
        foreach (var t in CharacterData.Instance.weaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            foreach (var t1 in GetWeaponTemplateFromID(t.weaponTemplateID).spellbooks)
            {
                if (t1.spellbookID != spellbook.ID) continue;
                return t.currentWeaponLevel;
            }
        }

        return -1;
    }
    public static int getWeaponTemplateLevel(int ID)
    {
        foreach (var t in CharacterData.Instance.weaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return t.currentWeaponLevel;
        }

        return -1;
    }
    public static int getWeaponTemplateMaxLevel(int ID)
    {
        foreach (var t in CharacterData.Instance.weaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return GetLevelTemplateFromID(GetWeaponTemplateFromID(t.weaponTemplateID).levelTemplateID).levels;
        }

        return -1;
    }
    public static int getWeaponTemplateCurEXP(int ID)
    {
        foreach (var t in CharacterData.Instance.weaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return t.currentWeaponXP;
        }

        return -1;
    }
    public static int getWeaponTemplateMaxEXP(int ID)
    {
        foreach (var t in CharacterData.Instance.weaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return t.maxWeaponXP;
        }

        return -1;
    }
    public static int getWeaponTemplateIndexFromID(int ID)
    {
        for (var index = 0; index < CharacterData.Instance.weaponTemplates.Count; index++)
        {
            var t = CharacterData.Instance.weaponTemplates[index];
            if (t.weaponTemplateID != ID) continue;
            return index;
        }

        return -1;
    }

    private static Object GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID _assetType, int ID)
    {
        switch (_assetType)
        {
            case AssetIDHandler.ASSET_TYPE_ID.ability:
                foreach (var t in RPGBuilderEssentials.Instance.allAbilities)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.effect:
                foreach (var t in RPGBuilderEssentials.Instance.allEffects)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.npc:
                foreach (var t in RPGBuilderEssentials.Instance.allNPCs)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.stat:
                foreach (var t in RPGBuilderEssentials.Instance.allStats)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.treePoint:
                foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.item:
                foreach (var t in RPGBuilderEssentials.Instance.allItems)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.skill:
                foreach (var t in RPGBuilderEssentials.Instance.allSkills)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.levelTemplate:
                foreach (var t in RPGBuilderEssentials.Instance.allLevelTemplates)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.race:
                foreach (var t in RPGBuilderEssentials.Instance.allRaces)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID._class:
                foreach (var t in RPGBuilderEssentials.Instance.allClasses)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.lootTable:
                foreach (var t in RPGBuilderEssentials.Instance.allLootTables)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.merchantTable:
                foreach (var t in RPGBuilderEssentials.Instance.allMerchantTables)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.currency:
                foreach (var t in RPGBuilderEssentials.Instance.allCurrencies)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.craftingRecipe:
                foreach (var t in RPGBuilderEssentials.Instance.allCraftingRecipes)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.craftingStation:
                foreach (var t in RPGBuilderEssentials.Instance.allCraftingStation)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.talentTree:
                foreach (var t in RPGBuilderEssentials.Instance.allTalentTrees)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.bonus:
                foreach (var t in RPGBuilderEssentials.Instance.allBonuses)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.task:
                foreach (var t in RPGBuilderEssentials.Instance.allTasks)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.quest:
                foreach (var t in RPGBuilderEssentials.Instance.allQuests)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.worldPosition:
                foreach (var t in RPGBuilderEssentials.Instance.allWorldPositions)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.resourceNode:
                foreach (var t in RPGBuilderEssentials.Instance.allResourceNodes)
                    if (t.ID == ID)
                        return t;

                return null;

            case AssetIDHandler.ASSET_TYPE_ID.gameScene:
                foreach (var t in RPGBuilderEssentials.Instance.allGameScenes)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.gearSet:
                foreach (var t in RPGBuilderEssentials.Instance.allGearSets)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.enchantment:
                foreach (var t in RPGBuilderEssentials.Instance.allEnchantments)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.spellbook:
                foreach (var t in RPGBuilderEssentials.Instance.allSpellbooks)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.faction:
                foreach (var t in RPGBuilderEssentials.Instance.allFactions)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.weaponTemplate:
                foreach (var t in RPGBuilderEssentials.Instance.allWeaponTemplates)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.dialogue:
                foreach (var t in RPGBuilderEssentials.Instance.allDialogues)
                    if (t.ID == ID)
                        return t;
                return null;
            case AssetIDHandler.ASSET_TYPE_ID.gameModifier:
                foreach (var t in RPGBuilderEssentials.Instance.allGameModifiers)
                    if (t.ID == ID)
                        return t;

                return null;
            case AssetIDHandler.ASSET_TYPE_ID.species:
                foreach (var t in RPGBuilderEssentials.Instance.allSpecies)
                    if (t.ID == ID)
                        return t;
                return null;
            case AssetIDHandler.ASSET_TYPE_ID.combo:
                foreach (var t in RPGBuilderEssentials.Instance.allCombos)
                    if (t.ID == ID)
                        return t;
                return null;
            default: return null;
        }
    }

    public static RPGGameScene GetGameSceneFromName(string sceneName)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allGameScenes)
            if (t._name == sceneName)
                return t;

        return null;
    }

    public static RPGGameScene GetGameSceneFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.gameScene, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGGameScene) gameElement;
        return thisElementREF;
    }

    public static RPGFaction GetFactionFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.faction, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGFaction) gameElement;
        return thisElementREF;
    }

    public static RPGAbility GetAbilityFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.ability, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGAbility) gameElement;
        return thisElementREF;
    }

    public static RPGEffect GetEffectFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.effect, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGEffect) gameElement;
        return thisElementREF;
    }

    public static RPGNpc GetNPCFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.npc, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGNpc) gameElement;
        return thisElementREF;
    }

    public static RPGSpecies GetSpeciesFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.species, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGSpecies) gameElement;
        return thisElementREF;
    }
    public static RPGCombo GetComboFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.combo, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGCombo) gameElement;
        return thisElementREF;
    }

    public static RPGStat GetStatFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.stat, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGStat) gameElement;
        return thisElementREF;
    }

    public static RPGDialogue GetDialogueFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.dialogue, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGDialogue) gameElement;
        return thisElementREF;
    }

    public static RPGGameModifier GetGameModifierFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.gameModifier, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGGameModifier) gameElement;
        return thisElementREF;
    }
    
    public static RPGTreePoint GetTreePointFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.treePoint, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGTreePoint) gameElement;
        return thisElementREF;
    }

    public static RPGItem GetItemFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.item, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGItem) gameElement;
        return thisElementREF;
    }

    public static RPGEnchantment GetEnchantmentFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.enchantment, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGEnchantment) gameElement;
        return thisElementREF;
    }

    public static RPGItem GetItemFromName(string itemName)
    {
        foreach (RPGItem t in RPGBuilderEssentials.Instance.allItems)
        {
            if (t._name == itemName) return t;
        }

        return null;
    }

    public static RPGSkill GetSkillFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.skill, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGSkill) gameElement;
        return thisElementREF;
    }

    public static RPGWeaponTemplate GetWeaponTemplateFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.weaponTemplate, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGWeaponTemplate) gameElement;
        return thisElementREF;
    }

    public static RPGLevelsTemplate GetLevelTemplateFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.levelTemplate, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGLevelsTemplate) gameElement;
        return thisElementREF;
    }

    public static RPGClass GetClassFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID._class, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGClass) gameElement;
        return thisElementREF;
    }

    public static RPGSpellbook GetSpellbookFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.spellbook, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGSpellbook) gameElement;
        return thisElementREF;
    }

    public static RPGRace GetRaceFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.race, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGRace) gameElement;
        return thisElementREF;
    }

    public static RPGLootTable GetLootTableFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.lootTable, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGLootTable) gameElement;
        return thisElementREF;
    }

    public static RPGMerchantTable GetMerchantTableFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.merchantTable, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGMerchantTable) gameElement;
        return thisElementREF;
    }

    public static RPGCurrency GetCurrencyFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.currency, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGCurrency) gameElement;
        return thisElementREF;
    }

    public static RPGCraftingRecipe GetCraftingRecipeFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.craftingRecipe, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGCraftingRecipe) gameElement;
        return thisElementREF;
    }

    public static RPGCraftingStation GetCraftingStationFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.craftingStation, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGCraftingStation) gameElement;
        return thisElementREF;
    }

    public static RPGTalentTree GetTalentTreeFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.talentTree, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGTalentTree) gameElement;
        return thisElementREF;
    }

    public static RPGBonus GetBonusFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.bonus, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGBonus) gameElement;
        return thisElementREF;
    }

    public static RPGGearSet GetGearSetFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.gearSet, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGGearSet) gameElement;
        return thisElementREF;
    }

    public static RPGTask GetTaskFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.task, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGTask) gameElement;
        return thisElementREF;
    }

    public static RPGQuest GetQuestFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.quest, ID);
        if (gameElement != null)
        {
            var thisElementREF = (RPGQuest) gameElement;
            return thisElementREF;
        }

        return null;
    }

    public static RPGWorldPosition GetWorldPositionFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.worldPosition, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGWorldPosition) gameElement;
        return thisElementREF;
    }

    public static RPGResourceNode GetResourceNodeFromID(int ID)
    {
        var gameElement = GetGameElementDATA(AssetIDHandler.ASSET_TYPE_ID.resourceNode, ID);
        if (gameElement == null) return null;
        var thisElementREF = (RPGResourceNode) gameElement;
        return thisElementREF;
    }

    public static RPGAbility GetAbilityFromIDEditor(int ID, List<RPGAbility> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGNpc GetNPCFromIDEditor(int ID, List<RPGNpc> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGTask GetTaskFromIDEditor(int ID, List<RPGTask> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGGameScene GetGameSceneFromIDEditor(int ID, List<RPGGameScene> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGFaction GetFactionFromIDEditor(int ID, List<RPGFaction> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGWorldPosition GetWorldPositionFromIDEditor(int ID, List<RPGWorldPosition> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGTreePoint GetTreePointFromIDEditor(int ID, List<RPGTreePoint> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGQuest GetQuestFromIDEditor(int ID, List<RPGQuest> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGSkill GetSkillFromIDEditor(int ID, List<RPGSkill> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGRace GetRaceFromIDEditor(int ID, List<RPGRace> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGLevelsTemplate GetLevelTemplateFromIDEditor(int ID, List<RPGLevelsTemplate> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGItem GetItemFromIDEditor(int ID, List<RPGItem> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGEnchantment GetEnchantmentFromIDEditor(int ID, List<RPGEnchantment> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGEffect GetEffectFromIDEditor(int ID, List<RPGEffect> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGTalentTree GetTalentTreeFromIDEditor(int ID, List<RPGTalentTree> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGSpellbook GetSpellbookFromIDEditor(int ID, List<RPGSpellbook> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGStat GetStatFromIDEditor(int ID, List<RPGStat> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGCraftingRecipe GetCraftingRecipeFromIDEditor(int ID, List<RPGCraftingRecipe> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGResourceNode GetResourceNodeFromIDEditor(int ID, List<RPGResourceNode> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGBonus GetBonusFromIDEditor(int ID, List<RPGBonus> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGGearSet GetGearSetFromIDEditor(int ID, List<RPGGearSet> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }


    public static RPGClass GetClassFromIDEditor(int ID, List<RPGClass> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGWeaponTemplate GetWeaponTemplateFromIDEditor(int ID, List<RPGWeaponTemplate> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGMerchantTable GetMerchantTableFromIDEditor(int ID, List<RPGMerchantTable> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGDialogue GetDialogueFromIDEditor(int ID, List<RPGDialogue> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGGameModifier GetGameModifierFromIDEditor(int ID, List<RPGGameModifier> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGSpecies GetSpeciesFromIDEditor(int ID, List<RPGSpecies> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGCombo GetComboFromIDEditor(int ID, List<RPGCombo> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGLootTable GetlootTableFromIDEditor(int ID, List<RPGLootTable> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGCurrency GetCurrencyFromIDEditor(int ID, List<RPGCurrency> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static RPGCraftingStation GetCraftingStationFromIDEditor(int ID, List<RPGCraftingStation> rankList)
    {
        foreach (var t in rankList)
            if (t.ID == ID)
                return t;

        return null;
    }

    public static int getTreePointSpentAmount(RPGTalentTree tree)
    {
        foreach (var t in CharacterData.Instance.talentTrees)
            if (t.treeID == tree.ID)
                return t.pointsSpent;

        return -1;
    }

    public static int getAbilityRank(int ID)
    {
        foreach (var t in CharacterData.Instance.abilitiesData)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }

    public static int getRecipeRank(int ID)
    {
        foreach (var t in CharacterData.Instance.recipesData)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }

    public static int getResourceNodeRank(int ID)
    {
        foreach (var t in CharacterData.Instance.resourceNodeData)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }

    public static int getBonusRank(int ID)
    {
        foreach (var t in CharacterData.Instance.bonusesData)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }

    public static bool isAbilityKnown(int ID)
    {
        foreach (var t in CharacterData.Instance.abilitiesData)
            if (t.ID == ID)
                return t.known;

        return false;
    }

    public static bool isRecipeKnown(int ID)
    {
        foreach (var t in CharacterData.Instance.recipesData)
            if (t.ID == ID)
                return t.known;

        return false;
    }

    public static bool isResourceNodeKnown(int ID)
    {
        foreach (var t in CharacterData.Instance.resourceNodeData)
            if (t.ID == ID)
                return t.known;

        return false;
    }

    public static bool isBonusKnown(int ID)
    {
        foreach (var t in CharacterData.Instance.bonusesData)
            if (t.ID == ID)
                return t.known;

        return false;
    }



    public static void setAbilityData(int ID, int rank, bool known)
    {
        foreach (var t in CharacterData.Instance.abilitiesData)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
            if (RPGBuilderEssentials.Instance.isInGame && t.known && t.rank == 1)
            {
                CharacterEventsManager.Instance.AbilityLearned(GetAbilityFromID(t.ID));
            }
        }
    }

    

    public static List<RPGSpellbook.SpellBookData> GetCharacterSpellbookList()
    {
        List<RPGSpellbook.SpellBookData> spellbookList = new List<RPGSpellbook.SpellBookData>();

        if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
        {
            foreach (var t in GetClassFromID(CharacterData.Instance.classDATA.classID).spellbooks)
            {
                RPGSpellbook.SpellBookData newSpellbookData = new RPGSpellbook.SpellBookData();
                newSpellbookData.spellbook = GetSpellbookFromID(t.spellbookID);
                spellbookList.Add(newSpellbookData);
            }
        }

        foreach (var t in CharacterData.Instance.weaponTemplates)
        {
            RPGWeaponTemplate weaponTemplateREF = GetWeaponTemplateFromID(t.weaponTemplateID);
            foreach (var t1 in weaponTemplateREF.spellbooks)
            {
                RPGSpellbook.SpellBookData newSpellbookData = new RPGSpellbook.SpellBookData();
                newSpellbookData.spellbook = GetSpellbookFromID(t1.spellbookID);
                newSpellbookData.weaponTemplateID = t.weaponTemplateID;
                spellbookList.Add(newSpellbookData);
            }
        }

        return spellbookList;
    }


    public static bool isAbilityUnlockedFromSpellbook(int ID)
    {
        foreach (var t in GetCharacterSpellbookList())
        {
            foreach (var t1 in t.spellbook.nodeList)
            {
                if (t1.nodeType != RPGSpellbook.SpellbookNodeType.ability) continue;
                if (t1.abilityID != ID) continue;
                int lvlRequired = t.spellbook.sourceType == RPGSpellbook.spellbookSourceType._class
                    ? CharacterData.Instance.classDATA.currentClassLevel
                    : getWeaponTemplateLevel(t.weaponTemplateID);

                int unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Ability_Level_Required, t1.unlockLevel,
                    t.spellbook.ID, t1.abilityID);

                return unlockLevel <= lvlRequired;
            }
        }

        return false;
    }

    public static bool isBonusUnlockedFromSpellbook(int ID)
    {
        foreach (var t in GetCharacterSpellbookList())
        {
            foreach (var t1 in t.spellbook.nodeList)
            {
                if (t1.nodeType != RPGSpellbook.SpellbookNodeType.bonus) continue;
                if (t1.bonusID != ID) continue;
                int lvlRequired = t.spellbook.sourceType == RPGSpellbook.spellbookSourceType._class
                    ? CharacterData.Instance.classDATA.currentClassLevel
                    : getWeaponTemplateLevel(t.weaponTemplateID);

                int unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, t1.unlockLevel,
                    t.spellbook.ID, t1.bonusID);

                return unlockLevel <= lvlRequired;
            }
        }

        return false;
    }


    public static void setRecipeData(int ID, int rank, bool known)
    {
        foreach (var t in CharacterData.Instance.recipesData)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
        }
    }
    public static void setResourceNodeData(int ID, int rank, bool known)
    {
        foreach (var t in CharacterData.Instance.resourceNodeData)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
        }
    }
    public static void setBonusData(int ID, int rank, bool known)
    {
        foreach (var t in CharacterData.Instance.bonusesData)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
            if (rank >= 1)BonusManager.Instance.InitBonus(GetBonusFromID(ID));
            
            if (RPGBuilderEssentials.Instance.isInGame && t.known && t.rank == 1)
            {
                CharacterEventsManager.Instance.BonusLearned(GetBonusFromID(t.ID));
            }
        }
    }

    public static int getItemCount(RPGItem item)
    {
        var totalOfThisComponent = 0;
        foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
            if (slot.itemID != -1 && slot.itemID == item.ID)
                totalOfThisComponent += slot.itemStack;

        return totalOfThisComponent;
    }

    public static List<RPGCraftingRecipe> getRecipeListOfSkill(RPGSkill skill, RPGCraftingStation station)
    {
        var recipeList = new List<RPGCraftingRecipe>();
        foreach (var t in RPGBuilderEssentials.Instance.allCraftingRecipes)
            if (t.craftingSkillID == skill.ID &&
                t.craftingStationID == station.ID)
                if (isRecipeKnown(t.ID))
                    recipeList.Add(t);

        return recipeList;
    }

    
    public static void alterPointSpentToTree(RPGTalentTree tree, int points)
    {
        foreach (var t in CharacterData.Instance.talentTrees)
            if (t.treeID == tree.ID)
                t.pointsSpent += points;
    }

    public static List<RequirementsManager.RequirementDATA> getNodeRequirements(RPGTalentTree tree, RPGAbility ab)
    {
        foreach (var t in tree.nodeList)
        {
            if (t.nodeType != RPGTalentTree.TalentTreeNodeType.ability) continue;
            if (t.abilityID == ab.ID)
            {
                return t.requirements;
            }
        }

        return null;
    }

    public static List<RequirementsManager.RequirementDATA> getNodeRequirements(RPGTalentTree tree, RPGCraftingRecipe recipe)
    {
        foreach (var t in tree.nodeList)
        {
            if (t.nodeType != RPGTalentTree.TalentTreeNodeType.recipe) continue;
            if (t.recipeID == recipe.ID)
            {
                return t.requirements;
            }
        }

        return null;
    }

    public static List<RequirementsManager.RequirementDATA> getNodeRequirements(RPGTalentTree tree, RPGResourceNode resourceNode)
    {
        foreach (var t in tree.nodeList)
        {
            if (t.nodeType != RPGTalentTree.TalentTreeNodeType.resourceNode) continue;
            if (t.resourceNodeID == resourceNode.ID)
            {
                return t.requirements;
            }
        }

        return null;
    }

    public static List<RequirementsManager.RequirementDATA> getNodeRequirements(RPGTalentTree tree, RPGBonus bonus)
    {
        foreach (var t in tree.nodeList)
        {
            if (t.nodeType != RPGTalentTree.TalentTreeNodeType.bonus) continue;
            if (t.bonusID == bonus.ID)
            {
                return t.requirements;
            }
        }

        return null;
    }

    public static int getSkillLevel(int skillID)
    {
        foreach (var t in CharacterData.Instance.skillsDATA)
            if (skillID == t.skillID)
                return t.currentSkillLevel;

        return -1;
    }

    public static float getSkillEXPPercent(RPGSkill skill)
    {
        foreach (var t in CharacterData.Instance.skillsDATA)
            if (skill.ID == t.skillID)
                return (float) t.currentSkillXP /
                       (float) t.maxSkillXP;

        return -1;
    }

    public static int getSkillCurXP(RPGSkill skill)
    {
        foreach (var t in CharacterData.Instance.skillsDATA)
            if (skill.ID == t.skillID)
                return t.currentSkillXP;

        return -1;
    }

    public static int getSkillMaxXP(RPGSkill skill)
    {
        foreach (var t in CharacterData.Instance.skillsDATA)
            if (skill.ID == t.skillID)
                return t.maxSkillXP;

        return -1;
    }

    public static int getTreeIndex(RPGTalentTree tree)
    {
        for (var i = 0; i < CharacterData.Instance.talentTrees.Count; i++)
            if (CharacterData.Instance.talentTrees[i].treeID == tree.ID)
                return i;
        return -1;
    }

    public static int getAbilityIndexInTree(RPGAbility ab, RPGTalentTree tree)
    {
        for (var i = 0; i < tree.nodeList.Count; i++)
            if (tree.nodeList[i].abilityID == ab.ID)
                return i;
        return -1;
    }
    public static int getBonusIndexInTree(RPGBonus ab, RPGTalentTree tree)
    {
        for (var i = 0; i < tree.nodeList.Count; i++)
            if (tree.nodeList[i].bonusID == ab.ID)
                return i;
        return -1;
    }

    public static bool hasPointsToSpendInClassTrees()
    {
        int points = 0;
        RPGClass classREF = GetClassFromID(CharacterData.Instance.classDATA.classID);
        foreach (var t in classREF.talentTrees)
        {
            points += CharacterData.Instance.getTreePointsAmountByPoint(GetTalentTreeFromID(t.talentTreeID)
                .treePointAcceptedID);
        }

        return points > 0;
    }

    public static bool hasPointsToSpendInWeaponTemplateTrees()
    {
        int points = 0;
        foreach (var t1 in CharacterData.Instance.weaponTemplates)
        {
            RPGWeaponTemplate weaponREF = GetWeaponTemplateFromID(t1.weaponTemplateID);
            foreach (var t in weaponREF.talentTrees)
            {
                points += CharacterData.Instance.getTreePointsAmountByPoint(GetTalentTreeFromID(t.talentTreeID)
                    .treePointAcceptedID);
            }
        }

        return points > 0;
    }

    public static bool hasPointsToSpendInSkillTrees()
    {
        int points = 0;
        foreach (var t1 in CharacterData.Instance.skillsDATA)
        {
            RPGSkill skillREF = GetSkillFromID(t1.skillID);
            foreach (var t in skillREF.talentTrees)
            {
                points += CharacterData.Instance.getTreePointsAmountByPoint(GetTalentTreeFromID(t.talentTreeID)
                    .treePointAcceptedID);
            }
        }

        return points > 0;
    }
    public static bool hasPointsToSpendInSkill(int skillID)
    {
        int points = 0;
        RPGSkill skillREF = GetSkillFromID(skillID);
        foreach (var t in skillREF.talentTrees)
        {
            points += CharacterData.Instance.getTreePointsAmountByPoint(GetTalentTreeFromID(t.talentTreeID)
                .treePointAcceptedID);
        }

        return points > 0;
    }

    public static bool isGameModifierOn(int id)
    {
        foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
        {
            if(gameModifier.ID != id) continue;
            return gameModifier.On;
        }

        return false;
    }

    public static bool isGameModifierAdded(int id)
    {
        foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
        {
            if(gameModifier.ID != id) continue;
            return true;
        }

        return false;
    }

    public static void setGameModifierOnState(int id, bool isOn)
    {
        foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
        {
            if(gameModifier.ID != id) continue;
            gameModifier.On = isOn;
        }
    }

    public static int getNegativeModifiersCount()
    {
        int total = 0;
        foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
        {
            RPGGameModifier gameModRef = GetGameModifierFromID(gameModifier.ID);
            if (gameModRef.gameModifierType == RPGGameModifier.GameModifierType.Negative) total++;
        }

        return total;
    }
    public static int getPositiveModifiersCount()
    {
        int total = 0;
        foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
        {
            RPGGameModifier gameModRef = GetGameModifierFromID(gameModifier.ID);
            if (gameModRef.gameModifierType == RPGGameModifier.GameModifierType.Positive) total++;
        }

        return total;
    }
    
    public static Sprite getItemRaritySprite(string rarity)
    {
        int qualityIndex = -1;

        for (int i = 0; i < RPGBuilderEssentials.Instance.itemSettings.itemRarityList.Count; i++)
        {
            if (RPGBuilderEssentials.Instance.itemSettings.itemRarityList[i] == rarity)
            {
                qualityIndex = i;
            }
        }

        return qualityIndex == -1 ? null : RPGBuilderEssentials.Instance.itemSettings.itemRarityImagesList[qualityIndex];
    }
    public static Color getItemRarityColor(string rarity)
    {
        int qualityIndex = -1;

        for (int i = 0; i < RPGBuilderEssentials.Instance.itemSettings.itemRarityList.Count; i++)
        {
            if (RPGBuilderEssentials.Instance.itemSettings.itemRarityList[i] == rarity)
            {
                qualityIndex = i;
            }
        }

        return qualityIndex == -1 ? Color.clear : RPGBuilderEssentials.Instance.itemSettings.itemRarityColorsList[qualityIndex];
    }

    

    public static string addLineBreak(string text)
    {
        text += "\n";
        return text;
    }
    
    public static int getArmorSlotIndex(string slotType)
    {
        for (var i = 0; i < InventoryManager.Instance.equippedArmors.Count; i++)
            if (InventoryManager.Instance.equippedArmors[i].slotType == slotType)
                return i;
        return -1;
    }
    
    
    public static RPGItem getEquippedArmor(string slotType)
    {
        foreach (var t in InventoryManager.Instance.equippedArmors)
            if (t.slotType == slotType)
                return t.itemEquipped != null ? t.itemEquipped : null;

        return null;
    }
    public static bool isItemEquipped(int itemID)
    {
        foreach (var t in InventoryManager.Instance.equippedArmors)
            if (t.itemEquipped != null && t.itemEquipped.ID == itemID)
                return true;
        foreach (var t in InventoryManager.Instance.equippedWeapons)
            if (t.itemEquipped != null && t.itemEquipped.ID == itemID)
                return true;

        return false;
    }

    public static bool isItemPartOfGearSet(int itemID)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allGearSets)
        {
            foreach (var t1 in t.itemsInSet)
            {
                if (t1.itemID == itemID) return true;
            }
        }

        return false;
    }
    public static RPGGearSet getItemGearSet(int itemID)
    {
        foreach (var t in RPGBuilderEssentials.Instance.allGearSets)
        {
            foreach (var t1 in t.itemsInSet)
            {
                if (t1.itemID == itemID) return t;
            }
        }

        return null;
    }
    
    public static  StatCalculator.TemporaryActiveGearSetsDATA getGearSetState(int itemID)
    {
        RPGGearSet gearSetREF = null;
        foreach (var t in RPGBuilderEssentials.Instance.allGearSets)
        {
            foreach (var t1 in t.itemsInSet)
            {
                if (t1.itemID == itemID)
                    gearSetREF = t;
            }
        }

        StatCalculator.TemporaryActiveGearSetsDATA thisGearSetData = new StatCalculator.TemporaryActiveGearSetsDATA();
        thisGearSetData.gearSet = gearSetREF;
        int equippedPieces = 0;
        foreach (var t in gearSetREF.itemsInSet)
        {
            if (isItemEquipped(t.itemID))
            {
                equippedPieces++;
            }
        }

        thisGearSetData.activeTierIndex = getGearSetTierIndex(gearSetREF, equippedPieces);
        return thisGearSetData;
    }
    
    public static int getGearSetTierIndex(RPGGearSet gearSetREF)
    {
        int equippedPieces = 0;
        foreach (var t in gearSetREF.itemsInSet)
        {
            if (isItemEquipped(t.itemID))
            {
                equippedPieces++;
            }
        }

        return getGearSetTierIndex(gearSetREF, equippedPieces);
    }

    public static bool containsGearSet(RPGGearSet newGearSet, List<StatCalculator.TemporaryActiveGearSetsDATA> allActiveGearSets)
    {
        foreach (var t in allActiveGearSets)
        {
            if (t.gearSet == newGearSet) return true;
        }

        return false;
    }

    static int getGearSetTierIndex(RPGGearSet gearSetREF, int equipedPieces)
    {
        int tierIndex = -1;
        for (var index = 0; index < gearSetREF.gearSetTiers.Count; index++)
        {
            var t = gearSetREF.gearSetTiers[index];
            if (equipedPieces >= t.equippedAmount)
                tierIndex = index;
        }

        return tierIndex;
    }

    public static float getAmountDifference(float val1, float val2)
    {
        return val1 > val2 ? val1 - val2 : val2 - val1;
    }

    public static int getRandomItemIDFromDataID(int itemDataID)
    {
        foreach (var t in CharacterData.Instance.itemsDATA)
        {
            if (t.id == itemDataID) return t.rdmItemID;
        }
        return -1;
    }
    
    public static int HandleNewItemDATA(int itemID, CharacterData.ItemDataState state)
    {
        RPGItem itemREF = GetItemFromID(itemID);
        if (itemREF == null) return -1;
        if (itemREF.itemType != "ARMOR" && itemREF.itemType != "WEAPON") return -1;
        CharacterData.ItemDATA newItemDATA = new CharacterData.ItemDATA();
        newItemDATA.itemID = itemID;
        newItemDATA.rdmItemID = itemREF.randomStats.Count > 0 ? GenerateRandomItemStats(itemID) : -1;
        newItemDATA.id = CharacterData.Instance.nextAvailableItemID;
        newItemDATA.state = state;
        newItemDATA.itemName = itemREF._name;

        foreach (var t in itemREF.sockets)
        {
            CharacterData.ItemSocketData newSocket = new CharacterData.ItemSocketData();
            newSocket.socketType = t.socketType;
            newSocket.gemItemID = -1;
            newItemDATA.sockets.Add(newSocket);
        }
        
        CharacterData.Instance.nextAvailableItemID++;
        CharacterData.Instance.itemsDATA.Add(newItemDATA);
        return newItemDATA.id;
    }
    
    public static int getRandomItemIndexFromID(int ID)
    {
        for (int i = 0; i < CharacterData.Instance.allRandomizedItems.Count; i++)
        {
            if (CharacterData.Instance.allRandomizedItems[i].id == ID)
            {
                return i;
            }
        }
        return -1;
    }
    
    public static RPGItemDATA.RandomItemData getRandomItemData(int id)
    {
        RPGItemDATA.RandomItemData rdmItemData = new RPGItemDATA.RandomItemData();
        rdmItemData.randomItemID = id;
        rdmItemData.randomStats = CharacterData.Instance.allRandomizedItems[getRandomItemIndexFromID(id)].randomStats;
        return rdmItemData;
    }
    
    public static int GenerateRandomItemStats(int itemID)
    {
        RPGItem itemREF = GetItemFromID(itemID);
        List<RPGItemDATA.RandomizedStat> randomStats = new List<RPGItemDATA.RandomizedStat>();
        int statCount = 0;
        foreach (var t in itemREF.randomStats)
        {
            if(itemREF.randomStatsMax > 0 && statCount  >= itemREF.randomStatsMax) continue;
            if (!(Random.Range(0f, 100f) <= t.chance)) continue;
            RPGItemDATA.RandomizedStat rdmStat = new RPGItemDATA.RandomizedStat();
            rdmStat.statID = t.statID;
            rdmStat.statValue = (float)Math.Round(Random.Range(t.minValue, t.maxValue), 2);
            if (t.isInt)
            {
                rdmStat.statValue = (float)Math.Round(rdmStat.statValue, 0);
            }
            randomStats.Add(rdmStat);
            statCount++;
        }
        
        CharacterData.RandomizedItems newRandomItem = new CharacterData.RandomizedItems();
        newRandomItem.itemID = itemID;
        newRandomItem.id = CharacterData.Instance.nextAvailableRandomItemID;
        newRandomItem.randomStats = randomStats;

        CharacterData.Instance.nextAvailableRandomItemID++;
        CharacterData.Instance.allRandomizedItems.Add(newRandomItem);

        return newRandomItem.id;
    }
    
    public static int GetItemDataIndexFromDataID(int itemDataID)
    {
        for (var index = 0; index < CharacterData.Instance.itemsDATA.Count; index++)
        {
            var t = CharacterData.Instance.itemsDATA[index];
            if (t.id == itemDataID)
            {
                return index;
            }
        }

        return -1;
    }
    
    public static CharacterData.ItemDATA GetItemDataFromDataID(int itemDataID)
    {
        foreach (var t in CharacterData.Instance.itemsDATA)
        {
            if (t.id == itemDataID)
            {
                return t;
            }
        }

        return null;
    }

    public static void SetNewItemDataState(int itemDataID, CharacterData.ItemDataState newState)
    {
        int curIndex = GetItemDataIndexFromDataID(itemDataID);
        if (curIndex == -1) return;
        CharacterData.Instance.itemsDATA[curIndex].state = newState;
    }

    public static bool abIsPartOfActionAbilities(RPGAbility ab)
    {
        foreach (var actionAb in CharacterData.Instance.currentActionAbilities)
        {
            if (actionAb.ability == ab) return true;
        }

        return false;
    }

    public static void CheckRemoveActionAbilities(RPGItem item)
    {
        List<CharacterData.ActionAbility> actionAbToRemove = new List<CharacterData.ActionAbility>();
        foreach (var actionAb in CharacterData.Instance.currentActionAbilities)
        {
            if (actionAb.type == CharacterData.ActionAbilityType.fromItem && actionAb.sourceID == item.ID)
            {
                actionAbToRemove.Add(actionAb);
            }
        }

        foreach (var t in actionAbToRemove)
        {
            if (CharacterData.Instance.currentActionAbilities.Contains(t))
                CharacterData.Instance.currentActionAbilities.Remove(t);
        }
    }
    
    public static void UpdateActionAbilities(RPGItem item)
    {
        foreach (var actionAb in item.actionAbilities)
        {
            RPGBuilderEssentials.Instance.AddCurrentActionAb(actionAb, CharacterData.ActionAbilityType.fromItem, item.ID);
        }
    }
    
    public static bool characterHasDialogue(int ID)
    {
        foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
        {
            if(dialogue.ID != ID) continue;
            return true;
        }

        return false;
    }
    public static bool characterDialogueHasDialogueNode(int ID, RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
        {
            if(dialogue.ID != ID) continue;
            foreach (var node in dialogue.nodesData)
            {
                if(node.textNode != textNode) continue;
                return true;
            }
        }

        return false;
    }
    public static int getDialogueIndex(int ID)
    {
        for (var index = 0; index < CharacterData.Instance.dialoguesDATA.Count; index++)
        {
            var dialogue = CharacterData.Instance.dialoguesDATA[index];
            if (dialogue.ID != ID) continue;
            return index;
        }

        return -1;
    }
    
    public static bool dialogueNodeCanBeViewed(int dialogueID, RPGDialogueTextNode textNode, int viewCountMax)
    {
        foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
        {
            if(dialogue.ID != dialogueID) continue;
            foreach (var node in dialogue.nodesData)
            {
                if(node.textNode != textNode) continue;
                return node.currentlyViewedCount < viewCountMax;
            }
        }

        return false;
    }
    public static bool dialogueNodeCanBeClicked(int dialogueID, RPGDialogueTextNode textNode, int clickCountMax)
    {
        foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
        {
            if(dialogue.ID != dialogueID) continue;
            foreach (var node in dialogue.nodesData)
            {
                if(node.textNode != textNode) continue;
                return node.currentlyClickedCount < clickCountMax;
            }
        }

        return false;
    }

    public static void addNodeToDialogue(int dialogueID, RPGDialogueTextNode textNode)
    {
        int dialogueIndex = getDialogueIndex(dialogueID);
        CharacterData.DialoguesData.DialogueNodeData newNode = new CharacterData.DialoguesData.DialogueNodeData();
        newNode.currentlyViewedCount = 0;
        newNode.textNode = textNode;
        CharacterData.Instance.dialoguesDATA[dialogueIndex].nodesData.Add(newNode);
    }

    public static void addDialogueToCharacter(int dialogueID)
    {
        CharacterData.DialoguesData newDialogue = new CharacterData.DialoguesData();
        newDialogue.ID = dialogueID;
        CharacterData.Instance.dialoguesDATA.Add(newDialogue);
    }

    public static RPGDialogueTextNode getFirstNPCNode(RPGDialogueGraph graph)
    {
        RPGDialogueTextNode firstNode = null;
        foreach (var node in graph.nodes)
        {
            if (firstNode == null || firstNode.position.x > node.position.x)
            {
                firstNode = (RPGDialogueTextNode)node;
            }
        }

        return firstNode;
    }
    public static RPGDialogueTextNode getNextNPCNode(RPGDialogueTextNode playerNode)
    {
        foreach (var outputNode in playerNode.GetOutputPort("nextNodes").GetConnections())
        {
            RPGDialogueTextNode textNode = (RPGDialogueTextNode)outputNode.node;
            if (textNode.identityType == RPGDialogueTextNode.IdentityType.NPC)
            {
                return textNode;
            }
        }

        return null;
    }

    public static void completeDialogueLine(int dialogueID, RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
        {
            if (dialogue.ID != dialogueID) continue;
            foreach (var charNode in dialogue.nodesData)
            {
                if (charNode.textNode != textNode) continue;
                charNode.lineCompleted = true;
            }
        }
    }
    

    public static bool isDialogueLineCompleted(int dialogueID, RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in CharacterData.Instance.dialoguesDATA)
        {
            if (dialogue.ID != dialogueID) continue;
            foreach (var charNode in dialogue.nodesData)
            {
                if (charNode.textNode != textNode) continue;
                if (charNode.lineCompleted) return true;
            }
        }

        return false;
    }
    
    public static int getDialogueIDFromNodeInstanceID(RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in RPGBuilderEssentials.Instance.allDialogues)
        {
            foreach (var node in dialogue.dialogueGraph.nodes)
            {
                if ((RPGDialogueTextNode)node != textNode) continue;
                return dialogue.ID;
            }
        }

        return -1;
    }

    public static int HandleItemLooting(int itemID, int amount, bool equipped, bool showItemGain)
    {
        var amountPossibleToAdd = InventoryManager.Instance.canGetItem(itemID, amount);

        if (amountPossibleToAdd > amount)
        {
            amountPossibleToAdd = amount;
        }
        if (amountPossibleToAdd == 0)
        {
            ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
            return amount;
        }
        InventoryManager.Instance.AddItem(itemID, amountPossibleToAdd, equipped, HandleNewItemDATA(itemID,
            equipped ? CharacterData.ItemDataState.equipped : CharacterData.ItemDataState.inBag));
        
        if(showItemGain) ItemsGainEventDisplayManager.Instance.DisplayText(GetItemFromID(itemID).displayName + " x" + amountPossibleToAdd);
        return amount - amountPossibleToAdd;
    }
    public static int GetAllSlotsNeeded(List<InventoryManager.TemporaryLootItemData> craftlist)
    {
        int totalSlotsNeeded = 0;
        foreach (var craft in craftlist)
        {
            totalSlotsNeeded += InventoryManager.Instance.slotsNeededForLoot(craft.itemID, craft.count, GetItemFromID(craft.itemID).stackLimit).slotsNeeded;
        }
        
        return totalSlotsNeeded;
    }

    public static Sprite getRaceIcon()
    {
        RPGRace raceREF = GetRaceFromID(CharacterData.Instance.raceID);
        return CharacterData.Instance.gender == RPGRace.RACE_GENDER.Male ? raceREF.maleIcon : raceREF.femaleIcon;
    }
    public static Sprite getRaceIconByID(int ID)
    {
        RPGRace raceREF = GetRaceFromID(ID);
        return CharacterData.Instance.gender == RPGRace.RACE_GENDER.Male ? raceREF.maleIcon : raceREF.femaleIcon;
    }
    public static Sprite getRaceIcon(RPGRace raceREF)
    {
        return CharacterData.Instance.gender == RPGRace.RACE_GENDER.Male ? raceREF.maleIcon : raceREF.femaleIcon;
    }
    
    public static bool canActiveShapeshiftCameraAim(CombatNode cbtNode)
    {
        foreach (var state in cbtNode.nodeStateData)
        {
            if(state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Shapeshifting) continue;
            return GetEffectFromID(state.stateEffect.ID).ranks[state.effectRank].canCameraAim;
        }

        return true;
    }
    public static int getActiveShapeshiftingEffectID(CombatNode cbtNode)
    {
        foreach (var state in cbtNode.nodeStateData)
        {
            if(state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Shapeshifting) continue;
            return state.stateEffect.ID;
        }

        return -1;
    }
    public static int getShapeshiftingTagEffectID(RPGAbility.RPGAbilityRankData rankREF)
    {
        foreach (var tag in rankREF.tagsData)
        {
            if(tag.tag != RPGAbility.ABILITY_TAGS.shapeshifting) continue;
            return tag.effectID;
        }

        return -1;
    }
    public static int getActiveStealthEffectID(CombatNode cbtNode)
    {
        foreach (var state in cbtNode.nodeStateData)
        {
            if(state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Stealth) continue;
            return state.stateEffect.ID;
        }

        return -1;
    }
    public static int getStealthTagEffectID(RPGAbility.RPGAbilityRankData rankREF)
    {
        foreach (var tag in rankREF.tagsData)
        {
            if(tag.tag != RPGAbility.ABILITY_TAGS.stealth) continue;
            return tag.effectID;
        }

        return -1;
    }
    
    public static int getCurrentMoveSpeed(CombatNode cbtNode)
    {
        return (int)CombatManager.Instance.GetTotalOfStatType(cbtNode, RPGStat.STAT_TYPE.MOVEMENT_SPEED);
    }

    public static bool isStatAffectingMoveSpeed(RPGStat stat)
    {
        foreach (var bonus in stat.statBonuses)
        {
            if(bonus.statType != RPGStat.STAT_TYPE.MOVEMENT_SPEED) continue;
            return true;
        }

        return false;
    }

    public static bool isAbilityToggled(CombatNode cbtNode, RPGAbility ability)
    {
        foreach (var toggledAbility in cbtNode.activeToggledAbilities)
        {
            if(toggledAbility.ability != ability) continue;
            return true;
        }

        return false;
    }
    public static RPGAbility.RPGAbilityRankData getCurrentAbilityRankREF(CombatNode casterNode, RPGAbility ability, bool abMustBeKnown)
    {
        int curRank;
        if (casterNode.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
            casterNode.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
            casterNode.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
        {
            curRank = 0;
        }
        else
        {
            curRank = getAbilityRank(ability.ID);
            if (!abMustBeKnown || casterNode.appearanceREF.isShapeshifted) curRank = 0;
        }

        return ability.ranks[curRank];
    }

    public static string getWeaponType(int weapon)
    {
        return InventoryManager.Instance.equippedWeapons[weapon-1].itemEquipped != null ? InventoryManager.Instance.equippedWeapons[weapon-1].itemEquipped.weaponType : "";
    }
    
    public static bool isWeaponTypeEquipped(string weaponType)
    {
        return (InventoryManager.Instance.equippedWeapons[0].itemEquipped != null &&
                InventoryManager.Instance.equippedWeapons[0].itemEquipped.weaponType == weaponType) ||
               (InventoryManager.Instance.equippedWeapons[1].itemEquipped != null &&
                InventoryManager.Instance.equippedWeapons[1].itemEquipped.weaponType == weaponType);
    }

    public static RuntimeAnimatorController getNewWeaponAnimatorOverride()
    {
        string weapon1 = getWeaponType(1);
        string weapon2 = getWeaponType(2);
        
        if (!string.IsNullOrEmpty(weapon1) && string.IsNullOrEmpty(weapon2))
        {
            foreach (var animatorOverride in RPGBuilderEssentials.Instance.itemSettings.weaponAnimatorOverrides)
            {
                if (animatorOverride.weaponType1 == weapon1 && !animatorOverride.requireWeapon2)
                {
                    return CombatManager.playerCombatNode.inCombat ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
            }
        } else if (string.IsNullOrEmpty(weapon1) && !string.IsNullOrEmpty(weapon2))
        {
            foreach (var animatorOverride in RPGBuilderEssentials.Instance.itemSettings.weaponAnimatorOverrides)
            {
                if (animatorOverride.weaponType1 == weapon2 && !animatorOverride.requireWeapon2)
                {
                    return CombatManager.playerCombatNode.inCombat ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
            }
        }
        else if(!string.IsNullOrEmpty(weapon1) && !string.IsNullOrEmpty(weapon2))
        {
            RPGItemDATA.WeaponAnimatorOverride bestMatchOverride = getMatching2WeaponsOverride(weapon1, weapon2);
            if (bestMatchOverride != null)
            {
                return CombatManager.playerCombatNode.inCombat ? bestMatchOverride.combatAnimatorOverride : bestMatchOverride.restAnimatorOverride;
            }

            foreach (var animatorOverride in RPGBuilderEssentials.Instance.itemSettings.weaponAnimatorOverrides)
            {
                if (animatorOverride.weaponType1 == weapon1 && (!animatorOverride.requireWeapon2 || weapon2 != "" && weapon2 == animatorOverride.weaponType2))
                {
                    return CombatManager.playerCombatNode.inCombat ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
                if (animatorOverride.weaponType1 == weapon2 && (!animatorOverride.requireWeapon2 || weapon1 != "" && weapon1 == animatorOverride.weaponType2))
                {
                    return CombatManager.playerCombatNode.inCombat ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
            }
        }
        else
        {
            return null;
        }

        return null;
    }

    private static RPGItemDATA.WeaponAnimatorOverride getMatching2WeaponsOverride(string weapon1, string weapon2)
    {
        foreach (var animatorOverride in RPGBuilderEssentials.Instance.itemSettings.weaponAnimatorOverrides)
        {
            if (animatorOverride.weaponType1 == weapon1 && animatorOverride.requireWeapon2 && weapon2 == animatorOverride.weaponType2)
            {
                return animatorOverride;
            }
            if (animatorOverride.weaponType1 == weapon2 && animatorOverride.requireWeapon2 && weapon1 == animatorOverride.weaponType2)
            {
                return animatorOverride;
            }
        }

        return null;
    }

    public static bool isInventoryFull()
    {
        bool hasSpace = false;
        foreach (var invSlot in CharacterData.Instance.inventoryData.baseSlots)
        {
            if (invSlot.itemID != -1) continue;
            hasSpace = true;
            break;
        }

        return !hasSpace;
    }

    public static int getCurrentPlayerLevel()
    {
        if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            return CharacterData.Instance.classDATA.currentClassLevel;
        int totalWeaponLevels = 0;
        foreach (var weaponTemplate in CharacterData.Instance.weaponTemplates)
        {
           totalWeaponLevels += weaponTemplate.currentWeaponLevel;
        }

        if(totalWeaponLevels > 0) totalWeaponLevels /= CharacterData.Instance.weaponTemplates.Count;
        return totalWeaponLevels;
    }

    public static bool isActionKeyUnique(string actionKeyName)
    {
        foreach (var actionKey in RPGBuilderEssentials.Instance.generalSettings.actionKeys)
        {
            if(actionKey.actionName != actionKeyName) continue;
            return actionKey.isUnique;
        }

        return false;
    }
}