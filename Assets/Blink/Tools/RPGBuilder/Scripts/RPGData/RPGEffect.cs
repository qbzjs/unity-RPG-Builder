using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using UnityEngine;
using UnityEngine.Events;

public class RPGEffect : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public Sprite icon;

    public enum EFFECT_TYPE
    {
        InstantDamage,
        InstantHeal,
        DamageOverTime,
        HealOverTime,
        Stat,
        Stun,
        Sleep,
        Immune,
        Shapeshifting,
        Dispel,
        Teleport,
        Taunt,
        Root,
        Silence,
        Pet,
        RollLootTable,
        Knockback,
        Motion,
        Blocking,
        Flying,
        Stealth
    }

    public enum MAIN_DAMAGE_TYPE
    {
        NONE,
        PHYSICAL_DAMAGE,
        MAGICAL_DAMAGE
    }
    
    [Serializable]
    public class STAT_EFFECTS_DATA
    {
        [StatID] public int statID = -1;
        public RPGStat statREF;
        public float statEffectModification;
        public bool isPercent;
    }
    
    public enum TELEPORT_TYPE
    {
        gameScene,
        position,
        target,
        directional
    }

    public enum TELEPORT_DIRECTIONAL_TYPE
    {
        forward,
        left,
        right,
        backward,
        diagonalTopLeft,
        diagonalTopRight,
        diagonalBackwardLeft,
        diagonalBackwardRight
    }
    
    public enum PET_TYPE
    {
        combat
    }
    
    public EFFECT_TYPE effectType;
    public string effectTag;
    public bool isState, isBuffOnSelf;
    public int stackLimit = 1;
    public bool allowMultiple, allowMixedCaster;
    public int pulses = 1;
    public float duration;
    public bool endless;

    public enum BLOCK_DURATION_TYPE
    {
        Time,
        HoldKey
    }
    public enum BLOCK_END_TYPE
    {
        HitCount,
        MaxDamageBlocked,
        Stat
    }
    public enum DISPEL_TYPE
    {
        EffectType,
        EffectTag,
        Effect
    }
    public enum ON_BLOCK_ACTION_TYPE
    {
        Ability,
        Effect
    }
    
    [Serializable]
    public class ON_BLOCK_ACTION
    {
        public ON_BLOCK_ACTION_TYPE blockActionType;
        public int entryID = -1;
        public RPGCombatDATA.TARGET_TYPE target;
        public int effectRank;
        public bool abMustBeKnown;
        public float chance = 100f, delay;
    }
    
    [Serializable]
    public class RPGEffectRankData
    {
        public bool ShowedInEditor;

        public MAIN_DAMAGE_TYPE mainDamageType;

        public string secondaryDamageType;

        public int Damage;
        public RPGStat alteredStatREF;
        [StatID] public int alteredStatID = -1;
        public bool FlatCalculation;
        public bool CannotCrit;
        public bool removeStealth = true;
        public float skillModifier, weaponDamageModifier;
        public bool useWeapon1Damage = true, useWeapon2Damage = true;
        [SkillID] public int skillModifierID = -1;
        public RPGSkill skillModifierREF;
        public float lifesteal;
        public float maxHealthModifier;
        public float missingHealthModifier;
        public float UltimateGain;
        public float delay;

        public int projectilesReflectedCount;

        public RPGEffect requiredEffectREF;
        [EffectID] public int requiredEffectID = -1;
        public float requiredEffectDamageModifier;

        public RPGStat damageStatREF;
        [StatID] public int damageStatID = -1;
        public float damageStatModifier;
        public RPGAbility.COST_TYPES hitValueType;
        
        public TELEPORT_TYPE teleportType;
        [GameSceneID] public int gameSceneID = -1;
        public RPGGameScene gameSceneREF;
        public Vector3 teleportPOS;
        public TELEPORT_DIRECTIONAL_TYPE teleportDirectionalType;
        public float teleportDirectionalDistance;
        public LayerMask teleportDirectionalBlockLayers;

        [LootTableID] public int lootTableID = -1;

        public PET_TYPE petType;
        [NPCID] public int petNPCDataID = -1;
        public float petDuration;
        public int petSPawnCount;
        public bool petScaleWithCharacter;

        public float knockbackDistance;
        
        public float motionDistance = 5;
        public float motionSpeed = 0.5f;
        public Vector3 motionDirection;
        public bool motionIgnoreUseCondition, isImmuneDuringMotion;

        public bool blockAnyDamage = true, blockPhysicalDamage, blockMagicalDamage, isBlockChargeTime,
            isBlockLimitedDuration, isBlockPowerDecay, isBlockKnockback, blockStatDecay;
        public float blockChargeTime = 0.5f, blockDuration = 1, blockPowerModifier = 100, blockPowerDecay = 0.1f, blockAngle = 90f, blockStatDecayInterval = 1;
        public int blockPowerFlat, blockHitCount = 1, blockMaxDamage, blockStatDecayAmount = 1;
        public BLOCK_DURATION_TYPE blockDurationType;
        public BLOCK_END_TYPE blockEndType;
        [StatID] public int blockStatID = -1;
        
        [RPGDataList] public List<string> blockedDamageTypes = new List<string>();

        public DISPEL_TYPE dispelType;
        public EFFECT_TYPE dispelEffectType;
        public string dispelEffectTag;
        [EffectID] public int dispelEffectID = -1;
        
        public GameObject shapeshiftingModel;
        public Vector3 shapeshiftingmodelPosition, shapeshiftingmodelScale = Vector3.one;
        public RuntimeAnimatorController shapeshiftingAnimatorController;
        public Avatar shapeshiftingAnimatorAvatar;
        public bool shapeshiftingAnimatorUseRootMotion;
        public AnimatorUpdateMode shapeshiftingAnimatorUpdateMode = AnimatorUpdateMode.Normal;
        public AnimatorCullingMode shapeshiftingAnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;
        public bool shapeshiftingOverrideMainActionBar, canCameraAim;
        [RPGDataList] public List<int> shapeshiftingActiveAbilities = new List<int>();

        public bool showStealthActionBar = true;
        
        [RPGDataList] public List<RPGAbility.AbilityEffectsApplied> nestedEffects = new List<RPGAbility.AbilityEffectsApplied>();
        
        [RPGDataList] public List<RPGCombatDATA.CombatVisualEffect> visualEffects = new List<RPGCombatDATA.CombatVisualEffect>();
        [RPGDataList] public List<RPGCombatDATA.CombatVisualAnimation> visualAnimations = new List<RPGCombatDATA.CombatVisualAnimation>();
        
        [RPGDataList] public List<STAT_EFFECTS_DATA> statEffectsData = new List<STAT_EFFECTS_DATA>();
        
        [RPGDataList] public List<RPGBGameActions> GameActionsList = new List<RPGBGameActions>();
        [RPGDataList] public List<ON_BLOCK_ACTION> onBlockActions = new List<ON_BLOCK_ACTION>();
    }
    [RPGDataList] public List<RPGEffectRankData> ranks = new List<RPGEffectRankData>();
    
    
    public void updateThis(RPGEffect newEffectDATA)
    {
        ID = newEffectDATA.ID;
        _name = newEffectDATA._name;
        _fileName = newEffectDATA._fileName;
        icon = newEffectDATA.icon;
        displayName = newEffectDATA.displayName;
        
        ranks = newEffectDATA.ranks;
        effectType = newEffectDATA.effectType;
        effectTag = newEffectDATA.effectTag;
        isState = newEffectDATA.isState;
        isBuffOnSelf = newEffectDATA.isBuffOnSelf;
        endless = newEffectDATA.endless;
        stackLimit = newEffectDATA.stackLimit;
        allowMultiple = newEffectDATA.allowMultiple;
        allowMixedCaster = newEffectDATA.allowMixedCaster;
        pulses = newEffectDATA.pulses;
        duration = newEffectDATA.duration;
    }

    public void copyData(RPGEffectRankData original, RPGEffectRankData copied)
    {

        original.mainDamageType = copied.mainDamageType;
        original.secondaryDamageType = copied.secondaryDamageType;
        original.Damage = copied.Damage;
        original.lifesteal = copied.lifesteal;
        original.maxHealthModifier = copied.maxHealthModifier;
        original.missingHealthModifier = copied.missingHealthModifier;
        original.UltimateGain = copied.UltimateGain;
        original.delay = copied.delay;

        original.teleportType = copied.teleportType;
        original.gameSceneID = copied.gameSceneID;
        original.teleportPOS = copied.teleportPOS;
        original.teleportDirectionalType = copied.teleportDirectionalType;
        original.teleportDirectionalDistance = copied.teleportDirectionalDistance;
        original.teleportDirectionalBlockLayers = copied.teleportDirectionalBlockLayers;

        original.petType = copied.petType;
        original.petNPCDataID = copied.petNPCDataID;
        original.petDuration = copied.petDuration;
        original.petSPawnCount = copied.petSPawnCount;
        original.projectilesReflectedCount = copied.projectilesReflectedCount;
        original.petScaleWithCharacter = copied.petScaleWithCharacter;

        original.knockbackDistance = copied.knockbackDistance;

        original.motionDistance = copied.motionDistance;
        original.motionSpeed = copied.motionSpeed;
        original.motionDirection = copied.motionDirection;
        original.isImmuneDuringMotion = copied.isImmuneDuringMotion;
        original.motionIgnoreUseCondition = copied.motionIgnoreUseCondition;

        original.blockChargeTime = copied.blockChargeTime;
        original.blockDuration = copied.blockDuration;
        original.blockHitCount = copied.blockHitCount;
        original.blockPowerFlat = copied.blockPowerFlat;
        original.blockPowerModifier = copied.blockPowerModifier;
        original.blockPowerDecay = copied.blockPowerDecay;
        original.blockAngle = copied.blockAngle;
        original.blockedDamageTypes = copied.blockedDamageTypes;
        original.blockAnyDamage = copied.blockAnyDamage;
        original.blockPhysicalDamage = copied.blockPhysicalDamage;
        original.blockMagicalDamage = copied.blockMagicalDamage;
        original.isBlockChargeTime = copied.isBlockChargeTime;
        original.isBlockLimitedDuration = copied.isBlockLimitedDuration;
        original.isBlockPowerDecay = copied.isBlockPowerDecay;
        original.isBlockKnockback = copied.isBlockKnockback;
        original.blockDurationType = copied.blockDurationType;
        original.blockEndType = copied.blockEndType;
        original.blockMaxDamage = copied.blockMaxDamage;
        original.blockStatID = copied.blockStatID;
        original.blockStatDecay = copied.blockStatDecay;
        original.blockStatDecayAmount = copied.blockStatDecayAmount;
        original.blockStatDecayInterval = copied.blockStatDecayInterval;

        original.dispelType = copied.dispelType;
        original.dispelEffectTag = copied.dispelEffectTag;
        original.dispelEffectType = copied.dispelEffectType;
        original.dispelEffectID = copied.dispelEffectID;

        original.skillModifier = copied.skillModifier;
        original.skillModifierID = copied.skillModifierID;

        original.alteredStatID = copied.alteredStatID;
        original.FlatCalculation = copied.FlatCalculation;
        original.weaponDamageModifier = copied.weaponDamageModifier;
        original.useWeapon1Damage = copied.useWeapon1Damage;
        original.useWeapon2Damage = copied.useWeapon2Damage;
        
        original.CannotCrit = copied.CannotCrit;
        original.removeStealth = copied.removeStealth;

        original.requiredEffectID = copied.requiredEffectID;
        original.requiredEffectDamageModifier = copied.requiredEffectDamageModifier;

        original.damageStatID = copied.damageStatID;
        original.damageStatModifier = copied.damageStatModifier;
        original.hitValueType = copied.hitValueType;

        original.lootTableID = copied.lootTableID;

        original.shapeshiftingModel = copied.shapeshiftingModel;
        original.shapeshiftingmodelPosition = copied.shapeshiftingmodelPosition;
        original.shapeshiftingmodelScale = copied.shapeshiftingmodelScale;
        original.shapeshiftingAnimatorController = copied.shapeshiftingAnimatorController;
        original.shapeshiftingAnimatorAvatar = copied.shapeshiftingAnimatorAvatar;
        original.shapeshiftingAnimatorUseRootMotion = copied.shapeshiftingAnimatorUseRootMotion;
        original.shapeshiftingAnimatorUpdateMode = copied.shapeshiftingAnimatorUpdateMode;
        original.shapeshiftingAnimatorCullingMode = copied.shapeshiftingAnimatorCullingMode;
        original.shapeshiftingOverrideMainActionBar = copied.shapeshiftingOverrideMainActionBar;
        original.canCameraAim = copied.canCameraAim;
        original.shapeshiftingActiveAbilities = copied.shapeshiftingActiveAbilities;
        original.showStealthActionBar = copied.showStealthActionBar;

        original.nestedEffects = new List<RPGAbility.AbilityEffectsApplied>();
        for (var index = 0; index < copied.nestedEffects.Count; index++)
        {
            RPGAbility.AbilityEffectsApplied newRef = new RPGAbility.AbilityEffectsApplied();
            newRef.chance = copied.nestedEffects[index].chance;
            newRef.target = copied.nestedEffects[index].target;
            newRef.effectID = copied.nestedEffects[index].effectID;
            newRef.effectRank = copied.nestedEffects[index].effectRank;
            newRef.isSpread = copied.nestedEffects[index].isSpread;
            newRef.spreadDistanceMax = copied.nestedEffects[index].spreadDistanceMax;
            newRef.spreadUnitMax = copied.nestedEffects[index].spreadUnitMax;
            original.nestedEffects.Add(newRef);
        }

        original.visualEffects = new List<RPGCombatDATA.CombatVisualEffect>();
        for (var index = 0; index < copied.visualEffects.Count; index++)
        {
            RPGCombatDATA.CombatVisualEffect newRef = new RPGCombatDATA.CombatVisualEffect();
            newRef.Sound = copied.visualEffects[index].Sound;
            newRef.SocketName = copied.visualEffects[index].SocketName;
            newRef.EffectGO = copied.visualEffects[index].EffectGO;
            newRef.ParentedToCaster = copied.visualEffects[index].ParentedToCaster;
            newRef.SoundParentedToEffect = copied.visualEffects[index].SoundParentedToEffect;
            newRef.UseNodeSocket = copied.visualEffects[index].UseNodeSocket;
            newRef.isDestroyedOnDeath = copied.visualEffects[index].isDestroyedOnDeath;
            newRef.isDestroyedOnStun = copied.visualEffects[index].isDestroyedOnStun;
            newRef.isDestroyedOnStealth = copied.visualEffects[index].isDestroyedOnStealth;
            newRef.isDestroyedOnStealthEnd = copied.visualEffects[index].isDestroyedOnStealthEnd;
            original.visualEffects.Add(newRef);
        }

        original.visualAnimations = new List<RPGCombatDATA.CombatVisualAnimation>();
        for (var index = 0; index < copied.visualAnimations.Count; index++)
        {
            RPGCombatDATA.CombatVisualAnimation newRef = new RPGCombatDATA.CombatVisualAnimation();
            newRef.delay = copied.visualAnimations[index].delay;
            newRef.duration = copied.visualAnimations[index].duration;
            newRef.activationType = copied.visualAnimations[index].activationType;
            newRef.animationParameter = copied.visualAnimations[index].animationParameter;
            newRef.parameterType = copied.visualAnimations[index].parameterType;
            original.visualAnimations.Add(newRef);
        }

        original.statEffectsData = new List<STAT_EFFECTS_DATA>();
        for (var index = 0; index < copied.statEffectsData.Count; index++)
        {
            STAT_EFFECTS_DATA newRef = new STAT_EFFECTS_DATA();
            newRef.isPercent = copied.statEffectsData[index].isPercent;
            newRef.statEffectModification = copied.statEffectsData[index].statEffectModification;
            newRef.statID = copied.statEffectsData[index].statID;
            original.statEffectsData.Add(newRef);
        }

        original.onBlockActions = new List<ON_BLOCK_ACTION>();
        for (var index = 0; index < copied.onBlockActions.Count; index++)
        {
            ON_BLOCK_ACTION newRef = new ON_BLOCK_ACTION();
            newRef.entryID = copied.onBlockActions[index].entryID;
            newRef.effectRank = copied.onBlockActions[index].effectRank;
            newRef.target = copied.onBlockActions[index].target;
            newRef.blockActionType = copied.onBlockActions[index].blockActionType;
            newRef.abMustBeKnown = copied.onBlockActions[index].abMustBeKnown;
            newRef.chance = copied.onBlockActions[index].chance;
            newRef.delay = copied.onBlockActions[index].delay;
            original.onBlockActions.Add(newRef);
        }
    }
}