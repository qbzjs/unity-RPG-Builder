using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenOverlayManager : MonoBehaviour
{
    [System.Serializable]
    public class ScreenOverlay
    {
        public string overlayName;
        public GameObject go;
    }

    public List<ScreenOverlay> overlays = new List<ScreenOverlay>();
    
    
    public static ScreenOverlayManager Instance { get; private set; }
    
    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void ShowOverlay(string name)
    {
        foreach (var overlay in overlays)
        {
            if(overlay.overlayName != name) continue;
            overlay.go.SetActive(true);
        }
    }

    public void HideOverlay(string name)
    {
        foreach (var overlay in overlays)
        {
            if(overlay.overlayName != name) continue;
            overlay.go.SetActive(false);
        }
    }
    
}
