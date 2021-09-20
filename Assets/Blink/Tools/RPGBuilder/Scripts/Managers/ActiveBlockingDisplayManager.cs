using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBlockingDisplayManager : MonoBehaviour
{
    public static ActiveBlockingDisplayManager Instance { get; private set; }
    
    public CanvasGroup thisCG;
    
    public Image icon, durationBar;
    public TextMeshProUGUI durationText, powerFlat, powerModifier, damageBlocked;
    public Color barChargingColor, barActiveColor;

    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }
    
    public void Init()
    {
        powerFlat.text = "";
        powerModifier.text = "";
        RPGBuilderUtilities.EnableCG(thisCG);
    }

    public void UpdateDamageBlockedLeft()
    {
        Instance.damageBlocked.enabled = true;
        Instance.damageBlocked.text = CombatManager.playerCombatNode.curBlockedDamageLeft.ToString("F0");
    }

    public void Reset()
    {
        RPGBuilderUtilities.DisableCG(thisCG);
    }
}
