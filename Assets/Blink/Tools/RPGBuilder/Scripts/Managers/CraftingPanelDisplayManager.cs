using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class CraftingPanelDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;

        public List<CraftingRecipeSlotHolder> curRecipeSlots = new List<CraftingRecipeSlotHolder>();
        public List<GameObject> curComponentsSlots = new List<GameObject>();
        public List<GameObject> curItemsCraftedSlots = new List<GameObject>();
        public Transform recipeSlotsParent, requiredComponentSlotsParent, craftedItemSlotsParent;
        public GameObject recipeSlotPrefab, itemSlotPrefab;

        public TextMeshProUGUI craftingHeaderText;

        public Image castBarFill;

        private RPGCraftingRecipe selectedRecipe;
        private RPGSkill curSkill;
        private RPGCraftingStation curStation;

        private bool isCrafting;
        private float curCraftTime, maxCraftTime;

        private CraftingStation currentStationNode;
        private bool isShowing;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static CraftingPanelDisplayManager Instance { get; private set; }

        private void FixedUpdate()
        {
            if (!isCrafting) return;
            curCraftTime += Time.deltaTime;
            castBarFill.fillAmount = curCraftTime / maxCraftTime;

            if (!(curCraftTime >= maxCraftTime)) return;
            CraftingManager.Instance.GenerateCraftedItem(selectedRecipe);
            isCrafting = false;
            curCraftTime = 0;
            maxCraftTime = 0;
        }

        private void ClearAllRecipeSlots()
        {
            foreach (var t in curRecipeSlots) Destroy(t.gameObject);

            curRecipeSlots.Clear();
        }

        private void ClearAllComponentSlots()
        {
            foreach (var t in curComponentsSlots) Destroy(t);

            curComponentsSlots.Clear();
        }

        private void ClearAllItemsCraftedSlots()
        {
            foreach (var t in curItemsCraftedSlots) Destroy(t);

            curItemsCraftedSlots.Clear();
        }

        public void StopCurrentCraft()
        {
            isCrafting = false;
            curCraftTime = 0;
            maxCraftTime = 0;
            castBarFill.fillAmount = 0;
        }

        public void ClickCraftRecipe()
        {
            if (isCrafting || selectedRecipe == null) return;
            if (RPGBuilderUtilities.isInventoryFull())
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                return;
            }
            var curRank = RPGBuilderUtilities.getRecipeRank(selectedRecipe.ID);

            var rankREF = selectedRecipe.ranks[curRank];
            foreach (var t in rankREF.allComponents)
            {
                var totalOfThisComponent = 0;
                foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                    if (slot.itemID != -1 && slot.itemID == t.componentItemID)
                        totalOfThisComponent += slot.itemStack;

                if (totalOfThisComponent < t.count) return;
            }

            isCrafting = true;
            curCraftTime = 0;
            maxCraftTime = rankREF.craftTime;
        }

        public void DisplayRecipe(RPGCraftingRecipe recipe)
        {
            if (isCrafting) return;
            
            ClearAllComponentSlots();
            ClearAllItemsCraftedSlots();

            selectedRecipe = recipe;

            var curRank = RPGBuilderUtilities.getRecipeRank(recipe.ID);
            var rankREF = selectedRecipe.ranks[curRank];

            foreach (var t in rankREF.allComponents)
            {
                var newRecipeSlot = Instantiate(itemSlotPrefab, requiredComponentSlotsParent);
                curComponentsSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<CraftingItemSlotHolder>();
                var itemREF = RPGBuilderUtilities.GetItemFromID(t.componentItemID);
                var owned = RPGBuilderUtilities.getItemCount(itemREF) >= t.count;
                slotREF.InitSlot(itemREF.icon, owned, t.count, itemREF);
            }

            foreach (var t in rankREF.allCraftedItems)
            {
                var newRecipeSlot = Instantiate(itemSlotPrefab, craftedItemSlotsParent);
                curItemsCraftedSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<CraftingItemSlotHolder>();
                var itemREF = RPGBuilderUtilities.GetItemFromID(t.craftedItemID);
                slotREF.InitSlot(itemREF.icon, true, t.count, itemREF);
            }

            castBarFill.fillAmount = 0;
        }

        public void UpdateCraftingView()
        {
            foreach (var t in curRecipeSlots)
            {
                var craftCount = CraftingManager.Instance.getRecipeCraftCount(t.thisRecipe);
                var statusText = "";

                statusText = craftCount == 0 ? "<color=red> Missing Resources" : "<color=green> Craftable";

                t.UpdateState(statusText, craftCount);
            }

            DisplayRecipe(selectedRecipe);
        }

        private void InitCraftingPanel(RPGCraftingStation station)
        {
            ClearAllRecipeSlots();
            curStation = station;
            craftingHeaderText.text = station.displayName;
            var recipeList = new List<RPGCraftingRecipe>();
            foreach (var skillRef in station.craftSkills.Select(t1 =>
                RPGBuilderUtilities.GetSkillFromID(t1.craftSkillID)))
            {
                curSkill = skillRef;
                var tempRecipeList = RPGBuilderUtilities.getRecipeListOfSkill(skillRef, station);

                foreach (var t in tempRecipeList)
                {
                    var newRecipeSlot = Instantiate(recipeSlotPrefab, recipeSlotsParent);
                    var slotREF = newRecipeSlot.GetComponent<CraftingRecipeSlotHolder>();
                    curRecipeSlots.Add(slotREF);
                    slotREF.InitSlot(t);

                    var craftCount = CraftingManager.Instance.getRecipeCraftCount(t);
                    var statusText = "";

                    statusText = craftCount == 0 ? "<color=red> Missing Resources" : "<color=green> Craftable";

                    slotREF.UpdateState(statusText, craftCount);
                    recipeList.Add(t);
                }
            }

            if (recipeList.Count > 0)
                DisplayRecipe(recipeList[0]);
            else
            {
                ClearAllComponentSlots();
                ClearAllItemsCraftedSlots();
            }
        }

        public void Show()
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(true);
        }

        public void Show(CraftingStation station)
        {
            isShowing = true;
            currentStationNode = station;
            Show();
            InitCraftingPanel(station.station);
        }

        public void Hide()
        {
            isShowing = false;
            StopCurrentCraft();
            
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!isShowing || currentStationNode == null) return;
            if(Vector3.Distance(currentStationNode.transform.position, CombatManager.playerCombatNode.transform.position) > 4) Hide();
        }
    }
}