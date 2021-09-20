using System.Collections.Generic;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class SkillBookDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG, SkillListCG, SkillInfoCG;
        private bool showing;

        public GameObject skillSlotPrefab;
        public Transform skillSlotsParent;
        public List<GameObject> curSkillSlots = new List<GameObject>();

        public TextMeshProUGUI skillNameText, skillDescriptionText, skillLevelText, skillExperienceText;
        public Image skillIcon, skillExperienceBar;

        public GameObject treeSlotPrefab;
        public Transform treeSlotsParent;
        public List<GameObject> curTreeSlots = new List<GameObject>();

        public GameObject backButtonGO;

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static SkillBookDisplayManager Instance { get; private set; }

        private void ClearAllSkillSlots()
        {
            foreach (var t in curSkillSlots)
                Destroy(t);

            curSkillSlots.Clear();
        }

        private void ClearCurTreeSlots()
        {
            foreach (var t in curTreeSlots)
                Destroy(t);

            curTreeSlots.Clear();
        }

        private void InitSkillBook()
        {
            ShowSkillList();
        }

        public void ShowSkillList()
        {
            backButtonGO.SetActive(false);
            RPGBuilderUtilities.EnableCG(SkillListCG);
            RPGBuilderUtilities.DisableCG(SkillInfoCG);

            ClearAllSkillSlots();
            foreach (var t in CharacterData.Instance.skillsDATA)
            {
                var newRecipeSlot = Instantiate(skillSlotPrefab, skillSlotsParent);
                curSkillSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<SkillSlotHolder>();
                slotREF.InitSlot(RPGBuilderUtilities.GetSkillFromID(t.skillID));
            }
        }

        public void ShowSkillInfo(RPGSkill skill)
        {
            backButtonGO.SetActive(true);
            RPGBuilderUtilities.DisableCG(SkillListCG);
            RPGBuilderUtilities.EnableCG(SkillInfoCG);

            ClearCurTreeSlots();

            skillNameText.text = skill.displayName;
            skillIcon.sprite = skill.icon;
            skillDescriptionText.text = skill.description;
            skillExperienceBar.fillAmount = RPGBuilderUtilities.getSkillEXPPercent(skill);
            skillLevelText.text = RPGBuilderUtilities.getSkillLevel(skill.ID) + " / " +
                                  RPGBuilderUtilities.GetLevelTemplateFromID(skill.levelTemplateID).levels;
            skillExperienceText.text =
                RPGBuilderUtilities.getSkillCurXP(skill) + " / " + RPGBuilderUtilities.getSkillMaxXP(skill);

            foreach (var t in skill.talentTrees)
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

            InitSkillBook();
            CharacterPanelDisplayManager.Instance.Hide();
            WeaponTemplatesDisplayManager.Instance.Hide();
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