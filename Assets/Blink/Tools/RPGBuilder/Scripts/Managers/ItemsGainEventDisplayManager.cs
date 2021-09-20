using System.Collections;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class ItemsGainEventDisplayManager : MonoBehaviour
    {
        public GameObject textPrefab;
        public float duration;
        public Transform textParent;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static ItemsGainEventDisplayManager Instance { get; private set; }

        public void DisplayText(string message)
        {
            GameObject newText = Instantiate(textPrefab, textParent);
            newText.GetComponent<TextMeshProUGUI>().text = message;
            Destroy(newText, duration);
        }
    }
}
