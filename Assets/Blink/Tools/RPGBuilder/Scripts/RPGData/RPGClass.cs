using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGClass : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;
    public Sprite icon;

    [AbilityID] public int autoAttackAbilityID = -1;
    public RPGAbility autoAttackAbilityREF;

    [Serializable]
    public class CLASS_STATS_DATA
    {
        public string _name;
        [StatID] public int statID = -1;
        public RPGStat statREF;
        public float amount;
        public bool isPercent;
        public float bonusPerLevel;
    }

    [RPGDataList] public List<CLASS_STATS_DATA> stats = new List<CLASS_STATS_DATA>();

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
    public class SpellbookDATA
    {
        [SpellbookID] public int spellbookID = -1;
        public RPGSpellbook spellbookREF;
    }

    [RPGDataList] public List<SpellbookDATA> spellbooks = new List<SpellbookDATA>();
    
    [RPGDataList] public List<RPGItemDATA.StartingItemsDATA> startItems = new List<RPGItemDATA.StartingItemsDATA>();

    [RPGDataList] public List<RPGCombatDATA.ActionAbilityDATA> actionAbilities = new List<RPGCombatDATA.ActionAbilityDATA>();
    
    public int allocationStatPoints;
    [RPGDataList] public List<CharacterData.AllocatedStatEntry> allocatedStatsEntries = new List<CharacterData.AllocatedStatEntry>();
    
    [RPGDataList] public List<CharacterData.AllocatedStatEntry> allocatedStatsEntriesGame = new List<CharacterData.AllocatedStatEntry>();
    public void updateThis(RPGClass newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        icon = newData.icon;
        description = newData.description;
        stats = newData.stats;
        levelTemplateID = newData.levelTemplateID;
        talentTrees = newData.talentTrees;
        autoAttackAbilityID = newData.autoAttackAbilityID;
        startItems = newData.startItems;
        displayName = newData.displayName;
        actionAbilities = newData.actionAbilities;
        spellbooks = newData.spellbooks;
        allocationStatPoints = newData.allocationStatPoints;
        allocatedStatsEntries = newData.allocatedStatsEntries;
        allocatedStatsEntriesGame = newData.allocatedStatsEntriesGame;
    }
}