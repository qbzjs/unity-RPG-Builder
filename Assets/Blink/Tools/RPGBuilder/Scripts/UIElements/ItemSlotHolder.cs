using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace BLINK.RPGBuilder.UIElements
{
    public class ItemSlotHolder : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler,
        IDropHandler
    {
        public Image icon;
        [FormerlySerializedAs("quality")] public Image rarity;
        public TextMeshProUGUI stackText;

        public RPGItem thisItem;
        public int bagIndex;
        public int slotIndex;

        private GameObject curDraggedItem;

        public void InitSlot(RPGItem item, int bag_index, int slot_index)
        {
            thisItem = item;
            bagIndex = bag_index;
            slotIndex = slot_index;
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
            var curstack = CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemStack;
            stackText.text = curstack > 1 ? curstack.ToString() : "";
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
            ItemTooltip.Instance.Show(thisItem.ID, CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID, true);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if (MerchantPanelDisplayManager.Instance.thisCG.alpha == 1)
            {
                ConfirmationPopupManager.Instance.InitPopup(ConfirmationPopupManager.ConfirmationPopupType.sellItem,
                    thisItem, CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemStack, bagIndex, slotIndex);
                return;
            }
            if (EnchantingPanelDisplayManager.Instance.thisCG.alpha == 1)
            {
                EnchantingPanelDisplayManager.Instance.AssignEnchantedItem(thisItem, CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID);
                return;
            }
            if (SocketingPanelDisplayManager.Instance.thisCG.alpha == 1)
            {
                SocketingPanelDisplayManager.Instance.AssignSocketedItem(thisItem, CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID);
                return;
            }

            InventoryManager.Instance.UseItem(thisItem, bagIndex, slotIndex);
            ItemTooltip.Instance.Hide();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (thisItem == null) return;
            if(curDraggedItem != null) Destroy(curDraggedItem);
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
                for (var i = 0; i < CharacterData.Instance.inventoryData.baseSlots.Count; i++)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(
                        InventoryDisplayManager.Instance.allSlots[i],
                        Input.mousePosition)) continue;

                    if (bagIndex == 0 && slotIndex == i)
                    {
                        Destroy(curDraggedItem);
                        return;
                    }

                    InventoryManager.Instance.MoveItem(bagIndex, slotIndex, 0, i);

                    Destroy(curDraggedItem);
                    return;
                }
            }

            if (CharacterPanelDisplayManager.Instance.thisCG.alpha == 1)
            {
                switch (thisItem.itemType)
                {
                    case "ARMOR":
                    {
                        foreach (var t in CharacterPanelDisplayManager.Instance.armorSlotData)
                            if (RectTransformUtility.RectangleContainsScreenPoint(
                                t.itemUIRef.GetComponent<RectTransform>(),
                                Input.mousePosition))
                            {
                                InventoryManager.Instance.UseItem(thisItem, bagIndex, slotIndex);
                                Destroy(curDraggedItem);
                                return;
                            }

                        break;
                    }
                    case "WEAPON":
                    {
                        foreach (var t in CharacterPanelDisplayManager.Instance.weaponSlotData)
                            if (RectTransformUtility.RectangleContainsScreenPoint(
                                t.itemUIRef.GetComponent<RectTransform>(),
                                Input.mousePosition))
                            {
                                InventoryManager.Instance.UseItem(thisItem, bagIndex, slotIndex);
                                Destroy(curDraggedItem);
                                return;
                            }

                        break;
                    }
                }
            }

            if (EnchantingPanelDisplayManager.Instance.thisCG.alpha == 1)
            {
                switch (thisItem.itemType)
                {
                    case "ARMOR":
                    case "WEAPON":
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(
                            EnchantingPanelDisplayManager.Instance.enchantedItemParent.GetComponent<RectTransform>(),
                            Input.mousePosition))
                        {
                            EnchantingPanelDisplayManager.Instance.AssignEnchantedItem(thisItem,
                                CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID);
                            Destroy(curDraggedItem);
                            return;
                        }

                        break;
                    }
                }
            }

            if (SocketingPanelDisplayManager.Instance.thisCG.alpha == 1)
            {
                switch (thisItem.itemType)
                {
                    case "GEM":
                    {
                        foreach (var socketSlot in SocketingPanelDisplayManager.Instance.curSocketSlots)
                        {
                            if (!RectTransformUtility.RectangleContainsScreenPoint(
                                socketSlot.gemItemParent.GetComponent<RectTransform>(),
                                Input.mousePosition)) continue;
                            SocketingPanelDisplayManager.Instance.SetGemItemInSocketSlot(socketSlot, thisItem);
                            Destroy(curDraggedItem);
                            return;
                        }

                        break;
                    }
                    case "ARMOR":
                    case "WEAPON":
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(
                            SocketingPanelDisplayManager.Instance.socketedItemParent.GetComponent<RectTransform>(),
                            Input.mousePosition))
                        {
                            SocketingPanelDisplayManager.Instance.AssignSocketedItem(thisItem,
                                CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID);
                            Destroy(curDraggedItem);
                            return;
                        }

                        break;
                    }
                }
            }

            if (SocketingPanelDisplayManager.Instance.thisCG.alpha == 1 && thisItem.itemType == "GEM")
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    SocketingPanelDisplayManager.Instance.socketedItemParent.GetComponent<RectTransform>(),
                    Input.mousePosition))
                {
                    EnchantingPanelDisplayManager.Instance.AssignEnchantedItem(thisItem,
                        CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID);
                    Destroy(curDraggedItem);
                    return;
                }
            }

            for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                    Input.mousePosition))
                {
                    if (ActionBarManager.Instance.actionBarSlots[i].acceptItems)
                    {
                        ActionBarManager.Instance.SetItemToSlot(thisItem, i);
                    }
                    else
                    {
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("This action bar slot do not accept items", 3);
                    }
                    Destroy(curDraggedItem);
                    return;
                }

            
            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData pEventData = new PointerEventData(EventSystem.current);

            pEventData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pEventData, results);

            foreach (RaycastResult result in results)
            {
                switch (result.gameObject.name)
                {
                    case "Inventory":
                    case "Character":
                    case "SkillBook":
                    case "Spellbook": //book is lower case currently
                    case "EnchantingPanel":
                    case "SocketingPanel":
                    case "QuestJournal":
                    case "QuestStatesPanel":
                    case "QuestInteractionPanel":
                    case "Options":
                    case "CraftingPanel":
                    case "Minimap":
                    {
                        Destroy(curDraggedItem);
                        return;
                    }
                }
            }

            ConfirmationPopupManager.Instance.InitPopup(ConfirmationPopupManager.ConfirmationPopupType.deleteItem,
                thisItem,
                CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemStack, bagIndex, slotIndex);
            Destroy(curDraggedItem);
        }

        public void OnDrop(PointerEventData eventData)
        {
        }
    }
}