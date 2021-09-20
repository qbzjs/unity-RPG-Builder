using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGSpellbook : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;
    public Sprite icon;
    
    public enum SpellbookNodeType
    {
        ability,
        bonus
    }

    [Serializable]
    public class Node_DATA
    {
        public SpellbookNodeType nodeType;
        public string _name;
        [AbilityID] public int abilityID = -1;
        public RPGAbility abilityREF;
        [BonusID] public int bonusID = -1;
        public RPGBonus bonusREF;

        public int unlockLevel = 1;
    }
    [RPGDataList] public List<Node_DATA> nodeList = new List<Node_DATA>();
    
    public class SpellBookData
    {
        public RPGSpellbook spellbook;
        [WeaponTemplateID] public int weaponTemplateID = -1;
    }
    
    public enum spellbookSourceType
    {
        _class,
        _weapon
    }
    public spellbookSourceType sourceType;
    
    public void updateThis(RPGSpellbook newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        icon = newData.icon;
        description = newData.description;
        displayName = newData.displayName;
        nodeList = newData.nodeList;
        sourceType = newData.sourceType;
    }
}
