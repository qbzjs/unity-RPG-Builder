using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;
using UnityEngine.UI;

public class ShapeshiftSlot : MonoBehaviour
{
    public Image border, icon;
    private RPGAbility shapeshiftingAbility;
    public RPGAbility ThisAbility
    {
        get => shapeshiftingAbility;
        set => shapeshiftingAbility = value;
    }

    public void ShowTooltip()
    {
        if(shapeshiftingAbility!=null)AbilityTooltip.Instance.Show(shapeshiftingAbility);
    }

    public void HideTooltip()
    {
        AbilityTooltip.Instance.Hide();
    }

    public void ClickUseSlot()
    {
        if(shapeshiftingAbility != null) CombatManager.Instance.InitAbility(CombatManager.playerCombatNode, shapeshiftingAbility, true);
    }
}
