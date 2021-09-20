using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class SocketingPanelDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;
        private bool showing = false;

        public List<SocketSlotHolder> curSocketSlots = new List<SocketSlotHolder>();
        public GameObject socketSlotPrefab, itemSlotPrefab;
        public Transform socketSlotsParent, socketedItemParent;
        
        public class CurrentSocketedItemDATA
        {
            public RPGItem item;
            public int itemDataID = -1;
            public GameObject socketedItemGO;
            
            public class SocketsData
            {
                public RPGItem gemItemRef;
                public bool isSlotted;
                public string socketType;
            }
            public List<SocketsData> socketsData = new List<SocketsData>();
        }

        public CurrentSocketedItemDATA curSocketedItemData = new CurrentSocketedItemDATA();
        
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static SocketingPanelDisplayManager Instance { get; private set; }
        
        private void ClearAllSocketSlots()
        {
            foreach (var t in curSocketSlots) Destroy(t.gameObject);
            curSocketSlots.Clear();
        }
        
        public void SetGemItemInSocketSlot(SocketSlotHolder socketSlot, RPGItem gemItemREF)
        {
            if (gemItemREF.gemData.socketType != socketSlot.thisSocketType)
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("This cannot be slotted in this socket", 3);
                return;
            }
            
            if(socketSlot.curGemItemGO!=null) Destroy(socketSlot.curGemItemGO);
            socketSlot.curGemItemGO = Instantiate(itemSlotPrefab, socketSlot.gemItemParent);
            var slotREF = socketSlot.curGemItemGO.GetComponent<EnchantCostSlotHolder>();
            var itemREF = RPGBuilderUtilities.GetItemFromID(gemItemREF.ID);
            slotREF.InitSlot(itemREF.icon, true, 0, itemREF, false, -1);
            
            curSocketedItemData.socketsData[getSocketSlotIndex(socketSlot)].gemItemRef = gemItemREF;
        }

        int getSocketSlotIndex(SocketSlotHolder socketSlot)
        {
            for (int i = 0; i < curSocketSlots.Count; i++)
            {
                if (socketSlot == curSocketSlots[i]) return i;
            }

            return -1;
        }
        
        public void AssignSocketedItem(RPGItem item, int itemDataID)
        {
            if(item.itemType != "ARMOR" && item.itemType != "WEAPON") return;
            Destroy(curSocketedItemData.socketedItemGO);
            var socketedItemSlot = Instantiate(itemSlotPrefab, socketedItemParent);
            var slotREF = socketedItemSlot.GetComponent<EnchantCostSlotHolder>();
            var itemREF = RPGBuilderUtilities.GetItemFromID(item.ID);
            slotREF.InitSlot(itemREF.icon, true, 0, itemREF, false, itemDataID);
            
            curSocketedItemData.item = item;
            curSocketedItemData.itemDataID = itemDataID;
            curSocketedItemData.socketedItemGO = socketedItemSlot;
            
            refreshCurSocketedItemSocketsData(itemDataID);
            
            DisplaySocketView();
        }

        void refreshCurSocketedItemSocketsData(int itemDataID)
        {
            curSocketedItemData.socketsData.Clear();
            CharacterData.ItemDATA itemDataRef = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            foreach (var socket in itemDataRef.sockets)
            {
                CurrentSocketedItemDATA.SocketsData newSocketData = new CurrentSocketedItemDATA.SocketsData();
                newSocketData.socketType = socket.socketType;
                newSocketData.isSlotted = socket.gemItemID != -1;
                if(newSocketData.isSlotted)newSocketData.gemItemRef = RPGBuilderUtilities.GetItemFromID(socket.gemItemID);
                curSocketedItemData.socketsData.Add(newSocketData);
            }
        }
        
        public void ClickSocket()
        {
            if (curSocketedItemData.socketsData.Count == 0) return;
            for (var index = 0; index < curSocketedItemData.socketsData.Count; index++)
            {
                var socket = curSocketedItemData.socketsData[index];
                if (socket.gemItemRef == null) continue;
                if(socket.isSlotted && socket.gemItemRef.ID == RPGBuilderUtilities.GetItemDataFromDataID(curSocketedItemData.itemDataID).sockets[index].gemItemID) continue;
                
                if (RPGBuilderUtilities.getItemCount(socket.gemItemRef) > 0)
                {
                    InventoryManager.Instance.RemoveItem(socket.gemItemRef.ID, 1, -1, -1, false);
                    CharacterData.Instance
                        .itemsDATA[RPGBuilderUtilities.GetItemDataIndexFromDataID(curSocketedItemData.itemDataID)]
                        .sockets[index].gemItemID = socket.gemItemRef.ID;
                }
                else
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("Some gems are missing from the inventory", 3);
                }
            }
            
            refreshCurSocketedItemSocketsData(curSocketedItemData.itemDataID);
            ResetSocketData();
            DisplaySocketView();
        }

        private void DisplaySocketView()
        {
            ClearAllSocketSlots();

            if (curSocketedItemData.item == null) return;
            CharacterData.ItemDATA itemDataRef =
                RPGBuilderUtilities.GetItemDataFromDataID(curSocketedItemData.itemDataID);
            if (itemDataRef == null) return;
            foreach (var socket in curSocketedItemData.socketsData)
            {
                var newSocketSlot = Instantiate(socketSlotPrefab, socketSlotsParent);
                var slotREF = newSocketSlot.GetComponent<SocketSlotHolder>();
                curSocketSlots.Add(slotREF);
                slotREF.Init(socket.socketType, socket.gemItemRef);
            }
        }


        public void Show()
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            
            DisplaySocketView();
            showing = true;
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void Hide()
        {
            ResetSocketData();
            
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            showing = false;
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void ResetSocketData()
        {
            Destroy(curSocketedItemData.socketedItemGO);
            curSocketedItemData.itemDataID = -1;
            curSocketedItemData.item = null;
            curSocketedItemData.socketsData.Clear();
            ClearAllSocketSlots();
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
    }
}
