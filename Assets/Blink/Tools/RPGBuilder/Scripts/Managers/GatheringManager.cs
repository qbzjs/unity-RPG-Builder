using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class GatheringManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static GatheringManager Instance { get; private set; }

        private bool CheckResourceNodeRankingRequirements(RPGResourceNode resourceNode, RPGTalentTree tree, int rank)
        {
            var rankREF = resourceNode.ranks[rank];
            if (CharacterData.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rankREF.unlockCost)
            {
                //NOT ENOUGH POINTS
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("Not Enough Points", 3);
                return false;
            }

            List<bool> reqResults = new List<bool>();
            foreach (var t in RPGBuilderUtilities.getNodeRequirements(tree, resourceNode))
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
            
                reqResults.Add(RequirementsManager.Instance.HandleRequirementType(t, intValue1, intValue2,true));
            }

            return !reqResults.Contains(false);
        }

        private bool CheckResourceNodeRankingDown(RPGResourceNode resourceNode, RPGTalentTree tree)
        {
            foreach (var t in tree.nodeList)
            foreach (var t1 in t.requirements)
                if (t1.requirementType ==
                    RequirementsManager.RequirementType.resourceNodeKnown &&
                    t1.resourceNodeRequiredID == resourceNode.ID &&
                    RPGBuilderUtilities.isResourceNodeKnown(t.resourceNodeID) &&
                    RPGBuilderUtilities.getResourceNodeRank(resourceNode.ID) == 0)
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent(
                        "Cannot unlearn a resource node that is required for others", 3);
                    return false;
                }

            return true;
        }

        public void RankUpResourceNode(RPGResourceNode ab, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.resourceNodeData)
            {
                if (t.ID != ab.ID) continue;
                if (t.rank >= ab.ranks.Count) continue;
                if (!CheckResourceNodeRankingRequirements(ab, tree, t.rank)) continue;
                var rankREF = ab.ranks[t.rank];
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID,rankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, rankREF.unlockCost);
                t.rank++;
                t.known = true;

                TreesDisplayManager.Instance.InitTree(tree);

                if (t.rank == 1)
                    CharacterEventsManager.Instance.ResourceNodeLearned(ab);
            }
        }

        public void RankDownResourceNode(RPGResourceNode ab, RPGTalentTree tree)
        {
            foreach (var t in CharacterData.Instance.resourceNodeData)
            {
                if (t.ID != ab.ID) continue;
                if (t.rank <= 0) continue;
                if(ab.learnedByDefault && t.rank == 1) continue;
                if (!CheckResourceNodeRankingDown(ab, tree)) continue;
                var rankREF = ab.ranks[t.rank - 1];
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID, rankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, -rankREF.unlockCost);
                t.rank--;

                if (t.rank == 0)
                    t.known = false;
                TreesDisplayManager.Instance.InitTree(tree);
            }
        }
        

        public void HandleStartingResourceNodes()
        {
            foreach (var resourceNode in CharacterData.Instance.resourceNodeData)
            {
                RPGResourceNode resourceNodeREF = RPGBuilderUtilities.GetResourceNodeFromID(resourceNode.ID);
                if (!resourceNodeREF.learnedByDefault) continue;
                RPGBuilderUtilities.setResourceNodeData(resourceNode.ID, 1, true);
            }
        }

    }
}