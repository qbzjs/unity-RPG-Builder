using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.DisplayHandler
{
    public class EquipmentItemSlotDisplayHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
    {

        public CanvasGroup itemCG;
        public Image icon, background;
        public RPGItem curItem;

        public int weaponID;

        private GameObject curDraggedItem;
        private int itemDataID = -1;
        
        public void InitItem(RPGItem item, int dataID)
        {
            curItem = item;
            itemDataID = dataID;
            RPGBuilderUtilities.EnableCG(itemCG);
            icon.sprite = item.icon;
            background.enabled = true;
            background.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
        }

        public void ResetItem()
        {
            RPGBuilderUtilities.DisableCG(itemCG);
            icon.sprite = null;
            background.enabled = false;
            background.sprite = null;
            curItem = null;
            itemDataID = -1;
        }

        public void ShowTooltip()
        {
            if (curItem != null)
                ItemTooltip.Instance.Show(curItem.ID, itemDataID, false);
            else
                HideTooltip();
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if (curItem == null || RPGBuilderUtilities.isInventoryFull()) return;
            InventoryManager.Instance.UnequipItem(curItem, weaponID);
            curItem = null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (curItem == null) return;
            if (curDraggedItem != null) Destroy(curDraggedItem);
            curDraggedItem = Instantiate(InventoryManager.Instance.draggedItemImage, transform.position,
                Quaternion.identity);
            curDraggedItem.transform.SetParent(InventoryManager.Instance.draggedItemParent);
            curDraggedItem.GetComponent<Image>().sprite = curItem.icon;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (curItem == null || curDraggedItem == null) return;
            curDraggedItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (InventoryDisplayManager.Instance.thisCG.alpha == 1)
            {
                for (var index = 0; index < InventoryDisplayManager.Instance.allSlots.Count; index++)
                {
                    var t = InventoryDisplayManager.Instance.allSlots[index];
                    if (!RectTransformUtility.RectangleContainsScreenPoint(
                        t, Input.mousePosition)) continue;
                    
                    if (RPGBuilderUtilities.isInventoryFull())
                    {
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                        Destroy(curDraggedItem);
                        return;
                    }
                    if (CharacterData.Instance.inventoryData.baseSlots[index].itemID == -1)
                    {
                        InventoryManager.Instance.UnequipItem(curItem, weaponID);
                        Destroy(curDraggedItem);
                        return;
                    }

                    Destroy(curDraggedItem);
                    return;
                }
            }

            Destroy(curDraggedItem);
        }

        public void OnDrop(PointerEventData eventData)
        {

        }
    }
}
