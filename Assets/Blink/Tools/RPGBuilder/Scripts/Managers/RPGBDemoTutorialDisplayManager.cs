using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class RPGBDemoTutorialDisplayManager : MonoBehaviour, IDisplayPanel
{
    public static RPGBDemoTutorialDisplayManager Instance { get; private set; }

    public bool showTutorial = true;
    public CanvasGroup thisCG;
    public TextMeshProUGUI text;
    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public IEnumerator InitTutorial()
    {
        yield return new WaitForSeconds(0.2f);
        Show();
    }
    
    public void Show()
    {
        RPGBuilderUtilities.EnableCG(thisCG);
        transform.SetAsLastSibling();
        InitTutorialText();
        
        CustomInputManager.Instance.AddOpenedPanel(thisCG);
        if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(true);
    }

    public void Hide()
    {
        gameObject.transform.SetAsFirstSibling();
        RPGBuilderUtilities.DisableCG(thisCG);
        if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
    }

    private void InitTutorialText()
    {
        text.text = "Welcome to the RPG Builder demo!\n\n" +
                    "Switch to aiming mode with the <color=white>" +
                    RPGBuilderUtilities.GetCurrentKeyByActionKeyName("TOGGLE_CAMERA_AIM_MODE") + " Key </color> \n" +
                    "Enable the cursor control with the <color=white>" +
                    RPGBuilderUtilities.GetCurrentKeyByActionKeyName("TOGGLE_CURSOR") + " Key </color> \n" +
                    "Block incoming attacks by holding <color=white>" +
                    "Right Click </color> \n\n" +
                    "You can change all keybindings in the Settings\n" +
                    "Come chat on the Blink Discord!";
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/fYzpuYwPwJ");
    }
    
}
