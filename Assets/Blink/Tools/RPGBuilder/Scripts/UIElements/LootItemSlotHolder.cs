using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class LootItemSlotHolder : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler,
        IDropHandler
    {
        public Image itemIcon, background;
        public TextMeshProUGUI itemStackText, itemNameText;

        private int thisLootIndex;
        private LootBagHolder holder;

        private GameObject curDraggedItem;

        public void Init(int lootIndex, LootBagHolder bagHolder)
        {
            thisLootIndex = lootIndex;
            holder = bagHolder;
            itemIcon.sprite = holder.lootData[thisLootIndex].item.icon;
            background.sprite = RPGBuilderUtilities.getItemRaritySprite(holder.lootData[thisLootIndex].item.rarity);
            itemStackText.text = holder.lootData[thisLootIndex].count.ToString();
            itemNameText.text = holder.lootData[thisLootIndex].item.displayName;
        }


        public void ShowTooltip()
        {
            ItemTooltip.Instance.Show(holder.lootData[thisLootIndex].item.ID, holder.lootData[thisLootIndex].itemDataID, true);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(holder.lootData[thisLootIndex].item.ID, holder.lootData[thisLootIndex].count, false, false);
            if (itemsLeftOver == 0)
            {
                RPGBuilderUtilities.SetNewItemDataState(holder.lootData[thisLootIndex].itemDataID, CharacterData.ItemDataState.inBag);
                holder.lootData[thisLootIndex].looted = true;
                LootPanelDisplayManager.Instance.RemoveItemSlot(gameObject);
                holder.CheckLootState();
                Destroy(gameObject);
            }
            else
            {
                holder.lootData[thisLootIndex].count = itemsLeftOver;
                holder.CheckLootState();
            }
            ItemTooltip.Instance.Hide();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (curDraggedItem != null) Destroy(curDraggedItem);
            if (RPGBuilderUtilities.isInventoryFull())
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                return;
            }
            curDraggedItem = Instantiate(InventoryManager.Instance.draggedItemImage, transform.position,
                Quaternion.identity);
            curDraggedItem.transform.SetParent(InventoryManager.Instance.draggedItemParent);
            curDraggedItem.GetComponent<Image>().sprite = holder.lootData[thisLootIndex].item.icon;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (curDraggedItem == null) return;
            curDraggedItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (curDraggedItem == null) return;
            if (InventoryDisplayManager.Instance.thisCG.alpha == 1)
            {
                foreach (var t in InventoryDisplayManager.Instance.allSlots)
                    if (RectTransformUtility.RectangleContainsScreenPoint(t,
                        Input.mousePosition))
                    {
                        int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(
                            holder.lootData[thisLootIndex].item.ID, holder.lootData[thisLootIndex].count, false, false);
                        if (itemsLeftOver == 0)
                        {
                            Destroy(curDraggedItem);
                            holder.lootData[thisLootIndex].looted = true;
                            LootPanelDisplayManager.Instance.RemoveItemSlot(gameObject);
                            holder.CheckLootState();
                            Destroy(gameObject);
                        }
                        else
                        {
                            holder.lootData[thisLootIndex].count = itemsLeftOver;
                            holder.CheckLootState();
                        }

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