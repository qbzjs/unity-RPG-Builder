using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModifierSlotDataHolder : MonoBehaviour
{
    public TextMeshProUGUI nameText, costText;
    public Image icon;
    public RPGGameModifier thisModifier;

    public void ClickSlot()
    {
        MainMenuManager.Instance.ClickGameModifierSlot(thisModifier);
    }


}
