using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Character
{
    public class PlayerAnimatorLayerHandler : MonoBehaviour
    {
        private Animator thisAnim;
        private RPGBCharacterControllerEssentials controllerEssentials;

        private void Start()
        {
            thisAnim = GetComponent<Animator>();
            controllerEssentials = GetComponent<RPGBCharacterControllerEssentials>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (CombatManager.Instance == null) return;
            if (CombatManager.playerCombatNode == null) return;

            switch (thisAnim.layerCount)
            {
                case 1:
                    return;
                case 2:
                    thisAnim.SetLayerWeight(1, 1);
                    return;
                case 3:
                    if (!controllerEssentials.HasMovementRestrictions() && controllerEssentials.IsMoving())
                    {
                        thisAnim.SetLayerWeight(1, 0);
                        thisAnim.SetLayerWeight(2, 1);
                    }
                    else
                    {
                        thisAnim.SetLayerWeight(1, 1);
                        thisAnim.SetLayerWeight(2, 0);
                    }
                    return;
            }
        }
        
    }
}
