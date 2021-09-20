using System.Collections.Generic;
using UnityEngine;

public class RPGCombatDATA : ScriptableObject
{
    public string[] StatFunctions;
    public string[] UIStatsCategories;
    public string[] FactionStances;
    public List<string> StatFunctionsList = new List<string>();
    public List<string> UIStatsCategoriesList = new List<string>();
    public List<string> FactionStancesList = new List<string>();
    public List<string> AbilityCooldownTagList = new List<string>();
    public List<string> EffectTagList = new List<string>();
    
    public List<string> nodeSocketNames = new List<string>();
    
    public float CriticalDamageBonus = 2;

    public float GCDDuration = 0.75f;
    
    public int talentTreesNodePerTierCount = 6;

    public float outOfCombatDuration = 15;

    public bool useClasses = true;
    public bool targetPlayerOnClick = true;

    public bool useAutomaticCombatState = true;
    
    public RPGTreePoint pointREF;
    [PointID] public int pointID = -1;
    public bool spendAllStatPointsToCreateChar;
    public bool canDescreaseGameStatPoints;
    
    public RPGStat healthStatREF;
    [StatID] public int healthStatID = -1;

    [StatID] public int sprintStatDrainID = -1;
    public int sprintStatDrainAmount;
    public float sprintStatDrainInterval;
    
    public enum TARGET_TYPE
    {
        Target,
        Caster
    }
    public enum ALIGNMENT_TYPE
    {
        ALLY,
        NEUTRAL,
        ENEMY
    }
    
    
    public enum INTERACTABLE_TYPE
    {
        None,
        AlliedUnit,
        NeutralUnit,
        EnemyUnit,
        InteractiveNode,
        CraftingStation,
        LootBag,
        WorldDroppedItem
    }

    [System.Serializable]
    public class Faction_Reward_DATA
    {
        public RPGFaction factionREF;
        [FactionID] public int factionID = -1;

        public int amount;
    }

    [System.Serializable]
    public class ActionAbilityDATA
    {
        public ActionAbilityKeyType keyType;
        public KeyCode key;
        public string actionKeyName;
        public RPGAbility abilityREF;
        [AbilityID] public int abilityID = -1;
    }

    public enum ActionAbilityKeyType
    {
        OverrideKey,
        ActionKey
    }

    public enum CombatVisualActivationType
    {
        Activate,
        CastCompleted,
        Completed,
        Interrupted
    }
    
    [System.Serializable]
    public class CombatVisualEffect
    {
        public CombatVisualActivationType activationType;
        public GameObject EffectGO;
        public string SocketName;
        public bool UseNodeSocket;
        public bool ParentedToCaster;
        public AudioClip Sound;
        public bool SoundParentedToEffect;
        public Vector3 positionOffset;
        public Vector3 effectScale = Vector3.one;
        public float duration = 5, delay;
        public bool isDestroyedOnDeath = true;
        public bool isDestroyedOnStun = true;
        public bool isDestroyedOnStealth;
        public bool isDestroyedOnStealthEnd;
    }
    
    public enum CombatVisualAnimationParameterType
    {
        Bool,
        Int,
        Float,
        Trigger
    }
    
    [System.Serializable]
    public class CombatVisualAnimation
    {
        public CombatVisualActivationType activationType;
        public CombatVisualAnimationParameterType parameterType;
        public string animationParameter;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public float duration = 1;
        public float delay = 0;
        public bool showWeapons;
        public float showWeaponDuration;
    }
    
    public void updateThis(RPGCombatDATA newData)
    {
        CriticalDamageBonus = newData.CriticalDamageBonus;
        healthStatID = newData.healthStatID;
        outOfCombatDuration = newData.outOfCombatDuration;
        useClasses = newData.useClasses;
        useAutomaticCombatState = newData.useAutomaticCombatState;
        pointID = newData.pointID;
        spendAllStatPointsToCreateChar = newData.spendAllStatPointsToCreateChar;
        canDescreaseGameStatPoints = newData.canDescreaseGameStatPoints;
        talentTreesNodePerTierCount = newData.talentTreesNodePerTierCount;
        
        StatFunctionsList = newData.StatFunctionsList;
        UIStatsCategoriesList = newData.UIStatsCategoriesList;
        FactionStancesList = newData.FactionStancesList;
        nodeSocketNames = newData.nodeSocketNames;
        GCDDuration = newData.GCDDuration;
        AbilityCooldownTagList = newData.AbilityCooldownTagList;
        EffectTagList = newData.EffectTagList;
        
        targetPlayerOnClick = newData.targetPlayerOnClick;
        
        sprintStatDrainID = newData.sprintStatDrainID;
        sprintStatDrainInterval = newData.sprintStatDrainInterval;
        sprintStatDrainAmount = newData.sprintStatDrainAmount;
    }
}