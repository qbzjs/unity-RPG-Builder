using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

namespace BLINK.RPGBuilder.Logic
{
    public static class StatCalculator
    {

        private static void TriggerMoveSpeedChange()
        {
            CombatManager.playerCombatNode.playerControllerEssentials.MovementSpeedChange(RPGBuilderUtilities.getCurrentMoveSpeed(CombatManager.playerCombatNode));
        }
        
        public static void ResetPlayerStatsAfterRespawn()
        {
            foreach (var t in CombatManager.playerCombatNode.nodeStats)
                if (t.stat.isVitalityStat && t.stat.StartsAtMax)
                {
                    t.curValue = (int) t.curMaxValue;
                }

            UpdateStatUI();
        }

        private static void UpdateStatUI()
        {
            CombatManager.Instance.StatBarUpdate(RPGBuilderEssentials.Instance.healthStatReference._name);

            if (CharacterPanelDisplayManager.Instance.thisCG.alpha == 1)
                CharacterPanelDisplayManager.Instance.InitCharStats();
        }

        public static void UpdateClassLevelUpStats()
        {
            var thisClass = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
            bool moveSpeedChanged = false;
            foreach (var t in thisClass.stats)
            foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
            {
                var statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                if (t1._name != statREF._name) continue;
                HandleStat(CombatManager.playerCombatNode, statREF, t1, t.bonusPerLevel, t.isPercent, TemporaryStatSourceType.none);
                moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t1.stat);
            }

            UpdateStatUI();
            if(moveSpeedChanged) TriggerMoveSpeedChange();
        }

