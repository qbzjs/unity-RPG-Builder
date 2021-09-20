using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class QuestStateSlotHolder : MonoBehaviour
    {
        public enum QuestSlotPanelType
        {
            interactionPanel,
            questJournal
        }

        private QuestSlotPanelType panelType;

        public Image icon, background;
        public TextMeshProUGUI questNameText;
        private RPGQuest thisQuest;

        public void InitSlot(RPGQuest quest, Color bgColor, Sprite stateIcon, QuestSlotPanelType _type)
        {
            panelType = _type;
            icon.sprite = stateIcon;
            questNameText.text = quest.displayName;
            background.color = bgColor;
            thisQuest = quest;
        }

        public void ClickQuest()
        {
            if (panelType == QuestSlotPanelType.interactionPanel)
                QuestInteractionDisplayManager.Instance.InitializeQuestContent(thisQuest, true);
            else
                QuestJournalDisplayManager.Instance.InitializeQuestContent(thisQuest);
        }
    }
}