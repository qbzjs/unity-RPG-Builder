using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGBonus : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string displayName;
    public string _fileName;
    public Sprite icon;
    public bool learnedByDefault;

    [Serializable]
    public class RPGBonusRankDATA
    {
        public bool ShowedInEditor;
        public int unlockCost;

        [RPGDataList] public List<RequirementsManager.BonusRequirementDATA> activeRequirements =
            new List<RequirementsManager.BonusRequirementDATA>();

        [RPGDataList] public List<RPGEffect.STAT_EFFECTS_DATA> statEffectsData = new List<RPGEffect.STAT_EFFECTS_DATA>();
    }
    [RPGDataList] public List<RPGBonusRankDATA> ranks = new List<RPGBonusRankDATA>();

    public void copyData(RPGBonusRankDATA original, RPGBonusRankDATA copied)
    {
        original.unlockCost = copied.unlockCost;
        
        original.statEffectsData = new List<RPGEffect.STAT_EFFECTS_DATA>();
        for (var index = 0; index < copied.statEffectsData.Count; index++)
        {
            RPGEffect.STAT_EFFECTS_DATA newRef = new RPGEffect.STAT_EFFECTS_DATA();
            newRef.isPercent = copied.statEffectsData[index].isPercent;
            newRef.statEffectModification = copied.statEffectsData[index].statEffectModification;
            newRef.statID = copied.statEffectsData[index].statID;
            original.statEffectsData.Add(newRef);
        }
        original.activeRequirements = new List<RequirementsManager.BonusRequirementDATA>();
        for (var index = 0; index < copied.activeRequirements.Count; index++)
        {
            RequirementsManager.BonusRequirementDATA newRef = new RequirementsManager.BonusRequirementDATA();
            newRef.statValue = copied.activeRequirements[index].statValue;
            newRef.classLevelValue = copied.activeRequirements[index].classLevelValue;
            newRef.npcKillsRequired = copied.activeRequirements[index].npcKillsRequired;
            newRef.pointSpentValue = copied.activeRequirements[index].pointSpentValue;
            newRef.skillLevelValue = copied.activeRequirements[index].skillLevelValue;
            newRef.statID = copied.activeRequirements[index].statID;
            newRef.abilityRequiredID = copied.activeRequirements[index].abilityRequiredID;
            newRef.classRequiredID = copied.activeRequirements[index].classRequiredID;
            newRef.itemRequiredID = copied.activeRequirements[index].itemRequiredID;
            newRef.npcRequiredID = copied.activeRequirements[index].npcRequiredID;
            newRef.questRequiredID = copied.activeRequirements[index].questRequiredID;
            newRef.raceRequiredID = copied.activeRequirements[index].raceRequiredID;
            newRef.skillRequiredID = copied.activeRequirements[index].skillRequiredID;
            newRef.craftingRecipeRequiredID = copied.activeRequirements[index].craftingRecipeRequiredID;
            newRef.resourceNodeRequiredID = copied.activeRequirements[index].resourceNodeRequiredID;
            newRef.itemEquipped = copied.activeRequirements[index].itemEquipped;
            newRef.requirementType = copied.activeRequirements[index].requirementType;
            newRef.weaponRequired = copied.activeRequirements[index].weaponRequired;
            newRef.questStateRequired = copied.activeRequirements[index].questStateRequired;
            newRef.statStateRequired = copied.activeRequirements[index].statStateRequired;
            newRef.isStatValuePercent = copied.activeRequirements[index].isStatValuePercent;
            original.activeRequirements.Add(newRef);
        }
    }
    
    public void updateThis(RPGBonus newDATA)
    {
        ID = newDATA.ID;
        _name = newDATA._name;
        displayName = newDATA.displayName;
        _fileName = newDATA._fileName;
        icon = newDATA.icon;
        ranks = newDATA.ranks;
        learnedByDefault = newDATA.learnedByDefault;
    }
}