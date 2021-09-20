using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.World
{
    public class CraftingStation : MonoBehaviour, IPlayerInteractable
    {
        public RPGCraftingStation station;
        public float useDistanceMax;
        public float interactableUIoffsetY = 2;
        
        private void OnMouseOver()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject())
            {
                CursorManager.Instance.ResetCursor();
                return;
            }
            if (Input.GetMouseButtonUp(1))
                if (Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) <=
                    useDistanceMax)
                {
                    if (CraftingPanelDisplayManager.Instance.thisCG.alpha == 0)
                        InitCraftingStation();
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

            CursorManager.Instance.SetCursor(CursorManager.cursorType.craftingStation);
        }

        private void OnMouseExit()
        {
            CursorManager.Instance.ResetCursor();
        }

        private void InitCraftingStation()
        {
            CraftingPanelDisplayManager.Instance.Show(this);
        }

        public void Interact()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return;
            if (!(Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) <= useDistanceMax)) return;
            if (CraftingPanelDisplayManager.Instance.thisCG.alpha == 0)
                InitCraftingStation();
        }

        public void ShowInteractableUI()
        {
            var pos = transform;
            Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + interactableUIoffsetY, pos.position.z);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);
            
            WorldInteractableDisplayManager.Instance.Show(this);
        }

        public string getInteractableName()
        {
            return station.displayName;
        }

        public bool isReadyToInteract()
        {
            return true;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.CraftingStation;
        }
    }
}