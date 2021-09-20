using System.Collections.Generic;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class StatAllocationManager : MonoBehaviour
    {
        public static StatAllocationManager Instance { get; private set; }
    
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        public List<StatAllocationSlotDataHolder> SpawnStatAllocationSlot(CharacterData.AllocatedStatEntry allocatedStatEntry, GameObject prefab, Transform parent, List<StatAllocationSlotDataHolder> holderList, StatAllocationSlotDataHolder.SlotType slotType)
        {
            if(allocateStatSlotHasStatID(holderList, allocatedStatEntry.statID)) return holderList;
            RPGStat statREF = RPGBuilderUtilities.GetStatFromID(allocatedStatEntry.statID);
                
            GameObject newStatSlot = Instantiate(prefab, parent);
            newStatSlot.transform.localPosition = Vector3.zero;

            StatAllocationSlotDataHolder dataHolder = newStatSlot.GetComponent<StatAllocationSlotDataHolder>();
            dataHolder.thisStat = statREF;
            dataHolder.statNameText.text = statREF.displayName;
            dataHolder.slotType = slotType;
                
            holderList.Add(dataHolder);
            return holderList;
        }
        
        public void HandleStatAllocationButtons(int points, int maxPoints, List<StatAllocationSlotDataHolder> curStatAllocationSlots, StatAllocationSlotDataHolder.SlotType slotType)
        {
            if (points == 0)
            {
                foreach (var slot in curStatAllocationSlots)
                {
                    if (getAllocatedStatValue(slot.thisStat.ID, slotType) == 0)
                    {
                        slot.DecreaseButton.interactable = false;
                        slot.IncreaseButton.interactable = false;
                    }
                    else
                    {
                        slot.DecreaseButton.interactable = true;
                        slot.IncreaseButton.interactable = false;
                    }
                }
            }
            else if (points > 0 && points < maxPoints)
            {
                foreach (var slot in curStatAllocationSlots)
                {
                    if (getAllocatedStatValue(slot.thisStat.ID, slotType) == 0)
                    {
                        slot.DecreaseButton.interactable = false;
                        slot.IncreaseButton.interactable = true;
                    }
                    else
                    {
                        slot.DecreaseButton.interactable = true;
                        slot.IncreaseButton.interactable = true;
                    }
                }
            }
            else
            {
                foreach (var slot in curStatAllocationSlots)
                {
                    if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
                    {
                        slot.DecreaseButton.interactable = true;
                    }
                    else
                    {
                        slot.DecreaseButton.interactable = false;
                    }
                    slot.IncreaseButton.interactable = true;
                }
            }

            foreach (var allocatedStatSlot in curStatAllocationSlots)
            {
                int allocatedStatSlotValue = getAllocatedStatValue(allocatedStatSlot.thisStat.ID, slotType);
                if (!allocatedStatSlot.thisStat.isVitalityStat && allocatedStatSlot.thisStat.minCheck && allocatedStatSlotValue <= allocatedStatSlot.thisStat.minValue)
                {
                    allocatedStatSlot.DecreaseButton.interactable = false;
                }
                if (!allocatedStatSlot.thisStat.isVitalityStat && allocatedStatSlot.thisStat.maxCheck && allocatedStatSlotValue >= allocatedStatSlot.thisStat.maxValue)
                {
                    allocatedStatSlot.IncreaseButton.interactable = false;
                }
                
                float max = getMaxAllocatedStatValue(allocatedStatSlot.thisStat);
                if (max > 0 && allocatedStatSlotValue >= max)
                {
                    allocatedStatSlot.IncreaseButton.interactable = false;
                }
                
                int cost = GetStatAllocationCostAmount(allocatedStatSlot.thisStat.ID, slotType);
                if (cost > points)
                {
                    allocatedStatSlot.IncreaseButton.interactable = false;
                }

                if (slotType == StatAllocationSlotDataHolder.SlotType.Game && !RPGBuilderEssentials.Instance.combatSettings.canDescreaseGameStatPoints)
                {
                    allocatedStatSlot.DecreaseButton.interactable = false;
                }
            }
        }
        
        public void AlterAllocatedStat(int statID, bool increase, List<StatAllocationSlotDataHolder> curStatAllocationSlots, StatAllocationSlotDataHolder.SlotType slotType)
        {
            int statIndex = allocatedStatsHaveStatID(statID);
            int statSlotIndex = getAllocateStatSlotIndex(statID, curStatAllocationSlots);
            
            RPGStat statREF = RPGBuilderUtilities.GetStatFromID(statID);
            float max = getMaxAllocatedStatValue(statREF);
            int cost = GetStatAllocationCostAmount(statID, slotType);
            int currentPoints = getCurrentPoints(slotType);
            if (increase && cost > currentPoints) return;
            int valueAdded = GetStatAllocationAddAmount(statID, slotType);
            if (statIndex == -1)
            {
                CharacterData.AllocatedStatData newAllocatedStat = new CharacterData.AllocatedStatData();
                newAllocatedStat.statID = statID;
                newAllocatedStat.statName = statREF._name;
                if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
                {
                    newAllocatedStat.valueGame += increase ? valueAdded : -valueAdded;
                    newAllocatedStat.maxValueGame = max >= 0 ? (int)max : 0;
                    float currentValue = curStatAllocationSlots[statSlotIndex].thisStat.baseValue + newAllocatedStat.valueGame;
                    curStatAllocationSlots[statSlotIndex].curValueText.text = newAllocatedStat.maxValueGame > 0 ? currentValue + " / " + newAllocatedStat.maxValueGame :
                        currentValue.ToString();
                }
                else
                {
                    newAllocatedStat.value += increase ? valueAdded : -valueAdded;
                    float currentValue = curStatAllocationSlots[statSlotIndex].thisStat.baseValue + newAllocatedStat.value;
                    newAllocatedStat.maxValue = max >= 0 ? (int)max : 0;
                    curStatAllocationSlots[statSlotIndex].curValueText.text = newAllocatedStat.maxValue > 0 ? currentValue + " / " + newAllocatedStat.maxValue :
                        currentValue.ToString();
                }
                CharacterData.Instance.allocatedStatsData.Add(newAllocatedStat);
            }
            else
            {
                CharacterData.AllocatedStatData allocatedStat = CharacterData.Instance.allocatedStatsData[statIndex];
                if (increase && max > 0 && allocatedStat.value >= max) return;
                if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
                {
                    allocatedStat.valueGame += increase ? valueAdded : -valueAdded;
                    allocatedStat.maxValueGame = max >= 0 ? (int)max : 0;
                    float currentValue = curStatAllocationSlots[statSlotIndex].thisStat.baseValue + allocatedStat.valueGame;
                    curStatAllocationSlots[statSlotIndex].curValueText.text = allocatedStat.maxValueGame > 0 ? currentValue + " / " + allocatedStat.maxValueGame :
                        currentValue.ToString();
                }
                else
                {
                    allocatedStat.value += increase ? valueAdded : -valueAdded;
                    allocatedStat.maxValue = max >= 0 ? (int)max : 0;
                    float currentValue = curStatAllocationSlots[statSlotIndex].thisStat.baseValue + allocatedStat.value;
                    curStatAllocationSlots[statSlotIndex].curValueText.text = allocatedStat.maxValue > 0 ? currentValue + " / " + allocatedStat.maxValue :
                        currentValue.ToString();
                }
            }

            if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
            {
                if (increase)
                {
                    TreePointsManager.Instance.RemoveTreePoint(RPGBuilderEssentials.Instance.combatSettings.pointID, cost);
                    StatCalculator.UpdateStatAllocation(statID, valueAdded);
                }
                else
                {
                    TreePointsManager.Instance.AddTreePoint(RPGBuilderEssentials.Instance.combatSettings.pointID, cost);
                    StatCalculator.UpdateStatAllocation(statID, -valueAdded);
                }
                HandleStatAllocationButtons(
                    CharacterData.Instance.getTreePointsAmountByPoint(RPGBuilderEssentials.Instance.combatSettings.pointID),
                    0, StatAllocationDisplayManager.Instance.curStatSlots, slotType);
                StatAllocationDisplayManager.Instance.UpdateCurrentPointText();
                
            }
            else
            {
                CharacterData.Instance.mainMenuStatAllocationPoints += increase ? -cost : cost;
                HandleStatAllocationButtons(
                    CharacterData.Instance.mainMenuStatAllocationPoints,
                    CharacterData.Instance.mainMenuStatAllocationMaxPoints, MainMenuManager.Instance.curStatAllocationSlots, slotType);
                MainMenuManager.Instance.UpdateAllocationPointsText();
            }
            
            HandleBodyScaleFromStats(slotType);
        }

        private int getCurrentPoints(StatAllocationSlotDataHolder.SlotType slotType)
        {
            if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
            {
                return CharacterData.Instance.getTreePointsAmountByPoint(RPGBuilderEssentials.Instance.combatSettings
                    .pointID);
            }
            else
            {
                return CharacterData.Instance.mainMenuStatAllocationPoints;
            }
        }

        private int GetStatAllocationAddAmount(int statID, StatAllocationSlotDataHolder.SlotType slotType)
        {
            int addAmount = 0;
            if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
            {
                if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
                {
                    RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                    foreach (var statEntry in classREF.allocatedStatsEntriesGame)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.valueAdded > addAmount) addAmount = statEntry.valueAdded;
                    }
                }

                foreach (var skill in CharacterData.Instance.skillsDATA)
                {
                    RPGSkill skillREF = RPGBuilderUtilities.GetSkillFromID(skill.skillID);
                    foreach (var statEntry in skillREF.allocatedStatsEntriesGame)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.valueAdded > addAmount) addAmount = statEntry.valueAdded;
                    }
                }

                foreach (var weaponTemplate in CharacterData.Instance.weaponTemplates)
                {
                    RPGWeaponTemplate weaponTemplateREF =
                        RPGBuilderUtilities.GetWeaponTemplateFromID(weaponTemplate.weaponTemplateID);
                    foreach (var statEntry in weaponTemplateREF.allocatedStatsEntriesGame)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.valueAdded > addAmount) addAmount = statEntry.valueAdded;
                    }
                }
            }
            else
            {
                RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
                foreach (var statEntry in raceREF.allocatedStatsEntries)
                {
                    if (statEntry.statID != statID) continue;
                    addAmount = statEntry.valueAdded;
                }

                if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
                {
                    RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                    foreach (var statEntry in classREF.allocatedStatsEntries)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.valueAdded > addAmount) addAmount = statEntry.valueAdded;
                    }
                }
            }

            return addAmount;
        }

        private int GetStatAllocationCostAmount(int statID, StatAllocationSlotDataHolder.SlotType slotType)
        {
            int cost = 0;
            if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
            {
                if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
                {
                    RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                    foreach (var statEntry in classREF.allocatedStatsEntriesGame)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.cost > cost) cost = statEntry.cost;
                    }
                }

                foreach (var skill in CharacterData.Instance.skillsDATA)
                {
                    RPGSkill skillREF = RPGBuilderUtilities.GetSkillFromID(skill.skillID);
                    foreach (var statEntry in skillREF.allocatedStatsEntriesGame)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.cost > cost) cost = statEntry.cost;
                    }
                }

                foreach (var weaponTemplate in CharacterData.Instance.weaponTemplates)
                {
                    RPGWeaponTemplate weaponTemplateREF =
                        RPGBuilderUtilities.GetWeaponTemplateFromID(weaponTemplate.weaponTemplateID);
                    foreach (var statEntry in weaponTemplateREF.allocatedStatsEntriesGame)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.cost > cost) cost = statEntry.cost;
                    }
                }
            }
            else
            {
                RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
                foreach (var statEntry in raceREF.allocatedStatsEntries)
                {
                    if (statEntry.statID != statID) continue;
                    cost = statEntry.cost;
                }

                if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
                {
                    RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                    foreach (var statEntry in classREF.allocatedStatsEntries)
                    {
                        if (statEntry.statID != statID) continue;
                        if (statEntry.cost > cost) cost = statEntry.cost;
                    }
                }
            }

            return cost;
        }


        private void HandleBodyScaleFromStats(StatAllocationSlotDataHolder.SlotType slotType)
        {
            float bodyScaleModifier = 1;
            
            if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
            {
                foreach (var stat in CombatManager.playerCombatNode.nodeStats)
                {
                    foreach (var bonus in stat.stat.statBonuses)
                    {
                        if (bonus.statType == RPGStat.STAT_TYPE.BODY_SCALE)
                        {
                            bodyScaleModifier += stat.curValue * bonus.modifyValue;
                        }
                    }
                }
                
               CombatManager.playerCombatNode.appearanceREF.InitBodyScale(bodyScaleModifier);
            }
            else
            {
                foreach (var allocatedStat in CharacterData.Instance.allocatedStatsData)
                {
                    RPGStat statREF = RPGBuilderUtilities.GetStatFromID(allocatedStat.statID);
                    if(statREF==null) continue;
                    foreach (var bonus in statREF.statBonuses)
                    {
                        if(bonus.statType != RPGStat.STAT_TYPE.BODY_SCALE) continue;
                        if (slotType == StatAllocationSlotDataHolder.SlotType.Game)
                        {
                            bodyScaleModifier += bonus.modifyValue * allocatedStat.valueGame;
                        }
                        else
                        {
                            bodyScaleModifier += bonus.modifyValue * allocatedStat.value;
                        }
                    }
                }
                
                MainMenuManager.Instance.curPlayerModel.GetComponent<PlayerAppearanceHandler>().InitBodyScale(bodyScaleModifier);
            }
        }

        private int allocatedStatsHaveStatID(int statID)
        {
            for (var index = 0; index < CharacterData.Instance.allocatedStatsData.Count; index++)
            {
                var allocatedStat = CharacterData.Instance.allocatedStatsData[index];
                if (allocatedStat.statID == statID) return index;
            }

            return -1;
        }
        private int getAllocateStatSlotIndex(int statID, List<StatAllocationSlotDataHolder> curStatAllocationSlots)
        {
            for (var index = 0; index < curStatAllocationSlots.Count; index++)
            {
                var allocatedStat = curStatAllocationSlots[index];
                if (allocatedStat.thisStat.ID == statID) return index;
            }

            return -1;
        }
        
        public float getMaxAllocatedStatValue(RPGStat statREF)
        {
            float max = 0;
            RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);

            foreach (var allocatedStatEntry in raceREF.allocatedStatsEntries)
            {
                if (allocatedStatEntry.statID == statREF.ID && allocatedStatEntry.maxValue > 0)
                    max += allocatedStatEntry.maxValue;
            }
            
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);

                foreach (var allocatedStatEntry in classREF.allocatedStatsEntries)
                {
                    if (allocatedStatEntry.statID == statREF.ID && allocatedStatEntry.maxValue > 0)
                        max += allocatedStatEntry.maxValue;
                }
            }

            return max;
        }

        public int getAllocatedStatValue(int statID, StatAllocationSlotDataHolder.SlotType slotType)
        {
            foreach (var allocatedStat in CharacterData.Instance.allocatedStatsData)
            {
                if (allocatedStat.statID == statID)
                    return slotType == StatAllocationSlotDataHolder.SlotType.Game
                        ? allocatedStat.valueGame
                        : allocatedStat.value;
            }

            return 0;
        }

        public bool allocateStatSlotHasStatID(List<StatAllocationSlotDataHolder> holderList, int statID)
        {
            foreach (var allocatedStat in holderList)
            {
                if (allocatedStat.thisStat.ID == statID) return true;
            }

            return false;
        }
    }
}
