using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class CrosshairDisplayManager : MonoBehaviour
    {
        public Image crosshair;

        public static CrosshairDisplayManager Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void ShowCrosshair()
        {
            crosshair.enabled = true;
        }

        public void HideCrosshair()
        {
            crosshair.enabled = false;
        }
    }
}