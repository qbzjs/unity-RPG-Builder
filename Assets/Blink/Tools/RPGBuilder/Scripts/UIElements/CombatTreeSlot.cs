using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class CombatTreeSlot : MonoBehaviour
    {
        public Image combatTreeBackground;
        public TextMeshProUGUI combatTreeName;
        public Animator animator;

        private RPGTalentTree thisTree;

        public void InitSlot(RPGTalentTree cbtTree)
        {
            combatTreeBackground.sprite = cbtTree.icon;
            combatTreeName.text = cbtTree.displayName;
            thisTree = cbtTree;

            animator.SetBool("glowing", CharacterData.Instance.getTreePointsAmountByPoint(cbtTree.treePointAcceptedID) > 0);
        }

        public void InitTitle(string title)
        {
            combatTreeBackground.enabled = false;
            combatTreeName.text = title;
        }

        public void SelectCombatTree()
        {
            if (CharacterPanelDisplayManager.Instance.thisCG.alpha == 1)
            {
                TreesDisplayManager.Instance.curPreviousMenu = TreesDisplayManager.previousMenuType.charPanel;
                CharacterPanelDisplayManager.Instance.Hide();
            }
            else if(SkillBookDisplayManager.Instance.thisCG.alpha == 1)
            {
                TreesDisplayManager.Instance.curPreviousMenu = TreesDisplayManager.previousMenuType.skillBook;
                SkillBookDisplayManager.Instance.Hide();
            }
            else if(WeaponTemplatesDisplayManager.Instance.thisCG.alpha == 1)
            {
                TreesDisplayManager.Instance.curPreviousMenu = TreesDisplayManager.previousMenuType.weaponTemplates;
                WeaponTemplatesDisplayManager.Instance.Hide();
            }
            TreesDisplayManager.Instance.InitTree(thisTree);
        }
    }
}