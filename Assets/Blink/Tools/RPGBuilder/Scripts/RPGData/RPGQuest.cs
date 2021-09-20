using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGQuest : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string displayName;
    public string _fileName;


    public string description;
    public string ObjectiveText;
    public string ProgressText;

    public bool repeatable;
    public bool canBeTurnedInWithoutNPC;

    public List<RequirementsManager.RequirementDATA>
        questRequirements = new List<RequirementsManager.RequirementDATA>();

    [Serializable]
    public class QuestItemsGivenDATA
    {
        [ItemID] public int itemID = -1;
        public RPGItem itemREF;
        public int count;
    }

    [RPGDataList] public List<QuestItemsGivenDATA> itemsGiven = new List<QuestItemsGivenDATA>();

    public enum QuestObjectiveType
    {
        task
    }

    [Serializable]
    public class QuestObjectiveDATA
    {
        public QuestObjectiveType objectiveType;
        [TaskID] public int taskID = -1;
        public RPGTask taskREF;
        public float timeLimit;
    }

    [RPGDataList] public List<QuestObjectiveDATA> objectives = new List<QuestObjectiveDATA>();

    public enum QuestRewardType
    {
        item,
        currency,
        treePoint,
        Experience,
        FactionPoint,
        weaponTemplateEXP
    }

    [Serializable]
    public class QuestRewardDATA
    {
        public QuestRewardType rewardType;
        [ItemID] public int itemID = -1;
        public RPGItem itemREF;
        [CurrencyID] public int currencyID = -1;
        public RPGCurrency currencyREF;
        [PointID] public int treePointID = -1;
        public RPGTreePoint treePointREF;
        [FactionID] public int factionID = -1;
        public RPGFaction factionREF;
        [WeaponTemplateID] public int weaponTemplateID = -1;
        public RPGWeaponTemplate weaponTemplateREF;
        public int count;
        public int Experience;
    }

    [RPGDataList] public List<QuestRewardDATA> rewardsGiven = new List<QuestRewardDATA>();
    [RPGDataList] public List<QuestRewardDATA> rewardsToPick = new List<QuestRewardDATA>();

    public void updateThis(RPGQuest newItemDATA)
    {
        ID = newItemDATA.ID;
        _name = newItemDATA._name;
        _fileName = newItemDATA._fileName;
        displayName = newItemDATA.displayName;
        description = newItemDATA.description;
        ObjectiveText = newItemDATA.ObjectiveText;
        ProgressText = newItemDATA.ProgressText;
        repeatable = newItemDATA.repeatable;
        questRequirements = newItemDATA.questRequirements;
        itemsGiven = newItemDATA.itemsGiven;
        objectives = newItemDATA.objectives;
        rewardsGiven = newItemDATA.rewardsGiven;
        rewardsToPick = newItemDATA.rewardsToPick;
        canBeTurnedInWithoutNPC = newItemDATA.canBeTurnedInWithoutNPC;
    }
}