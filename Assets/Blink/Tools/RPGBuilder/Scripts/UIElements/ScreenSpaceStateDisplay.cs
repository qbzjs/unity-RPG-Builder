using BLINK.RPGBuilder.LogicMono;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class ScreenSpaceStateDisplay : MonoBehaviour
    {
        public Image border, icon;
        public TextMeshProUGUI stackText;
        private float curTime, maxTime;
        private NameplatesDATAHolder holderREF;

        public void InitializeState(Color borderColor, CombatNode.NodeStatesDATA nodeStateData, NameplatesDATAHolder holder, float _curTime)
        {
            border.color = borderColor;
            icon.sprite = nodeStateData.stateIcon;
            stackText.text = nodeStateData.curStack.ToString();
            curTime = _curTime;
            maxTime = nodeStateData.stateMaxDuration;
            holderREF = holder;
        }

        private void Update()
        {
            border.fillAmount = curTime / maxTime;
        }

        private void FixedUpdate()
        {
            curTime -= Time.deltaTime;

            if (!(curTime <= 0)) return;
            for (var i = 0; i < holderREF.statesList.Count; i++)
                if (holderREF.statesList[i].statesDisplay == this)
                    holderREF.statesList.RemoveAt(i);
            Destroy(gameObject);
        }
    }
}