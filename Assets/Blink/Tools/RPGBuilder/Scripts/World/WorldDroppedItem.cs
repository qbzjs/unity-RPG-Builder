using System;
using System.Collections;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.World
{
    public class WorldDroppedItem : MonoBehaviour, IPlayerInteractable
    {
        public float curLifetime, maxDuration;
        public RPGItem item;
        
        private void FixedUpdate()
        {
            curLifetime += Time.deltaTime;
            if (curLifetime >= maxDuration)
            {
                InventoryManager.Instance.DestroyWorldDroppedItem(this);
            }
        }


        public void InitPhysics()
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            foreach (var t in CombatManager.Instance.allCombatNodes)
            {
                Physics.IgnoreCollision(collider,
                    t.nodeType == CombatNode.COMBAT_NODE_TYPE.player
                        ? t.gameObject.GetComponent<CharacterController>()
                        : t.gameObject.GetComponent<Collider>());
            }

            rb.drag = 1;
            rb.AddRelativeForce(new Vector3(Random.Range(-5, 5), 4, Random.Range(-5, 5)), ForceMode.VelocityChange);
            rb.AddRelativeTorque(new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)), ForceMode.Impulse);
            ScreenSpaceWorldDroppedItems.Instance.RegisterNewNameplate(GetComponent<Renderer>(), gameObject, item);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != CombatManager.playerCombatNode.gameObject) return;
            
            InventoryManager.Instance.LootWorldDroppedItem(this);
        }
        
        public void Interact()
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return;
            if (!(Vector3.Distance(transform.position, CombatManager.playerCombatNode.transform.position) <= 3)) return;
            InventoryManager.Instance.LootWorldDroppedItem(this);
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
            return item.displayName;
        }

        public bool isReadyToInteract()
        {
            return true;
        }

        public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
        {
            return RPGCombatDATA.INTERACTABLE_TYPE.WorldDroppedItem;
        }
    }
}
