using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGCombo : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string displayName;
    public string fileName;

    public string description;

    [AbilityID] public int initialAbilityID = -1;
    public bool StartCancelOtherCombos;

    public enum KeyType {
        StartAbilityKey,
        OverrideKey,
        ActionKey
    }
    
    [System.Serializable]
    public class ComboEntry
    {
        [AbilityID] public int abilityID = -1;
        public bool abMustBeKnown;
        public bool mustHit;
        public float expireTime = 3, readyTime = 0;
        public KeyType keyType;
        public KeyCode overrideKey;
        public string actionKeyName;
        public List<RequirementsManager.RequirementDATA> requirements = new List<RequirementsManager.RequirementDATA>();
    }

    [RPGDataList] public List<ComboEntry> combos = new List<ComboEntry>();
    
    
    public void updateThis(RPGCombo newData)
    {
        ID = newData.ID;
        _name = newData._name;
        fileName = newData.fileName;
        displayName = newData.displayName;
        description = newData.description;
        initialAbilityID = newData.initialAbilityID;
        StartCancelOtherCombos = newData.StartCancelOtherCombos;
        
        combos = newData.combos;
    }
}
