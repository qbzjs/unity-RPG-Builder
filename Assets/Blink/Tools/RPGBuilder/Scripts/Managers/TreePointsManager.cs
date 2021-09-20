using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class TreePointsManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static TreePointsManager Instance { get; private set; }

        public void CheckIfItemGainPoints(RPGItem item)
        {
            foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
            foreach (var t1 in t.gainPointRequirements)
                if (t1.gainType ==
                    RPGTreePoint.TreePointGainRequirementTypes.itemGained
                    && t1.itemRequiredID == item.ID)
                    AddTreePoint(t.ID,
                        t1.amountGained);
        }

        public void CheckIfNPCkKilledGainPoints(RPGNpc npc)
        {
            foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
            foreach (var t1 in t.gainPointRequirements)
                if (t1.gainType ==
                    RPGTreePoint.TreePointGainRequirementTypes.npcKilled
                    && t1.npcRequiredID == npc.ID)
                    AddTreePoint(t.ID,
                        t1.amountGained);
        }

        public void CheckIfClassLevelUpGainPoints(RPGClass _class)
        {
            foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
            foreach (var t1 in t.gainPointRequirements)
                if (t1.gainType ==
                    RPGTreePoint.TreePointGainRequirementTypes.classLevelUp
                    && t1.classRequiredID == _class.ID)
                    AddTreePoint(t.ID,
                        t1.amountGained);
        }

        public void CheckIfWeaponTemplateLevelUpGainPoints(RPGWeaponTemplate weaponTemplate)
        {
            foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
            foreach (var t1 in t.gainPointRequirements)
                if (t1.gainType ==
                    RPGTreePoint.TreePointGainRequirementTypes.weaponTemplateLevelUp
                    && t1.weaponTemplateRequiredID == weaponTemplate.ID)
                    AddTreePoint(t.ID, t1.amountGained);
        }

        public void CheckIfSkillLevelUpGainPoints(RPGSkill _skill)
        {
            foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
            foreach (var t1 in t.gainPointRequirements)
                if (t1.gainType ==
                    RPGTreePoint.TreePointGainRequirementTypes.skillLevelUp
                    && t1.skillRequiredID == _skill.ID)
                    AddTreePoint(t.ID,
                        t1.amountGained);
        }

        public void AddTreePoint(int treeTypeID, int amount)
        {
            foreach (var t in CharacterData.Instance.treePoints)
            {
                if (t.treePointID != treeTypeID) continue;
                RPGTreePoint pointREF = RPGBuilderUtilities.GetTreePointFromID(t.treePointID);
                amount = getGainValue(pointREF, amount);
                t.amount += amount;
                Clamp(pointREF, t);
            }
            Toolbar.Instance.InitToolbar();
        }

        public void RemoveTreePoint(int ID, int amount)
        {
            foreach (var t in CharacterData.Instance.treePoints)
            {
                if (t.treePointID != ID) continue;
                t.amount -= amount;
                if (t.amount == 0)
                {
                    Toolbar.Instance.InitToolbar();
                }
            }
        }



        public static void Clamp(RPGTreePoint treePoint, CharacterData.TreePoints_DATA pointsData)
        {
            int maxValue = getMaxValue(treePoint);
            if (treePoint.maxPoints > 0 && CharacterData.Instance.getTreePointsAmountByPoint(treePoint.ID) > maxValue)
            {
                pointsData.amount = maxValue;
            }
        }



        private static int getMaxValue(RPGTreePoint treePoint)
        {
            return (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.TreePoint + "+" +
                RPGGameModifier.PointModifierType.Max, treePoint.maxPoints, treePoint.ID, -1);
        }

        private static int getGainValue(RPGTreePoint treePoint, int baseGainAmount)
        {
            return (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.TreePoint + "+" +
                RPGGameModifier.PointModifierType.Gain_Value, baseGainAmount, treePoint.ID, -1);
        }
    }
}