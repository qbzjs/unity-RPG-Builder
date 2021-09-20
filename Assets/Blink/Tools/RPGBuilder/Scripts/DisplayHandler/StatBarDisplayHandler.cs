using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.DisplayHandler
{
    public class StatBarDisplayHandler : MonoBehaviour
    {
        public Image fillBar;
        public TextMeshProUGUI amountText;

        public string STAT_NAME;

        public void UpdateBar()
        {
            if (STAT_NAME == "") return;
            if (fillBar != null) fillBar.fillAmount = CombatManager.playerCombatNode.getCurrentValue(STAT_NAME) / CombatManager.playerCombatNode.getCurrentMaxValue(STAT_NAME);
            if (amountText != null) amountText.text = (int)CombatManager.playerCombatNode.getCurrentValue(STAT_NAME) + " / " + (int)CombatManager.playerCombatNode.getCurrentMaxValue(STAT_NAME);
        }

    }
}
