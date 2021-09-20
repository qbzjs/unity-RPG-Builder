using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGItem : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public Sprite icon;

    public string equipmentSlot;
    public string itemType;
    public string weaponType;
    public string armorType;
    public string slotType;
    public string rarity;

    public enum ArmorPieceType
    {
        Name,
        Mesh
    }
    
    public string itemModelName;
    public GameObject weaponModel;
    public Material modelMaterial;
    public Mesh armorMesh;
    public ArmorPieceType armorPieceType;

    [Serializable]
    public class WeaponPositionData
    {
        [RaceID] public int raceID = -1;
        public RPGRace raceREF;

        [Serializable]
        public class GenderPositionData
        {
            public RPGRace.RACE_GENDER gender;
            
            public Vector3 CombatPositionInSlot = Vector3.zero;
            public Vector3 CombatRotationInSlot = Vector3.zero;
            public Vector3 CombatScaleInSlot = Vector3.one;
            public Vector3 RestPositionInSlot = Vector3.zero;
            public Vector3 RestRotationInSlot = Vector3.zero;
            public Vector3 RestScaleInSlot = Vector3.one;
            
            public Vector3 CombatPositionInSlot2 = Vector3.zero;
            public Vector3 CombatRotationInSlot2 = Vector3.zero;
            public Vector3 CombatScaleInSlot2 = Vector3.one;
            public Vector3 RestPositionInSlot2 = Vector3.zero;
            public Vector3 RestRotationInSlot2 = Vector3.zero;
            public Vector3 RestScaleInSlot2 = Vector3.one;
        }
        [RPGDataList] public List<GenderPositionData> genderPositionDatas = new List<GenderPositionData>();
    }
    [RPGDataList] public List<WeaponPositionData> weaponPositionDatas = new List<WeaponPositionData>();
    public bool showWeaponPositionData;
    
    public float AttackSpeed;
    public int minDamage;
    public int maxDamage;

    [AbilityID] public int autoAttackAbilityID = -1;
    public RPGAbility autoAttackAbilityREF;

    [Serializable]
    public class ITEM_STATS
    {
        [StatID] public int statID = -1;
        public RPGStat statREF;
        public float amount;
        public bool isPercent;
    }

    [RPGDataList] public List<ITEM_STATS> stats = new List<ITEM_STATS>();

    [RPGDataList] public List<RPGItemDATA.RandomizedStatData> randomStats = new List<RPGItemDATA.RandomizedStatData>();
    public int randomStatsMax;
    
    public int sellPrice;
    [CurrencyID] public int sellCurrencyID = -1;
    public RPGCurrency sellCurrencyREF;
    public int buyPrice;
    [CurrencyID] public int buyCurrencyID = -1;
    public RPGCurrency buyCurrencyREF;
    public int stackLimit = 1;
    public string description;

    public bool dropInWorld;
    public GameObject itemWorldModel;
    public float durationInWorld = 60f;
    public int worldInteractableLayer;

    public RPGEnchantment enchantmentREF;
    [EnchantmentID] public int enchantmentID = -1;
    public bool isEnchantmentConsumed;
    
    [Serializable]
    public class SOCKETS_DATA
    {
        public string socketType;
    }
    [RPGDataList] public List<SOCKETS_DATA> sockets = new List<SOCKETS_DATA>();
    
    [Serializable]
    public class GEM_DATA
    {
        public string socketType;
        
        [Serializable]
        public class GEM_STATS
        {
            [StatID] public int statID = -1;
            public RPGStat statREF;
            public float amount;
            public bool isPercent;
        }
        [RPGDataList] public List<GEM_STATS> gemStats = new List<GEM_STATS>();
    }
    public GEM_DATA gemData = new GEM_DATA();
    
    [RPGDataList] public List<RequirementsManager.RequirementDATA> useRequirements = new List<RequirementsManager.RequirementDATA>();

    public enum OnUseActionType
    {
        equip,
        useAbility,
        useEffect,
        learnAbility,
        learnRecipe,
        learnResourceNode,
        learnBonus,
        gainTreePoint,
        gainClassXP,
        gainSkillXP,
        gainClassLevel,
        gainSkillLevel,
        acceptQuest,
        currency,
        factionPoint,
        gainWeaponTemplateEXP
    }

    [Serializable]
    public class OnUseActionDATA
    {
        public OnUseActionType actionType;

        public int treePointGained;
        [PointID] public int treePointID = -1;
        public RPGTreePoint treePointREF;

        public int classLevelGained;
        public int classXPGained;

        public int skillLevelGained;
        public int skillXPGained;
        [SkillID] public int skillID = -1;
        public RPGSkill skillREF;

        [AbilityID] public int abilityID = -1;
        public RPGAbility abilityREF;

        [RecipeID] public int recipeID = -1;
        public RPGCraftingRecipe recipeREF;

        [ResourceNodeID] public int resourceNodeID = -1;
        public RPGResourceNode resourceNodeREF;

        [EffectID] public int effectID = -1;
        public RPGEffect effectREF;

        [BonusID] public int bonusID = -1;
        public RPGBonus bonusREF;

        [QuestID] public int questID = -1;
        public RPGQuest questREF;

        [CurrencyID] public int currencyID = -1;
        public RPGCurrency currencyREF;

        public int factionPointsGained;
        [FactionID] public int factionID = -1;
        public RPGFaction factionREF;
        
        public int weaponTemplateXPGained;
        [WeaponTemplateID] public int weaponTemplateID = -1;
        public RPGWeaponTemplate weaponTemplateREF;

        public RPGCombatDATA.TARGET_TYPE target;
        public bool isConsumed;
    }

    [RPGDataList] public List<OnUseActionDATA> onUseActions = new List<OnUseActionDATA>();
    [RPGDataList] public List<RPGCombatDATA.ActionAbilityDATA> actionAbilities = new List<RPGCombatDATA.ActionAbilityDATA>();

    public void updateThis(RPGItem newItemDATA)
    {
        ID = newItemDATA.ID;
        _name = newItemDATA._name;
        _fileName = newItemDATA._fileName;
        icon = newItemDATA.icon;
        itemType = newItemDATA.itemType;
        weaponType = newItemDATA.weaponType;
        armorType = newItemDATA.armorType;
        slotType = newItemDATA.slotType;
        itemModelName = newItemDATA.itemModelName;
        equipmentSlot = newItemDATA.equipmentSlot;
        weaponModel = newItemDATA.weaponModel;
        modelMaterial = newItemDATA.modelMaterial;
        weaponPositionDatas = newItemDATA.weaponPositionDatas;
        AttackSpeed = newItemDATA.AttackSpeed;
        minDamage = newItemDATA.minDamage;
        maxDamage = newItemDATA.maxDamage;
        stats = newItemDATA.stats;
        sellPrice = newItemDATA.sellPrice;
        buyPrice = newItemDATA.buyPrice;
        stackLimit = newItemDATA.stackLimit;
        description = newItemDATA.description;
        sellCurrencyID = newItemDATA.sellCurrencyID;
        buyCurrencyID = newItemDATA.buyCurrencyID;
        useRequirements = newItemDATA.useRequirements;
        onUseActions = newItemDATA.onUseActions;
        autoAttackAbilityID = newItemDATA.autoAttackAbilityID;
        displayName = newItemDATA.displayName;
        rarity = newItemDATA.rarity;
        dropInWorld = newItemDATA.dropInWorld;
        itemWorldModel = newItemDATA.itemWorldModel;
        durationInWorld = newItemDATA.durationInWorld;
        randomStats = newItemDATA.randomStats;
        enchantmentID = newItemDATA.enchantmentID;
        isEnchantmentConsumed = newItemDATA.isEnchantmentConsumed;
        sockets = newItemDATA.sockets;
        gemData = newItemDATA.gemData;
        actionAbilities = newItemDATA.actionAbilities;
        randomStatsMax = newItemDATA.randomStatsMax;
        armorPieceType = newItemDATA.armorPieceType;
        armorMesh = newItemDATA.armorMesh;
        worldInteractableLayer = newItemDATA.worldInteractableLayer;
    }
}