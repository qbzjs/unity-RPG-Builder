using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class CraftingManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static CraftingManager Instance { get; private set; }

        public int getRecipeCraftCount(RPGCraftingRecipe recipe)
        {
            var craftCount = Mathf.Infinity;
            var curRank = 0;
            curRank = RPGBuilderUtilities.getRecipeRank(recipe.ID);
            var recipeRankREF = recipe.ranks[curRank];
            foreach (var t in recipeRankREF.allComponents)
            {
                var totalOfThisComponent = InventoryManager.Instance.getTotalCountOfItemByItemID(t.componentItemID);
                totalOfThisComponent = totalOfThisComponent / t.count;
                if (totalOfThisComponent < craftCount) craftCount = totalOfThisComponent;
            }

            return (int)craftCount;
        }

        public void GenerateCraftedItem(RPGCraftingRecipe recipeToCraft)
        {

            var curRank = 0;
            curRank = RPGBuilderUtilities.getRecipeRank(recipeToCraft.ID);
            var recipeRankREF = recipeToCraft.ranks[curRank];


            foreach (var t in recipeRankREF.allComponents)
            {
                var totalOfThisComponent = 0;
                foreach (var slot in CharacterData.Instance.inventoryData.baseSlots)
                    if (slot.itemID != -1 && slot.itemID == t.componentItemID)
                        totalOfThisComponent += slot.itemStack;

                if (totalOfThisComponent < t.count)
                {
                    CraftingPanelDisplayManager.Instance.StopCurrentCraft();
                    return;
                }
            }

            List<InventoryManager.TemporaryLootItemData> allCrafts = new List<InventoryManager.TemporaryLootItemData>();
            foreach (var t in recipeRankREF.allCraftedItems)
            {
                var chance = Random.Range(0f, 100f);
                if (!(chance <= t.chance)) continue;
                InventoryManager.Instance.HandleLootList(t.craftedItemID, allCrafts, t.count);
            }

            if (RPGBuilderUtilities.GetAllSlotsNeeded(allCrafts) > InventoryManager.Instance.getEmptySlotsCount())
            {
                // Cancel craft
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                CraftingPanelDisplayManager.Instance.StopCurrentCraft();
                return;
            }

            foreach (var t in recipeRankREF.allComponents)
            {
                InventoryManager.Instance.RemoveItem(t.componentItemID, t.count, -1, -1, false);
            }

            foreach (var craft in allCrafts)
            {
                RPGBuilderUtilities.HandleItemLooting(craft.itemID, craft.count, false, true);
            }

            LevelingManager.Instance.GenerateSkillEXP(recipeToCraft.craftingSkillID, recipeRankREF.Experience);
            CraftingPanelDisplayManager.Instance.UpdateCraftingView();
        }

        private bool CheckRecipeRankingRequirements(RPGCraftingRecipe recipe, RPGTalentTree tree, int rank)
        {
            var recipeRankREF = recipe.ranks[rank];
            if (CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < recipeRankREF.unlockCost)
            {
                //NOT ENOUGH POINTS
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Not Enough Points", 3);
                return false;
            }

            List<bool> reqResults = new List<bool>();
            foreach (var t in RPGBuilderUtilities.getNodeRequirements(tree, recipe))
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
                    case RequirementsManager.RequirementType.pointSpent:
                        intValue1 = RPGBuilderUtilities.getTreePointSpentAmount(tree);
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

        private bool CheckRecipeRankingDown(RPGCraftingRecipe recipe, RPGTalentTree tree)
        {
            foreach (var t in tree.nodeList)
            foreach (var t1 in t.requirements)
            {
                if (t1.requirementType != RequirementsManager.RequirementType.recipeKnown ||
                    t1.craftingRecipeRequiredID != recipe.ID || !RPGBuilderUtilities.isRecipeKnown(t.recipeID) ||
                    RPGBuilderUtilities.getRecipeRank(recipe.ID) != 0) continue;
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Cannot unlearn a recipe that is required for others", 3);
                return false;
            }

            return true;
        }

        public void RankUpRecipe(RPGCraftingRecipe recipe, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.recipesData)
            {
                if (t.ID != recipe.ID) continue;
                if (t.rank >= recipe.ranks.Count) continue;
                if (!CheckRecipeRankingRequirements(recipe, tree, t.rank)) continue;
                var recipeRankREF = recipe.ranks[t.rank];
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID, recipeRankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, recipeRankREF.unlockCost);
                t.rank++;
                t.known = true;

                TreesDisplayManager.Instance.InitTree(tree);

                if (t.rank == 1)
                    CharacterEventsManager.Instance.RecipeLearned(recipe);
            }
        }

        public void RankDownRecipe(RPGCraftingRecipe ab, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.recipesData)
            {
                if (t.ID != ab.ID) continue;
                if (t.rank <= 0) continue;
                if(ab.learnedByDefault && t.rank == 1) continue;
                if (!CheckRecipeRankingDown(ab, tree)) continue;
                var recipeRankREF = ab.ranks[t.rank - 1];
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID, recipeRankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, recipeRankREF.unlockCost);
                t.rank--;

                if (t.rank == 0) t.known = false;
                TreesDisplayManager.Instance.InitTree(tree);
            }
        }

        public void HandleStartingRecipes()
        {
            foreach (var recipe in CharacterData.Instance.recipesData)
            {
                RPGCraftingRecipe recipeREF = RPGBuilderUtilities.GetCraftingRecipeFromID(recipe.ID);
                if (!recipeREF.learnedByDefault) continue;
                RPGBuilderUtilities.setRecipeData(recipe.ID, 1, true);
            }
        }

    }
}