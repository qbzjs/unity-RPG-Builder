using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BLINK.RPGBuilder.Managers
{
    public class EnchantingPanelDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;
        private bool showing = false;

        public List<EnchantSlotHolder> curEnchantSlots = new List<EnchantSlotHolder>();
        public List<EnchantmentDATA> enchantList = new List<EnchantmentDATA>();
        public List<GameObject> curCostSlots = new List<GameObject>();
        public Transform enchantSlotParent, costSlotsParent;
        public GameObject enchantSlotPrefab, itemSlotPrefab;
        public TextMeshProUGUI enchantTierStatBonusesText, enchantmentNameText;
        public Button enchantButton;
        public Transform enchantedItemParent;
        
        public Image castBarFill;

        public int selectedEnchant = -1;
        
        private bool isEnchanting;
        private float curEnchantTime, maxEnchantTime;

        public class CurrentEnchantedItemDATA
        {
            public RPGItem item;
            public int itemDataID;
            public GameObject enchantedItemGO;
        }

        public CurrentEnchantedItemDATA curEnchantedItemData = new CurrentEnchantedItemDATA();

        private RPGItem currentlyViewedItemEnchant;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static EnchantingPanelDisplayManager Instance { get; private set; }

        private void FixedUpdate()
        {
            if (!isEnchanting) return;
            curEnchantTime += Time.deltaTime;
            castBarFill.fillAmount = curEnchantTime / maxEnchantTime;

            if (!(curEnchantTime >= maxEnchantTime)) return;
            EnchantingManager.Instance.EnchantItem(curEnchantedItemData.itemDataID, CharacterData.Instance.itemsDATA[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)].enchantmentTierIndex, enchantList[selectedEnchant].enchantment);
            isEnchanting = false;
            curEnchantTime = 0;
            maxEnchantTime = 0;
        }
        
        private void ClearAllEnchantSlots()
        {
            foreach (var t in curEnchantSlots) Destroy(t.gameObject);
            curEnchantSlots.Clear();
        }

        private void ClearAllCostSlots()
        {
            foreach (var t in curCostSlots) Destroy(t);
            curCostSlots.Clear();
        }

        public void StopCurrentEnchant()
        {
            isEnchanting = false;
            curEnchantTime = 0;
            maxEnchantTime = 0;
            castBarFill.fillAmount = 0;
        }

        public void AssignEnchantedItem(RPGItem item, int itemDataID)
        {
            if(isEnchanting)StopCurrentEnchant();
            Destroy(curEnchantedItemData.enchantedItemGO);
            var enchantedItemSlot = Instantiate(itemSlotPrefab, enchantedItemParent);
            var slotREF = enchantedItemSlot.GetComponent<EnchantCostSlotHolder>();
            var itemREF = RPGBuilderUtilities.GetItemFromID(item.ID);
            slotREF.InitSlot(itemREF.icon, true, 0, itemREF, false, itemDataID);
            
            curEnchantedItemData.item = item;
            curEnchantedItemData.itemDataID = itemDataID;
            curEnchantedItemData.enchantedItemGO = enchantedItemSlot;
            
            UpdateEnchantingView();
        }

        private void ResetEnchantedItem()
        {
            curEnchantedItemData.item = null;
            curEnchantedItemData.itemDataID = -1;
            
            Destroy(curEnchantedItemData.enchantedItemGO);
        }
        
        public void ClickEnchant()
        {
            if (isEnchanting) return;
            int curItemEnchantTier = CharacterData.Instance.itemsDATA[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)].enchantmentTierIndex;
            if (curItemEnchantTier == -1)
            {
                curItemEnchantTier = 0;
            }

            foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[curItemEnchantTier].currencyCosts)
            {
                if (!InventoryManager.Instance.hasEnoughCurrency(t.currencyID, t.amount))
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("Not enough currency", 3);
                    return;
                }
            }
            foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[curItemEnchantTier].itemCosts)
            {
                var totalOfThisComponent = 0;
                foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                    if (slot.itemID != -1 && slot.itemID == t.itemID)
                        totalOfThisComponent += slot.itemStack;

                if (totalOfThisComponent < t.itemCount)
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("Items required are not in bags", 3);
                    return;
                }
            }

            isEnchanting = true;
            curEnchantTime = 0;
            maxEnchantTime = enchantList[selectedEnchant].enchantment.enchantmentTiers[curItemEnchantTier].enchantTime;
        }

        public void DisplayEnchant(int enchantmentIndex)
        {
            if (enchantList.Count == 0)
            {
                enchantmentNameText.text = "";
                enchantTierStatBonusesText.text = "";
                enchantButton.interactable = false;
                ClearAllCostSlots();
                return;
            }
            enchantmentNameText.text = enchantList[enchantmentIndex].enchantment.displayName;
            enchantTierStatBonusesText.text = "";
            enchantButton.interactable = true;
            ClearAllCostSlots();
            selectedEnchant = enchantmentIndex;

            if (curEnchantedItemData.item != null)
            {
                
                // CHECKING THE REQUIREMENTS
                List<bool> requirementResults = new List<bool>();
                foreach (var t in enchantList[enchantmentIndex].enchantment.applyRequirements)
                {
                    switch (t.type)
                    {
                        case RPGEnchantment.ApplyRequirementType.ItemType:
                            requirementResults.Add(curEnchantedItemData.item.itemType == t.itemType);
                            break;
                        case RPGEnchantment.ApplyRequirementType.ItemRarity:
                            requirementResults.Add(curEnchantedItemData.item.rarity == t.itemRarity);
                            break;
                        case RPGEnchantment.ApplyRequirementType.ArmorType:
                            requirementResults.Add(curEnchantedItemData.item.armorType == t.armorType);
                            break;
                        case RPGEnchantment.ApplyRequirementType.ArmorSlot:
                            requirementResults.Add(curEnchantedItemData.item.equipmentSlot == t.armorSlot);
                            break;
                        case RPGEnchantment.ApplyRequirementType.WeaponType:
                            requirementResults.Add(curEnchantedItemData.item.weaponType == t.weaponType);
                            break;
                        case RPGEnchantment.ApplyRequirementType.WeaponSlot:
                            requirementResults.Add(curEnchantedItemData.item.slotType == t.weaponSlot);
                            break;
                    }
                }

                if (requirementResults.Contains(false))
                {
                    enchantButton.interactable = false;
                    return;
                }
                
                int curItemEnchantTier = -1;
                int cachedCurItemEnchantTier = -1;
                
                if (CharacterData.Instance
                    .itemsDATA[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)]
                    .enchantmentID == enchantList[enchantmentIndex].enchantment.ID)
                {
                    curItemEnchantTier = CharacterData.Instance
                        .itemsDATA[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)]
                        .enchantmentTierIndex;
                    cachedCurItemEnchantTier = curItemEnchantTier;
                    if (curItemEnchantTier == -1)
                    {
                        curItemEnchantTier = 0;
                    }
                }
                else
                {
                    curItemEnchantTier = 0;
                }
                
                int viewedTier = cachedCurItemEnchantTier != -1 ? curItemEnchantTier + 1 : curItemEnchantTier;
                
                if (curItemEnchantTier == enchantList[selectedEnchant].enchantment.enchantmentTiers.Count - 1 && cachedCurItemEnchantTier != -1)
                {
                    enchantTierStatBonusesText.text = "The maximum enchantment tier is already active";
                    enchantButton.interactable = false;
                }
                else
                {
                    foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].currencyCosts)
                    {
                        var newRecipeSlot = Instantiate(itemSlotPrefab, costSlotsParent);
                        curCostSlots.Add(newRecipeSlot);
                        var slotREF = newRecipeSlot.GetComponent<EnchantCostSlotHolder>();
                        var currencyREF = RPGBuilderUtilities.GetCurrencyFromID(t.currencyID);
                        var owned = InventoryManager.Instance.hasEnoughCurrency(t.currencyID, t.amount);
                        slotREF.InitSlot(currencyREF.icon, owned, t.amount, currencyREF, curEnchantedItemData.itemDataID);
                    }

                    foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].itemCosts)
                    {
                        var newRecipeSlot = Instantiate(itemSlotPrefab, costSlotsParent);
                        curCostSlots.Add(newRecipeSlot);
                        var slotREF = newRecipeSlot.GetComponent<EnchantCostSlotHolder>();
                        var itemREF = RPGBuilderUtilities.GetItemFromID(t.itemID);
                        var owned = RPGBuilderUtilities.getItemCount(itemREF) >= t.itemCount;
                        slotREF.InitSlot(itemREF.icon, owned, t.itemCount, itemREF, true, curEnchantedItemData.itemDataID);
                    }
                    
                    int curTierStatIndex = 1;
                    enchantTierStatBonusesText.text =
                        RPGBuilderUtilities.addLineBreak("Tier " + (viewedTier + 1) + ":");
                    foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].stats)
                    {
                        string modifierText = t.amount > 0 ? "+" : "-";
                        string percentText = "";
                        if (t.isPercent)
                        {
                            percentText = "%";
                        }

                        enchantTierStatBonusesText.text += modifierText + t.amount + percentText + " " +
                                                           RPGBuilderUtilities.GetStatFromID(t.statID).displayName;
                        if (curTierStatIndex < enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].stats.Count)
                            enchantTierStatBonusesText.text += ", ";
                        curTierStatIndex++;
                    }
                }
            }

            castBarFill.fillAmount = 0;
        }

        public void UpdateEnchantingView()
        {
            HandleSelectedEnchant();
            DisplayEnchant(selectedEnchant);
        }

        public class EnchantmentDATA
        {
            public RPGEnchantment enchantment;
            public RPGItem itemREF;
        }

        private bool curEnchantmentListContainItem(RPGItem itemREF, List<EnchantmentDATA> curEnchantmentList)
        {
            foreach (var t in curEnchantmentList)
            {
                if (t.itemREF.ID == itemREF.ID) return true;
            }
            return false;
        }

        public void InitEnchantingPanel()
        {
            enchantList.Clear();
            ClearAllEnchantSlots();

            foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
            {
                if (slot.itemID == -1) continue;
                var itemRef = RPGBuilderUtilities.GetItemFromID(slot.itemID);
                if (itemRef.itemType != "ENCHANTMENT" || itemRef.enchantmentID == -1) continue;
                RPGEnchantment enchantREF = RPGBuilderUtilities.GetEnchantmentFromID(itemRef.enchantmentID);


                if (curEnchantmentListContainItem(itemRef, enchantList)) continue;
                EnchantmentDATA newEnchantData = new EnchantmentDATA {itemREF = itemRef, enchantment = enchantREF};
                enchantList.Add(newEnchantData);
            }

            for (var index = 0; index < enchantList.Count; index++)
            {
                var t = enchantList[index];
                var newRecipeSlot = Instantiate(enchantSlotPrefab, enchantSlotParent);
                var slotREF = newRecipeSlot.GetComponent<EnchantSlotHolder>();
                curEnchantSlots.Add(slotREF);
                slotREF.InitSlot(index);
            }
            
            HandleSelectedEnchant();
            DisplayEnchant(selectedEnchant);
        }

        void HandleSelectedEnchant()
        {
            if (enchantList.Count == 0)
            {
                ClearAllCostSlots();
                enchantTierStatBonusesText.text = "";
                selectedEnchant = -1;
                return;
            }
            if (selectedEnchant > (enchantList.Count - 1) || selectedEnchant == - 1)
            {
                selectedEnchant = 0;
            }
        }

        public void Show()
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            
            InitEnchantingPanel();
            showing = true;
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void Hide()
        {
            StopCurrentEnchant();
            
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
            
            ResetEnchantedItem();
            showing = false;
        }

        private void Awake()
        {
            Hide();
        }
        
        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }
    }
}
