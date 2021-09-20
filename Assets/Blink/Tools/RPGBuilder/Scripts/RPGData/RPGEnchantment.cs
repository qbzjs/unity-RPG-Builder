using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RPGEnchantment : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    
    public enum ApplyRequirementType
    {
        ItemType, 
        ItemRarity,
        ArmorType,
        ArmorSlot,
        WeaponType,
        WeaponSlot
    }
    
    [System.Serializable]
    public class ApplyRequirements
    {
        public ApplyRequirementType type;
        
        public string itemType;
        [FormerlySerializedAs("itemQuality")] public string itemRarity;
        public string weaponType;
        public string armorType;
        public string armorSlot;
        public string weaponSlot;
    }
    [RPGDataList] public List<ApplyRequirements> applyRequirements = new List<ApplyRequirements>();
    
    [System.Serializable]
    public class CurrencyCost
    {
        public RPGCurrency costCurrencyREF;
        [CurrencyID] public int currencyID = -1;
        public int amount;
    }
    
    [System.Serializable]
    public class ItemCost
    {
        public RPGItem itemREF;
        [ItemID] public int itemID = -1;
        public int itemCount;
    }
    
    [System.Serializable]
    public class TierStat
    {
        [StatID] public int statID = -1;
        public RPGStat statREF;
        public float amount;
        public bool isPercent;
    }
    
    [System.Serializable]
    public class EnchantmentTier
    {
        [RPGDataList] public List<CurrencyCost> currencyCosts = new List<CurrencyCost>();
        [RPGDataList] public List<ItemCost> itemCosts = new List<ItemCost>();

        public float successRate = 100f;
        public float enchantTime = 0f;
        public GameObject enchantingParticle;
        
        public RPGSkill skillREF;
        [SkillID] public int skillID = -1;
        public int skillXPAmount;
        
        [RPGDataList] public List<TierStat> stats = new List<TierStat>();
    }
    [RPGDataList] public List<EnchantmentTier> enchantmentTiers = new List<EnchantmentTier>();
    
    public void updateThis(RPGEnchantment newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        displayName = newData.displayName;
        
        applyRequirements = newData.applyRequirements;
        enchantmentTiers = newData.enchantmentTiers;
    }
}
