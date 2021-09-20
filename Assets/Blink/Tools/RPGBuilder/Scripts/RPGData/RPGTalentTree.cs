using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGTalentTree : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string displayName;
    public string _fileName;
    public Sprite icon;

    public int TiersAmount;

    [PointID] public int treePointAcceptedID = -1;
    public RPGTreePoint treePointAcceptedREF;

    public enum TalentTreeNodeType
    {
        ability,
        recipe,
        resourceNode,
        bonus
    }

    [Serializable]
    public class Node_DATA
    {
        public TalentTreeNodeType nodeType;
        public string _name;
        [AbilityID] public int abilityID = -1;
        public RPGAbility abilityREF;
        [RecipeID] public int recipeID = -1;
        public RPGCraftingRecipe recipeREF;
        public int resourceNodeID = -1;
        [ResourceNodeID] public RPGResourceNode resourceNodeREF;
        public int bonusID = -1;
        [BonusID] public RPGBonus bonusREF;
        public int Tier;
        public int Row;

        [RPGDataList] public List<RequirementsManager.RequirementDATA> requirements = new List<RequirementsManager.RequirementDATA>();
    }

    [RPGDataList] public List<Node_DATA> nodeList = new List<Node_DATA>();


    public void updateThis(RPGTalentTree newData)
    {
        ID = newData.ID;
        _name = newData._name;
        displayName = newData.displayName;
        _fileName = newData._fileName;

        nodeList = newData.nodeList;
        icon = newData.icon;
        treePointAcceptedID = newData.treePointAcceptedID;
        TiersAmount = newData.TiersAmount;
    }
}