using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class BossUISlotHolder : MonoBehaviour
    {
        public CanvasGroup thisCG;
        public Image HPBar;
        public TextMeshProUGUI HPText, BossNameText;
        public CombatNode thisNode;


        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static BossUISlotHolder Instance { get; private set; }

        public void Init(CombatNode nodeRef)
        {
            if (nodeRef.dead) return;
            RPGBuilderUtilities.EnableCG(thisCG);
            thisNode = nodeRef;
            if (HPBar != null) HPBar.fillAmount = nodeRef.getCurrentValue(RPGBuilderEssentials.Instance.healthStatReference._name) / nodeRef.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name);
            if (HPText != null)
                HPText.text = nodeRef.getCurrentValue(RPGBuilderEssentials.Instance.healthStatReference._name) + " / " + nodeRef.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name);
            if (BossNameText != null) BossNameText.text = nodeRef.npcDATA.displayName + " | LvL. " + nodeRef.NPCLevel;
        }

        public void UpdateHealth()
        {
            if (thisNode == null) ResetBossUI();
            var curHp = thisNode.getCurrentValue(RPGBuilderEssentials.Instance.healthStatReference._name);
            var maxHp = thisNode.getCurrentMaxValue(RPGBuilderEssentials.Instance.healthStatReference._name);
            if (HPBar != null) HPBar.fillAmount = curHp / maxHp;
            if (HPText != null) HPText.text = (int) curHp + " / " + (int) maxHp;
            if (curHp <= 0) ResetBossUI();
        }

        public void ResetBossUI()
        {
            RPGBuilderUtilities.DisableCG(thisCG);
            thisNode = null;
        }
    }
}