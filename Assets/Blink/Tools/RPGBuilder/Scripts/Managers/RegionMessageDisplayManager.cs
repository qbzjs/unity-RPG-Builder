using System.Collections;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class RegionMessageDisplayManager : MonoBehaviour
    {
        public TextMeshProUGUI regionMessageText;

        private Coroutine messageCoroutine;
        public Animator thisAnim;
        private static readonly int regionIn = Animator.StringToHash("RegionIn");
        private static readonly int regionOut = Animator.StringToHash("RegionOut");

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static RegionMessageDisplayManager Instance { get; private set; }

        public void ShowRegionMessage(string message, float duration)
        {
            if (messageCoroutine == null)
            {
                messageCoroutine = StartCoroutine(RegionEvent(message, duration));
            }
            else
            {
                StopCoroutine(messageCoroutine);
                messageCoroutine = StartCoroutine(RegionEvent(message, duration));
            }
        }

        private IEnumerator RegionEvent(string errorMessage, float duration)
        {
            thisAnim.Rebind();
            thisAnim.SetTrigger(regionIn);
            regionMessageText.text = errorMessage;
            yield return new WaitForSeconds(duration);
            thisAnim.SetTrigger(regionOut);
            
        }
    }
}
