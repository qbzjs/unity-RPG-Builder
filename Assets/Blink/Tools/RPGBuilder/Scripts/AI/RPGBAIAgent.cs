using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.AI
{
    public class RPGBAIAgent : MonoBehaviour
    {
        public enum AGENT_STATES
        {
            Idle = 0,
            Walk = 1,
            Run = 2,
            Combat = 3
        }
        public AGENT_STATES curState;

        public enum AGENT_MOVEMENT_STATES
        {
            idle,
            roam,
            chaseTarget,
            followOwner
        }
        public AGENT_MOVEMENT_STATES curMovementState;

        public NavMeshAgent thisAgent;
        public Animator thisAnim;

        public float StateUpdateRate = 0.2f, nextStateUpdate, rotationSpeed;

        public CombatNode thisCombatInfo;
        public CombatNode target;

        private bool isWaitingNextRoam;
        private float nextRoam;

        private float moveSpeed, cachedDefaultMoveSpeed = -1;

        private float nextAbilityCast;

        [System.Serializable]
        public class THREAT_TABLE_DATA
        {
            public CombatNode combatNode;
            public int curThreat;
        }
        public List<THREAT_TABLE_DATA> threatTable = new List<THREAT_TABLE_DATA>();

        private Vector3 cached_SpawnPos;

        public void SetCachedSpawnPos(Vector3 pos)
        {
            cached_SpawnPos = pos;
        }
        public void AlterThreatTable (CombatNode cbtNode, int Amount)
        {
            var cbtNodeIndex = getCombatNodeIndexInThreatTable(cbtNode);
            if(cbtNodeIndex != -1)
            {
                threatTable[cbtNodeIndex].curThreat += Amount;
            } else
            {
                var newThreatTableData = new THREAT_TABLE_DATA();
                newThreatTableData.combatNode = cbtNode;
                newThreatTableData.curThreat = Amount;
                threatTable.Add(newThreatTableData);
            }

            AssignHighestThreatTarget();
        }

        private THREAT_TABLE_DATA getThreatTableDATA (CombatNode cbtNode)
        {
            foreach (var t in threatTable)
                if (t.combatNode == cbtNode) return t;

            return null;
        }

        public void RemoveCombatNodeFromThreatTabble (CombatNode cbtNode)
        {
            var thisThreatTableDATA = getThreatTableDATA(cbtNode);
            if (thisThreatTableDATA != null) threatTable.Remove(thisThreatTableDATA);
            if (target == cbtNode)
            {
                target = null;
                nextRoam = 0;
                curMovementState = AGENT_MOVEMENT_STATES.idle;
                curState = AGENT_STATES.Idle;
                thisAgent.enabled = true;
            }

            if (threatTable.Count != 0) return;
            if (thisCombatInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet) return;
            curMovementState = AGENT_MOVEMENT_STATES.followOwner;
            curState = AGENT_STATES.Idle;
        }

        public void ClearThreatTable()
        {
            foreach (var t in threatTable)
            {
                if (target != t.combatNode) continue;
                target = null;
                nextRoam = 0;
                curMovementState = AGENT_MOVEMENT_STATES.idle;
                curState = AGENT_STATES.Idle;
                thisAgent.enabled = true;

            }
            threatTable.Clear();
            if (thisCombatInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet) return;
            curMovementState = AGENT_MOVEMENT_STATES.followOwner;
            curState = AGENT_STATES.Idle;
        }

        public void ResetTarget()
        {
            target = null;
            nextRoam = 0;
            curMovementState = AGENT_MOVEMENT_STATES.idle;
            curState = AGENT_STATES.Idle;
            thisAgent.enabled = true;

            if (thisCombatInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet) return;
            curMovementState = AGENT_MOVEMENT_STATES.followOwner;
        }

        private void AssignHighestThreatTarget ()
        {
            if (!thisCombatInfo.dead) SetTarget(getHighestThreatTarget());
        }

        private CombatNode getHighestThreatTarget ()
        {
            var highestThreat = -1;
            CombatNode cbtNode = null;

            for (var i = 0; i < threatTable.Count; i++)
            {
                if(threatTable[i].combatNode == null)
                {
                    threatTable.Remove(threatTable[i]);
                    continue;
                }

                if (threatTable[i].curThreat <= highestThreat) continue;
                highestThreat = threatTable[i].curThreat;
                cbtNode = threatTable[i].combatNode;
            }
            return cbtNode;
        }

        private int getCombatNodeIndexInThreatTable (CombatNode cbtNode)
        {
            for (var i = 0; i < threatTable.Count; i++)
                if(threatTable[i].combatNode == cbtNode) return i;
            return -1;
        }


        private void FixedUpdate()
        {
            if (thisCombatInfo.dead) return;
            UpdateState();
            if (thisCombatInfo.npcDATA.isMovementEnabled)
            {
                moveSpeed = RPGBuilderUtilities.getCurrentMoveSpeed(thisCombatInfo);
                if (cachedDefaultMoveSpeed == -1) cachedDefaultMoveSpeed = moveSpeed;
                switch (curMovementState)
                {
                    case AGENT_MOVEMENT_STATES.chaseTarget:
                    case AGENT_MOVEMENT_STATES.followOwner:
                        break;
                    default:
                        moveSpeed /= 2;
                        break;
                }
            }
            
            if (!thisCombatInfo.isStunned() && !thisCombatInfo.isSleeping())
            {
                OutsideStateLoopUpdate();
            } else
            {
                moveSpeed = 0;
                thisAgent.acceleration = 0;
            }

            thisAgent.speed = moveSpeed;
            thisAgent.acceleration = moveSpeed*3;

            HandleStandTime();
            HandleKnockback();
        }
        
        public bool knockbackActive;
        private Vector3 knockBackTarget;
        private float knockbackDistanceRequired;
        private Vector3 knockbackStartPOS;
        public void InitKnockback(float knockbackDistance, Transform attacker)
        {
            knockbackDistanceRequired = knockbackDistance;
            knockbackDistance *= 5;
            knockbackStartPOS = transform.position;
            knockbackActive = true;
            thisAgent.enabled = true;
            thisAgent.velocity = Vector3.zero;
            if(thisAgent.enabled) thisAgent.ResetPath();
            thisAgent.angularSpeed = 0;
            knockBackTarget = (transform.position - attacker.position).normalized * knockbackDistance;
            thisAgent.velocity = knockBackTarget;
        }

        private void resetKnockback()
        {
            knockbackActive = false;
            thisAgent.enabled = true;
            thisAgent.ResetPath();
            thisAgent.angularSpeed = thisCombatInfo.npcDATA.navmeshAgentAngularSpeed;
            thisAgent.speed = moveSpeed;
            thisAgent.acceleration = moveSpeed*3;
        }

        private void HandleKnockback()
        {
            if (!knockbackActive) return;
            if(thisCombatInfo.dead) resetKnockback();
            if (!(Vector3.Distance(knockbackStartPOS, transform.position) >= knockbackDistanceRequired)) return;
            thisAgent.velocity = Vector3.zero;
            resetKnockback();
        }

        private void HandleStandTime ()
        {
            if (!standTimeActive) return;
            currentStandTimeDur += Time.deltaTime;

            if (currentStandTimeDur >= maxStandTimeDur) resetStandTime();
        }

        private void OutsideStateLoopUpdate ()
        {
            if (target == null) return;
            if(knockbackActive) return;
            if (standTimeActive && !canRotateInStandTime) return;
            if (thisAgent.path != null && thisAgent.path.corners.Length > 0)
            {
                transform.LookAt(thisAgent.nextPosition);
            }
            else
            {
                transform.LookAt( new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
            }
        }


        private void UpdateState()
        {
            if (!(Time.time >= nextStateUpdate)) return;
            nextStateUpdate = Time.time + StateUpdateRate;
            
            if(thisCombatInfo.npcDATA.isMovementEnabled) HandleMovement();

            if (target == null)
            {
                if (thisCombatInfo.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                {
                    if(thisCombatInfo.ownerCombatInfo.currentPetsCombatActionType == CombatNode.PET_COMBAT_ACTION_TYPES.aggro) LookForTarget();
                }
                else
                {
                    LookForTarget();
                }
            }
            else
            {
                CheckTargetDistance();
                if(thisCombatInfo.npcDATA.isCombatEnabled)HandleCombat();
            }
            if (!standTimeActive && thisCombatInfo.npcDATA.isMovementEnabled) SetAnimation();
        }
    

        private float currentStandTimeDur, maxStandTimeDur;
        public bool standTimeActive;
        public bool canRotateInStandTime;
        public void InitStandTime(float max, bool canRotate)
        {
            standTimeActive = true;
            canRotateInStandTime = canRotate;
            currentStandTimeDur = 0;
            maxStandTimeDur = max;
            if(thisAgent.enabled) thisAgent.ResetPath();
            thisAgent.enabled = false;
            thisAgent.velocity = Vector3.zero;

            if (canAnimate()) thisAnim.SetInteger("MovementState", 3);
        }

        private void resetStandTime()
        {
            standTimeActive = false;
            currentStandTimeDur = 0;
            maxStandTimeDur = 0;
            thisAgent.enabled = true;
        }

        public void InitStun()
        {
            if(knockbackActive) resetKnockback();
            if(thisAgent.enabled) thisAgent.ResetPath();
            thisAgent.enabled = false;
            thisAgent.velocity = Vector3.zero;

            curMovementState = AGENT_MOVEMENT_STATES.idle;
            curState = AGENT_STATES.Idle;
            if (canAnimate()) thisAnim.SetInteger("MovementState", 0);
        }

        public void ResetStun()
        {
            thisAgent.enabled = true;
            if (canAnimate()) thisAnim.SetBool("Stunned", false);
            
            if(target == null)
            {
                switch (thisCombatInfo.nodeType)
                {
                    case CombatNode.COMBAT_NODE_TYPE.mob:
                        curMovementState = AGENT_MOVEMENT_STATES.idle;
                        break;
                    case CombatNode.COMBAT_NODE_TYPE.pet:
                        curMovementState = AGENT_MOVEMENT_STATES.followOwner;
                        break;
                }

                curState = AGENT_STATES.Idle;
            } else
            {
                curMovementState = AGENT_MOVEMENT_STATES.chaseTarget;
                curState = AGENT_STATES.Run;
            }
        }

        bool canBeAggroed(CombatNode t)
        {
            if (t.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
            {
                return true;
            }

            return t.npcDATA != null && t.npcDATA.isCombatEnabled;
        }

        private CombatNode getClosestNode ()
        {
            var potentialNodes = new List<CombatNode>();
            foreach (var t in CombatManager.Instance.allCombatNodes)
            {
                if(t == thisCombatInfo || t.isStealthed() || t.dead) continue;
                var dist = Vector3.Distance(thisCombatInfo.spawnerREF != null ? thisCombatInfo.spawnerREF.transform.position : cached_SpawnPos, t.transform.position);
                if (shouldAggro(t) && canBeAggroed(t) && dist <= GetResetDistance())
                {
                    potentialNodes.Add(t);
                }
            }

            CombatNode closestNode = null;
            float smallestDist = 0;

            foreach (var t in potentialNodes)
            {
                var dist = Vector3.Distance(transform.position, t.transform.position);
                if (smallestDist != 0 && !(dist < smallestDist)) continue;
                closestNode = t;
                smallestDist = dist;
            }
            return closestNode;
        }

        private bool shouldAggro (CombatNode potentialNode)
        {
            switch (FactionManager.Instance.GetCombatNodeAlignment(thisCombatInfo, potentialNode))
            {
                case RPGCombatDATA.ALIGNMENT_TYPE.ALLY:
                    return false;
                case RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                    return true;
                case RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                    return false;
            }

            return false;
        }

        private float GetAggroRange()
        {
            return GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.NPC + "+" +
                RPGGameModifier.NPCModifierType.Aggro_Range,
                thisCombatInfo.npcDATA.AggroRange, thisCombatInfo.npcDATA.ID, -1);
        }

        private float GetResetDistance()
        {
            return GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.NPC + "+" +
                RPGGameModifier.NPCModifierType.Reset_Target_Distance,
                thisCombatInfo.npcDATA.DistanceToTargetReset, thisCombatInfo.npcDATA.ID, -1);
        }

        private float GetRoamRange()
        {
            return GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.NPC + "+" +
                RPGGameModifier.NPCModifierType.Roam_Range,
                thisCombatInfo.npcDATA.RoamRange, thisCombatInfo.npcDATA.ID, -1);
        }

        private void LookForTarget ()
        {
            if (!CombatManager.Instance.combatEnabled) return;
            if (thisCombatInfo.dead) return;
            if (threatTable.Count != 0) return;
            var closestNode = getClosestNode();
            if (closestNode == null)
            {
                if (thisCombatInfo.nodeType != CombatNode.COMBAT_NODE_TYPE.pet) return;
                if (thisCombatInfo.ownerCombatInfo.currentPetsMovementActionType ==
                    CombatNode.PET_MOVEMENT_ACTION_TYPES.stay)
                {
                    curMovementState = AGENT_MOVEMENT_STATES.idle;
                    curState = AGENT_STATES.Idle;
                }
                else
                {
                    var distToOwner = Vector3.Distance(transform.position, thisCombatInfo.ownerCombatInfo.transform.position);
                    if (distToOwner <= thisCombatInfo.npcDATA.distanceFromOwner)
                    {
                        curMovementState = AGENT_MOVEMENT_STATES.followOwner;
                        curState = AGENT_STATES.Idle;
                    }
                    else
                    {
                        curMovementState = AGENT_MOVEMENT_STATES.followOwner;
                        curState = AGENT_STATES.Walk;
                    }
                }
                return;
            }
            var dist = Vector3.Distance(transform.position, closestNode.transform.position);
            if (!(dist < GetAggroRange())) return;
            target = closestNode;
            curMovementState = AGENT_MOVEMENT_STATES.chaseTarget;
            curState = AGENT_STATES.Run;
            thisAgent.enabled = true;

            if (closestNode.nodeType == CombatNode.COMBAT_NODE_TYPE.player &&
                CombatManager.Instance.PlayerTargetData.currentTarget == null)
            {
                CombatManager.Instance.SetPlayerTarget(thisCombatInfo, false);
            }

            if (thisCombatInfo.npcDATA.npcType != RPGNpc.NPC_TYPE.BOSS) return;
            if (BossUISlotHolder.Instance.thisNode == null || BossUISlotHolder.Instance.thisNode == thisCombatInfo) BossUISlotHolder.Instance.Init(thisCombatInfo);
        }

        public void SetTarget (CombatNode newTarget)
        {
            if (!CombatManager.Instance.combatEnabled) return;
            if (thisCombatInfo.dead) return;
            target = newTarget;
            curMovementState = AGENT_MOVEMENT_STATES.chaseTarget;
            curState = AGENT_STATES.Run;
            try
            {
                thisAgent.enabled = true;
            }
            catch
            {
                // ignored
            }
        }

        private void CheckTargetDistance()
        {
            float dist = 0;
            switch (thisCombatInfo.nodeType)
            {
                case CombatNode.COMBAT_NODE_TYPE.mob:
                    dist = Vector3.Distance(thisCombatInfo.spawnerREF != null
                        ? thisCombatInfo.spawnerREF.transform.position
                        : cached_SpawnPos, target.transform.position);
                    break;
                case CombatNode.COMBAT_NODE_TYPE.pet:
                    dist = Vector3.Distance(transform.position, target.transform.position);
                    break;
            }

            if (!(dist > (GetRoamRange() +GetResetDistance()))) return;
            target = null;
            nextRoam = 0;
            curMovementState = AGENT_MOVEMENT_STATES.idle;
            curState = AGENT_STATES.Idle;
            thisAgent.enabled = true;
        }

        private void HandleMovement()
        {
            if (knockbackActive || standTimeActive || thisCombatInfo.isStunned() || thisCombatInfo.isSleeping() ||
                thisCombatInfo.isRooted() || standTimeActive) return;
            if (target == null)
                switch (thisCombatInfo.nodeType)
                {
                    case CombatNode.COMBAT_NODE_TYPE.mob:
                        HandleMobRoaming();
                        break;
                    case CombatNode.COMBAT_NODE_TYPE.pet:
                        switch (thisCombatInfo.ownerCombatInfo.currentPetsMovementActionType)
                        {
                            case CombatNode.PET_MOVEMENT_ACTION_TYPES.stay:
                                curMovementState = AGENT_MOVEMENT_STATES.idle;
                                curState = AGENT_STATES.Idle;
                                thisAgent.enabled = false;
                                thisAgent.stoppingDistance = 0;
                                break;
                            case CombatNode.PET_MOVEMENT_ACTION_TYPES.follow:
                                HandlePetFollowOwner();
                                break;
                        }

                        break;
                }
            else
                HandleTargetChasing();
        }

        private void HandleTargetChasing ()
        {
            if (thisCombatInfo.dead) return;
            if (target.stealthed)
            {
                RemoveCombatNodeFromThreatTabble(target);
                return;
            }
            if (curMovementState != AGENT_MOVEMENT_STATES.chaseTarget &&
                curMovementState != AGENT_MOVEMENT_STATES.idle) return;
            if (curMovementState == AGENT_MOVEMENT_STATES.chaseTarget && curState == AGENT_STATES.Run && target == null)
            {
                ResetTarget();
                return;
            }
            var dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist >= thisCombatInfo.npcDATA.distanceFromTarget)
            {
                curMovementState = AGENT_MOVEMENT_STATES.chaseTarget;
                curState = AGENT_STATES.Run;
                thisAgent.enabled = true;
                thisAgent.stoppingDistance = thisCombatInfo.npcDATA.distanceFromTarget;
                thisAgent.SetDestination(target.transform.position);
            }
            else
            {
                curMovementState = AGENT_MOVEMENT_STATES.idle;
                curState = AGENT_STATES.Combat;
                if(thisAgent.enabled) thisAgent.ResetPath();
                thisAgent.enabled = false;
            }
        }

        private void HandleMobRoaming()
        {
            if (thisCombatInfo.dead) return;
            float roamRange = GetRoamRange();
            if (roamRange == 0) return;
            if (Time.time >= nextRoam && curMovementState == AGENT_MOVEMENT_STATES.idle)
            {
                nextRoam = Time.time + thisCombatInfo.npcDATA.RoamDelay;

                var newRoamDestination = new Vector3(
                    cached_SpawnPos.x + Random.Range(-roamRange,
                        roamRange), cached_SpawnPos.y,
                    cached_SpawnPos.z + Random.Range(-roamRange,
                        roamRange));

                curMovementState = AGENT_MOVEMENT_STATES.roam;
                curState = AGENT_STATES.Walk;
                thisAgent.enabled = true;
                thisAgent.stoppingDistance = 0;
                if(thisAgent.isOnNavMesh)thisAgent.SetDestination(newRoamDestination);

            }
            else if (curMovementState == AGENT_MOVEMENT_STATES.roam)
            {
                var dist = Vector3.Distance(transform.position, thisAgent.destination);
                if (!(dist < 1)) return;
                curMovementState = AGENT_MOVEMENT_STATES.idle;
                curState = AGENT_STATES.Idle;
                thisAgent.enabled = false;
                thisAgent.stoppingDistance = 0;
            }
        }

        private void HandlePetFollowOwner ()
        {
            if (thisCombatInfo.dead) return;
            if (curMovementState != AGENT_MOVEMENT_STATES.followOwner &&
                curMovementState != AGENT_MOVEMENT_STATES.idle) return;
            var dist = Vector3.Distance(transform.position, thisCombatInfo.ownerCombatInfo.transform.position);
            if (dist <= thisCombatInfo.npcDATA.distanceFromOwner)
            {
                curMovementState = AGENT_MOVEMENT_STATES.followOwner;
                curState = AGENT_STATES.Idle;
                thisAgent.enabled = false;
                return;
            }
            var newFollowDestination = thisCombatInfo.ownerCombatInfo.transform.position;
            newFollowDestination += thisCombatInfo.ownerCombatInfo.transform.forward * 1;

            curMovementState = AGENT_MOVEMENT_STATES.followOwner;
            curState = AGENT_STATES.Walk;
            thisAgent.enabled = true;
            thisAgent.stoppingDistance = 0;
            thisAgent.SetDestination(newFollowDestination);
        }

        private int lastAbilityUsedID = -1;
        private static readonly int movementState = Animator.StringToHash("MovementState");
        private static readonly int moveSpeedModifier = Animator.StringToHash("MoveSpeedModifier");

        private void HandleCombat()
        {
            if (!CombatManager.Instance.combatEnabled) return;
            for (var i = 0; i < thisCombatInfo.abilitiesData.Count; i++)
            {
                if (knockbackActive || standTimeActive || thisCombatInfo.abilitiesData[i].CDLeft != 0 || !(Time.time >= nextAbilityCast) || target == null || !shouldUseThisAb(thisCombatInfo.abilitiesData[i].curAbilityID)) continue;
                var dist = Vector3.Distance(transform.position, target.transform.position);
                if (!(dist <= thisCombatInfo.npcDATA.distanceFromTarget)) continue;
                nextAbilityCast = Time.time + CombatManager.Instance.NPC_GCD_DURATION;
                UseNPCAbility(i);
                lastAbilityUsedID = thisCombatInfo.abilitiesData[i].curAbilityID;
            }
        }

        private bool shouldUseThisAb(int ID)
        {
            if (lastAbilityUsedID == -1) return true;
            foreach (var ab in thisCombatInfo.abilitiesData)
            {
                if (ab.curAbilityID == lastAbilityUsedID || ab.curAbilityID == ID) continue;
                if (ab.CDLeft == 0) return true;
            }
            return true;
        }

        private void UseNPCAbility (int abilityIndex)
        {
            if (CombatManager.Instance == null || thisCombatInfo == null) return;
            CombatManager.Instance.InitAbility(thisCombatInfo, thisCombatInfo.abilitiesData[abilityIndex].currentAbility, true);
        }

        private void SetAnimation()
        {
            if (!canAnimate()) return;
            thisAnim.SetInteger(movementState, (int) curState);
            float speedMod = moveSpeed / cachedDefaultMoveSpeed;
            thisAnim.SetFloat(moveSpeedModifier, speedMod);
        }

        public bool canAnimate()
        {
            return thisAnim != null && thisAnim.runtimeAnimatorController != null;
        }
    }
}
