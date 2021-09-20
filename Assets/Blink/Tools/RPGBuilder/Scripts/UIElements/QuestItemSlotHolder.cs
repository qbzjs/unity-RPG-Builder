using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class QuestItemSlotHolder : MonoBehaviour
    {
        public enum QuestRewardType
        {
            itemGiven,
            rewardGiven,
            rewardToPick
        }

        public QuestRewardType thisType;

        public Image icon, background;
        public TextMeshProUGUI stackText;

        private RPGItem thisItem;
        private RPGCurrency thisCurrency;
        private RPGTreePoint thisTreePoint;
        private RPGQuest.QuestRewardDATA thisRewardDATA;

        public Image selectedBorder;

        public void InitItemGivenSlot(RPGItem item, int count)
        {
            selectedBorder.enabled = false;
            thisItem = item;
            icon.sprite = item.icon;
            background.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlot(RPGItem item, int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            thisItem = item;
            icon.sprite = item.icon;
            background.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlot(RPGCurrency currency, int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            thisCurrency = currency;
            icon.sprite = currency.icon;
            background.enabled = false;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlot(RPGTreePoint treePoint, int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            thisTreePoint = treePoint;
            icon.sprite = treePoint.icon;
            background.enabled = false;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlotEXP(int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            icon.sprite = QuestInteractionDisplayManager.Instance.experienceICON;
            background.enabled = false;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlotFACTION(int amount, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            icon.sprite = RPGBuilderUtilities.GetFactionFromID(rewardDATA.factionID).icon;
            background.enabled = false;
            var curstack = amount;
            stackText.text = curstack.ToString();
        }
        
        public void InitSlotWeaponXP(int amount, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            icon.sprite = RPGBuilderUtilities.GetWeaponTemplateFromID(rewardDATA.weaponTemplateID).icon;
            background.enabled = false;
            var curstack = amount;
            stackText.text = curstack.ToString();
        }

        public void SelectRewardToPick()
        {
            if (thisType == QuestRewardType.rewardToPick)
                QuestInteractionDisplayManager.Instance.SelectAReward(this, thisRewardDATA);
        }

        public void ShowTooltip()
        {
            if (thisItem != null) ItemTooltip.Instance.Show(thisItem.ID, -1, true);
            if (thisCurrency != null) ItemTooltip.Instance.ShowCurrencyTooltip(thisCurrency.ID);
            if (thisTreePoint != null) ItemTooltip.Instance.ShowTreePointTooltip(thisTreePoint.ID);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }
    }
}