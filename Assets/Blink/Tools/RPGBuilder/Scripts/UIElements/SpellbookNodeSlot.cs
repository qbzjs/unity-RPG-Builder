using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellbookNodeSlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
{
    public Image icon, Background;
    public TextMeshProUGUI nodeName, levelRequired;
    
    public RPGAbility thisAbility;
    public RPGBonus thisBonus;
    
    private GameObject curDraggedAbility;
    
    public void ShowTooltip()
    {
        if(thisAbility!=null)AbilityTooltip.Instance.Show(thisAbility);
        if(thisBonus!=null)AbilityTooltip.Instance.ShowBonus(thisBonus); 
    }

    public void HideTooltip()
    {
        AbilityTooltip.Instance.Hide();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thisAbility == null) return;
        if(curDraggedAbility != null) Destroy(curDraggedAbility);
        if (!RPGBuilderUtilities.isAbilityKnown(thisAbility.ID)) return;
        curDraggedAbility = Instantiate(TreesDisplayManager.Instance.draggedNodeImage, transform.position,
            Quaternion.identity);
        curDraggedAbility.transform.SetParent(TreesDisplayManager.Instance.draggedNodeParent);
        curDraggedAbility.GetComponent<Image>().sprite = thisAbility.icon;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (thisAbility == null || curDraggedAbility == null) return;
        curDraggedAbility.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (thisAbility == null || curDraggedAbility == null) return;
        for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                Input.mousePosition)) continue;
            if (ActionBarManager.Instance.actionBarSlots[i].acceptAbilities)
            {
                ActionBarManager.Instance.SetAbilityToSlot(thisAbility, i);
            }
            else
            {
                ErrorEventsDisplayManager.Instance.ShowErrorEvent("This action bar slot do not accept abilities", 3);
            }
        }

        Destroy(curDraggedAbility);
    }

    public void OnDrop(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }
}
