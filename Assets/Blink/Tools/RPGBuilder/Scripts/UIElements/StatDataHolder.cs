using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using TMPro;

namespace BLINK.RPGBuilder.UIElements
{
    public class StatDataHolder : MonoBehaviour
    {

        public TextMeshProUGUI statText;
        private RPGStat statREF;

        public void InitStatText(CombatNode.NODE_STATS statDataREF)
        { 
            statREF = statDataREF.stat;
            if (statREF.isVitalityStat)
                statText.text = statREF.displayName + ": " + statDataREF.curMaxValue;
            else
                statText.text = statREF.displayName + ": " + statDataREF.curValue;

            if (statREF.isPercentStat)
            {
                statText.text += "%";
            }
        }

        public void ShowStatTooltip()
        {
            if(statREF.description != "") CharacterPanelDisplayManager.Instance.ShowStatTooltipPanel(statREF);
        }

        public void HideStatTooltip()
        {
            CharacterPanelDisplayManager.Instance.HideStatTooltipPanel();
        }
        
    }
}
