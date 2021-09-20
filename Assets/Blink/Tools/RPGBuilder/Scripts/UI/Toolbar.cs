using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

namespace BLINK.RPGBuilder.UI
{
    public class Toolbar : MonoBehaviour
    {

        public Animator characterButtonAnimator;
        public Animator skillbookButtonAnimator;
        public Animator weaponTemplatebookButtonAnimator;
 
        public void InitToolbar()
        {
            if(RPGBuilderEssentials.Instance.combatSettings.useClasses)characterButtonAnimator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInClassTrees());
            skillbookButtonAnimator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInSkillTrees());
            weaponTemplatebookButtonAnimator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInWeaponTemplateTrees());
        }
    
    
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }
    
        public static Toolbar Instance { get; private set; }
    }
}
