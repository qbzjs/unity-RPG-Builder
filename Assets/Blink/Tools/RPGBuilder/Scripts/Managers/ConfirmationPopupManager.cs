using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class ConfirmationPopupManager : MonoBehaviour
    {
        public enum ConfirmationPopupType
        {
            deleteItem,
            sellItem
        }

        public CanvasGroup thisCG;
        public TextMeshProUGUI PopupText;
        private ConfirmationPopupType curType;

        private RPGItem itemREF;
        private int itemDeletedCount;
        private int tempBagIndex;
        private int tempBagSlotIndex;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static ConfirmationPopupManager Instance { get; private set; }

        public void InitPopup(ConfirmationPopupType popupType, RPGItem item, int count, int bagIndex, int bagSlotIndex)
        {
            curType = popupType;
            RPGBuilderUtilities.EnableCG(thisCG);

            switch (curType)
            {
                case ConfirmationPopupType.deleteItem:
                    PopupText.text = "Do you want to delete " + item.displayName + " x" + count + "?";
                    itemREF = item;
                    itemDeletedCount = count;
                    tempBagIndex = bagIndex;
                    tempBagSlotIndex = bagSlotIndex;
                    break;
                case ConfirmationPopupType.sellItem:
                    PopupText.text = "Do you want to sell " + item.displayName + " x" + count + "?";
                    itemREF = item;
                    itemDeletedCount = count;
                    tempBagIndex = bagIndex;
                    tempBagSlotIndex = bagSlotIndex;
                    break;
            }
        }

        public void ClickConfirm ()
        {
            switch (curType)
            {
                case ConfirmationPopupType.deleteItem:
                    InventoryManager.Instance.RemoveItem(itemREF.ID, itemDeletedCount, tempBagIndex, tempBagSlotIndex, true);
                    break;
                case ConfirmationPopupType.sellItem:
                    InventoryManager.Instance.SellItemToMerchant(itemREF.ID, itemDeletedCount, tempBagIndex, tempBagSlotIndex);
                    break;
            }
            ClickCancel();
        }

        public void ClickCancel ()
        {
            RPGBuilderUtilities.DisableCG(thisCG);
            itemREF = null;
            itemDeletedCount = -1;
        }

    }
}
