using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class SocketSlotHolder : MonoBehaviour
{
    public TextMeshProUGUI socketNameText;
    public Transform gemItemParent;
    public GameObject curGemItemGO;
    public string thisSocketType;
    public void Init(string socketType, RPGItem gemItemREF)
    {
        thisSocketType = socketType;
        if(curGemItemGO!=null) Destroy(curGemItemGO);
        socketNameText.text = socketType;

        if (gemItemREF == null) return;
        curGemItemGO = Instantiate(SocketingPanelDisplayManager.Instance.itemSlotPrefab, gemItemParent);
        var slotREF = curGemItemGO.GetComponent<EnchantCostSlotHolder>();
        var itemREF = RPGBuilderUtilities.GetItemFromID(gemItemREF.ID);
        slotREF.InitSlot(itemREF.icon, true, 0, itemREF, false, -1);
    }

    
}
