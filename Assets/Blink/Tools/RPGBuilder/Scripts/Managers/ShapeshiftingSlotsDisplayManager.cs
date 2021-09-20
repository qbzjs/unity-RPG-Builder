using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class ShapeshiftingSlotsDisplayManager : MonoBehaviour
{
    public GameObject shapeshiftSlotPrefab;
    public List<ShapeshiftSlot> slots = new List<ShapeshiftSlot>();

    public static ShapeshiftingSlotsDisplayManager Instance { get; private set; }

    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void ClearSlots()
    {
        foreach (var slot in slots)
        {
            Destroy(slot.gameObject);
        }

        slots.Clear();
    }

    public void DisplaySlots()
    {
        ClearSlots();

        foreach (var ability in CharacterData.Instance.abilitiesData)
        {
            if (!ability.known) continue;
            RPGAbility abREF = RPGBuilderUtilities.GetAbilityFromID(ability.ID);
            RPGAbility.RPGAbilityRankData rankREF = abREF.ranks[RPGBuilderUtilities.getAbilityRank(ability.ID)];
            if (!CombatManager.Instance.AbilityHasTag(rankREF, RPGAbility.ABILITY_TAGS.shapeshifting)) continue;
            GameObject newShapeshiftSlot = Instantiate(shapeshiftSlotPrefab, transform);
            ShapeshiftSlot slotREF = newShapeshiftSlot.GetComponent<ShapeshiftSlot>();
            slotREF.ThisAbility = abREF;
            slotREF.icon.sprite = abREF.icon;
            slots.Add(slotREF);
        }

        foreach (var slot in slots)
        {
            RPGAbility.RPGAbilityRankData rankREF =
                slot.ThisAbility.ranks[RPGBuilderUtilities.getAbilityRank(slot.ThisAbility.ID)];
            slot.border.enabled = RPGBuilderUtilities.getActiveShapeshiftingEffectID(CombatManager.playerCombatNode) ==
                                  RPGBuilderUtilities.getShapeshiftingTagEffectID(rankREF);
        }
    }

    public void ActivateShapeshift(int index)
    {
        CombatManager.Instance.InitAbility(CombatManager.playerCombatNode, slots[index].ThisAbility, true);
    }
    
}
