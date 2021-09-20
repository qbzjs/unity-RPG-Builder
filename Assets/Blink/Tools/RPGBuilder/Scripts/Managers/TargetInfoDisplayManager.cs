using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class TargetInfoDisplayManager : MonoBehaviour
    {
        public CanvasGroup thisCG;
        public TextMeshProUGUI targetNameText, targetHPText, targetManaText, targetLevelText;
        public Image targetHealthbar, targetManaBar, targetIcon;

        public Sprite allyHB, neutralHB, enemyHB;

        private CombatNode curTarget;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static TargetInfoDisplayManager Instance { get; private set; }

        public void InitTargetUI(CombatNode cbtNode)
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            curTarget = cbtNode;
            if (curTarget.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
            {
                targetNameText.text = CharacterData.Instance.CharacterName;
                targetHealthbar.sprite = allyHB;
                targetIcon.sprite = RPGBuilderUtilities.getRaceIcon();
                targetLevelText.text = CharacterData.Instance.classDATA.currentClassLevel.ToString();
            }
            else
            {
                targetNameText.text = cbtNode.npcDATA.displayName;
                targetIcon.sprite = cbtNode.npcDATA.icon;
                targetLevelText.text = cbtNode.NPCLevel.ToString();
                
                RPGCombatDATA.ALIGNMENT_TYPE thisNodeAlignment = FactionManager.Instance.GetAlignmentForPlayer(cbtNode.npcDATA.factionID);
                switch (thisNodeAlignment)
                {
                    case RPGCombatDATA.ALIGNMENT_TYPE.ALLY:
                        targetHealthbar.sprite = allyHB;
                        break;
                    case RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                        targetHealthbar.sprite = neutralHB;
                        break;
                    case RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                        targetHealthbar.sprite = enemyHB;
                        break;
                }
            }

            UpdateTargetHealthBar();
            UpdateTargetEnergyBar();
        }

        public void ResetTarget()
        {
            curTarget = null;
            RPGBuilderUtilities.DisableCG(thisCG);
        }

        public void UpdateTargetHealthBar()
        {
            if (curTarget != null)
            {
                var currentValue = curTarget.getCurrentValue(RPGBuilderEssentials.Instance.healthStatReference._name);
                var currentMaxValue = curTarget.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name);
                targetHealthbar.fillAmount = currentValue / currentMaxValue;
                targetHPText.text = (int)currentValue + " / " + (int)currentMaxValue;
            }
            else
            {
                CombatManager.Instance.ResetPlayerTarget();
            }
        }

        public void UpdateBars()
        {
            UpdateTargetHealthBar();
            UpdateTargetEnergyBar();
        }

        public void UpdateTargetEnergyBar()
        {
            if (curTarget != null)
            {
                var currentValue = curTarget.getCurrentValue("Energy");
                var currentMaxValue = curTarget.getCurrentMaxValue("Energy");
                targetManaBar.fillAmount = currentValue / currentMaxValue;
                targetManaText.text = (int)currentValue + " / " + (int)currentMaxValue;
            }
            else
            {
                CombatManager.Instance.ResetPlayerTarget();
            }
        }
    }
}