using System.Collections.Generic;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class SettingsPanelDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;

        public GameObject keybindSlotPrefab, keybindCategoryPrefab;
        public Transform keybindsParent;
        public List<KeybindSlotHolder> keybindSlots = new List<KeybindSlotHolder>();

        public Slider masterVolumeSlider;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
            
            masterVolumeSlider.onValueChanged.AddListener(delegate {SliderChange(masterVolumeSlider); });
        }

        public void InitializeKeybindSlots()
        {
            if(keybindSlots.Count > 0) return;
            foreach (var category in RPGBuilderEssentials.Instance.generalSettings.ActionKeyCategoryList)
            {
                GameObject categoryGO = Instantiate(keybindCategoryPrefab, keybindsParent);
                categoryGO.GetComponentInChildren<TextMeshProUGUI>().text = category;

                foreach (var actionKey in RPGBuilderEssentials.Instance.generalSettings.actionKeys)
                {
                    if(actionKey.category != category) continue;
                    GameObject actionKeyGO = Instantiate(keybindSlotPrefab, keybindsParent);
                    KeybindSlotHolder REF = actionKeyGO.GetComponent<KeybindSlotHolder>();
                    REF.InitializeSlot(actionKey);
                    keybindSlots.Add(REF);
                }
            }
        }

        public void UpdateKeybindSlot(string actionKeyName, KeyCode newKey)
        {
            foreach (var keybindSlot in keybindSlots)
            {
                if(keybindSlot.actionKeyName != actionKeyName) continue;
                keybindSlot.keybindValueText.text = RPGBuilderUtilities.GetKeybindText(newKey);
            }
        }

        private void SliderChange(Slider slider)
        {
            if (slider == masterVolumeSlider)
            {
                PlayerPrefs.SetFloat("MasterVolume", slider.value);
                AudioListener.volume = slider.value;
            }
        }

        private void InitSliders()
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        }

        public void Show()
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitializeKeybindSlots();
            InitSliders();
        
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            
            GameOptionsDisplayManager.Instance.HideAutomatic();
        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        public void Toggle()
        {
            if (thisCG.alpha == 1)
                Hide();
            else
                Show();
        }

        public static SettingsPanelDisplayManager Instance { get; private set; }
    }
}