using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.AI;

public class RPGNpc : ScriptableObject
{
    public enum ABILITY_CONDITION_TYPE
    {
        NONE,
        STAT_ABOVE,
        STAT_BELOW
    }

    public enum NPC_TYPE
    {
        MOB,
        RARE,
        BOSS,
        MERCHANT,
        BANK,
        QUEST_GIVER,
        DIALOGUE
    }

    public NPC_TYPE npcType;

    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public Sprite icon;

    public bool isDummyTarget;

    public RPGFaction factionREF;
    [FactionID] public int factionID = -1;
    
    [SpeciesID] public int speciesID = -1;
    
    [MerchantTableID] public int merchantTableID = -1;
    public RPGMerchantTable merchantTableREF;
    
    [DialogueID] public int dialogueID = -1;
    public RPGDialogue dialogueREF;

    [Serializable]
    public class NPC_QUEST_DATA
    {
        public int questID = -1;
        public RPGQuest questREF;
    }

    [RPGDataList] public List<NPC_QUEST_DATA> questGiven = new List<NPC_QUEST_DATA>();
    [RPGDataList] public List<NPC_QUEST_DATA> questCompleted = new List<NPC_QUEST_DATA>();

    public float AggroRange;
    public float distanceFromTarget;
    public float distanceFromOwner;
    public float DistanceToTargetReset;

    public float RoamRange;
    public float RoamDelay;

    public float MinRespawn;
    public float MaxRespawn;
    
    public float corpseDespawnTime = 15;

    public int MinEXP;
    public int MaxEXP;
    public int EXPBonusPerLevel;

    public List<RPGCombatDATA.Faction_Reward_DATA> factionRewards = new List<RPGCombatDATA.Faction_Reward_DATA>();

    public int MinLevel;
    public int MaxLevel;
    public bool isScalingWithPlayer;

    [Serializable]
    public class NPC_ABILITY_DATA
    {
        [AbilityID] public int abilityID = -1;
        public RPGAbility abilityREF;
        public ABILITY_CONDITION_TYPE condition;
    }

    [RPGDataList] public List<NPC_ABILITY_DATA> abilities = new List<NPC_ABILITY_DATA>();


    [Serializable]
    public class NPC_STATS_DATA
    {
        [StatID] public int statID = -1;
        public RPGStat statREF;
        public float minValue;
        public float maxValue;
        public float baseValue;
        public float bonusPerLevel;
        
        [RPGDataList] public List<RPGStat.VitalityActions> vitalityActions = new List<RPGStat.VitalityActions>();
    }

    [RPGDataList] public List<NPC_STATS_DATA> stats = new List<NPC_STATS_DATA>();

    [Serializable]
    public class LOOT_TABLES
    {
        [LootTableID] public int lootTableID = -1;
        public RPGLootTable lootTableREF;
        public float dropRate = 100f;
    }

    [RPGDataList] public List<LOOT_TABLES> lootTables = new List<LOOT_TABLES>();

    public bool isCombatEnabled = true;
    public bool isMovementEnabled = true;
    public bool isCollisionEnabled;
    public bool isTargetable = true;
    public bool isNameplateEnabled = true;
    public bool isPlayerInteractable = true;
    
    // PREFAB DATA
    public GameObject NPCVisual;
    public Vector3 modelPosition, modelScale = Vector3.one;

    public float nameplateYOffset;
    
    public RuntimeAnimatorController animatorController;
    public Avatar animatorAvatar;

    public bool animatorUseRootMotion;
    public AnimatorUpdateMode animatorUpdateMode = AnimatorUpdateMode.Normal;
    public AnimatorCullingMode AnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;

    public float navmeshAgentRadius, navmeshAgentHeight, navmeshAgentAngularSpeed = 400;
    public ObstacleAvoidanceType navmeshObstacleAvoidance;
    

    public enum NPCColliderType
    {
        Capsule,
        Sphere,
        Box
    }

    public NPCColliderType colliderType;

    public Vector3 colliderCenter, colliderSize;
    public float colliderRadius, colliderHeight;
    
    [Serializable]
    public class NPC_AGGRO_LINK
    {
        [NPCID] public int npcID = -1;
        public float maxDistance;
    }

    [RPGDataList] public List<NPC_AGGRO_LINK> aggroLinks = new List<NPC_AGGRO_LINK>();
    
    public void updateThis(RPGNpc newNPCData)
    {
        ID = newNPCData.ID;
        icon = newNPCData.icon;
        _name = newNPCData._name;
        _fileName = newNPCData._fileName;
        AggroRange = newNPCData.AggroRange;
        distanceFromTarget = newNPCData.distanceFromTarget;
        RoamRange = newNPCData.RoamRange;
        RoamDelay = newNPCData.RoamDelay;
        DistanceToTargetReset = newNPCData.DistanceToTargetReset;
        abilities = newNPCData.abilities;
        stats = newNPCData.stats;
        isDummyTarget = newNPCData.isDummyTarget;
        lootTables = newNPCData.lootTables;
        aggroLinks = newNPCData.aggroLinks;
        MinRespawn = newNPCData.MinRespawn;
        MaxRespawn = newNPCData.MaxRespawn;
        npcType = newNPCData.npcType;
        MinEXP = newNPCData.MinEXP;
        MaxEXP = newNPCData.MaxEXP;
        EXPBonusPerLevel = newNPCData.EXPBonusPerLevel;
        MinLevel = newNPCData.MinLevel;
        MaxLevel = newNPCData.MaxLevel;
        isScalingWithPlayer = newNPCData.isScalingWithPlayer;
        distanceFromOwner = newNPCData.distanceFromOwner;
        merchantTableID = newNPCData.merchantTableID;
        questGiven = newNPCData.questGiven;
        questCompleted = newNPCData.questCompleted;
        displayName = newNPCData.displayName;
        isCombatEnabled = newNPCData.isCombatEnabled;
        isMovementEnabled = newNPCData.isMovementEnabled;
        isCollisionEnabled = newNPCData.isCollisionEnabled;
        factionID = newNPCData.factionID;
        factionRewards = newNPCData.factionRewards;
        dialogueID = newNPCData.dialogueID;
        speciesID = newNPCData.speciesID;
        
        NPCVisual = newNPCData.NPCVisual;
        nameplateYOffset = newNPCData.nameplateYOffset;
        
        animatorController = newNPCData.animatorController;
        animatorAvatar = newNPCData.animatorAvatar;
        animatorUseRootMotion = newNPCData.animatorUseRootMotion;
        animatorUpdateMode = newNPCData.animatorUpdateMode;
        AnimatorCullingMode = newNPCData.AnimatorCullingMode;
        navmeshAgentRadius = newNPCData.navmeshAgentRadius;
        navmeshAgentHeight = newNPCData.navmeshAgentHeight;
        navmeshAgentAngularSpeed = newNPCData.navmeshAgentAngularSpeed;
        navmeshObstacleAvoidance = newNPCData.navmeshObstacleAvoidance;
        colliderType = newNPCData.colliderType;
        colliderCenter = newNPCData.colliderCenter;
        colliderSize = newNPCData.colliderSize;
        colliderRadius = newNPCData.colliderRadius;
        colliderHeight = newNPCData.colliderHeight;
        modelPosition = newNPCData.modelPosition;
        modelScale = newNPCData.modelScale;
        corpseDespawnTime = newNPCData.corpseDespawnTime;
        isTargetable = newNPCData.isTargetable;
        isNameplateEnabled = newNPCData.isNameplateEnabled;
        isPlayerInteractable = newNPCData.isPlayerInteractable;
    }
}