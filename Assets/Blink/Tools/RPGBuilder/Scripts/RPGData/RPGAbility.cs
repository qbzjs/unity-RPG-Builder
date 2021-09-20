using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using UnityEngine.Serialization;

public class RPGAbility : ScriptableObject
{
    [Header("-----BASE DATA-----")]
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public Sprite icon;
    public bool learnedByDefault;

    public enum AbilityType
    {
        Normal,
        PlayerAutoAttack,
        PlayerActionAbility
    }
    public AbilityType abilityType;

    public enum TARGET_TYPES
    {
        SELF,
        CONE,
        AOE,
        LINEAR,
        PROJECTILE,
        SQUARE,
        GROUND,
        GROUND_LEAP,
        TARGET_PROJECTILE,
        TARGET_INSTANT
    }

    public enum ABILITY_TAGS
    {
        onHit,
        onKill,
        shapeshifting,
        stealth
    }
    
    
    public enum AbilityActivationType
    {
        Instant,
        Casted,
        Channeled,
        Charged
    }

    [Serializable]
    public class AbilityEffectsApplied
    {
        [EffectID] public int effectID = -1;
        public float chance = 100f;
        public RPGEffect effectREF;
        public int effectRank;
        public RPGCombatDATA.TARGET_TYPE target;

        public bool isSpread;
        public float spreadDistanceMax;
        public int spreadUnitMax;

        public float delay;
    }

    public enum COST_TYPES
    {
        FLAT,
        PERCENT_OF_MAX,
        PERCENT_OF_CURRENT
    }

    [Serializable]
    public class AbilityTagsData
    {
        public ABILITY_TAGS tag;
        [EffectID] public int effectID = -1;
    }

    [Serializable]
    public class ConditionalEffectsData
    {
        public RPGCombatDATA.TARGET_TYPE targetType;
        public RPGStat.VitalityActionsValueType valueType;
        public float value;
        public bool isPercent;
        public RPGStat statREF;
        [StatID] public int statID = -1;

        public RPGEffect effectREF;
        [EffectID] public int effectID = -1;
        public int effectRank;

        public RPGCombatDATA.TARGET_TYPE requiredTargetType;
        public RPGEffect effectRequiredREF;
        [EffectID] public int effectRequiredID = -1;

        public bool isSpread;
        public float spreadDistanceMax;
        public int spreadUnitMax;

        public bool consumeRequiredEffect;
        
        public float delay;
    }


    [Serializable]
    public class HIT_SETTINGS
    {
        public RPGCombatDATA.ALIGNMENT_TYPE alignment;
        public bool hitPlayer, hitAlly, hitNeutral, hitEnemy, hitSelf, hitPet, hitOwner;
    }    
    
    
    [Serializable]
    public class USE_REQUIREMENT_EFFECTS
    {
        public RPGEffect effectRequiredREF;
        [EffectID] public int effectRequiredID = -1;
        public bool consumeEffect;
        public RPGCombatDATA.TARGET_TYPE target;
    }  
    
    [Serializable]
    public class RPGAbilityRankData
    {
        public bool ShowedInEditor;
        public int unlockCost;

        public AbilityActivationType activationType;
        public float castTime;
        public bool castInRun;
        public bool castBarVisible = true;
        public bool faceCursorWhileCasting = true;
        public bool faceCursorWhenOnCastStart = true;
        public bool faceCursorWhenOnCastEnd = true;
        public bool canBeUsedStunned;
        public bool animationTriggered;
        public bool comboStarsAfterCastComplete;
        public bool cancelStealth;

        [RPGDataList] public List<USE_REQUIREMENT_EFFECTS> effectsRequirements = new List<USE_REQUIREMENT_EFFECTS>(); 

        [RPGDataList] public List<RequirementsManager.AbilityUseRequirementDATA> useRequirements =
            new List<RequirementsManager.AbilityUseRequirementDATA>();

        public TARGET_TYPES targetType;

        [RPGDataList] public List<HIT_SETTINGS> HitSettings = new List<HIT_SETTINGS>();

        public bool isToggle, isToggleCostOnTrigger;
        public float toggledTriggerInterval;
        
        public int MaxUnitHit = 1;
        public float minRange;
        public float maxRange;

        public float standTimeDuration;
        public bool canRotateInStandTime;
        public float castSpeedSlowAmount;
        public float castSpeedSlowTime;
        public float castSpeedSlowRate;

