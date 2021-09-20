using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGMerchantTable : ScriptableObject
{
    [Header("-----BASE DATA-----")] public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;

    [Serializable]
    public class ON_SALE_ITEMS_DATA
    {
        [ItemID] public int itemID = -1;
        public RPGItem itemREF;
        [CurrencyID] public int currencyID = -1;
        public RPGCurrency currencyREF;
        public int cost;
    }

    [RPGDataList] public List<ON_SALE_ITEMS_DATA> onSaleItems = new List<ON_SALE_ITEMS_DATA>();

    public void updateThis(RPGMerchantTable newItemDATA)
    {
        ID = newItemDATA.ID;
        _name = newItemDATA._name;
        _fileName = newItemDATA._fileName;
        onSaleItems = newItemDATA.onSaleItems;
        displayName = newItemDATA.displayName;
    }
}