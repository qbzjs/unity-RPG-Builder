using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class GetItemSlot : MonoBehaviour
    {
        public RPGItem thisitem;
        public Image icon;


        public void GetItem()
        {
            DevUIManager.Instance.GetItem(thisitem);
        }

        public void ShowTooltip()
        {
            ItemTooltip.Instance.Show(thisitem.ID, -1, false);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }
    }
}