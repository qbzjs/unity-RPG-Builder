using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGWeaponTemplate : ScriptableObject
{
    public int ID = -1;
    public Sprite icon;
    public string _name;
    public string fileName;
    public string displayName;
    public string description;

    [LevelTemplateID] public int levelTemplateID = -1;
    public RPGLevelsTemplate levelTemplateREF;

    [Serializable]
    public class WeaponDATA
    {
        public string weaponType;
        public float weaponEXPModifier = 1f;
    }

    [RPGDataList] public List<WeaponDATA> weaponList = new List<WeaponDATA>();
    
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
    
    [Serializable]
    public class TalentTreesDATA
    {
        [TalentTreeID]  public int talentTreeID = -1;
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
    [RPGDataList] public List<CharacterData.AllocatedStatEntry> allocatedStatsEntriesGame = new List<CharacterData.AllocatedStatEntry>();
    
    public void updateThis(RPGWeaponTemplate newData)
    {
        ID = newData.ID;
        icon = newData.icon;
        _name = newData._name;
        displayName = newData.displayName;
        fileName = newData.fileName;
        description = newData.description;
        stats = newData.stats;
        levelTemplateID = newData.levelTemplateID;
        talentTrees = newData.talentTrees;
        spellbooks = newData.spellbooks;
        startItems = newData.startItems;
        weaponList = newData.weaponList;
        allocatedStatsEntriesGame = newData.allocatedStatsEntriesGame;
    }
}
