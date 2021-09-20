using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.UIElements
{
    public class NPCSpawnSlotHolder : MonoBehaviour
    {
   
        public RPGNpc thisNPC;
        public Image icon;
        public TextMeshProUGUI nameText;

        public void SpawnNPC()
        {
            DevUIManager.Instance.SpawnNPC(thisNPC);
        }
    }
}
