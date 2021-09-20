using System.Collections;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class ErrorEventsDisplayManager : MonoBehaviour
    {
        public CanvasGroup thisCGG;
        public TextMeshProUGUI errorMessageText;

        private Coroutine messageCoroutine;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static ErrorEventsDisplayManager Instance { get; private set; }

        public void ShowErrorEvent(string errorMessage, float duration)
        {
            if (messageCoroutine == null)
            {
                messageCoroutine = StartCoroutine(ErrorEvent(errorMessage, duration));
            }
            else
            {
                StopCoroutine(messageCoroutine);
                messageCoroutine = StartCoroutine(ErrorEvent(errorMessage, duration));
            }
        }

        private IEnumerator ErrorEvent(string errorMessage, float duration)
        {
            RPGBuilderUtilities.EnableCG(thisCGG);
            errorMessageText.text = errorMessage;
            yield return new WaitForSeconds(duration);
            RPGBuilderUtilities.DisableCG(thisCGG);
        }
    }
}