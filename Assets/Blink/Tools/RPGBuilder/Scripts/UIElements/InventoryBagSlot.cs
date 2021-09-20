using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryBagSlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler,
    IDropHandler
{
    public Image icon;
    public Image rarity;
    public RPGItem thisItem;

    public int bagIndex;

    private GameObject curDraggedItem;

    public void InitSlot(RPGItem item, int bag_index)
    {
        thisItem = item;
        bagIndex = bag_index;
        icon.sprite = item.icon;
        Sprite itemQualitySprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
        if (itemQualitySprite != null)
        {
            rarity.enabled = true;
            rarity.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
        }
        else
        {
            rarity.enabled = false;
        }
    }

    public void ClearDraggedSlot()
    {
        if (curDraggedItem != null)
        {
            Destroy(curDraggedItem);
        }
    }

    public void ShowTooltip()
    {
        ItemTooltip.Instance.Show(thisItem.ID, thisItem.ID, true);
    }

    public void HideTooltip()
    {
        ItemTooltip.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        ItemTooltip.Instance.Hide();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thisItem == null) return;
        if (curDraggedItem != null) Destroy(curDraggedItem);
        curDraggedItem = Instantiate(InventoryManager.Instance.draggedItemImage, transform.position,
            Quaternion.identity);
        curDraggedItem.transform.SetParent(InventoryManager.Instance.draggedItemParent);
        curDraggedItem.GetComponent<Image>().sprite = thisItem.icon;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (thisItem == null || curDraggedItem == null) return;
        curDraggedItem.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (thisItem == null || curDraggedItem == null) return;
        if (InventoryDisplayManager.Instance.thisCG.alpha == 1)
        {
            
        }

        Destroy(curDraggedItem);
    }

    public void OnDrop(PointerEventData eventData)
    {
    }

}
