using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactionSlotDataHolder : MonoBehaviour
{
    public Image factionIcon, factionProgressBar;
    public TextMeshProUGUI factionNameText, factionDescriptionText, factionXPText, factionStanceText;

    public void Init(CharacterData.Faction_DATA factionData)
    {
        RPGFaction factionREF = RPGBuilderUtilities.GetFactionFromID(factionData.ID);
        factionIcon.sprite = factionREF.icon;

        factionNameText.text = factionREF.displayName;
        if(factionDescriptionText!=null)factionDescriptionText.text = factionREF.description;
        if (factionREF.factionStances.Count > 0)
        {
            int pointsRequired = factionREF
                .factionStances[FactionManager.Instance.GetCurrentStanceIndex(factionREF, factionData.currentStance)]
                .pointsRequired;
            factionXPText.text = factionData.currentPoint + " / " + pointsRequired;
            factionProgressBar.fillAmount = (float) ((float) factionData.currentPoint / (float) pointsRequired);
        }
        
        string alignmentText = "";
        Color stanceColor = Color.white;
        switch (FactionManager.Instance.GetAlignmentForPlayer(factionData.ID))
        {
            case RPGCombatDATA.ALIGNMENT_TYPE.ALLY:
                alignmentText = "Allied";
                stanceColor = Color.green;
                break;
            case RPGCombatDATA.ALIGNMENT_TYPE.NEUTRAL:
                alignmentText = "Neutral";
                stanceColor = Color.yellow;
                break;
            case RPGCombatDATA.ALIGNMENT_TYPE.ENEMY:
                alignmentText = "Enemy";
                stanceColor = Color.red;
                break;
        }

        factionStanceText.text = factionData.currentStance + " (" + alignmentText + ")";
        factionStanceText.color = stanceColor;

    }
}
