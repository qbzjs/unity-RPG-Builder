using System.Collections.Generic;
using UnityEngine;

public class RPGCraftingStation : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public Sprite icon;

    public float maxDistance;

    [System.Serializable]
    public class CraftSkillsDATA
    {
        [SkillID] public int craftSkillID = -1;
        public RPGSkill craftSkillREF;
    }
    [RPGDataList] public List<CraftSkillsDATA> craftSkills = new List<CraftSkillsDATA>();

    public void updateThis(RPGCraftingStation newDATA)
    {
        _name = newDATA._name;
        _fileName = newDATA._fileName;
        icon = newDATA.icon;
        ID = newDATA.ID;
        maxDistance = newDATA.maxDistance;
        craftSkills = newDATA.craftSkills;
        displayName = newDATA.displayName;
    }
}