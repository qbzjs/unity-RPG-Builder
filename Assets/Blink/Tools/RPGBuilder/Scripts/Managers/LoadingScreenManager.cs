using System;
using System.Collections;
using BLINK.RPGBuilder.LogicMono;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class LoadingScreenManager : MonoBehaviour
    {
        public static LoadingScreenManager Instance { get; private set; }

        public Canvas loadingCanvas;
        public Image loadingBackground, loadingProgressImage;
        public TextMeshProUGUI gameSceneNameText, gameSceneDescriptionText, loadingProgressText;

        public bool isSceneLoading;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LoadGameScene(int gameSceneID)
        {
            RPGGameScene gameScene = RPGBuilderUtilities.GetGameSceneFromID(gameSceneID);
            if (gameScene == null) return;

            loadingCanvas.enabled = true;
            loadingBackground.sprite = gameScene.loadingBG;
            loadingProgressImage.fillAmount = 0;
            gameSceneNameText.text = gameScene.displayName;
            gameSceneDescriptionText.text = gameScene.description;

            loadingProgressText.text = 0 + " %";

            asyncLoad = new AsyncOperation();
            StartCoroutine(AsyncLoad(gameScene));
        }
        
        public void LoadMainMenu()
        {
            loadingCanvas.enabled = true;
            loadingBackground.sprite = RPGBuilderEssentials.Instance.generalSettings.mainMenuLoadingImage;
            loadingProgressImage.fillAmount = 0;
            gameSceneNameText.text = RPGBuilderEssentials.Instance.generalSettings.mainMenuLoadingName;
            gameSceneDescriptionText.text = RPGBuilderEssentials.Instance.generalSettings.mainMenuLoadingDescription;

            loadingProgressText.text = 0 + " %";

            asyncLoad = new AsyncOperation();
            StartCoroutine(AsyncLoadMainMenu());
        }

        private void ResetLoadingCanvas()
        {
            asyncLoad = null;
            loadingCanvas.enabled = false;
            loadingBackground.sprite = null;
            loadingProgressImage.fillAmount = 0;
            gameSceneNameText.text = "";
            gameSceneDescriptionText.text = "";

            loadingProgressText.text = 0 + " %";
        }

        private AsyncOperation asyncLoad = null;

        IEnumerator AsyncLoad(RPGGameScene gameSscene)
        {
            asyncLoad = SceneManager.LoadSceneAsync(gameSscene._name);
            asyncLoad.allowSceneActivation = !RPGBuilderEssentials.Instance.generalSettings.clickToLoadScene;

            isSceneLoading = true;
            
            while (!asyncLoad.isDone)
            {
                loadingProgressImage.fillAmount = asyncLoad.progress / 1f;
                int curProgress = (int) (asyncLoad.progress * 100f);
                loadingProgressText.text = curProgress + " %";

                if (!asyncLoad.allowSceneActivation && asyncLoad.progress >= 0.9f)
                {
                    loadingProgressText.text = "Click to continue";
                    loadingProgressImage.fillAmount = 1f;

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                        asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.25f);
            if (RPGBuilderEssentials.Instance.generalSettings.LoadingScreenEndDelay > 0)
            {
                loadingProgressText.text = "Loading World";
                loadingProgressImage.fillAmount = 1f;
                yield return new WaitForSeconds(RPGBuilderEssentials.Instance.generalSettings.LoadingScreenEndDelay);
            }

            isSceneLoading = false;
            ResetLoadingCanvas();
        }
        
        IEnumerator AsyncLoadMainMenu()
        {
            asyncLoad = SceneManager.LoadSceneAsync(RPGBuilderEssentials.Instance.generalSettings.mainMenuSceneName);
            asyncLoad.allowSceneActivation = true;
            
            while (!asyncLoad.isDone)
            {
                loadingProgressImage.fillAmount = asyncLoad.progress / 1f;
                int curProgress = (int) (asyncLoad.progress * 100f);
                loadingProgressText.text = curProgress + " %";
                yield return null;
            }
            ResetLoadingCanvas();
        }
    }
}
