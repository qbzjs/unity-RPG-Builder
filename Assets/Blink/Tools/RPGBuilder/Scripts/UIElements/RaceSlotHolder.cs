using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class RaceSlotHolder : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI raceName;
        public Image selectedBorder;
        public int raceIndex;

        public void Init(RPGRace thisRace, int index)
        {
            raceName.text = thisRace.displayName;
            raceIndex = index;
            icon.sprite = RPGBuilderUtilities.getRaceIcon(thisRace);
        }

        public void ClickSelect()
        {
            MainMenuManager.Instance.SelectRace(raceIndex);
        }
    }
}