using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGLootTable : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;

    [Serializable]
    public class LOOT_ITEMS
    {
        [ItemID] public int itemID = -1;
        public RPGItem itemREF;
        public int min = 1;
        public int max = 1;
        public float dropRate = 100f;
    }

    [RPGDataList] public List<LOOT_ITEMS> lootItems = new List<LOOT_ITEMS>();

    public void updateThis(RPGLootTable newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        lootItems = newData.lootItems;
    }
}