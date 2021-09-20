using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class MerchantPanelDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;

        public GameObject merchantItemSlotPrefab;

        public Transform merchantItemsSlotsParent;
        public List<GameObject> currentMerchantItemSlots = new List<GameObject>();

        private CombatNode currentMerchantNode;
        private bool isShowing;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static MerchantPanelDisplayManager Instance { get; private set; }


        private void ClearAllMerchantItemsSlots()
        {
            foreach (var t in currentMerchantItemSlots)
                Destroy(t);

            currentMerchantItemSlots.Clear();
        }


        private void InitializeMerchantPanel(RPGNpc npc)
        {
            ClearAllMerchantItemsSlots();
            var merchantTableREF = RPGBuilderUtilities.GetMerchantTableFromID(npc.merchantTableID);
            foreach (var t in merchantTableREF.onSaleItems)
            {
                var newItemSlot = Instantiate(merchantItemSlotPrefab, merchantItemsSlotsParent);
                var holder = newItemSlot.GetComponent<MerchantItemSlotHolder>();
                holder.Init(RPGBuilderUtilities.GetItemFromID(t.itemID),
                    RPGBuilderUtilities.GetCurrencyFromID(t.currencyID),
                    t.cost);
                currentMerchantItemSlots.Add(newItemSlot);
            }
        }

        public void Show(CombatNode cbtNode)
        {
            currentMerchantNode = cbtNode;
            Show();
            InitializeMerchantPanel(cbtNode.npcDATA);
        }

        public void Show()
        {
            isShowing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(true);
        }

        public void Hide()
        {
            isShowing = false;
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!isShowing || currentMerchantNode == null) return;
            if(Vector3.Distance(currentMerchantNode.transform.position, CombatManager.playerCombatNode.transform.position) > 4) Hide();
        }
    }
}