using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

public class DemoSceneDisclaimer : MonoBehaviour
{
    private void Start()
    {
        if (RPGBuilderEssentials.Instance != null)
        {
            Destroy(gameObject);
        }
    }
}
