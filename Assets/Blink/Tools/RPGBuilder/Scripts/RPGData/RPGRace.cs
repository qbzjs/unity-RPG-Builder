using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGRace : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;
    public int startingSceneID = -1;
    public RPGGameScene startingSceneREF;
    public int startingPositionID = -1;
    public RPGWorldPosition startingPositionREF;

    public RPGFaction factionREF;
    
    [FactionID] public int factionID = -1;
    [SpeciesID] public int speciesID = -1;
    
    public enum RACE_GENDER
    {
        Male,
        Female
    }

    public GameObject malePrefab;
    public Sprite maleIcon;
    public GameObject femalePrefab;
    public Sprite femaleIcon;

    public bool dynamicAnimator;
    public RuntimeAnimatorController restAnimatorController, combatAnimatorController;

    [Serializable]
    public class RACE_CLASSES_DATA
    {
        [ClassID] public int classID = -1;
        public RPGClass classREF;
    }

    [RPGDataList] public List<RACE_CLASSES_DATA> availableClasses = new List<RACE_CLASSES_DATA>();

    
    [Serializable]
    public class WEAPON_TEMPLATES_DATA
    {
        [WeaponTemplateID] public int weaponTemplateID = -1;
        public RPGWeaponTemplate weaponTemplateREF;
    }

    [RPGDataList] public List<WEAPON_TEMPLATES_DATA> weaponTemplates = new List<WEAPON_TEMPLATES_DATA>();
    
    [Serializable]
    public class RACE_STATS_DATA
    {
        [StatID] public int statID = -1;
        public RPGStat statREF;
        public float amount;
        public bool isPercent;
    }

    [RPGDataList] public List<RACE_STATS_DATA> stats = new List<RACE_STATS_DATA>();

    [RPGDataList] public List<RPGItemDATA.StartingItemsDATA> startItems = new List<RPGItemDATA.StartingItemsDATA>();
    
    [RPGDataList] public List<RPGCombatDATA.ActionAbilityDATA> actionAbilities = new List<RPGCombatDATA.ActionAbilityDATA>();

    public int allocationStatPoints;
    public List<CharacterData.AllocatedStatEntry> allocatedStatsEntries = new List<CharacterData.AllocatedStatEntry>();
    public void updateThis(RPGRace newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        maleIcon = newData.maleIcon;
        femaleIcon = newData.femaleIcon;
        description = newData.description;
        malePrefab = newData.malePrefab;
        femalePrefab = newData.femalePrefab;
        availableClasses = newData.availableClasses;
        weaponTemplates = newData.weaponTemplates;
        stats = newData.stats;
        startingSceneID = newData.startingSceneID;
        startingPositionID = newData.startingPositionID;
        startItems = newData.startItems;
        displayName = newData.displayName;
        actionAbilities = newData.actionAbilities;
        factionID = newData.factionID;
        allocatedStatsEntries = newData.allocatedStatsEntries;
        allocationStatPoints = newData.allocationStatPoints;
        speciesID = newData.speciesID;
        dynamicAnimator = newData.dynamicAnimator;
        restAnimatorController = newData.restAnimatorController;
        combatAnimatorController = newData.combatAnimatorController;
    }
}