        public static void UpdateStatAllocation(int statID, int amount)
        {
            bool moveSpeedChanged = false;
            foreach (var t in CombatManager.playerCombatNode.nodeStats)
            {
                if(t.stat.ID != statID) continue;
                var statREF = RPGBuilderUtilities.GetStatFromID(t.stat.ID);
                if (t._name != statREF._name) continue;
                HandleStat(CombatManager.playerCombatNode, statREF, t, amount, false, TemporaryStatSourceType.none);
                moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t.stat);
            }
            if(moveSpeedChanged) TriggerMoveSpeedChange();
            UpdateStatUI();
        }
        
        public static void UpdateWeaponTemplateLevelUpStats(int wpTemplateID)
        {
            var wpTemplate = RPGBuilderUtilities.GetWeaponTemplateFromID(wpTemplateID);
            bool moveSpeedChanged = false;
            foreach (var t in wpTemplate.stats)
            foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
            {
                var statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                if (t1._name != statREF._name) continue;
                HandleStat(CombatManager.playerCombatNode, statREF, t1, t.amount, t.isPercent, TemporaryStatSourceType.none);
                moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t1.stat);
            }

            UpdateStatUI();
            if(moveSpeedChanged) TriggerMoveSpeedChange();
        }

        public static void UpdateSkillLevelUpStats(int skillID)
        {
            var thisSkill = RPGBuilderUtilities.GetSkillFromID(skillID);

            bool moveSpeedChanged = false;
            foreach (var t in thisSkill.stats)
            foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
            {
                var statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                if (t1._name != statREF._name) continue;
                HandleStat(CombatManager.playerCombatNode, statREF, t1, t.amount, t.isPercent, TemporaryStatSourceType.none);
                moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t1.stat);
            }

            UpdateStatUI();
            if(moveSpeedChanged) TriggerMoveSpeedChange();
        }

        public static void InitCharacterStats()
        {
            var race = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);

            foreach (var t in race.stats)
            foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
            {
                var statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                if (t1._name != statREF._name) continue;
                HandleStat(CombatManager.playerCombatNode, statREF, t1, t.amount, t.isPercent, TemporaryStatSourceType.none);
            }

            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                var thisClass = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                foreach (var t in thisClass.stats)
                foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
                {
                    var statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                    if (t1._name != statREF._name) continue;
                    float amt = t.amount + (t.bonusPerLevel * CharacterData.Instance.classDATA.currentClassLevel);
                    HandleStat(CombatManager.playerCombatNode, statREF, t1, amt, t.isPercent, TemporaryStatSourceType.none);
                }
            }

            foreach (var skill in CharacterData.Instance.skillsDATA)
            {
                var thisSkill = RPGBuilderUtilities.GetSkillFromID(skill.skillID);
                foreach (var t in thisSkill.stats)
                foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
                {
                    var statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                    if (t1._name != statREF._name) continue;
                    float amt = t.amount + (t.bonusPerLevel * RPGBuilderUtilities.getSkillLevel(skill.skillID));
                    HandleStat(CombatManager.playerCombatNode, statREF, t1, amt, t.isPercent, TemporaryStatSourceType.none);
                }
            }
            
            foreach (var allocatedStat in CharacterData.Instance.allocatedStatsData)
            {
                foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
                {
                    var statREF = RPGBuilderUtilities.GetStatFromID(allocatedStat.statID);
                    if (t1._name != statREF._name) continue;
                    HandleStat(CombatManager.playerCombatNode, statREF, t1, allocatedStat.value+allocatedStat.valueGame, false, TemporaryStatSourceType.none);
                }
            }

            
            foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
            {
                CombatNode.NODE_STATS temp = GameModifierManager.Instance.GetStatValueAfterGameModifier("Combat+Stat+Settings", t1, -1, true);
                
                if (t1.stat.isVitalityStat)
                {
                    t1.curMaxValue = temp.curMaxValue;
                    if (t1.stat.StartsAtMax)
                    {
                        t1.curValue = t1.curMaxValue;
                    }
                }
                else
                {
                    t1.curValue = temp.curValue;
                }
                
                if (t1.stat.minCheck)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MinOverride, t1.stat.ID);
                    t1.curMinValue = statOverride != -1 ? statOverride : temp.curMinValue;
                }
                if (t1.stat.maxCheck)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, t1.stat.ID);
                    t1.curMaxValue = statOverride != -1 ? statOverride : temp.curMaxValue;
                }
                
                HandleStat(CombatManager.playerCombatNode, t1.stat, t1, 0, false, TemporaryStatSourceType.none);
            }

            UpdateStatUI();
            TriggerMoveSpeedChange();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
        }

        public static void SetVitalityToMax()
        {
            foreach (var t in CombatManager.playerCombatNode.nodeStats)
            {
                if (!t.stat.isVitalityStat) continue;
                if (t.stat.StartsAtMax)
                {
                    t.curValue = (int) t.curMaxValue;
                }
                else
                {
                    t.curValue = 0;
                }
            }
        }

        private class TemporaryStatsDATA
        {
            public RPGStat stat;
            public float value;
        }

        public enum TemporaryStatSourceType
        {
            none,
            item,
            effect,
            bonus,
            shapeshifting
        }
        
        public class TemporaryActiveGearSetsDATA
        {
            public RPGGearSet gearSet;
            public int activeTierIndex;
        }

        private static List<TemporaryStatsDATA> AddStatsToTemp (List<TemporaryStatsDATA> tempList, RPGStat statREF, float value)
        {
            foreach (var t in tempList)
            {
                if (t.stat != statREF) continue;
                t.value += value;
                return tempList;
            }
            
            TemporaryStatsDATA newTempStatData = new TemporaryStatsDATA();
            newTempStatData.stat = statREF;
            newTempStatData.value = value;
            tempList.Add(newTempStatData);
            return tempList;
        }

        public static void HandleStat(CombatNode cbtNode, RPGStat statREF, CombatNode.NODE_STATS nodeStatData, float amount, bool isPercent, TemporaryStatSourceType sourceType)
        {
            float addedValue = amount;
            if (isPercent)
            {
                tempStatList = AddStatsToTemp(tempStatList, statREF, addedValue);
                return;
            }

            if (statREF.isVitalityStat)
            {
                float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Stat + "+" +
                    RPGGameModifier.StatModifierType.MaxOverride, statREF.ID);
                if (statOverride != -1)
                {
                    nodeStatData.curMaxValue = statOverride;
                }
                else
                {
                    nodeStatData.curMaxValue += addedValue;
                }
            }
            else
            {
                nodeStatData.curValue += addedValue;
            }

            switch (sourceType)
            {
                case TemporaryStatSourceType.item:
                    nodeStatData.valueFromItem += addedValue;
                    break;
                case TemporaryStatSourceType.effect:
                    nodeStatData.valueFromEffect += addedValue;
                    break;
                case TemporaryStatSourceType.bonus:
                    nodeStatData.valueFromBonus += addedValue;
                    break;
                case TemporaryStatSourceType.shapeshifting:
                    nodeStatData.valueFromShapeshifting += addedValue;
                    break;
            }

            ClampStat(statREF, CombatManager.playerCombatNode);
            CombatManager.Instance.StatBarUpdate(nodeStatData._name);
            
            if(cbtNode == CombatManager.playerCombatNode && RPGBuilderUtilities.isStatAffectingMoveSpeed(statREF)) TriggerMoveSpeedChange();
        }
        
        private static List<TemporaryStatsDATA> tempStatList = new List<TemporaryStatsDATA>();

        private static void ResetItemStats()
        {
            bool moveSpeedChanged = false;
            foreach (var t2 in CombatManager.playerCombatNode.nodeStats)
            {
                if(t2.valueFromItem == 0)continue;
                if (t2.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, t2.stat.ID);
                    if (statOverride != -1)
                    {
                        t2.curMaxValue = statOverride;
                    }
                    else
                    {
                        t2.curMaxValue -= t2.valueFromItem;
                    }
                }
                else
                {
                    t2.curValue -= t2.valueFromItem;
                }
                t2.valueFromItem = 0;
                ClampStat(t2.stat, CombatManager.playerCombatNode);
                CombatManager.Instance.StatBarUpdate(t2._name);
                if(!moveSpeedChanged)moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t2.stat);
            }
            if(moveSpeedChanged) TriggerMoveSpeedChange();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
            UpdateStatUI();
        }
        private static void ResetBonusStats()
        {
            bool moveSpeedChanged = false;
            foreach (var t2 in CombatManager.playerCombatNode.nodeStats)
            {
                if(t2.valueFromBonus == 0)continue;
                if (t2.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, t2.stat.ID);
                    if (statOverride != -1)
                    {
                        t2.curMaxValue = statOverride;
                    }
                    else
                    {
                        t2.curMaxValue -= t2.valueFromBonus;
                    }
                }
                else
                {
                    t2.curValue -= t2.valueFromBonus;
                }
                t2.valueFromBonus = 0;
                ClampStat(t2.stat, CombatManager.playerCombatNode);
                CombatManager.Instance.StatBarUpdate(t2._name);
                if(!moveSpeedChanged)moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t2.stat);
            }
            if(moveSpeedChanged) TriggerMoveSpeedChange();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
            UpdateStatUI();
        }
        private static void ResetEffectsStats(CombatNode cbtNode)
        {
            bool moveSpeedChanged = false;
            foreach (var t2 in cbtNode.nodeStats)
            {
                if(t2.valueFromEffect == 0)continue;
                if (t2.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, t2.stat.ID);
                    if (statOverride != -1)
                    {
                        t2.curMaxValue = statOverride;
                    }
                    else
                    {
                        t2.curMaxValue -= t2.valueFromEffect;
                    }
                }
                else
                {
                    t2.curValue -= t2.valueFromEffect;
                }
                t2.valueFromEffect = 0;
                ClampStat(t2.stat, cbtNode);
                if(cbtNode == CombatManager.playerCombatNode) CombatManager.Instance.StatBarUpdate(t2._name);
                if(!moveSpeedChanged)moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t2.stat);
            }
            if(moveSpeedChanged) TriggerMoveSpeedChange();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
            UpdateStatUI();
        }
        
        public static void ResetShapeshiftingStats(CombatNode cbtNode)
        {
            bool moveSpeedChanged = false;
            foreach (var t2 in cbtNode.nodeStats)
            {
                if(t2.valueFromShapeshifting == 0)continue;
                if (t2.stat.isVitalityStat)
                {
                    float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.Stat + "+" +
                        RPGGameModifier.StatModifierType.MaxOverride, t2.stat.ID);
                    if (statOverride != -1)
                    {
                        t2.curMaxValue = statOverride;
                    }
                    else
                    {
                        t2.curMaxValue -= t2.valueFromShapeshifting;
                    }
                }
                else
                {
                    t2.curValue -= t2.valueFromShapeshifting;
                }
                t2.valueFromShapeshifting = 0;
                ClampStat(t2.stat, cbtNode);
                if(cbtNode == CombatManager.playerCombatNode) CombatManager.Instance.StatBarUpdate(t2._name);
                if(!moveSpeedChanged)moveSpeedChanged = RPGBuilderUtilities.isStatAffectingMoveSpeed(t2.stat);
            }
            if(moveSpeedChanged) TriggerMoveSpeedChange();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
            UpdateStatUI();
        }

        private static List<InventoryManager.INVENTORY_EQUIPPED_ITEMS> getAllEquippedItems()
        {
            List<InventoryManager.INVENTORY_EQUIPPED_ITEMS> allItems = new List<InventoryManager.INVENTORY_EQUIPPED_ITEMS>();

            foreach (var t in InventoryManager.Instance.equippedArmors)
            {
                if (t.itemEquipped == null) continue;
                InventoryManager.INVENTORY_EQUIPPED_ITEMS newItem = new InventoryManager.INVENTORY_EQUIPPED_ITEMS();
                newItem.itemEquipped = t.itemEquipped;
                newItem.temporaryItemDataID = t.temporaryItemDataID;
                allItems.Add(newItem);
            }
            foreach (var t in InventoryManager.Instance.equippedWeapons)
            {
                if (t.itemEquipped == null) continue;
                InventoryManager.INVENTORY_EQUIPPED_ITEMS newItem = new InventoryManager.INVENTORY_EQUIPPED_ITEMS();
                newItem.itemEquipped = t.itemEquipped;
                newItem.temporaryItemDataID = t.temporaryItemDataID;
                allItems.Add(newItem);
            }

            return allItems;
        }

        private class VitalityStateBeforeChangesDATA
        {
            public int statID;
            public float percent;
        }

        static List<VitalityStateBeforeChangesDATA> getVitStates()
        {
            List<VitalityStateBeforeChangesDATA> vitStates = new List<VitalityStateBeforeChangesDATA>();
            foreach (var statData in CombatManager.playerCombatNode.nodeStats)
            {
                if (!statData.stat.isVitalityStat) continue;
                VitalityStateBeforeChangesDATA newVitState = new VitalityStateBeforeChangesDATA();
                newVitState.statID = statData.stat.ID;
                newVitState.percent = CombatManager.playerCombatNode.getCurrentValue(statData.stat._name) /
                                      CombatManager.playerCombatNode.getCurrentMaxValue(statData.stat._name);
                newVitState.percent *= 100;
                if (newVitState.percent < 1) newVitState.percent = 1;
                newVitState.percent = (int) newVitState.percent;
                vitStates.Add(newVitState);
            }

            return vitStates;
        }

        public static void CalculateItemStats()
        {
            tempStatList.Clear();
            List<VitalityStateBeforeChangesDATA> vitStates = getVitStates();
            ResetItemStats();

            foreach (var statData in CombatManager.playerCombatNode.nodeStats)
            {
                List<InventoryManager.INVENTORY_EQUIPPED_ITEMS> allEquippedItems = getAllEquippedItems();
                foreach (var t in allEquippedItems)
                {
                    foreach (var t1 in t.itemEquipped.stats)
                    {
                        if (statData.stat.ID != t1.statID) continue;
                        HandleStat(CombatManager.playerCombatNode, statData.stat, statData, t1.amount, t1.isPercent,
                            TemporaryStatSourceType.item);
                    }

                    CharacterData.ItemDATA thisItemData =
                        RPGBuilderUtilities.GetItemDataFromDataID(t.temporaryItemDataID);
                    if (thisItemData.rdmItemID != -1)
                    {
                        int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(thisItemData.rdmItemID);
                        foreach (var v in t.itemEquipped.randomStats)
                        {
                            foreach (var u in CharacterData.Instance.allRandomizedItems[rdmItemIndex]
                                .randomStats)
                            {
                                if (statData.stat.ID != v.statID) continue;
                                if (v.statID != u.statID) continue;
                                HandleStat(CombatManager.playerCombatNode, statData.stat, statData, u.statValue,
                                    v.isPercent,
                                    TemporaryStatSourceType.item);
                            }
                        }
                    }

                    if (thisItemData.enchantmentID != -1)
                    {
                        RPGEnchantment enchantmentREF =
                            RPGBuilderUtilities.GetEnchantmentFromID(thisItemData.enchantmentID);
                        if (enchantmentREF == null) continue;
                        foreach (var v in enchantmentREF.enchantmentTiers[thisItemData.enchantmentTierIndex].stats)
                        {
                            if (statData.stat.ID != v.statID) continue;
                            HandleStat(CombatManager.playerCombatNode, statData.stat, statData, v.amount, v.isPercent,
                                TemporaryStatSourceType.item);
                        }
                    }

                    foreach (var socket in thisItemData.sockets)
                    {
                        if (socket.gemItemID == -1) continue;
                        RPGItem gemItemREF = RPGBuilderUtilities.GetItemFromID(socket.gemItemID);
                        if (gemItemREF == null) continue;
                        foreach (var v in gemItemREF.gemData.gemStats)
                        {
                            if (statData.stat.ID != v.statID) continue;
                            HandleStat(CombatManager.playerCombatNode, statData.stat, statData, v.amount, v.isPercent,
                                TemporaryStatSourceType.item);
                        }
                    }
                }

                List<TemporaryActiveGearSetsDATA> activeGearSets = getActiveGearSets(allEquippedItems);

                foreach (var c in activeGearSets)
                {
                    for (int curTierIndex = c.activeTierIndex; curTierIndex > -1; curTierIndex--)
                    {
                        foreach (var t in c.gearSet.gearSetTiers[curTierIndex].gearSetTierStats)
                        {
                            if (statData.stat.ID != t.statID) continue;
                            HandleStat(CombatManager.playerCombatNode, statData.stat, statData, t.amount, t.isPercent,
                                TemporaryStatSourceType.item);
                        }
                    }
                }
            }

            ProcessTempStatList(TemporaryStatSourceType.item, CombatManager.playerCombatNode);

            foreach (var vitState in vitStates)
            {
                RPGStat statREF = RPGBuilderUtilities.GetStatFromID(vitState.statID);
                float newValue = CombatManager.playerCombatNode.getCurrentMaxValue(statREF._name);
                newValue = (newValue / 100f) * vitState.percent;
                if (statREF.ID == RPGBuilderEssentials.Instance.combatSettings.healthStatID && newValue < 1)
                {
                    newValue = 1;
                }
                CombatManager.playerCombatNode.setCurrentValue(statREF._name, (int) newValue);
                
                ClampStat(statREF, CombatManager.playerCombatNode);
                CombatManager.Instance.StatBarUpdate(statREF._name);
            }
            
            UpdateStatUI();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
        }

        public static void CalculateBonusStats()
        {
            tempStatList.Clear();
            ResetBonusStats();

            foreach (var t in CharacterData.Instance.bonusesData)
            {
                if (!t.known) continue;
                if (!t.On) continue;

                RPGBonus bonusREF = RPGBuilderUtilities.GetBonusFromID(t.ID);

                foreach (var t1 in bonusREF.ranks[RPGBuilderUtilities.getBonusRank(t.ID)].statEffectsData)
                {
                    var statREF = RPGBuilderUtilities.GetStatFromID(t1.statID);
                    foreach (var t3 in CombatManager.playerCombatNode.nodeStats)
                    {
                        if (t3.stat._name != statREF._name) continue;
                        HandleStat(CombatManager.playerCombatNode, statREF, t3, t1.statEffectModification, t1.isPercent,
                            TemporaryStatSourceType.bonus);
                    }
                }
            }

            ProcessTempStatList(TemporaryStatSourceType.bonus, CombatManager.playerCombatNode);

            UpdateStatUI();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
        }

        public static void CalculateEffectsStats(CombatNode cbtNode)
        {
            tempStatList.Clear();
            ResetEffectsStats(cbtNode);

            foreach (var t in cbtNode.nodeStateData)
            {
                if (t.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Stat) continue;
                foreach (var t1 in t.stateEffect.ranks[t.effectRank].statEffectsData)
                {
                    var statREF = RPGBuilderUtilities.GetStatFromID(t1.statID);
                    foreach (var t3 in cbtNode.nodeStats)
                    {
                        if (t3.stat._name != statREF._name) continue;
                        HandleStat(cbtNode, statREF, t3, t1.statEffectModification * t.curStack, t1.isPercent,  TemporaryStatSourceType.effect);
                    }
                }
            }

            ProcessTempStatList(TemporaryStatSourceType.effect, cbtNode);
            
            if(cbtNode == CombatManager.Instance.PlayerTargetData.currentTarget) TargetInfoDisplayManager.Instance.UpdateBars();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
            UpdateStatUI();
        }
        
        public static void CalculateShapeshiftingStats(CombatNode cbtNode, List<RPGEffect.STAT_EFFECTS_DATA> stats)
        {
            tempStatList.Clear();
            ResetShapeshiftingStats(cbtNode);

                foreach (var t1 in stats)
                {
                    var statREF = RPGBuilderUtilities.GetStatFromID(t1.statID);
                    foreach (var t3 in cbtNode.nodeStats)
                    {
                        if (t3.stat._name != statREF._name) continue;
                        HandleStat(cbtNode, statREF, t3, t1.statEffectModification, t1.isPercent,  TemporaryStatSourceType.shapeshifting);
                    }
                }

            ProcessTempStatList(TemporaryStatSourceType.shapeshifting, cbtNode);
            
            if(cbtNode == CombatManager.Instance.PlayerTargetData.currentTarget) TargetInfoDisplayManager.Instance.UpdateBars();
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
            UpdateStatUI();
        }

        private static void ProcessTempStatList(TemporaryStatSourceType sourceType, CombatNode cbtNode)
        {
            foreach (var t in tempStatList)
            {
                foreach (var t2 in cbtNode.nodeStats)
                {
                    if (t2._name != t.stat._name) continue;
                    float addedValue;

                    if (t.stat.isVitalityStat)
                    {
                        addedValue = t2.curMaxValue * (t.value / 100);
                        
                        float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                            RPGGameModifier.CombatModuleType.Stat + "+" +
                            RPGGameModifier.StatModifierType.MaxOverride, t2.stat.ID);
                        if (statOverride != -1)
                        {
                            t2.curMaxValue = statOverride;
                        }
                        else
                        {
                            t2.curMaxValue += addedValue;
                        }
                    }
                    else
                    {
                        addedValue = t2.curValue * (t.value / 100);
                        t2.curValue += addedValue;
                    }

                    switch (sourceType)
                    {
                        case TemporaryStatSourceType.item:
                            t2.valueFromItem += addedValue;
                            break;
                        case TemporaryStatSourceType.effect:
                            t2.valueFromEffect += addedValue;
                            break;
                        case TemporaryStatSourceType.bonus:
                            t2.valueFromBonus += addedValue;
                            break;
                        case TemporaryStatSourceType.shapeshifting:
                            t2.valueFromShapeshifting += addedValue;
                            break;
                    }

                    ClampStat(t.stat, cbtNode);
                    if (cbtNode == CombatManager.playerCombatNode) CombatManager.Instance.StatBarUpdate(t2._name);

                    if (cbtNode == CombatManager.playerCombatNode && RPGBuilderUtilities.isStatAffectingMoveSpeed(t2.stat))
                        TriggerMoveSpeedChange();
                }
            }
        }

        private static List<TemporaryActiveGearSetsDATA> getActiveGearSets(List<InventoryManager.INVENTORY_EQUIPPED_ITEMS> allEquippedItems)
        {
            List<TemporaryActiveGearSetsDATA> activeGearSets = new List<TemporaryActiveGearSetsDATA>();
            foreach (var t in allEquippedItems)
            {
                if (t.itemEquipped == null || !RPGBuilderUtilities.isItemPartOfGearSet(t.itemEquipped.ID)) continue;
                TemporaryActiveGearSetsDATA newSetData = RPGBuilderUtilities.getGearSetState(t.itemEquipped.ID);
                if (!RPGBuilderUtilities.containsGearSet(newSetData.gearSet, activeGearSets) && newSetData.activeTierIndex != -1)
                    activeGearSets.Add(newSetData);
            }

            return activeGearSets;
        }

        public static void ClampStat(RPGStat stat, CombatNode cbtNode)
        {
            CombatNode.NODE_STATS nodeStat = cbtNode.nodeStats[cbtNode.getStatIndexFromName(stat._name)];
            if (stat.minCheck && nodeStat.curValue < getMinValue(nodeStat))
            {
                nodeStat.curValue = (int)nodeStat.curMinValue;
            }
            if (stat.maxCheck && nodeStat.curValue > getMaxValue(nodeStat))
            {
                nodeStat.curValue = (int)nodeStat.curMaxValue;
            }
        }

        private static float getMinValue(CombatNode.NODE_STATS nodeStat)
        {
            float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.Stat + "+" +
                RPGGameModifier.StatModifierType.MinOverride, nodeStat.stat.ID);
            return statOverride != -1 ? statOverride : nodeStat.curMinValue;
        }

        private static float getMaxValue(CombatNode.NODE_STATS nodeStat)
        {
            float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.Stat + "+" +
                RPGGameModifier.StatModifierType.MaxOverride, nodeStat.stat.ID);
            return statOverride != -1 ? statOverride : nodeStat.curMaxValue;
        }
    }
}
