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
    public class ScreenSpaceWorldDroppedItems : MonoBehaviour
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

        [Serializable]
        public class WorldDroppedItemsUIDATA
        {
            public GameObject NameplateGO;
            public WorldDroppedItemDataHolder dataHolder;

            public Renderer RendererReference;
            public float LastTimeUsed;
            public RectTransform panelRectTransform;
            public float PosYOffset;
            public GameObject thisItemGO;
            public float VisibleSince, TimeToDisapear = 10;

            public bool Used;
        }
        public List<WorldDroppedItemsUIDATA> ALLWorldDroppedItemsUI;


        public GameObject NameplatePrefab;
        
        private Camera mainCamera;

        public static ScreenSpaceWorldDroppedItems Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;

            for (var i = 0; i < PoolSize; i++)
            {
                var NP = Instantiate(NameplatePrefab, Vector3.zero, NameplatePrefab.transform.rotation,
                    NameplatesParent);
                NP.transform.localPosition = Vector3.zero;

                var thisNP = new WorldDroppedItemsUIDATA();

                thisNP.NameplateGO = NP;
                thisNP.LastTimeUsed = 0;
                thisNP.panelRectTransform = NP.GetComponent<RectTransform>();
                thisNP.dataHolder = NP.GetComponent<WorldDroppedItemDataHolder>();
                ALLWorldDroppedItemsUI.Add(thisNP);
                NP.SetActive(false);
            }
            
            canvasRectTransform = thisCanvas.transform as RectTransform;
        }

        public void InitCamera()
        {
            mainCamera = Camera.main;
        }

        

        private void ResetANP(GameObject itemGOToReset)
        {
            foreach (var t in ALLWorldDroppedItemsUI.Where(t => t.Used).Where(t => t.thisItemGO == itemGOToReset))
                t.dataHolder.ResetThisNameplate();
        }

        public void ResetThisNP(GameObject itemGOToReset)
        {
            foreach (var t in ALLWorldDroppedItemsUI.Where(t => t.Used).Where(t => t.thisItemGO == itemGOToReset))
            {
                t.Used = false;
                t.RendererReference = null;
                t.PosYOffset = 0;
                t.thisItemGO = null;
                t.VisibleSince = 0;
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

        private void FixedUpdate()
        {
            if (mainCamera == null) return;
            if (CombatManager.playerCombatNode == null) return;
            foreach (var t in ALLWorldDroppedItemsUI)
            {
                if (!t.Used) continue;
                if (t.thisItemGO == null)
                {
                    ResetANP(t.thisItemGO);
                    return;
                }

                var distance = Vector3.Distance(t.thisItemGO.transform.position,
                    CombatManager.playerCombatNode.transform.position);
                if (distance > 50)
                {
                    if (t.NameplateGO.activeSelf)
                    {
                        t.NameplateGO.SetActive(false);
                    }
                }
                else
                {
                    if (t.RendererReference != null)
                    {
                        if (t.RendererReference.isVisible || !t.RendererReference.IsVisibleFrom(mainCamera))
                        {
                            var rendererVisible = false;

                            var screenPoint =
                                mainCamera.WorldToViewportPoint(t.thisItemGO.transform.position);
                            rendererVisible = screenPoint.z > SPZPositive && screenPoint.x > SPXMin &&
                                              screenPoint.x < SPXMax && screenPoint.y > SPYMin &&
                                              screenPoint.y < SPYMax;

                            Vector3 worldPos;
                            if (rendererVisible)
                            {
                                var position = t.thisItemGO.transform.position;
                                var distanceApart = getSqrDistance(position, mainCamera.transform.position);

                                var lerp = mapValue(distanceApart, 0, 2500, 0f, 1f);

                                float LerpPosY = Mathf.Lerp(0.3f, 2.6f, lerp);

                                worldPos = new Vector3(position.x, position.y + (t.PosYOffset + LerpPosY),
                                    position.z);

                                var screenPos = mainCamera.WorldToScreenPoint(worldPos);
                                t.NameplateGO.transform.position =
                                    new Vector3(screenPos.x, screenPos.y, screenPos.z);

                            }
                            else
                            {
                                if (t.NameplateGO.activeSelf)
                                {
                                    t.NameplateGO.SetActive(false);
                                }

                                continue;
                            }
                        }
                    }

                    if (!t.NameplateGO.gameObject.activeSelf)
                    {
                        t.NameplateGO.SetActive(true);
                    }
                }
            }
        }

        public void RegisterNewNameplate(Renderer thisRenderRef, GameObject thisItem, RPGItem itemREF)
        {
            if (thisItem == null)
                return;
            foreach (var t in ALLWorldDroppedItemsUI.Where(t => !t.Used).Where(t => !t.NameplateGO.activeInHierarchy))
            {
                t.Used = true;
                t.NameplateGO.SetActive(true);
                t.RendererReference = thisRenderRef;
                t.PosYOffset = 0.3f;
                t.thisItemGO = thisItem;

                t.dataHolder.InitializeThisNameplate(thisItem, itemREF);
                break;
            }
        }
    }
}