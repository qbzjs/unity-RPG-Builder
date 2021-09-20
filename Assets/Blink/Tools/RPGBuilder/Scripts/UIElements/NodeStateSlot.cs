using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class NodeStateSlot : MonoBehaviour, IPointerClickHandler
    {
        public Image stateBorder, stateIcon;
        public TextMeshProUGUI stackText;
        public Color buffColor, debuffColor;
        private RPGEffect curEffect;
        public int thisIndex, curEffectRank;

        private float curDuration, maxDuration;

        private bool isUpdating;

        public void InitStateSlot(bool buff, RPGEffect effect, int effectRank, Sprite icon, float maxDur, int index)
        {
            stateBorder.color = buff ? buffColor : debuffColor;
            curEffect = effect;
            curEffectRank = effectRank;
            stateIcon.sprite = icon;
            stateBorder.fillAmount = 1;
            maxDuration = maxDur;
            curDuration = maxDur;
            thisIndex = index;

            UpdateStackText();

            isUpdating = true;
        }

        public void UpdateStackText()
        {
            if (CombatManager.playerCombatNode.nodeStateData[thisIndex].curStack == 1)
            {
                stackText.text = "";
                return;
            }

            stackText.text = "" + CombatManager.playerCombatNode.nodeStateData[thisIndex].curStack;
        }

        private void FixedUpdate()
        {
            if (curEffect.endless) return;
            if (isUpdating) curDuration -= Time.deltaTime;
            if (curDuration <= 0) PlayerStatesDisplayHandler.Instance.RemoveState(thisIndex);
        }

        private void Update()
        {
            if (curEffect.endless) return;
            if (isUpdating) stateBorder.fillAmount = curDuration / maxDuration;
        }
        
        public void ShowTooltip()
        {
            AbilityTooltip.Instance.ShowEffect(curEffect, curEffectRank);
        }

        public void HideTooltip()
        {
            AbilityTooltip.Instance.Hide();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if(!curEffect.isBuffOnSelf) return;
            CombatManager.playerCombatNode.RemoveEffectByIndex(thisIndex);
            AbilityTooltip.Instance.Hide();
        }
    }
}