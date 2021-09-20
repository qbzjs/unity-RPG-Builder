using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class QuestManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static QuestManager Instance { get; private set; }

        public enum questState
        {
            onGoing,
            completed,
            abandonned,
            failed,
            turnedIn
        }

        public enum questObjectiveState
        {
            onGoing,
            completed,
            failed
        }

        public bool isQuestMatchingState(RPGQuest _quest, questState state)
        {
            var thisQuestDATA = CharacterData.Instance.getQuestDATA(_quest);
            if (thisQuestDATA == null)
                return false;
            return thisQuestDATA.state == state;
        }

        public bool CheckQuestRequirements(RPGQuest quest)
        {
            List<bool> reqResults = new List<bool>();
            foreach (var t in quest.questRequirements)
            {
                var intValue1 = 0;
                var intValue2 = 0;
                switch (t.requirementType)
                {
                    case RequirementsManager.RequirementType.classLevel:
                        intValue1 = CharacterData.Instance.classDATA.currentClassLevel;
                        break;
                    case RequirementsManager.RequirementType.skillLevel:
                        intValue1 = RPGBuilderUtilities.getSkillLevel(t.skillRequiredID);
                        break;
                    case RequirementsManager.RequirementType._class:
                        intValue1 = t.classRequiredID;
                        break;
                    case RequirementsManager.RequirementType.weaponTemplateLevel:
                        intValue1 = RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateRequiredID);
                        break;
                }
                reqResults.Add(RequirementsManager.Instance.HandleRequirementType(t, intValue1, intValue2,false));
            }

            return !reqResults.Contains(false);
        }
    }
}