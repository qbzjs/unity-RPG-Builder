using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class WeaponTemplatesDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG;
        private bool showing;

        public GameObject weaponSlotPrefab;
        public Transform weaponSlotParent;
        public List<GameObject> curWeaponSlots = new List<GameObject>();

        public TextMeshProUGUI weaponTemplateName, weaponDescriptionText, weaponLevelText, weaponExperienceText;
        public Image weaponExperienceBar;

        public GameObject treeSlotPrefab;
        public Transform treeSlotsParent;
        public List<GameObject> curTreeSlots = new List<GameObject>();

        private int curSelectedWeaponTemplate = -1;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static WeaponTemplatesDisplayManager Instance { get; private set; }

        
        private void ClearAllSkillSlots()
        {
            foreach (var t in curWeaponSlots)
                Destroy(t);

            curWeaponSlots.Clear();
        }

        private void ClearCurTreeSlots()
        {
            foreach (var t in curTreeSlots)
                Destroy(t);

            curTreeSlots.Clear();
        }

        public void InitWeaponList()
        {
            ClearAllSkillSlots();
            foreach (var t in CharacterData.Instance.weaponTemplates)
            {
                var newRecipeSlot = Instantiate(weaponSlotPrefab, weaponSlotParent);
                curWeaponSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<WeaponTemplateSlotHolder>();
                slotREF.InitSlot(RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID));
            }

            if (curSelectedWeaponTemplate == -1 && CharacterData.Instance.weaponTemplates.Count > 0)
            {
                SelectWeapon(CharacterData.Instance.weaponTemplates[0].weaponTemplateID);
            }
        }

        public void SelectWeapon(int weaponTemplateID)
        {
            curSelectedWeaponTemplate = weaponTemplateID;
            UpdateWeaponView();
        }

        public void UpdateWeaponView()
        {
            RPGWeaponTemplate weaponTemplateREF = RPGBuilderUtilities.GetWeaponTemplateFromID(curSelectedWeaponTemplate);
            weaponTemplateName.text = weaponTemplateREF.displayName;
            weaponDescriptionText.text = weaponTemplateREF.description;
            RPGLevelsTemplate levelTemplateREF =
                RPGBuilderUtilities.GetLevelTemplateFromID(weaponTemplateREF.levelTemplateID);
            weaponLevelText.text = RPGBuilderUtilities.getWeaponTemplateLevel(curSelectedWeaponTemplate) + " / " + levelTemplateREF.levels;
            weaponExperienceText.text = RPGBuilderUtilities.getWeaponTemplateCurEXP(curSelectedWeaponTemplate) + " / " + RPGBuilderUtilities.getWeaponTemplateMaxEXP(curSelectedWeaponTemplate);
            weaponExperienceBar.fillAmount = (float)((float)RPGBuilderUtilities.getWeaponTemplateCurEXP(curSelectedWeaponTemplate) / (float)RPGBuilderUtilities.getWeaponTemplateMaxEXP(curSelectedWeaponTemplate));
            
            ClearCurTreeSlots();
            
            foreach (var t in weaponTemplateREF.talentTrees)
            {
                var newTreeSlot = Instantiate(treeSlotPrefab, treeSlotsParent);
                curTreeSlots.Add(newTreeSlot);
                var slotREF2 = newTreeSlot.GetComponent<CombatTreeSlot>();
                slotREF2.InitSlot(RPGBuilderUtilities.GetTalentTreeFromID(t.talentTreeID));
            }
        }
        
        
        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            InitWeaponList();
            
            CharacterPanelDisplayManager.Instance.Hide();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
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
}
