using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.World
{
    public class InteractiveNode : MonoBehaviour, IPlayerInteractable
    {
        public enum InteractiveNodeType
        {
            resourceNode,
            effectNode,
            abilityNode,
            questNode,
            giveTreePoint,
            teachSkill,
            giveClassEXP,
            giveSkillEXP,
            completeTask,
            container,
            UnityEvent
        }

        public InteractiveNodeType nodeType;


        [Serializable]
        public class containerLootTablesDATA
        {
            public RPGLootTable lootTable;
            public float chance = 100f;
        }

        public List<containerLootTablesDATA> containerTablesData = new List<containerLootTablesDATA>();

        [Serializable]
        public class effectsDATA
        {
            public RPGEffect effect;
            public float chance = 100f;
        }

        public List<effectsDATA> effectsData = new List<effectsDATA>();

        [Serializable]
        public class abilitiesDATA
        {
            public RPGAbility ability;
            public float chance = 100f;
        }

        public List<abilitiesDATA> abilitiesData = new List<abilitiesDATA>();

        [Serializable]
        public class questsDATA
        {
            public RPGQuest quest;
            public float chance = 100f;
        }

        public questsDATA questsData;

        [Serializable]
        public class treePointsDATA
        {
            public RPGTreePoint treePoint;
            public int amount;
            public float chance = 100f;
        }

        public List<treePointsDATA> treePointsData = new List<treePointsDATA>();

        [Serializable]
        public class skillsDATA
        {
            public RPGSkill skill;
            public float chance = 100f;
        }

        public List<skillsDATA> skillsData = new List<skillsDATA>();

        [Serializable]
        public class classExpDATA
        {
            public int expAmount;
            public float chance = 100f;
        }

        public classExpDATA classExpData;

        [Serializable]
        public class skillExpDATA
        {
            public RPGSkill skill;
            public int expAmount;
            public float chance = 100f;
        }

        public List<skillExpDATA> skillExpData = new List<skillExpDATA>();

        [Serializable]
        public class taskDATA
        {
            public RPGTask task;
            public float chance = 100f;
        }

        public List<taskDATA> taskData = new List<taskDATA>();

        public RPGResourceNode resourceNodeData;

        public UnityEvent unityEvent;

        public enum InteractiveNodeState
        {
            ready,
            cooldown,
            disabled
        }

        public InteractiveNodeState nodeState;

        public int useCount;
        public float cooldown, nextUse, interactionTime, useDistanceMax = 2;

        public GameObject readyVisual, onCooldownVisual, disabledVisual;
        public GameObject currentVisualGO;
        public bool isTrigger, isClick = true;

        [SerializeField]
        public List<RequirementsManager.RequirementDATA> useRequirement = new List<RequirementsManager.RequirementDATA>();

        public string nodeUseAnimation;

        public AudioClip nodeUseSound;

        public float interactableUIOffsetY = 2;
        public string interactableName;

        public List<RPGCombatDATA.CombatVisualEffect> visualEffects = new List<RPGCombatDATA.CombatVisualEffect>();
        public List<RPGCombatDATA.CombatVisualAnimation> visualAnimations = new List<RPGCombatDATA.CombatVisualAnimation>();

        public string weaponEquippedRequired;
        private void Start()
        {
            SetNodeState(InteractiveNodeState.ready);
        }

        private void HideAllVisuals()
        {
            if (readyVisual != null) readyVisual.SetActive(false);

            if (onCooldownVisual != null) onCooldownVisual.SetActive(false);

            if (disabledVisual != null) disabledVisual.SetActive(false);
        }

        private void ShowVisual(GameObject go)
        {
            if (go != null) go.SetActive(true);
        }

        private IEnumerator resetNode(float delay)
        {
            yield return new WaitForSeconds(delay);

            SetNodeState(InteractiveNodeState.ready);
        }

        private void SetNodeState(InteractiveNodeState newState)
        {
            nodeState = newState;
            HideAllVisuals();
            switch (nodeState)
            {
                case InteractiveNodeState.ready:
                    ShowVisual(readyVisual);
                    break;
                case InteractiveNodeState.cooldown:
                    ShowVisual(onCooldownVisual);
                    break;
                case InteractiveNodeState.disabled:
                    ShowVisual(disabledVisual);
                    break;
            }
        }

        public void UseNode()
        {
            if (!(Time.time >= nextUse)) return;

            if ((nodeType == InteractiveNodeType.resourceNode || nodeType == InteractiveNodeType.container) &&
                RPGBuilderUtilities.isInventoryFull())
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                return;
            }

            if (!string.IsNullOrEmpty(nodeUseAnimation))
            {
                Animator anim = GetComponent<Animator>();
                if (anim != null) anim.SetTrigger(nodeUseAnimation);
            }

            if (nodeUseSound != null)
            {
                RPGBuilderUtilities.PlaySound(gameObject, gameObject, nodeUseSound, true);
            }

            if (nodeType != InteractiveNodeType.resourceNode)
            {
                nextUse = Time.time + cooldown;
                StartCoroutine(resetNode(cooldown));
            }
            else
            {
                var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                var rankREF = resourceNodeData.ranks[curRank];
                nextUse = Time.time + rankREF.respawnTime;
                StartCoroutine(resetNode(rankREF.respawnTime));
            }

            SetNodeState(InteractiveNodeState.cooldown);
            
            if((InteractiveNode) WorldInteractableDisplayManager.Instance.cachedInteractable == this) WorldInteractableDisplayManager.Instance.Hide();

            var totalItemDropped = 0;
            switch (nodeType)
            {
                case InteractiveNodeType.container:
                    float LOOTCHANCEMOD = CombatManager.Instance.GetTotalOfStatType(CombatManager.playerCombatNode,
                        RPGStat.STAT_TYPE.LOOT_CHANCE_MODIFIER);
                    foreach (var t in containerTablesData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance != 0 && !(chance <= t.chance)) continue;
                        foreach (var t1 in t.lootTable.lootItems)
                        {
                            var itemDropAmount = Random.Range(0f, 100f);
                            if (LOOTCHANCEMOD > 0) itemDropAmount += itemDropAmount * (LOOTCHANCEMOD / 100);
                            if (!(itemDropAmount <= t1.dropRate)) continue;
                            var stack = 0;
                            if (t1.min ==
                                t1.max)
                                stack = t1.min;
                            else
                                stack = Random.Range(t1.min,
                                    t1.max + 1);

                            RPGItem itemREF = RPGBuilderUtilities.GetItemFromID(t1.itemID);
                            if (itemREF.dropInWorld && itemREF.itemWorldModel != null)
                            {
                                var newLoot = new InventoryManager.WorldLootItems_DATA();
                                newLoot.item = itemREF;
                                newLoot.count = stack;
                                GameObject newLootGO = Instantiate(itemREF.itemWorldModel, new Vector3(
                                    transform.position.x,
                                    transform.position.y + 1, transform.position.z), Quaternion.identity);
                                newLootGO.layer = itemREF.worldInteractableLayer;
                                newLoot.worldDroppedItemREF = newLootGO.AddComponent<WorldDroppedItem>();
                                newLoot.worldDroppedItemREF.curLifetime = 0;
                                newLoot.worldDroppedItemREF.maxDuration = itemREF.durationInWorld;
                                newLoot.worldDroppedItemREF.item = itemREF;
                                newLoot.itemDataID =
                                    RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID,
                                        CharacterData.ItemDataState.world);

                                newLoot.worldDroppedItemREF.InitPhysics();
                                InventoryManager.Instance.allWorldDroppedItems.Add(newLoot);
                            }
                            else
                            {
                                // TODOINVENTORYHANDLING SHOW A WINDOW, SO THAT WE CAN LEAVE THE ITEMS THAT COULD NOT BE LOOTED
                                RPGBuilderUtilities.HandleItemLooting(itemREF.ID, stack, false, true);
                            }

                            totalItemDropped++;
                        }
                    }

                    break;

                case InteractiveNodeType.resourceNode:
                    if (RPGBuilderUtilities.isResourceNodeKnown(resourceNodeData.ID))
                    {
                        var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                        var rankREF = resourceNodeData.ranks[curRank];
                        var lootTableREF = RPGBuilderUtilities.GetLootTableFromID(rankREF.lootTableID);
                        foreach (var t in lootTableREF.lootItems)
                        {
                            var chance = Random.Range(0f, 100f);
                            if (!(chance <= t.dropRate)) continue;
                            var stack = 0;
                            if (t.min == t.max)
                                stack = t.min;
                            else
                                stack = Random.Range(t.min,
                                    t.max + 1);

                            RPGItem itemREF = RPGBuilderUtilities.GetItemFromID(t.itemID);
                            if (itemREF.dropInWorld && itemREF.itemWorldModel != null)
                            {
                                var newLoot = new InventoryManager.WorldLootItems_DATA();
                                newLoot.item = itemREF;
                                newLoot.count = stack;
                                GameObject newLootGO = Instantiate(itemREF.itemWorldModel, new Vector3(
                                    transform.position.x,
                                    transform.position.y + 1, transform.position.z), Quaternion.identity);
                                newLootGO.layer = itemREF.worldInteractableLayer;
                                newLoot.worldDroppedItemREF = newLootGO.AddComponent<WorldDroppedItem>();
                                newLoot.worldDroppedItemREF.curLifetime = 0;
                                newLoot.worldDroppedItemREF.maxDuration = itemREF.durationInWorld;
                                newLoot.worldDroppedItemREF.item = itemREF;
                                newLoot.itemDataID =
                                    RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID,
                                        CharacterData.ItemDataState.world);

                                newLoot.worldDroppedItemREF.InitPhysics();
                                InventoryManager.Instance.allWorldDroppedItems.Add(newLoot);
                            }
                            else
                            {
                                // TODOINVENTORYHANDLING SHOW A WINDOW, SO THAT WE CAN LEAVE THE ITEMS THAT COULD NOT BE LOOTED
                                RPGBuilderUtilities.HandleItemLooting(itemREF.ID, stack, false, true);
                            }

                            totalItemDropped++;
                        }

                        LevelingManager.Instance.GenerateSkillEXP(resourceNodeData.skillRequiredID, rankREF.Experience);
                    }

                    break;

                case InteractiveNodeType.effectNode:
                    foreach (var t in effectsData)
                    {
                        var chance = Random.Range(0f, 100f);
                        if (t.chance == 0 || chance <= t.chance)
                            CombatManager.Instance.ExecuteEffect(CombatManager.playerCombatNode,
                                CombatManager.playerCombatNode, t.effect, 0, null, 0);
                    }

                    break;

                case InteractiveNodeType.questNode:
                    var chance2 = Random.Range(0f, 100f);
                    if (questsData.chance == 0 || chance2 <= questsData.chance)
                        QuestJournalDisplayManager.Instance.DisplayQuestContent(questsData.quest);

                    break;

                case InteractiveNodeType.giveTreePoint:
                    foreach (var t in treePointsData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance == 0 || chance <= t.chance)
                            TreePointsManager.Instance.AddTreePoint(t.treePoint.ID,
                                t.amount);
                    }

                    break;

                case InteractiveNodeType.giveClassEXP:
                {
                    if (!RPGBuilderEssentials.Instance.combatSettings.useClasses) return;
                    var chance = Random.Range(0, 100f);
                    if (classExpData.chance == 0 || chance <= classExpData.chance)
                        LevelingManager.Instance.AddClassXP(classExpData.expAmount);
                }
                    break;

                case InteractiveNodeType.giveSkillEXP:
                    foreach (var t in skillExpData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance == 0 || chance <= t.chance)
                            LevelingManager.Instance.AddSkillXP(t.skill.ID,
                                t.expAmount);
                    }

                    break;

                case InteractiveNodeType.completeTask:
                    foreach (var t in taskData)
                    {
                        var chance = Random.Range(0, 100f);
                        if (t.chance != 0 && !(chance <= t.chance)) continue;
                        for (var index = 0; index < CharacterData.Instance.questsData.Count; index++)
                        {
                            var quest = CharacterData.Instance.questsData[index];
                            for (var i = 0; i < quest.objectives.Count; i++)
                            {
                                var objective = quest.objectives[i];
                                if (objective.taskID != t.task.ID) continue;
                                TaskCheckerManager.Instance.CompleteTaskInstantly(index, i);
                                QuestTrackerDisplayManager.Instance.UpdateTrackerUI(
                                    RPGBuilderUtilities.GetQuestFromID(quest.questID), i);
                            }
                        }
                    }

                    break;

                case InteractiveNodeType.UnityEvent:
                    unityEvent.Invoke();
                    break;
            }
        }

        private void InitInteractionTime(float interactionTime)
        {
            CombatManager.playerCombatNode.InitInteracting(this, interactionTime);
            PlayerInfoDisplayManager.Instance.InitInteractionBar();
        }

        private bool UseRequirementsMet()
        {
            if (!string.IsNullOrEmpty(weaponEquippedRequired) &&
                !RPGBuilderUtilities.isWeaponTypeEquipped(weaponEquippedRequired))
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent(weaponEquippedRequired + " needs to be equipped", 3);
                return false;
            }

            switch (nodeType)
            {
                case InteractiveNodeType.resourceNode:
                    if (!RPGBuilderUtilities.isResourceNodeKnown(resourceNodeData.ID))
                    {
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent("This resource node is not unlocked", 3);
                        return false;
                    }

                    RPGResourceNode resourceNodeREF = RPGBuilderUtilities.GetResourceNodeFromID(resourceNodeData.ID);
                    int lvlRequired = resourceNodeREF.ranks[RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID)].skillLevelRequired;
                    if (RPGBuilderUtilities.getSkillLevel(resourceNodeREF.skillRequiredID) < lvlRequired)
                    {
                        ErrorEventsDisplayManager.Instance.ShowErrorEvent(RPGBuilderUtilities.GetSkillFromID(resourceNodeREF.skillRequiredID).displayName + " skill level " + lvlRequired + " required", 3);
                        return false;
                    }
                    break;
            }

            List<bool> reqResults = new List<bool>();
            foreach (var t in useRequirement)
            {
                var intValue1 = 0;
                var intValue2 = 0;
                switch (t.requirementType)
                {
                    case RequirementsManager.RequirementType.classLevel:
                        intValue1 = CharacterData.Instance.classDATA.currentClassLevel;
                        break;
                    case RequirementsManager.RequirementType.skillLevel:
                        intValue1 = RPGBuilderUtilities.getSkillLevel(t.skillRequiredID);
                        break;
                    case RequirementsManager.RequirementType._class:
                        intValue1 = t.classRequiredID;
                        break;
                    case RequirementsManager.RequirementType.weaponTemplateLevel:
                        intValue1 = RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateRequiredID);
                        break;
                }

                reqResults.Add(RequirementsManager.Instance.HandleRequirementType(t, intValue1, intValue2,true));
            }

            return !reqResults.Contains(false);
        }


        private void OnMouseOver()
        {
            if (!isClick) return;
            if (RPGBuilderUtilities.IsPointerOverUIObject())
            {
                CursorManager.Instance.ResetCursor();
                return;
            }
            if (Input.GetMouseButtonUp(1) && !CombatManager.playerCombatNode.isInteractiveNodeCasting)
                if (Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) <= useDistanceMax)
                {
                    if (UseRequirementsMet())
                    {
                        
                        CombatManager.Instance.ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType.Activate,
                            CombatManager.playerCombatNode, visualAnimations);
                        
                        if (nodeType == InteractiveNodeType.resourceNode)
                        {
                            var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                            var rankREF = resourceNodeData.ranks[curRank];
                            if (rankREF.gatherTime == 0)
                                UseNode();
                            else
                                InitInteractionTime(rankREF.gatherTime);
                        }
                        else
                        {
                            if (interactionTime == 0)
                                UseNode();
                            else
                                InitInteractionTime(interactionTime);
                        }
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

            if (nodeState == InteractiveNodeState.ready)
                CursorManager.Instance.SetCursor(CursorManager.cursorType.interactiveObject);
        }

        private void OnMouseExit()
        {
            CursorManager.Instance.ResetCursor();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isTrigger || other.gameObject != CombatManager.playerCombatNode.gameObject) return;
            if (CombatManager.playerCombatNode.isInteractiveNodeCasting || !UseRequirementsMet()) return;
            
            CombatManager.Instance.ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType.Activate,
                CombatManager.playerCombatNode, visualAnimations);
            
            if (nodeType == InteractiveNodeType.resourceNode)
            {
                var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                var rankREF = resourceNodeData.ranks[curRank];
                if (rankREF.gatherTime == 0)
                    UseNode();
                else
                    InitInteractionTime(rankREF.gatherTime);
            }
            else
            {
                if (interactionTime == 0)
                    UseNode();
                else
                    InitInteractionTime(interactionTime);
            }
        }

        public void ResetAllInteractionAnimations()
        {
            foreach (var visualAnimation in visualAnimations)
            {
                if(visualAnimation.activationType != RPGCombatDATA.CombatVisualActivationType.Activate) continue;
                if(visualAnimation.parameterType != RPGCombatDATA.CombatVisualAnimationParameterType.Bool) continue;
                
                CombatManager.playerCombatNode.playerControllerEssentials.anim.SetBool(visualAnimation.animationParameter, !visualAnimation.boolValue);
                CombatManager.Instance.ResetQueuedAnimation(CombatManager.playerCombatNode.playerControllerEssentials.anim, visualAnimation, CombatManager.playerCombatNode);
            }
        }
        
        public void Interact()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return;
            if (CombatManager.playerCombatNode.isInteractiveNodeCasting) return;
            if (!(Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) <= useDistanceMax)) return;
            if (!UseRequirementsMet()) return;

            CombatManager.Instance.ExecuteCombatVisualAnimationList(RPGCombatDATA.CombatVisualActivationType.Activate,
                CombatManager.playerCombatNode, visualAnimations);
            
            if (nodeType == InteractiveNodeType.resourceNode)
            {
                var curRank = RPGBuilderUtilities.getResourceNodeRank(resourceNodeData.ID);
                var rankREF = resourceNodeData.ranks[curRank];
                if (rankREF.gatherTime == 0)
                    UseNode();
                else
                    InitInteractionTime(rankREF.gatherTime);
            }
            else
            {
                if (interactionTime == 0)
                    UseNode();
                else
                    InitInteractionTime(interactionTime);
            }
        }

        public void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + interactableUIOffsetY, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            
            if ((InteractiveNode) WorldInteractableDisplayManager.Instance.cachedInteractable == this) return;
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return nodeType == InteractiveNodeType.resourceNode ? resourceNodeData.displayName : interactableName;
        }

        public bool isReadyToInteract()
        {
            return nodeState == InteractiveNodeState.ready;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.InteractiveNode;
        }
    }
}