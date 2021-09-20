using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldInteractableDisplayManager : MonoBehaviour
{
    public static WorldInteractableDisplayManager Instance { get; private set; }

    public CanvasGroup thisCG;
    public TextMeshProUGUI keyText, nameText;
    private bool isVisible;

    public bool IsVisible()
    {
        return isVisible;
    }

    public bool showInteractionBar;
    public Image interactionBar;
    
    public IPlayerInteractable cachedInteractable;
    
    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void Show(IPlayerInteractable playerInteractable)
    {
        cachedInteractable = playerInteractable;
        isVisible = true;
        keyText.text = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("INTERACT").ToString();
        nameText.text = cachedInteractable.getInteractableName();
        InitInteractionBar();
        RPGBuilderUtilities.EnableCG(thisCG);
    }
    
    public void Hide()
    {
        cachedInteractable = null;
        isVisible = false;
        RPGBuilderUtilities.DisableCG(thisCG);
    }

    public void Interact()
    {
        cachedInteractable.Interact();
    }

    private void InitInteractionBar()
    {
        interactionBar.fillAmount = 0f / 1f;
    }

    public void UpdateInteractionBar(float curTime, float maxTime)
    {
        interactionBar.fillAmount = curTime / maxTime;
    }

    public void ResetInteractionBarBar()
    {
        interactionBar.fillAmount = 0f;
    }
}
