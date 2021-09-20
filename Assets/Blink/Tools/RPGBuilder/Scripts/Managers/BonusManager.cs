using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class BonusManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static BonusManager Instance { get; private set; }

        public void InitBonuses()
        {
            foreach (var t in CharacterData.Instance.bonusesData)
                if (!t.On && RPGBuilderUtilities.isBonusKnown(t.ID)) InitBonus(RPGBuilderUtilities.GetBonusFromID(t.ID));
        }
    
        public void ResetAllOnBonuses()
        {
            foreach (var t in CharacterData.Instance.bonusesData) t.On = false;
        }


        private CharacterData.Bonus_DATA getBonusDATAByBonus (RPGBonus bonus)
        {
            foreach (var t in CharacterData.Instance.bonusesData)
                if (t.ID == bonus.ID) return t;

            return null;
        }

        public void InitBonus(RPGBonus ab)
        {
            var bnsDATA = getBonusDATAByBonus(ab);
            if (!RPGBuilderUtilities.isBonusKnown(ab.ID) || bnsDATA.On) return;
            var curRank = RPGBuilderUtilities.getBonusRank(ab.ID);
            if (curRank < 0) return;
            if (UseRequirementsMet(ab, curRank)) HandleBonusActions(ab);
        }

        private bool UseRequirementsMet(RPGBonus bonus, int curRank)
        {
            var rankREF = bonus.ranks[curRank];
            foreach (var t in rankREF.activeRequirements)
            {
                var intValue1 = 0;
                switch (t.requirementType)
                {
                    case RequirementsManager.BonusRequirementType.classLevel:
                        intValue1 = CharacterData.Instance.classDATA.currentClassLevel;
                        break;
                    case RequirementsManager.BonusRequirementType.skillLevel:
                        intValue1 = RPGBuilderUtilities.getSkillLevel(t.skillRequiredID);
                        break;
                    case RequirementsManager.BonusRequirementType.weaponTemplateLevel:
                        intValue1 = RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateRequiredID);
                        break;
                }
                if (!RequirementsManager.Instance.HandleBonusRequirementUseType(t, intValue1, false)) return false;
            }
            return true;
        }

        private void CancelBonus(RPGBonus ab, int curRank)
        {
            AlterBonusState(ab, false);
            StatCalculator.CalculateBonusStats();
        }

        private void AlterBonusState (RPGBonus bonus, bool isOn)
        {
            foreach (var bns in CharacterData.Instance.bonusesData)
                if(bns.ID == bonus.ID) bns.On = isOn;
        }

        public void CancelBonusFromUnequippedWeapon(string weaponType)
        {
            foreach (var t in CharacterData.Instance.bonusesData)
            {
                if (!t.On) continue;
                var bonusREF = RPGBuilderUtilities.GetBonusFromID(t.ID);
                var curRank = RPGBuilderUtilities.getBonusRank(bonusREF.ID);
                if (bonusRequireThisWeaponType(weaponType, bonusREF, curRank)) CancelBonus(bonusREF, curRank);
            }
        }

        private bool bonusRequireThisWeaponType(string weaponType, RPGBonus ab, int curRank)
        {
            var rankREF = ab.ranks[curRank];
            foreach (var t in rankREF.activeRequirements)
                if (t.requirementType == RequirementsManager.BonusRequirementType.weaponTypeEquipped
                    && t.weaponRequired == weaponType
                    && !RequirementsManager.Instance.isWeaponTypeEquipped(weaponType))
                    return true;
            return false;
        }

        private void HandleBonusActions(RPGBonus bonus)
        {
            AlterBonusState(bonus, true);
            StatCalculator.CalculateBonusStats();
        }


        public void RankDownBonus(RPGBonus bonus, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.bonusesData)
            {
                if (t.ID != bonus.ID) continue;
                if (t.rank <= 0) continue;
                if(bonus.learnedByDefault && t.rank == 1) continue;
                if (!CheckBonusRankingDown(bonus, tree)) continue;
                switch (t.rank)
                {
                    case 1 when bonus.learnedByDefault:
                    case 1 when RPGBuilderUtilities.isBonusUnlockedFromSpellbook(bonus.ID):
                        continue;
                }
                
                CancelBonus(bonus, t.rank - 1);
                var rankREF = bonus.ranks[t.rank - 1];
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID, rankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, -rankREF.unlockCost);
                t.rank--;

                if (t.rank == 0)
                {
                    t.known = false;
                }
                else
                {
                    InitBonus(bonus);
                }

                TreesDisplayManager.Instance.InitTree(tree);

                AbilityTooltip.Instance.Hide();
                TreesDisplayManager.Instance.HideRequirements();
            }
        }


        public void RankUpBonus(RPGBonus bonus, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.bonusesData)
            {
                if (t.ID != bonus.ID) continue;
                if (t.rank >= bonus.ranks.Count) continue;
                if (!CheckBonusRankingRequirements(bonus, tree, t.rank)) continue;
                var rankREF = bonus.ranks[t.rank];
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID, rankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree ,rankREF.unlockCost);
                t.rank++;
                t.known = true;

                TreesDisplayManager.Instance.InitTree(tree);

                if (t.rank == 1)
                {
                    CharacterEventsManager.Instance.BonusLearned(bonus);
                    InitBonus(bonus);
                }
                else if (t.rank > 1)
                {
                    var previousRank = -1;
                    previousRank += t.rank - 1;
                    CancelBonus(bonus, previousRank);
                    InitBonus(bonus);
                }

                AbilityTooltip.Instance.Hide();
                TreesDisplayManager.Instance.HideRequirements();
            }
        }


        private bool CheckBonusRankingDown(RPGBonus bonus, RPGTalentTree tree)
        {
            foreach (var t in tree.nodeList)
            foreach (var t1 in t.requirements)
            {
                if (t1.requirementType != RequirementsManager.RequirementType.bonusKnown ||
                    t1.bonusRequiredID != bonus.ID || !RPGBuilderUtilities.isBonusKnown(t.bonusID) ||
                    RPGBuilderUtilities.getBonusRank(bonus.ID) != 0) continue;
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Cannot unlearn a node that is required for others", 3);
                return false;
            }

            return true;
        }


        private bool CheckBonusRankingRequirements(RPGBonus bonus, RPGTalentTree tree, int rank)
        {
            var rankREF = bonus.ranks[rank];
            if (CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rankREF.unlockCost)
            {
                //NOT ENOUGH POINTS
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Not enough points", 3);
                return false;
            }


            List<bool> reqResults = new List<bool>();
            foreach (var t in RPGBuilderUtilities.getNodeRequirements(tree, bonus))
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
            
                reqResults.Add(RequirementsManager.Instance.HandleRequirementType(t, intValue1,0, true));
            }

            return !reqResults.Contains(false);
        }

    }
}
