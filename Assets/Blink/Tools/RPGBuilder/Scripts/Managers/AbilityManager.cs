using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class AbilityManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static AbilityManager Instance { get; private set; }

        private bool abilityRequiresThisWeaponType(string weaponType, RPGAbility ab, int curRank)
        {
            var abilityRankID = ab.ranks[curRank];
            foreach (var t in abilityRankID.useRequirements)
                if (t.requirementType ==
                    RequirementsManager.AbilityUseRequirementType.weaponTypeEquipped
                    && t.weaponRequired == weaponType
                    && !RequirementsManager.Instance.isWeaponTypeEquipped(weaponType))
                    return true;

            return false;
        }

        public void RankDownAbility(RPGAbility ab, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.abilitiesData)
            {
                if (t.ID != ab.ID) continue;
                if (t.rank <= 0) continue;
                if(ab.learnedByDefault && t.rank == 1) continue;
                if (!CheckAbilityRankingDown(ab, tree)) continue;
                switch (t.rank)
                {
                    case 1 when ab.learnedByDefault:
                    case 1 when RPGBuilderUtilities.isAbilityUnlockedFromSpellbook(ab.ID):
                        continue;
                }

                var abilityRankID = ab.ranks[t.rank - 1];
                int unlockCost = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Ability + "+" +
                    RPGGameModifier.AbilityModifierType.Unlock_Cost,
                    abilityRankID.unlockCost, ab.ID, -1);
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID,unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, -unlockCost);
                t.rank--;

                if (t.rank == 0) t.known = false;
                TreesDisplayManager.Instance.InitTree(tree);
                AbilityTooltip.Instance.Hide();
                TreesDisplayManager.Instance.HideRequirements();
            }
        }

        public void RankUpAbility(RPGAbility ab, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.abilitiesData)
            {
                if (t.ID != ab.ID) continue;
                if (t.rank >= ab.ranks.Count) continue;
                if (!CheckAbilityRankingRequirements(ab, tree, t.rank)) continue;
                var abilityRankID = ab.ranks[t.rank];
                int unlockCost = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Ability + "+" +
                    RPGGameModifier.AbilityModifierType.Unlock_Cost,
                    abilityRankID.unlockCost, ab.ID, -1);
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID,unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, unlockCost);
                t.rank++;
                t.known = true;

                TreesDisplayManager.Instance.InitTree(tree);

                if (t.rank == 1)
                    CharacterEventsManager.Instance.AbilityLearned(ab);

                AbilityTooltip.Instance.Hide();
                TreesDisplayManager.Instance.HideRequirements();
            }
        }

        private bool CheckAbilityRankingDown(RPGAbility ab, RPGTalentTree tree)
        {
            foreach (var t in tree.nodeList)
            foreach (var t1 in t.requirements)
                if (t1.requirementType == RequirementsManager.RequirementType.abilityKnown &&
                    t1.abilityRequiredID == ab.ID &&
                    RPGBuilderUtilities.isAbilityKnown(t.abilityID) &&
                    RPGBuilderUtilities.getAbilityRank(ab.ID) == 0)
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent(
                        "Cannot unlearn an ability that is required for others", 3);
                    return false;
                }

            return true;
        }

        private bool CheckAbilityRankingRequirements(RPGAbility ab, RPGTalentTree tree, int rank)
        {
            var abilityRankID = ab.ranks[rank];
            if (CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < abilityRankID.unlockCost)
            {
                //NOT ENOUGH POINTS
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Not Enough Points", 3);
                return false;
            }

            List<bool> reqResults = new List<bool>();
            foreach (var t in RPGBuilderUtilities.getNodeRequirements(tree, ab))
            {
                var intValue1 = 0;
                switch (t.requirementType)
                {
                    case RequirementsManager.RequirementType.classLevel:
                        intValue1 = CharacterData.Instance.classDATA.currentClassLevel;
                        break;
                    case RequirementsManager.RequirementType.skillLevel:
                        intValue1 = RPGBuilderUtilities.getSkillLevel(t.skillRequiredID);
                        break;
                    case RequirementsManager.RequirementType.pointSpent:
                        intValue1 = RPGBuilderUtilities.getTreePointSpentAmount(tree);
                        break;
                    case RequirementsManager.RequirementType._class:
                        intValue1 = t.classRequiredID;
                        break;
                    case RequirementsManager.RequirementType.weaponTemplateLevel:
                        intValue1 = RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateRequiredID);
                        break;
                }
                reqResults.Add(RequirementsManager.Instance.HandleRequirementType(t, intValue1, 0, true));
            }

            return !reqResults.Contains(false);
        }
    }
}