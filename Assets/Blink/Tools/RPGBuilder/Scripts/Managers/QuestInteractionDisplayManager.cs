using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class QuestInteractionDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG, questStatesCG, questContentCG;

        public TextMeshProUGUI questNameText, descriptionText, repeatableText;

        public Transform itemsGivenParent,
            automaticRewardsParent,
            rewardsToPickParent,
            AvailableQuestTextParent,
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

        private CombatNode currentQuestGiverNode;
        private bool isShowing;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static QuestInteractionDisplayManager Instance { get; private set; }

        public void ClickAcceptQuest()
        {
            AcceptQuest(curQuestViewed);
        }

        public void AcceptQuest(RPGQuest questToAccept)
        {
            var thisQuestINDEX = CharacterData.Instance.getQuestINDEX(questToAccept);
            if (thisQuestINDEX != -1)
            {
                if (CharacterData.Instance.questsData[thisQuestINDEX].state == QuestManager.questState.abandonned)
                {
                    List<InventoryManager.TemporaryLootItemData> allLoot = new List<InventoryManager.TemporaryLootItemData>();
                    foreach (var t in questToAccept.itemsGiven)
                    {
                        InventoryManager.Instance.HandleLootList(t.itemID, allLoot, t.count);
                    }
                        
                    if (RPGBuilderUtilities.GetAllSlotsNeeded(allLoot) > InventoryManager.Instance.getEmptySlotsCount())
                    {
                        // Cancel Items Given
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                        return;
                    }

                    foreach (var loot in allLoot)
                    {
                        RPGBuilderUtilities.HandleItemLooting(loot.itemID, loot.count, false, false);
                    }
                    
                    CharacterData.Instance.questsData[thisQuestINDEX].state = QuestManager.questState.onGoing;
                    CharacterData.Instance.questsData[thisQuestINDEX].objectives.Clear();
                    foreach (var t in questToAccept.objectives)
                    {
                        var newObjective = new CharacterData.QuestDATA.Quest_ObjectiveDATA();
                        var taskREF = RPGBuilderUtilities.GetTaskFromID(t.taskID);
                        newObjective.taskID = taskREF.ID;
                        newObjective.state = QuestManager.questObjectiveState.onGoing;
                        newObjective.currentProgressValue = 0;
                        newObjective.maxProgressValue = taskREF.taskValue;
                        CharacterData.Instance.questsData[thisQuestINDEX].objectives.Add(newObjective);
                    }
                    
                    InitializeQuestStates(curNPCViewed);
                }
            }
            else
            {
                List<InventoryManager.TemporaryLootItemData> allLoot = new List<InventoryManager.TemporaryLootItemData>();
                foreach (var t in questToAccept.itemsGiven)
                {
                    InventoryManager.Instance.HandleLootList(t.itemID, allLoot, t.count);
                }
                        
                if (RPGBuilderUtilities.GetAllSlotsNeeded(allLoot) > InventoryManager.Instance.getEmptySlotsCount())
                {
                    // Cancel Items Given
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                    return;
                }

                foreach (var loot in allLoot)
                {
                    RPGBuilderUtilities.HandleItemLooting(loot.itemID, loot.count, false, false);
                }
                
                var newQuest = new CharacterData.QuestDATA();
                newQuest.questID = questToAccept.ID;
                newQuest.state = QuestManager.questState.onGoing;
                foreach (var t in questToAccept.objectives)
                {
                    var newObjective = new CharacterData.QuestDATA.Quest_ObjectiveDATA();
                    var taskREF = RPGBuilderUtilities.GetTaskFromID(t.taskID);
                    newObjective.taskID = taskREF.ID;
                    newObjective.state = QuestManager.questObjectiveState.onGoing;
                    newObjective.currentProgressValue = 0;
                    newObjective.maxProgressValue = taskREF.taskValue;
                    newQuest.objectives.Add(newObjective);
                }

                CharacterData.Instance.questsData.Add(newQuest);
                InitializeQuestStates(curNPCViewed);
            }

            ScreenSpaceNameplates.Instance.TriggerNameplateInteractionIconUpdate();
        }

        public void ClickAbandonQuest()
        {
            AbandonQuest(curQuestViewed);
            InitializeQuestStates(curNPCViewed);
        }

        public void AbandonQuest(RPGQuest questToAccept)
        {
            var thisQuestINDEX = CharacterData.Instance.getQuestINDEX(questToAccept);
            if (thisQuestINDEX != -1)
            {
                if (CharacterData.Instance.questsData[thisQuestINDEX].state == QuestManager.questState.completed
                    || CharacterData.Instance.questsData[thisQuestINDEX].state == QuestManager.questState.failed
                    || CharacterData.Instance.questsData[thisQuestINDEX].state == QuestManager.questState.onGoing
                )
                {
                    CharacterData.Instance.questsData[thisQuestINDEX].state = QuestManager.questState.abandonned;
                    CharacterData.Instance.questsData[thisQuestINDEX].objectives.Clear();
                }

                if (QuestTrackerDisplayManager.Instance.isQuestAlreadyTracked(questToAccept))
                    QuestTrackerDisplayManager.Instance.UnTrackQuest(questToAccept);
            }

            ScreenSpaceNameplates.Instance.TriggerNameplateInteractionIconUpdate();
        }

        public void ClickCompleteQuest()
        {
            var thisQuestINDEX = CharacterData.Instance.getQuestINDEX(curQuestViewed);
            if (CharacterData.Instance.questsData[thisQuestINDEX].state == QuestManager.questState.completed)
                if (RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[thisQuestINDEX].questID)
                    .rewardsToPick.Count > 0)
                    if (selectedRewardData == null)
                    {
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("You need to pick a reward", 3);
                        return;
                    }
            CompleteQuest(curQuestViewed, selectedRewardData);
        }

        private bool QuestItemsConditionChecked(RPGQuest questToAccept)
        {
            var conditionsMet = false;
            foreach (var t in questToAccept.objectives)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(t.taskID);
                switch (taskREF.taskType)
                {
                    case RPGTask.TASK_TYPE.getItem when !taskREF.keepItems:
                    {
                        var itemREF = RPGBuilderUtilities.GetItemFromID(taskREF.itemToGetID);
                        var ttlcount = InventoryManager.Instance.getTotalCountOfItem(itemREF);

                        if (ttlcount >= taskREF.taskValue)
                        {
                            InventoryManager.Instance.RemoveItem(itemREF.ID, taskREF.taskValue, -1, -1, false);
                            conditionsMet = true;
                        }
                        else
                        {
                            return false;
                        }

                        break;
                    }
                    case RPGTask.TASK_TYPE.getItem when taskREF.keepItems:
                        conditionsMet = true;
                        break;
                    default:
                        return true;
                }
            }

            return conditionsMet;
        }

        public void CompleteQuest(RPGQuest questToAccept, RPGQuest.QuestRewardDATA rewardDATA)
        {
            var thisQuestINDEX = CharacterData.Instance.getQuestINDEX(questToAccept);
            if (thisQuestINDEX != -1)
                if (CharacterData.Instance.questsData[thisQuestINDEX].state == QuestManager.questState.completed)
                {
                    var questREF =
                        RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[thisQuestINDEX].questID);
                    if (!QuestItemsConditionChecked(questToAccept))
                    {
                        return;
                    }

                    List<InventoryManager.TemporaryLootItemData> lootList =
                        new List<InventoryManager.TemporaryLootItemData>();
                    if (questREF.rewardsToPick.Count > 0)
                    {
                        if (selectedRewardData != null)
                        {
                            if (rewardDATA.rewardType == RPGQuest.QuestRewardType.item)
                            {
                                InventoryManager.Instance.HandleLootList(rewardDATA.itemID, lootList, rewardDATA.count);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    
                    foreach (var t in questREF.rewardsGiven)
                    {
                        if (t.rewardType == RPGQuest.QuestRewardType.item)
                        {
                            InventoryManager.Instance.HandleLootList(t.itemID, lootList, t.count);
                        }
                        else
                        {
                            ProcessQuestReward(t);
                        }
                    }
                    
                    
                    if (RPGBuilderUtilities.GetAllSlotsNeeded(lootList) > InventoryManager.Instance.getEmptySlotsCount())
                    {
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                        return;
                    }

                    if (questREF.rewardsToPick.Count > 0)
                    {
                        if (selectedRewardData != null)
                        {
                            if (rewardDATA.rewardType != RPGQuest.QuestRewardType.item)
                            {
                                ProcessQuestReward(rewardDATA);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    foreach (var loot in lootList)
                    {
                        RPGBuilderUtilities.HandleItemLooting(loot.itemID, loot.count, false, false);
                    }

                    CharacterData.Instance.questsData[thisQuestINDEX].state = QuestManager.questState.turnedIn;
                    CharacterData.Instance.questsData[thisQuestINDEX].objectives.Clear();


                    if (QuestTrackerDisplayManager.Instance.isQuestAlreadyTracked(questToAccept))
                        QuestTrackerDisplayManager.Instance.UnTrackQuest(questToAccept);
                    
                    InitializeQuestStates(curNPCViewed);
                }

            ScreenSpaceNameplates.Instance.TriggerNameplateInteractionIconUpdate();
        }

        private void ProcessQuestReward(RPGQuest.QuestRewardDATA rewardDATA)
        {
            switch (rewardDATA.rewardType)
            {
                case RPGQuest.QuestRewardType.item:
                    RPGBuilderUtilities.HandleItemLooting(rewardDATA.itemID, rewardDATA.count, false, false);
                    break;
                case RPGQuest.QuestRewardType.currency:
                    InventoryManager.Instance.AddCurrency(rewardDATA.currencyID, rewardDATA.count);
                    break;
                case RPGQuest.QuestRewardType.Experience:
                    if(RPGBuilderEssentials.Instance.combatSettings.useClasses)LevelingManager.Instance.AddClassXP(rewardDATA.Experience);
                    break;
                case RPGQuest.QuestRewardType.treePoint:
                    TreePointsManager.Instance.AddTreePoint(rewardDATA.treePointID, rewardDATA.count);
                    break;
                case RPGQuest.QuestRewardType.FactionPoint:
                    if (rewardDATA.count > 0)
                    {
                        FactionManager.Instance.AddFactionPoint(rewardDATA.factionID, rewardDATA.count);
                    }
                    else
                    {
                        FactionManager.Instance.RemoveFactionPoint(rewardDATA.factionID, Mathf.Abs(rewardDATA.count));
                    }
                    break;
                case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    LevelingManager.Instance.AddWeaponTemplateXP(rewardDATA.weaponTemplateID, rewardDATA.Experience);
                    break;
            }
        }


        public void BackToQuestStates()
        {
            InitializeQuestStates(curNPCViewed);
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

        public void InitializeQuestContent(RPGQuest ClickedQuest, bool fromNPC)
        {
            if(thisCG.alpha == 0) Show();
            backButton.SetActive(fromNPC);

            curQuestViewed = ClickedQuest;
            var thisQuestDATA = CharacterData.Instance.getQuestDATA(curQuestViewed);
            ClearAllQuestContentSlots();
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

            foreach (var t in ClickedQuest.objectives)
            {
                var newObjectiveSlot = Instantiate(objectiveTextPrefab, QuestContentSlotParent);
                newObjectiveSlot.transform.SetSiblingIndex(repeatableText.transform.GetSiblingIndex() + 1);
                var slotRef = newObjectiveSlot.GetComponent<QuestObjectiveTextSlot>();
                slotRef.InitSlot(
                    GenerateObjectiveText(RPGBuilderUtilities.GetTaskFromID(t.taskID)));
                curQuestObjectiveTextSlots.Add(newObjectiveSlot);
            }

            if (thisQuestDATA != null)
                switch (thisQuestDATA.state)
                {
                    case QuestManager.questState.abandonned:
                        AcceptQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.completed:
                        TurnInQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.failed:
                    case QuestManager.questState.onGoing:
                        AbandonQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.turnedIn:
                        break;
                }
            else
                AcceptQuestButton.SetActive(true);
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
                    slotRef.InitSlotWeaponXP(ClickedQuest.rewardsGiven[i].Experience, type, ClickedQuest.rewardsGiven[i]);
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
            }

            return slotRef;
        }

        public void SelectAReward(QuestItemSlotHolder slotREF, RPGQuest.QuestRewardDATA rewardData)
        {
            var thisQuestDATA = CharacterData.Instance.getQuestDATA(curQuestViewed);
            if (thisQuestDATA == null) return;
            if (CharacterData.Instance.questsData[CharacterData.Instance.getQuestINDEX(curQuestViewed)].state !=
                QuestManager.questState.completed) return;
            foreach (var t in curQuestRewardsPickedSlots)
                t.selectedBorder.enabled = false;

            slotREF.selectedBorder.enabled = true;
            selectedRewardData = rewardData;
        }

        public string GenerateObjectiveText(RPGTask task)
        {
            var objectiveText = "  - ";
            switch (task.taskType)
            {
                case RPGTask.TASK_TYPE.enterRegion:

                    break;
                case RPGTask.TASK_TYPE.enterScene:
                    objectiveText += "Enter " + task.sceneName;
                    break;
                case RPGTask.TASK_TYPE.getItem:
                    objectiveText += "Obtain " + task.taskValue + " " +
                                     RPGBuilderUtilities.GetItemFromID(task.itemToGetID).displayName;
                    break;
                case RPGTask.TASK_TYPE.killNPC:
                    objectiveText += "Kill " + task.taskValue + " " +
                                     RPGBuilderUtilities.GetNPCFromID(task.npcToKillID).displayName;
                    break;
                case RPGTask.TASK_TYPE.learnAbility:
                    objectiveText += "Learn the " + RPGBuilderUtilities.GetAbilityFromID(task.abilityToLearnID).displayName +
                                     " ability";
                    break;
                case RPGTask.TASK_TYPE.learnRecipe:

                    break;
                case RPGTask.TASK_TYPE.reachLevel:
                    objectiveText += "Reach " + RPGBuilderUtilities.GetClassFromID(task.classRequiredID).displayName + " level " +
                                     task.taskValue;
                    break;
                case RPGTask.TASK_TYPE.reachSkillLevel:
                    objectiveText += "Reach " + RPGBuilderUtilities.GetSkillFromID(task.skillRequiredID).displayName + " level " +
                                     task.taskValue;
                    break;
                case RPGTask.TASK_TYPE.talkToNPC:
                    objectiveText += "Talk to " + RPGBuilderUtilities.GetNPCFromID(task.npcToTalkToID).displayName;
                    break;
                case RPGTask.TASK_TYPE.useItem:
                    objectiveText += "Use the " + RPGBuilderUtilities.GetItemFromID(task.itemToUseID).displayName;
                    break;
            }

            return objectiveText;
        }


        private void InitializeQuestStates(RPGNpc npc)
        {
            if (npc == null)
            {
                Hide();
                return;
            }
            selectedRewardData = null;
            curNPCViewed = npc;
            backButton.SetActive(false);
            ClearAllQuestStateSlots();
            RPGBuilderUtilities.DisableCG(questContentCG);
            RPGBuilderUtilities.EnableCG(questStatesCG);

            AvailableQuestTextParent.gameObject.SetActive(false);
            OnGoingQuestTextParent.gameObject.SetActive(false);
            CompletedQuestTextParent.gameObject.SetActive(false);

            foreach (var t in npc.questGiven)
            {
                var questREF1 = RPGBuilderUtilities.GetQuestFromID(t.questID);
                if (!QuestManager.Instance.CheckQuestRequirements(questREF1)) continue;
                var thisQuestDATA = CharacterData.Instance.getQuestDATA(questREF1);

                if (thisQuestDATA != null)
                {
                    var questREF = RPGBuilderUtilities.GetQuestFromID(thisQuestDATA.questID);
                    switch (thisQuestDATA.state)
                    {
                        case QuestManager.questState.abandonned:
                        {
                            AvailableQuestTextParent.gameObject.SetActive(true);
                            var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                            newQuestStateSlot.transform.SetSiblingIndex(
                                AvailableQuestTextParent.transform.GetSiblingIndex() + 1);
                            var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                            slotRef.InitSlot(questREF, QuestStateSlotAvailableColor, questStateAvailableIcon,
                                QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                            curQuestStateSlots.Add(newQuestStateSlot);
                            break;
                        }
                        case QuestManager.questState.failed:
                        case QuestManager.questState.onGoing:
                        {
                            OnGoingQuestTextParent.gameObject.SetActive(true);
                            var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                            newQuestStateSlot.transform.SetSiblingIndex(OnGoingQuestTextParent.transform.GetSiblingIndex() +
                                                                        1);
                            var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                            slotRef.InitSlot(questREF, QuestStateOnGoingColor, questStateAvailableIcon,
                                QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                            curQuestStateSlots.Add(newQuestStateSlot);
                            break;
                        }
                    }
                }
                else
                {
                    AvailableQuestTextParent.gameObject.SetActive(true);
                    var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                    newQuestStateSlot.transform.SetSiblingIndex(
                        AvailableQuestTextParent.transform.GetSiblingIndex() + 1);
                    var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                    slotRef.InitSlot(questREF1, QuestStateSlotAvailableColor, questStateAvailableIcon,
                        QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                    curQuestStateSlots.Add(newQuestStateSlot);
                }
            }

            for (var i = 0; i < npc.questCompleted.Count; i++)
            {
                var questREF1 = RPGBuilderUtilities.GetQuestFromID(npc.questCompleted[i].questID);
                var thisQuestDATA = CharacterData.Instance.getQuestDATA(questREF1);
                // TODO CHECK QUEST REQUIREMENTS HERE
                if (thisQuestDATA == null) continue;
                if (thisQuestDATA.state != QuestManager.questState.completed) continue;
                var questREF = RPGBuilderUtilities.GetQuestFromID(thisQuestDATA.questID);
                CompletedQuestTextParent.gameObject.SetActive(true);
                var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                newQuestStateSlot.transform.SetSiblingIndex(
                    CompletedQuestTextParent.transform.GetSiblingIndex() + 1);
                var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                slotRef.InitSlot(questREF, QuestStateSlotCompletedColor, questStateCompletedIcon,
                    QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                curQuestStateSlots.Add(newQuestStateSlot);
            }
        }

        public void Show()
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(true);
        }
        public void Show(CombatNode cbtNode)
        {
            isShowing = true;
            currentQuestGiverNode = cbtNode;
            Show();
            InitializeQuestStates(cbtNode.npcDATA);
        }

        public void UpdateShow()
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            InitializeQuestStates(curNPCViewed);
        }


        public void Hide()
        {
            isShowing = false;
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!isShowing || currentQuestGiverNode == null) return;
            if(Vector3.Distance(currentQuestGiverNode.transform.position, CombatManager.playerCombatNode.transform.position) > 4) Hide();
        }
    }
}