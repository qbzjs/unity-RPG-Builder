using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class CombosDisplayManager : MonoBehaviour
{
    public static CombosDisplayManager Instance { get; private set; }

    public GameObject comboSlotPrefab;
    public Transform comboSlotsParent;
    
    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void InitNewCombo(int activeComboIndex, RPGCombo.ComboEntry comboEntry, KeyCode key)
    {
        CombatNode.ActiveCombo activeCombo = CombatManager.playerCombatNode.activeCombos[activeComboIndex];
        activeCombo.keyRequired = key;
        RPGBuilderUtilities.SetAbilityComboActive(activeCombo.initialAbilityID, true);
        
        RPGAbility abREF = RPGBuilderUtilities.GetAbilityFromID(comboEntry.abilityID);
        if (abREF == null) return;
        GameObject newComboSlot = Instantiate(comboSlotPrefab, comboSlotsParent);
        
        activeCombo.UISlotREF = newComboSlot.GetComponent<ComboSlot>();
        activeCombo.UISlotREF.abilityIcon.sprite = abREF.icon;
        activeCombo.UISlotREF.expireTimeBar.fillAmount = 1;
        activeCombo.UISlotREF.abilityNameText.text = abREF.displayName;
        activeCombo.UISlotREF.expireTimeText.text = comboEntry.expireTime + "s";
        activeCombo.UISlotREF.KeyText.text = GetKeybindText(key);

        if(comboEntry.keyType == RPGCombo.KeyType.StartAbilityKey) UpdateActionBarSlotsImage(abREF, activeComboIndex);
    }

    public void UpdateComboEntry(int activeComboIndex, KeyCode key, bool useInitialKey)
    {
        RPGBuilderUtilities.SetAbilityComboActive(CombatManager.playerCombatNode.activeCombos[activeComboIndex].initialAbilityID, true);
        CombatNode.ActiveCombo activeCombo = CombatManager.playerCombatNode.activeCombos[activeComboIndex];
        RPGAbility abREF = RPGBuilderUtilities.GetAbilityFromID(activeCombo.combo.combos[activeCombo.comboIndex].abilityID);
        if (abREF == null) return;
        activeCombo.readyTime = activeCombo.combo.combos[activeCombo.comboIndex].readyTime;
        activeCombo.curLoadTime = 0;
        activeCombo.expireTime = activeCombo.combo.combos[activeCombo.comboIndex].expireTime;
        activeCombo.curTime = activeCombo.expireTime;
        activeCombo.keyRequired = key;

        activeCombo.UISlotREF.abilityIcon.sprite = abREF.icon;
        activeCombo.UISlotREF.expireTimeBar.fillAmount = 1;
        activeCombo.UISlotREF.abilityNameText.text = abREF.displayName;
        activeCombo.UISlotREF.expireTimeText.text =
            activeCombo.expireTime + "s";
        activeCombo.UISlotREF.KeyText.text = GetKeybindText(key);
        
        activeCombo.UISlotREF.gameObject.SetActive(false);
        activeCombo.UISlotREF.gameObject.SetActive(true);
        
        if(useInitialKey) UpdateActionBarSlotsImage(abREF, activeComboIndex);
    }

    private void UpdateActionBarSlotsImage(RPGAbility abREF, int activeComboIndex)
    {
        foreach (var abSlot in ActionBarManager.Instance.actionBarSlots)
        {
            if(abSlot.contentType != CharacterData.ActionBarSlotContentType.Ability) continue;
            if(abSlot.ThisAbility.ID != CombatManager.playerCombatNode.activeCombos[activeComboIndex].initialAbilityID) continue;
            abSlot.icon.sprite = abREF.icon;
        }
    }

    public void ResetActionBarSlotsImage(int abID)
    {
        foreach (var abSlot in ActionBarManager.Instance.actionBarSlots)
        {
            if(abSlot.contentType != CharacterData.ActionBarSlotContentType.Ability) continue;
            if(abSlot.ThisAbility.ID != abID) continue;
            abSlot.icon.sprite = abSlot.ThisAbility.icon;
        }
    }

    private void FixedUpdate()
    {
        if (CombatManager.playerCombatNode == null) return;
        if (CombatManager.playerCombatNode.activeCombos.Count == 0) return;
        foreach (var combo in CombatManager.playerCombatNode.activeCombos)
        {
            if (combo.readyTime > 0)
            {
                combo.UISlotREF.expireTimeBar.fillAmount =
                    combo.curLoadTime /
                    combo.readyTime;
                combo.UISlotREF.expireTimeText.text =
                    combo.curLoadTime.ToString("F1") + "s";
            }
            else
            {
                combo.UISlotREF.expireTimeBar.fillAmount =
                    combo.curTime /
                    combo.expireTime;
                combo.UISlotREF.expireTimeText.text =
                    combo.curTime.ToString("F1") + "s";
            }
            
        }
    }
    
    private string GetKeybindText(KeyCode key)
    {
        var KeyBindString = key.ToString();
        if (KeyBindString.Contains("Alpha"))
        {
            var alphakey = KeyBindString.Remove(0, 5);
            return alphakey;
        }

        if (!KeyBindString.Contains("Mouse")) return KeyBindString;
        {
            var alphakey = KeyBindString.Remove(0, 5);
            return "M" + alphakey;
        }

    }
}
