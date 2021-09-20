using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class FactionManager : MonoBehaviour
    {
        void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static FactionManager Instance { get; private set; }

        public RPGCombatDATA.ALIGNMENT_TYPE GetCombatNodeAlignment(CombatNode localNode, CombatNode otherNode)
        {
            RPGFaction localFaction;
            RPGFaction otherFaction;

            localFaction = RPGBuilderUtilities.GetFactionFromID(localNode == CombatManager.playerCombatNode
                ? RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).factionID
                : localNode.npcDATA.factionID);

            if (localNode == CombatManager.playerCombatNode)
            {
                if (otherNode == CombatManager.playerCombatNode)
                {
                    return RPGCombatDATA.ALIGNMENT_TYPE.ALLY;
                }

                otherFaction = RPGBuilderUtilities.GetFactionFromID(otherNode.npcDATA.factionID);

                return getConditionalAlignment(otherFaction, getCurrentPlayerStance(otherFaction));

            }
            else
            {
                if (otherNode == CombatManager.playerCombatNode)
                {
                    return getConditionalAlignment(localFaction, getCurrentPlayerStance(localFaction));
                }

                otherFaction = RPGBuilderUtilities.GetFactionFromID(otherNode.npcDATA.factionID);

                return getConditionalAlignment(otherFaction, getDefaultNPCStance(localFaction, otherFaction));
            }
        }

        public class CanHitResult
        {
            public bool canHit;
            public string errorMessage;
        }

        public CanHitResult AttackerCanHitTarget(RPGAbility.RPGAbilityRankData rankREF, CombatNode attackerNode,
            CombatNode targetNode)
        {
            CanHitResult hitResult = new CanHitResult();
            if (CharacterData.Instance.raceID == -1)
            {
                hitResult.canHit = false;
                return hitResult;
            }

            var attackerFaction = RPGBuilderUtilities.GetFactionFromID(attackerNode == CombatManager.playerCombatNode
                ? RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).factionID
                : attackerNode.npcDATA.factionID);

            var targetFaction = RPGBuilderUtilities.GetFactionFromID(targetNode == CombatManager.playerCombatNode
                ? RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).factionID
                : targetNode.npcDATA.factionID);

            RPGCombatDATA.ALIGNMENT_TYPE attackerAlignment = attackerNode == CombatManager.playerCombatNode
                ? RPGCombatDATA.ALIGNMENT_TYPE.ALLY
                : getNPCAlignmentToPlayer(attackerFaction);
            RPGCombatDATA.ALIGNMENT_TYPE targetAlignment = RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL;

            if (attackerNode == CombatManager.playerCombatNode)
            {
                targetAlignment = targetNode == CombatManager.playerCombatNode
                    ? RPGCombatDATA.ALIGNMENT_TYPE.ALLY
                    : getConditionalAlignment(attackerFaction, getPlayerStanceToFaction(targetFaction));
            }
            else
            {
                targetAlignment = targetNode == CombatManager.playerCombatNode
                    ? RPGCombatDATA.ALIGNMENT_TYPE.ALLY
                    : getConditionalAlignment(attackerFaction, getDefaultNPCStance(targetFaction, attackerFaction));
            }

            foreach (RPGAbility.HIT_SETTINGS t in rankREF.HitSettings)
            {
                if (t.alignment != attackerAlignment) continue;
                if (targetNode == CombatManager.playerCombatNode)
                {
                    hitResult.canHit = t.hitPlayer;
                    hitResult.errorMessage = t.hitPlayer ? "" : "This ability cannot be casted on the player";
                    return hitResult;
                }

                if (attackerNode.currentPets.Contains(targetNode))
                {
                    hitResult.canHit = t.hitPet;
                    hitResult.errorMessage = t.hitPet ? "" : "This ability cannot be casted on pet";
                    return hitResult;
                }

                if (targetNode.currentPets.Contains(attackerNode))
                {
                    hitResult.canHit = t.hitOwner;
                    hitResult.errorMessage = t.hitOwner ? "" : "This ability cannot be casted on the pet owner";
                    return hitResult;
                }

                if (targetNode == attackerNode)
                {
                    hitResult.canHit = t.hitSelf;
                    hitResult.errorMessage = t.hitSelf ? "" : "This ability cannot be casted on yourself";
                    return hitResult;
                }

                switch (targetAlignment)
                {
                    case RPGCombatDATA.ALIGNMENT_TYPE.ALLY:
                        hitResult.canHit = t.hitAlly;
                        hitResult.errorMessage = t.hitAlly ? "" : "This ability cannot be casted on allied units";
                        break;
                    case RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                        hitResult.canHit = t.hitEnemy;
                        hitResult.errorMessage = t.hitEnemy ? "" : "This ability cannot be casted on enemy units";
                        break;
                    case RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                        hitResult.canHit = t.hitNeutral;
                        hitResult.errorMessage = t.hitNeutral ? "" : "This ability cannot be casted on neutral units";
                        break;
                }
                return hitResult;
            }

            hitResult.canHit = false;
            hitResult.errorMessage = "This ability cannot be casted";
            return hitResult;
        }

        public RPGCombatDATA.ALIGNMENT_TYPE getNPCAlignmentToPlayer(RPGFaction npcFaction)
        {
            return getConditionalAlignment(RPGBuilderUtilities.GetFactionFromID(RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).factionID),
                getCurrentPlayerStance(npcFaction));
        }

        public RPGCombatDATA.ALIGNMENT_TYPE getConditionalAlignment(RPGFaction otherFaction, string currentStance)
        {
            foreach (var stance in otherFaction.factionStances)
            {
                if(stance.stance != currentStance) continue;
                return stance.playerAlignment;
            }

            return RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL;
        }
        

        public string getCurrentPlayerStance(RPGFaction factionREF)
        {
            foreach (var faction in CharacterData.Instance.factionsData)
            {
                if(faction.ID != factionREF.ID) continue;
                return faction.currentStance;
            }

            return "";
        }
        public string getDefaultNPCStance(RPGFaction localFaction, RPGFaction otherFaction)
        {
            foreach (var interaction in localFaction.factionInteractions)
            {
                if(interaction.factionID != otherFaction.ID) continue;
                return interaction.defaultStance;
            }

            return "";
        }
        public string getPlayerStanceToFaction(RPGFaction otherFaction)
        {
            foreach (var faction in CharacterData.Instance.factionsData)
            {
                if(faction.ID != otherFaction.ID) continue;
                return faction.currentStance;
            }

            return "";
        }

        public RPGCombatDATA.ALIGNMENT_TYPE GetAlignmentForPlayer(int factionID)
        {
            RPGFaction thisNodeFaction = RPGBuilderUtilities.GetFactionFromID(factionID);
            return getConditionalAlignment(thisNodeFaction, getCurrentPlayerStance(thisNodeFaction));
        }


        public void RemoveFactionPoint(int factionID, int amount)
        {
            foreach (var faction in CharacterData.Instance.factionsData)
            {
                if (faction.ID != factionID) continue;
                RPGFaction factionREF = RPGBuilderUtilities.GetFactionFromID(factionID);
                ScreenTextDisplayManager.Instance.ScreenEventHandler("FACTION_POINT", "-" + amount + " " + factionREF.displayName + " points", "", CombatManager.playerCombatNode.gameObject);
                if (faction.currentPoint >= amount)
                {
                    faction.currentPoint -= amount;
                }
                else
                {
                    if (FactionHasLowerStance(factionREF, faction.currentStance))
                    {
                        int amountToRemove = amount;
                        while (amountToRemove > 0)
                        {
                            int currentTempStanceIndex = GetCurrentStanceIndex(factionREF, faction.currentStance);

                            var pointsRemaining = faction.currentPoint;

                            if (amountToRemove > pointsRemaining)
                            {
                                faction.currentStance = factionREF.factionStances[currentTempStanceIndex - 1].stance;
                                faction.currentPoint =
                                    factionREF.factionStances[currentTempStanceIndex - 1].pointsRequired - 1;
                                amountToRemove -= pointsRemaining + 1;
                                CharacterEventsManager.Instance.FactionStanceChange(factionREF);
                            }
                            else
                            {
                                faction.currentPoint -= amountToRemove;
                                amountToRemove = 0;
                            }
                        }
                    }
                    else
                    {
                        faction.currentPoint = 0;
                    }
                }
                
                if (CharacterPanelDisplayManager.Instance.thisCG.alpha == 1 &&
                    CharacterPanelDisplayManager.Instance.curCharInfoType ==
                    CharacterPanelDisplayManager.characterInfoTypes.factions)
                {
                    CharacterPanelDisplayManager.Instance.InitCharFactions();
                }
            }
        }

        public void AddFactionPoint(int factionID, int amount)
        {
            foreach (var faction in CharacterData.Instance.factionsData)
            {
                if (faction.ID != factionID) continue;
                RPGFaction factionREF = RPGBuilderUtilities.GetFactionFromID(factionID);
                ScreenTextDisplayManager.Instance.ScreenEventHandler("FACTION_POINT", "+" + amount + " " + factionREF.displayName + " points", "",CombatManager.playerCombatNode.gameObject);
                int currentStanceIndex = GetCurrentStanceIndex(factionREF, faction.currentStance);
                if (factionREF.factionStances[currentStanceIndex].pointsRequired - faction.currentPoint > amount)
                {
                    faction.currentPoint += amount;
                }
                else
                {
                    if (FactionHasHigherStance(factionREF, faction.currentStance))
                    {
                        int amountToAdd = amount;
                        while (amountToAdd > 0)
                        {
                            int currentTempStanceIndex = GetCurrentStanceIndex(factionREF, faction.currentStance);

                            var pointsRemaining = factionREF.factionStances[currentTempStanceIndex].pointsRequired -
                                                  faction.currentPoint;

                            if (amountToAdd >= pointsRemaining)
                            {
                                faction.currentPoint = 0;
                                faction.currentStance = factionREF.factionStances[currentTempStanceIndex + 1].stance;
                                amountToAdd -= pointsRemaining;
                                CharacterEventsManager.Instance.FactionStanceChange(factionREF);
                            }
                            else
                            {
                                faction.currentPoint += amountToAdd;
                                amountToAdd = 0;
                            }
                        }
                    }
                    else
                    {
                        int currentTempStanceIndex = GetCurrentStanceIndex(factionREF, faction.currentStance);
                        faction.currentPoint = factionREF.factionStances[currentTempStanceIndex].pointsRequired;
                    }
                }

                if (CharacterPanelDisplayManager.Instance.thisCG.alpha == 1 &&
                    CharacterPanelDisplayManager.Instance.curCharInfoType ==
                    CharacterPanelDisplayManager.characterInfoTypes.factions)
                {
                    CharacterPanelDisplayManager.Instance.InitCharFactions();
                }
            }
        }

        public int GetCurrentStanceIndex(RPGFaction factionREF, string stanceName)
        {
            for (var index = 0; index < factionREF.factionStances.Count; index++)
            {
                if (factionREF.factionStances[index].stance == stanceName)
                {
                    return index;
                }
            }

            return -1;
        }
        private bool FactionHasLowerStance(RPGFaction factionREF, string currentStance)
        {
            int stanceIndex = -1;
            for (var index = 0; index < factionREF.factionStances.Count; index++)
            {
                if (factionREF.factionStances[index].stance == currentStance) stanceIndex = index;
            }

            return stanceIndex > 0;
        }
        private bool FactionHasHigherStance(RPGFaction factionREF, string currentStance)
        {
            int stanceIndex = -1;
            for (var index = 0; index < factionREF.factionStances.Count; index++)
            {
                if (factionREF.factionStances[index].stance == currentStance) stanceIndex = index;
            }

            return stanceIndex < (factionREF.factionStances.Count-1);
        }
        
        public void GenerateMobFactionReward(RPGNpc npcDATA)
        {
            foreach (var reward in npcDATA.factionRewards)
            {
                if(reward.factionID == -1) continue;
                if(reward.amount == 0) continue;

                int amount = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.NPC + "+" +
                    RPGGameModifier.NPCModifierType.Faction_Reward,
                    reward.amount, npcDATA.ID, -1);
                if (amount > 0)
                {
                    AddFactionPoint(reward.factionID, amount);
                }
                else
                {
                    RemoveFactionPoint(reward.factionID, Mathf.Abs(amount));
                }
            }
        }
    }
}
