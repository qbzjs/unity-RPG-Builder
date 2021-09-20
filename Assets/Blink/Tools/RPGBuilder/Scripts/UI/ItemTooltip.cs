using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UI
{
    public class ItemTooltip : MonoBehaviour
    {
        public CanvasGroup thisCG;
        public RectTransform canvasRect, thisRect, contentRect;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemSlotTypeText;
        public TextMeshProUGUI itemTypeText, itemQualityText;
        public Image icon, itemBackground;
        
        public TextMeshProUGUI statsText, descriptionText, requirementsText, sellPriceText, statsChangeText, gearSetText;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        private void Update()
        {
            if (thisCG.alpha != 1) return;
            HandleTooltipPosition();
        }

        private void HandleTooltipPosition()
        {
            Vector2 anchoredPos = Input.mousePosition / canvasRect.localScale.x;
            if (cursorIsRightSide())
            {
                if (anchoredPos.x + (thisRect.rect.width+100f) > canvasRect.rect.width)
                    anchoredPos.x -= thisRect.rect.width + 10f;
                else
                    anchoredPos.x += 10f;
            }
            else
            {
                anchoredPos.x += 10f;
            }

            anchoredPos.y += contentRect.sizeDelta.y + 10f;

            if (anchoredPos.y + thisRect.rect.height > canvasRect.rect.height)
            {
                anchoredPos.y = canvasRect.rect.height - thisRect.rect.height;
            }

            thisRect.anchoredPosition = anchoredPos;
        }
        

        private bool cursorIsRightSide()
        {
            return Input.mousePosition.x > Screen.width / 2.0f;
        }
        

        void ResetContent()
        {
            statsText.text = "";
            descriptionText.text = "";
            requirementsText.text = "";
            sellPriceText.text = "";
            statsChangeText.text = "";
            gearSetText.text = "";
        }

        void HandleShowTooltip()
        {
            if (tooltipVisibleCoroutine != null)
            {
                StopCoroutine(tooltipVisibleCoroutine);
                tooltipVisibleCoroutine = null;
            }
            tooltipVisibleCoroutine = StartCoroutine(tooltipVisible());
        }
        
        public void ShowCurrencyTooltip(int currencyID)
        {
            ResetContent();

            var currency = RPGBuilderUtilities.GetCurrencyFromID(currencyID);

            itemNameText.text = currency.displayName;
            icon.sprite = currency.icon;
            itemBackground.enabled = false;
            itemSlotTypeText.text = "";
            itemTypeText.text = "";
            HandleShowTooltip();
        }

        public void ShowTreePointTooltip(int ID)
        {
            ResetContent();

            var treePoint = RPGBuilderUtilities.GetTreePointFromID(ID);

            itemNameText.text = treePoint._displayName;
            icon.sprite = treePoint.icon;
            itemBackground.enabled = false;
            itemSlotTypeText.text = "";
            HandleShowTooltip();
        }

        private Coroutine tooltipVisibleCoroutine;
        private IEnumerator tooltipVisible()
        {
            yield return new WaitForSeconds(0.05f);
            thisCG.alpha = 1;
        }

        public void Show(int itemID, int itemDataID, bool showCompare)
        {
            ResetContent();

            var item = RPGBuilderUtilities.GetItemFromID(itemID);

            itemNameText.text = item.displayName;
            itemSlotTypeText.text = "";
            itemTypeText.text = "";
            switch (item.itemType)
            {
                case "WEAPON":
                    itemTypeText.text = item.weaponType;
                    itemSlotTypeText.text = item.slotType;
                    break;
                case "ARMOR":
                    itemTypeText.text = item.equipmentSlot;
                    itemSlotTypeText.text = item.armorType;
                    break;
                case "ENCHANTMENT":
                    if (item.enchantmentID != -1)
                    {
                        RPGEnchantment enchantmentREF = RPGBuilderUtilities.GetEnchantmentFromID(item.enchantmentID);
                        statsText.text = "Apply the " + enchantmentREF.displayName +
                                         " Enchantment to an item. \n \n";
                        int tierIndex = 1;
                        foreach (var t in enchantmentREF.enchantmentTiers)
                        {
                            statsText.text += "Tier " + tierIndex + ": \n";
                            foreach (var t1 in t.stats)
                            {
                                float amt = t1.amount;
                                if (amt == 0) continue;

                                RPGStat statREF = RPGBuilderUtilities.GetStatFromID(t1.statID);
                                string modifierText = amt > 0 ? "+" : "-";
                                string percentText = "";
                                if (t1.isPercent || statREF.isPercentStat)
                                {
                                    percentText = "%";
                                }

                                statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.displayName);
                            }

                            statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                            tierIndex++;
                        }

                        foreach (var t in enchantmentREF.applyRequirements)
                        {
                            string reqText = "";
                            switch (t.type)
                            {
                                case RPGEnchantment.ApplyRequirementType.ItemType:
                                    reqText = "Item Type required: " + t.itemType;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.ItemRarity:
                                    reqText = "Item Rarity required: " + t.itemRarity;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.ArmorType:
                                    reqText = "Armor Type required: " + t.armorType;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.ArmorSlot:
                                    reqText = "Armor Slot required: " + t.armorSlot;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.WeaponType:
                                    reqText = "Weapon Type required: " + t.weaponType;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.WeaponSlot:
                                    reqText = "Weapon Slot required: " + t.weaponSlot;
                                    break;
                            }
                            requirementsText.text += RPGBuilderUtilities.addLineBreak(reqText);
                        }
                    }
                    break;
                
                    case "GEM":
                        statsText.text = "Can be socketted to an armor or weapon.\n \n";
                        statsText.text += "Socket Type: " + item.gemData.socketType + "\n";
                    
                        foreach (var stat in item.gemData.gemStats)
                        {
                            float amt = stat.amount;
                            if (amt == 0) continue;

                            string modifierText = amt > 0 ? "+" : "-";
                            string percentText = "";
                            
                            RPGStat statREF = RPGBuilderUtilities.GetStatFromID(stat.statID);
                            if (stat.isPercent || statREF.isPercentStat)
                            {
                                percentText = "%";
                            }

                            statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.displayName);
                        }
                        statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                        break;

            }

            if (item.itemType != "WEAP0ON" && item.itemType != "ARMOR")
            {
                itemTypeText.text = item.itemType;
            }

            Color itemQualityColor = RPGBuilderUtilities.getItemRarityColor(item.rarity);
            itemQualityText.text = item.rarity;
            itemQualityText.color = itemQualityColor;
            itemNameText.color = itemQualityColor;
            
            icon.sprite = item.icon;
            itemBackground.enabled = true;
            itemBackground.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);

            if (item.itemType == "WEAPON")
            {
                if (item.maxDamage > 0)
                {
                    statsText.text += "Attack Speed: " + item.AttackSpeed + "\nDamage: " + item.minDamage + " - " + item.maxDamage + "\n\n";
                }
            }

            foreach (var t in item.stats)
            {
                float amt = t.amount;
                if (amt == 0) continue;

                string modifierText = amt > 0 ? "+" : "";
                string percentText = "";
                            
                RPGStat statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                if (t.isPercent || statREF.isPercentStat)
                {
                    percentText = "%";
                }

                statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.displayName);
            }

            CharacterData.ItemDATA itemData = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            if (itemData != null)
            {
                if (itemData.rdmItemID != -1)
                {
                    List<RPGItemDATA.RandomizedStat> rdmStatList = new List<RPGItemDATA.RandomizedStat>();
                    int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(itemData.rdmItemID);
                    rdmStatList = CharacterData.Instance.allRandomizedItems[rdmItemIndex].randomStats;


                    if (rdmItemIndex != -1)
                    {
                        int rdmIndex = 0;
                        foreach (var t in rdmStatList)
                        {
                            float amt = t.statValue;
                            if (amt == 0) continue;

                            string modifierText = amt > 0 ? "+" : "";
                            string percentText = "";
                            
                            RPGStat statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                            if (item.randomStats[rdmIndex].isPercent || statREF.isPercentStat)
                            {
                                percentText = "%";
                            }

                            statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.displayName);
                            rdmIndex++;
                        }
                    }
                }

                if (itemData.enchantmentID != -1)
                {
                    RPGEnchantment enchantREF = RPGBuilderUtilities.GetEnchantmentFromID(itemData.enchantmentID);
                    statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                    statsText.text += RPGBuilderUtilities.addLineBreak("<color=#00DB96> Enchanted: " + enchantREF.displayName);
                    foreach (var t in enchantREF.enchantmentTiers[itemData.enchantmentTierIndex].stats)
                    {
                        float amt = t.amount;
                        if (amt == 0) continue;

                        string modifierText = amt > 0 ? "+" : "-";
                        string percentText = "";
                            
                        RPGStat statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                        if (t.isPercent || statREF.isPercentStat)
                        {
                            percentText = "%";
                        }
                        statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.displayName);
                    }

                    statsText.text += "</color>";
                }

                if (itemData.sockets.Count > 0)
                {
                    statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                    statsText.text += RPGBuilderUtilities.addLineBreak("<color=#00DB96>Sockets:");
                    foreach (var t in itemData.sockets)
                    {
                        if (t.gemItemID != -1)
                        {
                            RPGItem gemItemREF = RPGBuilderUtilities.GetItemFromID(t.gemItemID);
                            if (gemItemREF == null) continue;
                            statsText.text += RPGBuilderUtilities.addLineBreak(t.socketType + ": " + gemItemREF.displayName);
                            foreach (var v in gemItemREF.gemData.gemStats)
                            {
                                float amt = v.amount;
                                if (amt == 0) continue;

                                string modifierText = amt > 0 ? "+" : "-";
                                string percentText = "";
                            
                                RPGStat statREF = RPGBuilderUtilities.GetStatFromID(v.statID);
                                if (v.isPercent || statREF.isPercentStat)
                                {
                                    percentText = "%";
                                }
                                statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.displayName);
                            }
                        }
                        else
                        {
                            statsText.text += RPGBuilderUtilities.addLineBreak("<color=#575757>" + t.socketType + ": (Empty)</color>");
                        }
                    }
                    statsText.text += "</color>";
                }
            }
            
            descriptionText.text = item.description;

            if (item.sellCurrencyID != -1)
            {
                RPGCurrency currencyREF = RPGBuilderUtilities.GetCurrencyFromID(item.sellCurrencyID);
                sellPriceText.text = item.sellPrice + " " + currencyREF.displayName;
            }
            

            foreach (var t in item.useRequirements)
            {
                bool reqMet = false;
                string reqText = "";
                var intValue1 = -1;
                var intValue2 = -1;
                switch (t.requirementType)
                {
                    case RequirementsManager.RequirementType._class:
                        intValue1 = t.classRequiredID;
                        reqText = "Class: " + RPGBuilderUtilities.GetClassFromID(t.classRequiredID).displayName;
                        break;
                    case RequirementsManager.RequirementType.race:
                        reqText = "Race: " + RPGBuilderUtilities.GetRaceFromID(t.raceRequiredID).displayName;
                        break;
                    case RequirementsManager.RequirementType.abilityKnown:
                        reqText = RPGBuilderUtilities.GetAbilityFromID(t.abilityRequiredID).displayName + " Known";
                        break;
                    case RequirementsManager.RequirementType.bonusKnown:
                        reqText = RPGBuilderUtilities.GetBonusFromID(t.bonusRequiredID).displayName + " Known";
                        break;
                    case RequirementsManager.RequirementType.classLevel:
                        intValue1 = CharacterData.Instance.classDATA.currentClassLevel;
                        reqText = "Level: " + t.classLevelValue;
                        break;
                    case RequirementsManager.RequirementType.itemOwned:
                        reqText = "Item Owned: " + RPGBuilderUtilities.GetItemFromID(t.itemRequiredID).displayName;
                        break;
                    case RequirementsManager.RequirementType.npcKilled:
                        reqText = "Killed: " + RPGBuilderUtilities.GetNPCFromID(t.npcRequiredID).displayName;
                        break;
                    case RequirementsManager.RequirementType.questState:
                        reqText = "Quest: " + RPGBuilderUtilities.GetQuestFromID(t.questRequiredID).displayName + " " + t.questStateRequired;
                        break;
                    case RequirementsManager.RequirementType.recipeKnown:
                        reqText = "Recipe: " + RPGBuilderUtilities.GetCraftingRecipeFromID(t.craftingRecipeRequiredID).displayName + " Known";
                        break;
                    case RequirementsManager.RequirementType.skillLevel:
                        intValue1 = RPGBuilderUtilities.getSkillLevel(t.skillRequiredID);
                        reqText = RPGBuilderUtilities.GetSkillFromID(t.skillRequiredID).displayName + " Level " + t.skillLevelValue;
                        break;
                    case RequirementsManager.RequirementType.abilityNotKnown:
                        reqText = RPGBuilderUtilities.GetAbilityFromID(t.abilityRequiredID).displayName + " Unkown";
                        break;
                    case RequirementsManager.RequirementType.bonusNotKnown:
                        reqText = RPGBuilderUtilities.GetBonusFromID(t.bonusRequiredID).displayName + " Unkown";
                        break;
                    case RequirementsManager.RequirementType.recipeNotKnown:
                        reqText = RPGBuilderUtilities.GetCraftingRecipeFromID(t.craftingRecipeRequiredID).displayName + " Unkown";
                        break;
                    case RequirementsManager.RequirementType.resourceNodeKnown:
                        reqText = RPGBuilderUtilities.GetResourceNodeFromID(t.resourceNodeRequiredID).displayName + " Known";
                        break;
                    case RequirementsManager.RequirementType.resourceNodeNotKnown:
                        reqText = RPGBuilderUtilities.GetResourceNodeFromID(t.resourceNodeRequiredID).displayName + " Unkown";
                        break;
                    case RequirementsManager.RequirementType.weaponTemplateLevel:
                        intValue1 = RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateRequiredID);
                        reqText = RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateRequiredID).displayName + " Level " + t.weaponTemplateLevelValue;
                        break;
                }
                
                reqMet = RequirementsManager.Instance.HandleRequirementType(t, intValue1, intValue2,false);
                reqText = AssignRequirementColor(reqMet, reqText);
                requirementsText.text += RPGBuilderUtilities.addLineBreak(reqText);
            }

            RPGGearSet itemGearSet = RPGBuilderUtilities.getItemGearSet(item.ID);
            if (itemGearSet != null)
            {
                gearSetText.text = itemGearSet.displayName + ":";
                gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);

                int curItemIndex = 1;
                foreach (var t in itemGearSet.itemsInSet)
                {
                    if (RPGBuilderUtilities.isItemEquipped(t.itemID))
                    {
                        gearSetText.text += "<color=green>";
                    }
                    else
                    {
                        gearSetText.text += "<color=#575757>";
                    }

                    gearSetText.text += RPGBuilderUtilities.GetItemFromID(t.itemID).displayName + "</color>";
                    if (curItemIndex < itemGearSet.itemsInSet.Count) gearSetText.text += " - ";
                    curItemIndex++;
                }

                gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                int gearSetTierIndex = RPGBuilderUtilities.getGearSetTierIndex(itemGearSet);
                int curTierIndex = 1;
                for (var index = 0; index < itemGearSet.gearSetTiers.Count; index++)
                {
                    if (index <= gearSetTierIndex)
                    {
                        gearSetText.text += "<color=green>";
                    }
                    else
                    {
                        gearSetText.text += "<color=#575757>";
                    }

                    gearSetText.text += "(" + itemGearSet.gearSetTiers[index].equippedAmount + ") Tier " + curTierIndex + ": ";
                    gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                    int curTierStatIndex = 1;
                    foreach (var t in itemGearSet.gearSetTiers[index].gearSetTierStats)
                    {
                        string modifierText = t.amount > 0 ? "+" : "-";
                        string percentText = "";
                            
                        RPGStat statREF = RPGBuilderUtilities.GetStatFromID(t.statID);
                        if (t.isPercent || statREF.isPercentStat)
                        {
                            percentText = "%";
                        }
                        gearSetText.text += modifierText + t.amount + percentText + " " + statREF.displayName;
                        if (curTierStatIndex < itemGearSet.gearSetTiers[index].gearSetTierStats.Count) gearSetText.text += ", ";
                        curTierStatIndex++;
                    }
                    gearSetText.text += "</color>";
                    gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                    curTierIndex++;
                }
            }

            if (showCompare)
            {
                // SHOW STAT DIFFERENCES

                RPGItem itemREF = null;
                List<string> statGains = new List<string>();
                List<string> statLosses = new List<string>();
                switch (item.itemType)
                {
                    case "ARMOR":
                        itemREF = RPGBuilderUtilities.getEquippedArmor(item.equipmentSlot);
                        int armorIndex = RPGBuilderUtilities.getArmorSlotIndex(item.equipmentSlot);
                        if (itemREF != null)
                        {
                            foreach (var t in CombatManager.playerCombatNode.nodeStats)
                            {
                                float inspectedArmorStatVal = getItemStatValue(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1);

                                float armorPieceVal = getItemStatValue(t.stat.ID, itemREF,
                                    RPGBuilderUtilities.getRandomItemIDFromDataID(InventoryManager.Instance.equippedArmors[armorIndex].temporaryItemDataID));

                                if (inspectedArmorStatVal == 0 && armorPieceVal == 0) continue;
                                if (inspectedArmorStatVal == armorPieceVal) continue;
                                if (inspectedArmorStatVal != 0)
                                {
                                    bool isGain = !(armorPieceVal > inspectedArmorStatVal);
                                    float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedArmorStatVal,
                                        armorPieceVal);
                                    diffAmt = (float) Math.Round(diffAmt, 2);
                                    string statChangeText = "";
                                    string modifierText = isGain ? "+" : "-";
                                    string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                    statChangeText += AssignRequirementColor(isGain,
                                        modifierText + diffAmt + percentText + " " + t.stat.displayName);
                                    statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                    if (isGain)
                                        statGains.Add(statChangeText);
                                    else
                                        statLosses.Add(statChangeText);
                                }
                                else
                                {
                                    if (armorPieceVal == 0) continue;
                                    armorPieceVal = (float) Math.Round(armorPieceVal, 2);
                                    string statChangeText = "";
                                    string modifierText = "-";
                                    string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                    statChangeText += AssignRequirementColor(false,
                                        modifierText + armorPieceVal + percentText + " " + t.stat.displayName);
                                    statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                    statLosses.Add(statChangeText);
                                }
                            }
                        }

                        break;
                    case "WEAPON":
                        switch (item.slotType)
                        {
                            case "TWO HAND":
                                itemREF = InventoryManager.Instance.equippedWeapons[0].itemEquipped;
                                RPGItem itemREF2 = InventoryManager.Instance.equippedWeapons[1].itemEquipped;

                                if (itemREF == null && itemREF2 == null) break;
                                foreach (var t in CombatManager.playerCombatNode.nodeStats)
                                {
                                    float weapon1StatVal = 0;
                                    float weapon2StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(InventoryManager.Instance.equippedWeapons[0].temporaryItemDataID));
                                    }

                                    if (itemREF2 != null)
                                    {
                                        weapon2StatVal = getItemStatValue(t.stat.ID, itemREF2,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(InventoryManager.Instance.equippedWeapons[1].temporaryItemDataID));
                                    }
                                    
                                    float otherWeaponsStatVal = weapon1StatVal + weapon2StatVal;
                                    if(inspectedWeaponStatVal == 0 && otherWeaponsStatVal == 0) continue;
                                    if(inspectedWeaponStatVal == otherWeaponsStatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(otherWeaponsStatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            otherWeaponsStatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (otherWeaponsStatVal == 0) continue;
                                        otherWeaponsStatVal = (float)Math.Round(otherWeaponsStatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + otherWeaponsStatVal + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                            case "MAIN HAND":
                                itemREF = InventoryManager.Instance.equippedWeapons[0].itemEquipped;

                                if (itemREF == null) break;
                                foreach (var t in CombatManager.playerCombatNode.nodeStats)
                                {
                                    float weapon1StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(InventoryManager.Instance.equippedWeapons[0].temporaryItemDataID));
                                    }
                                    
                                    if(inspectedWeaponStatVal == 0 && weapon1StatVal == 0) continue;
                                    if(inspectedWeaponStatVal == weapon1StatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(weapon1StatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            weapon1StatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (weapon1StatVal == 0) continue;
                                        weapon1StatVal = (float)Math.Round(weapon1StatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + weapon1StatVal + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                            case "OFF HAND":
                                itemREF = InventoryManager.Instance.equippedWeapons[1].itemEquipped;

                                if (itemREF == null) break;
                                foreach (var t in CombatManager.playerCombatNode.nodeStats)
                                {
                                    float weapon1StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(InventoryManager.Instance.equippedWeapons[1].temporaryItemDataID));
                                    }
                                    
                                    if(inspectedWeaponStatVal == 0 && weapon1StatVal == 0) continue;
                                    if(inspectedWeaponStatVal == weapon1StatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(weapon1StatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            weapon1StatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (weapon1StatVal == 0) continue;
                                        weapon1StatVal = (float)Math.Round(weapon1StatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + weapon1StatVal + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                            case "ANY HAND":
                                int weaponComparedIndex = 0;
                                itemREF = InventoryManager.Instance.equippedWeapons[0].itemEquipped;

                                if (itemREF == null)
                                {
                                    itemREF = InventoryManager.Instance.equippedWeapons[1].itemEquipped;
                                    weaponComparedIndex = 1;
                                }
                                if (itemREF == null) break;
                                foreach (var t in CombatManager.playerCombatNode.nodeStats)
                                {
                                    float weapon1StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(InventoryManager.Instance.equippedWeapons[weaponComparedIndex].temporaryItemDataID));
                                    }
                                    
                                    if(inspectedWeaponStatVal == 0 && weapon1StatVal == 0) continue;
                                    if(inspectedWeaponStatVal == weapon1StatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(weapon1StatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            weapon1StatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (weapon1StatVal == 0) continue;
                                        weapon1StatVal = (float)Math.Round(weapon1StatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.stat.ID, item, itemData!= null ? itemData.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + weapon1StatVal + percentText + " " + t.stat.displayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                        }

                        break;
                }

                if (statLosses.Count + statGains.Count > 0)
                {
                    statsChangeText.text = RPGBuilderUtilities.addLineBreak("STAT CHANGES IF EQUIPPED:");

                    foreach (var t in statGains)
                    {
                        statsChangeText.text += t;
                    }

                    foreach (var t in statLosses)
                    {
                        statsChangeText.text += t;
                    }
                }
            }
            HandleShowTooltip();
        }

        bool isItemStatPercent(int statID, RPGItem item, int rdmItemID)
        {
            if (rdmItemID != -1)
            {
                List<RPGItemDATA.RandomizedStat> rdmStatList = new List<RPGItemDATA.RandomizedStat>();
                int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(rdmItemID);
                    rdmStatList = CharacterData.Instance.allRandomizedItems[rdmItemIndex].randomStats;
                
                foreach (var t in item.randomStats)
                {
                    foreach (var t1 in rdmStatList)
                    {
                        if (t.statID != t1.statID) continue;
                        if (t.statID != statID) continue;
                        return t.isPercent;
                    }
                }
            }
            
            foreach (var t in item.stats)
            {
                if (t.statID == statID)
                {
                    return t.isPercent;
                }
            }

            RPGStat statREF = RPGBuilderUtilities.GetStatFromID(statID);
            return statREF.isPercentStat;
        }

        float getItemStatValue(int statID, RPGItem item, int rdmItemID)
        {
            float totalAmt = 0;
            foreach (var t in item.stats)
            {
                if(t.statID == statID)
                {
                    totalAmt += t.amount;
                }
            }

            if (rdmItemID == -1) return totalAmt;
            {
                List<RPGItemDATA.RandomizedStat> rdmStatList = new List<RPGItemDATA.RandomizedStat>();
                int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(rdmItemID);
                rdmStatList = CharacterData.Instance.allRandomizedItems[rdmItemIndex].randomStats;

                if (rdmItemIndex == -1) return totalAmt;
                foreach (var t in item.randomStats)
                {
                    foreach (var t1 in rdmStatList)
                    {
                        if (t.statID != t1.statID) continue;
                        if (t.statID != statID) continue;
                        totalAmt += t1.statValue;
                    }
                }
            }

            return totalAmt;
        }

        string AssignRequirementColor(bool reqMet, string reqText)
        {
            return reqMet ? "<color=green>" + reqText + "</color>" : "<color=red>" + reqText + "</color>";
        }

        public void Hide()
        {
            if (tooltipVisibleCoroutine != null)
            {
                StopCoroutine(tooltipVisibleCoroutine);
                tooltipVisibleCoroutine = null;
            }
            thisCG.alpha = 0f;
            thisCG.blocksRaycasts = false;
            thisCG.interactable = false;
            ResetContent();
        }

        private void Awake()
        {
            Hide();
        }
        
        

        public static ItemTooltip Instance { get; private set; }
    }
}