using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class CustomInputManager : MonoBehaviour
    {
        private string currentlyModifiedActionKey;
        private bool isKeyChecking;
        
        public List<CanvasGroup> allOpenedPanels = new List<CanvasGroup>();

        private int tabTargetSelected;
        private List<CombatNode> previouslySelectedCbtNodes = new List<CombatNode>();
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void AddOpenedPanel(CanvasGroup cg)
        {
            if(!allOpenedPanels.Contains(cg)) allOpenedPanels.Insert(0, cg);
        }

        private void Update()
        {
            if (!RPGBuilderEssentials.Instance.isInGame) return;
            if (Input.GetKeyDown(KeyCode.Escape) && !HandleEscape()) return;
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HandleTabTarget();
            }

            if (CombatManager.playerCombatNode == null) return;
            if (DevUIManager.Instance.thisCG.alpha == 1 && DevUIManager.Instance.IsTypingInField()) return;

            if (CheckComboKeys()) return;
            CheckActionKeys();
            HandleActionAbilities();
                    
            if (!isKeyChecking) return;
            HandleKeyChange();
        }

        void HandleActionAbilities()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return;
            foreach (var ActionAbility in CharacterData.Instance.currentActionAbilities)
            {
                KeyCode key = KeyCode.None;
                switch (ActionAbility.keyType)
                {
                    case RPGCombatDATA.ActionAbilityKeyType.OverrideKey:
                        key = ActionAbility.key;
                        break;
                    case RPGCombatDATA.ActionAbilityKeyType.ActionKey:
                        key = RPGBuilderUtilities.GetCurrentKeyByActionKeyName(ActionAbility.actionKeyName);
                        break;
                }
                if (Input.GetKeyDown(key) && Time.time >= ActionAbility.NextTimeUse)
                {
                    CombatManager.Instance.InitAbility(CombatManager.playerCombatNode, ActionAbility.ability, false);
                }
                if (Input.GetKeyUp(key))
                {
                    CombatManager.Instance.AbilityKeyUp(CombatManager.playerCombatNode, ActionAbility.ability, false);
                }
            }
        }

        private bool HandleEscape()
        {
            if (allOpenedPanels.Count > 0)
            {
                if (allOpenedPanels[0].gameObject == null)
                {
                    allOpenedPanels.Clear();
                    return true;
                }

                allOpenedPanels[0].gameObject.GetComponent<IDisplayPanel>().Hide();
                return false;
            }

            if (CombatManager.playerCombatNode.IsCasting)
            {
                CombatManager.playerCombatNode.ResetCasting();
                return false;
            }

            if (CombatManager.Instance.PlayerTargetData.currentTarget == null) return true;
            CombatManager.Instance.ResetPlayerTarget();
            return false;
        }

        void HandleTabTarget()
        {
            float maxAngle = 70;
            float maxDist = 50;

            CombatNode newTarget = null;
            float lastDist = 1000;
            int validTargets = 0;

            foreach (var cbtNode in CombatManager.Instance.allCombatNodes)
            {
                if (cbtNode == CombatManager.playerCombatNode) continue;
                RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment = FactionManager.Instance.GetAlignmentForPlayer(cbtNode.npcDATA.factionID);
                
                if (thisNodeAlignment == RPGCombatDATA.ALIGNMENT_TYPE.ALLY) continue;
                if (cbtNode == CombatManager.Instance.PlayerTargetData.currentTarget) continue;
                float thisDist = Vector3.Distance(cbtNode.transform.position,
                    CombatManager.playerCombatNode.transform.position);
                if (thisDist > maxDist) continue;
                var pointDirection = cbtNode.transform.position - CombatManager.playerCombatNode.transform.position;
                var angle = Vector3.Angle(CombatManager.playerCombatNode.transform.forward, pointDirection);
                if (!(angle < maxAngle)) continue;
                validTargets++;
                if (previouslySelectedCbtNodes.Contains(cbtNode)) continue;
                if (!(lastDist > thisDist)) continue;
                newTarget = cbtNode;
                lastDist = thisDist;
            }

            if (newTarget == null)
            {
                previouslySelectedCbtNodes.Clear();
                tabTargetSelected = 0;
                if(validTargets>0) HandleTabTarget();
                return;
            }

            CombatManager.Instance.SetPlayerTarget(newTarget, false);
            previouslySelectedCbtNodes.Add(newTarget);
            tabTargetSelected++;
            if (tabTargetSelected > 5)
            {
                previouslySelectedCbtNodes.RemoveAt(0);
            }
        }

        private void HandleKeyChange()
        {
            foreach (KeyCode keyPressed in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKeyDown(keyPressed))
                {
                    ModifyKeybind(currentlyModifiedActionKey, keyPressed);
                    isKeyChecking = false;
                    currentlyModifiedActionKey = "";
                }
        }

        private void CheckActionKeyUp(string actionKeyName)
        {
            switch (actionKeyName)
            {
                case "SPRINT":
                    CombatManager.playerCombatNode.playerControllerEssentials.EndSprint();
                    break;
            }
        }
        
        private void CheckActionKeys()
        {
            foreach (var t in CharacterData.Instance.actionKeys)
            {
                bool up = Input.GetKeyUp(t.currentKey);
                bool down = Input.GetKeyDown(t.currentKey);
                if (up) CheckActionKeyUp(t.actionKeyName);
                if (t.actionKeyName.Contains("ACTION_BAR_SLOT_"))
                {
                    if (!down && !up) continue;
                    if (!up && RPGBuilderUtilities.IsPointerOverUIObject()) return;
                    ActionBarSlot slot = ActionBarManager.Instance.GetActionBarSlotFromActionKeyName(t.actionKeyName);
                    if(slot == null) continue;
                    if(up && slot.contentType != CharacterData.ActionBarSlotContentType.Ability) continue;
                    
                    switch (slot.contentType)
                    {
                        case CharacterData.ActionBarSlotContentType.None:
                            break;
                        case CharacterData.ActionBarSlotContentType.Ability:
                            if (up)
                            {
                                CombatManager.Instance.AbilityKeyUp(CombatManager.playerCombatNode, slot.ThisAbility,
                                    false);
                            }
                            else
                            {
                                if (RPGBuilderUtilities.IsAbilityInCombo(slot.ThisAbility.ID)) continue;
                                CombatManager.Instance.InitAbility(CombatManager.playerCombatNode, slot.ThisAbility, true);
                            }
                            break;
                        case CharacterData.ActionBarSlotContentType.Item:
                            InventoryManager.Instance.UseItemFromBar(slot.ThisItem);
                            break;
                    }
                    
                }
                else if (t.actionKeyName.Contains("UI_PANEL_"))
                {
                    if (!down) continue;
                    // WE USED AN UI KEY
                    var uiPanelString = t.actionKeyName.Replace("UI_PANEL_", "");
                    switch (uiPanelString)
                    {
                        case "CHARACTER":
                            CharacterPanelDisplayManager.Instance.Toggle();
                            break;
                        case "INVENTORY":
                            InventoryDisplayManager.Instance.Toggle();
                            break;
                        case "SKILLS":
                            SkillBookDisplayManager.Instance.Toggle();
                            break;
                        case "QUEST":
                            QuestJournalDisplayManager.Instance.Toggle();
                            break;
                        case "OPTIONS":
                            if (CombatManager.Instance.PlayerTargetData.currentTarget == null)
                                GameOptionsDisplayManager.Instance.Toggle();
                            break;
                        case "LOOTALL":
                            if (LootPanelDisplayManager.Instance.thisCG.alpha == 1)
                            {
                                LootPanelDisplayManager.Instance.LootAll();
                            }

                            break;
                        case "ENCHANTING":
                            EnchantingPanelDisplayManager.Instance.Toggle();
                            break;
                        case "SOCKETING":
                            SocketingPanelDisplayManager.Instance.Toggle();
                            break;
                    }
                }
                else
                {
                    if (!down) continue;
                    switch (t.actionKeyName)
                    {
                        case "TOGGLE_CURSOR":
                            CombatManager.playerCombatNode.playerControllerEssentials.ToggleCameraMouseLook();
                            break;
                        case "SPRINT":
                            CombatManager.playerCombatNode.playerControllerEssentials.StartSprint();
                            break;
                        case "INTERACT":
                            if(WorldInteractableDisplayManager.Instance.cachedInteractable != null) WorldInteractableDisplayManager.Instance.Interact();
                            break;
                        case "TOGGLE_COMBAT_STATE":
                            if (RPGBuilderEssentials.Instance.combatSettings.useAutomaticCombatState) return;
                            if(!CombatManager.playerCombatNode.inCombat) CombatManager.Instance.HandleCombatAction(CombatManager.playerCombatNode);
                            else CombatManager.Instance.ResetCombat(CombatManager.playerCombatNode);
                            break;
                        case "PETS_FOLLOW":
                            PetPanelDisplayManager.Instance.selectMovementActionButton("follow");
                            break;
                        case "PETS_STAY":
                            PetPanelDisplayManager.Instance.selectMovementActionButton("stay");
                            break;
                        case "PETS_AGGRESSIVE":
                            PetPanelDisplayManager.Instance.selectCombatActionButton("aggro");
                            break;
                        case "PETS_DEFEND":
                            PetPanelDisplayManager.Instance.selectCombatActionButton("defend");
                            break;
                        case "PETS_RESET":
                            PetPanelDisplayManager.Instance.resetPetsActions();
                            break;
                        case "PETS_ATTACK":
                            PetPanelDisplayManager.Instance.requestPetsAttack();
                            break;
                    }

                    if (t.actionKeyName.Contains("SHAPESHIFT_"))
                    {
                        string numberText = t.actionKeyName.Replace("SHAPESHIFT_", "");
                        int shapeshiftNumber = int.Parse(numberText);

                        if (ShapeshiftingSlotsDisplayManager.Instance.slots.Count >= shapeshiftNumber)
                        {
                            ShapeshiftingSlotsDisplayManager.Instance.ActivateShapeshift(shapeshiftNumber-1);
                        }
                    }
                }
            }
        }

        public void HandleUIPanelClose(CanvasGroup cg)
        {
            if (allOpenedPanels.Contains(cg))
            {
                allOpenedPanels.Remove(cg);

                if (CombatManager.playerCombatNode != null && allOpenedPanels.Count == 0)
                {
                    CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(false);
                }
            }
            else
            {
                CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(false);
            }
        }

        private bool CheckComboKeys()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return false;
            foreach (var t in CombatManager.playerCombatNode.activeCombos)
            {
                if (!Input.GetKeyDown(t.keyRequired)) continue;
                if (t.readyTime > 0) continue;
                CombatManager.Instance.CancelOtherComboOptions(CombatManager.playerCombatNode, t.combo);
                CombatManager.Instance.InitAbility(CombatManager.playerCombatNode,
                    RPGBuilderUtilities.GetAbilityFromID(t.combo.combos[t.comboIndex].abilityID),
                    t.combo.combos[t.comboIndex].abMustBeKnown);
                return true;
            }

            return false;
        }

        public void InitKeyChecking(string keybindName)
        {
            isKeyChecking = true;
            currentlyModifiedActionKey = keybindName;
        }

        private void ModifyKeybind(string actionKeyName, KeyCode newKey)
        {
            foreach (var actionKey in CharacterData.Instance.actionKeys)
            {
                if (actionKey.actionKeyName != actionKeyName) continue;
                if (!isKeyAvailable(newKey))
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This key is not available", 3);
                    return;
                }
                actionKey.currentKey = newKey;

                if (actionKey.actionKeyName.Contains("ACTION_BAR_SLOT_"))
                {
                    var slotIDString = actionKey.actionKeyName.Replace("ACTION_BAR_SLOT_", "");
                    var ID = int.Parse(slotIDString);
                    ActionBarManager.Instance.UpdateSlotKeyText(ID-1, newKey);
                }
            }

            SettingsPanelDisplayManager.Instance.UpdateKeybindSlot(actionKeyName, newKey);
        }

        private bool isKeyAvailable(KeyCode key)
        {
            foreach (var actionKey in CharacterData.Instance.actionKeys)
            {
                if(actionKey.currentKey != key) continue;
                     return !RPGBuilderUtilities.isActionKeyUnique(actionKey.actionKeyName);
            }

            return true;
        }

        public void ResetKey(string actionKeyName)
        {
            foreach (var actionKey in CharacterData.Instance.actionKeys)
            {
                if (actionKey.actionKeyName != actionKeyName) continue;
                actionKey.currentKey = KeyCode.None;

                if (actionKey.actionKeyName.Contains("ACTION_BAR_SLOT_"))
                {
                    var slotIDString = actionKey.actionKeyName.Replace("ACTION_BAR_SLOT_", "");
                    var ID = int.Parse(slotIDString);
                    ActionBarManager.Instance.UpdateSlotKeyText(ID - 1, actionKey.currentKey);
                }
                
                SettingsPanelDisplayManager.Instance.UpdateKeybindSlot(actionKeyName, actionKey.currentKey);
            }
        }


        public static CustomInputManager Instance { get; private set; }
    }
}