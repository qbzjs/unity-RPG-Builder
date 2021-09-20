using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class SkillSlotHolder : MonoBehaviour
    {
        public Image skillIcon, skillBackground, skillEXPBar;
        public TextMeshProUGUI skillNameText, skillLevelText;
        private RPGSkill thisSkill;
        public Animator animator;
        

        public void InitSlot(RPGSkill skill)
        {
            skillIcon.sprite = skill.icon;
            skillBackground.sprite = skill.icon;
            skillEXPBar.fillAmount = RPGBuilderUtilities.getSkillEXPPercent(skill);
            skillLevelText.text = RPGBuilderUtilities.getSkillLevel(skill.ID) + " / " +
                                  RPGBuilderUtilities.GetLevelTemplateFromID(skill.levelTemplateID).levels;
            skillNameText.text = skill.displayName;
            thisSkill = skill;
            
            animator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInSkill(skill.ID));
        }

        public void SelectSkill()
        {
            SkillBookDisplayManager.Instance.ShowSkillInfo(thisSkill);
        }
    }
}