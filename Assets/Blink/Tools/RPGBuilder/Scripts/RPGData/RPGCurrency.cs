using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGCurrency : ScriptableObject
{
    public int ID = -1;
    public Sprite icon;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;
    public int minValue;
    public int maxValue;
    public int baseValue;

    public int AmountToConvert;

    [CurrencyID] public int convertToCurrencyID = -1;
    public RPGCurrency convertToCurrencyREF;

    [CurrencyID] public int lowestCurrencyID = -1;
    public RPGCurrency lowestCurrencyREF;

    [Serializable]
    public class AboveCurrencyDATA
    {
        [CurrencyID] public int currencyID = -1;
        public RPGCurrency currencyREF;
    }

    [RPGDataList] public List<AboveCurrencyDATA> aboveCurrencies = new List<AboveCurrencyDATA>();

    public void updateThis(RPGCurrency newStatDATA)
    {
        ID = newStatDATA.ID;
        icon = newStatDATA.icon;
        _name = newStatDATA._name;
        _fileName = newStatDATA._fileName;
        minValue = newStatDATA.minValue;
        maxValue = newStatDATA.maxValue;
        baseValue = newStatDATA.baseValue;
        displayName = newStatDATA.displayName;
        description = newStatDATA.description;
        AmountToConvert = newStatDATA.AmountToConvert;
        convertToCurrencyID = newStatDATA.convertToCurrencyID;
        lowestCurrencyID = newStatDATA.lowestCurrencyID;
        aboveCurrencies = newStatDATA.aboveCurrencies;
    }
}