using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class CraftingItemSlotHolder : MonoBehaviour
    {
        public Image itemIcon, bg, background;
        public TextMeshProUGUI countText;

        public Color ownedColor, notOwnedColor;
        private RPGItem thisItem;

        public void InitSlot(Sprite icon, bool owned, int count, RPGItem item)
        {
            itemIcon.sprite = icon;
            background.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
            countText.text = count.ToString();

            bg.color = owned ? ownedColor : notOwnedColor;
            thisItem = item;
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