using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnchantCostSlotHolder : MonoBehaviour
{
    public Image itemIcon, bg, background;
    public TextMeshProUGUI countText;

    public Color ownedColor, notOwnedColor;
    private RPGItem thisItem;
    private RPGCurrency thisCurrency;
    private int itemDataID = -1;
    public void InitSlot(Sprite icon, bool owned, int count, RPGItem item, bool showCount, int itmDataID)
    {
        itemDataID = itmDataID;
        itemIcon.sprite = icon;
        countText.text = showCount ? count.ToString() : "";
        background.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
        bg.color = owned ? ownedColor : notOwnedColor;
        thisItem = item;
    }
    public void InitSlot(Sprite icon, bool owned, int count, RPGCurrency currency, int itmDataID)
    {
        itemDataID = itmDataID;
        itemIcon.sprite = icon;
        countText.text = count.ToString();
        background.enabled = false;
        bg.color = owned ? ownedColor : notOwnedColor;
        thisCurrency = currency;
    }

    public void ShowTooltip()
    {
        if(thisItem!=null)ItemTooltip.Instance.Show(thisItem.ID, itemDataID, true);
    }

    public void HideTooltip()
    {
        if(thisItem!=null)ItemTooltip.Instance.Hide();
    }
}
