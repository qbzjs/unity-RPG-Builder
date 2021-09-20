using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RPGItemDATA : ScriptableObject
{
    [FormerlySerializedAs("itemQuality")] public string[] itemRarity;
    [FormerlySerializedAs("itemQualityList")] public List<string> itemRarityList = new List<string>();
    [FormerlySerializedAs("itemQualityImages")] public Sprite[] itemRarityImages;
    [FormerlySerializedAs("itemQualityImagesList")] public List<Sprite> itemRarityImagesList = new List<Sprite>();
    [FormerlySerializedAs("itemQualityColors")] public Color[] itemRarityColors;
    [FormerlySerializedAs("itemQualityColorsList")] public List<Color> itemRarityColorsList = new List<Color>();
    public string[] itemType;
    public List<string> itemTypeList = new List<string>();
    public string[] weaponType;
    public List<string> weaponTypeList = new List<string>();
    public string[] armorType;
    public List<string> armorTypeList = new List<string>();
    public string[] armorSlots;
    public List<string> armorSlotsList = new List<string>();
    public string[] weaponSlots;
    public List<string> weaponSlotsList = new List<string>();
    public string[] slotType;
    public List<string> slotTypeList = new List<string>();
    public string[] socketType;
    public List<string> socketTypeList = new List<string>();

    [System.Serializable]
    public class WeaponAnimatorOverride
    {
        public string weaponType1, weaponType2;
        public bool requireWeapon2;
        public RuntimeAnimatorController restAnimatorOverride;
        public RuntimeAnimatorController combatAnimatorOverride;
    }

    public List<WeaponAnimatorOverride> weaponAnimatorOverrides;
    
    public int InventorySlots;


    [System.Serializable]
    public class StartingItemsDATA
    {
        public int itemID = -1;
        public RPGItem itemREF;
        public int count = 1;
        public bool equipped;
    }

    
    [System.Serializable]
    public class RandomItemData
    {
        public List<RandomizedStat> randomStats = new List<RandomizedStat>();
        public int randomItemID = -1;
    }
    
    [System.Serializable]
    public class RandomizedStat
    {
        public int statID = -1;
        public float statValue;
    }
    
    [System.Serializable]
    public class RandomizedStatData
    {
        public int statID = -1;
        public RPGStat statREF;
        public float minValue, maxValue = 1f;
        public bool isPercent;
        public bool isInt;
        public float chance = 100f;
    }
    
    public void updateThis(RPGItemDATA newData)
    {
        itemTypeList = newData.itemTypeList;
        weaponTypeList = newData.weaponTypeList;
        armorTypeList = newData.armorTypeList;
        itemRarityList = newData.itemRarityList;
        armorSlotsList = newData.armorSlotsList;
        weaponSlotsList = newData.weaponSlotsList;
        slotTypeList = newData.slotTypeList;
        InventorySlots = newData.InventorySlots;
        itemRarityImagesList = newData.itemRarityImagesList;
        itemRarityColorsList = newData.itemRarityColorsList;
        socketTypeList = newData.socketTypeList;
        weaponAnimatorOverrides = newData.weaponAnimatorOverrides;
    }
}