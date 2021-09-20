using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.Utility;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class ScreenTextDisplayManager : MonoBehaviour
    {
        [Serializable]
        public class ScreenTextData
        {
            public ScreenTextObject FloatingPanel;
            public GameObject ScreenTextGO;
            public Renderer RendererReference;
            public float LastTimeUsed;
            public GameObject nodeGO;
        }

        public List<ScreenTextData> AllScreenTextDATA;

        public int CombatTextPoolAmount = 50;
        public ScreenTextObject panelPrefab;
        private Camera cam;

        // Start is called before the first frame update
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;

            for (var i = 0; i < CombatTextPoolAmount; i++)
            {
                var textPanel = Instantiate(panelPrefab, transform, false);
                textPanel.transform.localScale = Vector3.one;

                var newCombatText = new ScreenTextData();
                newCombatText.FloatingPanel = textPanel;
                newCombatText.ScreenTextGO = textPanel.gameObject;
                newCombatText.ScreenTextGO.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;

                AllScreenTextDATA.Add(newCombatText);
                AllScreenTextDATA[i].ScreenTextGO.SetActive(false);
            }

            cam = Camera.main;
        }


        public static ScreenTextDisplayManager Instance { get; private set; }

        public void ScreenEventHandler(string eventType, string message, string blockedDmg, GameObject target)
        {
            var weFoundOne = false;
            for (var i = 0; i < AllScreenTextDATA.Count; i++)
                if (!AllScreenTextDATA[i].ScreenTextGO.activeInHierarchy)
                {
                    AllScreenTextDATA[i].ScreenTextGO.SetActive(true);
                    AllScreenTextDATA[i].LastTimeUsed = Time.time;
                    AllScreenTextDATA[i].RendererReference = target.GetComponentInChildren<Renderer>();
                    AllScreenTextDATA[i].nodeGO = target;
                    AllScreenTextDATA[i].FloatingPanel.FloatingPanelController = this;
                    AllScreenTextDATA[i].FloatingPanel.ThisPanelID = i;
                    AllScreenTextDATA[i].FloatingPanel.SetMobDetails(target);
                    AllScreenTextDATA[i].FloatingPanel.ShowCombatText(message, eventType, blockedDmg);
                    weFoundOne = true;
                    break;
                }

            if (weFoundOne) return;
            var TextToResetID = GetCombatTextToReset();

            AllScreenTextDATA[TextToResetID].ScreenTextGO.SetActive(true);
            AllScreenTextDATA[TextToResetID].LastTimeUsed = Time.time;
            AllScreenTextDATA[TextToResetID].RendererReference = target.GetComponentInChildren<Renderer>();
            AllScreenTextDATA[TextToResetID].nodeGO = target;
            AllScreenTextDATA[TextToResetID].FloatingPanel.FloatingPanelController = this;
            AllScreenTextDATA[TextToResetID].FloatingPanel.ThisPanelID = TextToResetID;
            AllScreenTextDATA[TextToResetID].FloatingPanel.SetMobDetails(target);
            AllScreenTextDATA[TextToResetID].FloatingPanel.ShowCombatText(message, eventType, blockedDmg);
        }

        private int GetCombatTextToReset()
        {
            var allTimes = new float[AllScreenTextDATA.Count];

            for (var i = 0; i < allTimes.Length; i++) allTimes[i] = AllScreenTextDATA[i].LastTimeUsed;

            var LongestTime = Mathf.Min(allTimes);
            var PanelID = Array.IndexOf(allTimes, LongestTime);

            return PanelID;
        }

        // Update is called once per frame
        private void Update()
        {
            if (cam == null)
                cam = Camera.main;
            foreach (var t in AllScreenTextDATA)
                if (t.ScreenTextGO.activeInHierarchy)
                {
                    if (t.nodeGO == null)
                    {
                        t.nodeGO = null;
                        t.RendererReference = null;
                        t.LastTimeUsed = 0;
                        t.ScreenTextGO.SetActive(false);
                        t.LastTimeUsed = Time.time;
                        t.RendererReference = null;
                        t.FloatingPanel.FloatingPanelController = null;
                        break;
                    }

                    var distance = Vector3.Distance(t.nodeGO.transform.position, cam.transform.position);

                    if (distance > t.FloatingPanel.renderDistance)
                    {
                        if (t.ScreenTextGO.activeSelf)
                            t.ScreenTextGO.SetActive(false);

                        // TODO RESET COMBAT TEXT
                    }
                    else
                    {
                        if (t.RendererReference != null)
                            if (t.RendererReference.isVisible ||
                                !t.RendererReference.IsVisibleFrom(cam))
                            {
                                var rendererVisible = t.RendererReference.isVisible &&
                                                      t.RendererReference.IsVisibleFrom(cam);


                                if (rendererVisible)
                                {
                                    var screenPoint =
                                        cam.WorldToViewportPoint(t.nodeGO.transform.position);
                                    rendererVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 &&
                                                      screenPoint.y > 0 && screenPoint.y < 1;
                                }

                                if (!rendererVisible)
                                {
                                    if (t.ScreenTextGO.activeSelf)
                                        t.ScreenTextGO.SetActive(false);

                                    // TODO RESET COMBAT TEXT
                                    continue;
                                }
                            }

                        if (!t.ScreenTextGO.gameObject.activeSelf)
                            t.ScreenTextGO.SetActive(true);
                        t.FloatingPanel.RunUpdate(cam);
                    }
                }
        }
    }
}