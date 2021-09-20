using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class GroundIndicatorManager : MonoBehaviour
    {
        public LayerMask MouseRaycast = ~0;
        public GroundIndicator Indicator;

        public void HideIndicator()
        {
            if (Indicator != null) Indicator.gameObject.SetActive(false);
        }

        public void ShowIndicator(float Radius, float Range)
        {
            HideIndicator();

            Indicator.gameObject.SetActive(true);
            Indicator.SetScale(Radius);
            Indicator.Range = Range;
        }

        public Vector3 Get3DMousePosition()
        {
            RaycastHit hit;
            return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 300.0f, MouseRaycast) ? hit.point : Vector3.zero;
        }

        public Vector3 GetIndicatorPosition()
        {
            return Indicator != null ? Indicator.transform.position : Get3DMousePosition();
        }
    }
}