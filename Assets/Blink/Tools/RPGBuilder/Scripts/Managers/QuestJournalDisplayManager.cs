using System.Collections.Generic;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class QuestJournalDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG, questStatesCG, questContentCG;
        private bool showing;

        public TextMeshProUGUI questNameText, descriptionText, repeatableText;

        public Transform itemsGivenParent,
            automaticRewardsParent,
            rewardsToPickParent,
            FailedQuestTextParent,
            CompletedQuestTextParent,
            OnGoingQuestTextParent,
            QuestStateSlotParent,
            QuestContentSlotParent;

        public GameObject questItemSlotPrefab, objectiveTextPrefab, questStateSlotPrefab;

        public Color QuestStateSlotAvailableColor,
            QuestStateSlotCompletedColor,
            QuestStateSlotFailedColor,
            QuestStateOnGoingColor;

        public Sprite questStateCompletedIcon, questStateAvailableIcon, questStateFailedIcon;

        public List<GameObject> curQuestStateSlots = new List<GameObject>();
        public List<GameObject> curQuestItemsGivenSlots = new List<GameObject>();
        public List<GameObject> curQuestRewardsGivenSlots = new List<GameObject>();
        public List<QuestItemSlotHolder> curQuestRewardsPickedSlots = new List<QuestItemSlotHolder>();
        public List<GameObject> curQuestObjectiveTextSlots = new List<GameObject>();

        public GameObject backButton, AcceptQuestButton, AbandonQuestButton, TurnInQuestButton;

        private RPGNpc curNPCViewed;
        private RPGQuest curQuestViewed;
        private RPGQuest.QuestRewardDATA selectedRewardData;

        public Sprite experienceICON;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static QuestJournalDisplayManager Instance { get; private set; }

        public void ClickAcceptQuest()
        {
            QuestInteractionDisplayManager.Instance.AcceptQuest(curQuestViewed);
            InitializeQuestStates();
        }

        public void ClickAbandonQuest()
        {
            QuestInteractionDisplayManager.Instance.AbandonQuest(curQuestViewed);
            InitializeQuestStates();
        }

        public void ClickCompleteQuest()
        {
            QuestInteractionDisplayManager.Instance.CompleteQuest(curQuestViewed, selectedRewardData);
            InitializeQuestStates();
        }

        public void BackToQuestStates()
        {
            InitializeQuestStates();
        }

        private void ClearAllQuestContentSlots()
        {
            foreach (var t in curQuestItemsGivenSlots)
                Destroy(t);

            curQuestItemsGivenSlots.Clear();


            foreach (var t in curQuestRewardsGivenSlots)
                Destroy(t);

            curQuestRewardsGivenSlots.Clear();


            foreach (var t in curQuestRewardsPickedSlots)
                Destroy(t.gameObject);

            curQuestRewardsPickedSlots.Clear();

            foreach (var t in curQuestObjectiveTextSlots)
                Destroy(t);

            curQuestObjectiveTextSlots.Clear();
        }

        private void ClearAllQuestStateSlots()
        {
            foreach (var t in curQuestStateSlots)
                Destroy(t);

            curQuestStateSlots.Clear();
        }

        public void DisplayQuestContent(RPGQuest quest)
        {
            Show();
            InitializeQuestContent(quest);
        }

        public void InitializeQuestContent(RPGQuest ClickedQuest)
        {
            curQuestViewed = ClickedQuest;
            var thisQuestDATA = CharacterData.Instance.getQuestDATA(curQuestViewed);
            ClearAllQuestContentSlots();
            backButton.SetActive(true);
            RPGBuilderUtilities.EnableCG(questContentCG);
            RPGBuilderUtilities.DisableCG(questStatesCG);
            AcceptQuestButton.SetActive(false);
            AbandonQuestButton.SetActive(false);
            TurnInQuestButton.SetActive(false);

            questNameText.text = ClickedQuest.displayName;
            descriptionText.text = ClickedQuest.description;

            repeatableText.text = ClickedQuest.repeatable ? "<color=green>This quest can be repeated." : "<color=red>This quest cannot be repeated.";

            if (ClickedQuest.itemsGiven.Count > 0)
            {
                itemsGivenParent.transform.parent.gameObject.SetActive(true);
                for (var i = 0; i < ClickedQuest.itemsGiven.Count; i++)
                    curQuestItemsGivenSlots.Add(SpawnItemsSlot(ClickedQuest, i));
            }
            else
            {
                itemsGivenParent.transform.parent.gameObject.SetActive(false);
            }

            if (ClickedQuest.rewardsGiven.Count > 0)
            {
                automaticRewardsParent.transform.parent.gameObject.SetActive(true);
                for (var i = 0; i < ClickedQuest.rewardsGiven.Count; i++)
                    curQuestRewardsGivenSlots.Add(SpawnGivenRewardSlot(ClickedQuest, i));
            }
            else
            {
                automaticRewardsParent.transform.parent.gameObject.SetActive(false);
            }

            if (ClickedQuest.rewardsToPick.Count > 0)
            {
                rewardsToPickParent.transform.parent.gameObject.SetActive(true);
                for (var i = 0; i < ClickedQuest.rewardsToPick.Count; i++)
                    curQuestRewardsPickedSlots.Add(SpawnPickedRewardSlot(ClickedQuest, i));
            }
            else
            {
                rewardsToPickParent.transform.parent.gameObject.SetActive(false);
            }

            for (var i = 0; i < ClickedQuest.objectives.Count; i++)
            {
                var newObjectiveSlot = Instantiate(objectiveTextPrefab, QuestContentSlotParent);
                newObjectiveSlot.transform.SetSiblingIndex(repeatableText.transform.GetSiblingIndex() + 1);
                var slotRef = newObjectiveSlot.GetComponent<QuestObjectiveTextSlot>();
                slotRef.InitSlot(
                    QuestInteractionDisplayManager.Instance.GenerateObjectiveText(
                        RPGBuilderUtilities.GetTaskFromID(ClickedQuest.objectives[i].taskID)));
                curQuestObjectiveTextSlots.Add(newObjectiveSlot);
            }

            if (thisQuestDATA != null)
            {
                var questREF = RPGBuilderUtilities.GetQuestFromID(thisQuestDATA.questID);
                switch (thisQuestDATA.state)
                {
                    case QuestManager.questState.abandonned:
                        AcceptQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.completed when questREF.canBeTurnedInWithoutNPC:
                        TurnInQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.completed when !questREF.canBeTurnedInWithoutNPC:
                    case QuestManager.questState.failed:
                    case QuestManager.questState.onGoing:
                        AbandonQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.turnedIn:
                        break;
                }
            }
            else
            {
                AcceptQuestButton.SetActive(true);
            }
        }

        private GameObject SpawnItemsSlot(RPGQuest ClickedQuest, int i)
        {
            var newRewardSlot = Instantiate(questItemSlotPrefab, itemsGivenParent);
            var slotRef = newRewardSlot.GetComponent<QuestItemSlotHolder>();
            slotRef.InitItemGivenSlot(RPGBuilderUtilities.GetItemFromID(ClickedQuest.itemsGiven[i].itemID),
                ClickedQuest.itemsGiven[i].count);
            return newRewardSlot;
        }

        private GameObject SpawnGivenRewardSlot(RPGQuest ClickedQuest, int i)
        {
            var newRewardSlot = Instantiate(questItemSlotPrefab, automaticRewardsParent);
            var slotRef = newRewardSlot.GetComponent<QuestItemSlotHolder>();
            var type = QuestItemSlotHolder.QuestRewardType.rewardGiven;
            switch (ClickedQuest.rewardsGiven[i].rewardType)
            {
                case RPGQuest.QuestRewardType.item:
                    slotRef.InitSlot(RPGBuilderUtilities.GetItemFromID(ClickedQuest.rewardsGiven[i].itemID),
                        ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.currency:
                    slotRef.InitSlot(RPGBuilderUtilities.GetCurrencyFromID(ClickedQuest.rewardsGiven[i].currencyID),
                        ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.treePoint:
                    slotRef.InitSlot(RPGBuilderUtilities.GetTreePointFromID(ClickedQuest.rewardsGiven[i].treePointID),
                        ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.Experience:
                    slotRef.InitSlotEXP(ClickedQuest.rewardsGiven[i].Experience, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.FactionPoint:
                    slotRef.InitSlotFACTION(ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    slotRef.InitSlotWeaponXP(ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
            }

            return newRewardSlot;
        }

        private QuestItemSlotHolder SpawnPickedRewardSlot(RPGQuest ClickedQuest, int i)
        {
            var newRewardSlot = Instantiate(questItemSlotPrefab, rewardsToPickParent);
            var slotRef = newRewardSlot.GetComponent<QuestItemSlotHolder>();
            var type = QuestItemSlotHolder.QuestRewardType.rewardToPick;
            switch (ClickedQuest.rewardsToPick[i].rewardType)
            {
                case RPGQuest.QuestRewardType.item:
                    slotRef.InitSlot(RPGBuilderUtilities.GetItemFromID(ClickedQuest.rewardsToPick[i].itemID),
                        ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.currency:
                    slotRef.InitSlot(RPGBuilderUtilities.GetCurrencyFromID(ClickedQuest.rewardsToPick[i].currencyID),
                        ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.treePoint:
                    slotRef.InitSlot(RPGBuilderUtilities.GetTreePointFromID(ClickedQuest.rewardsToPick[i].treePointID),
                        ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.Experience:
                    slotRef.InitSlotEXP(ClickedQuest.rewardsToPick[i].Experience, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.FactionPoint:
                    slotRef.InitSlotFACTION(ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    slotRef.InitSlotWeaponXP(ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
            }

            return slotRef;
        }

        public void SelectAReward(QuestItemSlotHolder slotREF, RPGQuest.QuestRewardDATA rewardData)
        {
            if (CharacterData.Instance.questsData[CharacterData.Instance.getQuestINDEX(curQuestViewed)].state !=
                QuestManager.questState.completed) return;
            foreach (var t in curQuestRewardsPickedSlots)
                t.selectedBorder.enabled = false;

            slotREF.selectedBorder.enabled = true;
            selectedRewardData = rewardData;
        }


        private void InitializeQuestStates()
        {
            selectedRewardData = null;
            backButton.SetActive(false);
            ClearAllQuestStateSlots();
            RPGBuilderUtilities.DisableCG(questContentCG);
            RPGBuilderUtilities.EnableCG(questStatesCG);

            FailedQuestTextParent.gameObject.SetActive(false);
            OnGoingQuestTextParent.gameObject.SetActive(false);
            CompletedQuestTextParent.gameObject.SetActive(false);

            foreach (var t in CharacterData.Instance.questsData)
            {
                var thisQuestDATA =
                    CharacterData.Instance.getQuestDATA(
                        RPGBuilderUtilities.GetQuestFromID(t.questID));
                // TODO CHECK QUEST REQUIREMENTS HERE
                if (thisQuestDATA == null) continue;
                var questREF = RPGBuilderUtilities.GetQuestFromID(thisQuestDATA.questID);
                switch (thisQuestDATA.state)
                {
                    case QuestManager.questState.abandonned:
                        break;
                    case QuestManager.questState.failed:
                    {
                        FailedQuestTextParent.gameObject.SetActive(true);
                        var newQuestStateSlot = Instantiate(questStateSlotPrefab, FailedQuestTextParent);
                        newQuestStateSlot.transform.SetSiblingIndex(FailedQuestTextParent.transform.GetSiblingIndex() + 1);
                        var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                        slotRef.InitSlot(questREF, QuestStateSlotFailedColor, questStateFailedIcon,
                            QuestStateSlotHolder.QuestSlotPanelType.questJournal);
                        curQuestStateSlots.Add(newQuestStateSlot);
                        break;
                    }
                    case QuestManager.questState.onGoing:
                    {
                        OnGoingQuestTextParent.gameObject.SetActive(true);
                        var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                        newQuestStateSlot.transform.SetSiblingIndex(OnGoingQuestTextParent.transform.GetSiblingIndex() + 1);
                        var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                        slotRef.InitSlot(questREF, QuestStateOnGoingColor, questStateAvailableIcon,
                            QuestStateSlotHolder.QuestSlotPanelType.questJournal);
                        curQuestStateSlots.Add(newQuestStateSlot);
                        break;
                    }
                    case QuestManager.questState.completed:
                    {
                        CompletedQuestTextParent.gameObject.SetActive(true);
                        var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                        newQuestStateSlot.transform.SetSiblingIndex(
                            CompletedQuestTextParent.transform.GetSiblingIndex() + 1);
                        var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                        slotRef.InitSlot(questREF, QuestStateSlotCompletedColor, questStateCompletedIcon,
                            QuestStateSlotHolder.QuestSlotPanelType.questJournal);
                        curQuestStateSlots.Add(newQuestStateSlot);
                        break;
                    }
                }
            }
        }

        public void ClickTrackQuest()
        {
            QuestTrackerDisplayManager.Instance.TrackQuest(curQuestViewed);
        }

        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            InitializeQuestStates();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void UpdateShow()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            InitializeQuestStates();
        }


        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();
            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }
    }
}