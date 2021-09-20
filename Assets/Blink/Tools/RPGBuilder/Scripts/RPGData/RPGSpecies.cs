using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGSpecies : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string displayName;
    public string fileName;
    public string description;
    public Sprite icon;
    
    [Serializable]
    public class SPECIES_STAT_DATA
    {
        [StatID] public int statID = -1;
        public float value;
        
        public List<RPGStat.VitalityActions> vitalityActions = new List<RPGStat.VitalityActions>();
    }

    [RPGDataList] public List<SPECIES_STAT_DATA> stats = new List<SPECIES_STAT_DATA>();
    
    [Serializable]
    public class SPECIES_TRAIT
    {
        public string statFunction;
        public float modifier = 100;
    }

    public List<SPECIES_TRAIT> traits = new List<SPECIES_TRAIT>();
    
    public void updateThis(RPGSpecies newData)
    {
        ID = newData.ID;
        _name = newData._name;
        fileName = newData.fileName;
        displayName = newData.displayName;
        description = newData.description;
        icon = newData.icon;

        stats = newData.stats;
        traits = newData.traits;
    }
}
