using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class SpellbookDisplayManager : MonoBehaviour, IDisplayPanel
{
    public CanvasGroup thisCG;
    private bool showing = false;
    private RPGSpellbook selectedSpellbook;
    
    public List<SpellbookSlot> curSpellbookSlots = new List<SpellbookSlot>();
    public List<SpellbookNodeSlot> curSpellbookNodeSlots = new List<SpellbookNodeSlot>();
    public TextMeshProUGUI titleText;
    public GameObject spellbookSlotPrefab, spellbookNodePrefab;
    public Transform spellbookSlotParent, spellbookNodeParent;

    public Color unlockedColor, lockedColor;
    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public static SpellbookDisplayManager Instance { get; private set; }
        
    private void ClearAllSpellbookSlots()
    {
        foreach (var t in curSpellbookSlots) Destroy(t.gameObject);
        curSpellbookSlots.Clear();
    }
    private void ClearAllSpellbookNodeSlots()
    {
        foreach (var t in curSpellbookNodeSlots) Destroy(t.gameObject);
        curSpellbookNodeSlots.Clear();
    }

    void DisplaySpellbookView()
    {
        ClearAllSpellbookSlots();
        
        foreach (var spellbook in RPGBuilderUtilities.GetCharacterSpellbookList())
        {
            var newSpellbookSlot = Instantiate(spellbookSlotPrefab, spellbookSlotParent);
            var slotREF = newSpellbookSlot.GetComponent<SpellbookSlot>();
            curSpellbookSlots.Add(slotREF);

            slotREF.icon.sprite = spellbook.spellbook.icon;
            slotREF.thisSpellbook = spellbook.spellbook;
        }

        if (curSpellbookSlots.Count > 0)
        {
            selectedSpellbook = curSpellbookSlots[0].thisSpellbook;
            UpdateSpellbookView();
        }

    }

    public void UpdateSpellbookView()
    {
        if (selectedSpellbook == null) return;
        ClearAllSpellbookNodeSlots();

        if (titleText != null) titleText.text = selectedSpellbook.displayName;

        foreach (var node in selectedSpellbook.nodeList)
        {
            var newSpellbookSlot = Instantiate(spellbookNodePrefab, spellbookNodeParent);
            var slotREF = newSpellbookSlot.GetComponent<SpellbookNodeSlot>();
            curSpellbookNodeSlots.Add(slotREF);

            int unlockLevel = -1;
            if (node.nodeType == RPGSpellbook.SpellbookNodeType.ability)
            {
                RPGAbility abilityREF = RPGBuilderUtilities.GetAbilityFromID(node.abilityID);
                slotREF.icon.sprite = abilityREF.icon;
                slotREF.thisAbility = abilityREF;
                slotREF.nodeName.text = abilityREF.displayName;

                unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Ability_Level_Required, node.unlockLevel,
                    selectedSpellbook.ID, node.abilityID);
            }
            else
            {
                RPGBonus bonusREF = RPGBuilderUtilities.GetBonusFromID(node.bonusID);
                slotREF.icon.sprite = bonusREF.icon;
                slotREF.thisBonus = bonusREF;
                slotREF.nodeName.text = bonusREF.displayName;

                unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, node.unlockLevel,
                    selectedSpellbook.ID, node.bonusID);
            }


            slotREF.levelRequired.text = unlockLevel.ToString();

            int lvl = selectedSpellbook.sourceType == RPGSpellbook.spellbookSourceType._class
                ? CharacterData.Instance.classDATA.currentClassLevel
                : RPGBuilderUtilities.getWeaponTemplateLevel(selectedSpellbook.ID, selectedSpellbook);
            slotREF.levelRequired.color = lvl >= unlockLevel
                ? Color.green
                : Color.red;
            slotREF.Background.color = lvl >= unlockLevel
                ? unlockedColor
                : lockedColor;
        }
    }

    public void SelectSpellbook(RPGSpellbook spellbook)
    {
        selectedSpellbook = spellbook;
        
        UpdateSpellbookView();
    }


    public void Show()
    {
        RPGBuilderUtilities.EnableCG(thisCG);
        transform.SetAsLastSibling();
        CustomInputManager.Instance.AddOpenedPanel(thisCG);
            
        DisplaySpellbookView();
        showing = true;
        if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
    }

    public void Hide()
    {
        gameObject.transform.SetAsFirstSibling();
        RPGBuilderUtilities.DisableCG(thisCG);
        showing = false;
        if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
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
