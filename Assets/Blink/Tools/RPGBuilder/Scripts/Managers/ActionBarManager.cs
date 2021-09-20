using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace BLINK.RPGBuilder.Managers
{
    public class ActionBarManager : MonoBehaviour
    {
        
        public static ActionBarManager Instance { get; private set; }
        
        public List<ActionBarSlot> actionBarSlots = new List<ActionBarSlot>();

        
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        public ActionBarSlot GetActionBarSlotFromActionKeyName(string actionKeyName)
        {
            foreach (var actionBarSlot in actionBarSlots)
            {
                if(actionBarSlot.actionKeyName != actionKeyName) continue;
                return actionBarSlot;
            }

            return null;
        }
        
        public void SetItemToSlot (RPGItem item, int index)
        {
            if (index + 1 > actionBarSlots.Count) return;
            actionBarSlots[index].Init(item);

            if (actionBarSlots[index].actionBarType == CharacterData.ActionBarType.Main)
            {
                if (!CombatManager.playerCombatNode.stealthed)
                {
                    CharacterData.Instance.actionBarSlotsDATA[index].contentType =
                        CharacterData.ActionBarSlotContentType.Item;
                    CharacterData.Instance.actionBarSlotsDATA[index].slotType = actionBarSlots[index].actionBarType;
                    CharacterData.Instance.actionBarSlotsDATA[index].ID = item.ID;
                }
                else
                {
                    CharacterData.Instance.stealthedActionBarSlotsDATA[index].contentType =
                        CharacterData.ActionBarSlotContentType.Item;
                    CharacterData.Instance.stealthedActionBarSlotsDATA[index].slotType = actionBarSlots[index].actionBarType;
                    CharacterData.Instance.stealthedActionBarSlotsDATA[index].ID = item.ID;
                }
            }
            else
            {
                CharacterData.Instance.actionBarSlotsDATA[index].contentType =
                    CharacterData.ActionBarSlotContentType.Item;
                CharacterData.Instance.actionBarSlotsDATA[index].slotType = actionBarSlots[index].actionBarType;
                CharacterData.Instance.actionBarSlotsDATA[index].ID = item.ID;
            }
            
            

            UpdateToggledAbilities();
        }
        
        
        public void SetAbilityToSlot (RPGAbility ab, int index)
        {
            if (index + 1 > actionBarSlots.Count) return;
            actionBarSlots[index].Init(ab);

            if (actionBarSlots[index].actionBarType == CharacterData.ActionBarType.Main)
            {
                if (!CombatManager.playerCombatNode.stealthed)
                {
                    CharacterData.Instance.actionBarSlotsDATA[index].contentType =
                        CharacterData.ActionBarSlotContentType.Ability;
                    CharacterData.Instance.actionBarSlotsDATA[index].slotType = actionBarSlots[index].actionBarType;
                    CharacterData.Instance.actionBarSlotsDATA[index].ID = ab.ID;
                }
                else
                {
                    CharacterData.Instance.stealthedActionBarSlotsDATA[index].contentType =
                        CharacterData.ActionBarSlotContentType.Ability;
                    CharacterData.Instance.stealthedActionBarSlotsDATA[index].slotType =
                        actionBarSlots[index].actionBarType;
                    CharacterData.Instance.stealthedActionBarSlotsDATA[index].ID = ab.ID;
                }
            }
            else
            {
                CharacterData.Instance.actionBarSlotsDATA[index].contentType =
                    CharacterData.ActionBarSlotContentType.Ability;
                CharacterData.Instance.actionBarSlotsDATA[index].slotType = actionBarSlots[index].actionBarType;
                CharacterData.Instance.actionBarSlotsDATA[index].ID = ab.ID;
            }

            UpdateToggledAbilities();
        }

        public void ResetActionSlot(int index, bool clearData)
        {
            actionBarSlots[index].Reset();
            actionBarSlots[index].icon.enabled = false;
            actionBarSlots[index].background.enabled = false;
            actionBarSlots[index].stackText.enabled = false;
            actionBarSlots[index].cooldownOverlay.fillAmount = 0;
            actionBarSlots[index].cooldownText.enabled = false;
            actionBarSlots[index].ThisAbility = null;
            actionBarSlots[index].ThisItem = null;
            
            if (clearData)
            {
                if (actionBarSlots[index].actionBarType == CharacterData.ActionBarType.Main)
                {
                    if (!CombatManager.playerCombatNode.stealthed)
                    {
                        CharacterData.Instance.actionBarSlotsDATA[index].contentType =
                            CharacterData.ActionBarSlotContentType.None;
                        CharacterData.Instance.actionBarSlotsDATA[index].ID = -1;
                    }
                    else
                    {
                        CharacterData.Instance.stealthedActionBarSlotsDATA[index].contentType =
                            CharacterData.ActionBarSlotContentType.None;
                        CharacterData.Instance.stealthedActionBarSlotsDATA[index].ID = -1;
                    }
                }
                else
                {
                    CharacterData.Instance.actionBarSlotsDATA[index].contentType =
                        CharacterData.ActionBarSlotContentType.None;
                    CharacterData.Instance.actionBarSlotsDATA[index].ID = -1;
                }
            }

            UpdateToggledAbilities();
        }

        public void UpdateSlotKeyText(int index, KeyCode newKey)
        {
            actionBarSlots[index].keyText.text = RPGBuilderUtilities.GetKeybindText(newKey);
        }

        public void InitActionBar()
        {
            actionBarSlots.Clear();
            int slotIndex = 0;
            foreach (var actionBarSlot in FindObjectsOfType<ActionBarSlot>())
            {
                actionBarSlots.Add(actionBarSlot);
                actionBarSlot.SlotIndex = slotIndex;
            }

            actionBarSlots = actionBarSlots.OrderBy(w => int.Parse(w.actionKeyName.Replace("ACTION_BAR_SLOT_", "")))
                .ToList();

            foreach (var slot in actionBarSlots)
            {
                slot.SlotIndex = slotIndex;
                slotIndex++;
            }
        }
        
        public void InitStealthActionBar()
        {
            for (var index = 0; index < actionBarSlots.Count; index++)
            {
                var slot = actionBarSlots[index];
                if (slot.actionBarType != CharacterData.ActionBarType.Main) continue;
                if (CharacterData.Instance.stealthedActionBarSlotsDATA[index].ID != -1)
                {
                    switch (CharacterData.Instance.stealthedActionBarSlotsDATA[index].contentType)
                    {
                        case CharacterData.ActionBarSlotContentType.None:
                            break;
                        case CharacterData.ActionBarSlotContentType.Ability:
                            SetAbilityToSlot(RPGBuilderUtilities.GetAbilityFromID(CharacterData.Instance.stealthedActionBarSlotsDATA[index].ID), index);
                            break;
                        case CharacterData.ActionBarSlotContentType.Item:
                            SetItemToSlot(RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.stealthedActionBarSlotsDATA[index].ID), index);
                            break;
                    }
                }
                else
                {
                    ResetActionSlot(index, false);
                }
            }
        }
        
        public void ResetStealthActionBar()
        {
            for (var index = 0; index < actionBarSlots.Count; index++)
            {
                var slot = actionBarSlots[index];
                if (slot.actionBarType != CharacterData.ActionBarType.Main) continue;
                if(CharacterData.Instance.actionBarSlotsDATA[index].slotType != CharacterData.ActionBarType.Main) continue;
                
                ResetActionSlot(index, false);
                
                if (CharacterData.Instance.actionBarSlotsDATA[index].ID != -1)
                {
                    switch (CharacterData.Instance.actionBarSlotsDATA[index].contentType)
                    {
                        case CharacterData.ActionBarSlotContentType.Ability:
                            SetAbilityToSlot(RPGBuilderUtilities.GetAbilityFromID(CharacterData.Instance.actionBarSlotsDATA[index].ID), index);
                            break;
                        case CharacterData.ActionBarSlotContentType.Item:
                            SetItemToSlot(RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.actionBarSlotsDATA[index].ID), index);
                            break;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (CombatManager.playerCombatNode == null) return;

            foreach (var t in actionBarSlots)
            {
                switch (t.contentType)
                {
                    case CharacterData.ActionBarSlotContentType.None:
                        continue;
                    case CharacterData.ActionBarSlotContentType.Ability when t.ThisAbility == null || RPGBuilderUtilities.IsAbilityInCombo(t.ThisAbility.ID):
                        t.cooldownOverlay.fillAmount = 0;
                        t.cooldownText.text = "";
                        continue;
                    case CharacterData.ActionBarSlotContentType.Ability:
                    {
                        CharacterData.AbilityCDState cdState = CharacterData.Instance.getAbilityCDState(t.ThisAbility);
                        if (cdState != null)
                        {
                            if (!cdState.canUseDuringGCD && CombatManager.Instance.currentGCD > 0 && cdState.CDLeft < CombatManager.Instance.currentGCD)
                            {
                                t.cooldownOverlay.fillAmount = CombatManager.Instance.currentGCD / RPGBuilderEssentials.Instance.combatSettings.GCDDuration;
                                t.cooldownText.text = CombatManager.Instance.currentGCD.ToString("F0");
                            }
                            else if (cdState.NextUse > 0)
                            {
                                t.cooldownOverlay.fillAmount = cdState.CDLeft / cdState.NextUse;
                                t.cooldownText.text = cdState.CDLeft.ToString("F0");
                            }
                            else
                            {
                                t.cooldownOverlay.fillAmount = 0;
                                t.cooldownText.text = "";
                            }
                        }
                        else
                        {
                            t.cooldownOverlay.fillAmount = 0;
                            t.cooldownText.text = "";
                        }

                        break;
                    }
                }
            }
        }

        public void InitializeSlots()
        {
            foreach (var slot in actionBarSlots)
            {
                switch (slot.contentType)
                {
                    case CharacterData.ActionBarSlotContentType.None:
                        slot.icon.enabled = false;
                        slot.Reset();
                        break;
                    case CharacterData.ActionBarSlotContentType.Ability:
                        slot.icon.enabled = true;
                        slot.Init(slot.ThisAbility);
                        break;
                    case CharacterData.ActionBarSlotContentType.Item:
                        slot.icon.enabled = true;
                        slot.Init(slot.ThisItem);
                        break;
                }
            }
        }

        public void CheckItemBarState()
        {
            for (int i = 0; i < actionBarSlots.Count; i++)
            {
                if (actionBarSlots[i].contentType != CharacterData.ActionBarSlotContentType.Item) continue;
                int ttlCount = InventoryManager.Instance.getTotalCountOfItem(actionBarSlots[i].ThisItem);
                if (ttlCount <= 0)
                {
                    ResetActionSlot(i, true);
                }
                else
                {
                    actionBarSlots[i].UpdateSlot(ttlCount);
                }
            }
        }

        public void HandleSlotSetup(CharacterData.ActionBarSlotContentType contentType, RPGItem thisItem,
            RPGAbility thisAb, int draggedOnIndex)
        {
            switch (contentType)
            {
                case CharacterData.ActionBarSlotContentType.Ability:
                    SetAbilityToSlot(thisAb, draggedOnIndex);
                    break;
                case CharacterData.ActionBarSlotContentType.Item:
                    SetItemToSlot(thisItem, draggedOnIndex);
                    break;
            }
        }

        public void UpdateToggledAbilities()
        {
            foreach (var actionSlot in actionBarSlots)
            {
                actionSlot.toggledOverlay.gameObject.SetActive(
                    actionSlot.ThisAbility != null &&
                    RPGBuilderUtilities.isAbilityToggled(CombatManager.playerCombatNode, actionSlot.ThisAbility));
            }
        }
    }
}
