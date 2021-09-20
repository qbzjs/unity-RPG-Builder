using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class ClassSlotHolder : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI className;
        public Image selectedBorder;
        public int classIndex;

        public void Init(RPGClass thisClass, int index)
        {
            icon.sprite = thisClass.icon;
            className.text = thisClass.displayName;
            classIndex = index;
        }

        public void ClickSelect()
        {
            MainMenuManager.Instance.SelectClass(classIndex);
        }
    }
}