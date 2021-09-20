using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class TreesDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG, requirementsCG, errorMessageCG;
        public TextMeshProUGUI TreeNameText, requirementsText, errorMessageText, availablePointsText;
        private bool showing;

        public bool useNodeColor;
        public bool useNodeImage = true;
        public Color UnlockedColor, NotUnlockedColor, NotUnlockableColor, MaxRankColor;
        public Sprite UnlockedImage, NotUnlockedImage, NotUnlockableImage, MaxRankImage, UnlockedImagePassive, NotUnlockedImagePassive, NotUnlockableImagePassive, MaxRankImagePassive;

        public GameObject NodeSlotPrefabActive, NodeSlotPrefabPassive;

        public Transform draggedNodeParent;
        public GameObject draggedNodeImage;

        public GameObject TierSlotPrefab;
        public Transform TierSlotsParent;
        private readonly List<GameObject> curTreesTiersSlots = new List<GameObject>();

        [Serializable]
        public class TreeNodeSlotDATA
        {
            public RPGTalentTree.TalentTreeNodeType type;
            public RPGAbility ability;
            public RPGCraftingRecipe recipe;
            public RPGResourceNode resourceNode;
            public RPGBonus bonus;
        }

        [Serializable]
        public class TREE_UI_DATA
        {
            public GridLayoutGroup GridLayoutRef;
            public List<TreeNodeSlotDATA> slotsDATA = new List<TreeNodeSlotDATA>();
            public List<TreeNodeHolder> nodesREF = new List<TreeNodeHolder>();
        }

        public List<TREE_UI_DATA> treeUIData = new List<TREE_UI_DATA>();
        private readonly List<GameObject> curNodeSlots = new List<GameObject>();

        public GameObject TreeNodeLinePrefab;

        public float nodeXStartOffset = 0.35f;
        public float nodeDistanceOffset = 0.95f;
        public float nodeDistanceOffsetBonusPerTier = 0.25f;
        public float nodeOffsetWhenAbove = 0.25f;
        public float nodeDistanceOffsetWhenAbove = 0.95f;
        public float nodeDistanceOffsetBonusPerTierWhenAbove = 0.25f;

        public static TreesDisplayManager Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public enum previousMenuType
        {
            charPanel,
            skillBook,
            weaponTemplates
        }

        public previousMenuType curPreviousMenu;


        public void ShowAbilityRequirements(RPGAbility ab, RPGTalentTree tree)
        {
            var curRank = RPGBuilderUtilities.getAbilityRank(ab.ID);
            if (curRank == -1) curRank = 0;
            var rankREF = ab.ranks[curRank];

            var requirements = "";
            if (rankREF.unlockCost > 0)
            {
                var color = "";
                color = CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rankREF.unlockCost ? "<color=red>" : "<color=green>";
                requirements += color + "Requires " + rankREF.unlockCost + " points" + "\n";
            }

            var abIndex = RPGBuilderUtilities.getAbilityIndexInTree(ab, tree);
            var treeIndex = RPGBuilderUtilities.getTreeIndex(tree);

            RequirementHandling(tree, abIndex, treeIndex, requirements);
        }

        public void ShowBonusRequirements(RPGBonus bonus, RPGTalentTree tree)
        {
            var curRank = RPGBuilderUtilities.getBonusRank(bonus.ID);
            if (curRank == -1) curRank = 0;
            var rankREF = bonus.ranks[curRank];

            var requirements = "";
            if (rankREF.unlockCost > 0)
            {
                var color = "";
                color = CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rankREF.unlockCost ? "<color=red>" : "<color=green>";
                requirements += color + "Requires " + rankREF.unlockCost + " points" + "\n";
            }

            var abIndex = RPGBuilderUtilities.getBonusIndexInTree(bonus, tree);
            var treeIndex = RPGBuilderUtilities.getTreeIndex(tree);

            RequirementHandling(tree, abIndex, treeIndex, requirements);
        }

        void RequirementHandling(RPGTalentTree tree, int abIndex, int treeIndex, string requirements)
        {
            foreach (var t in tree.nodeList[abIndex].requirements)
            {
                var color = "";
                switch (t.requirementType)
                {
                    case RequirementsManager.RequirementType.pointSpent:
                        if (CharacterData.Instance.talentTrees[treeIndex].pointsSpent <
                            t.pointSpentValue)
                            color = "<color=red>";
                        else
                            color = "<color=green>";
                        requirements += color + "Requires " + t.pointSpentValue +
                                        " points spent in this tree.\n";
                        break;
                    case RequirementsManager.RequirementType.classLevel:
                        if (CharacterData.Instance.classDATA.currentClassLevel <
                            t.classLevelValue)
                            color = "<color=red>";
                        else
                            color = "<color=green>";
                        requirements += color + "Requires class level to be " +
                                        t.classLevelValue + " or above.\n";
                        break;
                    case RequirementsManager.RequirementType.abilityKnown:
                        color = !CharacterData.Instance
                            .getAbilityData(t.abilityRequiredID).known ? "<color=red>" : "<color=green>";
                        requirements += color + "Requires the " +
                                        RPGBuilderUtilities
                                            .GetAbilityFromID(t.abilityRequiredID)
                                            .displayName + " ability to be known.\n";
                        break;
                    case RequirementsManager.RequirementType.itemOwned:
                        if (!InventoryManager.Instance.isItemOwned(t.itemRequiredID,
                            t.itemRequiredCount))
                            color = "<color=red>";
                        else
                            color = "<color=green>";
                        requirements += color + "Requires to own at least " +
                                        t.itemRequiredCount + " " +
                                        RPGBuilderUtilities
                                            .GetAbilityFromID(t.itemRequiredID).displayName +
                                        ".\n";
                        break;
                }
            }

            if (requirements != "")
            {
                RPGBuilderUtilities.EnableCG(requirementsCG);
                requirementsText.text = requirements;
            }
            else
            {
                RPGBuilderUtilities.DisableCG(requirementsCG);
            }
        }
        
        public void HideRequirements()
        {
            RPGBuilderUtilities.DisableCG(requirementsCG);
        }

        private void ClearAllTiersData()
        {
            foreach (var t in curNodeSlots)
                Destroy(t);

            curNodeSlots.Clear();

            foreach (var t in curTreesTiersSlots)
                Destroy(t);

            curTreesTiersSlots.Clear();
            treeUIData.Clear();
        }


        public void GoToPreviousMenu()
        {
            switch (curPreviousMenu)
            {
                case previousMenuType.charPanel:
                    Hide();
                    CharacterPanelDisplayManager.Instance.Show();
                    break;
                case previousMenuType.skillBook:
                    Hide();
                    SkillBookDisplayManager.Instance.Show();
                    break;
                case previousMenuType.weaponTemplates:
                    Hide();
                    WeaponTemplatesDisplayManager.Instance.Show();
                    break;
            }
        }

        public void InitTree(RPGTalentTree tree)
        {
            Show();
            ClearAllTiersData();

            TreeNameText.text = tree.displayName;

            for (var i = 0; i < tree.TiersAmount; i++)
            {
                var newTierSlot = Instantiate(TierSlotPrefab, TierSlotsParent);
                var newTierData = new TREE_UI_DATA();
                newTierData.GridLayoutRef = newTierSlot.GetComponent<GridLayoutGroup>();
                for (var x = 0; x < RPGBuilderEssentials.Instance.combatSettings.talentTreesNodePerTierCount; x++)
                {
                    newTierData.slotsDATA.Add(new TreeNodeSlotDATA());
                    foreach (var t in tree.nodeList)
                    {
                        if (t.Tier != i + 1) continue;
                        if (t.Row == x + 1)
                            switch (t.nodeType)
                            {
                                case RPGTalentTree.TalentTreeNodeType.ability:
                                    newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.ability;
                                    newTierData.slotsDATA[x].ability =
                                        RPGBuilderUtilities.GetAbilityFromID(t.abilityID);
                                    break;
                                case RPGTalentTree.TalentTreeNodeType.recipe:
                                    newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.recipe;
                                    newTierData.slotsDATA[x].recipe =
                                        RPGBuilderUtilities.GetCraftingRecipeFromID(t.recipeID);
                                    break;
                                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                                    newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.resourceNode;
                                    newTierData.slotsDATA[x].resourceNode =
                                        RPGBuilderUtilities.GetResourceNodeFromID(t.resourceNodeID);
                                    break;
                                case RPGTalentTree.TalentTreeNodeType.bonus:
                                    newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.bonus;
                                    newTierData.slotsDATA[x].bonus =
                                        RPGBuilderUtilities.GetBonusFromID(t.bonusID);
                                    break;
                            }
                    }
                }

                treeUIData.Add(newTierData);
                curTreesTiersSlots.Add(newTierSlot);
            }

            foreach (var t in treeUIData)
            foreach (var t1 in t.slotsDATA)
            {
                var newAb = Instantiate(t1.type == RPGTalentTree.TalentTreeNodeType.bonus ? NodeSlotPrefabPassive : NodeSlotPrefabActive, t.GridLayoutRef.transform);
                var holder = newAb.GetComponent<TreeNodeHolder>();
                t.nodesREF.Add(holder);
                if ((t1.type == RPGTalentTree.TalentTreeNodeType.ability &&
                    t1.ability != null)
                    || (t1.type == RPGTalentTree.TalentTreeNodeType.recipe &&
                        t1.recipe != null)
                        || (t1.type == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                            t1.resourceNode != null)
                            || (t1.type == RPGTalentTree.TalentTreeNodeType.bonus &&
                                t1.bonus != null)
                )
                    holder.Init(tree, t1);
                else
                    holder.InitHide();

                curNodeSlots.Add(newAb);
            }

            availablePointsText.text =
                "Points: " + CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID);

            InitTalentTreeLines(tree);
        }

        private void InitTalentTreeLines(RPGTalentTree tree)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.nodesREF.Count; x++)
                    if (t.nodesREF[x].used)
                        InitTalentTreeNodeLines(tree, t.slotsDATA[x], t.nodesREF[x].transform);
        }

        private bool checkReqMet(RPGTalentTree.Node_DATA nodeDATA)
        {
            switch (nodeDATA.nodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    return RPGBuilderUtilities.isAbilityKnown(nodeDATA.abilityID);
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    return RPGBuilderUtilities.isRecipeKnown(nodeDATA.recipeID);
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    return RPGBuilderUtilities.isResourceNodeKnown(nodeDATA.resourceNodeID);
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    return RPGBuilderUtilities.isBonusKnown(nodeDATA.bonusID);
                default:
                    return false;
            }
        }

        private void InitTalentTreeNodeLines(RPGTalentTree thisTree, TreeNodeSlotDATA nodeData, Transform nodeTransform)
        {
            var isabilityNotNull = nodeData?.ability != null;
            var isrecipeNotNull = nodeData?.recipe != null;
            var isresourceNodeNotNull = nodeData?.resourceNode != null;
            var isbonusNotNull = nodeData?.bonus != null;
            foreach (var t in thisTree.nodeList)
            foreach (var t1 in t.requirements)
            {
                TreeNodeHolder otherNodeREF;
                switch (t1.requirementType)
                {
                    case RequirementsManager.RequirementType.abilityKnown when  isabilityNotNull && t1.abilityRequiredID == nodeData.ability.ID:
                    {
                        var isRequirementMet = checkReqMet(t);

                        otherNodeREF = getAbilityNodeREF(RPGBuilderUtilities.GetAbilityFromID(t.abilityID));

                        if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, isRequirementMet);
                        break;
                    }
                    case RequirementsManager.RequirementType.recipeKnown when  isrecipeNotNull && t1.craftingRecipeRequiredID == nodeData.recipe.ID:
                    {
                        var isRequirementMet = checkReqMet(t);
                        otherNodeREF =
                            getCraftingRecipeNodeREF(
                                RPGBuilderUtilities.GetCraftingRecipeFromID(t.recipeID));

                        if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, isRequirementMet);
                        break;
                    }
                    case RequirementsManager.RequirementType.resourceNodeKnown when isresourceNodeNotNull && t1.resourceNodeRequiredID == nodeData.resourceNode.ID:
                    {
                        var isRequirementMet = checkReqMet(t);
                        otherNodeREF =
                            getResourceNodeNodeREF(
                                RPGBuilderUtilities.GetResourceNodeFromID(t.resourceNodeID));

                        if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, isRequirementMet);
                        break;
                    }
                    case RequirementsManager.RequirementType.bonusKnown when isbonusNotNull && t1.bonusRequiredID == nodeData.bonus.ID:
                    {
                        var isRequirementMet = checkReqMet(t);
                        otherNodeREF = getBonusNodeREF(RPGBuilderUtilities.GetBonusFromID(t.bonusID));

                        if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, isRequirementMet);
                        break;
                    }
                }
            }
        }

        private void GenerateLine(RPGTalentTree.Node_DATA TreeNodeData, TreeNodeSlotDATA NodeDATA, Transform trs,
            bool reqMet)
        {
            var otherTierSlot = getNodeTierSlotIndex(TreeNodeData);
            var thisTierSlot = getNodeTierSlotIndex(NodeDATA);
            var otherAbTier = otherTierSlot[0];
            var otherAbSlot = otherTierSlot[1];
            var thisAbTier = thisTierSlot[0];
            var thisAbSlot = thisTierSlot[1];


            var tierDifference = getTierDifference(otherAbTier - thisAbTier);


            var slotDifference = 0;
            var otherNodeIsLeft = false;
            if (otherAbSlot != thisAbSlot)
            {
                slotDifference = otherAbSlot - thisAbSlot;
                if (slotDifference < 0)
                {
                    slotDifference = Mathf.Abs(slotDifference);
                    otherNodeIsLeft = true;
                }
                else
                {
                    slotDifference = -slotDifference;
                }
            }
            else
            {
                slotDifference = 0;
            }

            var newTreeNodeLine = Instantiate(TreeNodeLinePrefab, trs);
            var lineREF = newTreeNodeLine.GetComponent<UILineRenderer>();

            HandleLine(tierDifference, slotDifference, lineREF, thisAbTier, otherAbTier, otherNodeIsLeft);

            lineREF.color = reqMet ? MaxRankColor : NotUnlockableColor;
        }

        private void HandleLine(int tierDifference, int slotDifference, UILineRenderer lineREF, int thisTier, int otherTier,
            bool isLeft)
        {
            if (slotDifference == 0)
            {
                // straight line
                lineREF.points.Clear();
                if (thisTier < otherTier)
                {
                    lineREF.points.Add(new Vector2(nodeXStartOffset, 0));
                    var YOffset = nodeDistanceOffset * tierDifference;
                    YOffset += tierDifference * nodeDistanceOffsetBonusPerTier;
                    if (YOffset < 0)
                        YOffset = Mathf.Abs(YOffset);
                    else
                        YOffset = -YOffset;
                    lineREF.points.Add(new Vector2(YOffset, 0));
                }
                else
                {
                    var y = nodeXStartOffset;
                    y += nodeOffsetWhenAbove;
                    y = -y;
                    lineREF.points.Add(new Vector2(y, 0));

                    var YOffset = nodeDistanceOffsetWhenAbove * tierDifference;
                    YOffset += tierDifference * nodeDistanceOffsetBonusPerTierWhenAbove;
                    if (YOffset < 0)
                        YOffset = Mathf.Abs(YOffset);
                    else
                        YOffset = -YOffset;
                    lineREF.points.Add(new Vector2(YOffset, 0));
                }
            }
            else
            {
                // line requires 3 points
                lineREF.points.Clear();

                if (isLeft)
                    lineREF.points.Add(new Vector2(0, nodeXStartOffset));
                else
                    lineREF.points.Add(new Vector2(0, -nodeXStartOffset));
                var XOffset = nodeDistanceOffset * slotDifference;
                if (XOffset < 0)
                    XOffset = Mathf.Abs(XOffset);
                else
                    XOffset = -XOffset;
                lineREF.points.Add(new Vector2(0, -XOffset));
                var YOffset = nodeDistanceOffset * tierDifference;
                YOffset += tierDifference * nodeDistanceOffsetBonusPerTier;
                if (YOffset < 0)
                    YOffset = Mathf.Abs(YOffset);
                else
                    YOffset = -YOffset;
                lineREF.points.Add(new Vector2(YOffset, -XOffset));
            }
        }

        private int getTierDifference(int initialValue)
        {
            if (initialValue < 0)
                return Mathf.Abs(initialValue);
            return -initialValue;
        }

        private TreeNodeHolder getAbilityNodeREF(RPGAbility ab)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].ability == ab)
                        return t.nodesREF[x];

            return null;
        }

        private TreeNodeHolder getCraftingRecipeNodeREF(RPGCraftingRecipe ab)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].recipe == ab)
                        return t.nodesREF[x];

            return null;
        }

        private TreeNodeHolder getBonusNodeREF(RPGBonus bonus)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].bonus == bonus)
                        return t.nodesREF[x];

            return null;
        }

        private TreeNodeHolder getResourceNodeNodeREF(RPGResourceNode ab)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].resourceNode == ab)
                        return t.nodesREF[x];

            return null;
        }

        private int[] getNodeTierSlotIndex(RPGTalentTree.Node_DATA nodeDATA)
        {
            var tierSlot = new int[2];
            for (var i = 0; i < treeUIData.Count; i++)
            for (var x = 0; x < treeUIData[i].slotsDATA.Count; x++)
                if (nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.ability &&
                    treeUIData[i].slotsDATA[x].ability != null &&
                    treeUIData[i].slotsDATA[x].ability.ID == nodeDATA.abilityID
                    || nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.recipe &&
                    treeUIData[i].slotsDATA[x].recipe != null && treeUIData[i].slotsDATA[x].recipe.ID == nodeDATA.recipeID
                    || nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                    treeUIData[i].slotsDATA[x].resourceNode != null &&
                    treeUIData[i].slotsDATA[x].resourceNode.ID == nodeDATA.resourceNodeID
                    || nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.bonus &&
                    treeUIData[i].slotsDATA[x].bonus != null && treeUIData[i].slotsDATA[x].bonus.ID == nodeDATA.bonusID
                )
                {
                    tierSlot[0] = i;
                    tierSlot[1] = x;
                    return tierSlot;
                }

            return tierSlot;
        }

        private int[] getNodeTierSlotIndex(TreeNodeSlotDATA nodeDATA)
        {
            var tierSlot = new int[2];
            for (var i = 0; i < treeUIData.Count; i++)
            for (var x = 0; x < treeUIData[i].slotsDATA.Count; x++)
                if (nodeDATA.type == RPGTalentTree.TalentTreeNodeType.ability &&
                    treeUIData[i].slotsDATA[x].ability != null && treeUIData[i].slotsDATA[x].ability == nodeDATA.ability
                    || nodeDATA.type == RPGTalentTree.TalentTreeNodeType.recipe &&
                    treeUIData[i].slotsDATA[x].recipe != null && treeUIData[i].slotsDATA[x].recipe == nodeDATA.recipe
                    || nodeDATA.type == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                    treeUIData[i].slotsDATA[x].resourceNode != null &&
                    treeUIData[i].slotsDATA[x].resourceNode == nodeDATA.resourceNode
                    || nodeDATA.type == RPGTalentTree.TalentTreeNodeType.bonus &&
                    treeUIData[i].slotsDATA[x].bonus != null && treeUIData[i].slotsDATA[x].bonus == nodeDATA.bonus
                )
                {
                    tierSlot[0] = i;
                    tierSlot[1] = x;
                    return tierSlot;
                }

            return tierSlot;
        }

        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
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