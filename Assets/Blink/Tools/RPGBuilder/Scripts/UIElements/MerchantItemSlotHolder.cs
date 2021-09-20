using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class MerchantItemSlotHolder : MonoBehaviour
    {
        public Image itemIcon, background;
        public TextMeshProUGUI ItemNameText, ItemPriceText;

        private RPGItem thisItem;
        private RPGCurrency thisCurrency;
        public int thisCost;

        public void Init(RPGItem item, RPGCurrency currency, int cost)
        {
            thisItem = item;
            thisCurrency = currency;
            thisCost = cost;
            itemIcon.sprite = thisItem.icon;
            background.sprite = RPGBuilderUtilities.getItemRaritySprite(thisItem.rarity);
            ItemNameText.text = thisItem.displayName;
            var costText = "";
            var currencyREF = RPGBuilderUtilities.GetCurrencyFromID(currency.convertToCurrencyID);
            if (currencyREF != null && thisCurrency.AmountToConvert > 0)
            {
                if (thisCost >= currency.AmountToConvert)
                {
                    var convertedCurrencyCount = thisCost / thisCurrency.AmountToConvert;
                    var remaining = thisCost % thisCurrency.AmountToConvert;
                    costText = convertedCurrencyCount + " " + currencyREF.displayName + " " + remaining + " " +
                               thisCurrency.displayName;
                }
                else
                {
                    costText = thisCost + " " + thisCurrency.displayName;
                }
            }
            else
            {
                costText = thisCost + " " + thisCurrency.displayName;
            }

            ItemPriceText.text = costText;
        }


        public void BuyThisItem()
        {
            InventoryManager.Instance.BuyItemFromMerchant(thisItem, thisCurrency, thisCost);
        }

        public void ShowTooltip()
        {
            ItemTooltip.Instance.Show(thisItem.ID, -1, false);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }
    }
}