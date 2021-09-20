using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace BLINK.RPGBuilder.UI
{
    public class ScreenSpaceNameplates : MonoBehaviour
    {
        public int PoolSize;
        public Transform NameplatesParent;

        public float SPZPositive, SPXMin, SPXMax, SPYMin, SPYMax;

        private RectTransform canvasRectTransform;

        private bool clampedToLeft;
        private bool clampedToRight;
        private bool clampedToTop;
        private bool clampedToBottom;

        public Canvas thisCanvas;

        public float FocusedScale, UnfocusedScale;

        [Serializable]
        public class NameplateData
        {
            public GameObject NameplateGO;
            public NameplatesDATAHolder dataHolder;
            public RectTransform TargetUnitAreaRectT;

            public GameObject UnitGO;
            public Renderer RendererReference;
            public float LastTimeUsed;
            public CombatNode CurrentNode;
            public RectTransform panelRectTransform;
            public float PosYOffset;
            public CombatNode thisCombatInfo;

            public float VisibleSince, TimeToDisapear = 10;

            public bool ShouldBeVisible;
            public bool IsLocalPlayer;
            public bool Focused;
            public bool Used;
            public bool IsUser;
        }

        public List<NameplateData> ALLNameplatesDATA;


        public GameObject NameplatePrefab;
        public GameObject statePrefab;

        public float LocalPlayerNPYOffset, DistanceNeededToUpdateLocalNameplate;
        private Camera mainCamera;

        public static ScreenSpaceNameplates Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;

            for (var i = 0; i < PoolSize; i++)
            {
                var NP = Instantiate(NameplatePrefab, Vector3.zero, NameplatePrefab.transform.rotation,
                    NameplatesParent);
                NP.transform.localPosition = Vector3.zero;

                var thisNP = new NameplateData();

                thisNP.NameplateGO = NP;
                thisNP.LastTimeUsed = 0;
                thisNP.panelRectTransform = NP.GetComponent<RectTransform>();
                thisNP.dataHolder = NP.GetComponent<NameplatesDATAHolder>();
                thisNP.TargetUnitAreaRectT = thisNP.dataHolder.TargetUnitAreaRectREF;
                ALLNameplatesDATA.Add(thisNP);
                NP.SetActive(false);
            }


            canvasRectTransform = thisCanvas.transform as RectTransform;
        }

        public void InitCamera()
        {
            mainCamera = Camera.main;
        }

        public void InitNameplateState(CombatNode unitOID, CombatNode.NodeStatesDATA nodeStateData)
        {
            foreach (var t in ALLNameplatesDATA.Where(t => t.Used && t.dataHolder.gameObject.activeInHierarchy).Where(t => t.thisCombatInfo == unitOID))
                t.dataHolder.AddState(nodeStateData, true);
        }

        public void UpdateNameplateState(CombatNode unitOID, CombatNode.NodeStatesDATA nodeStateData)
        {
            foreach (var t in ALLNameplatesDATA)
                if (t.Used)
                    if (t.thisCombatInfo == unitOID)
                        foreach (var t1 in t.dataHolder.statesList)
                            if (t1.stateData == nodeStateData)
                                t1.statesDisplay.InitializeState(
                                    t1.statesDisplay.border.color, nodeStateData,
                                    t.dataHolder, nodeStateData.stateMaxDuration);
        }

        public void RemoveState(CombatNode cbtNode, CombatNode.NodeStatesDATA nodeStateData)
        {
            foreach (var t in ALLNameplatesDATA.Where(t => t.Used).Where(t => t.thisCombatInfo == cbtNode))
                for (var x = 0; x < t.dataHolder.statesList.Count; x++)
                    if (t.dataHolder.statesList[x].stateData == nodeStateData)
                    {
                        Destroy(t.dataHolder.statesList[x].statesDisplay.gameObject);
                        t.dataHolder.statesList.RemoveAt(x);
                    }
        }

        private void ClearAllStates(NameplatesDATAHolder dataHolder)
        {
            if (dataHolder.statesList.Count == 0) return;
            for (var x = 0; x < dataHolder.statesList.Count; x++)
            {
                Destroy(dataHolder.statesList[x].statesDisplay.gameObject);
                dataHolder.statesList.RemoveAt(x);
            }
        }
        
        private void InitAllStates(NameplatesDATAHolder dataHolder)
        {
            CombatNode cbtNode = dataHolder.GetThisCombatNode();

            foreach (var t in cbtNode.nodeStateData)
            {
                foreach (var t1 in ALLNameplatesDATA.Where(t1 => t1.Used).Where(t1 => t1.thisCombatInfo == cbtNode))
                {
                    t1.dataHolder.AddState(t, false);
                }
            }
        }

        public void SetNPToVisible(CombatNode unitOID)
        {
            foreach (var t in ALLNameplatesDATA.Where(t => t.Used).Where(t => t.thisCombatInfo == unitOID))
            {
                t.ShouldBeVisible = true;
                t.VisibleSince = 0;
            }
        }

        public void SetNPToFocused(CombatNode unitOID)
        {
            foreach (var t in ALLNameplatesDATA.Where(t => t.Used))
            {
                if (t.thisCombatInfo == unitOID)
                {
                    t.Focused = true;
                    t.dataHolder.IsFocused = true;
                    t.dataHolder.transform.SetAsLastSibling();
                }
                else
                {
                    t.Focused = false;
                    t.dataHolder.IsFocused = false;
                }

                t.dataHolder.SetScale();
            }
        }

        public void UpdateNPBar(CombatNode unitOID)
        {
            foreach (var t in ALLNameplatesDATA)
                if (t.Used)
                {
                    if (t.dataHolder == null) continue;
                    if (t.thisCombatInfo == unitOID)
                        t.dataHolder.UpdateBar();
                }
        }


        public void InitACastBar(CombatNode unitOID, RPGAbility thisAB)
        {
            foreach (var t in ALLNameplatesDATA)
                if (t.Used)
                    if (t.thisCombatInfo == unitOID)
                        t.dataHolder.InitCasting(thisAB);
        }

        public void InitAChannelBar(CombatNode unitOID, RPGAbility thisAB)
        {
            foreach (var t in ALLNameplatesDATA)
                if (t.Used)
                    if (t.thisCombatInfo == unitOID)
                        t.dataHolder.InitChanneling(thisAB);
        }

        private void ResetANP(CombatNode OIDToReset)
        {
            foreach (var t in ALLNameplatesDATA.Where(t => t.Used).Where(t => t.thisCombatInfo == OIDToReset))
                t.dataHolder.ResetThisNameplate();
        }


        public void UpdateANPText(CombatNode OIDToUpdate)
        {
            foreach (var t in ALLNameplatesDATA)
                if (t.Used)
                    if (t.thisCombatInfo == OIDToUpdate)
                        t.dataHolder.UpdateTexts();
        }

        public void UpdateANPColors(CombatNode OIDToUpdate)
        {
            foreach (var t in ALLNameplatesDATA)
                if (t.Used)
                    if (t.thisCombatInfo == OIDToUpdate)
                        t.dataHolder.SetColors();
        }

        public void UpdateAllNameplateAfterFactionChange(RPGFaction factionREF)
        {
            foreach (var nameplate in ALLNameplatesDATA)
            {
                if(!nameplate.Used) continue;
                if(nameplate.CurrentNode == null) continue;
                if (nameplate.CurrentNode.npcDATA.factionID == factionREF.ID)
                {
                    nameplate.dataHolder.SetColors();
                }
            }
        }

        public void ResetThisNP(CombatNode OIDToReset)
        {
            foreach (var t in ALLNameplatesDATA.Where(t => t.Used).Where(t => t.thisCombatInfo == OIDToReset))
            {
                ClearAllStates(t.dataHolder);
                t.Used = false;
                t.TargetUnitAreaRectT = null;
                t.UnitGO = null;
                t.RendererReference = null;
                t.CurrentNode = null;
                t.PosYOffset = 0;
                t.thisCombatInfo = null;
                t.IsLocalPlayer = false;
                t.Focused = false;
                t.ShouldBeVisible = false;
                t.VisibleSince = 0;
                t.IsUser = false;
                t.dataHolder.isUser = false;
                t.dataHolder.ResetCastBar();
                if (t.NameplateGO != null) t.NameplateGO.SetActive(false);
            }
        }

        private float getSqrDistance(Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).sqrMagnitude;
        }

        private float mapValue(float mainValue, float inValueMin, float inValueMax, float outValueMin,
            float outValueMax)
        {
            return (mainValue - inValueMin) * (outValueMax - outValueMin) / (inValueMax - inValueMin) + outValueMin;
        }

        public void TriggerNameplateInteractionIconUpdate()
        {
            foreach (var t in ALLNameplatesDATA)
                if (t.Used && t.UnitGO != null)
                    if (t.CurrentNode.nodeType == CombatNode.COMBAT_NODE_TYPE.mob ||
                        t.CurrentNode.nodeType == CombatNode.COMBAT_NODE_TYPE.pet)
                        if (t.CurrentNode.npcDATA.npcType == RPGNpc.NPC_TYPE.QUEST_GIVER)
                            t.dataHolder.SetInteractionIcon();
        }

        private void FixedUpdate()
        {
            if (mainCamera == null) return;
            if (CombatManager.playerCombatNode == null) return;
            foreach (var t in ALLNameplatesDATA)
                if (t.Used)
                {
                    if (t.UnitGO == null)
                    {
                        ResetANP(t.thisCombatInfo);
                        return;
                    }

                    var distance = Vector3.Distance(t.UnitGO.transform.position,
                        CombatManager.playerCombatNode.transform.position);
                    if (distance > 50)
                    {
                        if (t.NameplateGO.activeSelf)
                        {
                            ClearAllStates(t.dataHolder);
                            t.NameplateGO.SetActive(false);
                        }
                    }
                    else
                    {
                        if (t.RendererReference != null)
                            if (t.RendererReference.isVisible ||
                                !t.RendererReference.IsVisibleFrom(mainCamera))
                            {
                                var rendererVisible = false;

                                var screenPoint =
                                    mainCamera.WorldToViewportPoint(t.UnitGO.transform.position);
                                rendererVisible = screenPoint.z > SPZPositive && screenPoint.x > SPXMin &&
                                                  screenPoint.x < SPXMax && screenPoint.y > SPYMin &&
                                                  screenPoint.y < SPYMax;

                                if (rendererVisible)
                                {
                                    Vector3 worldPos;
                                    if (!t.IsLocalPlayer)
                                    {
                                        var position = t.UnitGO.transform.position;
                                        var distanceApart = getSqrDistance(
                                            position,
                                            mainCamera.transform.position);

                                        var lerp = mapValue(distanceApart, 0, 2500, 0f, 1f);

                                        float LerpPosY = 0;
                                        float NewWidth = 0;
                                        float NewHeight = 0;
                                        float NewPOSY = 0;

                                        LerpPosY = t.Focused ? Mathf.Lerp(0.3f, 2.6f, lerp) : Mathf.Lerp(0, 0.7f, lerp);

                                        NewWidth = Mathf.Lerp(150, 40, lerp);
                                        NewHeight = Mathf.Lerp(210, 80, lerp);
                                        NewPOSY = Mathf.Lerp(-110, -45, lerp);

                                        worldPos = new Vector3(position.x,
                                            position.y +
                                            (t.PosYOffset + LerpPosY),
                                            position.z);


                                        var screenPos = mainCamera.WorldToScreenPoint(worldPos);
                                        t.NameplateGO.transform.position =
                                            new Vector3(screenPos.x, screenPos.y, screenPos.z);


                                        if (t.TargetUnitAreaRectT != null)
                                        {
                                            t.TargetUnitAreaRectT.sizeDelta =
                                                new Vector2(NewWidth, NewHeight);
                                            var localPosition = t.TargetUnitAreaRectT.localPosition;
                                            localPosition =
                                                new Vector3(localPosition.x,
                                                    NewPOSY, localPosition.z);
                                            t.TargetUnitAreaRectT.localPosition = localPosition;
                                        }
                                    }
                                    else
                                    {
                                        t.NameplateGO.transform.SetAsLastSibling();

                                        var position = t.UnitGO.transform.position;
                                        worldPos = new Vector3(position.x,
                                            position.y,
                                            position.z);

                                        var screenPos = mainCamera.WorldToScreenPoint(worldPos);
                                        var localPosition = t.NameplateGO.transform.localPosition;
                                        var previouspos = new Vector3(0,
                                            localPosition.y,
                                            localPosition.z);
                                        var newpos = new Vector3(0, screenPos.y, screenPos.z);

                                        var dist = Vector3.Distance(previouspos, newpos);
                                        if (dist >= DistanceNeededToUpdateLocalNameplate)
                                        {
                                            float posY;
                                            posY = 200;

                                            t.NameplateGO.transform.localPosition =
                                                new Vector3(0, posY, screenPos.z);
                                        }
                                    }

                                    if (!t.Focused)
                                    {
                                        if (distance <= 50)
                                        {
                                            if (t.dataHolder.thisUnitType !=
                                                NameplatesDATAHolder.NameplateUnitType.Neutral)
                                            {
                                                float maxIntensity = 1;
                                                float speed = 100;
                                                var intensity = (1 - distance / 50) * maxIntensity;

                                                t.dataHolder.PlayerNameCG.alpha =
                                                    Mathf.MoveTowards(
                                                        t.dataHolder.PlayerNameCG.alpha, intensity,
                                                        speed * Time.deltaTime);
                                            }
                                            else
                                            {
                                                t.dataHolder.PlayerNameCG.alpha = 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        t.dataHolder.PlayerNameCG.alpha = 1;
                                    }
                                }

                                if (!rendererVisible)
                                {
                                    if (t.NameplateGO.activeSelf)
                                    {
                                        ClearAllStates(t.dataHolder);
                                        t.NameplateGO.SetActive(false);
                                    }
                                    continue;
                                }
                            }


                        if (!t.NameplateGO.gameObject.activeSelf && !t.CurrentNode.dead)
                        {
                            InitAllStates(t.dataHolder);
                            t.NameplateGO.SetActive(true);
                        }
                    }

                    if (t.ShouldBeVisible)
                    {
                        t.dataHolder.BarCG.alpha = 1;
                        t.VisibleSince += Time.deltaTime;

                        if (!(t.VisibleSince >= t.TimeToDisapear)) continue;
                        if (!t.Focused)
                            t.ShouldBeVisible = false;
                    }
                    else
                    {
                        t.dataHolder.BarCG.alpha = 0;
                    }
                }
        }

        public void RegisterNewNameplate(Renderer thisRenderRef, CombatNode thisNode, GameObject thisUnitGO,
            float NameplateYOffset, bool IsLocalPlayer)
        {
            if (thisNode == null)
                return;
            foreach (var t in ALLNameplatesDATA.Where(t => !t.Used).Where(t => !t.NameplateGO.activeInHierarchy))
            {
                t.Used = true;
                t.NameplateGO.SetActive(true);
                t.UnitGO = thisUnitGO;
                t.RendererReference = thisRenderRef;
                t.CurrentNode = thisNode;
                t.PosYOffset = NameplateYOffset;
                t.thisCombatInfo = thisNode;
                t.IsLocalPlayer = IsLocalPlayer;
                t.Focused = false;
                if (thisNode.nodeType == CombatNode.COMBAT_NODE_TYPE.player)
                {
                    t.IsUser = true;
                    t.dataHolder.isUser = true;
                }
                else
                {
                    t.IsUser = false;
                    t.dataHolder.isUser = false;
                }

                t.dataHolder.InitializeThisNameplate(thisNode);
                break;
            }
        }
    }
}