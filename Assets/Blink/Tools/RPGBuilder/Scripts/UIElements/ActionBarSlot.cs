using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionBarSlot : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
{
    public CharacterData.ActionBarType actionBarType;
    public string actionKeyName;
    private int slotIndex;
    public int SlotIndex
    {
        get => slotIndex;
        set => slotIndex = value;
    }
    public CharacterData.ActionBarSlotContentType contentType;

    public bool acceptAbilities = true, acceptItems = true;
    
    public Image icon, background, cooldownOverlay, toggledOverlay;
    public TextMeshProUGUI cooldownText, keyText, stackText;
    
    private RPGAbility thisAb;
    public RPGAbility ThisAbility
    {
        get => thisAb;
        set => thisAb = value;
    }
    private RPGItem thisItem;
    public RPGItem ThisItem
    {
        get => thisItem;
        set => thisItem = value;
    }

    private GameObject curDraggedSlot;
    
    public bool dragAllowed = true;

    public void Init(RPGAbility ab)
    {
        contentType = CharacterData.ActionBarSlotContentType.Ability;
        thisItem = null;
        thisAb = ab;
        cooldownText.enabled = true;
        cooldownText.text = "";
        cooldownOverlay.fillAmount = 0;
        icon.enabled = true;
        icon.sprite = thisAb.icon;
        background.enabled = false;
        stackText.enabled = false;
        UpdateKeyText();
        
    }
    public void Init(RPGItem item)
    {
        contentType = CharacterData.ActionBarSlotContentType.Item;
        thisAb = null;
        thisItem = item;
        cooldownText.enabled = true;
        cooldownText.text = "";
        cooldownOverlay.fillAmount = 0;
        icon.enabled = true;
        icon.sprite = thisItem.icon;
        background.enabled = true;
        
        Sprite itemQualitySprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
        if (itemQualitySprite != null)
        {
            background.enabled = true;
            background.sprite = RPGBuilderUtilities.getItemRaritySprite(item.rarity);
        }
        else
        {
            background.enabled = false;
        }
        
        int ttlCount = InventoryManager.Instance.getTotalCountOfItem(item);
        UpdateSlot(ttlCount);
        UpdateKeyText();
    }

    private void UpdateKeyText()
    {
        keyText.text = RPGBuilderUtilities.GetKeybindText(RPGBuilderUtilities.GetCurrentKeyByActionKeyName(actionKeyName));
    }
    
    public void UpdateSlot(int ttlStack)
    {
        stackText.enabled = true;
        stackText.text = ttlStack.ToString();
    }

    public void Reset()
    {
        contentType = CharacterData.ActionBarSlotContentType.None;
        thisAb = null;
        thisItem = null;
        
        UpdateKeyText();
    }
    
    public void ClickUseSlot()
    {
        switch (contentType)
        {
            case CharacterData.ActionBarSlotContentType.Ability:
                CombatManager.Instance.InitAbility(CombatManager.playerCombatNode, thisAb, true);
                AbilityTooltip.Instance.Hide();
                break;
            case CharacterData.ActionBarSlotContentType.Item:
                InventoryManager.Instance.UseItemFromBar(thisItem);
                break;
        }
    }

    public void ShowTooltip()
    {
        switch (contentType)
        {
            case CharacterData.ActionBarSlotContentType.Ability:
                AbilityTooltip.Instance.Show(thisAb);
                break;
            case CharacterData.ActionBarSlotContentType.Item:
                ItemTooltip.Instance.Show(thisItem.ID, -1, false);
                break;
        }
    }

    public void HideTooltip()
    {
        switch (contentType)
        {
            case CharacterData.ActionBarSlotContentType.Ability:
                AbilityTooltip.Instance.Hide();
                break;
            case CharacterData.ActionBarSlotContentType.Item:
                ItemTooltip.Instance.Hide();
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dragAllowed) return;
        if (curDraggedSlot != null) Destroy(curDraggedSlot);
        if (contentType == CharacterData.ActionBarSlotContentType.None) return;
        curDraggedSlot = Instantiate(TreesDisplayManager.Instance.draggedNodeImage, transform.position,
            Quaternion.identity);
        curDraggedSlot.transform.SetParent(TreesDisplayManager.Instance.draggedNodeParent);

        switch (contentType)
        {
            case CharacterData.ActionBarSlotContentType.Ability:
                curDraggedSlot.GetComponent<Image>().sprite = thisAb.icon;
                break;
            case CharacterData.ActionBarSlotContentType.Item:
                curDraggedSlot.GetComponent<Image>().sprite = thisItem.icon;
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (curDraggedSlot == null) return;
        if (contentType == CharacterData.ActionBarSlotContentType.None) return;
        curDraggedSlot.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (curDraggedSlot == null) return;
        if (contentType == CharacterData.ActionBarSlotContentType.None) return;
        for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                Input.mousePosition)) continue;
            
            switch (contentType)
            {
                case CharacterData.ActionBarSlotContentType.Ability when !ActionBarManager.Instance.actionBarSlots[i].acceptAbilities:
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This action bar slot do not accept abilities", 3);
                    Destroy(curDraggedSlot);
                    return;
                case CharacterData.ActionBarSlotContentType.Item when !ActionBarManager.Instance.actionBarSlots[i].acceptItems:
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("This action bar slot do not accept items", 3);
                    Destroy(curDraggedSlot);
                    return;
            }

            switch (ActionBarManager.Instance.actionBarSlots[i].contentType)
            {
                case CharacterData.ActionBarSlotContentType.None:
                    ActionBarManager.Instance.HandleSlotSetup(contentType, thisItem, thisAb, i);
                    ActionBarManager.Instance.ResetActionSlot(slotIndex, true);
                    break;
                case CharacterData.ActionBarSlotContentType.Ability:
                    RPGItem cachedItem = ActionBarManager.Instance.actionBarSlots[i].ThisItem;
                    RPGAbility cachedAbility = ActionBarManager.Instance.actionBarSlots[i].ThisAbility;
                    CharacterData.ActionBarSlotContentType cachedContentType =
                        ActionBarManager.Instance.actionBarSlots[i].contentType;
                    ActionBarManager.Instance.HandleSlotSetup(contentType, thisItem, thisAb, i);
                    ActionBarManager.Instance.HandleSlotSetup(cachedContentType, cachedItem,
                        cachedAbility, slotIndex);
                    break;
                case CharacterData.ActionBarSlotContentType.Item:
                    break;
            }

            Destroy(curDraggedSlot);
            return;
        }
        
        ActionBarManager.Instance.ResetActionSlot(slotIndex, true);
        Destroy(curDraggedSlot);
    }

    public void OnDrop(PointerEventData eventData)
    {
    }
}
