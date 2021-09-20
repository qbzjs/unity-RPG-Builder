using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class DevStatSlotHolder : MonoBehaviour
{
    public RPGStat thisStat;
    public TextMeshProUGUI nameText;

    public void SetStat()
    {
        DevUIManager.Instance.SetSelectedStat(thisStat);
    }
}
