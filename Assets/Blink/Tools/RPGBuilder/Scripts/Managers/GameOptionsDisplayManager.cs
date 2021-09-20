using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.Managers
{
    public class GameOptionsDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;
        private bool showing;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static GameOptionsDisplayManager Instance { get; private set; }

        public void BackToMainMenu()
        {
            RPGBuilderEssentials.Instance.ClearAllWorldItemData();
            if (CombatManager.playerCombatNode.appearanceREF.isShapeshifted)
            {
                CombatManager.Instance.ResetPlayerShapeshift();
            }
            RPGBuilderJsonSaver.SaveCharacterData(CharacterData.Instance.CharacterName, CharacterData.Instance);
            Hide();
            RPGBuilderEssentials.Instance.mainGameCanvas.enabled = false;
            RPGBuilderEssentials.Instance.HandleDATAReset();
            CharacterData.Instance.RESET_CHARACTER_DATA(true);
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            LoadingScreenManager.Instance.LoadMainMenu();
        }
        public void QuitGame()
        {
            RPGBuilderEssentials.Instance.ClearAllWorldItemData();
            if (CombatManager.playerCombatNode.appearanceREF.isShapeshifted)
            {
                CombatManager.Instance.ResetPlayerShapeshift();
            }
            RPGBuilderJsonSaver.SaveCharacterData(CharacterData.Instance.CharacterName, CharacterData.Instance);
            Application.Quit();
        }

        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        public void HideAutomatic()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }
    }
}