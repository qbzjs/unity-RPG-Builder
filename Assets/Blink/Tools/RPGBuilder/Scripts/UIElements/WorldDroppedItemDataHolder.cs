using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class WorldDroppedItemDataHolder : MonoBehaviour
    {
        public Image BackgroundImage, BackgroundBorder;
        public TextMeshProUGUI NameText;


        public float defaultWaitBeforeStart;

        public float InterpolateSpeed;

        private Coroutine hpfilldelayCoroutine;

        private GameObject thisItemGO;

        private RPGItem thisItem;
        private WorldDroppedItem thisWorldDroppedItemREF;
        public GameObject GetThisItemGO()
        {
            return thisItemGO;
        }

        public void InitializeThisNameplate(GameObject itemGO, RPGItem itemREF)
        {
            thisItemGO = itemGO;

            BackgroundBorder.color = RPGBuilderUtilities.getItemRarityColor(itemREF.rarity);
            NameText.text = itemREF.displayName;
            NameText.color = RPGBuilderUtilities.getItemRarityColor(itemREF.rarity);
            thisItem = itemREF;
            thisWorldDroppedItemREF = thisItemGO.GetComponent<WorldDroppedItem>();
        }

        public void LootItem()
        {
            HideTooltip();
            InventoryManager.Instance.LootWorldDroppedItem(thisWorldDroppedItemREF);
        }

        public void ShowTooltip()
        {
            ItemTooltip.Instance.Show(thisItem.ID, InventoryManager.Instance.getWorldDroppedItemDataID(thisWorldDroppedItemREF), true);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void ResetThisNameplate()
        {
            ScreenSpaceWorldDroppedItems.Instance.ResetThisNP(thisItemGO);
        }


        
    }
}