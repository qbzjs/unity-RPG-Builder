using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.UIElements
{
    public class KeybindSlotHolder : MonoBehaviour
    {
        public TextMeshProUGUI keybindNameText, keybindValueText;
        public string actionKeyName;

        public void ClickKeybindSlot()
        {
            CustomInputManager.Instance.InitKeyChecking(actionKeyName);
        }

        public void InitializeSlot(RPGGeneralDATA.ActionKey action)
        {
            actionKeyName = action.actionName;
            keybindNameText.text = action.actionDisplayName;
            keybindValueText.text = RPGBuilderUtilities.GetKeybindText(RPGBuilderUtilities.GetCurrentKeyByActionKeyName(action.actionName));
        }

        public void ResetKeybind()
        {
            CustomInputManager.Instance.ResetKey(actionKeyName);
        }
    }
}