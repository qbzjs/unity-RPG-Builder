using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class TaskCheckerManager : MonoBehaviour
    {
        public static TaskCheckerManager Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }


        public void CheckQuestObjectives(RPGNpc npcKilled)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.killNPC ||
                    taskREF.npcToKillID != npcKilled.ID) continue;
                UpdateQuestObjectiveProgress(i, x, 1);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }
        
        public void CheckQuestObjectives(RPGNpc npcTalkedTo, bool talked)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.talkToNPC ||
                    taskREF.npcToTalkToID != npcTalkedTo.ID) continue;
                CompleteQuestObjective(i, x);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }

        public void CompleteTaskInstantly(int questIndex, int taskIndex)
        {
            CharacterData.Instance.questsData[questIndex].objectives[taskIndex].state =
                QuestManager.questObjectiveState.completed;
            CharacterData.Instance.questsData[questIndex].objectives[taskIndex].currentProgressValue =
                CharacterData.Instance.questsData[questIndex].objectives[taskIndex].maxProgressValue;
            isQuestCompleted(questIndex, taskIndex);
        }

        public void CheckQuestObjectives(RPGItem itemGained, int count)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.getItem ||
                    taskREF.itemToGetID != itemGained.ID) continue;
                UpdateQuestObjectiveProgress(i, x, count);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }
        
        public void CheckQuestObjectives(RPGItem itemGained, bool itemUsed)
        {
            if (!itemUsed) return;
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.useItem ||
                    taskREF.itemToUseID != itemGained.ID) continue;
                CompleteTaskInstantly(i, x);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }

        public void CheckQuestObjectives(string sceneName)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.enterScene ||
                    taskREF.sceneName != sceneName) continue;
                CompleteQuestObjective(i, x);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }

        public void CheckQuestObjectives(RPGAbility ab)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.learnAbility ||
                    taskREF.abilityToLearnID != ab.ID) continue;
                CompleteQuestObjective(i, x);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }

        public void CheckQuestObjectives(int classLevel)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.reachLevel || taskREF.taskValue != classLevel) continue;
                CompleteQuestObjective(i, x);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }
        
        public void CheckQuestObjectives(RPGWeaponTemplate wpTemplateREF, int level)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.reachWeaponTemplateLevel ||
                    taskREF.weaponTemplateRequiredID != wpTemplateREF.ID || taskREF.taskValue != level) continue;
                CompleteQuestObjective(i, x);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }

        public void CheckQuestObjectives(RPGSkill _skill, int skillLevel)
        {
            for (var i = 0; i < CharacterData.Instance.questsData.Count; i++)
            for (var x = 0; x < CharacterData.Instance.questsData[i].objectives.Count; x++)
            {
                var taskREF = RPGBuilderUtilities.GetTaskFromID(CharacterData.Instance.questsData[i].objectives[x].taskID);
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.reachSkillLevel ||
                    taskREF.skillRequiredID != _skill.ID || taskREF.taskValue != skillLevel) continue;
                CompleteQuestObjective(i, x);
                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                    RPGBuilderUtilities.GetQuestFromID(CharacterData.Instance.questsData[i].questID), x);
            }
        }

        private bool isQuestCompleted(int i, int x)
        {
            var tasksCompleted = true;
            foreach (var t in CharacterData.Instance.questsData[i].objectives)
                if (t.state != QuestManager.questObjectiveState.completed)
                    return false;

            CharacterData.Instance.questsData[i].state = QuestManager.questState.completed;
            if (QuestInteractionDisplayManager.Instance.thisCG.alpha == 1)
            {
                QuestInteractionDisplayManager.Instance.UpdateShow();
            }

            ScreenSpaceNameplates.Instance.TriggerNameplateInteractionIconUpdate();
            return tasksCompleted;
        }

        private bool isObjectiveActive(int i, int x)
        {
            return CharacterData.Instance.questsData[i].objectives[x].state != QuestManager.questObjectiveState.completed &&
                   CharacterData.Instance.questsData[i].objectives[x].state != QuestManager.questObjectiveState.failed;
        }

        private void CompleteQuestObjective(int i, int x)
        {
            CharacterData.Instance.questsData[i].objectives[x].state = QuestManager.questObjectiveState.completed;
            bool isQuestCompleted = this.isQuestCompleted(i, x);

        }

        private void FailQuestObjective(int i, int x)
        {
            CharacterData.Instance.questsData[i].objectives[x].state = QuestManager.questObjectiveState.failed;
        }

        private void UpdateQuestObjectiveProgress(int i, int x, int progressValue)
        {
            int curProgressValue = CharacterData.Instance.questsData[i].objectives[x].currentProgressValue;
            int maxProgressValue = CharacterData.Instance.questsData[i].objectives[x].maxProgressValue;
            if (curProgressValue >= maxProgressValue) return;
            CharacterData.Instance.questsData[i].objectives[x].currentProgressValue += progressValue;
            curProgressValue = CharacterData.Instance.questsData[i].objectives[x].currentProgressValue;
            
            if (curProgressValue > maxProgressValue)
            {
                CharacterData.Instance.questsData[i].objectives[x].currentProgressValue =
                    CharacterData.Instance.questsData[i].objectives[x].maxProgressValue;
                curProgressValue = CharacterData.Instance.questsData[i].objectives[x].currentProgressValue;
            }
            if (curProgressValue < 0)
            {
                CharacterData.Instance.questsData[i].objectives[x].currentProgressValue = 0;
                curProgressValue = 0;
            }
            if (curProgressValue == maxProgressValue) CompleteQuestObjective(i, x);
        }
        
    }
}