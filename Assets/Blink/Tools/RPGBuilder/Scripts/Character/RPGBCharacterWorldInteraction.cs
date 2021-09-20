using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class RPGBCharacterWorldInteraction : MonoBehaviour
{
    public float maxDistance = 5;
    private Camera cachedCamera;

    private int interactableMask;
    private void Start()
    {
        cachedCamera = Camera.main;
        interactableMask = 1 << RPGBuilderEssentials.Instance.generalSettings.worldInteractableLayer; 
    }

    private void FixedUpdate()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        if (!Physics.Raycast(ray, out var hit, maxDistance + Vector3.Distance(transform.position, cachedCamera.transform.position), interactableMask))
        {
            if (WorldInteractableDisplayManager.Instance.IsVisible() && canHideInteractable())
            {
                WorldInteractableDisplayManager.Instance.Hide();
            }
            return;
        }
        if (hit.transform.gameObject.layer != RPGBuilderEssentials.Instance.generalSettings.worldInteractableLayer) return;
        var interactable = hit.transform.gameObject.GetComponent<IPlayerInteractable>();
        if (interactable == null) return;
        if (!interactable.isReadyToInteract()) return;
        interactable.ShowInteractableUI();
    }

    private bool canHideInteractable()
    {
        return !CombatManager.playerCombatNode.isInteractiveNodeCasting;
    }
}
