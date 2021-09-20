using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.UI
{
    public class FPSCounter : MonoBehaviour
    {
        public float timer, refresh, avgFramerate;
        private readonly string display = "{0} FPS";
        public TextMeshProUGUI m_Text;


        private void Update()
        {
            var timelapse = Time.smoothDeltaTime;
            timer = timer <= 0 ? refresh : timer -= timelapse;

            if (timer <= 0) avgFramerate = (int) (1f / timelapse);
            m_Text.text = string.Format(display, avgFramerate.ToString());
        }
    }
}