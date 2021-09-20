using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class TreeNodeHolder : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
    {
        public RectTransform rect;
        public Image border, rankBorder, costBorder;
        public Image icon, background;
        public TextMeshProUGUI curRankText, costText;
        public CanvasGroup thisCG, costCG;
        private RPGAbility thisAb;
        private RPGCraftingRecipe thisRecipe;
        private RPGResourceNode thisResourceNode;
        private RPGBonus thisBonus;
        private RPGTalentTree thisTree;
        private GameObject curDraggedAbility;
        private RPGTalentTree.TalentTreeNodeType curTalentTreeNodeType;

        public bool used;

        public void Init(RPGTalentTree tree, TreesDisplayManager.TreeNodeSlotDATA nodeDATA)
        {
            used = true;
            curTalentTreeNodeType = nodeDATA.type;

            thisTree = tree;

            enableAllElements();
            var unlockCost = 0;
            var rank = -1;
            var maxRank = -1;
            bool isKnown = false;
            switch (nodeDATA.type)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    icon.sprite = nodeDATA.ability.icon;
                    background.sprite = null;
                    thisAb = nodeDATA.ability;
                    rank = RPGBuilderUtilities.getAbilityRank(nodeDATA.ability.ID);
                    isKnown = RPGBuilderUtilities.isAbilityKnown(nodeDATA.ability.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF = nodeDATA.ability.ranks[rank];
                    unlockCost = rankREF.unlockCost;
                    maxRank = nodeDATA.ability.ranks.Count;

                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    icon.sprite = nodeDATA.recipe.icon;
                    thisRecipe = nodeDATA.recipe;
                    rank = RPGBuilderUtilities.getRecipeRank(nodeDATA.recipe.ID);
                    isKnown = RPGBuilderUtilities.isRecipeKnown(nodeDATA.recipe.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF2 = nodeDATA.recipe.ranks[rank];
                    unlockCost = rankREF2.unlockCost;
                    maxRank = nodeDATA.recipe.ranks.Count;
                    background.sprite = RPGBuilderUtilities.getItemRaritySprite(RPGBuilderUtilities.GetItemFromID(rankREF2.allCraftedItems[0].craftedItemID).rarity);
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    icon.sprite = nodeDATA.resourceNode.icon;
                    thisResourceNode = nodeDATA.resourceNode;
                    rank = RPGBuilderUtilities.getResourceNodeRank(nodeDATA.resourceNode.ID);
                    isKnown = RPGBuilderUtilities.isResourceNodeKnown(nodeDATA.resourceNode.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF3 = nodeDATA.resourceNode.ranks[rank];
                    unlockCost = rankREF3.unlockCost;
                    maxRank = nodeDATA.resourceNode.ranks.Count;
                    background.sprite = RPGBuilderUtilities.getItemRaritySprite(RPGBuilderUtilities
                        .GetItemFromID(RPGBuilderUtilities.GetLootTableFromID(rankREF3.lootTableID).lootItems[0].itemID).rarity);
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    icon.sprite = nodeDATA.bonus.icon;
                    background.sprite = null;
                    thisBonus = nodeDATA.bonus;
                    rank = RPGBuilderUtilities.getBonusRank(nodeDATA.bonus.ID);
                    isKnown = RPGBuilderUtilities.isBonusKnown(nodeDATA.bonus.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF4 = nodeDATA.bonus.ranks[rank];
                    unlockCost = rankREF4.unlockCost;
                    maxRank = nodeDATA.bonus.ranks.Count;
                    break;
            }

            handleBorders(isKnown);
            int displayRank = rank;
            if (isKnown)
            {
                displayRank++;
            }
            handleRank(displayRank == maxRank,unlockCost,CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < unlockCost, rank);

            setCurRankText(displayRank + " / " + maxRank);
        }

        private void handleRank(bool maxRank, int cost, bool enoughPoints, int rank)
        {
            if (maxRank)
            {
                RPGBuilderUtilities.DisableCG(costCG);
                if (TreesDisplayManager.Instance.useNodeColor) border.color = TreesDisplayManager.Instance.MaxRankColor;
                if (TreesDisplayManager.Instance.useNodeImage)
                {
                    border.sprite = isPassive() ? TreesDisplayManager.Instance.MaxRankImagePassive : TreesDisplayManager.Instance.MaxRankImage;
                }
                rankBorder.color = TreesDisplayManager.Instance.MaxRankColor;
            }
            else
            {
                costText.text = cost.ToString();
                if (enoughPoints)
                {
                    //NOT ENOUGH POINTS
                    if (TreesDisplayManager.Instance.useNodeColor) costBorder.color = TreesDisplayManager.Instance.NotUnlockedColor;
                    costText.color = TreesDisplayManager.Instance.NotUnlockedColor;
                }
                else
                {
                    if(TreesDisplayManager.Instance.useNodeColor) costBorder.color = TreesDisplayManager.Instance.UnlockedColor;
                    costText.color = TreesDisplayManager.Instance.UnlockedColor;
                }
            }
        }

        private void handleBorders(bool known)
        {
            if (known)
            {
                if (TreesDisplayManager.Instance.useNodeColor)
                {
                    border.color = TreesDisplayManager.Instance.UnlockedColor;
                    rankBorder.color = TreesDisplayManager.Instance.UnlockedColor;
                }
                if (TreesDisplayManager.Instance.useNodeImage)
                {
                    border.sprite = isPassive() ? TreesDisplayManager.Instance.UnlockedImagePassive : TreesDisplayManager.Instance.UnlockedImage;
                }
            }
            else
            {
                if (TreesDisplayManager.Instance.useNodeColor)
                {
                    border.color = TreesDisplayManager.Instance.NotUnlockedColor;
                    rankBorder.color = TreesDisplayManager.Instance.NotUnlockedColor;
                }
                if (TreesDisplayManager.Instance.useNodeImage)
                {
                    border.sprite = isPassive() ? TreesDisplayManager.Instance.NotUnlockedImagePassive : TreesDisplayManager.Instance.NotUnlockedImage;
                }
            }
        }

        private bool isPassive()
        {
            return curTalentTreeNodeType == RPGTalentTree.TalentTreeNodeType.bonus;
        }

        private void enableAllElements()
        {
            RPGBuilderUtilities.EnableCG(costCG);
        }

        private void setCurRankText(string text)
        {
            curRankText.text = text;
        }

        public void InitHide()
        {
            used = false;
            RPGBuilderUtilities.DisableCG(thisCG);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                RankDown();
            }
            else
            {
                RankUp();
            }
        }
        
        public void RankUp()
        {
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    AbilityManager.Instance.RankUpAbility(thisAb, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    CraftingManager.Instance.RankUpRecipe(thisRecipe, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    GatheringManager.Instance.RankUpResourceNode(thisResourceNode, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    BonusManager.Instance.RankUpBonus(thisBonus, thisTree);
                    break;
            }
        }

        public void RankDown()
        {
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    AbilityManager.Instance.RankDownAbility(thisAb, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    CraftingManager.Instance.RankDownRecipe(thisRecipe, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    GatheringManager.Instance.RankDownResourceNode(thisResourceNode, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    BonusManager.Instance.RankDownBonus(thisBonus, thisTree);
                    break;
            }
        }

        public void ShowTooltip()
        {
            var curRank = 0;
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    AbilityTooltip.Instance.Show(thisAb);
                    TreesDisplayManager.Instance.ShowAbilityRequirements(thisAb, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    curRank = RPGBuilderUtilities.getRecipeRank(thisRecipe.ID);
                    if (curRank == -1) curRank = 0;
                    var rankREF = thisRecipe.ranks[curRank];
                    ItemTooltip.Instance.Show(rankREF.allCraftedItems[0].craftedItemID, -1, true);
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    curRank = RPGBuilderUtilities.getResourceNodeRank(thisResourceNode.ID);
                    if (curRank == -1) curRank = 0;
                    var rankREF2 = thisResourceNode.ranks[curRank];
                    ItemTooltip.Instance.Show(RPGBuilderUtilities
                        .GetItemFromID(RPGBuilderUtilities.GetLootTableFromID(rankREF2.lootTableID).lootItems[0].itemID)
                        .ID, -1, true);
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    AbilityTooltip.Instance.ShowBonus(thisBonus);
                    TreesDisplayManager.Instance.ShowBonusRequirements(thisBonus, thisTree);
                    break;
            }
        }

        public void HideTooltip()
        {
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    AbilityTooltip.Instance.Hide();
                    TreesDisplayManager.Instance.HideRequirements();
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    ItemTooltip.Instance.Hide();
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    ItemTooltip.Instance.Hide();
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    AbilityTooltip.Instance.Hide();
                    TreesDisplayManager.Instance.HideRequirements();
                    break;
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (curTalentTreeNodeType != RPGTalentTree.TalentTreeNodeType.ability) return;
            if(curDraggedAbility!=null) Destroy(curDraggedAbility);
            if (!RPGBuilderUtilities.isAbilityKnown(thisAb.ID)) return;
            curDraggedAbility = Instantiate(TreesDisplayManager.Instance.draggedNodeImage, transform.position,
                Quaternion.identity);
            curDraggedAbility.transform.SetParent(TreesDisplayManager.Instance.draggedNodeParent);
            curDraggedAbility.GetComponent<Image>().sprite = thisAb.icon;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (curTalentTreeNodeType != RPGTalentTree.TalentTreeNodeType.ability) return;
            if (curDraggedAbility != null)
                curDraggedAbility.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (curTalentTreeNodeType != RPGTalentTree.TalentTreeNodeType.ability) return;
            if (curDraggedAbility == null) return;
            for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                    Input.mousePosition)) continue;

                if (ActionBarManager.Instance.actionBarSlots[i].acceptAbilities)
                {
                    ActionBarManager.Instance.SetAbilityToSlot(thisAb, i);
                }
                else
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This action bar slot do not accept abilities",
                        3);
                }
            }

            Destroy(curDraggedAbility);
        }

        public void OnDrop(PointerEventData eventData)
        {
        }
    }
}