using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.AI;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.Utility;
using BLINK.RPGBuilder.World;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.LogicMono
{
    public class CombatNode : MonoBehaviour, IPlayerInteractable
    {
        public enum COMBAT_NODE_TYPE
        {
            mob,
            player,
            objectAction,
            pet
        }

        public COMBAT_NODE_TYPE nodeType;

        [Serializable]
        public class NodeStatesDATA
        {
            public string stateName;
            public CombatNode casterNode;
            public RPGEffect stateEffect;
            public int effectRank;
            public RPGAbility.RPGAbilityRankData rankREF;
            public Sprite stateIcon;
            public int maxPulses;
            public int curPulses;
            public float nextPulse;
            public float pulseInterval;
            public float stateMaxDuration;
            public float stateCurDuration;
            public int curStack;
            public int maxStack;
            public GameObject stateEffectGO;
        }

        public List<NodeStatesDATA> nodeStateData = new List<NodeStatesDATA>();
        public RPGNpc npcDATA;
        public NPCSpawner spawnerREF;
        public RPGBAIAgent agentREF;
        public int NPCLevel;
        public RPGBCharacterControllerEssentials playerControllerEssentials;
        public Renderer thisRendererREF;
        public float nameplateYOffset;
        public PlayerAppearanceHandler appearanceREF;
        public GroundIndicatorManager indicatorManagerREF;

        public List<GameObject> ownedCombatVisuals = new List<GameObject>();
        public List<GameObject> ownedLogicCombatVisuals = new List<GameObject>();
        public List<GameObject> destroyedOnStealthCombatVisuals = new List<GameObject>();
        public List<GameObject> destroyedOnStealthEndCombatVisuals = new List<GameObject>();
        
        public enum PET_MOVEMENT_ACTION_TYPES
        {
            follow,
            stay
        }


        public PET_MOVEMENT_ACTION_TYPES currentPetsMovementActionType = PET_MOVEMENT_ACTION_TYPES.follow;

        public enum PET_COMBAT_ACTION_TYPES
        {
            defend,
            aggro
        }

        public PET_COMBAT_ACTION_TYPES currentPetsCombatActionType = PET_COMBAT_ACTION_TYPES.defend;
        public List<CombatNode> currentPets = new List<CombatNode>();
        public CombatNode ownerCombatInfo;
        public bool IsCasting, IsChanneling, isInteractiveNodeCasting;

        public float currentCastProgress,
            targetCastTime,
            currentChannelProgress,
            targetChannelTime,
            currentInteractionProgress,
            targetInteractionTime;

        public int currentAbilityCastedSlot;
        public RPGAbility.RPGAbilityRankData currentAbilityCastedCurRank;
        public RPGAbility currentAbilityCasted;
        public CombatNode currentTargetCasted;
        public InteractiveNode currentInteractiveNodeCasted;
        public Vector3 currentProjectileClickPos;
        public bool scaleWithOwner;
        public bool dead, stealthed;
        private float nextAutoAttack;
        public float lastCombatActionTimer;
        public bool inCombat;

        public NodeSockets nodeSocketsREF;

        public bool isActiveBlocking;
        public float curBlockChargeTime, targetBlockChargeTime, blockDurationLeft, cachedBlockMaxDuration, curBlockPowerFlat, curBlockPowerModifier, nextBlockStatDrain;
        public int curBlockHitCount, curBlockedDamageLeft;
        public RPGEffect.RPGEffectRankData curBlockEffect;
        public bool isMotionImmune;

        [Serializable]
        public class ActiveAnimationCoroutine
        {
            public Coroutine coroutine;
            public RPGCombatDATA.CombatVisualAnimation visualAnimation;
            public Animator anim;
        }
        public List<ActiveAnimationCoroutine> allAnimationCoroutines = new List<ActiveAnimationCoroutine>();
        
        [Serializable]
        public class ActiveCombo
        {
            public RPGCombo combo;
            public int initialAbilityID;
            public int comboIndex;
            public float curTime, expireTime;
            public float curLoadTime, readyTime;
            public ComboSlot UISlotREF;
            public KeyCode keyRequired = KeyCode.None;
        }
        public List<ActiveCombo> activeCombos = new List<ActiveCombo>();
        
        [Serializable]
        public class ActiveToggledAbilities
        {
            public RPGAbility ability;
            public RPGAbility.RPGAbilityRankData rankREF;
            public float nextTrigger;
        }
        public List<ActiveToggledAbilities> activeToggledAbilities = new List<ActiveToggledAbilities>();
        
        public class AutoAttackDATA
        {
            public int currentAutoAttackAbilityID = -1;
            public RPGItem weaponItem;
        }

        public AutoAttackDATA AutoAttackData = new AutoAttackDATA();

        [Serializable]
        public class AbilitiesDATA
        {
            public int curAbilityID;
            public RPGAbility currentAbility;
            public float NextTimeUse, CDLeft;
        }
        public List<AbilitiesDATA> abilitiesData = new List<AbilitiesDATA>();

        public AbilitiesDATA getAbilityDATA(RPGAbility ab)
        {
            foreach (var t in abilitiesData)
                if (t.currentAbility == ab)
                    return t;

            return null;
        }

        public int getAbilityDATAIndex(RPGAbility ab)
        {
            for (var i = 0; i < abilitiesData.Count; i++)
                if (abilitiesData[i].currentAbility == ab)
                    return i;
            return -1;
        }

        [Serializable]
        public class NODE_STATS
        {
            public string _name;
            public RPGStat stat;
            public float curMinValue;
            public float curMaxValue;
            public float curValue;
            public float nextCombatShift, nextRestShift;
            public float valueFromItem;
            public float valueFromBonus;
            public float valueFromEffect;
            public float valueFromShapeshifting;
        }

        public List<NODE_STATS> nodeStats = new List<NODE_STATS>();

        public float getCurrentValue(string StatName)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    return t.curValue;
            return 0;
        }
        public float getCurrentValue(int statID)
        {
            foreach (var t in nodeStats)
                if (t.stat.ID == statID)
                    return t.curValue;
            return 0;
        }

        public float getCurrentValue(string statName, string statType)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == statName)
                    foreach (var t1 in t.stat.statBonuses)
                        if (t1.statType.ToString() == statType)
                            return t.curValue * t1.modifyValue;
            return 0;
        }

        public float getCurrentValue(int statID, string statType)
        {
            foreach (var t in nodeStats)
                if (t.stat.ID == statID)
                    foreach (var t1 in t.stat.statBonuses)
                        if (t1.statType.ToString() == statType)
                            return t.curValue * t1.modifyValue;
            return 0;
        }

        public void setCurrentValue(string StatName, float newValue)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    t.curValue = (int)newValue;
        }
        public void addCurrentValue(string StatName, float addedValue)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    t.curValue += addedValue;
        }

        public float getCurrentMaxValue(string StatName)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    return t.curMaxValue;
            return 0;
        }

        public void setCurrentMaxValue(string StatName, float newValue)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    t.curMaxValue = (int)newValue;
        }
        public void addCurrentMaxValue(string StatName, float addedValue)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    t.curMaxValue += addedValue;
        }

        public float getCurrentMinValue(string StatName)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    return t.curMinValue;
            return 0;
        }

        public void setCurrentMinValue(string StatName, float newValue)
        {
            foreach (var t in nodeStats)
                if (t.stat._name == StatName)
                    t.curMinValue = newValue;
        }

        public bool isLeaping;
        private Vector3 LeapEndPOS;
        private Vector3 startPos;
        private float leapHeight;
        private float leapSpeed;

        public void InitLeap(Vector3 startpos, Vector3 endpos, float height, float speed)
        {
            isLeaping = true;
            startPos = startpos;
            LeapEndPOS = new Vector3(endpos.x, endpos.y - 0.025f, endpos.z);
            leapHeight = height;
            leapSpeed = speed;
            transform.LookAt(endpos);
        }

        private float leapAnimation;

        private void checkIsLeaping()
        {
            if (!isLeaping) return;
            leapAnimation += Time.deltaTime;

            if (Physics.Raycast(transform.position, transform.forward, 0.3f,
                CombatManager.Instance.LeapInterruptLayers))
            {
                ResetLeap();
                return;
            }

            playerControllerEssentials.lastPosition = transform.position;
            Vector3 movement = MathParabola.Parabola(startPos, LeapEndPOS, leapHeight, leapAnimation / leapSpeed) -
                               transform.position;
            playerControllerEssentials.charController.Move(movement);

            if (playerControllerEssentials.IsInMotionWithoutProgress(0.05f))
            {
                ResetLeap();
                return;
            }

            if (Vector3.Distance(transform.position, LeapEndPOS) < 0.15f)
            {
                ResetLeap();
            }
        }

        private void ResetLeap()
        {
            CombatManager.Instance.LeapEnded(this);
            CombatManager.playerCombatNode.playerControllerEssentials.EndGroundLeap();
            isLeaping = false;
            leapAnimation = 0;
            leapHeight = 0;
            leapSpeed = 0;
        }

        private void checkIsCasting()
        {
            if (!IsCasting) return;
            currentCastProgress += Time.deltaTime;
            if (nodeType == COMBAT_NODE_TYPE.player)
            {
                if (!currentAbilityCastedCurRank.castInRun && playerControllerEssentials.ShouldCancelCasting())
                {
                    ResetCasting();
                    return;
                }

                if (currentAbilityCastedCurRank.castBarVisible)
                    PlayerInfoDisplayManager.Instance.UpdateCastBar(currentCastProgress, targetCastTime);
            }

            if (currentAbilityCastedCurRank.animationTriggered && effectTriggered)
            {
                effectTriggered = false;
                currentCastProgress = targetCastTime;
            }

            if (!(currentCastProgress >= targetCastTime)) return;

            if ((currentAbilityCastedCurRank.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE ||
                 currentAbilityCastedCurRank.targetType == RPGAbility.TARGET_TYPES.TARGET_INSTANT)
                && currentTargetCasted.dead)
            {
                if (this == CombatManager.playerCombatNode)
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("The target is dead!", 3);
                ResetCasting();
                return;
            }

            CombatManager.Instance.ExecuteCombatVisualEffectList(
                RPGCombatDATA.CombatVisualActivationType.CastCompleted, this,
                currentAbilityCastedCurRank.visualEffects);
            CombatManager.Instance.ExecuteCombatVisualAnimationList(
                RPGCombatDATA.CombatVisualActivationType.CastCompleted, this,
                currentAbilityCastedCurRank.visualAnimations);

            CombatManager.Instance.HandleAbilityTypeActions(this, currentTargetCasted, currentAbilityCasted,
                currentAbilityCastedCurRank, false);

            if (!currentAbilityCastedCurRank.startCDOnActivate)
            {
                CombatManager.Instance.StartCooldown(this, currentAbilityCastedSlot, currentAbilityCastedCurRank,
                    currentAbilityCasted.ID);
                if (this == CombatManager.playerCombatNode && currentAbilityCastedCurRank.isGCD)
                    CombatManager.Instance.StartGCD();
            }

            CombatManager.Instance.HandleAbilityCost(this, currentAbilityCastedCurRank);
            if (currentAbilityCastedCurRank.comboStarsAfterCastComplete)
                CombatManager.Instance.AbilityUsed(this, currentAbilityCasted.ID);
            ResetCasting();
        }

        private void checkIsChanneling()
        {
            if (!IsChanneling) return;
            currentChannelProgress -= Time.deltaTime;
            if (nodeType == COMBAT_NODE_TYPE.player)
            {
                if (!currentAbilityCastedCurRank.castInRun && playerControllerEssentials.IsMoving())
                {
                    ResetChanneling();
                    return;
                }

                PlayerInfoDisplayManager.Instance.UpdateChannelBar(currentChannelProgress, currentAbilityCastedCurRank.channelTime);
            }

            if (currentChannelProgress <= 0) ResetChanneling();
        }

        private void checkIsInteractiveNodeCasting()
        {
            if (!isInteractiveNodeCasting) return;
            currentInteractionProgress += Time.deltaTime;
            if (nodeType == COMBAT_NODE_TYPE.player)
            {
                if (PlayerInfoDisplayManager.Instance.showInteractionBar)
                {
                    PlayerInfoDisplayManager.Instance.UpdateInteractionBar(currentInteractionProgress, targetInteractionTime);
                }
                if (WorldInteractableDisplayManager.Instance.showInteractionBar)
                {
                    WorldInteractableDisplayManager.Instance.UpdateInteractionBar(currentInteractionProgress, targetInteractionTime);
                }
            }
        
            if (playerControllerEssentials.IsMoving())
            {
                ResetInteractiveNodeCasting();
                return;
            }
        
            if (!(currentInteractionProgress >= targetInteractionTime)) return;
            currentInteractiveNodeCasted.UseNode();
            ResetInteractiveNodeCasting();
        }

        private void HandleCombatState()
        {
            lastCombatActionTimer += Time.deltaTime;
            if (!(lastCombatActionTimer >= RPGBuilderEssentials.Instance.combatSettings.outOfCombatDuration)) return;
            if ((this == CombatManager.playerCombatNode && !CombatManager.Instance.inCombatOverriden && RPGBuilderEssentials.Instance.combatSettings.useAutomaticCombatState) ||
                (this != CombatManager.playerCombatNode && agentREF.target == null))
            {
                CombatManager.Instance.ResetCombat(this);
            }
        }

        private void HandleStatShifting()
        {
            if (dead) return;
            foreach (var t in nodeStats)
                if (t.stat.isVitalityStat)
                {
                    switch (inCombat)
                    {
                        case true when Time.time >= t.nextCombatShift && t.stat.isShiftingInCombat && CanShift(t.stat):
                            t.nextCombatShift = Time.time + t.stat.shiftIntervalInCombat;
                            UpdateStat(t.stat._name, "curBase",
                                t.stat.shiftAmountInCombat + GetTotalAmountOfBonusShift(t.stat));
                            break;
                        case false when Time.time >= t.nextRestShift && t.stat.isShiftingOutsideCombat && CanShift(t.stat):
                            t.nextRestShift = Time.time + t.stat.shiftIntervalOutsideCombat;
                            UpdateStat(t.stat._name, "curBase",
                                t.stat.shiftAmountOutsideCombat + GetTotalAmountOfBonusShift(t.stat));
                            break;
                    }
                }
        }

        private bool CanShift(RPGStat stat)
        {
            if (CombatManager.playerCombatNode != this) return true;

            if (CombatManager.playerCombatNode.playerControllerEssentials.isSprinting() &&
                !stat.isShiftingInSprint) return false;
            
            if (CombatManager.playerCombatNode.isActiveBlocking &&
                !stat.isShiftingInBlock) return false;

            return true;
        }

        float GetTotalAmountOfBonusShift(RPGStat statREF)
        {
            float total = 0;
            foreach (var t in nodeStats)
            {
                foreach (var t1 in t.stat.statBonuses)
                {
                    switch (t1.statType)
                    {
                        case RPGStat.STAT_TYPE.VITALITY_REGEN:
                            if (t1.statID == statREF.ID)
                            {
                                total += t.curValue;
                            }

                            break;
                    }
                }
            }

            return total;
        }

        public bool autoAttackIsReady()
        {
            return Time.time >= nextAutoAttack;
        }
        

        public void InitAACooldown(float nextAA)
        {
            nextAutoAttack = Time.time + nextAA;
        }
        
        
        public void InitActionAbilityCooldown(int id, float nextUse)
        {
            foreach (var actionAb in CharacterData.Instance.currentActionAbilities)
            {
                if (actionAb.ability.ID == id)
                {
                    actionAb.NextTimeUse = Time.time + nextUse;
                }
            }
        }

        private void HandleAutoAttack()
        {
            if (AutoAttackData.currentAutoAttackAbilityID == -1) return;
            if (!(Time.time >= nextAutoAttack)) return;
            var abilityREF = RPGBuilderUtilities.GetAbilityFromID(AutoAttackData.currentAutoAttackAbilityID);
            if (abilityREF == null) return;
            var rankREF = abilityREF.ranks[0];
            if (rankREF != null) CombatManager.Instance.InitAbility(CombatManager.playerCombatNode, abilityREF, false);
        }

        public void AddCombatVisualToDestroyList(GameObject go)
        {
            ownedCombatVisuals.Add(go);
        }
        public void AddLogicCombatVisualToDestroyList(GameObject go)
        {
            ownedLogicCombatVisuals.Add(go);
        }
        
        public void AddCombatVisualToDestroyOnStealthList(GameObject go)
        {
            destroyedOnStealthCombatVisuals.Add(go);
        }
        
        public void AddCombatVisualToDestroyOnStealthEndList(GameObject go)
        {
            destroyedOnStealthEndCombatVisuals.Add(go);
        }
        private void FixedUpdate()
        {
            if (nodeType != COMBAT_NODE_TYPE.objectAction)
            {

                if (inCombat) HandleCombatState();

                HandleStatShifting();
                HandleActiveCombos();

                if (nodeType == COMBAT_NODE_TYPE.player)
                {
                    checkIsLeaping();
                    checkIsInteractiveNodeCasting();

                    if (CombatManager.Instance.PlayerTargetData.currentTarget != null &&
                        CombatManager.Instance.PlayerTargetData.isAttacking)
                        HandleAutoAttack();
                }
                else
                {
                    UpdateCooldowns();
                }

                checkIsCasting();
                checkIsChanneling();

                if (isActiveBlocking)
                {
                    HandleActiveBlocking();
                }
                
                HandleToggledAbilities();
            }

            UpdateStates();
        }

        private void HandleToggledAbilities()
        {
            foreach (var toggledAbility in activeToggledAbilities)
            {
                if (!(Time.time >= toggledAbility.nextTrigger)) continue;
                if (!CombatManager.Instance.UseRequirementsMet(this, toggledAbility.ability, toggledAbility.rankREF, true))
                {
                    RemoveToggledAbility(toggledAbility.ability);
                    return;
                }
                toggledAbility.nextTrigger = Time.time + toggledAbility.rankREF.toggledTriggerInterval;
                    
                CombatManager.Instance.HandleAbilityTypeActions(this, this, toggledAbility.ability,
                    toggledAbility.rankREF, true);
                CombatManager.Instance.ExecuteCombatVisualEffectList(
                    RPGCombatDATA.CombatVisualActivationType.Activate, this,
                    toggledAbility.rankREF.visualEffects);
                if (toggledAbility.rankREF.isToggleCostOnTrigger)
                {
                    CombatManager.Instance.HandleAbilityCost(this, toggledAbility.rankREF);
                }
            }
        }

        public void RemoveToggledAbility(RPGAbility ability)
        {
            foreach (var toggledAbility in activeToggledAbilities)
            {
                if(toggledAbility.ability != ability) continue;
                activeToggledAbilities.Remove(toggledAbility);
                ActionBarManager.Instance.UpdateToggledAbilities();
                return;
            }
        }

        public bool CanActiveBlockThis(CombatNode attacker)
        {
            return (isActiveBlocking && blockIsDoneCharging) && CombatManager.Instance.CanCombatNodeBlockThis(this, attacker);
        }

        private void HandleActiveCombos()
        {
            foreach (var combo in activeCombos)
            {
                if (combo.readyTime > 0)
                {
                    combo.curLoadTime += Time.deltaTime;
                    if (!(combo.curLoadTime >= combo.readyTime)) continue;
                    combo.curLoadTime = 0;
                    combo.readyTime = 0;
                }
                else
                {
                    combo.curTime -= Time.deltaTime;
                
                    if(combo.curTime > 0) continue;
                    StartCoroutine(RemoveComboEntry(combo, true, true));
                }
            }
        }
        
        public IEnumerator RemoveComboEntry(ActiveCombo combo, bool resetActionBarImage, bool resetComboActive)
        {
            yield return new WaitForFixedUpdate();
            if(resetComboActive) RPGBuilderUtilities.SetAbilityComboActive(combo.initialAbilityID, false);
            if(resetActionBarImage) CombosDisplayManager.Instance.ResetActionBarSlotsImage(combo.initialAbilityID);
            if(this == CombatManager.playerCombatNode)Destroy(combo.UISlotREF.gameObject);
            activeCombos.Remove(combo);
        }
        
        private void ResetInteractiveNodeCasting()
        {
            isInteractiveNodeCasting = false;
            
            currentInteractiveNodeCasted.ResetAllInteractionAnimations();
            currentInteractiveNodeCasted = null;
            
            currentInteractionProgress = 0;
            targetInteractionTime = 0;
            
            if (nodeType == COMBAT_NODE_TYPE.player)
            {
                if (PlayerInfoDisplayManager.Instance.showInteractionBar)
                {
                    PlayerInfoDisplayManager.Instance.ResetInteractionBarBar();
                }
                if (WorldInteractableDisplayManager.Instance.showInteractionBar)
                {
                    WorldInteractableDisplayManager.Instance.ResetInteractionBarBar();
                }
            }
        }

        public void ResetCasting()
        {
            if (nodeType == COMBAT_NODE_TYPE.player)
            {
                playerControllerEssentials.AbilityEndCastActions(currentAbilityCastedCurRank);
                if (currentAbilityCastedCurRank.castBarVisible) PlayerInfoDisplayManager.Instance.ResetCastBar();
            }

            IsCasting = false;
            currentAbilityCasted = null;
            currentTargetCasted = null;
            currentCastProgress = 0;
            targetCastTime = 0;
            currentAbilityCastedSlot = -1;
            currentAbilityCastedCurRank = null;
        }

        private void ResetChanneling()
        {
            IsChanneling = false;
            currentAbilityCasted = null;
            currentChannelProgress = 0;
            targetChannelTime = 0;
            currentAbilityCastedSlot = -1;
            PlayerInfoDisplayManager.Instance.ResetChannelBar();
            currentAbilityCastedCurRank = null;
        }

        private void UpdateStates()
        {
            for (var i = 0; i < nodeStateData.Count; i++)
            {
                if(!nodeStateData[i].stateEffect.endless) nodeStateData[i].stateCurDuration += Time.deltaTime;
                if (nodeStateData[i].curPulses > 0)
                {
                    nodeStateData[i].nextPulse -= Time.deltaTime;
                }
                
                if (nodeStateData[i].nextPulse <= 0 && nodeStateData[i].curPulses < nodeStateData[i].maxPulses)
                {
                    nodeStateData[i].nextPulse = nodeStateData[i].pulseInterval;
                    nodeStateData[i].curPulses++;
                    
                    switch (nodeStateData[i].stateEffect.effectType)
                    {
                        case RPGEffect.EFFECT_TYPE.DamageOverTime:
                            CombatManager.Instance.ExecuteDOTPulse(nodeStateData[i].casterNode, this,
                                nodeStateData[i].stateEffect, nodeStateData[i].effectRank, nodeStateData[i].curStack,
                                nodeStateData[i].rankREF);
                            break;
                        case RPGEffect.EFFECT_TYPE.HealOverTime:
                            CombatManager.Instance.ExecuteHOTPulse(nodeStateData[i].casterNode, this,
                                nodeStateData[i].stateEffect, nodeStateData[i].effectRank, nodeStateData[i].curStack);
                            break;
                        case RPGEffect.EFFECT_TYPE.Stat:
                            StatCalculator.CalculateEffectsStats(this);
                            CombatManager.Instance.ExecuteCombatVisualEffectList(
                                RPGCombatDATA.CombatVisualActivationType.Activate, this,
                                nodeStateData[i].stateEffect.ranks[nodeStateData[i].effectRank].visualEffects);
                            break;
                    }

                    if (i + 1 > nodeStateData.Count) return;
                }
                
                if (nodeStateData[i].stateEffect.endless || !(nodeStateData[i].stateCurDuration >= nodeStateData[i].stateMaxDuration)) continue;
                HandleEffectEnd(i);
                return;
            }
        }

        public void HandleEffectEnd(int nodeStateIndex)
        {
            switch (nodeStateData[nodeStateIndex].stateEffect.effectType)
            {
                case RPGEffect.EFFECT_TYPE.Stun:
                case RPGEffect.EFFECT_TYPE.Sleep:
                case RPGEffect.EFFECT_TYPE.Root:
                    switch (nodeType)
                    {
                        case COMBAT_NODE_TYPE.mob:
                        case COMBAT_NODE_TYPE.pet:
                            if (agentREF != null)
                                agentREF.ResetStun();
                            break;
                        case COMBAT_NODE_TYPE.player:
                            playerControllerEssentials.anim.SetBool("Stunned", false);
                            break;
                    }

                    break;
                case RPGEffect.EFFECT_TYPE.Shapeshifting:
                    switch (nodeType)
                    {
                        case COMBAT_NODE_TYPE.mob:
                        case COMBAT_NODE_TYPE.pet:

                            break;
                        case COMBAT_NODE_TYPE.player:
                            if (appearanceREF.isShapeshifted)
                            {
                                CombatManager.Instance.ResetPlayerShapeshift();
                                return;
                            }

                            break;
                    }

                    break;

                case RPGEffect.EFFECT_TYPE.Flying:
                    switch (nodeType)
                    {
                        case COMBAT_NODE_TYPE.mob:
                        case COMBAT_NODE_TYPE.pet:

                            break;
                        case COMBAT_NODE_TYPE.player:
                            playerControllerEssentials.EndFlying();
                            break;
                    }

                    break;

                case RPGEffect.EFFECT_TYPE.Stealth:
                    CombatManager.Instance.CancelStealth(this);
                    return;
            }

            bool statEffect = nodeStateData[nodeStateIndex].stateEffect.effectType == RPGEffect.EFFECT_TYPE.Stat;
            nodeStateData.RemoveAt(nodeStateIndex);

            if (statEffect)
            {
                StatCalculator.CalculateEffectsStats(this);
            }
        }

        public void RemoveEffect(int ID)
        {
            for (var index = 0; index < nodeStateData.Count; index++)
            {
                var effect = nodeStateData[index];
                if (effect.stateEffect.ID != ID) continue;

                HandleEffectEnd(index);
                
                if(this == CombatManager.playerCombatNode) PlayerStatesDisplayHandler.Instance.RemoveState(index);
                break;
            }
        }
        
        public void RemoveEffectByIndex(int index)
        {
            for (var state = 0; state < nodeStateData.Count; state++)
            {
                if(state != index) continue;
                
                HandleEffectEnd(index);
                
                if(this == CombatManager.playerCombatNode) PlayerStatesDisplayHandler.Instance.RemoveState(index);
                break;
            }
        }
        
        public void CancelState(int stateIndex)
        {
            HandleEffectEnd(stateIndex);

            if (nodeType == COMBAT_NODE_TYPE.player)
                for (var i = 0; i < PlayerStatesDisplayHandler.Instance.curStatesSlots.Count; i++)
                    PlayerStatesDisplayHandler.Instance.RemoveState(i);
        }

        public void InterruptCastActions(bool isStun)
        {
            switch (nodeType)
            {
                case COMBAT_NODE_TYPE.mob:
                case COMBAT_NODE_TYPE.pet:
                    if (agentREF != null)
                    {
                        agentREF.thisAnim.Rebind();
                        if (isStun)
                        {
                           if(agentREF.canAnimate()) agentREF.thisAnim.SetBool("Stunned", true);
                        }
                    }
                    break;
                case COMBAT_NODE_TYPE.player:
                    //playerControllerEssentials.anim.Rebind();
                    if (isStun)
                    {
                        playerControllerEssentials.anim.SetBool("Stunned", true);
                    }

                    break;
            }

            if (IsCasting || IsChanneling)
            {
                CombatManager.Instance.ExecuteCombatVisualEffectList(
                    RPGCombatDATA.CombatVisualActivationType.Interrupted, this, currentAbilityCastedCurRank.visualEffects);
                CombatManager.Instance.ExecuteCombatVisualAnimationList(
                    RPGCombatDATA.CombatVisualActivationType.CastCompleted, this, currentAbilityCastedCurRank.visualAnimations);
            }

            if(IsCasting)ResetCasting();
            if(IsChanneling)ResetChanneling();
            if(isInteractiveNodeCasting)ResetInteractiveNodeCasting();
        }

        private void InitCurrentClickPos(RPGAbility.RPGAbilityRankData rankREF)
        {
            if (this != CombatManager.playerCombatNode || rankREF.targetType != RPGAbility.TARGET_TYPES.PROJECTILE ||
                !rankREF.projectileShootOnClickPosition) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up,
                new Vector3(0, transform.position.y + rankREF.effectPositionOffset.y, 0));
            if (plane.Raycast(ray, out var distance))
            {
                currentProjectileClickPos = ray.GetPoint(distance);
            }
        }

        public void InitInteracting(InteractiveNode interactiveNode, float duration)
        {
            isInteractiveNodeCasting = true;
            currentInteractiveNodeCasted = interactiveNode;
            currentInteractionProgress = 0;
            targetInteractionTime = duration;
        }

        public void InitCasting(RPGAbility thisAbility, int abSlotIndex, RPGAbility.RPGAbilityRankData rankREF)
        {
            effectTriggered = false;
            IsCasting = true;
            currentAbilityCasted = thisAbility;
            currentAbilityCastedCurRank = rankREF;
            currentCastProgress = 0;
            targetCastTime = CombatManager.Instance.CalculateCastTime(this, rankREF.castTime);
            currentAbilityCastedSlot = abSlotIndex;
            if (rankREF.startCDOnActivate)
            {
                CombatManager.Instance.StartCooldown(this, abSlotIndex, rankREF, thisAbility.ID);
                if (this == CombatManager.playerCombatNode && currentAbilityCastedCurRank.isGCD)
                    CombatManager.Instance.StartGCD();
            }

            if ((nodeType == COMBAT_NODE_TYPE.mob || nodeType == COMBAT_NODE_TYPE.pet) && rankREF.castBarVisible)
            {
                ScreenSpaceNameplates.Instance.InitACastBar(this, thisAbility);
            }

            InitCurrentClickPos(rankREF);
        }

        public void InitChanneling(RPGAbility thisAbility, int abSlotIndex, RPGAbility.RPGAbilityRankData rankREF)
        {
            IsChanneling = true;
            currentAbilityCasted = thisAbility;
            currentAbilityCastedCurRank = rankREF;
            currentChannelProgress = rankREF.channelTime;
            targetChannelTime = 0;
            currentAbilityCastedSlot = abSlotIndex;
            if (nodeType == COMBAT_NODE_TYPE.mob || nodeType == COMBAT_NODE_TYPE.pet)
                ScreenSpaceNameplates.Instance.InitAChannelBar(this, thisAbility);
        }

        public bool isStunned()
        {
            foreach (var t in nodeStateData)
                if (t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Stun)
                    return true;

            return false;
        }

        public bool isSilenced()
        {
            foreach (var t in nodeStateData)
                if (t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Silence)
                    return true;

            return false;
        }

        public bool isSleeping()
        {
            foreach (var t in nodeStateData)
                if (t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Sleep)
                    return true;

            return false;
        }

        public bool isRooted()
        {
            foreach (var t in nodeStateData)
                if (t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Root)
                    return true;

            return false;
        }

        public bool isTaunted()
        {
            foreach (var t in nodeStateData)
                if (t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Taunt)
                    return true;

            return false;
        }

        public bool isImmune()
        {
            foreach (var t in nodeStateData)
                if (t.stateEffect.effectType == RPGEffect.EFFECT_TYPE.Immune)
                    return true;

            return this == CombatManager.playerCombatNode && CombatManager.playerCombatNode.isMotionImmune;
        }
        
        public bool isInMotion()
        {
            return this == CombatManager.playerCombatNode && playerControllerEssentials.motionActive;
        }

        public bool isStealthed()
        {
            return stealthed;
        }

        private void UpdateCooldowns()
        {
            foreach (var t in abilitiesData)
                if (t.NextTimeUse > 0)
                {
                    t.CDLeft -= Time.deltaTime;
                    if (!(t.CDLeft <= 0)) continue;
                    t.CDLeft = 0;
                    t.NextTimeUse = 0;
                }
        }

        public void InitStats()
        {
            foreach (var t in RPGBuilderEssentials.Instance.allStats)
            {
                var newAttributeToLoad = new NODE_STATS();
                newAttributeToLoad._name = t._name;
                newAttributeToLoad.stat = t;
                if (nodeType == COMBAT_NODE_TYPE.player)
                {
                    newAttributeToLoad.curMinValue = t.minValue;
                    newAttributeToLoad.curValue = t.baseValue;
                    newAttributeToLoad.curMaxValue = t.maxValue;
                }
                else
                {
                    RPGSpecies speciesREF = RPGBuilderUtilities.GetSpeciesFromID(npcDATA.speciesID);
                    if (npcHasCustomStat(t))
                    {
                        var index = getCustomStatIndex(t);
                        if (index != -1)
                        {
                            newAttributeToLoad.curMinValue = npcDATA.stats[index].minValue;
                            newAttributeToLoad.curValue = npcDATA.stats[index].baseValue;
                            newAttributeToLoad.curMaxValue = npcDATA.stats[index].maxValue;
                            if (RPGBuilderUtilities.GetStatFromID(npcDATA.stats[index].statID).isVitalityStat)
                            {
                                newAttributeToLoad.curMaxValue += npcDATA.stats[index].bonusPerLevel * NPCLevel;
                                newAttributeToLoad.curValue = newAttributeToLoad.curMaxValue;
                            }
                            else
                            {
                                newAttributeToLoad.curValue += npcDATA.stats[index].bonusPerLevel * NPCLevel;
                            }

                            if (nodeType == COMBAT_NODE_TYPE.pet)
                            {
                                if (newAttributeToLoad.stat._name ==
                                    RPGBuilderEssentials.Instance.healthStatReference._name)
                                {
                                    newAttributeToLoad.curMaxValue += newAttributeToLoad.curMaxValue *
                                                                      (CombatManager.Instance.GetTotalOfStatType(
                                                                          ownerCombatInfo,
                                                                          RPGStat.STAT_TYPE.MINION_HEALTH) / 100);
                                    newAttributeToLoad.curValue = newAttributeToLoad.curMaxValue;
                                }
                            }
                        }
                    }
                    else
                    {
                        newAttributeToLoad.curMinValue = t.minValue;
                        newAttributeToLoad.curValue = t.baseValue;
                        newAttributeToLoad.curMaxValue = t.maxValue;

                        if (nodeType == COMBAT_NODE_TYPE.pet)
                        {
                            if (newAttributeToLoad.stat._name ==
                                RPGBuilderEssentials.Instance.healthStatReference._name)
                            {
                                newAttributeToLoad.curMaxValue += newAttributeToLoad.curMaxValue *
                                                                  (CombatManager.Instance.GetTotalOfStatType(
                                                                      ownerCombatInfo,
                                                                      RPGStat.STAT_TYPE.MINION_HEALTH) / 100);
                                newAttributeToLoad.curValue = newAttributeToLoad.curMaxValue;
                            }
                        }
                    }

                    if (speciesREF != null)
                    {
                        foreach (var stat in speciesREF.stats)
                        {
                            if(stat.statID != t.ID) continue;
                            if (RPGBuilderUtilities.GetStatFromID(stat.statID).isVitalityStat)
                            {
                                newAttributeToLoad.curMaxValue += stat.value;
                                newAttributeToLoad.curValue = newAttributeToLoad.curMaxValue;
                            }
                            else
                            {
                                newAttributeToLoad.curValue += stat.value;
                            }
                        }
                    }
                }

                nodeStats.Add(newAttributeToLoad);
            }

            if (nodeType != COMBAT_NODE_TYPE.player) return;
            StatCalculator.InitCharacterStats();
            CombatManager.Instance.InitAllStatBar();
        }

        private int getCustomStatIndex(RPGStat stat)
        {
            for (var i = 0; i < npcDATA.stats.Count; i++)
                if (npcDATA.stats[i].statID == stat.ID)
                    return i;
            return -1;
        }

        private bool npcHasCustomStat(RPGStat stat)
        {
            foreach (var t in npcDATA.stats)
                if (t.statID == stat.ID)
                    return true;

            return false;
        }

        private void Awake()
        {
            if (nodeType != COMBAT_NODE_TYPE.player) return;
            playerControllerEssentials = GetComponent<RPGBCharacterControllerEssentials>();
        }
        
        private void Start()
        {
            switch (nodeType)
            {
                // INIT PLAYER UI
                case COMBAT_NODE_TYPE.player:
                    CombatManager.playerCombatNode = this;
                    CombatManager.groundIndicatorManager = indicatorManagerREF;
                    break;
                case COMBAT_NODE_TYPE.objectAction:
                {
                    InitNPCLevel();
                    InitStats();
                    if (thisRendererREF != null && npcDATA.isNameplateEnabled)
                    {
                        ScreenSpaceNameplates.Instance.RegisterNewNameplate(thisRendererREF, this, gameObject, nameplateYOffset,false);
                    }

                    break;
                }
            }

            nodeSocketsREF = GetComponentInChildren<NodeSockets>();
        }

        public void InitializeCombatNode()
        {
            if (nodeType != COMBAT_NODE_TYPE.mob && nodeType != COMBAT_NODE_TYPE.pet) return;
            if (agentREF != null) agentREF.SetCachedSpawnPos(transform.position);
            InitNPCAbilities();
            InitNPCLevel();
            InitStats();
            if(!npcDATA.isCollisionEnabled) InitCollisions();
            if (thisRendererREF != null && npcDATA.isNameplateEnabled)
            {
                ScreenSpaceNameplates.Instance.RegisterNewNameplate(thisRendererREF, this, gameObject, nameplateYOffset,false);
            }

            if (nodeType == COMBAT_NODE_TYPE.pet)
                agentREF.curMovementState = RPGBAIAgent.AGENT_MOVEMENT_STATES.followOwner;
        }

        private void InitCollisions()
        {
            Collider collider = gameObject.GetComponent<Collider>();
            foreach (var t in CombatManager.Instance.allCombatNodes)
            {
                Physics.IgnoreCollision(collider,
                    t.nodeType == COMBAT_NODE_TYPE.player
                        ? t.gameObject.GetComponent<CharacterController>()
                        : t.gameObject.GetComponent<Collider>());
            }
        }

        private void InitNPCLevel()
        {
            int finalLevel = 0;
            switch (nodeType)
            {
                case COMBAT_NODE_TYPE.mob:
                    finalLevel = npcDATA.isScalingWithPlayer ? RPGBuilderUtilities.getCurrentPlayerLevel() : Random.Range(npcDATA.MinLevel, npcDATA.MaxLevel + 1);
                    break;
                case COMBAT_NODE_TYPE.pet:
                    finalLevel = scaleWithOwner ? RPGBuilderUtilities.getCurrentPlayerLevel() : Random.Range(npcDATA.MinLevel, npcDATA.MaxLevel + 1);
                    break;
            }
            
            finalLevel = (int)GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.NPC + "+" +
                RPGGameModifier.NPCModifierType.Level,
                finalLevel, npcDATA.ID, -1);
            
            NPCLevel = finalLevel;
        }

        private void InitNPCAbilities()
        {
            if (npcDATA == null) return;
            foreach (var t in npcDATA.abilities)
            {
                var abilityREF = RPGBuilderUtilities.GetAbilityFromID(t.abilityID);
                if (abilityREF == null) continue;
                var newAb = new AbilitiesDATA();
                newAb.currentAbility = abilityREF;
                newAb.curAbilityID = abilityREF.ID;
                abilitiesData.Add(newAb);
            }
        }

        bool statHasDeathCondition(RPGStat statREF)
        {
            foreach (RPGStat.VitalityActions t in statREF.vitalityActions)
            {
                if (t.type == RPGStat.VitalityActionsTypes.Death) return true;
            }

            return false;
        }

        private void CheckAutoEnableAttacking(CombatNode attacker)
        {
            if (this == CombatManager.playerCombatNode || attacker != CombatManager.playerCombatNode) return;
            if (CombatManager.Instance.PlayerTargetData.currentTarget == this && !CombatManager.Instance.PlayerTargetData.isAttacking)
            {
                CombatManager.Instance.SetPlayerTarget(CombatManager.Instance.PlayerTargetData.currentTarget, true);
            }
        }

        public void AlterVitalityStat(int Amount, int alteredStatID)
        {
            if (dead) return;
            
            RPGStat alteredStatREF = RPGBuilderUtilities.GetStatFromID(alteredStatID);
            if (alteredStatREF == null) return;
            
            UpdateStat(alteredStatREF._name, "curBase", -Amount);
        }
        
        public void TakeDamage(CombatNode attacker, int Amount, RPGAbility.RPGAbilityRankData rankREF,
            int alteredStatID)
        {
            if (dead) return;
            
            if(attacker.currentPets.Count > 0) CombatManager.Instance.CheckIfPetsShouldAttack(attacker, this);
            
            CheckAutoEnableAttacking(attacker);

            RPGStat alteredStatREF = RPGBuilderUtilities.GetStatFromID(alteredStatID);
            if (alteredStatREF == null) return;
            bool isHealthDamage = alteredStatID == RPGBuilderEssentials.Instance.combatSettings.healthStatID;

            if (statHasDeathCondition(alteredStatREF))
            {
                latestAttacker = attacker;
                latestRankREF = rankREF;
            }

            ScreenSpaceNameplates.Instance.SetNPToVisible(this);
            if (nodeType == COMBAT_NODE_TYPE.mob || nodeType == COMBAT_NODE_TYPE.pet || nodeType == COMBAT_NODE_TYPE.objectAction)
            {
                if (agentREF != null && isHealthDamage && !attacker.dead) agentREF.AlterThreatTable(attacker, Amount);
                if (npcDATA.isDummyTarget)
                {
                    if (Amount >= nodeStats[getStatIndexFromName(alteredStatREF._name)].curValue)
                        Heal((int) nodeStats[getStatIndexFromName(alteredStatREF._name)].curMaxValue, alteredStatID);
                    else
                        UpdateStat(alteredStatREF._name, "curBase", -Amount);
                }
                else
                {
                    UpdateStat(alteredStatREF._name, "curBase", -Amount);
                }

                if (npcDATA.npcType == RPGNpc.NPC_TYPE.BOSS)
                {
                    if (BossUISlotHolder.Instance.thisNode == null)
                    {
                        BossUISlotHolder.Instance.Init(this);
                    }
                }

            }
            else
            {
                UpdateStat(alteredStatREF._name, "curBase", -Amount);
            }

            if ((this == CombatManager.playerCombatNode && !CombatManager.Instance.inCombatOverriden && RPGBuilderEssentials.Instance.combatSettings.useAutomaticCombatState) ||
                this != CombatManager.playerCombatNode)
            {
               CombatManager.Instance.HandleCombatAction(this);
            }

            if ((attacker == CombatManager.playerCombatNode && !CombatManager.Instance.inCombatOverriden && RPGBuilderEssentials.Instance.combatSettings.useAutomaticCombatState) ||
                attacker != CombatManager.playerCombatNode)
            {
                if (RPGBuilderEssentials.Instance.combatSettings.useAutomaticCombatState)CombatManager.Instance.HandleCombatAction(attacker);
            }
        }


        public void Heal(int Amount, int alteredStatID)
        {
            RPGStat alteredStatREF = RPGBuilderUtilities.GetStatFromID(alteredStatID);
            if (alteredStatREF == null) return;
            bool isHealthHeal = alteredStatID == RPGBuilderEssentials.Instance.combatSettings.healthStatID;
            UpdateStat(alteredStatREF._name, "curBase", Amount);
            if (this != CombatManager.playerCombatNode)
            {
                ScreenSpaceNameplates.Instance.SetNPToVisible(this);
                ScreenSpaceNameplates.Instance.UpdateNPBar(this);
            }

            if (!isHealthHeal)
            {
                if (nodeType == COMBAT_NODE_TYPE.pet)
                    if (CombatManager.playerCombatNode.currentPets.Contains(this))
                        PetPanelDisplayManager.Instance.UpdateHealthBar();
                if (this == CombatManager.Instance.PlayerTargetData.currentTarget)
                    TargetInfoDisplayManager.Instance.UpdateTargetHealthBar();
            }
            else
            {
                if (alteredStatREF._name != "Energy") return;
                if (this == CombatManager.Instance.PlayerTargetData.currentTarget)
                    TargetInfoDisplayManager.Instance.UpdateTargetEnergyBar();
            }
        }

        
        
        private void UpdateStat(string _name, string valueType, float Amount)
        {
            var statIndex = getStatIndexFromName(_name);
            if (statIndex == -1) return;
            float newValue = 0;
            bool triggerVitalityActions = false;
            switch (valueType)
            {
                case "curMin":
                    newValue = nodeStats[statIndex].curMinValue += Amount;
                    nodeStats[statIndex].curMinValue = newValue;
                    break;
                case "curBase":
                    newValue = nodeStats[statIndex].curValue += Amount;
                    nodeStats[statIndex].curValue = newValue;
                    triggerVitalityActions = nodeStats[statIndex].stat.isVitalityStat;
                    break;
                case "curMax":
                    newValue = nodeStats[statIndex].curMaxValue += Amount;
                    nodeStats[statIndex].curMaxValue = newValue;
                    break;
                case "defaultMin":
                    nodeStats[statIndex].stat.minValue = newValue;
                    break;
                case "defaultBase":
                    nodeStats[statIndex].stat.baseValue = newValue;
                    break;
                case "defaultMax":
                    nodeStats[statIndex].stat.maxValue = newValue;
                    break;
            }
            
            StatCalculator.ClampStat(nodeStats[statIndex].stat, this);
            
            if (triggerVitalityActions)
            {
                CombatManager.Instance.HandleVitalityStatActions(this, nodeStats[statIndex].stat, statIndex);
            }

            if (nodeStats[statIndex].stat._name != RPGBuilderEssentials.Instance.healthStatReference._name)
            {
                HandleActionsForSpecialStats(nodeStats[statIndex].stat._name);
            }
            else
            {
                CombatManager.Instance.HandleHealthStatChange(this);
            }
        }

        private void OnDestroy()
        {
            if (RPGBuilderEssentials.Instance.getCurrentScene().name ==
                RPGBuilderEssentials.Instance.generalSettings.mainMenuSceneName) return;
            CombatManager.Instance.RemoveCombatNodeFromList(this);
            OnMouseExit();
        }

        private void OnMouseDown()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return;
            if ((nodeType == COMBAT_NODE_TYPE.player &&
                 RPGBuilderEssentials.Instance.combatSettings.targetPlayerOnClick) ||
                npcDATA != null && npcDATA.isTargetable)
            {
                CombatManager.Instance.SetPlayerTarget(this, false);
            }
        }

        private void OnMouseOver()
        {
            if (nodeType != COMBAT_NODE_TYPE.mob && nodeType != COMBAT_NODE_TYPE.pet) return;
            if (RPGBuilderUtilities.IsPointerOverUIObject())
            {
                CursorManager.Instance.ResetCursor();
                return;
            }
            
            RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment = FactionManager.Instance.GetAlignmentForPlayer(npcDATA.factionID);

            if (Input.GetMouseButtonUp(1))
            {
                HandleInteraction(thisNodeAlignment, true);
            }

            switch (npcDATA.npcType)
            {
                case RPGNpc.NPC_TYPE.MERCHANT:
                    CursorManager.Instance.SetCursor(CursorManager.cursorType.merchant);
                    break;
                case RPGNpc.NPC_TYPE.QUEST_GIVER:
                    CursorManager.Instance.SetCursor(CursorManager.cursorType.questGiver);
                    break;
                case RPGNpc.NPC_TYPE.MOB when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    CursorManager.Instance.SetCursor(CursorManager.cursorType.enemy);
                    break;
                case RPGNpc.NPC_TYPE.RARE when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    CursorManager.Instance.SetCursor(CursorManager.cursorType.enemy);
                    break;
                case RPGNpc.NPC_TYPE.BOSS when thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    CursorManager.Instance.SetCursor(CursorManager.cursorType.enemy);
                    break;
            }
        }

        private void HandleInteraction(RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment, bool click)
        {
            if (click && (thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY ||
                thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL))
            {
                if (npcDATA == null || !npcDATA.isTargetable) return;
                CombatManager.Instance.SetPlayerTarget(this, true);
                return;
            }

            if (Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) < 3)
                switch (npcDATA.npcType)
                {
                    case RPGNpc.NPC_TYPE.MERCHANT:
                    {
                        if (MerchantPanelDisplayManager.Instance.thisCG.alpha == 0)
                            MerchantPanelDisplayManager.Instance.Show(this);
                        break;
                    }
                    case RPGNpc.NPC_TYPE.QUEST_GIVER:
                    {
                        CharacterEventsManager.Instance.TalkedToNPC(npcDATA);
                        if (QuestInteractionDisplayManager.Instance.thisCG.alpha == 0)
                            QuestInteractionDisplayManager.Instance.Show(this);
                        break;
                    }
                    case RPGNpc.NPC_TYPE.DIALOGUE:
                    {
                        DialogueDisplayManager.Instance.InitDialogue(this);
                        break;
                    }
                }
            else
            {
                if (CombatManager.playerCombatNode.playerControllerEssentials.GETControllerType() ==
                    RPGGeneralDATA.ControllerTypes.TopDownClickToMove)
                {

                }
                else
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This is too far", 3);
                }
            }

        }

        private void OnMouseExit()
        {
            if (nodeType != COMBAT_NODE_TYPE.mob && nodeType != COMBAT_NODE_TYPE.pet) return;
            RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment = FactionManager.Instance.GetAlignmentForPlayer(npcDATA.factionID);
            if (npcDATA.npcType == RPGNpc.NPC_TYPE.MERCHANT || npcDATA.npcType == RPGNpc.NPC_TYPE.QUEST_GIVER || thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ENEMY)
                CursorManager.Instance.ResetCursor();
        }

        private CombatNode latestAttacker;
        private RPGAbility.RPGAbilityRankData latestRankREF;
        public void DEATH()
        {
            if (dead) return;
            dead = true;
            switch (nodeType)
            {
                case COMBAT_NODE_TYPE.mob:
                {
                    var respawnTime = Random.Range(npcDATA.MinRespawn, npcDATA.MaxRespawn);
                    respawnTime = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                        RPGGameModifier.CategoryType.Combat + "+" +
                        RPGGameModifier.CombatModuleType.NPC + "+" +
                        RPGGameModifier.NPCModifierType.Respawn_Time,
                        respawnTime, npcDATA.ID, -1);
                    if (spawnerREF != null)
                    {
                        spawnerREF.StartCoroutine(spawnerREF.ExecuteSpawner(respawnTime));
                    }
                    CombatManager.Instance.HandleCombatNodeDEATH(this);
                    break;
                }
                case COMBAT_NODE_TYPE.pet:
                case COMBAT_NODE_TYPE.player:
                case COMBAT_NODE_TYPE.objectAction:
                    CombatManager.Instance.HandleCombatNodeDEATH(this);
                    break;
            }
            CombatManager.Instance.HandleOnKillActions(latestAttacker, this, latestRankREF);
        }
        
        protected bool effectTriggered;
        protected virtual void TriggerEffect() {
            if (IsCasting) {
                effectTriggered = true;
            }
        }

        public void InitRespawn(Vector3 respawnPOS)
        {
            playerControllerEssentials.CancelDeath();
            
            playerControllerEssentials.TeleportToTarget(respawnPOS);
            dead = false;
            latestAttacker = null;
            latestRankREF = null;
        }

        public void HandleActionsForSpecialStats(string statName)
        {
            if (nodeType == COMBAT_NODE_TYPE.player) CombatManager.Instance.StatBarUpdate(statName);
            if(this == CombatManager.Instance.PlayerTargetData.currentTarget && statName == "Energy") TargetInfoDisplayManager.Instance.UpdateTargetEnergyBar(); 
        }

        public int getStatIndexFromName(string statname)
        {
            for (var i = 0; i < nodeStats.Count; i++)
                if (nodeStats[i]._name == statname)
                    return i;
            return -1;
        }

        public int getStateIndexFromEffectID(int ID)
        {
            for (var i = 0; i < nodeStateData.Count; i++)
                if (nodeStateData[i].stateEffect.ID == ID)
                    return i;
            return -1;
        }

        public void InitActiveBlocking(RPGEffect.RPGEffectRankData blockEffect)
        {
            curBlockEffect = blockEffect;
            isActiveBlocking = true;
            curBlockChargeTime = 0;
            curBlockHitCount = 0;

            if (blockEffect.blockEndType == RPGEffect.BLOCK_END_TYPE.MaxDamageBlocked)
            {
                curBlockedDamageLeft = blockEffect.blockMaxDamage;
                if (this == CombatManager.playerCombatNode)
                {
                    ActiveBlockingDisplayManager.Instance.UpdateDamageBlockedLeft();
                }
            }
            
            if (blockEffect.isBlockChargeTime)
            {
                targetBlockChargeTime = blockEffect.blockChargeTime;
                targetBlockChargeTime *= 1 - (CombatManager.Instance.GetTotalOfStatType(this,
                    RPGStat.STAT_TYPE.BLOCK_ACTIVE_CHARGE_TIME_SPEED_MODIFIER) / 100f);
            }
            else
            {
                blockIsDoneCharging = true;
                targetBlockChargeTime = 0;
            }
            if (blockEffect.isBlockLimitedDuration)
            {
                blockDurationLeft = blockEffect.blockDuration;
                blockDurationLeft +=  blockDurationLeft * (CombatManager.Instance.GetTotalOfStatType(this,
                    RPGStat.STAT_TYPE.BLOCK_ACTIVE_DURATION_MODIFIER) / 100f);
                cachedBlockMaxDuration = blockDurationLeft;
            }
            else
            {
                blockDurationLeft = 0;
            }

            curBlockPowerFlat = blockEffect.blockPowerFlat + (int)CombatManager.Instance.GetTotalOfStatType(this,
                RPGStat.STAT_TYPE.BLOCK_ACTIVE_FLAT_AMOUNT);
            curBlockPowerModifier = blockEffect.blockPowerModifier + (int)CombatManager.Instance.GetTotalOfStatType(this,
                RPGStat.STAT_TYPE.BLOCK_ACTIVE_PERCENT_AMOUNT);
            if (curBlockPowerModifier > 100)
            {
                curBlockPowerModifier = 100;
            }
            
            if (this == CombatManager.playerCombatNode)
            {
                ActiveBlockingDisplayManager.Instance.Init();
                playerControllerEssentials.anim.SetBool("isActiveBlocking", true);
            }
        }

        public void ResetActiveBlocking()
        {
            isActiveBlocking = false;
            curBlockChargeTime = 0;
            targetBlockChargeTime = 0;
            blockDurationLeft = 0;
            curBlockPowerFlat = 0;
            curBlockHitCount = 0;
            curBlockPowerModifier = 0;
            curBlockedDamageLeft = 0;
            blockIsDoneCharging = false;

            if (this == CombatManager.playerCombatNode)
            {
                ActiveBlockingDisplayManager.Instance.Reset();
                playerControllerEssentials.anim.SetBool("isActiveBlocking", false);
            }
        }

        public void ReduceBlockedDamageLeft(int amount)
        {
            curBlockedDamageLeft -= amount;
            
            if(curBlockedDamageLeft <= 0) ResetActiveBlocking();
        }

        public IEnumerator IncreaseBlockHitCount()
        {
            curBlockHitCount++;
            yield return new WaitForEndOfFrame();
            if (curBlockHitCount >= (curBlockEffect.blockHitCount + CombatManager.Instance.GetTotalOfStatType(this,
                RPGStat.STAT_TYPE.BLOCK_ACTIVE_COUNT)))
            {
                ResetActiveBlocking();
            }
        }

        private bool blockIsDoneCharging;

        private void HandleActiveBlocking()
        {
            if (curBlockEffect.isBlockChargeTime && !blockIsDoneCharging)
            {
                // TODO AFFECT THIS BY STATS
                curBlockChargeTime += Time.deltaTime;

                if (this == CombatManager.playerCombatNode)
                {
                    ActiveBlockingDisplayManager.Instance.durationBar.fillAmount =
                        curBlockChargeTime / targetBlockChargeTime;
                    ActiveBlockingDisplayManager.Instance.durationBar.color =
                        ActiveBlockingDisplayManager.Instance.barChargingColor;
                    ActiveBlockingDisplayManager.Instance.durationText.text =
                        (targetBlockChargeTime - curBlockChargeTime).ToString("F1") + "s";
                }

                if (curBlockChargeTime >= targetBlockChargeTime)
                {
                    blockIsDoneCharging = true;
                    if (this == CombatManager.playerCombatNode)
                    {
                        ActiveBlockingDisplayManager.Instance.durationBar.color =
                            ActiveBlockingDisplayManager.Instance.barActiveColor;
                    }
                }
            }

            if (!blockIsDoneCharging) return;

            if (curBlockEffect.blockStatDecay && curBlockEffect.blockStatID != -1)
            {
                if (Time.time >= nextBlockStatDrain)
                {
                    nextBlockStatDrain = Time.time + curBlockEffect.blockStatDecayInterval;
                    AlterVitalityStat(curBlockEffect.blockStatDecayAmount, curBlockEffect.blockStatID);
                }
            }

            if (curBlockEffect.blockDurationType == RPGEffect.BLOCK_DURATION_TYPE.Time && curBlockEffect.isBlockLimitedDuration && blockDurationLeft > 0)
            {
                blockDurationLeft -= Time.deltaTime;

                if (this == CombatManager.playerCombatNode)
                {
                    ActiveBlockingDisplayManager.Instance.durationBar.fillAmount =
                        blockDurationLeft / cachedBlockMaxDuration;
                    ActiveBlockingDisplayManager.Instance.durationText.enabled = true;
                    ActiveBlockingDisplayManager.Instance.durationText.text = blockDurationLeft.ToString("F1") + "s";
                }

                if (blockDurationLeft <= 0)
                {
                    ResetActiveBlocking();
                }
            }
            else
            {
                ActiveBlockingDisplayManager.Instance.durationText.enabled = false;
            }

            if (this == CombatManager.playerCombatNode)
            {
                ActiveBlockingDisplayManager.Instance.powerFlat.text = curBlockPowerFlat.ToString("F0");
                ActiveBlockingDisplayManager.Instance.powerModifier.text = curBlockPowerModifier.ToString("F0") + " %";
            }

            if (curBlockEffect.isBlockPowerDecay)
            {
                float decayAmount = Time.deltaTime;
                decayAmount *= (curBlockEffect.blockPowerDecay + (curBlockEffect.blockPowerDecay *
                                                                  CombatManager.Instance.GetTotalOfStatType(this,
                                                                      RPGStat.STAT_TYPE.BLOCK_ACTIVE_DECAY_MODIFIER)/100f));
                curBlockPowerFlat -= decayAmount;
                if (curBlockPowerFlat < 0) curBlockPowerFlat = 0;
                curBlockPowerModifier -= decayAmount;
                if (curBlockPowerModifier < 0) curBlockPowerModifier = 0;
            }
        }

        private bool HasInteractions()
        {
            return npcDATA != null && (npcDATA.npcType == RPGNpc.NPC_TYPE.BANK ||
                                       npcDATA.npcType == RPGNpc.NPC_TYPE.DIALOGUE ||
                                       npcDATA.npcType == RPGNpc.NPC_TYPE.MERCHANT ||
                                       npcDATA.npcType == RPGNpc.NPC_TYPE.QUEST_GIVER);
        }

        public void Interact()
        {
            if (this == CombatManager.playerCombatNode || !HasInteractions() || RPGBuilderUtilities.IsPointerOverUIObject()) return;
            if (!(Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) <= 3)) return;
            RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment = FactionManager.Instance.GetAlignmentForPlayer(npcDATA.factionID);
            HandleInteraction(thisNodeAlignment, false);
        }

        public void ShowInteractableUI()
        {
            if (CombatManager.playerCombatNode == null) return;
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + npcDATA.nameplateYOffset+0.25f, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return npcDATA == null ? "" : npcDATA.displayName;
        }

        public bool isReadyToInteract()
        {
            return HasInteractions();
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            if (this == CombatManager.playerCombatNode || npcDATA == null) return RPGCombatDATA.INTERACTABLE_TYPE.None;
            
            switch (FactionManager.Instance.GetAlignmentForPlayer(npcDATA.factionID))
            {
                case RPGCombatDATA.ALIGNMENT_TYPE.ALLY:
                    return RPGCombatDATA.INTERACTABLE_TYPE.AlliedUnit;
                case RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                    return RPGCombatDATA.INTERACTABLE_TYPE.NeutralUnit;
                case RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    return RPGCombatDATA.INTERACTABLE_TYPE.EnemyUnit;
                default:
                    return RPGCombatDATA.INTERACTABLE_TYPE.None;
            }
        }
    }
}