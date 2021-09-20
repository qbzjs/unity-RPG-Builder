using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class EnchantSlotHolder : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public RPGEnchantment thisEnchant;
    private int thisEnchantmentINDEX = -1;
    
    public void InitSlot(int thisIndex)
    {
        nameText.text = EnchantingPanelDisplayManager.Instance.enchantList[thisIndex].enchantment.displayName;
        thisEnchant = EnchantingPanelDisplayManager.Instance.enchantList[thisIndex].enchantment;
        thisEnchantmentINDEX = thisIndex;
    }

    public void SelectEnchantment()
    {
        EnchantingPanelDisplayManager.Instance.DisplayEnchant(thisEnchantmentINDEX);
    }
}
