using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.World;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public Transform draggedItemParent;
        public GameObject draggedItemImage;

        [Serializable]
        public class INVENTORY_EQUIPPED_ITEMS
        {
            public string slotType;
            public RPGItem itemEquipped;
            public int temporaryItemDataID = -1;
        }

        public List<INVENTORY_EQUIPPED_ITEMS> equippedArmors = new List<INVENTORY_EQUIPPED_ITEMS>();
        public List<INVENTORY_EQUIPPED_ITEMS> equippedWeapons = new List<INVENTORY_EQUIPPED_ITEMS>();

        public GameObject lootBagPrefab;

        [Serializable]
        public class WorldLootItems_DATA
        {
            public RPGItem item;
            public int count;
            public WorldDroppedItem worldDroppedItemREF;
            public int itemDataID = -1;
        }
        public List<WorldLootItems_DATA> allWorldDroppedItems = new List<WorldLootItems_DATA>();

        public void LootWorldDroppedItem(WorldDroppedItem worldDroppedItemREF)
        {
            for (int i = 0; i < allWorldDroppedItems.Count; i++)
            {
                if (allWorldDroppedItems[i].worldDroppedItemREF != worldDroppedItemREF) continue;
                
                int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(allWorldDroppedItems[i].item.ID, allWorldDroppedItems[i].count, false, true);
                if (itemsLeftOver == 0)
                {
                    RPGBuilderUtilities.SetNewItemDataState(allWorldDroppedItems[i].itemDataID, CharacterData.ItemDataState.inBag);
                    Destroy(allWorldDroppedItems[i].worldDroppedItemREF.gameObject);
                    allWorldDroppedItems.RemoveAt(i); 
                }
                else
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                }
                return;
            }
        }
        
        public int getWorldDroppedItemDataID(WorldDroppedItem worldDroppedItemREF)
        {
            foreach (var t in allWorldDroppedItems)
            {
                if (t.worldDroppedItemREF != worldDroppedItemREF) continue;
                return t.itemDataID;
            }

            return -1;
        }
        
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void DestroyWorldDroppedItem(WorldDroppedItem worldItemREF)
        {
            for (var index = 0; index < allWorldDroppedItems.Count; index++)
            {
                var v = allWorldDroppedItems[index];
                if (v.worldDroppedItemREF != worldItemREF) continue;
                Destroy(v.worldDroppedItemREF.gameObject);
                allWorldDroppedItems.RemoveAt(index);
            }
        }


        public void InitEquippedItems()
        {
            foreach (var t in CharacterData.Instance.armorsEquipped)
                if (t.itemID != -1)
                    InitEquipArmor(RPGBuilderUtilities.GetItemFromID(t.itemID), t.itemDataID);

            for (int i = 0; i < CharacterData.Instance.weaponsEquipped.Count; i++)
            {
                if (CharacterData.Instance.weaponsEquipped[i].itemID == -1) continue;
                InitEquipWeapon(RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.weaponsEquipped[i].itemID),
                    i, CharacterData.Instance.weaponsEquipped[i].itemDataID);
            }
        }


        
        public void SellItemToMerchant(int itemID, int count, int bagIndex, int bagSlotIndex)
        {
            RemoveItem(itemID, count, bagIndex, bagSlotIndex, true);
            var itemREF = RPGBuilderUtilities.GetItemFromID(itemID);
            for (var i = 0; i < count; i++) AddCurrency(itemREF.sellCurrencyID, itemREF.sellPrice);
        }

        

        public void HideAllItemsMainMenu(PlayerAppearanceHandler appearanceREF)
        {
            foreach (var t in appearanceREF.armorPieces)
            {
                t.SetActive(false);
            }
            
            if(appearanceREF.weapon1GO != null) Destroy(appearanceREF.weapon1GO);
            if(appearanceREF.weapon2GO != null) Destroy(appearanceREF.weapon2GO);

            if (appearanceREF.useBodyParts)
            {
                foreach (var bodyRenderer in appearanceREF.bodyRenderers)
                {
                    bodyRenderer.bodyRenderer.enabled = true;
                }
            }
        }
        
        public void InitEquipItemMainMenu(RPGItem itemToEquip, PlayerAppearanceHandler appearanceRef, int i, int itemDataID)
        {
            if (itemToEquip == null || itemToEquip.itemType != "ARMOR" && itemToEquip.itemType != "WEAPON") return;
            if (itemToEquip.itemType == "ARMOR")
            {
                if (itemToEquip.itemModelName != "" || itemToEquip.armorPieceType == RPGItem.ArmorPieceType.Mesh)
                {
                    appearanceRef.ShowArmor(itemToEquip, getMeshManager(itemDataID));
                }
            }
            else
            {
                var weaponID = 0;
                switch (i)
                {
                    case 0:
                        weaponID = 1;
                        break;
                    case 1:
                        weaponID = 2;
                        break;
                }

                if (itemToEquip.weaponModel != null) appearanceRef.ShowWeapon(itemToEquip, weaponID, getMeshManager(itemDataID));
            }
        }
        
        public void InitEquipClassItemMainMenu(RPGItem itemToEquip, PlayerAppearanceHandler appearanceRef, int i)
        {
            if (itemToEquip.itemType != "ARMOR" && itemToEquip.itemType != "WEAPON") return;
            if (itemToEquip.itemType == "ARMOR")
            {
                if (itemToEquip.itemModelName != "" || itemToEquip.armorPieceType == RPGItem.ArmorPieceType.Mesh)
                {
                    appearanceRef.ShowArmor(itemToEquip, null);
                }
            }
            else
            {
                var weaponID = 0;
                switch (itemToEquip.slotType)
                {
                    case "TWO HAND":
                        weaponID = 1;
                        break;
                    case "MAIN HAND":
                        weaponID = 1;
                        break;
                    case "OFF HAND":
                        weaponID = 2;
                        break;
                    case "ANY HAND":
                        weaponID = appearanceRef.weapon1GO == null ? 1 : 2;
                        break;
                }

                if (itemToEquip.weaponModel != null) appearanceRef.ShowWeapon(itemToEquip, weaponID, null);
            }
        }

        private void InitEquipArmor(RPGItem itemToEquip, int itemDataID)
        {
            if (itemToEquip.itemType != "ARMOR") return;
            int armorSlotIndex = RPGBuilderUtilities.getArmorSlotIndex(itemToEquip.equipmentSlot);
            equippedArmors[armorSlotIndex].itemEquipped = itemToEquip;
            equippedArmors[armorSlotIndex].temporaryItemDataID = itemDataID;

            CharacterPanelDisplayManager.Instance.InitCharEquippedItems();
            if (itemToEquip.itemModelName != "" || itemToEquip.armorPieceType == RPGItem.ArmorPieceType.Mesh)
            {
                CombatManager.playerCombatNode.appearanceREF.ShowArmor(itemToEquip, getMeshManager(itemDataID));
            }

            StatCalculator.CalculateItemStats();
        }

        private void InitEquipWeapon(RPGItem itemToEquip, int weaponIndex, int itemDataID)
        {
            if (itemToEquip.itemType != "WEAPON") return;
            // EQUIP WEAPON

            equippedWeapons[weaponIndex].itemEquipped = itemToEquip;
            equippedWeapons[weaponIndex].temporaryItemDataID = itemDataID;
            CharacterPanelDisplayManager.Instance.InitCharEquippedItems();

            var weaponID = 0;
            switch (weaponIndex)
            {
                case 0:
                {
                    weaponID = 1;
                    if (itemToEquip.autoAttackAbilityID != -1)
                    {
                        CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                            itemToEquip.autoAttackAbilityID;
                        CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                            itemToEquip;
                    }

                    break;
                }
                case 1:
                {
                    weaponID = 2;
                    if (CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID == -1 &&
                        itemToEquip.autoAttackAbilityID != -1)
                    {
                        CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                            itemToEquip.autoAttackAbilityID;
                        CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                            itemToEquip;
                    }

                    break;
                }
            }

            if (itemToEquip.weaponModel != null)
                CombatManager.playerCombatNode.appearanceREF.ShowWeapon(itemToEquip, weaponID, getMeshManager(itemDataID));

            StatCalculator.CalculateItemStats();
        }

        private bool CheckItemRequirements(RPGItem item)
        {
            List<bool> reqResults = new List<bool>();
            foreach (var t in item.useRequirements)
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
                reqResults.Add(RequirementsManager.Instance.HandleRequirementType(t, intValue1, intValue2, true));
            }

            return !reqResults.Contains(false);
        }

        public void UseItemFromBar(RPGItem itemToUse)
        {
            int slotIndex = -1;

            for (int i = 0; i < CharacterData.Instance.inventoryData.baseSlots.Count; i++)
            {
                if (CharacterData.Instance.inventoryData.baseSlots[i].itemID == -1) continue;
                if (RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.inventoryData.baseSlots[i].itemID) == itemToUse)
                {
                    slotIndex = i;
                }
            }
            
            UseItem(itemToUse, 0 , slotIndex);
        }
        
        public void UseItem(RPGItem itemUsed, int bagIndex, int slotIndex)
        {
            if (!CheckItemRequirements(itemUsed)) return;
            var isConsumed = false;
            foreach (var t in itemUsed.onUseActions)
            {
                if (t.isConsumed) isConsumed = true;
                switch (t.actionType)
                {
                    case RPGItem.OnUseActionType.acceptQuest:
                        GameActionsManager.Instance.ProposeQuest(t.questID);
                        break;
                    case RPGItem.OnUseActionType.equip:
                        EquipItem(itemUsed, bagIndex, slotIndex, CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID);
                        break;
                    case RPGItem.OnUseActionType.gainClassLevel:
                        GameActionsManager.Instance.GainLevel(t.classLevelGained);
                        break;
                    case RPGItem.OnUseActionType.gainClassXP:
                        GameActionsManager.Instance.GainEXP(t.classXPGained);
                        break;
                    case RPGItem.OnUseActionType.gainSkillLevel:
                        GameActionsManager.Instance.GainSkillLevel(t.skillID, t.skillLevelGained);
                        break;
                    case RPGItem.OnUseActionType.gainSkillXP:
                        GameActionsManager.Instance.GainSkillEXP(t.skillID, t.skillXPGained);
                        break;
                    case RPGItem.OnUseActionType.gainTreePoint:
                        GameActionsManager.Instance.GainTreePoint(t.treePointID, t.treePointGained);
                        break;
                    case RPGItem.OnUseActionType.learnAbility:
                        GameActionsManager.Instance.LearnAbility(t.abilityID);
                        break;
                    case RPGItem.OnUseActionType.learnRecipe:
                        GameActionsManager.Instance.LearnRecipe(t.recipeID);
                        break;
                    case RPGItem.OnUseActionType.learnResourceNode:
                        GameActionsManager.Instance.LearnResourceNode(t.resourceNodeID);
                        break;
                    case RPGItem.OnUseActionType.learnBonus:
                        GameActionsManager.Instance.LearnBonus(t.bonusID);
                        break;
                    case RPGItem.OnUseActionType.useAbility:
                        GameActionsManager.Instance.UseAbility(CombatManager.playerCombatNode, t.abilityID);
                        break;
                    case RPGItem.OnUseActionType.useEffect:
                        GameActionsManager.Instance.ApplyEffect(t.target, CombatManager.playerCombatNode, t.effectID);
                        break;
                    case RPGItem.OnUseActionType.factionPoint:
                        if (t.factionPointsGained > 0)
                        {
                            GameActionsManager.Instance.GainFactionpoint(t.factionID, t.factionPointsGained);
                        }
                        else
                        {
                            GameActionsManager.Instance.LoseFactionpoint(t.factionID, Mathf.Abs(t.factionPointsGained));
                        }
                        break;
                    case RPGItem.OnUseActionType.gainWeaponTemplateEXP:
                        GameActionsManager.Instance.GainWeaponTemplateEXP(t.weaponTemplateID, t.weaponTemplateXPGained);
                        break;
                }
            }

            if (isConsumed) RemoveItem(itemUsed.ID, 1, bagIndex, slotIndex, true);
            CharacterEventsManager.Instance.ItemUsed(itemUsed);
            ActionBarManager.Instance.CheckItemBarState();
        }

        private int[] getWeaponSituation(RPGItem weaponToEquip, RPGItem weaponEquipped1, RPGItem weaponEquipped2, int itemDataID)
        {
            var newWeaponState = new int[2];
            if (weaponEquipped1 == null && weaponEquipped2 == null)
            {
                if (weaponToEquip.slotType == "OFF HAND")
                {
                    newWeaponState[0] = 1;
                    newWeaponState[1] = 2;
                }
                else
                {
                    newWeaponState[0] = 0;
                    newWeaponState[1] = 1;
                }
            }
            else if (weaponEquipped1 != null)
            {
                switch (weaponToEquip.slotType)
                {
                    case "TWO HAND":
                    {
                        UnequipItem(weaponEquipped1, 1);
                        newWeaponState[0] = 0;
                        newWeaponState[1] = 1;

                        if (weaponEquipped2 != null) UnequipItem(weaponEquipped2, 2);
                        break;
                    }
                    case "MAIN HAND" when weaponEquipped1.slotType == "OFF HAND" || weaponEquipped1.slotType == "ANY HAND":
                    {
                        equippedWeapons[0].itemEquipped = weaponEquipped1;
                        equippedWeapons[0].temporaryItemDataID = itemDataID;
                        CombatManager.playerCombatNode.appearanceREF.HideWeapon(1);
                        if (weaponEquipped1.weaponModel != null)
                            CombatManager.playerCombatNode.appearanceREF.ShowWeapon(weaponEquipped1, 2, getMeshManager(itemDataID));

                        newWeaponState[0] = 0;
                        newWeaponState[1] = 1;
                        break;
                    }
                    case "MAIN HAND":
                        // CONDITIONS NOT MET
                        UnequipItem(weaponEquipped1, 1);
                        newWeaponState[0] = 0;
                        newWeaponState[1] = 1;
                        break;
                    case "ANY HAND" when weaponEquipped1.slotType == "MAIN HAND" || weaponEquipped1.slotType == "ANY HAND":
                    {
                        if (weaponEquipped2 != null) UnequipItem(weaponEquipped2, 2);
                        newWeaponState[0] = 1;
                        newWeaponState[1] = 2;
                        break;
                    }
                    case "ANY HAND":
                        UnequipItem(weaponEquipped1, 1);
                        newWeaponState[0] = 0;
                        newWeaponState[1] = 1;
                        break;
                    case "OFF HAND":
                    {
                        if (weaponEquipped2 != null) UnequipItem(weaponEquipped2, 2);
                        if(weaponEquipped1.slotType == "TWO HAND") UnequipItem(weaponEquipped1, 1);
                        newWeaponState[0] = 1;
                        newWeaponState[1] = 2;
                        break;
                    }
                }
            }
            else if (weaponEquipped2 != null)
            {
                if (weaponToEquip.slotType == "MAIN HAND" || weaponToEquip.slotType == "ANY HAND")
                {
                    newWeaponState[0] = 0;
                    newWeaponState[1] = 1;
                }
                else
                {
                    if(weaponEquipped1 != null)UnequipItem(weaponEquipped1, 1);
                    if (weaponToEquip.slotType == "TWO HAND")
                    {
                        UnequipItem(weaponEquipped2, 2);
                        newWeaponState[0] = 0;
                        newWeaponState[1] = 1;
                    } else if (weaponToEquip.slotType == "OFF HAND")
                    {
                        UnequipItem(weaponEquipped2, 2);
                        newWeaponState[0] = 1;
                        newWeaponState[1] = 2;
                    }
                }
            }

            return newWeaponState;
        }

        GameObject getMeshManager(int itemDataID)
        {
            CharacterData.ItemDATA itemDataRef = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            if (itemDataRef == null || itemDataRef.enchantmentID == -1) return null;
            RPGEnchantment enchantRef = RPGBuilderUtilities.GetEnchantmentFromID(itemDataRef.enchantmentID);
            if (enchantRef != null && enchantRef.enchantmentTiers[itemDataRef.enchantmentTierIndex].enchantingParticle != null)
            {
                return enchantRef.enchantmentTiers[itemDataRef.enchantmentTierIndex]
                    .enchantingParticle;
            }

            return null;
        }

        private void EquipItem(RPGItem itemToEquip, int bagIndex, int slotIndex, int itemDataID)
        {
            if (itemToEquip.itemType != "ARMOR" && itemToEquip.itemType != "WEAPON") return;
            if (itemToEquip.itemType == "ARMOR")
            {
                int armorSlotIndex = RPGBuilderUtilities.getArmorSlotIndex(itemToEquip.equipmentSlot);
                var itemToUnequip = RPGBuilderUtilities.getEquippedArmor(itemToEquip.equipmentSlot);
                if (itemToUnequip != null)
                {
                    UnequipItem(itemToUnequip, 0);
                }

                equippedArmors[armorSlotIndex].itemEquipped = itemToEquip;
                equippedArmors[armorSlotIndex].temporaryItemDataID = itemDataID;

                CharacterData.Instance.armorsEquipped[armorSlotIndex].itemID = itemToEquip.ID;
                CharacterData.Instance.armorsEquipped[armorSlotIndex].itemDataID = itemDataID;

                RemoveEquippedItem(bagIndex, slotIndex);
                CharacterPanelDisplayManager.Instance.InitCharEquippedItems();

                if (itemToEquip.itemModelName != "" || itemToEquip.armorPieceType == RPGItem.ArmorPieceType.Mesh)
                {
                    CombatManager.playerCombatNode.appearanceREF.ShowArmor(itemToEquip, getMeshManager(itemDataID));
                }
            }
            else
            {
                // EQUIP WEAPON

                var weaponState = getWeaponSituation(itemToEquip, equippedWeapons[0].itemEquipped,
                    equippedWeapons[1].itemEquipped, itemDataID);

                equippedWeapons[weaponState[0]].itemEquipped = itemToEquip;
                equippedWeapons[weaponState[0]].temporaryItemDataID = itemDataID;
                RemoveEquippedItem(bagIndex, slotIndex);
                CharacterData.Instance.weaponsEquipped[weaponState[0]].itemID = itemToEquip.ID;
                CharacterData.Instance.weaponsEquipped[weaponState[0]].itemDataID = itemDataID;
                
                CharacterPanelDisplayManager.Instance.InitCharEquippedItems();

                if (itemToEquip.weaponModel != null)
                {
                    CombatManager.playerCombatNode.appearanceREF.ShowWeapon(itemToEquip, weaponState[1], getMeshManager(itemDataID));
                }

                BonusManager.Instance.InitBonuses();

                if (weaponState[0] == 0)
                {
                    if (equippedWeapons[1].itemEquipped == null)
                    {
                        if (equippedWeapons[0].itemEquipped.autoAttackAbilityID != -1)
                        {
                            CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                equippedWeapons[0].itemEquipped.autoAttackAbilityID;
                            CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                                equippedWeapons[0].itemEquipped;
                        }
                    }
                    else
                    {
                        if (equippedWeapons[0].itemEquipped.autoAttackAbilityID == -1)
                            if (equippedWeapons[1].itemEquipped.autoAttackAbilityID != -1)
                            {
                                CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                    equippedWeapons[1].itemEquipped.autoAttackAbilityID;
                                CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                                    equippedWeapons[1].itemEquipped;
                            }
                    }
                }
                else
                {
                    if (equippedWeapons[0].itemEquipped == null)
                    {
                        if (equippedWeapons[1].itemEquipped.autoAttackAbilityID != -1)
                        {
                            CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                equippedWeapons[1].itemEquipped.autoAttackAbilityID;
                            CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                                equippedWeapons[1].itemEquipped;
                        }
                    }
                    else
                    {
                        if (equippedWeapons[0].itemEquipped.autoAttackAbilityID != -1)
                        {
                            CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                equippedWeapons[0].itemEquipped.autoAttackAbilityID;
                            CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                                equippedWeapons[0].itemEquipped;
                        }
                    }
                }
                
                CombatManager.playerCombatNode.appearanceREF.HandleAnimatorOverride();
            }

            RPGBuilderUtilities.SetNewItemDataState(itemDataID, CharacterData.ItemDataState.equipped);
            StatCalculator.CalculateItemStats();
            CharacterEventsManager.Instance.ItemEquipped(itemToEquip);
            RPGBuilderUtilities.UpdateActionAbilities(itemToEquip);
        }

        public void UnequipItem(RPGItem itemToUnequip, int weaponID)
        {
            int cachedItemDataID = -1;
            switch (itemToUnequip.itemType)
            {
                case "ARMOR":
                {
                    int armorSlotIndex = RPGBuilderUtilities.getArmorSlotIndex(itemToUnequip.equipmentSlot);
                    cachedItemDataID = equippedArmors[armorSlotIndex].temporaryItemDataID;
                    equippedArmors[armorSlotIndex].itemEquipped = null;
                    equippedArmors[armorSlotIndex].temporaryItemDataID = -1;
                
                    CharacterData.Instance.armorsEquipped[armorSlotIndex].itemID = -1;
                    CharacterData.Instance.armorsEquipped[armorSlotIndex].itemDataID = -1;

                    if (itemToUnequip.itemModelName != "" || itemToUnequip.armorPieceType == RPGItem.ArmorPieceType.Mesh)
                    {
                        CombatManager.playerCombatNode.appearanceREF.HideArmor(itemToUnequip);
                    }
                    
                    RPGBuilderUtilities.SetNewItemDataState(
                        equippedArmors[armorSlotIndex].temporaryItemDataID, CharacterData.ItemDataState.inBag);

                    break;
                }
                case "WEAPON":
                {
                    if (weaponID == 1)
                    {
                        cachedItemDataID = equippedWeapons[0].temporaryItemDataID;
                        equippedWeapons[0].itemEquipped = null;
                        equippedWeapons[0].temporaryItemDataID = -1;
                        CharacterData.Instance.weaponsEquipped[0].itemID = -1;
                        CharacterData.Instance.weaponsEquipped[0].itemDataID = -1;
                    }
                    else
                    {
                        cachedItemDataID = equippedWeapons[1].temporaryItemDataID;
                        equippedWeapons[1].itemEquipped = null;
                        equippedWeapons[1].temporaryItemDataID = -1;
                        CharacterData.Instance.weaponsEquipped[1].itemID = -1;
                        CharacterData.Instance.weaponsEquipped[1].itemDataID = -1;
                    }

                    if (itemToUnequip.weaponModel != null)
                        CombatManager.playerCombatNode.appearanceREF.HideWeapon(weaponID);

                    BonusManager.Instance.CancelBonusFromUnequippedWeapon(itemToUnequip.weaponType);

                    if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
                    {
                        if (weaponID == 1)
                        {
                            if (equippedWeapons[1].itemEquipped == null)
                            {
                                CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                    RPGBuilderUtilities
                                        .GetClassFromID(CharacterData.Instance.classDATA.classID).autoAttackAbilityID;
                                CombatManager.playerCombatNode.AutoAttackData.weaponItem = null;
                            }
                            else
                            {
                                if (equippedWeapons[1].itemEquipped.autoAttackAbilityID != -1)
                                {
                                    CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                        equippedWeapons[1].itemEquipped.autoAttackAbilityID;
                                    CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                                        equippedWeapons[1].itemEquipped;
                                }
                                else
                                {
                                    CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                        RPGBuilderUtilities
                                            .GetClassFromID(CharacterData.Instance.classDATA.classID)
                                            .autoAttackAbilityID;
                                    CombatManager.playerCombatNode.AutoAttackData.weaponItem = null;
                                }
                            }
                        }
                        else
                        {
                            if (equippedWeapons[0].itemEquipped == null)
                            {
                                CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                    RPGBuilderUtilities
                                        .GetClassFromID(CharacterData.Instance.classDATA.classID).autoAttackAbilityID;
                                CombatManager.playerCombatNode.AutoAttackData.weaponItem = null;
                            }
                            else
                            {
                                if (equippedWeapons[0].itemEquipped.autoAttackAbilityID != -1)
                                {
                                    CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                        equippedWeapons[0].itemEquipped.autoAttackAbilityID;
                                    CombatManager.playerCombatNode.AutoAttackData.weaponItem =
                                        equippedWeapons[0].itemEquipped;
                                }
                                else
                                {
                                    CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID =
                                        RPGBuilderUtilities
                                            .GetClassFromID(CharacterData.Instance.classDATA.classID)
                                            .autoAttackAbilityID;
                                    CombatManager.playerCombatNode.AutoAttackData.weaponItem = null;
                                }
                            }
                        }
                    }

                    CombatManager.playerCombatNode.appearanceREF.HandleAnimatorOverride();
                    break;
                }
            }

            CharacterPanelDisplayManager.Instance.InitCharEquippedItems();

            RPGBuilderUtilities.SetNewItemDataState(cachedItemDataID, CharacterData.ItemDataState.inBag);
            AddItem(itemToUnequip.ID, 1, false, cachedItemDataID);

            StatCalculator.CalculateItemStats();
            RPGBuilderUtilities.CheckRemoveActionAbilities(itemToUnequip);
        }

        private void HideBodyPart(string slotType)
        {
            foreach (var bodyRenderer in CombatManager.playerCombatNode.appearanceREF.bodyRenderers.Where(bodyRenderer => bodyRenderer.armorSlotType == slotType))
            {
                bodyRenderer.bodyRenderer.enabled = false;
            }
        }

        private void RemoveEquippedItem(int bagIndex, int slotIndex)
        {
            CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemStack = 0;
            CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemID = -1;
            CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID = -1;

            if (InventoryDisplayManager.Instance.thisCG.alpha == 1) InventoryDisplayManager.Instance.UpdateSlots();
        }

        public void AddCurrency(int currencyID, int amount)
        {
            foreach (var t in CharacterData.Instance.currencies)
            {
                var currencyREF = RPGBuilderUtilities.GetCurrencyFromID(t.currencyID);
                if (currencyREF.ID != currencyID) continue;
                var curCurrencyAmount = t.amount;
                var convertCurrencyREF = RPGBuilderUtilities.GetCurrencyFromID(currencyREF.convertToCurrencyID);
                if (convertCurrencyREF != null && currencyREF.AmountToConvert > 0)
                {
                    // CONVERT CURRENCY
                    if (curCurrencyAmount + amount >= currencyREF.AmountToConvert)
                    {
                        var amountToAdd = amount;
                        while (amountToAdd + curCurrencyAmount >= currencyREF.AmountToConvert)
                        {
                            var amountAdded = currencyREF.AmountToConvert - curCurrencyAmount;
                            curCurrencyAmount = 0;
                            amountToAdd -= amountAdded;

                            // ADD THE CONVERTED CURRENCY
                            AddCurrency(convertCurrencyREF.ID, 1);
                        }

                        curCurrencyAmount = amountToAdd;
                    }
                    else
                    {
                        curCurrencyAmount += amount;
                    }
                }
                else
                {
                    if (currencyREF.maxValue > 0 && curCurrencyAmount + amount >= currencyREF.maxValue)
                        curCurrencyAmount = currencyREF.maxValue;
                    else
                        curCurrencyAmount += amount;
                }

                t.amount = curCurrencyAmount;
                CharacterEventsManager.Instance.CurrencyGain();
            }
        }

        public void GenerateDroppedLoot(RPGNpc npc, CombatNode nodeRef)
        {
            var totalItemDropped = 0;
            var lootData = new List<LootBagHolder.Loot_Data>();
            float LOOTCHANCEMOD = CombatManager.Instance.GetTotalOfStatType(CombatManager.playerCombatNode,
                RPGStat.STAT_TYPE.LOOT_CHANCE_MODIFIER);
            foreach (var t in npc.lootTables)
            {
                var dropAmount = Random.Range(0f, 100f);
                dropAmount = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.NPC + "+" +
                    RPGGameModifier.NPCModifierType.Loot_Table_Chance,
                    dropAmount, npc.ID, -1);
                if (!(dropAmount <= t.dropRate)) continue;
                var lootTableREF = RPGBuilderUtilities.GetLootTableFromID(t.lootTableID);
                foreach (var t1 in lootTableREF.lootItems)
                {
                    var itemDropAmount = Random.Range(0f, 100f);
                    if (LOOTCHANCEMOD > 0) itemDropAmount += itemDropAmount * (LOOTCHANCEMOD / 100);
                    if (!(itemDropAmount <= t1.dropRate)) continue;
                    var stack = t1.min == t1.max ? t1.min : Random.Range(t1.min, t1.max + 1);

                    RPGItem itemREF = RPGBuilderUtilities.GetItemFromID(t1.itemID);
                    if (itemREF.dropInWorld && itemREF.itemWorldModel != null)
                    {
                        var newLoot = new WorldLootItems_DATA();
                        newLoot.item = itemREF;
                        newLoot.count = stack;
                        GameObject newLootGO = Instantiate(itemREF.itemWorldModel, new Vector3(
                            nodeRef.transform.position.x,
                            nodeRef.transform.position.y + 1, nodeRef.transform.position.z), Quaternion.identity);
                        newLootGO.layer = itemREF.worldInteractableLayer;
                        newLoot.worldDroppedItemREF = newLootGO.AddComponent<WorldDroppedItem>();
                        newLoot.worldDroppedItemREF.curLifetime = 0;
                        newLoot.worldDroppedItemREF.maxDuration = itemREF.durationInWorld;
                        newLoot.worldDroppedItemREF.item = itemREF;
                        
                        newLoot.itemDataID =
                            RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID, CharacterData.ItemDataState.world);
                        
                        newLoot.worldDroppedItemREF.InitPhysics();
                        allWorldDroppedItems.Add(newLoot);
                    }
                    else
                    {
                        var newLoot = new LootBagHolder.Loot_Data();
                        newLoot.item = itemREF;
                        newLoot.count = stack;

                        newLoot.itemDataID = RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID, CharacterData.ItemDataState.world);
                        
                        lootData.Add(newLoot);
                    }

                    totalItemDropped++;
                }
            }

            if (totalItemDropped <= 0) return;
            if (lootData.Count <= 0) return;
            var lootbag = Instantiate(lootBagPrefab, nodeRef.gameObject.transform.position,
                lootBagPrefab.transform.rotation);

            var lootBagRef = lootbag.GetComponent<LootBagHolder>();
            lootBagRef.lootData = lootData;
            lootBagRef.lootBagName = npc.displayName + "'s Loot";
        }




        public void MoveItem(int prevBagIndex, int prevSlotIndex, int newBagIndex, int newSlotIndex)
        {
            if (CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemID != -1)
            {
                var previousSlot = new CharacterData.InventorySlotDATA();
                previousSlot.itemDataID = CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemDataID;
                previousSlot.itemID = CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemID;
                previousSlot.itemStack = CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemStack;

                var newSlot = new CharacterData.InventorySlotDATA();
                newSlot.itemDataID = CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemDataID;
                newSlot.itemID = CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemID;
                newSlot.itemStack = CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemStack;

                // Get current stack size
                var prevStackSize = previousSlot.itemStack;
                var newStackSize = newSlot.itemStack;

                RPGItem prevItemREF = RPGBuilderUtilities.GetItemFromID(previousSlot.itemID);
                RPGItem newItemREF = RPGBuilderUtilities.GetItemFromID(newSlot.itemID);

                if (previousSlot.itemID == newSlot.itemID && newStackSize < newItemREF.stackLimit)
                {

                    // Total stack count less then or equal to stack limit
                    if (prevStackSize + newStackSize <= newItemREF.stackLimit)
                    {
                        CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemDataID =
                            previousSlot.itemDataID;
                        CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemID = previousSlot.itemID;
                        CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemStack =
                            prevStackSize + newStackSize;

                        CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemDataID = -1;
                        CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemID = -1;
                        CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemStack = 0;
                    }
                    else
                    {
                        var removedFromStack = newItemREF.stackLimit - newStackSize;

                        CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemDataID = newSlot.itemDataID;
                        CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemID = newSlot.itemID;
                        CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemStack = newItemREF.stackLimit;

                        CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemDataID =
                            previousSlot.itemDataID;
                        CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemID = previousSlot.itemID;
                        CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemStack =
                            prevStackSize - removedFromStack;
                    }
                }
                else
                {
                    CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemDataID = previousSlot.itemDataID;
                    CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemID = previousSlot.itemID;
                    CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemStack = previousSlot.itemStack;

                    CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemDataID = newSlot.itemDataID;
                    CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemID = newSlot.itemID;
                    CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemStack = newSlot.itemStack;
                }
            }
            else
            {
                CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemDataID =
                    CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemDataID;
                CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemID =
                    CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemID;
                CharacterData.Instance.inventoryData.baseSlots[newSlotIndex].itemStack =
                    CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemStack;

                CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemID = -1;
                CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemDataID = -1;
                CharacterData.Instance.inventoryData.baseSlots[prevSlotIndex].itemStack = 0;
            }


            if (InventoryDisplayManager.Instance.thisCG.alpha == 1) InventoryDisplayManager.Instance.UpdateSlots();
        }



        public void RemoveItem(int itemID, int Amount, int bagIndex, int bagSlotIndex, bool removeAtSlot)
        {
            if (removeAtSlot)
            {
                if (CharacterData.Instance.inventoryData.baseSlots[bagSlotIndex].itemStack == Amount)
                {
                    CharacterData.Instance.inventoryData.baseSlots[bagSlotIndex].itemID = -1;
                    CharacterData.Instance.inventoryData.baseSlots[bagSlotIndex].itemDataID = -1;
                    CharacterData.Instance.inventoryData.baseSlots[bagSlotIndex].itemStack = 0;
                } else if (CharacterData.Instance.inventoryData.baseSlots[bagSlotIndex].itemStack > Amount)
                {
                    CharacterData.Instance.inventoryData.baseSlots[bagSlotIndex].itemStack -= Amount;
                }
            }
            else
            {
                foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                {
                    if(slot.itemID == -1 || slot.itemID != itemID) continue;
                    
                    if (slot.itemStack == Amount)
                    {
                        slot.itemDataID = -1;
                        slot.itemID = -1;
                        slot.itemStack = 0;
                        break;
                    }

                    if (slot.itemStack > Amount)
                    {
                        slot.itemStack -= Amount;
                        break;
                    }

                    if (slot.itemStack >= Amount) continue;
                    var remainingStacks = Amount - slot.itemStack;
                    slot.itemDataID = -1;
                    slot.itemID = -1;
                    slot.itemStack = 0;
                    RemoveItem(itemID, remainingStacks, -1, -1, false);
                    break;
                }
            }

            if (InventoryDisplayManager.Instance.thisCG.alpha == 1) InventoryDisplayManager.Instance.UpdateSlots();
            if (CraftingPanelDisplayManager.Instance.thisCG.alpha == 1)
                CraftingPanelDisplayManager.Instance.UpdateCraftingView();
            
            CharacterEventsManager.Instance.ItemLoss(RPGBuilderUtilities.GetItemFromID(itemID), Amount);
            
            ActionBarManager.Instance.CheckItemBarState();
        }


        public int canGetItem(int itemID, int Amount)
        {
            RPGItem itemREF = RPGBuilderUtilities.GetItemFromID(itemID);
            ItemCheckDATA itemCheckData = slotsNeededForLoot(itemID, Amount, itemREF.stackLimit);
            return itemCheckData.canLootAmount;
        }

        public int getEmptySlotsCount()
        {
            int total = 0;
            foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
            {
                if (slot.itemID == -1) total++;
            }

            return total;
        }

        public class ItemCheckDATA
        {
            public bool canBeLooted;
            public int canLootAmount;
            public int slotsNeeded;
        }

        public ItemCheckDATA slotsNeededForLoot(int itemID, int count, int stackMax)
        {
            ItemCheckDATA newItemCheckData = new ItemCheckDATA();

            int StacksNeededLeft = count;
            int total = 0;
            int canLootMax = 0;
            
                foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                {
                    if (slot.itemID != -1)
                    {
                        if (slot.itemID != itemID) continue;
                        if (slot.itemStack == stackMax) continue;
                        StacksNeededLeft -= stackMax - slot.itemStack;
                        canLootMax += stackMax - slot.itemStack;
                    }
                    else
                    {
                        StacksNeededLeft -= stackMax;
                        canLootMax += stackMax;
                        total++;
                    }

                    if (StacksNeededLeft < 0)
                    {
                        StacksNeededLeft = 0;
                    }

                    if (StacksNeededLeft == 0)
                    {
                        break;
                    }
                }

            newItemCheckData.slotsNeeded = total;
            newItemCheckData.canLootAmount = canLootMax;
            return newItemCheckData;
        }


        public void AddItem(int itemID, int Amount, bool automaticallyEquip, int itemDataID)
        {
            
            var itemToAdd = RPGBuilderUtilities.GetItemFromID(itemID);
            var stacked = false;

            if (itemToAdd.itemType == "CURRENCY")
            {
                AddCurrency(itemToAdd.onUseActions[0].currencyID, Amount);
            }
            else
            {
                int slotIndex = -1;
                foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                {
                    slotIndex++;
                    if (slot.itemID == -1 || slot.itemID != itemID) continue;
                    if (slot.itemStack >= itemToAdd.stackLimit) continue;
                    if (slot.itemStack + Amount > itemToAdd.stackLimit) continue;
                    slot.itemStack += Amount;
                    CharacterEventsManager.Instance.ItemGain(itemToAdd, Amount);
                    TreePointsManager.Instance.CheckIfItemGainPoints(itemToAdd);
                    stacked = true;
                    break;
                }

                if (!stacked)
                {
                    slotIndex = -1;
                    foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                    {
                        slotIndex++;
                        if (slot.itemID != -1) continue;
                        if (Amount <= itemToAdd.stackLimit)
                        {
                            slot.itemID = itemToAdd.ID;
                            slot.itemStack += Amount;
                            slot.itemDataID = itemDataID;

                            CharacterEventsManager.Instance.ItemGain(itemToAdd, Amount);
                            TreePointsManager.Instance.CheckIfItemGainPoints(itemToAdd);

                            if (automaticallyEquip)
                            {
                                EquipItem(itemToAdd, -1, slotIndex, slot.itemDataID);
                            }

                            break;
                        }

                        var remainingStacks = Amount - itemToAdd.stackLimit;
                        slot.itemID = itemToAdd.ID;
                        slot.itemStack = itemToAdd.stackLimit;
                        AddItem(itemID, remainingStacks, false, -1);
                        CharacterEventsManager.Instance.ItemGain(itemToAdd, itemToAdd.stackLimit);
                        TreePointsManager.Instance.CheckIfItemGainPoints(itemToAdd);
                        if (automaticallyEquip)
                        {
                            EquipItem(itemToAdd, -1, slotIndex,
                                CharacterData.Instance.inventoryData.baseSlots[slotIndex].itemDataID);
                        }

                        break;
                    }
                }
            }

            if (InventoryDisplayManager.Instance.thisCG.alpha == 1) InventoryDisplayManager.Instance.UpdateSlots();
            if (CraftingPanelDisplayManager.Instance.thisCG.alpha == 1)
                CraftingPanelDisplayManager.Instance.UpdateCraftingView();

            ActionBarManager.Instance.CheckItemBarState();
        }

        public bool isItemOwned(int ID, int count)
        {
            foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
            {
                if (slot.itemID == -1) continue;
                if (slot.itemID != ID) continue;
                if (slot.itemStack >= count)
                    return true;
            }

            return false;
        }

        public bool hasEnoughCurrency(int currencyID, int amount)
        {
            RPGCurrency currency = RPGBuilderUtilities.GetCurrencyFromID(currencyID);
            var curTotalCurrencyAmount = getTotalCurrencyOfGroup(currency);
            var priceInLowestCurrency = getValueInLowestCurrency(currency, amount);
            return curTotalCurrencyAmount >= priceInLowestCurrency;
        }

        private int getTotalCurrencyOfGroup(RPGCurrency initialCurrency)
        {
            var thisTotalLowestCurrency = 0;
            var lowestCurrency = RPGBuilderUtilities.GetCurrencyFromID(initialCurrency.lowestCurrencyID);

            for (var x = lowestCurrency.aboveCurrencies.Count; x > 0; x--)
            {
                var currenciesBeforeThisOne = x - 1;
                if (currenciesBeforeThisOne > 0)
                {
                    var thisCurrencyAmount = CharacterData.Instance.getCurrencyAmount(
                        RPGBuilderUtilities.GetCurrencyFromID(lowestCurrency.aboveCurrencies[x - 1].currencyID));
                    for (var i = 0; i < currenciesBeforeThisOne; i++)
                        thisCurrencyAmount *= RPGBuilderUtilities
                            .GetCurrencyFromID(lowestCurrency.aboveCurrencies[x - 2].currencyID).AmountToConvert;
                    thisCurrencyAmount *= lowestCurrency.AmountToConvert;
                    thisTotalLowestCurrency += thisCurrencyAmount;
                }
                else
                {
                    var thisCurrencyAmount =
                        CharacterData.Instance.getCurrencyAmount(
                            RPGBuilderUtilities.GetCurrencyFromID(lowestCurrency.aboveCurrencies[x - 1].currencyID)) *
                        RPGBuilderUtilities.GetCurrencyFromID(lowestCurrency.aboveCurrencies[x - 1].currencyID)
                            .AmountToConvert;
                    thisTotalLowestCurrency += thisCurrencyAmount;
                }
            }

            thisTotalLowestCurrency += CharacterData.Instance.getCurrencyAmount(lowestCurrency);
            return thisTotalLowestCurrency;
        }

        public int getTotalCountOfItem(RPGItem item)
        {
            var totalcount = 0;
            foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                if (slot.itemID != -1 && slot.itemID == item.ID)
                    totalcount += slot.itemStack;

            return totalcount;
        }

        public int getTotalCountOfItemByItemID(int ItemID)
        {
            var totalcount = 0;
            foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                if (slot.itemID != -1 && slot.itemID == ItemID)
                    totalcount += slot.itemStack;

            return totalcount;
        }

        private int getValueInLowestCurrency(RPGCurrency initialCurrency, int amount)
        {
            var lowestCurrency = RPGBuilderUtilities.GetCurrencyFromID(initialCurrency.lowestCurrencyID);
            if (initialCurrency == lowestCurrency && initialCurrency.aboveCurrencies.Count == 0)
            {
                return amount;
            }
            var thisTotalLowestCurrency = 0;
            if (lowestCurrency == initialCurrency && amount < initialCurrency.maxValue) return amount;

            var amountOfAboveCurrency = amount / initialCurrency.AmountToConvert;
            var restOfThisCurrency = amount % initialCurrency.AmountToConvert;

            amountOfAboveCurrency =
                amountOfAboveCurrency * initialCurrency.AmountToConvert * initialCurrency.AmountToConvert;
            restOfThisCurrency = restOfThisCurrency * initialCurrency.AmountToConvert;
            thisTotalLowestCurrency += amountOfAboveCurrency;
            thisTotalLowestCurrency += restOfThisCurrency;
            return thisTotalLowestCurrency;
        }

        private void ConvertCurrenciesToGroups(RPGCurrency lowestCurrency, int totalAmount)
        {
            setCurrencyAmount(lowestCurrency, 0);
            foreach (var t in lowestCurrency.aboveCurrencies)
            {
                var aboceCurrency = RPGBuilderUtilities.GetCurrencyFromID(t.currencyID);
                setCurrencyAmount(aboceCurrency, 0);
            }

            for (var i = lowestCurrency.aboveCurrencies.Count; i > 0; i--)
            {
                var inferiorCurrenciesCount = i - 1;
                var hasToBeDividedBy = 0;
                for (var u = inferiorCurrenciesCount; u > 0; u--)
                    if (hasToBeDividedBy == 0)
                        hasToBeDividedBy += RPGBuilderUtilities
                            .GetCurrencyFromID(lowestCurrency.aboveCurrencies[u - 1].currencyID).AmountToConvert;
                    else
                        hasToBeDividedBy *= RPGBuilderUtilities
                            .GetCurrencyFromID(lowestCurrency.aboveCurrencies[u - 1].currencyID).AmountToConvert;
                if (hasToBeDividedBy == 0)
                    hasToBeDividedBy += lowestCurrency.AmountToConvert;
                else
                    hasToBeDividedBy *= lowestCurrency.AmountToConvert;

                if (hasToBeDividedBy <= 0) continue;
                var amountOfThisCurrency = totalAmount / hasToBeDividedBy;
                totalAmount -= amountOfThisCurrency * hasToBeDividedBy;
                setCurrencyAmount(
                    RPGBuilderUtilities.GetCurrencyFromID(lowestCurrency.aboveCurrencies[i - 1].currencyID),
                    amountOfThisCurrency);
            }

            setCurrencyAmount(lowestCurrency, totalAmount);
        }

        private void setCurrencyAmount(RPGCurrency currency, int amount)
        {
            CharacterData.Instance.currencies[CharacterData.Instance.getCurrencyIndex(currency)].amount = amount;
        }

        private void TryBuyItemFromMerchant(RPGItem item, RPGCurrency currency, int cost)
        {
            var curTotalCurrencyAmount = getTotalCurrencyOfGroup(currency);
            var priceInLowestCurrency = getValueInLowestCurrency(currency, cost);
            if (curTotalCurrencyAmount >= priceInLowestCurrency)
            {
                // enough to buy
                int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(item.ID, 1, false, false);
                if (itemsLeftOver == 0)
                {
                    curTotalCurrencyAmount -= priceInLowestCurrency;
                    ConvertCurrenciesToGroups(RPGBuilderUtilities.GetCurrencyFromID(currency.lowestCurrencyID), curTotalCurrencyAmount);
                }
                else
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                }
            }
            else
            {
                // not enough to buy
                return;
            }

            if (InventoryDisplayManager.Instance.thisCG.alpha == 1) InventoryDisplayManager.Instance.UpdateCurrency();
        }
        
        public bool isitemAlreadyInLootList(int itemID, List<TemporaryLootItemData> lootList)
        {
            foreach (var item in lootList)
            {
                if(item.itemID != itemID) continue;
                return true;
            }

            return false;
        }
        
        
        public List<TemporaryLootItemData> updateItemDataInLootList(int itemID, List<TemporaryLootItemData> lootList, int amount)
        {
            foreach (var item in lootList)
            {
                if(item.itemID != itemID) continue;
                item.count += amount;
            }

            return lootList;
        }

        public List<TemporaryLootItemData> HandleLootList(int itemID, List<TemporaryLootItemData> lootList, int count)
        {
            if (isitemAlreadyInLootList(itemID, lootList))
            {
                return updateItemDataInLootList(itemID, lootList, count);
            }

            TemporaryLootItemData newCraftedItem = new TemporaryLootItemData();
            newCraftedItem.itemID = itemID;
            newCraftedItem.count = count;
            lootList.Add(newCraftedItem);
            return lootList;
        }

        public class TemporaryLootItemData
        {
            public int itemID;
            public int count;
        }

        public void RemoveCurrency(int currencyID, int amount)
        {
            RPGCurrency currencyREF = RPGBuilderUtilities.GetCurrencyFromID(currencyID);
            if (currencyREF == null) return;
            var curTotalCurrencyAmount = getTotalCurrencyOfGroup(currencyREF);
            var priceInLowestCurrency = getValueInLowestCurrency(currencyREF, amount);
            if (curTotalCurrencyAmount < priceInLowestCurrency) return;
            curTotalCurrencyAmount -= priceInLowestCurrency;
            ConvertCurrenciesToGroups(RPGBuilderUtilities.GetCurrencyFromID(currencyREF.lowestCurrencyID),
                curTotalCurrencyAmount);
            CharacterEventsManager.Instance.CurrencyLoss();
        }
        public void BuyItemFromMerchant(RPGItem item, RPGCurrency currency, int amount)
        {
            TryBuyItemFromMerchant(item, currency, amount);
        }


        public static InventoryManager Instance { get; private set; }
    }
}