using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.UIElements
{
    public class LootBagHolder : MonoBehaviour, IPlayerInteractable
    {
        [Serializable]
        public class Loot_Data
        {
            public RPGItem item;
            public int count;
            public bool looted;
            public int itemDataID = -1;
        }

        public List<Loot_Data> lootData = new List<Loot_Data>();
        public string lootBagName;

        public void CheckLootState()
        {
            var nonLootedItem = 0;
            foreach (var t in lootData)
            {
                if (!t.looted) nonLootedItem++;
            }
            if (nonLootedItem == 0)
            {
                LootPanelDisplayManager.Instance.Hide();
                LootPanelDisplayManager.Instance.ClearAllLootItemSlots();
                Destroy(gameObject);
            }
            else
            {
                if (LootPanelDisplayManager.Instance.thisCG.alpha == 1)
                {
                    LootPanelDisplayManager.Instance.DisplayLoot(this);
                }
            }
        }


        private void OnMouseOver()
        {
            if (!Input.GetMouseButtonUp(1)) return;
            if (Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) < 4)
            {
                LootPanelDisplayManager.Instance.DisplayLoot(this);
            }
            else
            {
                if (CombatManager.playerCombatNode.playerControllerEssentials.GETControllerType() ==
                    RPGGeneralDATA.ControllerTypes.TopDownClickToMove)
                {
                }
                else
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This is too far", 3);
                }
            }
        }
        
        public void Interact()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return;
            if (!(Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) <= 3)) return;
            LootPanelDisplayManager.Instance.DisplayLoot(this);
        }

        public void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + 1.5f, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return lootBagName;
        }

        public bool isReadyToInteract()
        {
            return true;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.LootBag;
        }
    }
}