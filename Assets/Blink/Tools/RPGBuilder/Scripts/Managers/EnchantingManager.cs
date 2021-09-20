using System.Linq;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class EnchantingManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static EnchantingManager Instance { get; private set; }

        public void EnchantItem(int itemDataID, int curTier, RPGEnchantment enchantment)
        {
            CharacterData.ItemDATA itemData = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            if (itemData == null) return;
            int upcomingTier = -1;
            if (itemData.enchantmentID == enchantment.ID)
            {
                upcomingTier = curTier + 1;
            }
            else
            {
                upcomingTier = 0;
            }

            // if enchant is consumed, check if we still own at leadst 1 of its item first
            if (EnchantingPanelDisplayManager.Instance
                .enchantList[EnchantingPanelDisplayManager.Instance.selectedEnchant].itemREF.isEnchantmentConsumed)
            {
                if (RPGBuilderUtilities.getItemCount(EnchantingPanelDisplayManager.Instance
                    .enchantList[EnchantingPanelDisplayManager.Instance.selectedEnchant].itemREF) > 0)
                {
                    InventoryManager.Instance.RemoveItem(EnchantingPanelDisplayManager.Instance
                            .enchantList[EnchantingPanelDisplayManager.Instance.selectedEnchant].itemREF.ID, 1, -1, -1,
                        false);

                }
                else
                {
                    EnchantingPanelDisplayManager.Instance.StopCurrentEnchant();
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("The enchantment item is not owned anymore", 3);
                    return;
                }
            }

            if (enchantment.enchantmentTiers[upcomingTier].currencyCosts.Any(t =>
                !InventoryManager.Instance.hasEnoughCurrency(t.currencyID, t.amount)))
            {
                EnchantingPanelDisplayManager.Instance.StopCurrentEnchant();
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Not enough currency", 3);
                return;
            }

            foreach (var itemCost in enchantment.enchantmentTiers[upcomingTier].itemCosts)
            {
                int totalOfThisComponent = 0;
                foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                {
                    if(slot.itemID == -1 || slot.itemID != itemCost.itemID) continue;
                    totalOfThisComponent += slot.itemStack;
                }

                if (totalOfThisComponent >= itemCost.itemCount) continue;
                EnchantingPanelDisplayManager.Instance.StopCurrentEnchant();
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Items required are not in bags", 3);
                return;
            }

            foreach (var t in enchantment.enchantmentTiers[upcomingTier].itemCosts)
                InventoryManager.Instance.RemoveItem(t.itemID, t.itemCount, -1, -1, false);
            foreach (var t in enchantment.enchantmentTiers[upcomingTier].currencyCosts)
                InventoryManager.Instance.RemoveCurrency(t.currencyID, t.amount);

            var success = Random.Range(0f, 100f);
            if (!(success <= enchantment.enchantmentTiers[upcomingTier].successRate))
            {
                EnchantingPanelDisplayManager.Instance.StopCurrentEnchant();
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("The enchantment failed", 3);
                return;
            }

            if (curTier == -1)
            {
                curTier = 0;
            }

            if (enchantment.enchantmentTiers[curTier].skillID != -1)
            {
                LevelingManager.Instance.AddSkillXP(enchantment.enchantmentTiers[curTier].skillID,
                    enchantment.enchantmentTiers[curTier].skillXPAmount);
            }

            int itemDataIndex = RPGBuilderUtilities.GetItemDataIndexFromDataID(itemDataID);
            if (itemDataIndex != -1)
            {
                CharacterData.Instance.itemsDATA[itemDataIndex].enchantmentID = enchantment.ID;
                CharacterData.Instance.itemsDATA[itemDataIndex].enchantmentTierIndex = upcomingTier;
            }


            EnchantingPanelDisplayManager.Instance.UpdateEnchantingView();

        }

        public void ApplyEnchantParticle(GameObject meshManager, GameObject target)
        {
            if (target == null) return;
            GameObject meshManagerGO = Instantiate(meshManager, target.transform);
            meshManagerGO.transform.position = Vector3.zero;
            meshManagerGO.transform.localPosition = Vector3.zero;

            MeshParticleManager meshManagerRef = meshManagerGO.GetComponent<MeshParticleManager>();
            if (meshManagerRef != null)
            {
                meshManagerRef.Init(target);
            }
        }
    }
}
