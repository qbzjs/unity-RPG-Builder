using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.UIElements
{
    public class AbilityTooltipElement : MonoBehaviour
    {
        public enum ABILITY_TOOLTIP_ELEMENT_TYPE
        {
            CenteredTitle,
            Description,
            Separation
        }

        public ABILITY_TOOLTIP_ELEMENT_TYPE elementType;

        public TextMeshProUGUI text;

        public void InitTitle(string Text, Color color)
        {
            text.text = Text;
            text.color = color;
        }

        public void InitDescription(string Text, Color color)
        {
            text.text = Text;
            text.color = color;
        }
    }
}