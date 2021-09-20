using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.DisplayHandler
{
    public class EXPBarDisplayHandler : MonoBehaviour
    {
        public enum EXPBar_Type
        {
            ClassEXP,
            SkillEXP
        }
        public EXPBar_Type expBarType;
    
        public RPGSkill _skill;

        public Image EXPBar;

        public void UpdateBar()
        {
            if (expBarType != EXPBar_Type.ClassEXP) return;
            EXPBar.fillAmount = (float)CharacterData.Instance.classDATA.currentClassXP / (float)CharacterData.Instance.classDATA.maxClassXP;
        }
    }
}
