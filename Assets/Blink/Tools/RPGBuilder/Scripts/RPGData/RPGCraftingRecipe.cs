using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGCraftingRecipe : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public Sprite icon;
    public bool learnedByDefault;
    [SkillID] public int craftingSkillID = -1;
    public RPGSkill craftingSkillREF;
    [CraftingStationID] public int craftingStationID = -1;
    public RPGCraftingStation craftingStationREF;

    [Serializable]
    public class CraftedItemsDATA
    {
        public float chance = 100f;
        public int count = 1;
        [ItemID] public int craftedItemID = -1;
        public RPGItem craftedItemREF;
    }
    
    
    [Serializable]
    public class ComponentsRequired
    {
        public int count = 1;
        [ItemID] public int componentItemID = -1;
        public RPGItem componentItemREF;
    }
    
    
    [Serializable]
    public class RPGCraftingRecipeRankData
    {
        public bool ShowedInEditor;
        public int unlockCost;
        public int Experience;
        public float craftTime;

        [RPGDataList] public List<CraftedItemsDATA> allCraftedItems = new List<CraftedItemsDATA>();
        [RPGDataList] public List<ComponentsRequired> allComponents = new List<ComponentsRequired>();
    }
    [RPGDataList] public List<RPGCraftingRecipeRankData> ranks = new List<RPGCraftingRecipeRankData>();

    public void copyData(RPGCraftingRecipeRankData original, RPGCraftingRecipeRankData copied)
    {
        original.Experience = copied.Experience;
        original.craftTime = copied.craftTime;
        original.unlockCost = copied.unlockCost;

        original.allCraftedItems = new List<CraftedItemsDATA>();
        for (var index = 0; index < copied.allCraftedItems.Count; index++)
        {
            CraftedItemsDATA newRef = new CraftedItemsDATA();
            newRef.chance = copied.allCraftedItems[index].chance;
            newRef.count = copied.allCraftedItems[index].count;
            newRef.craftedItemID = copied.allCraftedItems[index].craftedItemID;
            original.allCraftedItems.Add(newRef);
        }

        original.allComponents = new List<ComponentsRequired>();
        for (var index = 0; index < copied.allComponents.Count; index++)
        {
            ComponentsRequired newRef = new ComponentsRequired();
            newRef.count = copied.allComponents[index].count;
            newRef.componentItemID = copied.allComponents[index].count;
            original.allComponents.Add(newRef);
        }
    }

    public void updateThis(RPGCraftingRecipe newDATA)
    {
        _name = newDATA._name;
        _fileName = newDATA._fileName;
        icon = newDATA.icon;
        ID = newDATA.ID;
        ranks = newDATA.ranks;
        learnedByDefault = newDATA.learnedByDefault;
        craftingSkillID = newDATA.craftingSkillID;
        craftingStationID = newDATA.craftingStationID;
        displayName = newDATA.displayName;
    }
}