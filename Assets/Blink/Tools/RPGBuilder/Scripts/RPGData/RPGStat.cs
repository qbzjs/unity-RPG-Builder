using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGStat : ScriptableObject
{
    [Header("-----BASE DATA-----")]
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;
    public bool minCheck;
    public float minValue;
    public bool maxCheck;
    public float maxValue;
    public float baseValue;
    
    
    public bool isShiftingInSprint = true;
    public bool isShiftingInBlock = true;
    public bool isShiftingOutsideCombat;
    public bool isShiftingInCombat;
    public float shiftAmountOutsideCombat;
    public float shiftIntervalOutsideCombat;
    public float shiftAmountInCombat;
    public float shiftIntervalInCombat;
    public bool StartsAtMax = true;
    
    public bool isPercentStat;

    public enum VitalityActionsTypes
    {
        Death,
        Effect,
        Ability,
        ResetActiveBlock,
        ResetSprint
    }
    
    public enum VitalityActionsValueType
    {
        Equal,
        EqualOrBelow,
        EqualOrAbove,
        Below,
        Above
    }
    
    [Serializable]
    public class VitalityActions
    {
        public VitalityActionsTypes type;
        public float value;
        public VitalityActionsValueType valueType;
        public bool isPercent;

        public RPGAbility abilityREF;
        public RPGEffect effectREF;
        public int effectRank;
        public RPGItem itemREF;

        public int elementID = -1;

        public int itemCount;
    }
    [RPGDataList] public List<VitalityActions> vitalityActions = new List<VitalityActions>();
    
    [Serializable]
    public class OnHitEffectsData
    {
        public RPGEffect effectREF;
        [EffectID] public int effectID = -1;
        public int effectRank;
        public RPGCombatDATA.TARGET_TYPE targetType;
        public RPGAbility.ABILITY_TAGS tagType;
        public float chance = 100f;
    }
    
    [RPGDataList] public List<OnHitEffectsData> onHitEffectsData = new List<OnHitEffectsData>();


    public bool isVitalityStat;
    [Serializable]
    public class StatBonusData
    {
        public STAT_TYPE statType;
        public float modifyValue = 1;
        
        public string StatFunction;
        public string OppositeStat;
        
        [StatID] public int statID = -1;
    }
    [RPGDataList] public List<StatBonusData> statBonuses = new List<StatBonusData>();
    
    public enum STAT_TYPE
    {
        NONE,
        DAMAGE,
        RESISTANCE,
        PENETRATION,
        HEALING,
        ABSORBTION,
        CC_POWER,
        CC_RESISTANCE,
        DMG_TAKEN,
        DMG_DEALT,
        HEAL_RECEIVED,
        HEAL_DONE,
        CAST_SPEED,
        CRIT_CHANCE,
        BASE_DAMAGE_TYPE,
        BASE_RESISTANCE_TYPE,
        SUMMON_COUNT,
        CD_RECOVERY_SPEED,
        GLOBAL_HEALING,
        LIFESTEAL,
        THORN,
        BLOCK_CHANCE,
        BLOCK_FLAT,
        BLOCK_MODIFIER,
        DODGE_CHANCE,
        GLANCING_BLOW_CHANCE,
        CRIT_POWER,
        DOT_BONUS,
        HOT_BONUS,
        HEALTH_ON_HIT,
        HEALTH_ON_KILL,
        HEALTH_ON_BLOCK,
        EFFECT_TRIGGER,
        LOOT_CHANCE_MODIFIER,
        EXPERIENCE_MODIFIER,
        VITALITY_REGEN,
        MINION_DAMAGE,
        MINION_PHYSICAL_DAMAGE,
        MINION_MAGICAL_DAMAGE,
        MINION_HEALTH,
        MINION_CRIT_CHANCE,
        MINION_CRIT_POWER,
        MINION_DURATION,
        MINION_LIFESTEAL,
        MINION_THORN,
        MINION_DODGE_CHANCE,
        MINION_GLANCING_BLOW_CHANCE,
        MINION_HEALTH_ON_HIT,
        MINION_HEALTH_ON_KILL,
        MINION_HEALTH_ON_BLOCK,
        PROJECTILE_SPEED,
        PROJECTILE_ANGLE_SPREAD,
        PROJECTILE_RANGE,
        PROJECTILE_COUNT,
        AOE_RADIUS,
        ABILITY_MAX_HIT,
        ABILITY_TARGET_MAX_RANGE,
        ABILITY_TARGET_MIN_RANGE,
        ATTACK_SPEED,
        BODY_SCALE,
        GCD_DURATION,
        BLOCK_ACTIVE_ANGLE,
        BLOCK_ACTIVE_COUNT,
        BLOCK_ACTIVE_CHARGE_TIME_SPEED_MODIFIER,
        BLOCK_ACTIVE_DURATION_MODIFIER,
        BLOCK_ACTIVE_DECAY_MODIFIER,
        BLOCK_ACTIVE_FLAT_AMOUNT,
        BLOCK_ACTIVE_PERCENT_AMOUNT,
        MOVEMENT_SPEED
    }

    public string StatUICategory;

    public void updateThis(RPGStat newStatDATA)
    {
        ID = newStatDATA.ID;
        _name = newStatDATA._name;
        _fileName = newStatDATA._fileName;
        isShiftingInSprint = newStatDATA.isShiftingInSprint;
        isShiftingInBlock = newStatDATA.isShiftingInBlock;
        isShiftingInCombat = newStatDATA.isShiftingInCombat;
        isShiftingOutsideCombat = newStatDATA.isShiftingOutsideCombat;
        shiftAmountInCombat = newStatDATA.shiftAmountInCombat;
        shiftIntervalInCombat = newStatDATA.shiftIntervalInCombat;
        shiftAmountOutsideCombat = newStatDATA.shiftAmountOutsideCombat;
        shiftIntervalOutsideCombat = newStatDATA.shiftIntervalOutsideCombat;
        minValue = newStatDATA.minValue;
        maxValue = newStatDATA.maxValue;
        baseValue = newStatDATA.baseValue;
        displayName = newStatDATA.displayName;
        description = newStatDATA.description;
        StatUICategory = newStatDATA.StatUICategory;
        minCheck = newStatDATA.minCheck;
        maxCheck = newStatDATA.maxCheck;
        onHitEffectsData = newStatDATA.onHitEffectsData;
        isPercentStat = newStatDATA.isPercentStat;
        vitalityActions = newStatDATA.vitalityActions;
        statBonuses = newStatDATA.statBonuses;
        isVitalityStat = newStatDATA.isVitalityStat;
        StartsAtMax = newStatDATA.StartsAtMax;
    }
}