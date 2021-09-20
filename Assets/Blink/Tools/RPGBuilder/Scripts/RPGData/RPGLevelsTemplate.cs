using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGLevelsTemplate : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public int levels;
    public int baseXPValue;
    public float increaseAmount;

    [Serializable]
    public class LEVELS_DATA
    {
        public string levelName;
        public int level;
        public int XPRequired;
    }

    [RPGDataList] public List<LEVELS_DATA> allLevels = new List<LEVELS_DATA>();

    public void updateThis(RPGLevelsTemplate newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        levels = newData.levels;
        baseXPValue = newData.baseXPValue;
        increaseAmount = newData.increaseAmount;
        allLevels = newData.allLevels;
    }
}