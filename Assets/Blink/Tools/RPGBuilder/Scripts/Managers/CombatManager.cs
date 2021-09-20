using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.World;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class CombatManager : MonoBehaviour
    {
        public bool combatEnabled = true;
        public bool inCombatOverriden = false;
        
        public LayerMask ProjectileCheckLayer;
        public LayerMask ProjectileDestroyLayer;
        public LayerMask LeapInterruptLayers;
        public List<CombatNode> allCombatNodes = new List<CombatNode>();
        public List<NPCSpawner> allNPCSpawners = new List<NPCSpawner>();
        public static GroundIndicatorManager groundIndicatorManager;
        public static CombatNode playerCombatNode;
        public bool playerIsGroundCasting;
        public float NPC_GCD_DURATION;
        private StatBarDisplayHandler[] allStatsBar;
        private EXPBarDisplayHandler[] allExpBar;
        public List<GraveyardHandler> allGraveyards = new List<GraveyardHandler>();
        public CanvasGroup deathPopupCG;

        public float currentGCD;
        public class PlayerTargetDATA
        {
            public CombatNode currentTarget;
            public bool isAttacking;
        }

        public PlayerTargetDATA PlayerTargetData = new PlayerTargetDATA();
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;

            allStatsBar = FindObjectsOfType<StatBarDisplayHandler>();
            allExpBar = FindObjectsOfType<EXPBarDisplayHandler>();
        }

        public void HandleTurnOffCombat()
        {
            foreach (var t in allCombatNodes)
            {
                if ((t.nodeType == CombatNode.COMBAT_NODE_TYPE.pet || t.nodeType == CombatNode.COMBAT_NODE_TYPE.mob) &&
                    t.agentREF != null)
                {
                    t.agentREF.ClearThreatTable();
                    for (var x = 0; x < t.nodeStateData.Count; x++)
                    {
                        t.CancelState(x);
                    }
                }
                else
                {
                    
                }
            }
        }

        public void RemoveCombatNodeFromList(CombatNode cbtNode)
        {
            if (allCombatNodes.Contains(cbtNode))
            {
                allCombatNodes.Remove(cbtNode);
            }

            switch (cbtNode.nodeType)
            {
                case CombatNode.COMBAT_NODE_TYPE.mob:
                case CombatNode.COMBAT_NODE_TYPE.pet:
                    foreach (var npcSpawner in allNPCSpawners.Where(npcSpawner => npcSpawner.curNPCs.Contains(cbtNode)))
                    {
                        npcSpawner.curNPCs.Remove(cbtNode);
                    }
                    break;
            }
        }

        public void RemoveSpawnerFromList(NPCSpawner spawner)
        {
            foreach (var npc in spawner.curNPCs)
            {
                Destroy(npc.gameObject);
            }
            if (allNPCSpawners.Contains(spawner)) allNPCSpawners.Remove(spawner);
        }

        private void RemoveCombatNodeFromThreatTables(CombatNode cbtNode)
        {
            foreach (var t in allCombatNodes)
                if (t != cbtNode &&
                    (t.nodeType == CombatNode.COMBAT_NODE_TYPE.pet || t.nodeType == CombatNode.COMBAT_NODE_TYPE.mob)
                    && t.agentREF != null)
                    t.agentREF.RemoveCombatNodeFromThreatTabble(cbtNode);
        }

        private void DestroyDeadNodeCombatEntities(CombatNode cbtNode)
        {
            foreach (var t in cbtNode.ownedCombatVisuals)
            {
                if (cbtNode.ownedLogicCombatVisuals.Contains(t)) cbtNode.ownedLogicCombatVisuals.Remove(t);
                if (cbtNode.destroyedOnStealthCombatVisuals.Contains(t)) cbtNode.destroyedOnStealthCombatVisuals.Remove(t);
                if (cbtNode.destroyedOnStealthEndCombatVisuals.Contains(t)) cbtNode.destroyedOnStealthEndCombatVisuals.Remove(t);
                Destroy(t.gameObject);
            }
        }
        
        private void DestroyStunnedNodeCombatEntities(CombatNode cbtNode)
        {
            foreach (var t in cbtNode.ownedLogicCombatVisuals)
            {
                if (cbtNode.ownedCombatVisuals.Contains(t)) cbtNode.ownedCombatVisuals.Remove(t);
                if (cbtNode.destroyedOnStealthCombatVisuals.Contains(t)) cbtNode.destroyedOnStealthCombatVisuals.Remove(t);
                if (cbtNode.destroyedOnStealthEndCombatVisuals.Contains(t)) cbtNode.destroyedOnStealthEndCombatVisuals.Remove(t);
                Destroy(t.gameObject);
            }
        }
        
        private void DestroyStealthNodeCombatEntities(CombatNode cbtNode)
        {
            foreach (var t in cbtNode.destroyedOnStealthCombatVisuals)
            {
                if (cbtNode.ownedCombatVisuals.Contains(t)) cbtNode.ownedCombatVisuals.Remove(t);
                if (cbtNode.ownedLogicCombatVisuals.Contains(t)) cbtNode.ownedLogicCombatVisuals.Remove(t);
                if (cbtNode.destroyedOnStealthEndCombatVisuals.Contains(t)) cbtNode.destroyedOnStealthEndCombatVisuals.Remove(t);
                Destroy(t.gameObject);
            }
        }
        
        public void DestroyStealthEndNodeCombatEntities(CombatNode cbtNode)
        {
            foreach (var t in cbtNode.destroyedOnStealthEndCombatVisuals)
            {
                if (cbtNode.ownedCombatVisuals.Contains(t)) cbtNode.ownedCombatVisuals.Remove(t);
                if (cbtNode.ownedLogicCombatVisuals.Contains(t)) cbtNode.ownedLogicCombatVisuals.Remove(t);
                if (cbtNode.destroyedOnStealthCombatVisuals.Contains(t)) cbtNode.destroyedOnStealthCombatVisuals.Remove(t);
                Destroy(t.gameObject);
            }
        }

        public void HandleCombatNodeDEATH(CombatNode cbtNode)
        {
            RemoveCombatNodeFromThreatTables(cbtNode);
            cbtNode.InterruptCastActions(false);
            DestroyDeadNodeCombatEntities(cbtNode);

            if (cbtNode == BossUISlotHolder.Instance.thisNode)
            {
                BossUISlotHolder.Instance.ResetBossUI();
            }

            // CLEARING ALL STATES COMING FROM THE DEAD NODE ON OTHER NODES
            foreach (var t in allCombatNodes)
            {
                for (var x = 0; x < t.nodeStateData.Count; x++)
                {
                    if (t.nodeStateData[x].casterNode == cbtNode)
                    {
                        t.CancelState(x);
                    }
                }
            }

            // CLEARING ALL STATES FOR THE DEAD NODE
            for (var x = 0; x < cbtNode.nodeStateData.Count; x++)
            {
                cbtNode.CancelState(x);
            }

            if (cbtNode == PlayerTargetData.currentTarget)
            {
                TargetInfoDisplayManager.Instance.ResetTarget();
            }

            switch (cbtNode.nodeType)
            {
                case CombatNode.COMBAT_NODE_TYPE.mob:
                case CombatNode.COMBAT_NODE_TYPE.pet:
                {
                    if (cbtNode.agentREF.thisAnim != null &&
                        cbtNode.agentREF.thisAnim.runtimeAnimatorController != null)
                    {
                        cbtNode.agentREF.thisAnim.Rebind();
                        if(cbtNode.agentREF.thisAnim.GetBool("Stunned")) cbtNode.agentREF.thisAnim.SetBool("Stunned", false);
                        cbtNode.agentREF.thisAnim.SetTrigger("Death");
                    }
                    Destroy(cbtNode.agentREF.thisAgent);
                    Destroy(cbtNode.agentREF);
                    if (cbtNode.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                        !playerCombatNode.currentPets.Contains(cbtNode))
                    {
                        if(shouldRewardPlayer(cbtNode))
                        {
                            LevelingManager.Instance.GenerateMobEXP(cbtNode.npcDATA, cbtNode);
                            InventoryManager.Instance.GenerateDroppedLoot(cbtNode.npcDATA, cbtNode);
                            CharacterEventsManager.Instance.NPCKilled(cbtNode.npcDATA);
                            FactionManager.Instance.GenerateMobFactionReward(cbtNode.npcDATA);
                        }
                    }

                    if (cbtNode.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                    {
                        cbtNode.ownerCombatInfo.currentPets.Remove(cbtNode);
                        if (cbtNode.ownerCombatInfo == playerCombatNode)
                        {
                            if (cbtNode.ownerCombatInfo.currentPets.Count == 0)
                            {
                                PetPanelDisplayManager.Instance.Hide();
                            }
                            else
                            {
                                PetPanelDisplayManager.Instance.UpdateSummonCountText();
                                PetPanelDisplayManager.Instance.UpdateHealthBar();
                            }
                        }
                    }
                    
                    foreach (var t in cbtNode.currentPets)
                    {
                        RemoveCombatNodeFromList(t);
                        Destroy(t.gameObject);
                    }

                    RemoveCombatNodeFromList(cbtNode);

                    Destroy(cbtNode.GetComponent<Collider>());
                    
                    Destroy(cbtNode.gameObject, cbtNode.npcDATA.corpseDespawnTime);
                    break;
                }
                case CombatNode.COMBAT_NODE_TYPE.player:
                {
                    CharacterData.Instance.position = getClosestGraveyardPosition();
                    
                    if (RPGBCharacterControllerEssentials.IsUsingBuiltInControllers())
                    {
                        if(playerCombatNode.playerControllerEssentials.builtInController.CurrentController == RPGBCharacterController.ControllerType.ThirdPerson && !playerCombatNode.playerControllerEssentials.builtInController.ClickToRotate)
                        {
                            playerCombatNode.playerControllerEssentials.builtInController.ClickToRotate = true;
                        }
                    }
                    
                    for (var i = 0; i < cbtNode.currentPets.Count; i++)
                    {
                        RemoveCombatNodeFromList(cbtNode.currentPets[i]);
                        Destroy(cbtNode.currentPets[i].gameObject);

                        cbtNode.currentPets.Remove(cbtNode.currentPets[i]);
                        if (cbtNode.currentPets.Count == 0)
                        {
                            PetPanelDisplayManager.Instance.Hide();
                        }
                        else
                        {
                            PetPanelDisplayManager.Instance.UpdateSummonCountText();
                            PetPanelDisplayManager.Instance.UpdateHealthBar();
                        }
                    }

                    if(LootPanelDisplayManager.Instance.thisCG.alpha == 1)LootPanelDisplayManager.Instance.Hide();
                    if(MerchantPanelDisplayManager.Instance.thisCG.alpha == 1)MerchantPanelDisplayManager.Instance.Hide();
                    if(QuestInteractionDisplayManager.Instance.thisCG.alpha == 1)QuestInteractionDisplayManager.Instance.Hide();
                    
                    RPGBuilderUtilities.EnableCG(deathPopupCG);
                    
                    playerCombatNode.playerControllerEssentials.InitDeath();
                    ResetPlayerTarget();
                    break;
                }
                case CombatNode.COMBAT_NODE_TYPE.objectAction:
                {
                    ScreenSpaceNameplates.Instance.ResetThisNP(cbtNode);
                    Animator anim = cbtNode.GetComponent<Animator>();
                    if (anim != null) anim.SetTrigger("Death");
                    
                    if(shouldRewardPlayer(cbtNode))
                    {
                        LevelingManager.Instance.GenerateMobEXP(cbtNode.npcDATA, cbtNode);
                        InventoryManager.Instance.GenerateDroppedLoot(cbtNode.npcDATA, cbtNode);
                        CharacterEventsManager.Instance.NPCKilled(cbtNode.npcDATA);
                        FactionManager.Instance.GenerateMobFactionReward(cbtNode.npcDATA);
                    }

                    RemoveCombatNodeFromList(cbtNode);

                    Destroy(cbtNode.GetComponent<Collider>());
                    
                    Destroy(cbtNode.gameObject, cbtNode.npcDATA.corpseDespawnTime);
                    break;
                }
            }
        }


        bool shouldRewardPlayer(CombatNode deadUnit)
        {
            if (deadUnit.agentREF == null) return false;
            foreach (var t in deadUnit.agentREF.threatTable)
            {
                if (t.combatNode == playerCombatNode || isPlayerPetNode(t.combatNode))
                {
                    return true;
                }
            }
            return false;
        }

        bool isPlayerPetNode(CombatNode potentialNode)
        {
            foreach (var t in playerCombatNode.currentPets)
            {
                if (t == potentialNode)
                {
                    return true;
                }
            }

            return false;
        }


        public void SetPlayerTarget(CombatNode cbtNode, bool isAttacking)
        {
            if (cbtNode == null) return;
            if (Cursor.lockState == CursorLockMode.Locked && cbtNode == playerCombatNode) return;
            PlayerTargetData.currentTarget = cbtNode;
            PlayerTargetData.isAttacking = isAttacking;
            TargetInfoDisplayManager.Instance.InitTargetUI(PlayerTargetData.currentTarget);
            ScreenSpaceNameplates.Instance.SetNPToFocused(PlayerTargetData.currentTarget);
        }

        public void ResetPlayerTarget()
        {
            PlayerTargetData.currentTarget = null;
            TargetInfoDisplayManager.Instance.ResetTarget();
            ScreenSpaceNameplates.Instance.SetNPToFocused(null);
        }

        public void HandleHealthStatChange(CombatNode cbtNode)
        {
            if (cbtNode == null) return;
            if (cbtNode == PlayerTargetData.currentTarget) TargetInfoDisplayManager.Instance.UpdateTargetHealthBar();

            switch (cbtNode.nodeType)
            {
                case CombatNode.COMBAT_NODE_TYPE.mob:
                case CombatNode.COMBAT_NODE_TYPE.pet:
                case CombatNode.COMBAT_NODE_TYPE.objectAction:
                    ScreenSpaceNameplates.Instance.UpdateNPBar(cbtNode);
                    if (cbtNode.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                        if (playerCombatNode.currentPets.Contains(cbtNode))
                            PetPanelDisplayManager.Instance.UpdateHealthBar();
                    if (cbtNode.npcDATA.npcType == RPGNpc.NPC_TYPE.BOSS)
                    {
                        if (BossUISlotHolder.Instance.thisNode == cbtNode)
                        {
                            BossUISlotHolder.Instance.UpdateHealth();
                        }
                    }

                    break;

                case CombatNode.COMBAT_NODE_TYPE.player:
                    StatBarUpdate(RPGBuilderEssentials.Instance.healthStatReference._name);
                    break;
            }
        }

        public void InitAllStatBar()
        {
            foreach (var t in allStatsBar)
                t.UpdateBar();

            foreach (var t in allExpBar)
                t.UpdateBar();
        }

        public void StatBarUpdate(string statName)
        {
            foreach (var t in allStatsBar)
                if (t.STAT_NAME == statName)
                    t.UpdateBar();
        }

        public void EXPBarUpdate()
        {
            foreach (var t in allExpBar)
                if (t.expBarType == EXPBarDisplayHandler.EXPBar_Type.ClassEXP)
                    t.UpdateBar();
        }

        public void EXPBarUpdate(RPGSkill _skill)
        {
            foreach (var t in allExpBar)
                if (t._skill == _skill)
                    t.UpdateBar();
        }

        public static CombatManager Instance { get; private set; }

        public bool LayerContains(LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        public List<CombatNode> getPotentialCombatNodes(Collider[] allColliders, CombatNode casterCbtInfo,
            RPGAbility.RPGAbilityRankData rankREF)
        {
            List<CombatNode> allCbtNodes = new List<CombatNode>();
            foreach (Collider t in allColliders)
            {
                CombatNode thisCbtNode = t.GetComponent<CombatNode>();
                if (thisCbtNode == null) continue;
                FactionManager.CanHitResult hitResult =
                    FactionManager.Instance.AttackerCanHitTarget(rankREF, casterCbtInfo, thisCbtNode);
                if (hitResult.canHit) allCbtNodes.Add(thisCbtNode);
            }

            return allCbtNodes;
        }


        private bool CasterCanHitPlayer(RPGAbility.RPGAbilityRankData rankREF, RPGCombatDATA.ALIGNMENT_TYPE casterAlignment)
        {
            foreach (RPGAbility.HIT_SETTINGS t in rankREF.HitSettings)
            {
                if (t.alignment == casterAlignment)
                {
                    return t.hitPlayer;
                }
            }

            return false;
        }

        private bool isGraveyardAccepted(GraveyardHandler graveyard)
        {
            if (graveyard.requiredClasses.Count <= 0)
                return graveyard.requiredRaces.Count <= 0 ||
                       graveyard.requiredRaces.Contains(
                           RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID));
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses &&  !graveyard.requiredClasses.Contains(
                RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)))
                return false;
            return graveyard.requiredRaces.Count <= 0 || graveyard.requiredRaces.Contains(RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID));
        }

        public void HandleCombatAction(CombatNode cbtNode)
        {
            if (!cbtNode.inCombat)
            {
                cbtNode.inCombat = true;
                if(cbtNode == playerCombatNode)ErrorEventsDisplayManager.Instance.ShowErrorEvent("Entered Combat", 3);
            }

            if (cbtNode == playerCombatNode)
            {
                playerCombatNode.appearanceREF.UpdateWeaponStates(cbtNode.inCombat);
                playerCombatNode.appearanceREF.HandleAnimatorOverride();
            }
            
            cbtNode.lastCombatActionTimer = 0;
        }

        public void ResetCombat(CombatNode cbtNode)
        {
            if (cbtNode == null) return;
            cbtNode.inCombat = false;
            cbtNode.lastCombatActionTimer = 0;
            if (cbtNode == playerCombatNode)
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Out of combat", 3);
                playerCombatNode.appearanceREF.UpdateWeaponStates(cbtNode.inCombat);
                playerCombatNode.appearanceREF.HandleAnimatorOverride();
            }
            else
            {
                foreach (var t in cbtNode.nodeStats.Where(t =>
                    t.stat.isVitalityStat))
                    t.curValue = (int) t.curMaxValue;
                
                HandleHealthStatChange(cbtNode);

                if(cbtNode.agentREF)cbtNode.agentREF.ClearThreatTable();

            }
        }

        public Vector3 getClosestGraveyardPosition()
        {
            Vector3 respawnPOS = Vector3.zero;
            if (allGraveyards.Count > 0)
            {
                GraveyardHandler closestGraveyard = null;
                var closestDist = Mathf.Infinity;
                foreach (var t in allGraveyards)
                    if (isGraveyardAccepted(t))
                    {
                        var dist = Vector3.Distance(playerCombatNode.transform.position, t.transform.position);
                        if (!(dist < closestDist)) continue;
                        closestDist = dist;
                        closestGraveyard = t;
                    }

                if (closestGraveyard != null)
                {
                    respawnPOS = closestGraveyard.transform.position;
                }
                else
                {
                    respawnPOS = RPGBuilderUtilities.GetWorldPositionFromID(RPGBuilderUtilities
                            .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID)
                        .position;
                }
            }
            else
            {
                respawnPOS = RPGBuilderUtilities.GetWorldPositionFromID(RPGBuilderUtilities
                        .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID)
                    .position;
            }

            return respawnPOS;
        }

        public void RespawnPlayerToGraveyard()
        {
            RPGBuilderUtilities.DisableCG(deathPopupCG);
            Vector3 respawnPOS = Vector3.zero;
            if (allGraveyards.Count > 0)
            {
                GraveyardHandler closestGraveyard = null;
                var closestDist = Mathf.Infinity;
                foreach (var t in allGraveyards)
                    if (isGraveyardAccepted(t))
                    {
                        var dist = Vector3.Distance(playerCombatNode.transform.position, t.transform.position);
                        if (!(dist < closestDist)) continue;
                        closestDist = dist;
                        closestGraveyard = t;
                    }

                if (closestGraveyard != null)
                {
                    respawnPOS = closestGraveyard.transform.position;
                }
                else
                {
                    respawnPOS = RPGBuilderUtilities.GetWorldPositionFromID(RPGBuilderUtilities
                            .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID)
                        .position;
                }
            }
            else
            {
                respawnPOS = RPGBuilderUtilities.GetWorldPositionFromID(RPGBuilderUtilities
                        .GetGameSceneFromName(RPGBuilderEssentials.Instance.getCurrentScene().name).startPositionID)
                    .position;
            }

            StatCalculator.ResetPlayerStatsAfterRespawn();
            playerCombatNode.InitRespawn(respawnPOS);
        }

        private CombatNode.NodeStatesDATA GenerateNewState(RPGEffect effect, int effectRank, CombatNode casterInfo, Sprite icon)
        {
            return new CombatNode.NodeStatesDATA
            {
                stateName = effect._name,
                casterNode = casterInfo,
                stateMaxDuration = effect.duration,
                stateCurDuration = 0,
                curStack = 1,
                maxStack = effect.stackLimit,
                stateEffect = effect,
                effectRank = effectRank,
                stateIcon = icon
            };
        }

        private bool isEffectCC(RPGEffect.EFFECT_TYPE effectType)
        {
            return effectType == RPGEffect.EFFECT_TYPE.Stun
                   || effectType == RPGEffect.EFFECT_TYPE.Root
                   || effectType == RPGEffect.EFFECT_TYPE.Silence
                   || effectType == RPGEffect.EFFECT_TYPE.Sleep
                   || effectType == RPGEffect.EFFECT_TYPE.Taunt;
        }
        private bool isEffectUnique(RPGEffect.EFFECT_TYPE effectType)
        {
            return effectType == RPGEffect.EFFECT_TYPE.Immune
                   || effectType == RPGEffect.EFFECT_TYPE.Stun
                   || effectType == RPGEffect.EFFECT_TYPE.Root
                   || effectType == RPGEffect.EFFECT_TYPE.Silence
                   || effectType == RPGEffect.EFFECT_TYPE.Sleep
                   || effectType == RPGEffect.EFFECT_TYPE.Taunt
                   || effectType == RPGEffect.EFFECT_TYPE.Shapeshifting
                   || effectType == RPGEffect.EFFECT_TYPE.Flying
                   || effectType == RPGEffect.EFFECT_TYPE.Stealth;
        }
        private bool isEffectState(RPGEffect.EFFECT_TYPE effectType)
        {
            return effectType == RPGEffect.EFFECT_TYPE.DamageOverTime
                   || effectType == RPGEffect.EFFECT_TYPE.HealOverTime
                   || effectType == RPGEffect.EFFECT_TYPE.Stat
                   || effectType == RPGEffect.EFFECT_TYPE.Shapeshifting
                   || effectType == RPGEffect.EFFECT_TYPE.Flying
                   || effectType == RPGEffect.EFFECT_TYPE.Stealth;
        }

        private void InitNewStateEffect(RPGEffect effect, int effectRank, CombatNode casterInfo, Sprite icon, CombatNode targetInfo)
        {
            var newState = GenerateNewState(effect, effectRank, casterInfo, icon);

            newState.curPulses = 0;
            newState.maxPulses = effect.pulses;
            newState.pulseInterval = effect.duration / effect.pulses;

            targetInfo.nodeStateData.Add(newState);
            
            if (targetInfo == playerCombatNode)
                PlayerStatesDisplayHandler.Instance.DisplayState(newState);
            else
                ScreenSpaceNameplates.Instance.InitNameplateState(targetInfo, newState);
        }

        private float getTotalCCDuration(CombatNode casterInfo, CombatNode targetInfo, float duration)
        {
            float CC_POWER = GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.CC_POWER);
            float CC_RES = GetTotalOfStatType(targetInfo, RPGStat.STAT_TYPE.CC_RESISTANCE);
                    
            duration += duration * (CC_POWER / 100f);
            float CCResMod = 1 - (CC_RES / 100f);
            duration *= CCResMod;
            return duration;
        }
        
        private IEnumerator InitNodeState(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank, Sprite icon, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            var hasSameUniqueEffect = false;
            var hasSameState = false;
            var curStateIndex = -1;
            var allNodeStates = targetInfo.nodeStateData;
            
            if (isEffectUnique(effect.effectType))
            {
                ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Activate, targetInfo, effect.ranks[effectRank].visualEffects);
                ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType.Activate, targetInfo, effect.ranks[effectRank].visualAnimations);
            }
            
            for (var i = 0; i < allNodeStates.Count; i++)
                if (isEffectUnique(effect.effectType) && effect.effectType == allNodeStates[i].stateEffect.effectType)
                {
                    hasSameUniqueEffect = true;
                    curStateIndex = i;
                    break;
                }

            if (!hasSameUniqueEffect)
            {
                for (var i = 0; i < allNodeStates.Count; i++)
                    if (isEffectState(effect.effectType) && effect == allNodeStates[i].stateEffect)
                    {
                        hasSameState = true;
                        curStateIndex = i;
                        break;
                    }
            }

            if (hasSameUniqueEffect)
            {
                if (effect.effectType == RPGEffect.EFFECT_TYPE.Shapeshifting)
                {
                    ResetPlayerShapeshift();
                }
                else
                {

                    if (targetInfo == playerCombatNode)
                    {
                        PlayerStatesDisplayHandler.Instance.RemoveState(curStateIndex);
                    }
                    else
                    {
                        ScreenSpaceNameplates.Instance.RemoveState(targetInfo, targetInfo.nodeStateData[curStateIndex]);
                    }

                    targetInfo.nodeStateData.RemoveAt(curStateIndex);
                }

                var newState = GenerateNewState(effect, effectRank, casterInfo, icon);

                if (isEffectCC(effect.effectType))
                {
                    newState.stateMaxDuration =
                        getTotalCCDuration(casterInfo, targetInfo, newState.stateMaxDuration);
                }

                targetInfo.nodeStateData.Add(newState);
                if (targetInfo == playerCombatNode)
                    PlayerStatesDisplayHandler.Instance.DisplayState(newState);
                else
                    ScreenSpaceNameplates.Instance.InitNameplateState(targetInfo, newState);

                
                switch (effect.effectType)
                {
                    case RPGEffect.EFFECT_TYPE.Shapeshifting:
                    {
                        if(targetInfo == playerCombatNode) ShapeshiftPlayer(effect, effectRank);
                        break;
                    }
                    case RPGEffect.EFFECT_TYPE.Flying:
                    {
                        if(targetInfo == playerCombatNode) playerCombatNode.playerControllerEssentials.InitFlying();
                        break;
                    }
                    case RPGEffect.EFFECT_TYPE.Stealth:
                    {
                        InitStealth(targetInfo, effect.ranks[effectRank].showStealthActionBar, effect.ranks[effectRank].nestedEffects);
                        break;
                    }
                }
                
                if (targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.mob &&
                    targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet ||
                    effect.effectType != RPGEffect.EFFECT_TYPE.Root &&
                    effect.effectType != RPGEffect.EFFECT_TYPE.Sleep &&
                    effect.effectType != RPGEffect.EFFECT_TYPE.Stun)
                {
                    yield break;
                }

                if (targetInfo.agentREF != null) targetInfo.agentREF.InitStun();
                HandleMobAggro(casterInfo, targetInfo);
            }
            else if (hasSameState)
            {
                if (targetInfo.nodeStateData[curStateIndex].casterNode == casterInfo ||
                    targetInfo.nodeStateData[curStateIndex].stateEffect.allowMixedCaster
                ) // same effect: from same caster || mixed caster is allowed
                {
                    if (targetInfo.nodeStateData[curStateIndex].curStack < targetInfo.nodeStateData[curStateIndex].maxStack)
                        targetInfo.nodeStateData[curStateIndex].curStack++;
                    else
                    {
                        if (effect.allowMultiple)
                        {
                            InitNewStateEffect(effect, effectRank, casterInfo, icon, targetInfo);
                            yield break;
                        }
                    }

                    // REFRESH THE EFFECT
                    targetInfo.nodeStateData[curStateIndex].curPulses = 0;
                    targetInfo.nodeStateData[curStateIndex].nextPulse = 0;
                    targetInfo.nodeStateData[curStateIndex].stateCurDuration = 0;

                    if (targetInfo == playerCombatNode)
                        PlayerStatesDisplayHandler.Instance.UpdateState(curStateIndex);
                    else
                        ScreenSpaceNameplates.Instance.UpdateNameplateState(targetInfo,
                            targetInfo.nodeStateData[curStateIndex]);
                }
                else if (targetInfo.nodeStateData[curStateIndex].stateEffect.allowMultiple)
                {
                    // caster is: not same || mixed caster is not allowed
                    // we add it as a new effect
                    InitNewStateEffect(effect, effectRank, casterInfo, icon, targetInfo);
                }
                HandleMobAggro(casterInfo, targetInfo);
            }
            else
            {
                var newState = GenerateNewState(effect, effectRank, casterInfo, icon);

                if (isEffectCC(effect.effectType))
                {
                    newState.stateMaxDuration = getTotalCCDuration(casterInfo, targetInfo, newState.stateMaxDuration);
                    if (newState.stateMaxDuration == 0) 
                        yield break;
                }

                if (effect.effectType == RPGEffect.EFFECT_TYPE.DamageOverTime ||
                    effect.effectType == RPGEffect.EFFECT_TYPE.HealOverTime ||
                    effect.effectType == RPGEffect.EFFECT_TYPE.Stat)
                {
                    newState.curPulses = 0;
                    newState.maxPulses = effect.pulses;
                    newState.pulseInterval = effect.duration / effect.pulses;
                }

                targetInfo.nodeStateData.Add(newState);
                if (targetInfo == playerCombatNode)
                    PlayerStatesDisplayHandler.Instance.DisplayState(newState);
                else
                    ScreenSpaceNameplates.Instance.InitNameplateState(targetInfo, newState);


                switch (effect.effectType)
                {
                    case RPGEffect.EFFECT_TYPE.Stun:
                        targetInfo.InterruptCastActions(true);
                        DestroyStunnedNodeCombatEntities(targetInfo);
                        break;
                    case RPGEffect.EFFECT_TYPE.Shapeshifting:
                    {
                        if(targetInfo == playerCombatNode) ShapeshiftPlayer(effect, effectRank);
                        break;
                    }
                    case RPGEffect.EFFECT_TYPE.Flying:
                    {
                        if(targetInfo == playerCombatNode) playerCombatNode.playerControllerEssentials.InitFlying();
                        break;
                    }
                    case RPGEffect.EFFECT_TYPE.Stealth:
                    {
                        InitStealth(targetInfo, effect.ranks[effectRank].showStealthActionBar, effect.ranks[effectRank].nestedEffects);
                        break;
                    }
                }

                if (targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.mob &&
                    targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet ||
                    effect.effectType != RPGEffect.EFFECT_TYPE.Root &&
                    effect.effectType != RPGEffect.EFFECT_TYPE.Sleep &&
                    effect.effectType != RPGEffect.EFFECT_TYPE.Stun)
                {
                    yield break;
                }
                if (targetInfo.agentREF != null) targetInfo.agentREF.InitStun();
                HandleMobAggro(casterInfo, targetInfo);
            }
        }


        private void HandleNestedEffects(CombatNode targetInfo, List<RPGAbility.AbilityEffectsApplied> nestedEffects)
        {
            foreach (var effectApplied in nestedEffects)
            {
                var chance = Random.Range(0, 100f);
                if (effectApplied.chance != 0 && !(chance <= effectApplied.chance)) continue;
                GameActionsManager.Instance.ApplyEffect(RPGCombatDATA.TARGET_TYPE.Caster, targetInfo, effectApplied.effectID);
            }
        }

        private void ResetNestedEffects(CombatNode targetInfo, RPGEffect parentEffect, int parentEffectRank)
        {
            List<int> effectsToRemove = new List<int>();
            foreach (var nestedEffect in parentEffect.ranks[parentEffectRank].nestedEffects)
            {
                if (RPGBuilderUtilities.GetEffectFromID(nestedEffect.effectID).endless)
                    effectsToRemove.Add(nestedEffect.effectID);
            }

            foreach (var effectToRemove in effectsToRemove)
            {
                targetInfo.RemoveEffect(effectToRemove);
            }
        }

        public float GetTotalOfStatType(CombatNode nodeInfo, RPGStat.STAT_TYPE statType)
        {
            float total = 0;
            foreach (var t in nodeInfo.nodeStats)
            {
                foreach (var t1 in t.stat.statBonuses)
                {
                    if (t1.statType == statType)
                    {
                        total += t.curValue * t1.modifyValue;
                    }
                }
            }

            return total;
        }

        public float CalculateCastTime(CombatNode cbtNode, float baseCastTime)
        {
            float curCastMod = GetTotalOfStatType(cbtNode, RPGStat.STAT_TYPE.CAST_SPEED);

            if (curCastMod == 0) return baseCastTime;
            curCastMod /= 100;
            if (curCastMod > 0)
            {
                curCastMod = 1 - curCastMod;
                if (curCastMod < 0) curCastMod = 0;
                baseCastTime *= curCastMod;
                return baseCastTime;
            }

            curCastMod = Mathf.Abs(curCastMod);
            baseCastTime += baseCastTime * curCastMod;
            return baseCastTime;

        }

        public void LeapEnded(CombatNode nodeCombatInfo)
        {
            var curRank = 0;
            if (nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                curRank = 0;
            else
                curRank = RPGBuilderUtilities.getAbilityRank(playerCombatNode.currentAbilityCasted.ID);
            var rankREF = playerCombatNode.currentAbilityCasted.ranks[curRank];

            if (rankREF.targetType != RPGAbility.TARGET_TYPES.GROUND_LEAP) return;
            if (rankREF.extraAbilityExecuted != null) InitExtraAbility(nodeCombatInfo, rankREF.extraAbilityExecuted);
        }

        private bool checkCooldown(CombatNode nodeCombatInfo, RPGAbility thisAbility)
        {
            if (nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
                return CharacterData.Instance.isAbilityCDReady(thisAbility);
            return nodeCombatInfo.getAbilityDATA(thisAbility).CDLeft == 0;
        }

        public void InitExtraAbility(CombatNode nodeCombatInfo, RPGAbility thisAbility)
        {
            var curRank = 0;
            var rankREF = thisAbility.ranks[curRank];

            if (rankREF.targetType != RPGAbility.TARGET_TYPES.GROUND &&
                rankREF.targetType != RPGAbility.TARGET_TYPES.GROUND_LEAP)
            {
                ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Activate, nodeCombatInfo, rankREF.visualEffects);
            }
            switch (rankREF.targetType)
            {
                case RPGAbility.TARGET_TYPES.AOE:
                    EXECUTE_AOE_ABILITY(nodeCombatInfo, thisAbility, rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.CONE:
                    EXECUTE_CONE_ABILITY(nodeCombatInfo, rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.PROJECTILE:
                    if (nodeCombatInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.objectAction)
                        StartCoroutine(EXECUTE_PROJECTILE_ABILITY(nodeCombatInfo, null, thisAbility, rankREF));
                    break;

                case RPGAbility.TARGET_TYPES.LINEAR:
                    EXECUTE_LINEAR_ABILITY(nodeCombatInfo, thisAbility, RPGCombatDATA.CombatVisualActivationType.Activate, rankREF);
                    break;
            }
        }

        private bool checkTarget(CombatNode casterInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.targetType != RPGAbility.TARGET_TYPES.TARGET_INSTANT &&
                rankREF.targetType != RPGAbility.TARGET_TYPES.TARGET_PROJECTILE) return true;
            CombatNode checkedTarget;
            if (casterInfo == playerCombatNode)
            {
                if (PlayerTargetData.currentTarget != null)
                {
                    checkedTarget = PlayerTargetData.currentTarget;
                }
                else
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This ability requires a target", 3);
                    return false;
                }
            }
            else
            {
                if (casterInfo.agentREF.target != null)
                    checkedTarget = casterInfo.agentREF.target;
                else
                    return false;
            }

            FactionManager.CanHitResult hitResult =
                FactionManager.Instance.AttackerCanHitTarget(rankREF, casterInfo, checkedTarget);
            if (checkedTarget != null)
            {
                if (checkedTarget == playerCombatNode && casterInfo == playerCombatNode)
                {
                    if (!hitResult.canHit)
                    {
                        return false;
                    }

                    if (!CasterCanHitPlayer(rankREF, RPGCombatDATA.ALIGNMENT_TYPE.ALLY))
                    {
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("This ability cannot be used on yourself", 3);
                        return false;
                    }
                }
                

                var dist = Vector3.Distance(casterInfo.transform.position, checkedTarget.transform.position);

                if (rankREF.mustLookAtTarget)
                {
                    var pointDirection = checkedTarget.transform.position - casterInfo.transform.position;
                    var angle = Vector3.Angle(casterInfo.transform.forward, pointDirection);
                    if (!(angle < 70))
                    {
                        if (casterInfo == playerCombatNode)ErrorEventsDisplayManager.Instance.ShowErrorEvent("The target is not in line of sight", 3);
                        return false;
                    }
                }

                float totalMinRange = rankREF.minRange + (rankREF.minRange *
                                                          (GetTotalOfStatType(casterInfo,
                                                              RPGStat.STAT_TYPE.ABILITY_TARGET_MIN_RANGE) / 100));
                if (dist < totalMinRange)
                {
                    if (casterInfo == playerCombatNode) ErrorEventsDisplayManager.Instance.ShowErrorEvent("The target is too close", 3);
                    return false;
                }

                float totalMaxRange = rankREF.maxRange + (rankREF.maxRange *
                                                          (GetTotalOfStatType(casterInfo,
                                                              RPGStat.STAT_TYPE.ABILITY_TARGET_MAX_RANGE) / 100));

                if (dist > totalMaxRange)
                {
                    if (casterInfo == playerCombatNode) ErrorEventsDisplayManager.Instance.ShowErrorEvent("The target is too far", 3);
                    return false;
                }

                bool processHit = hitResult.canHit;

                if (processHit || casterInfo != playerCombatNode) return processHit;
                ErrorEventsDisplayManager.Instance.ShowErrorEvent(hitResult.errorMessage, 3);

                return false;
            }

            ErrorEventsDisplayManager.Instance.ShowErrorEvent("This ability requires a target", 3);
            return false;

        }

        public bool UseRequirementsMet(CombatNode casterInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF, bool abMustBeKnown)
        {
            if (casterInfo == playerCombatNode && !rankREF.CanUseDuringGCD && currentGCD > 0)
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Not ready to use abilities yet", 3);
                return false;
            }
            
            if (abMustBeKnown)
            {
                if (casterInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.objectAction && !checkCooldown(casterInfo, ability))
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This ability is not ready yet", 3);
                    return false;
                }
            }


            if (casterInfo == playerCombatNode && rankREF.activationType == RPGAbility.AbilityActivationType.Casted && !rankREF.castInRun)
            {
                if (!playerCombatNode.playerControllerEssentials.IsGrounded()) return false;
                if (playerCombatNode.playerControllerEssentials.IsMoving() && playerCombatNode.playerControllerEssentials.GETControllerType() != RPGGeneralDATA.ControllerTypes.TopDownClickToMove) return false;
            }

            bool noEffectsReq = GameModifierManager.Instance.GetGameModifierBool(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.Ability + "+" +
                RPGGameModifier.AbilityModifierType.No_Effect_Requirement, ability.ID);
            if (!noEffectsReq && !AbilityRequiredEffectsChecking(rankREF, casterInfo))
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("The required effects are not on the target", 3);
                return false;
            }

            
            bool noReq = GameModifierManager.Instance.GetGameModifierBool(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.Ability + "+" +
                RPGGameModifier.AbilityModifierType.No_Use_Requirement, ability.ID);
            
            bool all = true;
            if (noReq) return checkTarget(casterInfo, rankREF) && all;
            foreach (var t in rankREF.useRequirements)
            {
                if (RequirementsManager.Instance.HandleAbilityRequirementUseType(casterInfo, t, true)) continue;
                all = false;
                break;
            }

            if (all)
            {
                ConsumeItemsFromAbilityUseRequirements(rankREF.useRequirements);
            }

            return checkTarget(casterInfo, rankREF) && all;
        }

        private void ConsumeItemsFromAbilityUseRequirements(List<RequirementsManager.AbilityUseRequirementDATA> requirementList)
        {
            foreach (var req in requirementList)
            {
                if(req.requirementType != RequirementsManager.AbilityUseRequirementType.item) continue;
                InventoryManager.Instance.RemoveItem(req.itemRequiredID, 1, -1, -1, false);
            }
        }

        private CombatNode getCorrectTargetToCheck(CombatNode casterInfo)
        {
            if (casterInfo == playerCombatNode && PlayerTargetData.currentTarget != null)
            {
                return PlayerTargetData.currentTarget;
            }

            if (casterInfo == null || casterInfo.agentREF == null) return null;
            return casterInfo.agentREF.target != null ? casterInfo.agentREF.target : null;
        }

        bool AbilityRequiredEffectsChecking(RPGAbility.RPGAbilityRankData rankREF, CombatNode casterInfo)
        {
            List<RPGEffect> effectsToRemove = new List<RPGEffect>();
            List<CombatNode> nodesToCheck = new List<CombatNode>();
            foreach (var effectRequired in rankREF.effectsRequirements)
            {
                RPGEffect effectREF = RPGBuilderUtilities.GetEffectFromID(effectRequired.effectRequiredID);
                CombatNode nodeChecked = effectRequired.target == RPGCombatDATA.TARGET_TYPE.Caster
                    ? casterInfo
                    : getCorrectTargetToCheck(casterInfo);
                
                if (!isEffectActiveOnTarget(effectREF, nodeChecked)) return false;
                if (!effectRequired.consumeEffect) continue;
                effectsToRemove.Add(effectREF);
                nodesToCheck.Add(nodeChecked);
            }

            for (var index = 0; index < effectsToRemove.Count; index++)
            {
                nodesToCheck[index].CancelState(nodesToCheck[index].getStateIndexFromEffectID(effectsToRemove[index].ID));
            }

            return true;
        }

        public void HandleAbilityCost(CombatNode nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            foreach (var t in rankREF.useRequirements)
                if (t.requirementType == RequirementsManager.AbilityUseRequirementType.statCost)
                {
                    int CostValue = t.useCost;
                    var statREF = RPGBuilderUtilities.GetStatFromID(t.statCostID);
                    float useMod = 0;
                    switch (t.costType)
                    {
                        case RPGAbility.COST_TYPES.PERCENT_OF_CURRENT:
                            useMod = (float)t.useCost / 100f;
                            CostValue = (int) (nodeCombatInfo.getCurrentValue(statREF._name) * useMod);
                            break;
                        case RPGAbility.COST_TYPES.PERCENT_OF_MAX:
                            useMod = (float)t.useCost / 100f;
                            CostValue = (int) (nodeCombatInfo.getCurrentMaxValue(statREF._name) * useMod);
                            break;
                    }
                    if (CostValue < 1) CostValue = 1;
                    
                    nodeCombatInfo.AlterVitalityStat(CostValue, t.statCostID);
                }
        }

        public void AbilityUsed(CombatNode cbtNode, int abilityID)
        {
            List<RPGCombo> newCombos = new List<RPGCombo>();
            bool cancelCombos = false;
            foreach (var combo in RPGBuilderEssentials.Instance.allCombos)
            {
                if (combo.initialAbilityID != abilityID) continue;
                if (combo.combos.Count == 0) continue;
                int isComboActive = RPGBuilderUtilities.IsComboActive(cbtNode, combo.ID, 0);
                if (isComboActive != -1)
                {
                    KeyCode nextKey = KeyCode.None;
                    switch (combo.combos[0].keyType)
                    {
                        case RPGCombo.KeyType.StartAbilityKey:
                            nextKey = RPGBuilderUtilities.GetAbilityKey(abilityID);
                            break;
                        case RPGCombo.KeyType.OverrideKey:
                            nextKey = combo.combos[0].overrideKey;
                            break;
                        case RPGCombo.KeyType.ActionKey:
                            nextKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(combo.combos[0].actionKeyName);
                            break;
                    }

                    CombosDisplayManager.Instance.UpdateComboEntry(isComboActive, nextKey,
                        combo.combos[0].keyType == RPGCombo.KeyType.StartAbilityKey);
                }
                else
                {
                    if (combo.StartCancelOtherCombos) cancelCombos = true;

                    CombatNode.ActiveCombo newCombo = new CombatNode.ActiveCombo();
                    newCombo.combo = combo;
                    newCombo.initialAbilityID = combo.initialAbilityID;
                    newCombo.readyTime = combo.combos[0].readyTime;
                    newCombo.curLoadTime = 0;
                    newCombo.expireTime = combo.combos[0].expireTime;
                    newCombo.curTime = newCombo.expireTime;

                    newCombos.Add(combo);

                    cbtNode.activeCombos.Add(newCombo);
                    if (cbtNode != playerCombatNode) continue;

                    KeyCode nextKey = KeyCode.None;
                    switch (combo.combos[0].keyType)
                    {
                        case RPGCombo.KeyType.StartAbilityKey:
                            nextKey = RPGBuilderUtilities.GetAbilityKey(abilityID);
                            break;
                        case RPGCombo.KeyType.OverrideKey:
                            nextKey = combo.combos[0].overrideKey;
                            break;
                        case RPGCombo.KeyType.ActionKey:
                            nextKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(combo.combos[0].actionKeyName);
                            break;
                    }

                    CombosDisplayManager.Instance.InitNewCombo(cbtNode.activeCombos.Count - 1, combo.combos[0],
                        nextKey);
                }
            }

            if (cancelCombos)
            {
                foreach (var previousCombo in cbtNode.activeCombos)
                {
                    if (newCombos.Contains(previousCombo.combo)) continue;
                    cbtNode.StartCoroutine(cbtNode.RemoveComboEntry(previousCombo, true, true));
                }
            }

            for (var index = 0; index < cbtNode.activeCombos.Count; index++)
            {
                var activeCombo = cbtNode.activeCombos[index];
                foreach (var comboEntry in activeCombo.combo.combos)
                {
                    if (comboEntry.abilityID != abilityID) continue;
                    if ((activeCombo.combo.combos.Count - 1) <= activeCombo.comboIndex)
                    {
                        cbtNode.StartCoroutine(cbtNode.RemoveComboEntry(activeCombo, true, true));
                    }
                    else
                    {
                        activeCombo.comboIndex++;

                        KeyCode nextKey = KeyCode.None;
                        switch (activeCombo.combo.combos[activeCombo.comboIndex].keyType)
                        {
                            case RPGCombo.KeyType.StartAbilityKey:
                                nextKey = RPGBuilderUtilities.GetAbilityKey(activeCombo.initialAbilityID);
                                break;
                            case RPGCombo.KeyType.OverrideKey:
                                nextKey = activeCombo.combo.combos[activeCombo.comboIndex].overrideKey;
                                break;
                            case RPGCombo.KeyType.ActionKey:
                                nextKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(activeCombo.combo.combos[activeCombo.comboIndex].actionKeyName);
                                break;
                        }

                        CombosDisplayManager.Instance.UpdateComboEntry(index, nextKey,
                            activeCombo.combo.combos[activeCombo.comboIndex].keyType == RPGCombo.KeyType.StartAbilityKey);
                    }
                }
            }
        }

        public void CancelOtherComboOptions(CombatNode cbtNode, RPGCombo combo)
        {
            foreach (var activeCombos in playerCombatNode.activeCombos)
            {
                if (activeCombos.combo == combo) continue;
                cbtNode.StartCoroutine(cbtNode.RemoveComboEntry(activeCombos, false, false));
            }
        }

        public void HandleAbilityTypeActions(CombatNode casterInfo, CombatNode targetInfo, RPGAbility thisAbility, RPGAbility.RPGAbilityRankData rankREF, bool OnStart)
        {
            switch (rankREF.targetType)
            {
                case RPGAbility.TARGET_TYPES.SELF:
                    EXECUTE_SELF_ABILITY(casterInfo, thisAbility, rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.AOE:
                    EXECUTE_AOE_ABILITY(casterInfo, thisAbility, rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.CONE:
                    EXECUTE_CONE_ABILITY(casterInfo, rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.PROJECTILE:
                case RPGAbility.TARGET_TYPES.TARGET_PROJECTILE:
                    StartCoroutine(EXECUTE_PROJECTILE_ABILITY(casterInfo, targetInfo, thisAbility, rankREF));
                    break;

                case RPGAbility.TARGET_TYPES.LINEAR:
                    EXECUTE_LINEAR_ABILITY(casterInfo, thisAbility, OnStart
                        ? RPGCombatDATA.CombatVisualActivationType.Activate
                        : RPGCombatDATA.CombatVisualActivationType.Completed, rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.GROUND:
                    if (casterInfo != playerCombatNode) return;
                    playerIsGroundCasting = true;
                    if (RPGBCharacterControllerEssentials.IsUsingBuiltInControllers())
                    {
                        playerCombatNode.playerControllerEssentials.builtInController.StartCoroutine(
                            playerCombatNode.playerControllerEssentials.builtInController.UpdateCachedGroundCasting(playerIsGroundCasting));
                    }

                    playerCombatNode.currentAbilityCastedSlot = 0;
                    playerCombatNode.currentAbilityCasted = thisAbility;
                    playerCombatNode.currentAbilityCastedCurRank = rankREF;
                    INIT_GROUND_ABILITY(rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.GROUND_LEAP:
                    if (casterInfo != playerCombatNode) return;
                    playerIsGroundCasting = true;
                    if (RPGBCharacterControllerEssentials.IsUsingBuiltInControllers())
                    {
                        playerCombatNode.playerControllerEssentials.builtInController.StartCoroutine(
                            playerCombatNode.playerControllerEssentials.builtInController.UpdateCachedGroundCasting(playerIsGroundCasting));
                    }

                    playerCombatNode.currentAbilityCastedSlot = 0;
                    playerCombatNode.currentAbilityCasted = thisAbility;
                    playerCombatNode.currentAbilityCastedCurRank = rankREF;
                    INIT_GROUND_ABILITY(rankREF);
                    break;

                case RPGAbility.TARGET_TYPES.TARGET_INSTANT:
                    if (casterInfo == playerCombatNode)
                    {
                        CombatNode correctTarget = rankREF.activationType == RPGAbility.AbilityActivationType.Casted
                            ? casterInfo.currentTargetCasted
                            : PlayerTargetData.currentTarget;
                        if (correctTarget != null && !correctTarget.dead)
                            EXECUTE_TARGET_INSTANT_ABILITY(casterInfo, correctTarget, rankREF);
                    }
                    else
                    {
                        if (casterInfo.agentREF.target != null && !casterInfo.agentREF.target.dead)
                            EXECUTE_TARGET_INSTANT_ABILITY(casterInfo, casterInfo.agentREF.target, rankREF);
                    }
                    break;
            }
        }

        private float getAutoAttackCD()
        {
            float cd = playerCombatNode.AutoAttackData.weaponItem != null ?
                playerCombatNode.AutoAttackData.weaponItem.AttackSpeed : RPGBuilderUtilities.GetAbilityFromID(playerCombatNode.AutoAttackData.currentAutoAttackAbilityID).ranks[0].cooldown;

            float curAttackSpeed = 0;
            foreach (var t in playerCombatNode.nodeStats)
            {
                foreach (var t1 in t.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.ATTACK_SPEED:
                            curAttackSpeed += t.curValue;
                            break;
                    }
                }
            }

            if (curAttackSpeed != 0)
            {
                cd = cd - cd * (curAttackSpeed / 100);
            }
            return cd;
        }

        public bool CombatNodeCanInitAbility(CombatNode cbtNode, RPGAbility thisAbility, bool abMustBeKnown)
        {
            return !cbtNode.isInMotion() && !cbtNode.isSleeping() && !cbtNode.isStunned() && !cbtNode.isSilenced() && !cbtNode.dead && !cbtNode.IsCasting && (cbtNode != playerCombatNode || !playerIsGroundCasting) && !cbtNode.IsChanneling && !cbtNode.isLeaping && (cbtNode != playerCombatNode || !abMustBeKnown || (RPGBuilderUtilities.isAbilityKnown(thisAbility.ID) || playerCombatNode.appearanceREF.isShapeshifted)) && combatEnabled;
        }
        public bool CombatNodeCanInitMotion(CombatNode cbtNode)
        {
            return !cbtNode.dead && !cbtNode.IsCasting && !cbtNode.IsChanneling && !cbtNode.isLeaping;
        }

        public void AbilityKeyUp(CombatNode casterNode, RPGAbility thisAbility, bool abMustBeKnown)
        {
            int curRank = RPGBuilderUtilities.getAbilityRank(thisAbility.ID);
            if (!abMustBeKnown) curRank = 0;
            var rankREF = thisAbility.ranks[curRank];

            foreach (var effectApplied in rankREF.effectsApplied)
            {
                RPGEffect effect = RPGBuilderUtilities.GetEffectFromID(effectApplied.effectID);
                RPGEffect.RPGEffectRankData effectRankREF = RPGBuilderUtilities.GetEffectFromID(effectApplied.effectID)
                    .ranks[effectApplied.effectRank];
                switch (effect.effectType)
                {
                   case RPGEffect.EFFECT_TYPE.Blocking:
                       if (playerCombatNode.isActiveBlocking && playerCombatNode.curBlockEffect == effectRankREF && effectRankREF.blockDurationType == RPGEffect.BLOCK_DURATION_TYPE.HoldKey)
                       {
                           playerCombatNode.ResetActiveBlocking();
                       }
                       break;
                }
            }
        }

        
        public void InitStealth(CombatNode targetInfo, bool showActionBar, List<RPGAbility.AbilityEffectsApplied> nestedEffects)
        {
            targetInfo.stealthed = true;
            DestroyStealthNodeCombatEntities(targetInfo);

            if (showActionBar)
            {
                ActionBarManager.Instance.InitStealthActionBar();
            }
                        
            HandleNestedEffects(targetInfo, nestedEffects);
            ScreenOverlayManager.Instance.ShowOverlay("STEALTH");
        }
        
        public void CancelStealth(CombatNode cbtNode)
        {
            cbtNode.stealthed = false;
            DestroyStealthEndNodeCombatEntities(cbtNode);

            for (var index = 0; index < cbtNode.nodeStateData.Count; index++)
            {
                var state = cbtNode.nodeStateData[index];
                if (state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Stealth) continue;

                if (state.stateEffect.ranks[state.effectRank].showStealthActionBar)
                {
                    ActionBarManager.Instance.ResetStealthActionBar();
                }

                ResetNestedEffects(cbtNode, state.stateEffect, state.effectRank);

                cbtNode.nodeStateData.Remove(state);
                PlayerStatesDisplayHandler.Instance.RemoveState(index);
                break;
            }
            
            ScreenOverlayManager.Instance.HideOverlay("STEALTH");
        }

        private bool HandleInitAbilityStealthActions(CombatNode casterNode, RPGAbility.RPGAbilityRankData rankREF)
        {
            int tagEffectID = RPGBuilderUtilities.getStealthTagEffectID(rankREF);
            int currentActiveStealthEffectID = RPGBuilderUtilities.getActiveStealthEffectID(casterNode);
            if (tagEffectID == currentActiveStealthEffectID)
            {
                CancelStealth(casterNode);
                return true;
            }

            if (!rankREF.cancelStealth) return false;
            CancelStealth(casterNode);
            return false;

        }

        private bool HandleInitAbilityShapeshiftingActions(CombatNode casterNode, RPGAbility.RPGAbilityRankData rankREF)
        {
            int tagEffectID = RPGBuilderUtilities.getShapeshiftingTagEffectID(rankREF);
            int currentActiveShapeshiftingEffectID =
                RPGBuilderUtilities.getActiveShapeshiftingEffectID(casterNode);
            if (tagEffectID != currentActiveShapeshiftingEffectID) return false;
            ResetPlayerShapeshift();
            return true;
        }

        private void HandleInitAbilityControllerActions(RPGAbility.RPGAbilityRankData rankREF)
        {
            playerCombatNode.playerControllerEssentials.AbilityInitActions(rankREF);
        }

        private void HandleInitAbilityActions(CombatNode casterNode, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.targetType == RPGAbility.TARGET_TYPES.GROUND ||
                rankREF.targetType == RPGAbility.TARGET_TYPES.GROUND_LEAP) return;

            ExecuteCasterEffects(casterNode, rankREF);

            ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Activate,
                casterNode, rankREF.visualEffects);
            ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType.Activate,
                casterNode, rankREF.visualAnimations);


            if (casterNode == playerCombatNode)
            {
                if (rankREF.standTimeDuration > 0)
                {
                    playerCombatNode.playerControllerEssentials.InitStandTime(rankREF.standTimeDuration);
                }

                if (rankREF.castSpeedSlowAmount > 0)
                {
                    playerCombatNode.playerControllerEssentials.InitCastMoveSlow(
                        rankREF.castSpeedSlowAmount,
                        rankREF.castSpeedSlowTime, rankREF.castSpeedSlowRate);
                }
            }
            else
            {
                if (rankREF.standTimeDuration > 0)
                {
                    casterNode.agentREF.InitStandTime(rankREF.standTimeDuration, rankREF.canRotateInStandTime);
                }
            }
        }

        private void HandleInitAbilityToggleActions(CombatNode casterNode, RPGAbility.RPGAbilityRankData rankREF, RPGAbility ability)
        {
            if (RPGBuilderUtilities.isAbilityToggled(casterNode, ability))
            {
                casterNode.RemoveToggledAbility(ability);
            }
            else
            {
                CombatNode.ActiveToggledAbilities newToggledAbility = new CombatNode.ActiveToggledAbilities {ability = ability, rankREF = rankREF, nextTrigger = 0};
                playerCombatNode.activeToggledAbilities.Add(newToggledAbility);

                if (!rankREF.isToggleCostOnTrigger)
                {
                    HandleAbilityCost(casterNode, rankREF);
                }

                ActionBarManager.Instance.UpdateToggledAbilities();
            }
        }

        private void HandleInitCooldown(CombatNode casterNode, RPGAbility.RPGAbilityRankData rankREF, RPGAbility ability, int abSlotIndex)
        {
            if (rankREF.targetType == RPGAbility.TARGET_TYPES.GROUND || rankREF.targetType == RPGAbility.TARGET_TYPES.GROUND_LEAP) return;
            switch (ability.abilityType)
            {
                case RPGAbility.AbilityType.Normal:
                    StartCooldown(casterNode, abSlotIndex, rankREF, ability.ID);
                    break;
                case RPGAbility.AbilityType.PlayerAutoAttack:
                    playerCombatNode.InitAACooldown(getAutoAttackCD());
                    break;
                case RPGAbility.AbilityType.PlayerActionAbility:
                    playerCombatNode.InitActionAbilityCooldown(ability.ID,
                        rankREF.cooldown);
                    break;
            }

            if (rankREF.isGCD)
            {
                StartGCD();
            }
        }

        public void InitAbility(CombatNode casterNode, RPGAbility thisAbility, bool abMustBeKnown)
        {
            if (!CombatNodeCanInitAbility(casterNode, thisAbility, abMustBeKnown)) return;

            var rankREF = RPGBuilderUtilities.getCurrentAbilityRankREF(casterNode, thisAbility, abMustBeKnown);
            var abSlotIndex = casterNode.getAbilityDATAIndex(thisAbility);

            switch (casterNode.nodeType)
            {
                case CombatNode.COMBAT_NODE_TYPE.player:
                {
                    if (UseRequirementsMet(casterNode, thisAbility, rankREF, abMustBeKnown))
                    {
                        if (casterNode.isStealthed() && HandleInitAbilityStealthActions(casterNode, rankREF)) return;
                        if (casterNode.appearanceREF.isShapeshifted &&
                            HandleInitAbilityShapeshiftingActions(casterNode, rankREF)) return;

                        HandleInitAbilityControllerActions(rankREF);

                        HandleInitAbilityActions(casterNode, rankREF);

                        switch (rankREF.activationType)
                        {
                            case RPGAbility.AbilityActivationType.Instant:
                            case RPGAbility.AbilityActivationType.Channeled:
                                if (rankREF.isToggle)
                                {
                                    HandleInitAbilityToggleActions(casterNode, rankREF, thisAbility);
                                }
                                else
                                {
                                    HandleAbilityTypeActions(casterNode, getCorrectTargetToCheck(casterNode),
                                        thisAbility,
                                        rankREF, true);

                                    HandleInitCooldown(casterNode, rankREF, thisAbility, abSlotIndex);

                                    if (rankREF.activationType == RPGAbility.AbilityActivationType.Channeled)
                                    {
                                        casterNode.InitChanneling(thisAbility, abSlotIndex, rankREF);
                                        PlayerInfoDisplayManager.Instance.InitChannelBar(thisAbility);
                                    }

                                    HandleAbilityCost(casterNode, rankREF);
                                }

                                AbilityUsed(casterNode, thisAbility.ID);
                                break;
                            case RPGAbility.AbilityActivationType.Casted:
                                if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT ||
                                    rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
                                {
                                    if (PlayerTargetData.currentTarget.dead)
                                    {
                                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("The target is dead", 3);
                                        return;
                                    }

                                    casterNode.currentTargetCasted = PlayerTargetData.currentTarget;
                                }

                                casterNode.InitCasting(thisAbility, abSlotIndex, rankREF);
                                if (rankREF.castBarVisible) PlayerInfoDisplayManager.Instance.InitCastBar(thisAbility);

                                if (rankREF.targetType == RPGAbility.TARGET_TYPES.PROJECTILE ||
                                    rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
                                {
                                    ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Activate,
                                        casterNode, rankREF.visualEffects);
                                    ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType.Activate,
                                        casterNode, rankREF.visualAnimations);

                                }

                                if (!rankREF.comboStarsAfterCastComplete) AbilityUsed(casterNode, thisAbility.ID);
                                break;
                            case RPGAbility.AbilityActivationType.Charged:
                                break;
                        }
                    }

                    break;
                }
                case CombatNode.COMBAT_NODE_TYPE.mob:
                case CombatNode.COMBAT_NODE_TYPE.pet:
                case CombatNode.COMBAT_NODE_TYPE.objectAction:
                {
                    if (casterNode.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
                        UseRequirementsMet(casterNode, thisAbility, rankREF, true))
                    {
                        HandleInitAbilityActions(casterNode, rankREF);

                        if (rankREF.activationType == RPGAbility.AbilityActivationType.Casted)
                        {
                            if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT ||
                                rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
                            {
                                if (casterNode.agentREF.target.dead)
                                {
                                    return;
                                }

                                casterNode.currentTargetCasted = casterNode.agentREF.target;
                            }

                            casterNode.InitCasting(thisAbility, abSlotIndex, rankREF);
                        }
                        else
                        {
                            HandleAbilityTypeActions(casterNode, getCorrectTargetToCheck(casterNode), thisAbility,
                                rankREF, true);

                            if (casterNode.nodeType != CombatNode.COMBAT_NODE_TYPE.objectAction &&
                                rankREF.targetType != RPGAbility.TARGET_TYPES.GROUND &&
                                rankREF.targetType != RPGAbility.TARGET_TYPES.GROUND_LEAP)
                            {
                                HandleInitCooldown(casterNode, rankREF, thisAbility, abSlotIndex);
                            }

                            HandleAbilityCost(casterNode, rankREF);

                            if (rankREF.activationType == RPGAbility.AbilityActivationType.Channeled)
                                casterNode.InitChanneling(thisAbility, abSlotIndex, rankREF);
                        }
                    }

                    break;
                }
            }
        }



        public bool actionAbIsReady(RPGAbility ab)
        {
            foreach (var t in CharacterData.Instance.currentActionAbilities)
            {
                if (t.ability == ab) return Time.time >= t.NextTimeUse;
            }
            return false;
        }
        
        private void Update()
        {
            if (playerIsGroundCasting) HandleGroundCasting();
        }

        private void FixedUpdate()
        {
            if (currentGCD > 0)
            {
                currentGCD -= Time.deltaTime;
            }
            else
            {
                currentGCD = 0;
            }
        }

        private void HandleGroundCasting()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                EXECUTE_GROUND_ABILITY(playerCombatNode, playerCombatNode.currentAbilityCasted,
                    RPGCombatDATA.CombatVisualActivationType.Activate,
                    playerCombatNode.currentAbilityCastedCurRank);
                playerIsGroundCasting = false;
                playerCombatNode.currentAbilityCastedSlot = -1;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                groundIndicatorManager.HideIndicator();
                playerIsGroundCasting = false;
                playerCombatNode.currentAbilityCastedSlot = -1;
            }

            if (RPGBCharacterControllerEssentials.IsUsingBuiltInControllers())
            {
                playerCombatNode.playerControllerEssentials.builtInController.StartCoroutine(
                    playerCombatNode.playerControllerEssentials.builtInController.UpdateCachedGroundCasting(playerIsGroundCasting));
            }
        }

        public void StartGCD()
        {
            var finalCD = RPGBuilderEssentials.Instance.combatSettings.GCDDuration;
            float cdrecoveryspeed = GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.GCD_DURATION);

            if (cdrecoveryspeed != 0)
            {
                cdrecoveryspeed /= 100;
                finalCD -= finalCD * cdrecoveryspeed;
            }
            currentGCD = finalCD;
        }
        public void StartCooldown(CombatNode casterInfo, int abilitySlotID, RPGAbility.RPGAbilityRankData rankREF, int abID)
        {
            var finalCD = rankREF.cooldown;
            float cdrecoveryspeed = GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.CD_RECOVERY_SPEED);

            if (cdrecoveryspeed != 0)
            {
                cdrecoveryspeed /= 100;
                finalCD -= finalCD * cdrecoveryspeed;
            }

            if (casterInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
            {
                CharacterData.Instance.InitAbilityCooldown(abID, finalCD);
                
                if (rankREF.isSharingCooldown)
                {
                    foreach (var ab in RPGBuilderEssentials.Instance.allAbilities)
                    {
                        if(!RPGBuilderUtilities.isAbilityKnown(ab.ID)) continue;
                        RPGAbility.RPGAbilityRankData thisRankREF = ab.ranks[RPGBuilderUtilities.getAbilityRank(ab.ID)];
                        if (thisRankREF.isSharingCooldown && thisRankREF.cooldownTag == rankREF.cooldownTag)
                        {
                            float thisCD = thisRankREF.cooldown - (thisRankREF.cooldown * cdrecoveryspeed);
                            CharacterData.Instance.InitAbilityCooldown(ab.ID, thisCD);
                        }
                    }
                }
            }
            else
            {
                casterInfo.abilitiesData[abilitySlotID].NextTimeUse = finalCD;
                casterInfo.abilitiesData[abilitySlotID].CDLeft = finalCD;
            }
        }

        private void EXECUTE_LINEAR_ABILITY(CombatNode nodeCombatInfo, RPGAbility ability, RPGCombatDATA.CombatVisualActivationType activationType, RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteCombatVisualEffectList(activationType, nodeCombatInfo, rankREF.visualEffects);
            
            if (rankREF.ConeHitCount == 1)
            {
                List<CombatNode> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rankREF.linearLength), nodeCombatInfo, rankREF);
                List<CombatNode> cbtNodeInArea = new List<CombatNode>();

                double Fi, cs, sn;

                foreach (var t in allCbtNodes)
                {
                    Fi = nodeCombatInfo.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
                    cs = Mathf.Cos((float) Fi);
                    sn = Mathf.Sin((float) Fi);

                    var tx = t.gameObject.transform.position.x - nodeCombatInfo.transform.position.x;
                    var tz = t.gameObject.transform.position.z - nodeCombatInfo.transform.position.z;

                    var ptx = (float) (cs * tx - sn * tz);
                    var ptz = (float) (sn * tx + cs * tz);

                    if (!(-(rankREF.linearWidth / 2) <= ptx) || !(ptx <= rankREF.linearWidth / 2)) continue;
                    if (ptz >= 0 && ptz <= rankREF.linearLength) cbtNodeInArea.Add(t);
                }

                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, cbtNodeInArea, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);
            }
            else
            {
                StartCoroutine(EXECUTE_LINEAR_ABILITY_PULSE(nodeCombatInfo, ability));
            }
        }

        private IEnumerator EXECUTE_LINEAR_ABILITY_PULSE(CombatNode nodeCombatInfo, RPGAbility ability)
        {
            var curRank = 0;
            if (nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                curRank = 0;
            else
                curRank = RPGBuilderUtilities.getAbilityRank(ability.ID);
            var rankREF = ability.ranks[curRank];

            for (var x = 0; x < rankREF.ConeHitCount; x++)
            {
                if(nodeCombatInfo.dead) yield break;
                List<CombatNode> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rankREF.linearLength), nodeCombatInfo, rankREF);
                List<CombatNode> cbtNodeInArea = new List<CombatNode>();

                double Fi, cs, sn;

                foreach (var t in allCbtNodes)
                {
                    Fi = nodeCombatInfo.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
                    cs = Mathf.Cos((float) Fi);
                    sn = Mathf.Sin((float) Fi);

                    var tx = t.gameObject.transform.position.x - nodeCombatInfo.transform.position.x;
                    var tz = t.gameObject.transform.position.z - nodeCombatInfo.transform.position.z;

                    var ptx = (float) (cs * tx - sn * tz);
                    var ptz = (float) (sn * tx + cs * tz);

                    if (!(-(rankREF.linearWidth / 2) <= ptx) || !(ptx <= rankREF.linearWidth / 2)) continue;
                    if (ptz >= 0 && ptz <= rankREF.linearLength) cbtNodeInArea.Add(t);
                }

                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, cbtNodeInArea, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);

                yield return new WaitForSeconds(rankREF.ConeHitInterval);
            }
        }

        private List<CombatNode> RemoveAlreadyHitTargetsFromArray(ProjectileHitDetection projREF, List<CombatNode> allCbtNodes)
        {
            foreach (var t in projREF.alreadyHitNodes)
            {
                if (allCbtNodes.Contains(t)) allCbtNodes.Remove(t);
            }

            return allCbtNodes;
        }

        public void FIND_NEARBY_UNITS(CombatNode nodeCombatInfo, GameObject projectileGO, RPGAbility ability,
            ProjectileHitDetection projREF)
        {
            var curRank = 0;
            if (nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                curRank = 0;
            else
                curRank = RPGBuilderUtilities.getAbilityRank(ability.ID);
            var rankREF = ability.ranks[curRank];

            List<CombatNode> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(projectileGO.transform.position, rankREF.projectileNearbyUnitDistanceMax), nodeCombatInfo, rankREF);

            var hitNodeList = RemoveAlreadyHitTargetsFromArray(projREF, allCbtNodes);
            if (hitNodeList.Count == 0)
            {
                Destroy(projectileGO);
            }
            else
            {
                int totalUnitHit = rankREF.MaxUnitHit + (int) GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestNearbyUnits(projREF.gameObject, hitNodeList, totalUnitHit);
                if (closestUnits.Count > 0)
                    projREF.curNearbyTargetGO = closestUnits[0].gameObject;
                else
                    Destroy(projectileGO);
            }
        }

        private void EXECUTE_SELF_ABILITY(CombatNode casterInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteAbilityEffects(casterInfo, casterInfo, rankREF);
        }

        private void EXECUTE_AOE_ABILITY(CombatNode nodeCombatInfo, RPGAbility ability, RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Completed, nodeCombatInfo, rankREF.visualEffects);

            if (rankREF.AOEHitCount == 1)
            {
                float totalRadius = rankREF.AOERadius +
                                    (rankREF.AOERadius * (GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.AOE_RADIUS)/100));
                
                List<CombatNode> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, totalRadius), nodeCombatInfo, rankREF);
                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, allCbtNodes, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);
            }
            else
            {
                StartCoroutine(EXECUTE_AOE_ABILITY_PULSE(nodeCombatInfo, rankREF));
            }
        }

        private IEnumerator EXECUTE_AOE_ABILITY_PULSE(CombatNode nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (nodeCombatInfo == null) yield break;

            for (var x = 0; x < rankREF.AOEHitCount; x++)
            {
                if(nodeCombatInfo.dead) yield break;
                if (nodeCombatInfo == null) yield break;
                if (!rankREF.canBeUsedStunned && nodeCombatInfo.isStunned()) yield break;
                
                float totalRadius = rankREF.AOERadius +
                                    (rankREF.AOERadius * (GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.AOE_RADIUS)/100));
                List<CombatNode> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, totalRadius), nodeCombatInfo, rankREF);
                
                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, allCbtNodes, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);

                yield return new WaitForSeconds(rankREF.AOEHitInterval);
            }
        }

        private void EXECUTE_CONE_ABILITY(CombatNode nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {

            if (rankREF.ConeHitCount == 1)
            {
                List<CombatNode> allCbtNodes =
                    getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rankREF.minRange), nodeCombatInfo, rankREF);

                var unitInCone = new List<CombatNode>();
                foreach (var t in allCbtNodes)
                {
                    var pointDirection = t.transform.position - nodeCombatInfo.transform.position;
                    var angle = Vector3.Angle(nodeCombatInfo.transform.forward, pointDirection);
                    if (angle < rankREF.coneDegree)
                        unitInCone.Add(t);
                }

                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int) GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, unitInCone, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);
            }
            else
            {
                StartCoroutine(EXECUTE_CONE_ABILITY_PULSE(nodeCombatInfo, rankREF));
            }
        }

        private IEnumerator EXECUTE_CONE_ABILITY_PULSE(CombatNode nodeCombatInfo, RPGAbility.RPGAbilityRankData rankREF)
        {

            for (var x = 0; x < rankREF.ConeHitCount; x++)
            {
                if(nodeCombatInfo.dead) yield break;
                List<CombatNode> allCbtNodes = getPotentialCombatNodes(Physics.OverlapSphere(nodeCombatInfo.transform.position, rankREF.minRange), nodeCombatInfo, rankREF);
                var unitInCone = new List<CombatNode>();
                foreach (var t in allCbtNodes)
                {
                    var pointDirection = t.transform.position - nodeCombatInfo.transform.position;
                    var angle = Vector3.Angle(nodeCombatInfo.transform.forward, pointDirection);
                    if (angle < rankREF.coneDegree)
                        unitInCone.Add(t);
                }

                int totalUnitHit = rankREF.MaxUnitHit +
                                   (int)GetTotalOfStatType(nodeCombatInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
                var closestUnits = getClosestUnits(nodeCombatInfo, unitInCone, totalUnitHit);
                foreach (var t in closestUnits)
                    ExecuteAbilityEffects(nodeCombatInfo, t, rankREF);

                yield return new WaitForSeconds(rankREF.ConeHitInterval);
            }
        }

        private void EXECUTE_TARGET_INSTANT_ABILITY(CombatNode casterInfo, CombatNode targetInfo,
            RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.hitEffect != null)
            {
                SpawnHitEffect(targetInfo, rankREF);
            }
            ExecuteAbilityEffects(casterInfo, targetInfo, rankREF);
        }

        public void ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType activationType, CombatNode cbtNode, List<RPGCombatDATA.CombatVisualEffect> combatVisualEffects)
        {
            foreach (var t in combatVisualEffects)
            {
                StartCoroutine(ExecuteCombatVisualEffect(t, activationType, cbtNode, false, Vector3.zero));
            }
        }

        private IEnumerator ExecuteCombatVisualEffect(RPGCombatDATA.CombatVisualEffect visualEffect,
            RPGCombatDATA.CombatVisualActivationType activationType, CombatNode cbtNode, bool isOverridePOS,
            Vector3 overridePosition)
        {
            yield return new WaitForSeconds(visualEffect.delay);
            if (visualEffect.activationType != activationType) yield break;
            if (visualEffect.EffectGO == null)
            {
                if (visualEffect.Sound != null && !visualEffect.SoundParentedToEffect)
                {
                    RPGBuilderUtilities.PlaySound(cbtNode.gameObject, null, visualEffect.Sound,
                        visualEffect.SoundParentedToEffect);
                }
                yield break;
            }

            Vector3 spawnPos = isOverridePOS
                ? overridePosition
                : GetEffectSpawnPosition(cbtNode, visualEffect.UseNodeSocket, visualEffect.positionOffset,
                    visualEffect.SocketName);
            var cbtVisualEffect = Instantiate(visualEffect.EffectGO,
                spawnPos, Quaternion.identity);
            cbtVisualEffect.transform.localScale = visualEffect.effectScale;

            if (!isOverridePOS)
            {
                cbtVisualEffect.transform.SetParent(GetEffectTransform(cbtNode, visualEffect.UseNodeSocket,
                    visualEffect.SocketName));
                cbtVisualEffect.transform.localPosition =
                    visualEffect.UseNodeSocket ? Vector3.zero : visualEffect.positionOffset;
                cbtVisualEffect.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }

            if (!visualEffect.ParentedToCaster)
            {
                cbtVisualEffect.transform.SetParent(null);
            }
            
            if (visualEffect.Sound != null)
            {
                RPGBuilderUtilities.PlaySound(cbtNode.gameObject, cbtVisualEffect, visualEffect.Sound,
                    visualEffect.SoundParentedToEffect);
            }

            Destroy(cbtVisualEffect, visualEffect.duration);

            if (visualEffect.isDestroyedOnStun) cbtNode.AddLogicCombatVisualToDestroyList(cbtVisualEffect);
            if (visualEffect.isDestroyedOnDeath) cbtNode.AddCombatVisualToDestroyList(cbtVisualEffect);
            if (visualEffect.isDestroyedOnStealth) cbtNode.AddCombatVisualToDestroyOnStealthList(cbtVisualEffect);
            if (visualEffect.isDestroyedOnStealthEnd) cbtNode.AddCombatVisualToDestroyOnStealthEndList(cbtVisualEffect);
        }

        public void ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType activationType, CombatNode cbtNode, List<RPGCombatDATA.CombatVisualAnimation> combatVisualAnimations)
        {
            foreach (var t in combatVisualAnimations)
            {
                ExecuteCombatVisualAnimation(t, activationType, cbtNode);
            }
        }
        
        public void ExecuteCombatVisualAnimation(RPGCombatDATA.CombatVisualAnimation visualAnimation, RPGCombatDATA.CombatVisualActivationType activationType, CombatNode cbtNode)
        {
            if (visualAnimation.activationType != activationType) return;
            if (string.IsNullOrEmpty(visualAnimation.animationParameter)) return;

            switch (cbtNode.nodeType)
            {
                case CombatNode.COMBAT_NODE_TYPE.mob:
                case CombatNode.COMBAT_NODE_TYPE.pet:
                    if (cbtNode.agentREF != null)
                    {
                        StartCoroutine(HandleAnimationPlay(cbtNode.agentREF.thisAnim, visualAnimation, cbtNode));
                    }
                    break;
                case CombatNode.COMBAT_NODE_TYPE.player:
                    CombatNode.ActiveAnimationCoroutine newAnimationCoroutine =
                        new CombatNode.ActiveAnimationCoroutine();
                    newAnimationCoroutine.anim = cbtNode.playerControllerEssentials.anim;
                    newAnimationCoroutine.visualAnimation = visualAnimation;
                    newAnimationCoroutine.coroutine = StartCoroutine(HandleAnimationPlay(cbtNode.playerControllerEssentials.anim, visualAnimation, cbtNode));
                    cbtNode.allAnimationCoroutines.Add(newAnimationCoroutine);
                    if (visualAnimation.showWeapons && !playerCombatNode.inCombat)
                        playerCombatNode.appearanceREF.ShowWeaponsTemporarily(visualAnimation.showWeaponDuration);
                    break;
                case CombatNode.COMBAT_NODE_TYPE.objectAction:
                    break;
            }
        }

        private IEnumerator HandleAnimationPlay(Animator anim, RPGCombatDATA.CombatVisualAnimation visualAnimation, CombatNode cbtNode)
        {
            yield return new WaitForSeconds(visualAnimation.delay);
            if(cbtNode.dead) yield break;
            switch (visualAnimation.parameterType)
            {
                case RPGCombatDATA.CombatVisualAnimationParameterType.Bool:
                    anim.SetBool(visualAnimation.animationParameter, visualAnimation.boolValue);
                    yield return new WaitForSeconds(visualAnimation.duration);
                    anim.SetBool(visualAnimation.animationParameter, !visualAnimation.boolValue);
                    ResetQueuedAnimation(anim, visualAnimation, cbtNode);
                    break;
                case RPGCombatDATA.CombatVisualAnimationParameterType.Int:
                    anim.SetInteger(visualAnimation.animationParameter, visualAnimation.intValue);
                    break;
                case RPGCombatDATA.CombatVisualAnimationParameterType.Float:
                    anim.SetFloat(visualAnimation.animationParameter, visualAnimation.floatValue);
                    break;
                case RPGCombatDATA.CombatVisualAnimationParameterType.Trigger:
                    anim.SetTrigger(visualAnimation.animationParameter);
                    break;
            }
        }

        public void ResetQueuedAnimation(Animator anim, RPGCombatDATA.CombatVisualAnimation visualAnimation, CombatNode cbtNode)
        {
            List<CombatNode.ActiveAnimationCoroutine> animationCoroutinesToRemove =
                new List<CombatNode.ActiveAnimationCoroutine>();
            foreach (var animationCoroutine in cbtNode.allAnimationCoroutines)
            {
                if (anim != animationCoroutine.anim || visualAnimation != animationCoroutine.visualAnimation) continue;
                
                StopCoroutine(animationCoroutine.coroutine);
                animationCoroutinesToRemove.Add(animationCoroutine);
            }
            
            foreach (var animationCoroutine in animationCoroutinesToRemove)
            {
                cbtNode.allAnimationCoroutines.Remove(animationCoroutine);
            }
        }

        private void INIT_GROUND_ABILITY(RPGAbility.RPGAbilityRankData rankREF)
        {
            groundIndicatorManager.ShowIndicator(rankREF.groundRadius * 1.25f, rankREF.groundRange);
        }

        private void EXECUTE_GROUND_ABILITY(CombatNode nodeCombatInfo, RPGAbility ability,
            RPGCombatDATA.CombatVisualActivationType activationType, RPGAbility.RPGAbilityRankData rankREF)
        {

            if (rankREF.targetType == RPGAbility.TARGET_TYPES.GROUND_LEAP)
                EXECUTE_GROUND_LEAP_MOVEMENT(nodeCombatInfo, ability, groundIndicatorManager.GetIndicatorPosition());

            if (rankREF.groundVisualEffect != null)
            {
                Vector3 spawnPos = groundIndicatorManager.GetIndicatorPosition() + rankREF.effectPositionOffset;
                
                var cbtVisualGO = Instantiate(rankREF.groundVisualEffect, spawnPos, rankREF.groundVisualEffect.transform.rotation);
                var groundHitRef = cbtVisualGO.AddComponent<GroundHitDetection>();
                groundHitRef.InitGroundAbility(nodeCombatInfo, rankREF.groundVisualEffectDuration, ability);
            }
            groundIndicatorManager.HideIndicator();
            
            ExecuteCombatVisualEffectList(activationType, nodeCombatInfo, rankREF.visualEffects);
            ExecuteCombatVisualAnimationList(activationType, nodeCombatInfo, rankREF.visualAnimations);
            
            StartCooldown(nodeCombatInfo, playerCombatNode.currentAbilityCastedSlot, rankREF, ability.ID);

            if (rankREF.isGCD && nodeCombatInfo == playerCombatNode)
            {
                StartGCD();
            }
            
            if (rankREF.standTimeDuration > 0)
                playerCombatNode.playerControllerEssentials.InitStandTime(rankREF.standTimeDuration);

            if (rankREF.castSpeedSlowAmount > 0)
                playerCombatNode.playerControllerEssentials.InitCastMoveSlow(rankREF.castSpeedSlowAmount,
                    rankREF.castSpeedSlowTime, rankREF.castSpeedSlowRate);
        }

        private void EXECUTE_GROUND_LEAP_MOVEMENT(CombatNode nodeCombatInfo, RPGAbility ability, Vector3 LeapEndPOS)
        {
            var curRank = 0;
            if (nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.objectAction ||
                nodeCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                curRank = 0;
            else
                curRank = RPGBuilderUtilities.getAbilityRank(ability.ID);
            var rankREF = ability.ranks[curRank];

            playerCombatNode.playerControllerEssentials.InitGroundLeap();
            nodeCombatInfo.InitLeap(nodeCombatInfo.transform.position, LeapEndPOS, rankREF.groundLeapHeight,
                rankREF.groundLeapSpeed);
        }

        public void EXECUTE_GROUND_ABILITY_HIT(CombatNode casterInfo, CombatNode targetInfo, RPGAbility ability,
            RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteAbilityEffects(casterInfo, targetInfo, rankREF);
        }

        private ProjectileHitDetection InitProjectileComponents(GameObject projGO, RPGAbility.RPGAbilityRankData rankREF)
        {
            // INIT PROJECTILE
            var projHitRef = projGO.AddComponent<ProjectileHitDetection>();
            Rigidbody rb = projGO.AddComponent<Rigidbody>();
            projHitRef.RB = rb;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            switch (rankREF.projectileColliderType)
            {
                case RPGNpc.NPCColliderType.Capsule:
                    CapsuleCollider capsule = projGO.AddComponent<CapsuleCollider>();
                    capsule.center = rankREF.colliderCenter;
                    capsule.radius = rankREF.colliderRadius;
                    capsule.height = rankREF.colliderHeight;
                    capsule.isTrigger = true;
                    break;
                case RPGNpc.NPCColliderType.Sphere:
                    SphereCollider sphere = projGO.AddComponent<SphereCollider>();
                    sphere.center = rankREF.colliderCenter;
                    sphere.radius = rankREF.colliderRadius;
                    sphere.isTrigger = true;
                    break;
                case RPGNpc.NPCColliderType.Box:
                    BoxCollider box = projGO.AddComponent<BoxCollider>();
                    box.center = rankREF.colliderCenter;
                    box.size = rankREF.colliderSize;
                    box.isTrigger = true;
                    break;
            }

            return projHitRef;
        }

        public void SpawnHitEffect(CombatNode cbtNode, RPGAbility.RPGAbilityRankData rankREF)
        {
            Vector3 spawnPos = GetEffectSpawnPosition(cbtNode, rankREF.hitEffectUseSocket, rankREF.hitEffectPositionOffset, rankREF.hitEffectSocketName);
            var hitEffectGO = Instantiate(rankREF.hitEffect, spawnPos, Quaternion.identity);
            if (rankREF.hitAttachedToNode)
            {
                hitEffectGO.transform.SetParent(cbtNode.transform);
            }
            Destroy(hitEffectGO, rankREF.hitEffectDuration);
        }

        public Vector3 GetEffectSpawnPosition(CombatNode casterNode, bool useSocket, Vector3 POSOffset,
            string socketName)
        {
            if (!useSocket) return casterNode.transform.position + POSOffset;
            if (casterNode == playerCombatNode && casterNode.appearanceREF.isShapeshifted)
            {
                if (casterNode.appearanceREF.shapeshiftNodeSocketsREF != null)
                {
                    Transform shapeshiftSocketREF =
                        casterNode.appearanceREF.shapeshiftNodeSocketsREF.GetSocketByName(socketName);
                    return shapeshiftSocketREF != null
                        ? shapeshiftSocketREF.position
                        : casterNode.transform.position + POSOffset;
                }

                return casterNode.transform.position + POSOffset;
            }

            if (casterNode.nodeSocketsREF == null)
                return casterNode.transform.position + POSOffset;
            Transform socketREF =
                casterNode.nodeSocketsREF.GetSocketByName(socketName);
            return socketREF != null
                ? socketREF.position
                : casterNode.transform.position + POSOffset;
        }

        private Transform GetEffectTransform(CombatNode cbtNode, bool useSocket, string socketName)
        {
            if (!useSocket) return cbtNode.transform;
            
            if (cbtNode == playerCombatNode && cbtNode.appearanceREF.isShapeshifted)
            {
                if (cbtNode.appearanceREF.shapeshiftNodeSocketsREF != null)
                {
                    Transform shapeshiftSocketREF =
                        cbtNode.appearanceREF.shapeshiftNodeSocketsREF.GetSocketByName(socketName);
                    return shapeshiftSocketREF != null
                        ? shapeshiftSocketREF
                        : cbtNode.transform;
                }

                return cbtNode.transform;
            }
            
            if (cbtNode.nodeSocketsREF == null)
                return cbtNode.transform;
            Transform socketREF =
                cbtNode.nodeSocketsREF.GetSocketByName(socketName);
            return socketREF != null
                ? socketREF
                : cbtNode.transform;
        }

        private IEnumerator EXECUTE_PROJECTILE_ABILITY(CombatNode casterNode, CombatNode targetInfo, RPGAbility ab, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF.projectileEffect == null) yield break;
            float totalCount = rankREF.projectileCount +
                               GetTotalOfStatType(casterNode, RPGStat.STAT_TYPE.PROJECTILE_COUNT);
            if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
            {
                for (var i = 0; i < totalCount; i++)
                    if (casterNode.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
                    {
                        if (rankREF.projectileDelay > 0)
                            yield return new WaitForSeconds(rankREF.projectileDelay);
                        HandleProjectile(casterNode, targetInfo, casterNode.playerControllerEssentials.GETControllerType(), ab, rankREF, 0);
                    }
                    else
                    {
                        if (rankREF.projectileDelay > 0)
                            yield return new WaitForSeconds(rankREF.projectileDelay);
                        Vector3 spawnPos = GetEffectSpawnPosition(casterNode, rankREF.projectileUseNodeSocket, rankREF.effectPositionOffset, rankREF.projectileSocketName);
                        
                        var cbtVisualGO = Instantiate(rankREF.projectileEffect, spawnPos, Quaternion.identity);

                        ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                        projHitRef.InitProjectile(casterNode, ab, rankREF);

                        if (rankREF.projectileParentedToCaster)
                        {
                            cbtVisualGO.transform.SetParent(casterNode.transform);
                            cbtVisualGO.transform.localPosition = Vector3.zero;
                        }

                        projHitRef.transform.rotation = Quaternion.Euler(0,
                            casterNode.transform.rotation.eulerAngles.y, 0);

                        float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed *
                            (GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) / 100));
                        projHitRef.RB.velocity = projHitRef.transform.forward * totalProjectileSpeed;
                        
                        projHitRef.RB.useGravity = rankREF.projectileAffectedByGravity;
                        if (!rankREF.projectileAffectedByGravity)
                        {
                            projHitRef.RB.constraints = RigidbodyConstraints.FreezeRotationX |
                                                        RigidbodyConstraints.FreezeRotationY |
                                                        RigidbodyConstraints.FreezeRotationZ;
                        }

                        if (rankREF.projectileSound != null)
                            RPGBuilderUtilities.PlaySound(casterNode.gameObject, cbtVisualGO, rankREF.projectileSound,
                                rankREF.projectileSoundParentedToEffect);
                    }
            }
            else
            {
                if (rankREF.projectileAngleSpread > 0)
                {
                    float totalSpread = rankREF.projectileAngleSpread + (rankREF.projectileAngleSpread *
                                                                         (GetTotalOfStatType(casterNode,
                                                                             RPGStat.STAT_TYPE
                                                                                 .PROJECTILE_ANGLE_SPREAD) / 100));

                    var OFFSET = -(totalSpread / 2);
                    var projCountOnEachSide = (totalCount - 1) / 2;
                    var intervalOffset = totalSpread / 2 / projCountOnEachSide;

                    for (var i = 0; i < totalCount; i++)
                        if (casterNode.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
                        {
                            if (rankREF.projectileDelay > 0)
                                yield return new WaitForSeconds(rankREF.projectileDelay);
                            HandleProjectile(casterNode, targetInfo, casterNode.playerControllerEssentials.GETControllerType(), ab, rankREF,
                                OFFSET);
                            OFFSET += intervalOffset;
                        }
                        else
                        {
                            if (rankREF.projectileDelay > 0)
                                yield return new WaitForSeconds(rankREF.projectileDelay);
                            Vector3 spawnPos = GetEffectSpawnPosition(casterNode, rankREF.projectileUseNodeSocket, rankREF.effectPositionOffset, rankREF.projectileSocketName);
                            
                            var cbtVisualGO = Instantiate(rankREF.projectileEffect, spawnPos, Quaternion.identity);

                            ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                            projHitRef.InitProjectile(casterNode, ab, rankREF);

                            if (rankREF.projectileParentedToCaster)
                            {
                                cbtVisualGO.transform.SetParent(casterNode.transform);
                                cbtVisualGO.transform.localPosition = Vector3.zero;
                            }

                            // projHitRef.transform.rotation = Quaternion.Euler(0, casterNode.transform.rotation.eulerAngles.y + OFFSET, 0);
                           var transform1 = casterNode.agentREF.target.transform;
                           var position = transform1.position;
                           projHitRef.transform.LookAt(new Vector3(position.x, position.y + 1, position.z));
                           

                            float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed *
                                (GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) / 100));
                            projHitRef.RB.velocity = projHitRef.transform.forward * totalProjectileSpeed;
                            projHitRef.RB.useGravity = rankREF.projectileAffectedByGravity;
                            if (!rankREF.projectileAffectedByGravity)
                            {
                                projHitRef.RB.constraints = RigidbodyConstraints.FreezeRotationX |
                                                            RigidbodyConstraints.FreezeRotationY |
                                                            RigidbodyConstraints.FreezeRotationZ;
                            }
                            
                            OFFSET += intervalOffset;

                            if (rankREF.projectileSound != null)
                                RPGBuilderUtilities.PlaySound(casterNode.gameObject, cbtVisualGO,
                                    rankREF.projectileSound, rankREF.projectileSoundParentedToEffect);
                        }
                }
                else
                {
                    for (var i = 0; i < totalCount; i++)
                        if (casterNode.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
                        {
                            if (rankREF.projectileDelay > 0)
                                yield return new WaitForSeconds(rankREF.projectileDelay);
                            HandleProjectile(casterNode, targetInfo, casterNode.playerControllerEssentials.GETControllerType(), ab, rankREF, 0);
                        }
                        else
                        {
                            if (rankREF.projectileDelay > 0)
                                yield return new WaitForSeconds(rankREF.projectileDelay);
                            Vector3 spawnPos = GetEffectSpawnPosition(casterNode, rankREF.projectileUseNodeSocket, rankREF.effectPositionOffset, rankREF.projectileSocketName);
                            
                            var cbtVisualGO = Instantiate(rankREF.projectileEffect, spawnPos, Quaternion.identity);
                            
                            ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                            projHitRef.InitProjectile(casterNode, ab, rankREF);

                            if (rankREF.projectileParentedToCaster)
                            {
                                cbtVisualGO.transform.SetParent(casterNode.transform);
                                cbtVisualGO.transform.localPosition = Vector3.zero;
                            }

                            //projHitRef.transform.rotation = Quaternion.Euler(0, casterNode.transform.rotation.eulerAngles.y, 0);
                            var transform1 = casterNode.agentREF.target.transform;
                            var position = transform1.position;
                            projHitRef.transform.LookAt(new Vector3(position.x, position.y + 1, position.z));
                            
                            float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed *
                                (GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) / 100));
                            projHitRef.RB.velocity = projHitRef.transform.forward * totalProjectileSpeed;
                            projHitRef.RB.useGravity = rankREF.projectileAffectedByGravity;
                            if (!rankREF.projectileAffectedByGravity)
                            {
                                projHitRef.RB.constraints = RigidbodyConstraints.FreezeRotationX |
                                                            RigidbodyConstraints.FreezeRotationY |
                                                            RigidbodyConstraints.FreezeRotationZ;
                            }

                            if (rankREF.projectileSound != null)
                                RPGBuilderUtilities.PlaySound(casterNode.gameObject, cbtVisualGO,
                                    rankREF.projectileSound, rankREF.projectileSoundParentedToEffect);
                        }
                }
            }
        }

        private void HandleProjectile(CombatNode casterNode, CombatNode targetInfo, RPGGeneralDATA.ControllerTypes controllerType, RPGAbility ab, RPGAbility.RPGAbilityRankData rankREF, float OFFSET)
        {
            switch (controllerType)
            {
                case RPGGeneralDATA.ControllerTypes.TopDownClickToMove:
                case RPGGeneralDATA.ControllerTypes.TopDownWASD:
                {
                    Vector3 spawnPos = GetEffectSpawnPosition(casterNode, rankREF.projectileUseNodeSocket, rankREF.effectPositionOffset, rankREF.projectileSocketName);
                        
                    var cbtVisualGO = Instantiate(rankREF.projectileEffect, spawnPos, Quaternion.identity);
                    if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
                    {
                        ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                        projHitRef.targettedProjectileTarget = targetInfo;
                        projHitRef.targettedProjectileTargetTransform = GetEffectTransform(targetInfo,
                            rankREF.projectileTargetUseNodeSocket, rankREF.projectileTargetSocketName);
                        projHitRef.InitProjectile(casterNode, ab, rankREF);
                    }
                    else
                    {
                        if (!rankREF.projectileShootOnClickPosition)
                        {
                            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            var plane = new Plane(Vector3.up,
                                new Vector3(0, casterNode.transform.position.y + rankREF.effectPositionOffset.y, 0));
                            if (plane.Raycast(ray, out var distance))
                            {
                                var target = ray.GetPoint(distance);
                                var direction = target - casterNode.transform.position;
                                var rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                                ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                                projHitRef.InitProjectile(casterNode, ab, rankREF);

                                if (rankREF.projectileParentedToCaster)
                                {
                                    cbtVisualGO.transform.SetParent(casterNode.transform);
                                    cbtVisualGO.transform.localPosition = Vector3.zero;
                                }

                                projHitRef.transform.rotation = Quaternion.Euler(0, rotation + OFFSET, 0);
                                if (projHitRef.RB != null)
                                {
                                    projHitRef.RB.useGravity = rankREF.projectileAffectedByGravity;
                                    if (!rankREF.projectileAffectedByGravity)
                                    {
                                        projHitRef.RB.constraints = RigidbodyConstraints.FreezePositionY |
                                                                    RigidbodyConstraints.FreezeRotationX |
                                                                    RigidbodyConstraints.FreezeRotationY |
                                                                    RigidbodyConstraints.FreezeRotationZ;
                                    }

                                    float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed *
                                        (GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) /
                                         100));
                                    projHitRef.RB.velocity = projHitRef.transform.forward * totalProjectileSpeed;
                                }
                            }
                        }
                        else
                        {
                            var target = casterNode.currentProjectileClickPos;
                            var direction = target - casterNode.transform.position;
                            var rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                            ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                            projHitRef.InitProjectile(casterNode, ab, rankREF);

                            if (rankREF.projectileParentedToCaster)
                            {
                                cbtVisualGO.transform.SetParent(casterNode.transform);
                                cbtVisualGO.transform.localPosition = Vector3.zero;
                            }

                            projHitRef.transform.rotation = Quaternion.Euler(0, rotation + OFFSET, 0);
                            if (projHitRef.RB != null)
                            {
                                projHitRef.RB.useGravity = rankREF.projectileAffectedByGravity;
                                if (!rankREF.projectileAffectedByGravity)
                                {
                                    projHitRef.RB.constraints = RigidbodyConstraints.FreezePositionY |
                                                                RigidbodyConstraints.FreezeRotationX |
                                                                RigidbodyConstraints.FreezeRotationY |
                                                                RigidbodyConstraints.FreezeRotationZ;
                                }

                                float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed *
                                    (GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) /
                                     100));
                                projHitRef.RB.velocity = projHitRef.transform.forward * totalProjectileSpeed;
                            }
                        }
                    }

                    if (rankREF.projectileSound != null)
                        RPGBuilderUtilities.PlaySound(casterNode.gameObject, cbtVisualGO, rankREF.projectileSound,
                            rankREF.projectileSoundParentedToEffect);

                    break;
                }
                case RPGGeneralDATA.ControllerTypes.ThirdPersonShooter:
                case RPGGeneralDATA.ControllerTypes.ThirdPerson:
                {
                    Vector3 spawnPos = GetEffectSpawnPosition(casterNode, rankREF.projectileUseNodeSocket, rankREF.effectPositionOffset, rankREF.projectileSocketName);
                    
                    var cbtVisualGO = Instantiate(rankREF.projectileEffect, spawnPos, Quaternion.identity);
                    if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
                    {
                        ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                        projHitRef.targettedProjectileTarget = targetInfo;
                        projHitRef.targettedProjectileTargetTransform = GetEffectTransform(targetInfo,
                            rankREF.projectileTargetUseNodeSocket, rankREF.projectileTargetSocketName);
                        projHitRef.InitProjectile(casterNode, ab, rankREF);
                    }
                    else
                    {
                        ProjectileHitDetection projHitRef = InitProjectileComponents(cbtVisualGO, rankREF);
                        projHitRef.InitProjectile(casterNode, ab, rankREF);

                        if (rankREF.projectileParentedToCaster)
                        {
                            cbtVisualGO.transform.SetParent(casterNode.transform);
                            cbtVisualGO.transform.localPosition = Vector3.zero;
                        }

                        if (!casterNode.playerControllerEssentials.IsThirdPersonShooter())
                        {
                            projHitRef.transform.rotation = Quaternion.Euler(casterNode.transform.eulerAngles.x,
                                casterNode.transform.eulerAngles.y + OFFSET, casterNode.transform.eulerAngles.z);
                            if (projHitRef.RB != null)
                            {
                                projHitRef.RB.useGravity = rankREF.projectileAffectedByGravity;
                                if (!rankREF.projectileAffectedByGravity)
                                {
                                    projHitRef.RB.constraints = RigidbodyConstraints.FreezePositionY |
                                                                RigidbodyConstraints.FreezeRotationX |
                                                                RigidbodyConstraints.FreezeRotationY |
                                                                RigidbodyConstraints.FreezeRotationZ;
                                }

                                float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed *
                                    (GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) / 100));
                                projHitRef.RB.velocity = projHitRef.transform.forward * totalProjectileSpeed;
                            }
                        }
                        else
                        {
                            Vector3 v3LookPoint;
                            var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                            RaycastHit hit;

                            v3LookPoint = Physics.Raycast(ray, out hit, 300,
                                ProjectileCheckLayer)
                                ? hit.point
                                : ray.GetPoint(300);

                            projHitRef.transform.LookAt(v3LookPoint);
                            projHitRef.transform.rotation = Quaternion.Euler(projHitRef.transform.eulerAngles.x,
                                projHitRef.transform.eulerAngles.y + OFFSET, projHitRef.transform.eulerAngles.z);
                            if (projHitRef.RB != null)
                            {
                                projHitRef.RB.useGravity = rankREF.projectileAffectedByGravity;
                                if (!rankREF.projectileAffectedByGravity)
                                {
                                    projHitRef.RB.constraints = RigidbodyConstraints.FreezeRotationX |
                                                                RigidbodyConstraints.FreezeRotationY |
                                                                RigidbodyConstraints.FreezeRotationZ;
                                }

                                float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed *
                                    (GetTotalOfStatType(playerCombatNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED) / 100));
                                projHitRef.RB.AddRelativeForce(
                                    projHitRef.transform.forward * (totalProjectileSpeed * 50));
                            }
                        }
                    }

                    if (rankREF.projectileSound != null)
                        RPGBuilderUtilities.PlaySound(casterNode.gameObject, cbtVisualGO, rankREF.projectileSound,
                            rankREF.projectileSoundParentedToEffect);

                    break;
                }
            }
        }

        public void EXECUTE_PROJECTILE_ABILITY_HIT(CombatNode casterInfo, CombatNode targetInfo,
            RPGAbility.RPGAbilityRankData rankREF)
        {
            ExecuteAbilityEffects(casterInfo, targetInfo, rankREF);
        }

        public void ExecuteDOTPulse(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank, int curStack, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (targetInfo.dead) return;
            var dmg = damageCalculation(casterInfo, targetInfo, effect, effectRank, rankREF, true, curStack);
            dmg /= effect.pulses;
            dmg *= curStack;
            if (!targetInfo.isImmune())
            {
                targetInfo.TakeDamage(casterInfo, dmg, rankREF, effect.ranks[effectRank].alteredStatID);
                if(targetInfo.isStealthed() && effect.ranks[effectRank].removeStealth) CancelStealth(targetInfo);
                
                if (effect.ranks[effectRank].alteredStatID == RPGBuilderEssentials.Instance.combatSettings.healthStatID)
                {
                    HandleLifesteal(casterInfo, targetInfo, effect, effectRank, dmg);
                    if(dmg > 0)HandleThorn(casterInfo, targetInfo, dmg);
                }
                HandlePetDefendOwner(targetInfo, casterInfo);
            }
            
            HandleMobAggro(casterInfo, targetInfo);
            
            ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Activate, targetInfo, effect.ranks[effectRank].visualEffects);
        }

        private void HandlePetDefendOwner(CombatNode owner, CombatNode attacker)
        {
            if (owner.currentPets.Count <= 0) return;
            foreach (var node in owner.currentPets)
                if (node.agentREF.target == null)
                    node.agentREF.SetTarget(attacker);
        }

        public void ExecuteHOTPulse(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank, int curStack)
        {
            if (targetInfo.dead) return;
            var heal = healingCalculation(casterInfo, targetInfo, effect, effectRank,0,false);
            heal /= effect.pulses;
            heal *= curStack;
            HandleCombatTextTrigger(casterInfo, targetInfo, heal, 0,"HEAL");
            targetInfo.Heal(heal, effect.ranks[effectRank].alteredStatID);
            
            ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Activate, targetInfo, effect.ranks[effectRank].visualEffects);
        }

        private void TriggerUpdateStatUI(CombatNode nodeInfo, RPGStat attribute)
        {
            if (nodeInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.player) return;

            if (attribute._name == RPGBuilderEssentials.Instance.healthStatReference._name)
            {
                StatBarUpdate(RPGBuilderEssentials.Instance.healthStatReference._name);
            }

            if (CharacterPanelDisplayManager.Instance.thisCG.alpha == 1 &&
                CharacterPanelDisplayManager.Instance.curCharInfoType ==
                CharacterPanelDisplayManager.characterInfoTypes.stats)
                CharacterPanelDisplayManager.Instance.InitCharStats();
        }

        public void RemoveStatModification(CombatNode targetInfo, RPGStat stat, float statModValue)
        {
            float newValue;
            if (stat.isVitalityStat)
            {
                newValue = targetInfo.getCurrentMaxValue(stat._name);
                newValue -= statModValue;
                targetInfo.setCurrentMaxValue(stat._name, newValue);
            }
            else
            {
                newValue = targetInfo.getCurrentValue(stat._name);
                newValue -= statModValue;
                targetInfo.setCurrentValue(stat._name, newValue);
            }

            TriggerUpdateStatUI(targetInfo, stat);
        }

        private void HandleLifesteal(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank, int dmg)
        {
            // LIFESTEAL
            var lifesteal = getLifesteal(casterInfo, effect, effectRank, dmg);
            if (lifesteal <= 0) return;
            casterInfo.Heal(lifesteal, RPGBuilderEssentials.Instance.combatSettings.healthStatID);
            HandleCombatTextTrigger(casterInfo == playerCombatNode ? casterInfo : targetInfo, casterInfo, lifesteal, 0,
                "HEAL");
        }

        private void HandleThorn(CombatNode casterInfo, CombatNode targetInfo, int dmg)
        {
            if (playerCombatNode == null || casterInfo == null || targetInfo == null) return;
            float thorn = GetTotalOfStatType(targetInfo, RPGStat.STAT_TYPE.THORN);
            if (isPet(targetInfo))
            {
                thorn += GetTotalOfStatType(targetInfo.ownerCombatInfo, RPGStat.STAT_TYPE.MINION_THORN);
            }
            
            if (!(thorn > 0)) return;
            thorn /= 100;
            var thornDamage = (int) (dmg * thorn);
            if (thornDamage == 0) return;
            if (!targetInfo.isImmune())
            {
                casterInfo.TakeDamage(targetInfo, thornDamage, null, RPGBuilderEssentials.Instance.combatSettings.healthStatID);
                if(targetInfo.isStealthed()) CancelStealth(targetInfo);
                HandlePetDefendOwner(casterInfo, targetInfo);
                if (casterInfo == playerCombatNode || targetInfo == playerCombatNode || playerCombatNode.currentPets.Contains(casterInfo))
                    ScreenTextDisplayManager.Instance.ScreenEventHandler("THORN", thornDamage.ToString(), "", casterInfo.gameObject);
            }
            else
            {
                if (casterInfo == playerCombatNode || targetInfo == playerCombatNode ||
                    playerCombatNode.currentPets.Contains(casterInfo))
                    ScreenTextDisplayManager.Instance.ScreenEventHandler("IMMUNE", "", "", casterInfo.gameObject);
            }
        }

        private void HandleMobAggro(CombatNode casterInfo, CombatNode targetInfo)
        {
            if (casterInfo.dead) return;
            if (targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.mob &&
                targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet) return;
            HandleNPCAggroLinks(casterInfo, targetInfo);
            if (targetInfo.agentREF == null || targetInfo.agentREF.target != null) return;
            targetInfo.agentREF.AlterThreatTable(casterInfo, 0);
            targetInfo.agentREF.SetTarget(casterInfo);
        }

        private void HandleNPCAggroLinks(CombatNode casterInfo, CombatNode targetInfo)
        {
            foreach (var aggroLink in targetInfo.npcDATA.aggroLinks)
            {
                Collider[] colliders = Physics.OverlapSphere(targetInfo.transform.position, aggroLink.maxDistance);
                foreach (Collider t in colliders)
                {
                    CombatNode thisCbtNode = t.GetComponent<CombatNode>();
                    if (thisCbtNode == null || thisCbtNode == targetInfo || thisCbtNode.npcDATA == null || thisCbtNode.agentREF == null) continue;
                    if (casterInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.player ||
                        (targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.mob &&
                         targetInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet)) continue;
                    if(thisCbtNode.npcDATA.ID != aggroLink.npcID) continue;
                    if(thisCbtNode.agentREF.target != null) continue;
                    if (FactionManager.Instance.getNPCAlignmentToPlayer(
                            RPGBuilderUtilities.GetFactionFromID(thisCbtNode.npcDATA.factionID)) ==
                        RPGCombatDATA.ALIGNMENT_TYPE.ALLY) continue;
                    if(!casterInfo.dead) thisCbtNode.agentREF.AlterThreatTable(casterInfo, 1);
                }
            }
        }

        public void HandleFactionChangeAggro()
        {
            foreach (var cbtNode in allCombatNodes)
            {
                if(cbtNode == playerCombatNode) continue;
                if(cbtNode.agentREF.target != playerCombatNode) continue;
                
                if (FactionManager.Instance.getNPCAlignmentToPlayer(
                        RPGBuilderUtilities.GetFactionFromID(cbtNode.npcDATA.factionID)) ==
                    RPGCombatDATA.ALIGNMENT_TYPE.ALLY)
                {
                    cbtNode.agentREF.ResetTarget();
                }
            }
        }

        private void HandleCombatTextTrigger(CombatNode casterInfo, CombatNode targetInfo, int dmg, int dmgBlocked, string CombatTextType)
        {
            if (playerCombatNode == null) return;
            if (casterInfo == playerCombatNode || targetInfo == playerCombatNode ||
                playerCombatNode.currentPets.Contains(casterInfo))
                ScreenTextDisplayManager.Instance.ScreenEventHandler(CombatTextType, dmg.ToString(), dmgBlocked.ToString(), targetInfo.gameObject);
        }

        private bool VitalityActionConditionMet(CombatNode cbtNode, RPGStat.VitalityActions vitalityAction,
            int statIndex)
        {
            switch (vitalityAction.valueType)
            {
                case RPGStat.VitalityActionsValueType.Equal:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 == vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue == vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.Above:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 > vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue > vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.Below:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 < vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue < vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.EqualOrAbove:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 >= vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue >= vitalityAction.value;
                    }
                case RPGStat.VitalityActionsValueType.EqualOrBelow:
                    if (vitalityAction.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 <= vitalityAction.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue <= vitalityAction.value;
                    }
            }

            return false;
        }
        
        private bool VitalityActionConditionMet(CombatNode cbtNode, RPGAbility.ConditionalEffectsData conditionalData,
            int statIndex)
        {
            switch (conditionalData.valueType)
            {
                case RPGStat.VitalityActionsValueType.Equal:
                    if (conditionalData.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 == conditionalData.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue == conditionalData.value;
                    }
                case RPGStat.VitalityActionsValueType.Above:
                    if (conditionalData.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 > conditionalData.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue > conditionalData.value;
                    }
                case RPGStat.VitalityActionsValueType.Below:
                    if (conditionalData.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 < conditionalData.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue < conditionalData.value;
                    }
                case RPGStat.VitalityActionsValueType.EqualOrAbove:
                    if (conditionalData.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 >= conditionalData.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue >= conditionalData.value;
                    }
                case RPGStat.VitalityActionsValueType.EqualOrBelow:
                    if (conditionalData.isPercent)
                    {
                        return (cbtNode.nodeStats[statIndex].curValue / cbtNode.nodeStats[statIndex].curMaxValue) *
                            100 <= conditionalData.value;
                    }
                    else
                    {
                        return cbtNode.nodeStats[statIndex].curValue <= conditionalData.value;
                    }
            }

            return false;
        }
        
        

        public void HandleVitalityStatActions(CombatNode cbtNode, RPGStat statREF, int statIndex)
        {
            List<RPGStat.VitalityActions> allVitActions = new List<RPGStat.VitalityActions>(statREF.vitalityActions);

            switch (cbtNode.nodeType)
            {
                case CombatNode.COMBAT_NODE_TYPE.mob:
                case CombatNode.COMBAT_NODE_TYPE.pet:
                case CombatNode.COMBAT_NODE_TYPE.objectAction:
                    if (cbtNode.npcDATA != null)
                    {
                        foreach (var stat in cbtNode.npcDATA.stats)
                        {
                            if(stat.statID != statREF.ID) continue;
                            foreach (var extraVitAction in stat.vitalityActions)
                            {
                                allVitActions.Insert(0, extraVitAction);
                            }
                        }

                        RPGSpecies speciesREF = RPGBuilderUtilities.GetSpeciesFromID(cbtNode.npcDATA.speciesID);
                        if (speciesREF != null)
                        {
                            foreach (var stat in speciesREF.stats)
                            {
                                if(stat.statID != statREF.ID) continue;
                                foreach (var extraVitAction in stat.vitalityActions)
                                {
                                    allVitActions.Insert(0, extraVitAction);
                                }
                            }
                        }
                    }
                    break;
            }

            CombatNode cachedCombatNode = cbtNode;
            bool shouldDie = false;
            foreach (RPGStat.VitalityActions t in allVitActions)
            {
                if (!VitalityActionConditionMet(cbtNode, t, statIndex)) continue;
                if(t.type != RPGStat.VitalityActionsTypes.Death) continue;
                shouldDie = true;
            }

            if (shouldDie)
            {
                cbtNode.DEATH();
            }

            foreach (RPGStat.VitalityActions t in allVitActions)
            {
                if (!VitalityActionConditionMet(cbtNode, t, statIndex)) continue;
                switch (t.type)
                {
                    case RPGStat.VitalityActionsTypes.Effect:
                        RPGEffect eff = RPGBuilderUtilities.GetEffectFromID(t.elementID);
                        if (eff == null) break;
                        ExecuteEffect(cachedCombatNode, cachedCombatNode, eff, t.effectRank, null, 0);
                        break;
                    case RPGStat.VitalityActionsTypes.Ability:
                        InitExtraAbility(cachedCombatNode, RPGBuilderUtilities.GetAbilityFromID(t.elementID));
                        break;
                    case RPGStat.VitalityActionsTypes.ResetActiveBlock:
                        cachedCombatNode.ResetActiveBlocking();
                        break;
                    case RPGStat.VitalityActionsTypes.ResetSprint:
                        if(cachedCombatNode == playerCombatNode) cachedCombatNode.playerControllerEssentials.EndSprint();
                        break;
                }
            }
        }

        public void CheckIfPetsShouldAttack(CombatNode attacker, CombatNode attacked)
        {
            foreach (var pet in attacker.currentPets)
            {
                if(pet.agentREF.target == null) pet.agentREF.SetTarget(attacked);
            }
        }

        private IEnumerator EFFECTS_LOGIC(RPGEffect effect, int effectRank, CombatNode casterInfo,
            CombatNode targetInfo, RPGAbility.RPGAbilityRankData rankREF, float delay)
        {
            if (targetInfo.dead) yield break;

            yield return new WaitForSeconds(delay);
            
            if (targetInfo.dead) yield break;

            switch (effect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.InstantDamage:
                    var dmg = damageCalculation(casterInfo, targetInfo, effect, effectRank, rankREF, false, 0);
                    if (!targetInfo.isImmune())
                    {
                        targetInfo.TakeDamage(casterInfo, dmg, rankREF, effect.ranks[effectRank].alteredStatID);
                        HandlePetDefendOwner(targetInfo, casterInfo);
                        if(targetInfo.isStealthed() && effect.ranks[effectRank].removeStealth) CancelStealth(targetInfo);
                        if (effect.ranks[effectRank].alteredStatID == RPGBuilderEssentials.Instance.combatSettings.healthStatID)
                            HandleLifesteal(casterInfo, targetInfo, effect, effectRank, dmg);
                    }

                    HandleMobAggro(casterInfo, targetInfo);
                    if (dmg > 0 && effect.ranks[effectRank].alteredStatID ==
                        RPGBuilderEssentials.Instance.combatSettings.healthStatID)
                        HandleThorn(casterInfo, targetInfo, dmg);
                    break;
                case RPGEffect.EFFECT_TYPE.InstantHeal:
                    var heal = healingCalculation(casterInfo, targetInfo, effect, effectRank, 0, true);
                    targetInfo.Heal(heal, effect.ranks[effectRank].alteredStatID);
                    break;
                case RPGEffect.EFFECT_TYPE.Teleport:
                    switch (effect.ranks[effectRank].teleportType)
                    {
                        case RPGEffect.TELEPORT_TYPE.gameScene:
                            if (targetInfo == playerCombatNode)
                                RPGBuilderEssentials.Instance.TeleportToGameScene(effect.ranks[effectRank].gameSceneID,
                                    effect.ranks[effectRank].teleportPOS);
                            break;
                        case RPGEffect.TELEPORT_TYPE.position:
                            if (targetInfo == playerCombatNode)
                                playerCombatNode.playerControllerEssentials.TeleportToTarget(effect.ranks[effectRank]
                                    .teleportPOS);
                            break;
                        case RPGEffect.TELEPORT_TYPE.target:
                            if (PlayerTargetData.currentTarget != null && casterInfo == playerCombatNode)
                                playerCombatNode.playerControllerEssentials.TeleportToTarget(targetInfo);
                            break;
                        case RPGEffect.TELEPORT_TYPE.directional:
                            if (targetInfo == playerCombatNode)
                            {
                                RaycastHit obstacle;
                                Vector3 targetPOS = playerCombatNode.transform.position;
                                switch (effect.ranks[effectRank].teleportDirectionalType)
                                {
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.forward:
                                        targetPOS += playerCombatNode.transform.forward *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.left:
                                        targetPOS += -playerCombatNode.transform.right *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.right:
                                        targetPOS += playerCombatNode.transform.right *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.backward:
                                        targetPOS += -playerCombatNode.transform.forward *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopLeft:
                                        targetPOS += playerCombatNode.transform.forward *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        targetPOS += -playerCombatNode.transform.right *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopRight:
                                        targetPOS += playerCombatNode.transform.forward *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        targetPOS += playerCombatNode.transform.right *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardLeft:
                                        targetPOS += -playerCombatNode.transform.forward *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        targetPOS += -playerCombatNode.transform.right *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                    case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardRight:
                                        targetPOS += -playerCombatNode.transform.forward *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        targetPOS += playerCombatNode.transform.right *
                                                     effect.ranks[effectRank].teleportDirectionalDistance;
                                        break;
                                }

                                if (Physics.Linecast(new Vector3(playerCombatNode.transform.position.x,
                                        playerCombatNode.transform.position.y + 1f,
                                        playerCombatNode.transform.position.z), targetPOS, out obstacle,
                                    effect.ranks[effectRank].teleportDirectionalBlockLayers))
                                {
                                    targetPOS = playerCombatNode.transform.position;
                                    switch (effect.ranks[effectRank].teleportDirectionalType)
                                    {
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.forward:
                                            targetPOS += playerCombatNode.transform.forward * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.left:
                                            targetPOS += -playerCombatNode.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.right:
                                            targetPOS += playerCombatNode.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.backward:
                                            targetPOS += -playerCombatNode.transform.forward * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopLeft:
                                            targetPOS += playerCombatNode.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += -playerCombatNode.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalTopRight:
                                            targetPOS += playerCombatNode.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += playerCombatNode.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardLeft:
                                            targetPOS += -playerCombatNode.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += -playerCombatNode.transform.right * (obstacle.distance - 1f);
                                            break;
                                        case RPGEffect.TELEPORT_DIRECTIONAL_TYPE.diagonalBackwardRight:
                                            targetPOS += -playerCombatNode.transform.forward * (obstacle.distance - 1f);
                                            targetPOS += playerCombatNode.transform.right * (obstacle.distance - 1f);
                                            break;
                                    }

                                    if (obstacle.point.y > playerCombatNode.transform.position.y)
                                    {
                                        targetPOS.y += (obstacle.point.y - playerCombatNode.transform.position.y) *
                                                       1.1f;
                                    }
                                }

                                playerCombatNode.playerControllerEssentials.TeleportToTarget(targetPOS);
                            }

                            break;
                    }

                    break;
                case RPGEffect.EFFECT_TYPE.Pet:
                    var maxSummonCount = getCurrentSummonCount(casterInfo);
                    for (var x = 0; x < effect.ranks[effectRank].petSPawnCount; x++)
                        if (casterInfo.currentPets.Count < maxSummonCount)
                        {
                            CombatNode petNode =
                                SetupNPCPrefab(RPGBuilderUtilities.GetNPCFromID(effect.ranks[effectRank].petNPCDataID),
                                    true, effect.ranks[effectRank].petScaleWithCharacter, casterInfo,
                                    GetPetSpawnPosition(casterInfo), casterInfo.transform.rotation);
                            if (casterInfo != playerCombatNode)
                                casterInfo.currentPetsCombatActionType = CombatNode.PET_COMBAT_ACTION_TYPES.aggro;

                            float totalDuration = effect.ranks[effectRank].petDuration +
                                                  (effect.ranks[effectRank].petDuration *
                                                      GetTotalOfStatType(casterInfo,
                                                          RPGStat.STAT_TYPE.MINION_DURATION) / 100);
                            StartCoroutine(destroyPet(totalDuration, petNode));
                        }
                        else
                        {
                            break;
                        }

                    if (casterInfo == playerCombatNode)
                    {
                        if (casterInfo.currentPets.Count == 1)
                        {
                            PetPanelDisplayManager.Instance.Show();
                        }
                        else if (casterInfo.currentPets.Count > 1)
                        {
                            if (PetPanelDisplayManager.Instance.thisCG.alpha == 0)
                            {
                                PetPanelDisplayManager.Instance.Show();
                            }
                            else
                            {
                                PetPanelDisplayManager.Instance.UpdateSummonCountText();
                                PetPanelDisplayManager.Instance.UpdateHealthBar();
                            }
                        }
                    }

                    break;
                case RPGEffect.EFFECT_TYPE.RollLootTable:
                    var lootData = new List<LootBagHolder.Loot_Data>();
                    float LOOTCHANCEMOD = GetTotalOfStatType(playerCombatNode,
                        RPGStat.STAT_TYPE.LOOT_CHANCE_MODIFIER);
                    var lootTableREF = RPGBuilderUtilities.GetLootTableFromID(effect.ranks[effectRank].lootTableID);
                    foreach (var t1 in lootTableREF.lootItems)
                    {
                        var itemDropAmount = Random.Range(0f, 100f);
                        if (LOOTCHANCEMOD > 0) itemDropAmount += itemDropAmount * (LOOTCHANCEMOD / 100);
                        if (!(itemDropAmount <= t1.dropRate)) continue;
                        var stack = t1.min == t1.max ? t1.min : Random.Range(t1.min, t1.max + 1);

                        RPGItem itemREF = RPGBuilderUtilities.GetItemFromID(t1.itemID);
                        var newLoot = new LootBagHolder.Loot_Data();
                        newLoot.item = itemREF;
                        newLoot.count = stack;
                        newLoot.itemDataID =
                            RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID, CharacterData.ItemDataState.world);

                        lootData.Add(newLoot);
                    }

                    foreach (var t in lootData)
                    {
                        if (t.looted) continue;
                        int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(t.item.ID, t.count, false, true);
                        if (itemsLeftOver == 0)
                        {
                            RPGBuilderUtilities.SetNewItemDataState(t.itemDataID, CharacterData.ItemDataState.inBag);
                            t.looted = true;
                        }
                        else
                        {
                            t.count = itemsLeftOver;
                        }
                    }

                    ItemTooltip.Instance.Hide();

                    break;
                case RPGEffect.EFFECT_TYPE.Knockback:
                    if (targetInfo.CanActiveBlockThis(casterInfo) && targetInfo.curBlockEffect.isBlockKnockback) yield break;
                        switch (targetInfo.nodeType)
                        {
                            case CombatNode.COMBAT_NODE_TYPE.mob:
                            case CombatNode.COMBAT_NODE_TYPE.pet:
                                targetInfo.agentREF.InitKnockback(effect.ranks[effectRank].knockbackDistance, casterInfo.transform);
                                break;
                            case CombatNode.COMBAT_NODE_TYPE.player:
                                targetInfo.playerControllerEssentials.InitKnockback(
                                    effect.ranks[effectRank].knockbackDistance, casterInfo.transform);
                                break;
                        }
                    HandleMobAggro(casterInfo, targetInfo);

                    break;
                case RPGEffect.EFFECT_TYPE.Motion:
                    switch (targetInfo.nodeType)
                    {
                        case CombatNode.COMBAT_NODE_TYPE.mob:
                        case CombatNode.COMBAT_NODE_TYPE.pet:
                            
                            break;
                        case CombatNode.COMBAT_NODE_TYPE.player:
                            if (playerCombatNode.playerControllerEssentials.motionActive) yield break;
                            if(!effect.ranks[effectRank].motionIgnoreUseCondition && !CombatNodeCanInitMotion(targetInfo)) yield break;
                            targetInfo.playerControllerEssentials.InitMotion(effect.ranks[effectRank].motionDistance,
                                effect.ranks[effectRank].motionDirection, effect.ranks[effectRank].motionSpeed, effect.ranks[effectRank].isImmuneDuringMotion);
                            break;
                    }

                    break;
                case RPGEffect.EFFECT_TYPE.Blocking:
                    if (!targetInfo.CanActiveBlockThis(casterInfo))
                    {
                        switch (targetInfo.nodeType)
                        {
                            case CombatNode.COMBAT_NODE_TYPE.mob:
                            case CombatNode.COMBAT_NODE_TYPE.pet:

                                break;
                            case CombatNode.COMBAT_NODE_TYPE.player:
                                ActiveBlockingDisplayManager.Instance.icon.sprite = effect.icon;
                                targetInfo.InitActiveBlocking(effect.ranks[effectRank]);
                                break;
                        }
                    }

                    break;
                
                case RPGEffect.EFFECT_TYPE.Dispel:
                    List<int> effectsDispelledIndexes = new List<int>();
                    switch (effect.ranks[effectRank].dispelType)
                    {
                        case RPGEffect.DISPEL_TYPE.EffectType:
                            for (var index = 0; index < targetInfo.nodeStateData.Count; index++)
                            {
                                var state = targetInfo.nodeStateData[index];
                                if (state.stateEffect.effectType != effect.ranks[effectRank].dispelEffectType) continue;
                                effectsDispelledIndexes.Add(index);
                            }

                            break;
                        case RPGEffect.DISPEL_TYPE.EffectTag:
                            for (var index = 0; index < targetInfo.nodeStateData.Count; index++)
                            {
                                var state = targetInfo.nodeStateData[index];
                                if (state.stateEffect.effectTag != effect.ranks[effectRank].dispelEffectTag) continue;
                                effectsDispelledIndexes.Add(index);
                            }

                            break;
                        case RPGEffect.DISPEL_TYPE.Effect:
                            for (var index = 0; index < targetInfo.nodeStateData.Count; index++)
                            {
                                var state = targetInfo.nodeStateData[index];
                                if (state.stateEffect.ID != effect.ranks[effectRank].dispelEffectID) continue;
                                effectsDispelledIndexes.Add(index);
                            }
                            break;
                    }
                    
                    DispelEffects(targetInfo, effectsDispelledIndexes);
                    break;
            }
            
            ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType.Activate, targetInfo,
                effect.ranks[effectRank].visualAnimations);
            ExecuteCombatVisualEffectList(RPGCombatDATA.CombatVisualActivationType.Activate, targetInfo,
                effect.ranks[effectRank].visualEffects);
        }

        private Vector3 GetPetSpawnPosition(CombatNode owner)
        {
            Vector3 sourcePos = new Vector3(owner.transform.position.x, owner.transform.position.y + 0.25f,
                owner.transform.position.z);
            if (!Physics.Raycast(sourcePos, -owner.transform.right, 2f,
                ProjectileDestroyLayer))
            {
                return owner.transform.position + (-owner.transform.right * 1.5f);
            }
            
            if (!Physics.Raycast(sourcePos, owner.transform.right, 2f,
                ProjectileDestroyLayer))
            {
                return owner.transform.position + (owner.transform.right * 1.5f);
            }
            
            if (!Physics.Raycast(sourcePos, owner.transform.forward, 2f,
                ProjectileDestroyLayer))
            {
                return owner.transform.position + (owner.transform.forward * 1.5f);
            }
            
            if (!Physics.Raycast(sourcePos, -owner.transform.forward, 2f,
                ProjectileDestroyLayer))
            {
                return owner.transform.position + (-owner.transform.forward * 1.5f);
            }

            return owner.transform.position;
        }

        private void DispelEffects(CombatNode targetInfo, List<int> indexes)
        {
            foreach (var index in indexes)
            {
                targetInfo.RemoveEffectByIndex(index);
            }
        }

        private IEnumerator destroyPet(float duration, CombatNode nodeREF)
        {
            yield return  new WaitForSeconds(duration);
            
            HandleCombatNodeDEATH(nodeREF);
        }

        private IEnumerator HandleSpreadEffect(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank,
            int maxUnits, float maxDistance, RPGAbility.RPGAbilityRankData rankREF, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            float totalRadius = maxDistance +
                                (maxDistance * (GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.AOE_RADIUS) / 100));

            List<CombatNode> allCbtNodes =
                getPotentialCombatNodes(Physics.OverlapSphere(targetInfo.transform.position, totalRadius), casterInfo,
                    rankREF);
            if (allCbtNodes.Contains(targetInfo)) allCbtNodes.Remove(targetInfo);
            int totalUnitHit = maxUnits + (int) GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
            var closestUnits = getClosestUnits(targetInfo, allCbtNodes, totalUnitHit);
            foreach (var t in closestUnits)
            {
                if (effect.isState)
                    StartCoroutine(InitNodeState(casterInfo, t, effect, effectRank, effect.icon, 0));
                else
                    StartCoroutine(EFFECTS_LOGIC(effect, effectRank, casterInfo, t, rankREF, 0));
            }
        }

        bool isEffectActiveOnTarget(RPGEffect effectRequired, CombatNode target)
        {
            foreach (var effect in target.nodeStateData)
            {
                if (effect.stateEffect == effectRequired) return true;
            }

            return false;
        }

        private void ExecuteAbilityEffects(CombatNode casterInfo, CombatNode targetInfo, RPGAbility.RPGAbilityRankData rankREF)
        {

            foreach (var t in rankREF.conditionalEffects)
            {
                if (t.effectID == -1) continue;
                
                CombatNode checkedNode =
                    t.requiredTargetType == RPGCombatDATA.TARGET_TYPE.Caster ? casterInfo : targetInfo;
                if (t.effectRequiredID != -1 &&
                    !isEffectActiveOnTarget(RPGBuilderUtilities.GetEffectFromID(t.effectRequiredID), checkedNode))
                    continue;
                if (t.statID != -1 && !VitalityActionConditionMet(checkedNode, t,
                    checkedNode.getStatIndexFromName(RPGBuilderUtilities.GetStatFromID(t.statID)._name))) continue;

                var thisEffect = RPGBuilderUtilities.GetEffectFromID(t.effectID);
                CombatNode targetNode = t.targetType == RPGCombatDATA.TARGET_TYPE.Caster ? casterInfo : targetInfo;
                if (thisEffect.isState)
                    StartCoroutine(InitNodeState(casterInfo, targetNode, thisEffect, t.effectRank, thisEffect.icon, t.delay));
                else
                    StartCoroutine(EFFECTS_LOGIC(thisEffect, t.effectRank, casterInfo, targetNode, rankREF, t.delay));

                if (t.isSpread)
                {
                    StartCoroutine(HandleSpreadEffect(casterInfo, targetInfo, thisEffect, t.effectRank, t.spreadUnitMax, t.spreadDistanceMax, rankREF, t.delay));
                }

                if (t.effectRequiredID != -1 && t.consumeRequiredEffect)
                {
                    targetInfo.CancelState(targetInfo.getStateIndexFromEffectID(t.effectRequiredID));
                }
            }
            
            foreach (var t in rankREF.effectsApplied)
            {
                var thisEffect = RPGBuilderUtilities.GetEffectFromID(t.effectID);
                var rdmEffectChance = Random.Range(0f, 100f);
                if (!(rdmEffectChance <= t.chance)) continue;
                if (thisEffect.isState)
                    StartCoroutine(InitNodeState(casterInfo, t.target == RPGCombatDATA.TARGET_TYPE.Target ? targetInfo : casterInfo,
                        thisEffect, t.effectRank, thisEffect.icon, t.delay));
                else
                    StartCoroutine(EFFECTS_LOGIC(thisEffect, t.effectRank, casterInfo,
                        t.target == RPGCombatDATA.TARGET_TYPE.Target ? targetInfo : casterInfo, rankREF, t.delay));

                if (t.isSpread)
                {
                    StartCoroutine(HandleSpreadEffect(casterInfo, targetInfo, thisEffect, t.effectRank, t.spreadUnitMax,
                        t.spreadDistanceMax, rankREF, t.delay));
                }
            }
        }

        private void ExecuteCasterEffects(CombatNode casterInfo, RPGAbility.RPGAbilityRankData rankREF)
        {
            foreach (var t in rankREF.casterEffectsApplied)
            {
                var thisEffect = RPGBuilderUtilities.GetEffectFromID(t.effectID);
                var rdmEffectChance = Random.Range(0f, 100f);
                if (!(rdmEffectChance <= t.chance)) continue;
                if (thisEffect.isState)
                    StartCoroutine(InitNodeState(casterInfo, casterInfo,
                        thisEffect, t.effectRank, thisEffect.icon, t.delay));
                else
                    StartCoroutine(EFFECTS_LOGIC(thisEffect, t.effectRank, casterInfo,
                        casterInfo, rankREF, t.delay));
            }
        }

        public void ExecuteEffect(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank, RPGAbility.RPGAbilityRankData rankREF, float delay)
        {
            if (effect.isState)
                StartCoroutine(InitNodeState(casterInfo, targetInfo, effect, effectRank, effect.icon, delay));
            else
                StartCoroutine(EFFECTS_LOGIC(effect, effectRank, casterInfo, targetInfo, rankREF, delay));
        }

        public void ShapeshiftPlayer(RPGEffect effect, int effectRank)
        {
            if (playerCombatNode.appearanceREF.isShapeshifted)
            {
                ResetPlayerShapeshift();
            }
            
            if (effect.ranks[effectRank].shapeshiftingModel == null) return;
            GameObject shapeshiftModel = Instantiate(effect.ranks[effectRank].shapeshiftingModel,
                playerCombatNode.transform);
            shapeshiftModel.transform.localPosition = effect.ranks[effectRank].shapeshiftingmodelPosition;
            shapeshiftModel.transform.localScale = effect.ranks[effectRank].shapeshiftingmodelScale;

            playerCombatNode.appearanceREF.isShapeshifted = true;
            playerCombatNode.appearanceREF.shapeshiftModel = shapeshiftModel;
            playerCombatNode.appearanceREF.shapeshiftNodeSocketsREF = shapeshiftModel.GetComponent<NodeSockets>();

            playerCombatNode.appearanceREF.cachedBodyParent.SetActive(false);
            playerCombatNode.appearanceREF.cachedArmorsParent.SetActive(false);

            playerCombatNode.playerControllerEssentials.anim.runtimeAnimatorController =
                effect.ranks[effectRank].shapeshiftingAnimatorController;
            playerCombatNode.playerControllerEssentials.anim.avatar =
                effect.ranks[effectRank].shapeshiftingAnimatorAvatar;
            playerCombatNode.playerControllerEssentials.anim.applyRootMotion =
                effect.ranks[effectRank].shapeshiftingAnimatorUseRootMotion;
            playerCombatNode.playerControllerEssentials.anim.updateMode =
                effect.ranks[effectRank].shapeshiftingAnimatorUpdateMode;
            playerCombatNode.playerControllerEssentials.anim.cullingMode =
                effect.ranks[effectRank].shapeshiftingAnimatorCullingMode;

            
            playerCombatNode.appearanceREF.previousActionBarSlotsDATA.Clear();
            if (effect.ranks[effectRank].shapeshiftingOverrideMainActionBar)
            {
                
                for (var index = 0; index < ActionBarManager.Instance.actionBarSlots.Count; index++)
                {
                    var actionBarSlot = ActionBarManager.Instance.actionBarSlots[index];
                    if (actionBarSlot.actionBarType != CharacterData.ActionBarType.Main) continue;
                    
                    CharacterData.ActionBarSlotDATA acSlotData = new CharacterData.ActionBarSlotDATA();
                    acSlotData.contentType = actionBarSlot.contentType;
                    switch (acSlotData.contentType)
                    {
                        case CharacterData.ActionBarSlotContentType.None:
                            acSlotData.ID = -1;
                            break;
                        case CharacterData.ActionBarSlotContentType.Ability:
                            acSlotData.ID = actionBarSlot.ThisAbility.ID;
                            break;
                        case CharacterData.ActionBarSlotContentType.Item:
                            acSlotData.ID = actionBarSlot.ThisItem.ID;
                            break;
                    }
                    playerCombatNode.appearanceREF.previousActionBarSlotsDATA.Add(acSlotData);
                    
                    ActionBarManager.Instance.ResetActionSlot(index, true);
                    if (effect.ranks[effectRank].shapeshiftingActiveAbilities.Count - 1 >= index)
                    {
                        ActionBarManager.Instance.SetAbilityToSlot(RPGBuilderUtilities.GetAbilityFromID(effect.ranks[effectRank].shapeshiftingActiveAbilities[index]), index);
                    }
                }
            }
            
            HandleNestedEffects(playerCombatNode, effect.ranks[effectRank].nestedEffects);
            
            StatCalculator.CalculateShapeshiftingStats(playerCombatNode, effect.ranks[effectRank].statEffectsData);
            ShapeshiftingSlotsDisplayManager.Instance.DisplaySlots();
            
            playerCombatNode.appearanceREF.HideWeapon(1);
            playerCombatNode.appearanceREF.HideWeapon(2);
        }

        public void ResetPlayerShapeshift()
        {
            playerCombatNode.appearanceREF.isShapeshifted = false;
            Destroy(playerCombatNode.appearanceREF.shapeshiftModel);

            playerCombatNode.appearanceREF.cachedBodyParent.SetActive(true);
            playerCombatNode.appearanceREF.cachedArmorsParent.SetActive(true);

            playerCombatNode.playerControllerEssentials.anim.runtimeAnimatorController =
                playerCombatNode.appearanceREF.cachedAnimatorController;
            playerCombatNode.playerControllerEssentials.anim.avatar =
                playerCombatNode.appearanceREF.cachedAnimatorAvatar;
            playerCombatNode.playerControllerEssentials.anim.applyRootMotion =
                playerCombatNode.appearanceREF.cachedAnimatorUseRootMotion;
            playerCombatNode.playerControllerEssentials.anim.updateMode =
                playerCombatNode.appearanceREF.cachedAnimatorUpdateMode;
            playerCombatNode.playerControllerEssentials.anim.cullingMode =
                playerCombatNode.appearanceREF.cachedAnimatorCullingMode;
            
            playerCombatNode.appearanceREF.HandleAnimatorOverride();

            StatCalculator.ResetShapeshiftingStats(playerCombatNode);
            
            if (playerCombatNode.appearanceREF.previousActionBarSlotsDATA.Count != 0)
            {
                int mainSlotIndex = 0;
                for (var index = 0; index < ActionBarManager.Instance.actionBarSlots.Count; index++)
                {
                    var actionBarSlot = ActionBarManager.Instance.actionBarSlots[index];
                    if (actionBarSlot.actionBarType != CharacterData.ActionBarType.Main) continue;
                    ActionBarManager.Instance.ResetActionSlot(index, true);

                    if (mainSlotIndex > playerCombatNode.appearanceREF.previousActionBarSlotsDATA.Count - 1) continue;
                    switch (playerCombatNode.appearanceREF.previousActionBarSlotsDATA[mainSlotIndex].contentType)
                    {
                        case CharacterData.ActionBarSlotContentType.None:
                            break;
                        case CharacterData.ActionBarSlotContentType.Ability:
                            ActionBarManager.Instance.SetAbilityToSlot(
                                RPGBuilderUtilities.GetAbilityFromID(playerCombatNode.appearanceREF
                                    .previousActionBarSlotsDATA[mainSlotIndex].ID),
                                index);
                            break;
                        case CharacterData.ActionBarSlotContentType.Item:
                            ActionBarManager.Instance.SetItemToSlot(
                                RPGBuilderUtilities.GetItemFromID(playerCombatNode.appearanceREF
                                    .previousActionBarSlotsDATA[mainSlotIndex].ID),
                                index);
                            break;
                    }

                    mainSlotIndex++;
                }

                playerCombatNode.appearanceREF.previousActionBarSlotsDATA.Clear();
            }

            List<int> effectsToRemove = new List<int>();
            foreach (var state in playerCombatNode.nodeStateData)
            {
                if(state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Shapeshifting) continue;
                effectsToRemove.Add(state.stateEffect.ID);
                ResetNestedEffects(playerCombatNode, state.stateEffect, state.effectRank);
                break;
            }
            
            foreach (var effectToRemove in effectsToRemove)
            {
                playerCombatNode.RemoveEffect(effectToRemove);
            }
            
            ShapeshiftingSlotsDisplayManager.Instance.DisplaySlots();
            playerCombatNode.appearanceREF.ShowWeapon(InventoryManager.Instance.equippedWeapons[0].itemEquipped, 1, null);
            playerCombatNode.appearanceREF.ShowWeapon(InventoryManager.Instance.equippedWeapons[1].itemEquipped, 2, null);
        }

        public bool CanCombatNodeBlockThis(CombatNode targetInfo, CombatNode attacker)
        {
            var pointDirection = attacker.transform.position - targetInfo.transform.position;
            var angle = Vector3.Angle(targetInfo.transform.forward, pointDirection);
            return angle < (targetInfo.curBlockEffect.blockAngle + (targetInfo.curBlockEffect.blockAngle *
                                                                    GetTotalOfStatType(targetInfo,
                                                                        RPGStat.STAT_TYPE.BLOCK_ACTIVE_ANGLE)/100f));
        }

        public int getCurrentSummonCount(CombatNode nodeRef)
        {
            return (int)GetTotalOfStatType(nodeRef, RPGStat.STAT_TYPE.SUMMON_COUNT);
        }


        public bool isPet(CombatNode nodeInfo)
        {
            if (nodeInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
            {
                return nodeInfo.npcDATA != null && nodeInfo.ownerCombatInfo != null;
            }

            return false;
        }

        private int getCurrentWeaponAttackDamage(RPGEffect.RPGEffectRankData effectRank)
        {
            int finalDmg = 0;

            if (effectRank.useWeapon1Damage && InventoryManager.Instance.equippedWeapons[0].itemEquipped != null)
            {
                finalDmg += Random.Range(InventoryManager.Instance.equippedWeapons[0].itemEquipped.minDamage,
                    InventoryManager.Instance.equippedWeapons[0].itemEquipped.maxDamage);
            }
            if (effectRank.useWeapon2Damage && InventoryManager.Instance.equippedWeapons[1].itemEquipped != null)
            {
                finalDmg += Random.Range(InventoryManager.Instance.equippedWeapons[1].itemEquipped.minDamage,
                    InventoryManager.Instance.equippedWeapons[1].itemEquipped.maxDamage);
            }
            return finalDmg;
        }
        
        private int damageCalculation(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank,
            RPGAbility.RPGAbilityRankData rankREF, bool isDot, int curStack)
        {
            bool damageIsFromPet = isPet(casterInfo);
            float damage = 0;
            bool isActiveBlocked = false;
            int totalDmgBlocked = 0;
            RPGStat alteredStat = RPGBuilderUtilities.GetStatFromID(effect.ranks[effectRank].alteredStatID);
            switch (effect.ranks[effectRank].hitValueType)
            {
                case RPGAbility.COST_TYPES.FLAT:
                    damage = effect.ranks[effectRank].Damage;
                    break;
                case RPGAbility.COST_TYPES.PERCENT_OF_MAX:
                    damage = targetInfo.getCurrentMaxValue(alteredStat._name) * ((float)effect.ranks[effectRank].Damage/100f);
                    break;
                case RPGAbility.COST_TYPES.PERCENT_OF_CURRENT:
                    damage = targetInfo.getCurrentValue(alteredStat._name) * ((float)effect.ranks[effectRank].Damage/100f);
                    break;
            }
            
            if (casterInfo == playerCombatNode && effect.ranks[effectRank].weaponDamageModifier > 0)
            {
                int curWeaponDamage = getCurrentWeaponAttackDamage(effect.ranks[effectRank]);
                float weaponMod = effect.ranks[effectRank].weaponDamageModifier / 100;
                damage += curWeaponDamage * weaponMod;
            }

            if (effect.ranks[effectRank].requiredEffectID != -1 &&
                isEffectActiveOnTarget(RPGBuilderUtilities.GetEffectFromID(effect.ranks[effectRank].requiredEffectID), targetInfo))
            {
                damage += damage * (effect.ranks[effectRank].requiredEffectDamageModifier / 100);
            }

            if (effect.ranks[effectRank].damageStatID != -1 && effect.ranks[effectRank].damageStatModifier > 0)
            {
                damage += casterInfo.getCurrentValue(RPGBuilderUtilities.GetStatFromID(effect.ranks[effectRank].damageStatID)._name) *
                          (effect.ranks[effectRank].damageStatModifier / 100);
            }

            float elementalDMG = 0;
            float elementalRES = 0;
            float elementalPEN = 0;
            float resistanceTypeBonus = 0;
            float penTypeBonus = 0;
            float DMGDealtMOD = 0;
            float DMGTakenMOD = GetTotalOfStatType(targetInfo, RPGStat.STAT_TYPE.DMG_TAKEN);
            float CRITCHANCE = 0;
            float CRITPOWER = 0;
            var DamageTextType = effect.ranks[effectRank].mainDamageType.ToString();
            if (DamageTextType == "NONE") DamageTextType = "NO_DAMAGE_TYPE";
            // CHECKING CASTER STATS

            if (!effect.ranks[effectRank].FlatCalculation)
            {
                foreach (var t in casterInfo.nodeStats)
                {
                    foreach (var t1 in t.stat.statBonuses)
                    {
                        switch (t1.statType)
                        {
                            case RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE:
                            {
                                if (effect.ranks[effectRank].mainDamageType.ToString() == t1.StatFunction)
                                {
                                    damage += t.curValue * t1.modifyValue;

                                    var oppositeStat = t1.OppositeStat;
                                    if (oppositeStat != "NONE" && oppositeStat != "")
                                    {
                                        resistanceTypeBonus +=
                                            targetInfo.getCurrentValue(oppositeStat, "BASE_RESISTANCE_TYPE");

                                        var penStat = getOppositeStat(
                                            casterInfo.nodeStats[casterInfo.getStatIndexFromName(oppositeStat)].stat
                                                .statBonuses, "");
                                        if (penStat != "NONE" && penStat != "")
                                            // RESISTANCE STAT HAS A PENETRATION STAT
                                            penTypeBonus = casterInfo.getCurrentValue(penStat);
                                    }
                                }

                                break;
                            }
                            case RPGStat.STAT_TYPE.DMG_DEALT:
                                DMGDealtMOD += t.curValue * t1.modifyValue;
                                break;
                            case RPGStat.STAT_TYPE.CRIT_CHANCE:
                                if (!effect.ranks[effectRank].CannotCrit)
                                {
                                    CRITCHANCE += t.curValue * t1.modifyValue;
                                    if (damageIsFromPet)
                                        if (CRITCHANCE > 100)
                                            CRITCHANCE = 100;
                                }

                                break;
                            case RPGStat.STAT_TYPE.CRIT_POWER:
                                CRITPOWER += t.curValue * t1.modifyValue;
                                break;
                        }
                    }
                }


                if (damageIsFromPet)
                {
                    CRITPOWER += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                        RPGStat.STAT_TYPE.MINION_CRIT_POWER);

                    switch (effect.ranks[effectRank].mainDamageType)
                    {
                        case RPGEffect.MAIN_DAMAGE_TYPE.PHYSICAL_DAMAGE:
                            damage += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                                RPGStat.STAT_TYPE.MINION_PHYSICAL_DAMAGE);
                            break;
                        case RPGEffect.MAIN_DAMAGE_TYPE.MAGICAL_DAMAGE:
                            damage += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                                RPGStat.STAT_TYPE.MINION_MAGICAL_DAMAGE);
                            break;
                    }


                    DMGDealtMOD += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                        RPGStat.STAT_TYPE.MINION_DAMAGE);

                    CRITCHANCE += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                        RPGStat.STAT_TYPE.MINION_CRIT_CHANCE);
                    if (CRITCHANCE > 100) CRITCHANCE = 100;

                    CRITPOWER += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                        RPGStat.STAT_TYPE.MINION_CRIT_POWER);
                }

                if (effect.ranks[effectRank].CannotCrit)
                {
                    CRITCHANCE = 0;
                }

                if (damage > 0)
                {
                    if (resistanceTypeBonus > 0)
                    {
                        var finalBaseTypeRES = resistanceTypeBonus - penTypeBonus;
                        if (finalBaseTypeRES < 0) finalBaseTypeRES = 0;
                        finalBaseTypeRES /= 100;
                        finalBaseTypeRES = 1 - finalBaseTypeRES;

                        damage *= finalBaseTypeRES;
                    }
                }

                if (effect.ranks[effectRank].skillModifierID != -1 && effect.ranks[effectRank].skillModifier > 0)
                {
                    damage += RPGBuilderUtilities.getSkillLevel(effect.ranks[effectRank].skillModifierID) * effect.ranks[effectRank].skillModifier;
                }

                RPGSpecies targetSpeciesREF = targetInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.player
                    ? RPGBuilderUtilities.GetSpeciesFromID(RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).speciesID)
                    : RPGBuilderUtilities.GetSpeciesFromID(targetInfo.npcDATA.speciesID);
                if (effect.ranks[effectRank].secondaryDamageType != "NONE" && effect.ranks[effectRank].secondaryDamageType != "")
                {
                    // THIS EFFECT HAS A SECONDARY DAMAGE TYPE
                    elementalDMG = casterInfo.getCurrentValue(effect.ranks[effectRank].secondaryDamageType, "DAMAGE");
                    string oppositeStat = getOppositeStat(targetInfo.nodeStats[targetInfo.getStatIndexFromName(effect.ranks[effectRank].secondaryDamageType)].stat.statBonuses, effect.ranks[effectRank].secondaryDamageType);
                    
                    if (oppositeStat != "NONE" && oppositeStat != "")
                    {
                        // DAMAGE STAT HAS A RESISTANCE STAT
                        elementalRES = targetInfo.getCurrentValue(oppositeStat, "RESISTANCE");

                        var penStat = getOppositeStat(casterInfo.nodeStats[casterInfo.getStatIndexFromName(oppositeStat)].stat.statBonuses, "");
                        if (penStat != "NONE" && penStat != "")
                            // RESISTANCE STAT HAS A PENETRATION STAT
                            elementalPEN = casterInfo.getCurrentValue(penStat);
                    }

                    var finalRES = elementalRES - elementalPEN;
                    if (finalRES < 0) finalRES = 0;
                    finalRES /= 100;
                    finalRES = 1 - finalRES;

                    elementalDMG *= finalRES;

                    if (targetInfo.CanActiveBlockThis(casterInfo) && (targetInfo.curBlockEffect.blockAnyDamage || 
                        targetInfo.curBlockEffect.blockedDamageTypes.Contains(effect.ranks[effectRank].secondaryDamageType)))
                    {
                        float blockedAmount = elementalDMG * (targetInfo.curBlockPowerModifier/100f);
                        blockedAmount += (int) targetInfo.curBlockPowerFlat;
                        switch (targetInfo.curBlockEffect.blockEndType)
                        {
                            case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked when blockedAmount > targetInfo.curBlockedDamageLeft:
                                blockedAmount = targetInfo.curBlockedDamageLeft;
                                break;
                            case RPGEffect.BLOCK_END_TYPE.Stat:
                                int curStatAmount = (int)targetInfo.getCurrentValue(targetInfo.curBlockEffect.blockStatID);
                                if(blockedAmount > curStatAmount) blockedAmount = curStatAmount;
                                break;
                        }
                        if (blockedAmount < 0) blockedAmount = 0;
                        if (blockedAmount > elementalDMG)
                        {
                            blockedAmount = elementalDMG;
                            elementalDMG = 0;
                        }
                        else
                        {
                            elementalDMG -= blockedAmount;
                            if (elementalDMG < 0) elementalDMG = 0;
                        }

                        switch (targetInfo.curBlockEffect.blockEndType)
                        {
                            case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked:
                                targetInfo.ReduceBlockedDamageLeft((int)blockedAmount);
                                break;
                            case RPGEffect.BLOCK_END_TYPE.Stat:
                                targetInfo.AlterVitalityStat((int)blockedAmount, targetInfo.curBlockEffect.blockStatID);
                                break;
                        }
                        totalDmgBlocked += (int)blockedAmount;
                        DamageTextType = "BLOCKED";
                        isActiveBlocked = true;
                    }
                    
                    damage += elementalDMG;

                    if (targetSpeciesREF != null)
                    {
                        foreach (var trait in targetSpeciesREF.traits)
                        {
                            if(trait.statFunction != effect.ranks[effectRank].secondaryDamageType) continue;
                            damage *= (trait.modifier / 100f);
                        }
                    }
                }
                
                if (effect.effectType == RPGEffect.EFFECT_TYPE.DamageOverTime)
                {
                    float DOTBONUS = GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.DOT_BONUS);

                    if (DOTBONUS > 0)
                    {
                        damage += damage * (DOTBONUS / 100);
                    }
                }
                
                if (!effect.ranks[effectRank].CannotCrit && isDamageCrit(CRITCHANCE))
                {
                    var critBonus = RPGBuilderEssentials.Instance.combatSettings.CriticalDamageBonus + CRITPOWER;

                    critBonus /= 100;
                    damage *= critBonus;
                    DamageTextType += "_CRITICAL";
                }

                if (effect.ranks[effectRank].maxHealthModifier > 0)
                {
                    damage += (casterInfo.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name) /
                               100f) * effect.ranks[effectRank].maxHealthModifier;
                }

                if (effect.ranks[effectRank].missingHealthModifier > 0)
                {
                    float curMissingHealthPercent =
                        casterInfo.getCurrentValue(RPGBuilderEssentials.Instance.healthStatReference._name) /
                        casterInfo.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name);
                    curMissingHealthPercent = 1 - curMissingHealthPercent;
                    if (curMissingHealthPercent > 0)
                    {
                        curMissingHealthPercent *= 100;
                        curMissingHealthPercent *= effect.ranks[effectRank].missingHealthModifier / 100;
                        curMissingHealthPercent /= 100;
                        damage += damage * curMissingHealthPercent;
                    }
                }


                if (DMGDealtMOD != 0 || DMGTakenMOD != 0)
                {
                    DMGDealtMOD /= 100;
                    DMGTakenMOD /= 100;
                    damage += damage * DMGDealtMOD;
                    damage += damage * DMGTakenMOD;
                }
            }

            switch (effect.ranks[effectRank].mainDamageType)
            {
                case RPGEffect.MAIN_DAMAGE_TYPE.PHYSICAL_DAMAGE:
                {
                    if (effect.ranks[effectRank].alteredStatID != RPGBuilderEssentials.Instance.combatSettings.healthStatID) break;

                    float BLOCKCHANCE = 0, BLOCKFLAT = 0, BLOCKMODIFIER = 0, DODGECHANCE = 0, GLANCINGBLOWCHANCE = 0;
                    foreach (var t in targetInfo.nodeStats)
                    {
                        foreach (var t1 in t.stat.statBonuses)
                        {
                            switch (t1.statType)
                            {
                                case RPGStat.STAT_TYPE.BLOCK_CHANCE:
                                    BLOCKCHANCE += t.curValue;
                                    break;
                                case RPGStat.STAT_TYPE.BLOCK_FLAT:
                                    BLOCKFLAT += t.curValue;
                                    break;
                                case RPGStat.STAT_TYPE.BLOCK_MODIFIER:
                                    BLOCKMODIFIER += t.curValue;
                                    break;
                                case RPGStat.STAT_TYPE.DODGE_CHANCE:
                                    DODGECHANCE += t.curValue;
                                    break;
                                case RPGStat.STAT_TYPE.GLANCING_BLOW_CHANCE:
                                    GLANCINGBLOWCHANCE += t.curValue;
                                    break;
                            }
                        }
                    }

                    if (damageIsFromPet)
                    {
                        DODGECHANCE += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                            RPGStat.STAT_TYPE.MINION_DODGE_CHANCE);

                        GLANCINGBLOWCHANCE += GetTotalOfStatType(casterInfo.ownerCombatInfo,
                            RPGStat.STAT_TYPE.MINION_GLANCING_BLOW_CHANCE);
                    }

                    if (BLOCKCHANCE > 0)
                    {
                        if (Random.Range(0f, 100f) <= BLOCKCHANCE)
                        {
                            damage -= damage * (BLOCKMODIFIER / 100);
                            damage -= BLOCKFLAT;

                            if (damage < 0)
                            {
                                damage = 0;
                            }

                            DamageTextType = "BLOCKED";

                            int HEALTHONBLOCK = (int) GetTotalOfStatType(targetInfo, RPGStat.STAT_TYPE.HEALTH_ON_BLOCK);
                            bool targetIsPet = isPet(targetInfo);

                            if (targetIsPet)
                            {
                                HEALTHONBLOCK += (int) GetTotalOfStatType(targetInfo.ownerCombatInfo,
                                    RPGStat.STAT_TYPE.MINION_HEALTH_ON_BLOCK);
                            }

                            if (HEALTHONBLOCK > 0)
                            {
                                var heal = healingCalculation(targetInfo, targetInfo, null, 0,HEALTHONBLOCK, true);
                                targetInfo.Heal(heal, RPGBuilderEssentials.Instance.combatSettings.healthStatID);
                            }
                        }
                    }
                    else if (DODGECHANCE > 0)
                    {
                        if (Random.Range(0f, 100f) <= DODGECHANCE)
                        {
                            damage = 0;
                            DamageTextType = "DODGED";
                        }
                    }
                    else if (GLANCINGBLOWCHANCE > 0)
                    {
                        if (Random.Range(0f, 100f) <= GLANCINGBLOWCHANCE)
                        {
                            damage /= 2;
                        }
                    }

                    break;
                }
                case RPGEffect.MAIN_DAMAGE_TYPE.MAGICAL_DAMAGE:
                    break;
            }

            if (effect.ranks[effectRank].alteredStatID == RPGBuilderEssentials.Instance.combatSettings.healthStatID &&
                targetInfo.isImmune())
            {
                DamageTextType = "IMMUNE";
            }
            else
            {
                if (rankREF != null && effect.ranks[effectRank].alteredStatID ==
                    RPGBuilderEssentials.Instance.combatSettings.healthStatID)
                {
                    if (damage > 0)
                    {
                        if (AbilityHasTag(rankREF, RPGAbility.ABILITY_TAGS.onHit))
                        {
                            int HEALTHONHIT = (int) GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.HEALTH_ON_HIT);
                            if (damageIsFromPet)
                            {
                                HEALTHONHIT += (int) GetTotalOfStatType(casterInfo.ownerCombatInfo,
                                    RPGStat.STAT_TYPE.MINION_HEALTH_ON_HIT);
                            }

                            if (HEALTHONHIT > 0)
                            {
                                var heal = healingCalculation(casterInfo, casterInfo, null, 0, HEALTHONHIT, true);
                                targetInfo.Heal(heal, RPGBuilderEssentials.Instance.combatSettings.healthStatID);
                            }

                            foreach (var t in casterInfo.nodeStats)
                            {
                                foreach (var t1 in t.stat.statBonuses)
                                {
                                    switch (t1.statType)
                                    {
                                        case RPGStat.STAT_TYPE.EFFECT_TRIGGER:
                                            if (!(Random.Range(0f, 100f) <= t.curValue)) continue;
                                            foreach (var t2 in t.stat.onHitEffectsData)
                                            {
                                                if (t2.tagType != RPGAbility.ABILITY_TAGS.onHit) continue;
                                                if (Random.Range(0f, 100f) <= t2.chance)
                                                {
                                                    ExecuteEffect(casterInfo,
                                                        t2.targetType == RPGCombatDATA.TARGET_TYPE.Caster
                                                            ? casterInfo
                                                            : targetInfo,
                                                        RPGBuilderUtilities.GetEffectFromID(t2.effectID), t2.effectRank,
                                                        null, 0);
                                                }
                                            }

                                            break;
                                    }
                                }
                            }
                        }


                        if (targetInfo.CanActiveBlockThis(casterInfo))
                        {
                            if (targetInfo.curBlockEffect.blockAnyDamage ||
                                (effect.ranks[effectRank].mainDamageType ==
                                 RPGEffect.MAIN_DAMAGE_TYPE.PHYSICAL_DAMAGE &&
                                 targetInfo.curBlockEffect.blockPhysicalDamage) ||
                                effect.ranks[effectRank].mainDamageType == RPGEffect.MAIN_DAMAGE_TYPE.MAGICAL_DAMAGE &&
                                targetInfo.curBlockEffect.blockMagicalDamage)
                            {
                                float damageWithoutElem = damage - elementalDMG;
                                float blockedAmount =
                                    damageWithoutElem * (targetInfo.curBlockPowerModifier / 100f);
                                blockedAmount += (int) targetInfo.curBlockPowerFlat;
                                
                                switch (targetInfo.curBlockEffect.blockEndType)
                                {
                                    case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked when blockedAmount > targetInfo.curBlockedDamageLeft:
                                        blockedAmount = targetInfo.curBlockedDamageLeft;
                                        break;
                                    case RPGEffect.BLOCK_END_TYPE.Stat:
                                        int curStatAmount = (int)targetInfo.getCurrentValue(targetInfo.curBlockEffect.blockStatID);
                                        if(blockedAmount > curStatAmount) blockedAmount = curStatAmount;
                                        break;
                                }
                                
                                if (blockedAmount < 0) blockedAmount = 0;
                                
                                if (blockedAmount > damageWithoutElem)
                                {
                                    blockedAmount = damageWithoutElem;
                                }
                                
                                damage -= blockedAmount;
                                if (damage < 0) damage = 0;

                                switch (targetInfo.curBlockEffect.blockEndType)
                                {
                                    case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked:
                                        targetInfo.ReduceBlockedDamageLeft((int)blockedAmount);
                                        break;
                                    case RPGEffect.BLOCK_END_TYPE.Stat:
                                        targetInfo.AlterVitalityStat((int)blockedAmount, targetInfo.curBlockEffect.blockStatID);
                                        break;
                                }
                                
                                totalDmgBlocked += (int)blockedAmount;
                                DamageTextType = "BLOCKED";
                                isActiveBlocked = true;
                            }
                        }
                    }
                }
            }

            if (isActiveBlocked)
            {
                OnBlock(targetInfo, casterInfo, targetInfo.curBlockEffect.onBlockActions);
                
                switch (targetInfo.curBlockEffect.blockEndType)
                {
                    case RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked:
                        ActiveBlockingDisplayManager.Instance.UpdateDamageBlockedLeft();
                        break;
                    case RPGEffect.BLOCK_END_TYPE.HitCount:
                        targetInfo.StartCoroutine(targetInfo.IncreaseBlockHitCount());
                        break;
                }
                
            }

            if (isDot)
            {
                int dmgText = effect != null ? (int)(damage / effect.pulses) : (int)damage;
                dmgText *= curStack;
                HandleCombatTextTrigger(casterInfo, targetInfo, dmgText,
                    totalDmgBlocked, DamageTextType);
            }
            else
            {
                HandleCombatTextTrigger(casterInfo, targetInfo, (int) damage, totalDmgBlocked, DamageTextType);
            }

            return (int) damage;
        }

        private void OnBlock(CombatNode blockerNode, CombatNode blockedNode, List<RPGEffect.ON_BLOCK_ACTION> onBlockActions)
        {
            foreach (var onBlockAction in onBlockActions)
            {
                switch (onBlockAction.blockActionType)
                {
                    case RPGEffect.ON_BLOCK_ACTION_TYPE.Ability:
                        InitAbility(blockerNode, RPGBuilderUtilities.GetAbilityFromID(onBlockAction.entryID), onBlockAction.abMustBeKnown);
                        break;
                    case RPGEffect.ON_BLOCK_ACTION_TYPE.Effect:
                        var chance = Random.Range(0, 100f);
                        if (!(chance <= onBlockAction.chance)) continue;
                        ExecuteEffect(blockerNode, blockedNode, RPGBuilderUtilities.GetEffectFromID(onBlockAction.entryID), onBlockAction.effectRank, null, onBlockAction.delay);
                        break;
                }
            }
        }

        string getOppositeStat(List<RPGStat.StatBonusData> statBonuses, string stringValue)
        {
            foreach (var statBonus in statBonuses)
            {
                switch (statBonus.statType)
                {
                    case RPGStat.STAT_TYPE.RESISTANCE:
                        return statBonus.OppositeStat;
                    case RPGStat.STAT_TYPE.DAMAGE when statBonus.StatFunction == stringValue:
                        return statBonus.OppositeStat;
                    case RPGStat.STAT_TYPE.BASE_RESISTANCE_TYPE:
                        return statBonus.OppositeStat;
                    case RPGStat.STAT_TYPE.BASE_DAMAGE_TYPE when statBonus.StatFunction == stringValue:
                        return statBonus.OppositeStat;
                }
            }

            return "";
        }

        public void HandleOnKillActions(CombatNode attacker, CombatNode deadUnit, RPGAbility.RPGAbilityRankData rankREF)
        {
            if (rankREF == null) return;
            if (!AbilityHasTag(rankREF, RPGAbility.ABILITY_TAGS.onKill)) return;
            int HEALTHONKILL = (int) GetTotalOfStatType(attacker, RPGStat.STAT_TYPE.HEALTH_ON_KILL);
            if (isPet(attacker))
            {
                HEALTHONKILL += (int) GetTotalOfStatType(attacker.ownerCombatInfo,
                    RPGStat.STAT_TYPE.MINION_HEALTH_ON_KILL);
            }

            if (HEALTHONKILL > 0)
            {
                var heal = healingCalculation(attacker, attacker, null, 0,HEALTHONKILL, true);
                attacker.Heal(heal, RPGBuilderEssentials.Instance.combatSettings.healthStatID);
            }

            foreach (var t in attacker.nodeStats)
            {
                foreach (var t1 in t.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.EFFECT_TRIGGER:
                            if (!(Random.Range(0f, 100f) <= t.curValue)) continue;
                            foreach (var t2 in t.stat.onHitEffectsData)
                            {
                                if (t2.tagType != RPGAbility.ABILITY_TAGS.onKill) continue;
                                if (Random.Range(0f, 100f) <= t2.chance)
                                {
                                    ExecuteEffect(attacker, attacker, RPGBuilderUtilities.GetEffectFromID(t2.effectID), t2.effectRank,null, 0);
                                }
                            }

                            break;
                    }
                }
            }
        }

        public bool AbilityHasTag(RPGAbility.RPGAbilityRankData rankREF, RPGAbility.ABILITY_TAGS tag)
        {
            foreach (var t in rankREF.tagsData)
            {
                if (t.tag == tag) return true;
            }

            return false;
        }

        private int healingCalculation(CombatNode casterInfo, CombatNode targetInfo, RPGEffect effect, int effectRank, int baseValue, bool showCombatText)
        {
            float heal = 0;
            if (effect == null)
            {
                heal = baseValue;
            }
            else
            {
                RPGStat alteredStat = RPGBuilderUtilities.GetStatFromID(effect.ranks[effectRank].alteredStatID);
                switch (effect.ranks[effectRank].hitValueType)
                {
                    case RPGAbility.COST_TYPES.FLAT:
                        heal = effect.ranks[effectRank].Damage;
                        break;
                    case RPGAbility.COST_TYPES.PERCENT_OF_MAX:
                        heal = targetInfo.getCurrentMaxValue(alteredStat._name) * ((float)effect.ranks[effectRank].Damage/100f);
                        break;
                    case RPGAbility.COST_TYPES.PERCENT_OF_CURRENT:
                        heal = targetInfo.getCurrentValue(alteredStat._name) * ((float)effect.ranks[effectRank].Damage/100f);
                        break;
                }
            }
            
            if (effect != null)
            {
                if (casterInfo == playerCombatNode && effect.ranks[effectRank].weaponDamageModifier > 0)
                {
                    int curWeaponDamage = getCurrentWeaponAttackDamage(effect.ranks[effectRank]);
                    float weaponMod = effect.ranks[effectRank].weaponDamageModifier / 100;
                    heal += curWeaponDamage * weaponMod;
                }
            }

            if (effect != null && effect.ranks[effectRank].requiredEffectID != -1 && isEffectActiveOnTarget(RPGBuilderUtilities.GetEffectFromID(effect.ranks[effectRank].requiredEffectID),targetInfo))
            {
                heal += heal * (effect.ranks[effectRank].requiredEffectDamageModifier / 100);
            }
            
            if (effect != null && effect.ranks[effectRank].damageStatID != -1 && effect.ranks[effectRank].damageStatModifier > 0)
            {
                heal += casterInfo.getCurrentValue(RPGBuilderUtilities.GetStatFromID(effect.ranks[effectRank].damageStatID)._name) * (effect.ranks[effectRank].damageStatModifier / 100);
            }
            
            float elementalDMG = 0;
            float elementalAbsorb = 0;
            float DMGDealtMOD = 0;
            float DMGTakenMOD = GetTotalOfStatType(targetInfo, RPGStat.STAT_TYPE.HEAL_RECEIVED);
            float CRITCHANCE = 0;
            float CRITPOWER = 0;
            var DamageTextType = "HEAL";

            // CHECKING CASTER STATS
            if (effect != null && !effect.ranks[effectRank].FlatCalculation)
            {
                foreach (var t in casterInfo.nodeStats)
                {
                    foreach (var t1 in t.stat.statBonuses)
                    {
                        switch (t1.statType)
                        {
                            case RPGStat.STAT_TYPE.HEAL_DONE:
                                DMGDealtMOD += t.curValue * t1.modifyValue;
                                break;
                            case RPGStat.STAT_TYPE.CRIT_CHANCE:
                            {
                                if (!effect.ranks[effectRank].CannotCrit)
                                {
                                    CRITCHANCE += t.curValue * t1.modifyValue;
                                    if (CRITCHANCE > 100) CRITCHANCE = 100;
                                }

                                break;
                            }
                            case RPGStat.STAT_TYPE.CRIT_POWER:
                            {
                                CRITPOWER += t.curValue * t1.modifyValue;
                                break;
                            }
                            case RPGStat.STAT_TYPE.GLOBAL_HEALING:
                                heal += t.curValue * t1.modifyValue;
                                break;
                        }
                    }
                }

                if (effect != null && effect.ranks[effectRank].secondaryDamageType != "NONE" && effect.ranks[effectRank].secondaryDamageType != "")
                {
                    // THIS EFFECT HAS A SECONDARY DAMAGE TYPE
                    elementalDMG = casterInfo.getCurrentValue(effect.ranks[effectRank].secondaryDamageType);

                    var oppositeStat = getOppositeStat(targetInfo.nodeStats[casterInfo.getStatIndexFromName(effect.ranks[effectRank].secondaryDamageType)].stat.statBonuses, effect.ranks[effectRank].secondaryDamageType);
                    if (oppositeStat != "NONE" && oppositeStat != "")
                        // DAMAGE STAT HAS A RESISTANCE STAT
                        elementalAbsorb = targetInfo.getCurrentValue(oppositeStat);

                    var finalRES = elementalAbsorb;
                    if (finalRES < 0) finalRES = 0;
                    finalRES /= 100;
                    finalRES = 1 + finalRES;

                    heal += elementalDMG;
                    heal *= finalRES;
                }

                if (effect != null && effect.ranks[effectRank].skillModifierID != -1 && effect.ranks[effectRank].skillModifier > 0)
                {
                    heal += RPGBuilderUtilities.getSkillLevel(effect.ranks[effectRank].skillModifierID);
                }

                if (effect.ranks[effectRank].CannotCrit)
                {
                    CRITCHANCE = 0;
                }
                if (!effect.ranks[effectRank].CannotCrit && isDamageCrit(CRITCHANCE))
                {
                    var critBonus = RPGBuilderEssentials.Instance.combatSettings.CriticalDamageBonus + CRITPOWER;
                    critBonus /= 100;
                    heal *= critBonus;
                    DamageTextType += "_CRITICAL";
                }

                if (DMGDealtMOD != 0 || DMGTakenMOD != 0)
                {
                    DMGDealtMOD /= 100;
                    DMGTakenMOD /= 100;
                    heal += heal * DMGDealtMOD;
                    heal += heal * DMGTakenMOD;
                }

                if (effect != null && effect.effectType == RPGEffect.EFFECT_TYPE.HealOverTime)
                {
                    float HOTBONUS = GetTotalOfStatType(casterInfo, RPGStat.STAT_TYPE.HOT_BONUS);

                    if (HOTBONUS > 0)
                    {
                        heal += heal * (HOTBONUS / 100);
                    }
                }
            }

            if (showCombatText) HandleCombatTextTrigger(casterInfo, targetInfo, (int) heal, 0, DamageTextType);

            return (int) heal;
        }

        private bool isDamageCrit(float critRate)
        {
            if (!(critRate > 0)) return false;
            float critChance = Random.Range(0, 100);
            return critChance <= critRate;
        }


        private int getLifesteal(CombatNode casterInfo, RPGEffect effect, int effectRank, int dmg)
        {
            float lifestealStat = 0;

            foreach (var t in casterInfo.nodeStats)
            {
                foreach (var t1 in t.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.LIFESTEAL:
                            lifestealStat += t.curValue;
                            break;
                    }
                }
            }

            if (isPet(casterInfo))
            {
                lifestealStat += (int)GetTotalOfStatType(casterInfo.ownerCombatInfo,
                    RPGStat.STAT_TYPE.MINION_LIFESTEAL);
            }

            var curLifesteal = (effect.ranks[effectRank].lifesteal / 100) + (lifestealStat / 100);
            return (int) (dmg * curLifesteal);
        }


        private List<CombatNode> getClosestUnits(CombatNode playerCombatInfo, List<CombatNode> allCbtNodes, int maxUnitHit)
        {
            var closestUnits = new List<CombatNode>();
            var allDistances = new List<float>();

            foreach (var t in allCbtNodes)
                if (allDistances.Count >= maxUnitHit)
                {
                    var dist = Vector3.Distance(playerCombatInfo.gameObject.transform.position, t.transform.position);
                    var CurBiggestDistanceInArray = Mathf.Max(allDistances.ToArray());
                    var IndexOfBiggest = allDistances.IndexOf(CurBiggestDistanceInArray);
                    if (!(dist < CurBiggestDistanceInArray)) continue;
                    allDistances[IndexOfBiggest] = dist;
                    closestUnits[IndexOfBiggest] = t;
                }
                else
                {
                    allDistances.Add(Vector3.Distance(playerCombatInfo.gameObject.transform.position,
                        t.transform.position));
                    closestUnits.Add(t);
                }

            return closestUnits;
        }

        private List<CombatNode> getClosestUnits(CombatNode playerCombatInfo, List<Collider> hitColliders, int maxUnitHit)
        {
            var closestUnits = new List<CombatNode>();
            var allDistances = new List<float>();

            foreach (var t in hitColliders)
                if (allDistances.Count >= maxUnitHit)
                {
                    var dist = Vector3.Distance(playerCombatInfo.gameObject.transform.position, t.transform.position);
                    var CurBiggestDistanceInArray = Mathf.Max(allDistances.ToArray());
                    var IndexOfBiggest = allDistances.IndexOf(CurBiggestDistanceInArray);
                    if (!(dist < CurBiggestDistanceInArray)) continue;
                    allDistances[IndexOfBiggest] = dist;
                    closestUnits[IndexOfBiggest] = t.GetComponent<CombatNode>();
                }
                else
                {
                    allDistances.Add(Vector3.Distance(playerCombatInfo.gameObject.transform.position,
                        t.transform.position));
                    closestUnits.Add(t.GetComponent<CombatNode>());
                }

            return closestUnits;
        }

        private List<CombatNode> getClosestNearbyUnits(GameObject projGO, List<CombatNode> hitNodes, int maxUnitHit)
        {
            var closestUnits = new List<CombatNode>();
            var allDistances = new List<float>();

            foreach (var t in hitNodes)
                if (allDistances.Count >= maxUnitHit)
                {
                    var dist = Vector3.Distance(projGO.transform.position, t.transform.position);
                    var CurBiggestDistanceInArray = Mathf.Max(allDistances.ToArray());
                    var IndexOfBiggest = allDistances.IndexOf(CurBiggestDistanceInArray);
                    if (!(dist < CurBiggestDistanceInArray)) continue;
                    allDistances[IndexOfBiggest] = dist;
                    closestUnits[IndexOfBiggest] = t;
                }
                else
                {
                    allDistances.Add(Vector3.Distance(projGO.transform.position, t.transform.position));
                    closestUnits.Add(t);
                }

            return closestUnits;
        }

        public CombatNode SetupNPCPrefab(RPGNpc npcData, bool isPet, bool scaleWithOwner, CombatNode petOwner, Vector3 spawnPos, Quaternion rotation)
        {
            GameObject go = new GameObject(npcData._name);
            go.transform.position = spawnPos;
            go.transform.rotation = rotation;
            GameObject model = Instantiate(npcData.NPCVisual, go.transform);
            model.transform.SetParent(go.transform);
            model.transform.localPosition = npcData.modelPosition;
            model.transform.localScale = npcData.modelScale;

            if (npcData.isPlayerInteractable)
                go.layer = RPGBuilderEssentials.Instance.generalSettings.worldInteractableLayer;

            CombatNode combatNode = go.AddComponent<CombatNode>();
            Animator animator = go.AddComponent<Animator>();
            RPGBAIAgent rpgbaiAgent = go.AddComponent<RPGBAIAgent>();
            NavMeshAgent agent = go.AddComponent<NavMeshAgent>();

            combatNode.nodeType = CombatNode.COMBAT_NODE_TYPE.mob;
            combatNode.npcDATA = npcData;
            combatNode.agentREF = rpgbaiAgent;
            combatNode.nameplateYOffset = npcData.nameplateYOffset;
            combatNode.thisRendererREF = model.GetComponentInChildren<Renderer>();

            allCombatNodes.Add(combatNode);
            if (isPet)
            {
                combatNode.nodeType = CombatNode.COMBAT_NODE_TYPE.pet;
                combatNode.scaleWithOwner = scaleWithOwner;
                combatNode.ownerCombatInfo = petOwner;
                petOwner.currentPets.Add(combatNode);
            }

            animator.runtimeAnimatorController = npcData.animatorController;
            animator.avatar = npcData.animatorAvatar;
            animator.applyRootMotion = npcData.animatorUseRootMotion;
            animator.updateMode = npcData.animatorUpdateMode;
            animator.cullingMode = npcData.AnimatorCullingMode;

            rpgbaiAgent.thisAgent = agent;
            rpgbaiAgent.thisAnim = animator;
            rpgbaiAgent.thisCombatInfo = combatNode;

            agent.radius = npcData.navmeshAgentRadius;
            agent.height = npcData.navmeshAgentHeight;
            agent.angularSpeed = npcData.navmeshAgentAngularSpeed;
            agent.obstacleAvoidanceType = npcData.navmeshObstacleAvoidance;

            switch (npcData.colliderType)
            {
                case RPGNpc.NPCColliderType.Capsule:
                    CapsuleCollider capsule = go.AddComponent<CapsuleCollider>();
                    capsule.center = npcData.colliderCenter;
                    capsule.radius = npcData.colliderRadius;
                    capsule.height = npcData.colliderHeight;
                    break;
                case RPGNpc.NPCColliderType.Sphere:
                    SphereCollider sphere = go.AddComponent<SphereCollider>();
                    sphere.center = npcData.colliderCenter;
                    sphere.radius = npcData.colliderRadius;
                    break;
                case RPGNpc.NPCColliderType.Box:
                    BoxCollider box = go.AddComponent<BoxCollider>();
                    box.center = npcData.colliderCenter;
                    box.size = npcData.colliderSize;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            combatNode.InitializeCombatNode();
            return combatNode;
        }
    }
}