using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGResourceNode : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public Sprite icon;
    public bool learnedByDefault;
    [SkillID] public int skillRequiredID = -1;
    public RPGSkill skillRequiredREF;



    [Serializable]
    public class RPGResourceNodeRankData
    {
        public bool ShowedInEditor;
        public int unlockCost;

        [LootTableID] public int lootTableID = -1;
        public RPGLootTable lootTableREF;

        public int skillLevelRequired;

        public int Experience;

        public float distanceMax;

        public float gatherTime;
        public float respawnTime;
    }
    public List<RPGResourceNodeRankData> ranks = new List<RPGResourceNodeRankData>();

    public void copyData(RPGResourceNodeRankData original, RPGResourceNodeRankData copied)
    {
        original.unlockCost = copied.unlockCost;
        original.lootTableID = copied.lootTableID;
        original.skillLevelRequired = copied.skillLevelRequired;
        original.Experience = copied.Experience;
        original.distanceMax = copied.distanceMax;
        original.gatherTime = copied.gatherTime;
        original.respawnTime = copied.respawnTime;
    }
    
    public void updateThis(RPGResourceNode newDATA)
    {
        ID = newDATA.ID;
        _name = newDATA._name;
        _fileName = newDATA._fileName;
        icon = newDATA.icon;
        learnedByDefault = newDATA.learnedByDefault;
        ranks = newDATA.ranks;
        skillRequiredID = newDATA.skillRequiredID;
        displayName = newDATA.displayName;
    }
}