        public float coneDegree;
        public int ConeHitCount = 1;
        public float ConeHitInterval;
        public float AOERadius;
        public int AOEHitCount = 1;
        public float AOEHitInterval;
        public float linearWidth;
        public float linearHeight;
        public float linearLength;
        public float projectileSpeed;
        public float projectileDistance;
        public float projectileAngleSpread;
        public int projectileCount = 1;
        public float projectileDelay;
        public float projectileDuration = 5;
        public float projectileComeBackTime;
        public float projectileComeBackSpeed;
        public bool isProjectileComeBack;
        public float projectileNearbyUnitDistanceMax;
        public float projectileNearbyUnitMaxHit;
        public bool isProjectileNearbyUnit;
        public bool projectileDestroyedByEnvironment = true;
        public bool projectileAffectedByGravity;
        public bool projectileShootOnClickPosition;
        public bool mustLookAtTarget = true;
        public float squareWidth;
        public float squareLength;
        public float squareHeight;
        public float groundRadius;
        public float groundRange;
        public float groundHitTime;
        public int groundHitCount = 1;
        public float groundHitInterval;

        public GameObject projectileEffect;
        public string projectileSocketName;
        public string projectileTargetSocketName;
        public bool projectileUseNodeSocket;
        public bool projectileTargetUseNodeSocket;
        public bool projectileParentedToCaster;
        public AudioClip projectileSound;
        public bool projectileSoundParentedToEffect;
        
        public bool useCustomCollision;
        public RPGNpc.NPCColliderType projectileColliderType;
        public Vector3 colliderCenter, colliderSize;
        public float colliderRadius, colliderHeight;
        
        public GameObject groundVisualEffect;
        public Vector3 effectPositionOffset;
        public float groundVisualEffectDuration = 5;

        public bool hitEffectUseSocket;
        public GameObject hitEffect;
        public float hitEffectDuration;
        public string hitEffectSocketName;
        public Vector3 hitEffectPositionOffset;
        public bool hitAttachedToNode;
        
        public float channelTime;
        public float groundLeapDuration;
        public float groundLeapHeight;
        public float groundLeapSpeed;

        public float cooldown;
        public bool isGCD;
        public bool startCDOnActivate = true;
        public bool CanUseDuringGCD;
        public bool isSharingCooldown;
        public string cooldownTag;

        [RPGDataList] public List<AbilityEffectsApplied> effectsApplied = new List<AbilityEffectsApplied>();
        [RPGDataList] public List<AbilityEffectsApplied> casterEffectsApplied = new List<AbilityEffectsApplied>();

        [RPGDataList] public List<RPGCombatDATA.CombatVisualEffect> visualEffects = new List<RPGCombatDATA.CombatVisualEffect>();
        [RPGDataList] public List<RPGCombatDATA.CombatVisualAnimation> visualAnimations = new List<RPGCombatDATA.CombatVisualAnimation>();
        [RPGDataList] public List<AbilityTagsData> tagsData = new List<AbilityTagsData>();

        [RPGDataList] public List<ConditionalEffectsData> conditionalEffects =
            new List<ConditionalEffectsData>();

        public RPGAbility extraAbilityExecuted;
        public RPGCombatDATA.CombatVisualActivationType extraAbilityExecutedActivationType;

    }

    [RPGDataList] public List<RPGAbilityRankData> ranks = new List<RPGAbilityRankData>();

    public void updateThis(RPGAbility newAbilityDATA)
    {
        ID = newAbilityDATA.ID;
        _name = newAbilityDATA._name;
        _fileName = newAbilityDATA._fileName;
        icon = newAbilityDATA.icon;

        ranks = newAbilityDATA.ranks;
        learnedByDefault = newAbilityDATA.learnedByDefault;
        abilityType = newAbilityDATA.abilityType;
        displayName = newAbilityDATA.displayName;
    }

