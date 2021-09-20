using System.Collections.Generic;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class InventoryDisplayManager : MonoBehaviour, IDisplayPanel
    {
        private bool showing;
        public CanvasGroup thisCG;

        public GameObject slotPrefab, itemSlotPrefab;

        private readonly List<ItemSlotHolder> currentSlots = new List<ItemSlotHolder>();
        public List<CurrencyDisplaySlotHolder> allCurrencySlots = new List<CurrencyDisplaySlotHolder>();

        public List<RectTransform> allSlots = new List<RectTransform>();
        public Transform slotsParent;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        
        public void InitInventory()
        {
            if(allSlots.Count > 0) return;
            for (var i = 0; i < CharacterData.Instance.inventoryData.baseSlots.Count; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotsParent);
                allSlots.Add(newSlot.GetComponent<RectTransform>());
            }
        }

        public void ResetSlots()
        {
            foreach (var t in allSlots)
            {
                Destroy(t.gameObject);
            }
            allSlots.Clear();
        }

        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            UpdateSlots();
            UpdateCurrency();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }

        private void ClearSlots()
        {
            foreach (var t in currentSlots)
            {
                t.ClearDraggedSlot();
                Destroy(t.gameObject);
            }

            currentSlots.Clear();
        }

        public void UpdateCurrency()
        {
            foreach (var t in allCurrencySlots)
                t.UpdateCurrencySlot();
        }

        public void UpdateSlots()
        {
            ClearSlots();

            for (int i = 0; i < CharacterData.Instance.inventoryData.baseSlots.Count; i++)
            {
                if (CharacterData.Instance.inventoryData.baseSlots[i].itemID == -1) continue;
                var newSlot = Instantiate(itemSlotPrefab, allSlots[i].transform);
                var newSlotHolder = newSlot.GetComponent<ItemSlotHolder>();
                newSlotHolder.InitSlot(RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.inventoryData.baseSlots[i].itemID), -1, i);
                currentSlots.Add(newSlotHolder);
            }
        }

        public static InventoryDisplayManager Instance { get; private set; }
    }
}