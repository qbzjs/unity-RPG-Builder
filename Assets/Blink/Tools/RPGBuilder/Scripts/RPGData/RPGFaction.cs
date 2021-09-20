using System.Collections.Generic;
using UnityEngine;

public class RPGFaction : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;
    public Sprite icon;

    [System.Serializable]
    public class Faction_Stance_DATA
    {
        public string stance;
        public int pointsRequired;

        public RPGCombatDATA.ALIGNMENT_TYPE playerAlignment;
    }

    [RPGDataList] public List<Faction_Stance_DATA> factionStances = new List<Faction_Stance_DATA>();
    
    [System.Serializable]
    public class Faction_Interaction_DATA
    {
        public RPGFaction factionREF;
        [FactionID] public int factionID = -1;

        public string defaultStance;
        public int startingPoints;
    }

    [RPGDataList] public List<Faction_Interaction_DATA> factionInteractions = new List<Faction_Interaction_DATA>();
    
    public void updateThis(RPGFaction newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        icon = newData.icon;
        description = newData.description;
        displayName = newData.displayName;
        
        factionStances = newData.factionStances;
        factionInteractions = newData.factionInteractions;
    }
}
