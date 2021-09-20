using System;
using System.Linq;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class GameModifierManager : MonoBehaviour
    {

        public static GameModifierManager Instance { get; private set; }
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }


        public class ReturnedGameModifierValueData
        {
            public bool active, isOverride;
            public float flatValue = 0, percentValue = 0;
        }
            
            
        public float GetValueAfterGameModifier(string gameModifierIdentifier, float currentValue, int entryID, int requiredEntryID)
        {
            if (!RPGBuilderEssentials.Instance.generalSettings.useGameModifiers) return currentValue;
            ReturnedGameModifierValueData newReturnedGameModifierValueData = new ReturnedGameModifierValueData();
            foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
            {
                if(!gameModifier.On) continue;
                RPGGameModifier gameModifierRef = RPGBuilderUtilities.GetGameModifierFromID(gameModifier.ID);
                foreach (var modifier in gameModifierRef.gameModifiersList)
                {
                    if(modifier.modifierTypeName != gameModifierIdentifier) continue;
                    if (!modifier.isGlobal && !modifier.entryIDs.Contains(entryID)) continue;
                    if (requiredEntryID == -1 || requiredEntryID == modifier.amountModifier.entryID)
                    {
                        newReturnedGameModifierValueData.active = true;
                        switch (modifier.amountModifier.dataModifierType)
                        {
                            case RPGGameModifier.DataModifierType.Add when modifier.amountModifier.isPercent:
                                if (!newReturnedGameModifierValueData.isOverride)
                                    newReturnedGameModifierValueData.percentValue +=
                                        modifier.amountModifier.alterAmount;
                                break;
                            case RPGGameModifier.DataModifierType.Add:
                                if (!newReturnedGameModifierValueData.isOverride)
                                    newReturnedGameModifierValueData.flatValue +=
                                        modifier.amountModifier.alterAmount;
                                break;
                            case RPGGameModifier.DataModifierType.Override:
                            {
                                if (!newReturnedGameModifierValueData.isOverride)
                                {
                                    newReturnedGameModifierValueData.flatValue = 0;
                                    newReturnedGameModifierValueData.isOverride = true;
                                }

                                if (newReturnedGameModifierValueData.flatValue < 0 &&
                                    modifier.amountModifier.alterAmount < 0)
                                {
                                    if (modifier.amountModifier.alterAmount <
                                        newReturnedGameModifierValueData.flatValue)
                                        newReturnedGameModifierValueData.flatValue =
                                            modifier.amountModifier.alterAmount;
                                }
                                else if (newReturnedGameModifierValueData.flatValue > 0 &&
                                         modifier.amountModifier.alterAmount > 0)
                                {
                                    if (modifier.amountModifier.alterAmount >
                                        newReturnedGameModifierValueData.flatValue)
                                        newReturnedGameModifierValueData.flatValue =
                                            modifier.amountModifier.alterAmount;
                                }
                                else
                                {
                                    newReturnedGameModifierValueData.flatValue =
                                        modifier.amountModifier.alterAmount;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            if (!newReturnedGameModifierValueData.active) return currentValue;
            if (newReturnedGameModifierValueData.isOverride)
            {
                currentValue = newReturnedGameModifierValueData.flatValue;
            }
            else
            {
                currentValue += newReturnedGameModifierValueData.flatValue;
                currentValue += currentValue * (newReturnedGameModifierValueData.percentValue/100f);
            }

            return currentValue;
        }
        
        public bool GetGameModifierBool(string gameModifierIdentifier, int entryID)
        {
            if (!RPGBuilderEssentials.Instance.generalSettings.useGameModifiers) return false;
            ReturnedGameModifierValueData newReturnedGameModifierValueData = new ReturnedGameModifierValueData();
            foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
            {
                if(!gameModifier.On) continue;
                RPGGameModifier gameModifierRef = RPGBuilderUtilities.GetGameModifierFromID(gameModifier.ID);
                foreach (var modifier in gameModifierRef.gameModifiersList)
                {
                    if(modifier.modifierTypeName != gameModifierIdentifier) continue;
                    if (!modifier.isGlobal && !modifier.entryIDs.Contains(entryID)) continue;
                    newReturnedGameModifierValueData.active = true;
                    return modifier.boolValue;
                }
            }

            return false;
        }
        public float GetStatOverrideModifier(string gameModifierIdentifier, int entryID)
        {
            if (!RPGBuilderEssentials.Instance.generalSettings.useGameModifiers) return -1;
            foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
            {
                if(!gameModifier.On) continue;
                RPGGameModifier gameModifierRef = RPGBuilderUtilities.GetGameModifierFromID(gameModifier.ID);
                foreach (var modifier in gameModifierRef.gameModifiersList)
                {
                    if(modifier.modifierTypeName != gameModifierIdentifier) continue;
                    if (modifier.amountModifier.entryID != entryID) continue;
                    return modifier.amountModifier.alterAmount;
                }
            }

            return -1;
        }

        public CombatNode.NODE_STATS GetStatValueAfterGameModifier(string gameModifierIdentifier, CombatNode.NODE_STATS statData, int entryID, bool isPlayer)
        {
            if (!RPGBuilderEssentials.Instance.generalSettings.useGameModifiers) return statData;

            bool isOverride = false, isActive = false;
            float addedAmountFlat = 0;
            float addedMinAmountFlat = 0;
            float addedMaxAmountFlat = 0;
            foreach (var gameModifier in CharacterData.Instance.gameModifiersData)
            {
                if (!gameModifier.On) continue;
                RPGGameModifier gameModifierRef = RPGBuilderUtilities.GetGameModifierFromID(gameModifier.ID);
                foreach (var statModifier in from modifier in gameModifierRef.gameModifiersList where modifier.modifierTypeName == gameModifierIdentifier where isPlayer || modifier.isGlobal || modifier.entryIDs.Contains(entryID) from statModifier in modifier.statModifierData where statModifier.statID == statData.stat.ID select statModifier)
                {
                    isActive = true;
                    switch (statModifier.dataModifierType)
                    {
                        case RPGGameModifier.DataModifierType.Add:
                            if (!isOverride)
                            {
                                addedAmountFlat += statModifier.valueDefault;

                                if (statModifier.checkMin)
                                {
                                    addedMinAmountFlat += statModifier.valueMin;
                                }

                                if (statModifier.checkMax)
                                {
                                    addedMaxAmountFlat += statModifier.valueMax;
                                }
                            }

                            break;
                        case RPGGameModifier.DataModifierType.Override:
                        {
                            if (!isOverride)
                            {
                                addedAmountFlat = 0;
                                addedMinAmountFlat = 0;
                                addedMaxAmountFlat = 0;
                                isOverride = true;
                            }

                            if (addedAmountFlat < 0 && statModifier.valueDefault < 0)
                            {
                                if (statModifier.valueDefault < addedAmountFlat)
                                    addedAmountFlat = statModifier.valueDefault;
                            }
                            else if (addedAmountFlat > 0 && statModifier.valueDefault > 0)
                            {
                                if (statModifier.valueDefault > addedAmountFlat)
                                    addedAmountFlat = statModifier.valueDefault;
                            }
                            else
                            {
                                addedAmountFlat = statModifier.valueDefault;
                            }

                            if (statModifier.checkMin)
                            {
                                if (addedMinAmountFlat < 0 && statModifier.valueMin < 0)
                                {
                                    if (statModifier.valueMin < addedMinAmountFlat)
                                        addedMinAmountFlat = statModifier.valueMin;
                                }
                                else if (addedMinAmountFlat > 0 && statModifier.valueMin > 0)
                                {
                                    if (statModifier.valueMin > addedMinAmountFlat)
                                        addedMinAmountFlat = statModifier.valueMin;
                                }
                                else
                                {
                                    addedMinAmountFlat = statModifier.valueMin;
                                }
                            }

                            if (statModifier.checkMax)
                            {
                                if (addedMaxAmountFlat < 0 && statModifier.valueMax < 0)
                                {
                                    if (statModifier.valueMax < addedMaxAmountFlat)
                                        addedMaxAmountFlat = statModifier.valueMax;
                                }
                                else if (addedMaxAmountFlat > 0 && statModifier.valueMax > 0)
                                {
                                    if (statModifier.valueMax > addedMaxAmountFlat)
                                        addedMaxAmountFlat = statModifier.valueMax;
                                }
                                else
                                {
                                    addedMaxAmountFlat = statModifier.valueMax;
                                }
                            }

                            break;
                        }
                    }
                }
            }

            if (!isActive) return statData;
            if (isOverride)
            {
                statData.curValue = addedAmountFlat;
                statData.curMinValue = addedMinAmountFlat;
                statData.curMaxValue = addedMaxAmountFlat;
            }
            else
            {
                statData.curValue += addedAmountFlat;
                statData.curMinValue += addedMinAmountFlat;
                statData.curMaxValue += addedMaxAmountFlat;
            }

            return statData;
        }
    }
}
