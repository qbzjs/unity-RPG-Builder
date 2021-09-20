using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGSkill : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;
    public Sprite icon;
    public bool automaticlyAdded;
    public int MaxLevel;
    [LevelTemplateID] public int levelTemplateID = -1;
    public RPGLevelsTemplate levelTemplateREF;

    [Serializable]
    public class TalentTreesDATA
    {
        [TalentTreeID] public int talentTreeID = -1;
        public RPGTalentTree talentTreeREF;
    }

    [RPGDataList] public List<TalentTreesDATA> talentTrees = new List<TalentTreesDATA>();

    [Serializable]
    public class STATS_DATA
    {
        public string _name;
        [StatID] public int statID = -1;
        public RPGStat statREF;
        public float amount;
        public bool isPercent;
        public float bonusPerLevel;
    }

    [RPGDataList] public List<STATS_DATA> stats = new List<STATS_DATA>();
    
    [RPGDataList] public List<RPGItemDATA.StartingItemsDATA> startItems = new List<RPGItemDATA.StartingItemsDATA>();
    [RPGDataList] public List<RPGCombatDATA.ActionAbilityDATA> actionAbilities = new List<RPGCombatDATA.ActionAbilityDATA>();
    
    [RPGDataList] public List<CharacterData.AllocatedStatEntry> allocatedStatsEntriesGame = new List<CharacterData.AllocatedStatEntry>();
    public void updateThis(RPGSkill newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        icon = newData.icon;
        MaxLevel = newData.MaxLevel;
        description = newData.description;
        levelTemplateID = newData.levelTemplateID;
        automaticlyAdded = newData.automaticlyAdded;
        talentTrees = newData.talentTrees;
        startItems = newData.startItems;
        displayName = newData.displayName;
        actionAbilities = newData.actionAbilities;
        stats = newData.stats;
        allocatedStatsEntriesGame = newData.allocatedStatsEntriesGame;
    }
}