using System.Collections.Generic;
using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class CharacterPanelDisplayManager : MonoBehaviour, IDisplayPanel
    {
        public CanvasGroup thisCG, CharGearCG, CharInfoCG, CharStatsCG, CharTalentsCG, CharFactionsCG, StatTooltipCG;
        private bool showing = false;

        public TextMeshProUGUI CharacterNameText, RaceNameText, ClassNameText, LevelText, ExperienceText, StatTooltipText;
        public GameObject classTalentTreeButtonGO;
        [System.Serializable]
        public class EquipSlotDATA
        {
            public string slotType;
            public EquipmentItemSlotDisplayHandler itemUIRef;
        }
        public EquipSlotDATA[] armorSlotData;
        public EquipSlotDATA[] weaponSlotData;

        public GameObject StatTitlePrefab, StatTextPrefab, CombatTreeSlotPrefab, factionSlotPrefab;
        public Transform StatTextsParent, CombatTreeSlotsParent, factionSlotsParent;
        private List<GameObject> statTextGO = new List<GameObject>();
        private List<GameObject> cbtTreeSlots = new List<GameObject>();
        private List<GameObject> factionSlots = new List<GameObject>();

        public enum characterInfoTypes
        {
            gear,
            info,
            stats,
            talents,
            factions
        }
        public characterInfoTypes curCharInfoType;

        public Animator talenttCategoryAnimator;

        public Color defaultCategoryColor, selectedCategoryColor;
        public Sprite defaultCategorySprite, selectedCategorySprite;
        public Image gearCategoryImage, statsCategoryImage, talentsCategoryImage, factionsCategorryImage;
        public Button gearCategoryButton, statsCategoryButton, talentsCategoryButton, factionsCategorryButton;
        
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void Show()
        {
            CharacterNameText.text = CharacterData.Instance.CharacterName;
            classTalentTreeButtonGO.SetActive(RPGBuilderEssentials.Instance.combatSettings.useClasses);
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
        
            InitCharacterCategory(curCharInfoType.ToString());
            SkillBookDisplayManager.Instance.Hide();
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

        private void disableAllCharCategoriesCG ()
        {
            RPGBuilderUtilities.DisableCG(CharInfoCG);
            RPGBuilderUtilities.DisableCG(CharGearCG);
            RPGBuilderUtilities.DisableCG(CharStatsCG);
            RPGBuilderUtilities.DisableCG(CharTalentsCG);
            RPGBuilderUtilities.DisableCG(CharFactionsCG);
        }

        private void ResetCategoryButtons()
        {
            setButtonAppearance(gearCategoryButton, gearCategoryImage, false);
            setButtonAppearance(statsCategoryButton, statsCategoryImage,false);
            setButtonAppearance(factionsCategorryButton, factionsCategorryImage, false);
            setButtonAppearance(talentsCategoryButton, talentsCategoryImage,false);
        }

        private void setButtonAppearance(Button button, Image image, bool selected)
        {
            ColorBlock colorblock = button.colors;
            colorblock.normalColor = selected ? selectedCategoryColor : defaultCategoryColor;
            button.colors = colorblock;
            image.sprite = selected ? selectedCategorySprite : defaultCategorySprite;
        }
        
        public void InitCharacterCategory (string newCategory)
        {
            var parsedEnum = (characterInfoTypes)System.Enum.Parse(typeof(characterInfoTypes), newCategory);
            disableAllCharCategoriesCG();
            ResetCategoryButtons();
            switch(parsedEnum)
            {
                case characterInfoTypes.gear:
                    curCharInfoType = characterInfoTypes.gear;
                    RPGBuilderUtilities.EnableCG(CharGearCG);
                    setButtonAppearance(gearCategoryButton, gearCategoryImage, true);
                    InitCharEquippedItems();
                    break;
                case characterInfoTypes.info:
                    curCharInfoType = characterInfoTypes.info;
                    RPGBuilderUtilities.EnableCG(CharInfoCG);
                    InitCharacterInfo();
                    break;
                case characterInfoTypes.stats:
                    curCharInfoType = characterInfoTypes.stats;
                    RPGBuilderUtilities.EnableCG(CharStatsCG);
                    setButtonAppearance(statsCategoryButton, statsCategoryImage,true);
                    InitCharStats();
                    break;
                case characterInfoTypes.talents:
                    curCharInfoType = characterInfoTypes.talents;
                    RPGBuilderUtilities.EnableCG(CharTalentsCG);
                    setButtonAppearance(talentsCategoryButton, talentsCategoryImage,true);
                    InitCharCombatTrees();
                    break;
                case characterInfoTypes.factions:
                    curCharInfoType = characterInfoTypes.factions;
                    RPGBuilderUtilities.EnableCG(CharFactionsCG);
                    setButtonAppearance(factionsCategorryButton,factionsCategorryImage, true);
                    InitCharFactions();
                    break;
            }

            if(RPGBuilderEssentials.Instance.combatSettings.useClasses && talenttCategoryAnimator != null) talenttCategoryAnimator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInClassTrees());
        }

        private void InitCharacterInfo ()
        {
            CharacterNameText.text = CharacterData.Instance.CharacterName;
            RaceNameText.text = "Race: " + RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).displayName; ;

            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                ClassNameText.text = "Class: " + RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID).displayName;
            }
            else
            {
                ClassNameText.text = "";
            }

            LevelText.text = "Level: " + CharacterData.Instance.classDATA.currentClassLevel;
            ExperienceText.text = "Experience: " + CharacterData.Instance.classDATA.currentClassXP + " / " + CharacterData.Instance.classDATA.maxClassXP;
        }

        private void ClearCombatTreeSlots()
        {
            foreach (var t in cbtTreeSlots) Destroy(t);

            cbtTreeSlots.Clear();
        }

        private void InitCharCombatTrees()
        {
            ClearCombatTreeSlots();
            foreach (var t in RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID).talentTrees)
            {
                var cbtTree = Instantiate(CombatTreeSlotPrefab, CombatTreeSlotsParent);
                cbtTreeSlots.Add(cbtTree);
                var slotREF = cbtTree.GetComponent<CombatTreeSlot>();
                slotREF.InitSlot(RPGBuilderUtilities.GetTalentTreeFromID(t.talentTreeID));
            }
        }
        public void InitCharEquippedItems ()
        {
            for (var i = 0; i < armorSlotData.Length; i++)
                if (InventoryManager.Instance.equippedArmors[i].itemEquipped != null)
                    armorSlotData[i].itemUIRef.InitItem(InventoryManager.Instance.equippedArmors[i].itemEquipped, InventoryManager.Instance.equippedArmors[i].temporaryItemDataID);
                else
                    armorSlotData[i].itemUIRef.ResetItem();
            
            for (var i = 0; i < weaponSlotData.Length; i++)
                if (InventoryManager.Instance.equippedWeapons[i].itemEquipped != null)
                    weaponSlotData[i].itemUIRef.InitItem(InventoryManager.Instance.equippedWeapons[i].itemEquipped, InventoryManager.Instance.equippedWeapons[i].temporaryItemDataID);
                else
                    weaponSlotData[i].itemUIRef.ResetItem();
        }

        public void InitCharStats ()
        {
            ClearStatText();
            foreach (var t in RPGBuilderEssentials.Instance.combatSettings.UIStatsCategories)
            {
                if (t == "None") continue;
                var statTitle = Instantiate(StatTitlePrefab, StatTextsParent);
                statTextGO.Add(statTitle);
                statTitle.GetComponent<TextMeshProUGUI>().text = t;

                foreach (var t1 in CombatManager.playerCombatNode.nodeStats)
                {
                    if (t1.stat.StatUICategory != t) continue;
                    var statText = Instantiate(StatTextPrefab, StatTextsParent);
                    statTextGO.Add(statText);
                    StatDataHolder statREF = statText.GetComponent<StatDataHolder>();
                    if (statREF != null)
                    {
                        statREF.InitStatText(t1);
                    }
                }
            }
        }

        private void ClearFactionSlots()
        {
            foreach (var t in factionSlots) Destroy(t);

            factionSlots.Clear();
        }

        public void InitCharFactions()
        {
            ClearFactionSlots();
            foreach (var t in CharacterData.Instance.factionsData)
            {
                var factionSlot = Instantiate(factionSlotPrefab, factionSlotsParent);
                factionSlots.Add(factionSlot);
                FactionSlotDataHolder factionSlotREF = factionSlot.GetComponent<FactionSlotDataHolder>();
                if (factionSlotREF != null)
                {
                    factionSlotREF.Init(t);
                }

            }
        }

        public void ShowStatTooltipPanel(RPGStat stat)
        {
            StatTooltipCG.alpha = 1;
            StatTooltipText.text = stat.description;
        }
        public void HideStatTooltipPanel()
        {
            StatTooltipCG.alpha = 0;
            StatTooltipText.text = "";
        }

        private void ClearStatText ()
        {
            foreach (var t in statTextGO) Destroy(t);

            statTextGO.Clear();
        }

        public void Toggle()
        {
            if (showing)
                Hide();
            else
                Show();
        }

        public static CharacterPanelDisplayManager Instance { get; private set; }
    }
}
