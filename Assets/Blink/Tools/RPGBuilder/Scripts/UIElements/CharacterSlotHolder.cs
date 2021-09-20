using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.UIElements
{
    public class CharacterSlotHolder : MonoBehaviour
    {
        public TextMeshProUGUI CharacterNameText, LevelText, RaceText, ClassText;

        public void Init(CharacterData charData)
        {
            CharacterNameText.text = charData.CharacterName;
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                LevelText.text = "LvL. " + charData.classDATA.currentClassLevel;
                ClassText.text = RPGBuilderUtilities.GetClassFromID(charData.classDATA.classID).displayName;
            }
            else
            {
                LevelText.text = "";
                ClassText.text = "";
            }

            RaceText.text = RPGBuilderUtilities.GetRaceFromID(charData.raceID).displayName;
        }

        public void SelectCharacter()
        {
            MainMenuManager.Instance.SelectCharacter(CharacterNameText.text);
        }
    }
}