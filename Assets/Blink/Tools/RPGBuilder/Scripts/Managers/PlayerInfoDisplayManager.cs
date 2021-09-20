using BLINK.RPGBuilder.LogicMono;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class PlayerInfoDisplayManager : MonoBehaviour
    {
        public CanvasGroup castBarCG;
        public Image castBar;
        public TextMeshProUGUI castAbilityName, castAbilityTime, LevelText;

        public bool showInteractionBar = true;
        public CanvasGroup InteractionBarCG;
        public Image interactionBar;
        public TextMeshProUGUI interactionTime;

        public CanvasGroup channelBarCG;
        public Image channelBar;
        public TextMeshProUGUI channelAbilityName, channelAbilityTime, CharacterName;
        public Image portraitIcon;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void Init()
        {
            CharacterName.text = CharacterData.Instance.CharacterName;
            portraitIcon.sprite = RPGBuilderEssentials.Instance.combatSettings.useClasses ? RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID).icon : 
                RPGBuilderUtilities.getRaceIcon();
            LevelText.enabled = RPGBuilderEssentials.Instance.combatSettings.useClasses;
            LevelText.text = CharacterData.Instance.classDATA.currentClassLevel.ToString();
        }

        public void InitCastBar(RPGAbility castedAbility)
        {
            castBarCG.alpha = 1;
            castBar.fillAmount = 0f / 1f;
            castAbilityName.text = castedAbility.displayName;
            castAbilityTime.text = 0 + "";
        }

        public void InitInteractionBar()
        {
            if (!showInteractionBar) return;
            InteractionBarCG.alpha = 1;
            interactionBar.fillAmount = 0f / 1f;
            interactionTime.text = 0 + "";
        }

        public void UpdateInteractionBar(float curTime, float maxTime)
        {
            if (!showInteractionBar) return;
            interactionBar.fillAmount = curTime / maxTime;
            interactionTime.text = curTime.ToString("F1") + " / " + maxTime.ToString("F1");
        }

        public void ResetInteractionBarBar()
        {
            if (!showInteractionBar) return;
            InteractionBarCG.alpha = 0;
            interactionBar.fillAmount = 0f;
            interactionTime.text = "";
        }

        public void UpdateCastBar(float curTime, float maxTime)
        {
            castBar.fillAmount = curTime / maxTime;
            castAbilityTime.text = curTime.ToString("F1") + " / " + maxTime.ToString("F1");
        }

        public void UpdateLevelText()
        {
            LevelText.text = CharacterData.Instance.classDATA.currentClassLevel.ToString();
        }

        public void ResetCastBar()
        {
            castBarCG.alpha = 0;
            castBar.fillAmount = 0f;
            castAbilityName.text = "";
            castAbilityTime.text = "";
        }

        public void InitChannelBar(RPGAbility castedAbility)
        {
            channelBarCG.alpha = 1;
            channelBar.fillAmount = 1f / 1f;
            channelAbilityName.text = castedAbility.displayName;
            channelAbilityTime.text = 0 + "";
        }

        public void UpdateChannelBar(float curTime, float maxTime)
        {
            channelBar.fillAmount = curTime / maxTime;
            channelAbilityTime.text = curTime.ToString("F1") + " / " + maxTime.ToString("F1");
        }

        public void ResetChannelBar()
        {
            channelBarCG.alpha = 0;
            channelBar.fillAmount = 0f;
            channelAbilityName.text = "";
            channelAbilityTime.text = "";
        }

        public void TargetPlayer()
        {
            CombatManager.Instance.SetPlayerTarget(CombatManager.playerCombatNode, false);
        }

        public static PlayerInfoDisplayManager Instance { get; private set; }
    }
}