    public void copyData(RPGAbilityRankData original, RPGAbilityRankData copied)
    {
        original.unlockCost = copied.unlockCost;
        original.activationType = copied.activationType;
        original.castTime = copied.castTime;
        original.castInRun = copied.castInRun;
        original.castBarVisible = copied.castBarVisible;
        original.faceCursorWhileCasting = copied.faceCursorWhileCasting;
        original.faceCursorWhenOnCastEnd = copied.faceCursorWhenOnCastEnd;
        original.faceCursorWhenOnCastStart = copied.faceCursorWhenOnCastStart;
        original.canBeUsedStunned = copied.canBeUsedStunned;
        original.animationTriggered = copied.animationTriggered;
        original.useRequirements = copied.useRequirements;
        original.comboStarsAfterCastComplete = copied.comboStarsAfterCastComplete;
        original.cancelStealth = copied.cancelStealth;

        original.targetType = copied.targetType;

        original.isToggle = copied.isToggle;
        original.isToggleCostOnTrigger = copied.isToggleCostOnTrigger;
        original.toggledTriggerInterval = copied.toggledTriggerInterval;
        
        
        original.MaxUnitHit = copied.MaxUnitHit;
        original.minRange = copied.minRange;
        original.maxRange = copied.maxRange;

        original.standTimeDuration = copied.standTimeDuration;
        original.canRotateInStandTime = copied.canRotateInStandTime;
        original.castSpeedSlowAmount = copied.castSpeedSlowAmount;
        original.castSpeedSlowTime = copied.castSpeedSlowTime;
        original.castSpeedSlowRate = copied.castSpeedSlowRate;

        original.coneDegree = copied.coneDegree;
        original.ConeHitCount = copied.ConeHitCount;
        original.ConeHitInterval = copied.ConeHitInterval;
        original.AOERadius = copied.AOERadius;
        original.AOEHitCount = copied.AOEHitCount;
        original.AOEHitInterval = copied.AOEHitInterval;
        original.linearWidth = copied.linearWidth;
        original.linearHeight = copied.linearHeight;
        original.linearLength = copied.linearLength;
        original.projectileSpeed = copied.projectileSpeed;
        original.projectileDistance = copied.projectileDistance;
        original.projectileAngleSpread = copied.projectileAngleSpread;
        original.projectileCount = copied.projectileCount;
        original.projectileDelay = copied.projectileDelay;
        original.projectileComeBackTime = copied.projectileComeBackTime;
        original.projectileComeBackSpeed = copied.projectileComeBackSpeed;
        original.isProjectileComeBack = copied.isProjectileComeBack;
        original.projectileNearbyUnitDistanceMax = copied.projectileNearbyUnitDistanceMax;
        original.projectileNearbyUnitMaxHit = copied.projectileNearbyUnitMaxHit;
        original.isProjectileNearbyUnit = copied.isProjectileNearbyUnit;
        original.projectileDestroyedByEnvironment = copied.projectileDestroyedByEnvironment;
        original.projectileAffectedByGravity = copied.projectileAffectedByGravity;
        original.projectileShootOnClickPosition = copied.projectileShootOnClickPosition;
        original.mustLookAtTarget = copied.mustLookAtTarget;
        original.squareWidth = copied.squareWidth;
        original.squareLength = copied.squareLength;
        original.squareHeight = copied.squareHeight;
        original.groundRadius = copied.groundRadius;
        original.groundRange = copied.groundRange;
        original.groundHitTime = copied.groundHitTime;
        original.groundHitCount = copied.groundHitCount;
        original.groundHitInterval = copied.groundHitInterval;
    
        original.projectileEffect = copied.projectileEffect;
        original.projectileSocketName = copied.projectileSocketName;
        original.projectileTargetSocketName = copied.projectileTargetSocketName;
        original.hitEffectSocketName = copied.hitEffectSocketName;
        original.projectileUseNodeSocket = copied.projectileUseNodeSocket;
        original.projectileTargetUseNodeSocket = copied.projectileTargetUseNodeSocket;
        original.hitEffect = copied.hitEffect;
        original.hitEffectDuration = copied.hitEffectDuration;
        original.projectileParentedToCaster = copied.projectileParentedToCaster;
        original.projectileSound = copied.projectileSound;
        original.projectileSoundParentedToEffect = copied.projectileSoundParentedToEffect;
        original.useCustomCollision = copied.useCustomCollision;
        original.projectileColliderType = copied.projectileColliderType;
        original.colliderCenter = copied.colliderCenter;
        original.colliderRadius = copied.colliderRadius;
        original.colliderSize = copied.colliderSize;
        original.colliderHeight = copied.colliderHeight;
        original.groundVisualEffect = copied.groundVisualEffect;
        original.effectPositionOffset = copied.effectPositionOffset;
        
        original.channelTime = copied.channelTime;
        original.groundLeapDuration = copied.groundLeapDuration;
        original.groundLeapHeight = copied.groundLeapHeight;
        original.groundLeapSpeed = copied.groundLeapSpeed;

        original.cooldown = copied.cooldown;
        original.isGCD = copied.isGCD;
        original.startCDOnActivate = copied.startCDOnActivate;
        original.CanUseDuringGCD = copied.CanUseDuringGCD;
        original.isSharingCooldown = copied.isSharingCooldown;

        original.effectsApplied = new List<AbilityEffectsApplied>();
        for (var index = 0; index < copied.effectsApplied.Count; index++)
        {
            AbilityEffectsApplied newRef = new AbilityEffectsApplied();
            newRef.chance = copied.effectsApplied[index].chance;
            newRef.target = copied.effectsApplied[index].target;
            newRef.effectID = copied.effectsApplied[index].effectID;
            newRef.effectRank = copied.effectsApplied[index].effectRank;
            newRef.isSpread = copied.effectsApplied[index].isSpread;
            newRef.spreadDistanceMax = copied.effectsApplied[index].spreadDistanceMax;
            newRef.spreadUnitMax = copied.effectsApplied[index].spreadUnitMax;
            original.effectsApplied.Add(newRef);
        }
        
        original.casterEffectsApplied = new List<AbilityEffectsApplied>();
        for (var index = 0; index < copied.casterEffectsApplied.Count; index++)
        {
            AbilityEffectsApplied newRef = new AbilityEffectsApplied();
            newRef.chance = copied.casterEffectsApplied[index].chance;
            newRef.target = copied.casterEffectsApplied[index].target;
            newRef.effectID = copied.casterEffectsApplied[index].effectID;
            newRef.effectRank = copied.casterEffectsApplied[index].effectRank;
            newRef.isSpread = copied.casterEffectsApplied[index].isSpread;
            newRef.spreadDistanceMax = copied.casterEffectsApplied[index].spreadDistanceMax;
            newRef.spreadUnitMax = copied.casterEffectsApplied[index].spreadUnitMax;
            original.casterEffectsApplied.Add(newRef);
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
            newRef.duration = copied.visualEffects[index].duration;
            newRef.delay = copied.visualEffects[index].delay;
            newRef.activationType = copied.visualEffects[index].activationType;
            newRef.effectScale = copied.visualEffects[index].effectScale;
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
            newRef.showWeapons = copied.visualAnimations[index].showWeapons;
            newRef.showWeaponDuration = copied.visualAnimations[index].showWeaponDuration;
            newRef.floatValue = copied.visualAnimations[index].floatValue;
            newRef.boolValue = copied.visualAnimations[index].boolValue;
            newRef.intValue = copied.visualAnimations[index].intValue;
            original.visualAnimations.Add(newRef);
        }
        original.tagsData = new List<AbilityTagsData>();
        for (var index = 0; index < copied.tagsData.Count; index++)
        {
            AbilityTagsData newRef = new AbilityTagsData();
            newRef.tag = copied.tagsData[index].tag;
            newRef.effectID = copied.tagsData[index].effectID;
            original.tagsData.Add(newRef);
        }
        original.HitSettings = new List<HIT_SETTINGS>();
        for (var index = 0; index < copied.HitSettings.Count; index++)
        {
            HIT_SETTINGS newRef = new HIT_SETTINGS();
            newRef.alignment = copied.HitSettings[index].alignment;
            newRef.hitAlly = copied.HitSettings[index].hitAlly;
            newRef.hitEnemy = copied.HitSettings[index].hitEnemy;
            newRef.hitPlayer = copied.HitSettings[index].hitPlayer;
            newRef.hitNeutral = copied.HitSettings[index].hitNeutral;
            newRef.hitSelf = copied.HitSettings[index].hitSelf;
            newRef.hitPet = copied.HitSettings[index].hitPet;
            newRef.hitOwner = copied.HitSettings[index].hitOwner;
            original.HitSettings.Add(newRef);
        }
        original.conditionalEffects = new List<ConditionalEffectsData>();
        for (var index = 0; index < copied.conditionalEffects.Count; index++)
        {
            ConditionalEffectsData newRef = new ConditionalEffectsData();
            newRef.value = copied.conditionalEffects[index].value;
            newRef.spreadDistanceMax = copied.conditionalEffects[index].spreadDistanceMax;
            newRef.isPercent = copied.conditionalEffects[index].isPercent;
            newRef.isSpread = copied.conditionalEffects[index].isSpread;
            newRef.targetType = copied.conditionalEffects[index].targetType;
            newRef.valueType = copied.conditionalEffects[index].valueType;
            newRef.effectID = copied.conditionalEffects[index].effectID;
            newRef.effectRank = copied.conditionalEffects[index].effectRank;
            newRef.requiredTargetType = copied.conditionalEffects[index].requiredTargetType;
            newRef.spreadUnitMax = copied.conditionalEffects[index].spreadUnitMax;
            newRef.statID = copied.conditionalEffects[index].statID;
            newRef.effectRequiredID = copied.conditionalEffects[index].effectRequiredID;
            original.conditionalEffects.Add(newRef);
        }
        
        original.effectsRequirements = new List<USE_REQUIREMENT_EFFECTS>();
        for (var index = 0; index < copied.effectsRequirements.Count; index++)
        {
            USE_REQUIREMENT_EFFECTS newRef = new USE_REQUIREMENT_EFFECTS();
            newRef.effectRequiredID = copied.effectsRequirements[index].effectRequiredID;
            newRef.consumeEffect = copied.effectsRequirements[index].consumeEffect;
            original.effectsRequirements.Add(newRef);
        }

        original.extraAbilityExecuted = copied.extraAbilityExecuted;
        original.extraAbilityExecutedActivationType = copied.extraAbilityExecutedActivationType;

    }

}