using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGGearSet : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    
    [Serializable]
    public class itemInSet
    {
        public RPGItem itemREF;
        [ItemID] public int itemID = -1;
    }
    [RPGDataList] public List<itemInSet> itemsInSet = new List<itemInSet>();
    
    [Serializable]
    public class GearSetTier
    {
        public int equippedAmount;
        
        [Serializable]
        public class GearSetTierStat
        {
            [StatID] public int statID = -1;
            public RPGStat statREF;
            public float amount;
            public bool isPercent;
        }
        [RPGDataList] public List<GearSetTierStat> gearSetTierStats = new List<GearSetTierStat>();
        
    }
    [RPGDataList] public List<GearSetTier> gearSetTiers = new List<GearSetTier>();

    public void updateThis(RPGGearSet newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        displayName = newData.displayName;
        
        itemsInSet = newData.itemsInSet;
        gearSetTiers = newData.gearSetTiers;
    }
}
