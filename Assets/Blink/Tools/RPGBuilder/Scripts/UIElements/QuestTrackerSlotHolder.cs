using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class QuestTrackerSlotHolder : MonoBehaviour
    {
        public TextMeshProUGUI questNameText, globalCompletionText;
        public Transform objectiveTextParent;
        public Image backgroundImage;
        private RPGQuest thisQuest;

        public void InitSlot(RPGQuest _quest)
        {
            thisQuest = _quest;
        }

        public void ClickUntrackQuest()
        {
            if (QuestTrackerDisplayManager.Instance.isQuestAlreadyTracked(thisQuest))
                QuestTrackerDisplayManager.Instance.UnTrackQuest(thisQuest);
        }

        public void ClickQuestName()
        {
            QuestJournalDisplayManager.Instance.DisplayQuestContent(thisQuest);
        }
    }
}