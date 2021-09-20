using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.UIElements;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class DevUIManager : MonoBehaviour, IDisplayPanel
    {
        private bool showing;
        public CanvasGroup thisCG, getItemCG, spawnNPCCG;

        public Image GeneralCategory, CombatCategory, WorldCategory;
        public GameObject GeneralCategoryPanel, CombatCategoryPanel, WorldCategoryPanel;

        public TMP_Dropdown currencyDropdown, treepointsDropdown, skillsDropdown, gameScenesDropdown, factionDropdown, weaponTemplatesDropdown;
        public TMP_InputField addCurrencyField, addTreePointField, addSkillEXPField, teleportPosX, teleportPosY, teleportPosZ, alterFactionField, addWeaponTemplateEXPField;
        public TMP_InputField getItemName;
        public TMP_InputField getItemCount;
        public TMP_InputField spawnNPCName, statFieldName;
        public TMP_InputField spawnNPCCount;

        public TMP_InputField alterHealthField, alterStatField;
        public TMP_InputField classXPField;

        public TextMeshProUGUI currentSceneText, playerPOSText, playerROTText, allNPCsText, selectStatText;
        
        public Transform itemsParent;
        public GameObject getItemSlotPrefab;

        public Transform npcParent;
        public GameObject spawnNPCSlotPrefab, statSlotPrefab;

        public List<GameObject> curGetItemListSlots = new List<GameObject>();
        public List<GameObject> curSpawnNPCListSlots = new List<GameObject>();
        public List<GameObject> curStatListSlot = new List<GameObject>();

        public Color selectedColor, NotSelectedColor;

        public GameObject developerPanelButtonGO;

        private RPGStat selectedStat;

        public void SetSelectedStat(RPGStat stat)
        {
            selectedStat = stat;
            selectStatText.text = stat._name;
        }
        public void selectCategory(string categoryName)
        {
            switch (categoryName)
            {
                case "general":
                    GeneralCategory.color = selectedColor;
                    CombatCategory.color = NotSelectedColor;
                    WorldCategory.color = NotSelectedColor;
                    HideSpawnNPCPanel();
                    break;

                case "combat":
                    GeneralCategory.color = NotSelectedColor;
                    CombatCategory.color = selectedColor;
                    WorldCategory.color = NotSelectedColor;
                    HideGetItemPanel();
                    break;

                case "world":
                    GeneralCategory.color = NotSelectedColor;
                    CombatCategory.color = NotSelectedColor;
                    WorldCategory.color = selectedColor;
                    HideGetItemPanel();
                    HideSpawnNPCPanel();
                    break;
            }

            ShowCategory(categoryName);
        }

        private void ShowCategory(string categoryName)
        {
            switch (categoryName)
            {
                case "general":
                    GeneralCategoryPanel.SetActive(true);
                    CombatCategoryPanel.SetActive(false);
                    WorldCategoryPanel.SetActive(false);
                    PopulateCurrencyDropdown();
                    PopulateTreePointDropdown();
                    PopulateSkillsDropdown();
                    break;

                case "combat":
                    GeneralCategoryPanel.SetActive(false);
                    CombatCategoryPanel.SetActive(true);
                    WorldCategoryPanel.SetActive(false);
                    PopulateFactionDropdown();
                    PopulateWeaponTemplateDropdown();
                    break;

                case "world":
                    GeneralCategoryPanel.SetActive(false);
                    CombatCategoryPanel.SetActive(false);
                    WorldCategoryPanel.SetActive(true);
                    PopulateGameScenesDropdown();
                    break;
            }
        }

        private void PopulateCurrencyDropdown()
        {
            var currencyOptions = new List<TMP_Dropdown.OptionData>();
            foreach (var currency in RPGBuilderEssentials.Instance.allCurrencies)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = currency._name;
                newOption.image = currency.icon;
                currencyOptions.Add(newOption);
            }

            currencyDropdown.ClearOptions();
            currencyDropdown.options = currencyOptions;
        }
        
        private void PopulateTreePointDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var treePoint in RPGBuilderEssentials.Instance.allTreePoints)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = treePoint._name;
                newOption.image = treePoint.icon;
                options.Add(newOption);
            }

            treepointsDropdown.ClearOptions();
            treepointsDropdown.options = options;
        }
        
        private void PopulateSkillsDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var skill in RPGBuilderEssentials.Instance.allSkills)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = skill._name;
                newOption.image = skill.icon;
                options.Add(newOption);
            }

            skillsDropdown.ClearOptions();
            skillsDropdown.options = options;
        }
        
        private void PopulateGameScenesDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var gameScene in RPGBuilderEssentials.Instance.allGameScenes)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = gameScene._name;
                newOption.image = gameScene.minimapImage;
                options.Add(newOption);
            }

            gameScenesDropdown.ClearOptions();
            gameScenesDropdown.options = options;
        }
        
        private void PopulateFactionDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var faction in RPGBuilderEssentials.Instance.allFactions)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = faction._name;
                newOption.image = faction.icon;
                options.Add(newOption);
            }

            factionDropdown.ClearOptions();
            factionDropdown.options = options;
        }
        
        private void PopulateWeaponTemplateDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var wpTemplate in RPGBuilderEssentials.Instance.allWeaponTemplates)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = wpTemplate._name;
                newOption.image = wpTemplate.icon;
                options.Add(newOption);
            }

            weaponTemplatesDropdown.ClearOptions();
            weaponTemplatesDropdown.options = options;
        }

        public void DEVAlterCurrency()
        {
            RPGCurrency currency =RPGBuilderUtilities.getCurrencyByName(currencyDropdown.options[currencyDropdown.value].text);
            InventoryManager.Instance.AddCurrency(currency.ID,int.Parse(addCurrencyField.text));
        }
        public void DEVAddTreePoint()
        {
            RPGTreePoint point = RPGBuilderUtilities.getTreePointByName(treepointsDropdown.options[treepointsDropdown.value].text);
            TreePointsManager.Instance.AddTreePoint(point.ID,int.Parse(addTreePointField.text));
        }
        public void DEVAddSkillEXP()
        {
            RPGSkill skill = RPGBuilderUtilities.getSkillByName(skillsDropdown.options[skillsDropdown.value].text);
            LevelingManager.Instance.AddSkillXP(skill.ID,int.Parse(addSkillEXPField.text));
        }
        public void DEVAddWeaponTemplateEXP()
        {
            RPGWeaponTemplate wpTemplate = RPGBuilderUtilities.getWeaponTemplateByName(weaponTemplatesDropdown.options[weaponTemplatesDropdown.value].text);
            LevelingManager.Instance.AddWeaponTemplateXP(wpTemplate.ID,int.Parse(addWeaponTemplateEXPField.text));
        }

        public void DEVAlterHealth()
        {
            var value = int.Parse(alterHealthField.text);
            if (value < 0)
                value += Mathf.Abs(value) * 2;
            else
                value -= value * 2;
            CombatManager.playerCombatNode.TakeDamage(CombatManager.playerCombatNode, value, null, RPGBuilderEssentials.Instance.combatSettings.healthStatID);
        }
        
        public void DEVAlterFaction()
        {
            RPGFaction faction = RPGBuilderUtilities.getFactionByName(factionDropdown.options[factionDropdown.value].text);
            int amt = int.Parse(alterFactionField.text);
            if (amt > 0)
            {
                FactionManager.Instance.AddFactionPoint(faction.ID, amt);
            }
            else if(amt < 0)
            {
                FactionManager.Instance.RemoveFactionPoint(faction.ID, Mathf.Abs(amt));
            }
        }

        public void AddClassXP()
        {
            LevelingManager.Instance.AddClassXP(int.Parse(classXPField.text));
        }

        public void GetItem(RPGItem item)
        {
            if (item == null) return;
            int amt;
            if (getItemCount.text == "" || int.Parse(getItemCount.text) == 0 || int.Parse(getItemCount.text) == 1)
                amt = 1;
            else
                amt = int.Parse(getItemCount.text);

            for (int i = 0; i < amt; i++)
            {
                int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(item.ID, 1, false, true);
                if (itemsLeftOver != 0)
                {
                    ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
                }
            }
        }

        public void ClearInventory()
        {
            for (var i = 0; i < CharacterData.Instance.inventoryData.baseSlots.Count; i++)
            {
                if (CharacterData.Instance.inventoryData.baseSlots[i].itemID != -1)
                {
                    InventoryManager.Instance.RemoveItem(CharacterData.Instance.inventoryData.baseSlots[i].itemID,
                        CharacterData.Instance.inventoryData.baseSlots[i].itemStack, -1, i, true);
                }
            }
        }

        public void SpawnNPC(RPGNpc npc)
        {
            if (npc == null) return;
            int amt = 0;
            if (spawnNPCCount.text == "" || int.Parse(spawnNPCCount.text) == 0 || int.Parse(spawnNPCCount.text) == 1)
                amt = 1;
            else
                amt = int.Parse(spawnNPCCount.text);

            for (int i = 0; i < amt; i++)
            {
                CombatManager.Instance.SetupNPCPrefab(npc, false, false, null, CombatManager.playerCombatNode.transform.position, Quaternion.identity);
            }
        }
        
        
        public void AlterStat()
        {
            if (selectedStat == null) return;
            foreach (var t in CombatManager.playerCombatNode.nodeStats)
            {
                if (selectedStat.ID != t.stat.ID) continue;
                float amt = float.Parse(alterStatField.text);
                
                StatCalculator.HandleStat(CombatManager.playerCombatNode, selectedStat, t, amt, false, StatCalculator.TemporaryStatSourceType.none);
            }
            CombatManager.playerCombatNode.appearanceREF.HandleBodyScaleFromStats();
            if (CharacterPanelDisplayManager.Instance.thisCG.alpha == 1) CharacterPanelDisplayManager.Instance.InitCharStats();
        }

        public void DEVTeleportToPositon()
        {
            CombatManager.playerCombatNode.playerControllerEssentials.TeleportToTarget( new Vector3(float.Parse(teleportPosX.text), float.Parse(teleportPosY.text), float.Parse(teleportPosZ.text)));
        }
        public void DEVTeleportToGameScene()
        {
            RPGGameScene gameScene = RPGBuilderUtilities.GetGameSceneFromName(gameScenesDropdown.options[gameScenesDropdown.value].text);
            RPGBuilderEssentials.Instance.TeleportToGameScene(gameScene.ID, RPGBuilderUtilities.GetWorldPositionFromID(gameScene.startPositionID).position);
        }

        public void HideGetItemPanel()
        {
            RPGBuilderUtilities.DisableCG(getItemCG);
        }

        public void ShowGetItemPanel()
        {
            HideSpawnNPCPanel();
            if (getItemCG.alpha == 1)
            {
                HideGetItemPanel();
                return;
            }
            RPGBuilderUtilities.EnableCG(getItemCG);
            UpdateGetItemList();
        }

        private void ClearGetItemList()
        {
            foreach (var t in curGetItemListSlots)
                Destroy(t);

            curGetItemListSlots.Clear();
        }

        public void UpdateGetItemList()
        {
            ClearGetItemList();
            var curSearch = getItemName.text;

            var allItems = RPGBuilderEssentials.Instance.allItems;
            var validItems = new List<RPGItem>();


            if (curSearch.Length > 0 && !string.IsNullOrEmpty(curSearch) && !string.IsNullOrWhiteSpace(curSearch))
                foreach (var t in allItems)
                {
                    var itemNameToCheck = t._name;
                    itemNameToCheck = itemNameToCheck.ToLower();
                    curSearch = curSearch.ToLower();

                    if (itemNameToCheck.Contains(curSearch)) validItems.Add(t);
                }
            else
                validItems = allItems;


            foreach (var t in validItems)
            {
                var newGetItemSlot = Instantiate(getItemSlotPrefab, itemsParent);
                var newGetItemSlotRef = newGetItemSlot.GetComponent<GetItemSlot>();
                newGetItemSlotRef.thisitem = t;
                newGetItemSlotRef.icon.sprite = t.icon;
                curGetItemListSlots.Add(newGetItemSlot);
            }
        }
        
        public void HideSpawnNPCPanel()
        {
            RPGBuilderUtilities.DisableCG(spawnNPCCG);
        }

        public void ShowSpawnNPCPanel()
        {
            HideGetItemPanel();
            if (spawnNPCCG.alpha == 1)
            {
                HideSpawnNPCPanel();

                if (statFieldName.gameObject.activeInHierarchy)
                {
                    ShowSpawnNPCPanel();
                }
                return;
            }
            statFieldName.gameObject.SetActive(false);
            spawnNPCName.gameObject.SetActive(true);
            RPGBuilderUtilities.EnableCG(spawnNPCCG);
            UpdateSpawnNPCList();
        }

        public void ShowAlterStatPanel()
        {
            HideGetItemPanel();
            if (spawnNPCCG.alpha == 1)
            {
                HideSpawnNPCPanel();

                if (spawnNPCName.gameObject.activeInHierarchy)
                {
                    ShowAlterStatPanel();
                }
                return;
            }
            statFieldName.gameObject.SetActive(true);
            spawnNPCName.gameObject.SetActive(false);
            RPGBuilderUtilities.EnableCG(spawnNPCCG);
            UpdateStatList();
        }

        private void ClearSpawnNPCList()
        {
            foreach (var t in curSpawnNPCListSlots)
                Destroy(t);

            curSpawnNPCListSlots.Clear();
        }

        public void UpdateSpawnNPCList()
        {
            ClearStatList();
            ClearSpawnNPCList();
            var curSearch = spawnNPCName.text;

            var allNPCs = RPGBuilderEssentials.Instance.allNPCs;
            var validNPCs = new List<RPGNpc>();


            if (curSearch.Length > 0 && !string.IsNullOrEmpty(curSearch) && !string.IsNullOrWhiteSpace(curSearch))
                foreach (var t in allNPCs)
                {
                    var itemNameToCheck = t._name;
                    itemNameToCheck = itemNameToCheck.ToLower();
                    curSearch = curSearch.ToLower();

                    if (itemNameToCheck.Contains(curSearch)) validNPCs.Add(t);
                }
            else
                validNPCs = allNPCs;


            foreach (var t in validNPCs)
            {
                var newGetItemSlot = Instantiate(spawnNPCSlotPrefab, npcParent);
                var newGetItemSlotRef = newGetItemSlot.GetComponent<NPCSpawnSlotHolder>();
                newGetItemSlotRef.thisNPC = t;
                newGetItemSlotRef.icon.sprite = t.icon;
                newGetItemSlotRef.nameText.text = t._name;
                curSpawnNPCListSlots.Add(newGetItemSlot);
            }
        }
        
        
        private void ClearStatList()
        {
            foreach (var t in curStatListSlot)
                Destroy(t);

            curStatListSlot.Clear();
        }
        public void UpdateStatList()
        {
            ClearStatList();
            ClearSpawnNPCList();
            var curSearch = statFieldName.text;

            var allNPCs = RPGBuilderEssentials.Instance.allStats;
            var validNPCs = new List<RPGStat>();


            if (curSearch.Length > 0 && !string.IsNullOrEmpty(curSearch) && !string.IsNullOrWhiteSpace(curSearch))
                foreach (var t in allNPCs)
                {
                    var itemNameToCheck = t._name;
                    itemNameToCheck = itemNameToCheck.ToLower();
                    curSearch = curSearch.ToLower();

                    if (itemNameToCheck.Contains(curSearch)) validNPCs.Add(t);
                }
            else
                validNPCs = allNPCs;


            foreach (var t in validNPCs)
            {
                var newGetItemSlot = Instantiate(statSlotPrefab, npcParent);
                var newGetItemSlotRef = newGetItemSlot.GetComponent<DevStatSlotHolder>();
                newGetItemSlotRef.thisStat = t;
                newGetItemSlotRef.nameText.text = t._name;
                curStatListSlot.Add(newGetItemSlot);
            }
        }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;

            developerPanelButtonGO.SetActive(RPGBuilderEssentials.Instance.generalSettings.enableDevPanel);
        }

        public void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            //transform.SetAsLastSibling();

            selectCategory("general");
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(CombatManager.playerCombatNode!=null) CombatManager.playerCombatNode.playerControllerEssentials.GameUIPanelAction(showing);
        }

        public void Hide()
        {
            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
            
            HideSpawnNPCPanel();
            HideGetItemPanel();
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

        public bool IsTypingInField()
        {
            return addCurrencyField.isFocused || alterHealthField.isFocused || addTreePointField.isFocused
                   || classXPField.isFocused || addSkillEXPField.isFocused || getItemName.isFocused
                   || spawnNPCName.isFocused || statFieldName.isFocused || alterFactionField.isFocused;
        }
        
        private void Update()
        {
            if (!showing || CombatManager.playerCombatNode == null) return;
            currentSceneText.text = "Game Scene: <color=white>" + SceneManager.GetActiveScene().name;
            Vector3 pos = CombatManager.playerCombatNode.transform.position;
            playerPOSText.text = "Player Position: <color=white>" + (int)pos.x + " / " + (int)pos.y + " / " + (int)pos.z;
            Vector3 rot = CombatManager.playerCombatNode.transform.eulerAngles;
            playerROTText.text = "Player Rotation: <color=white>" + (int)rot.x + " / " + (int)rot.y + " / " + (int)rot.z;
            int npccount = CombatManager.Instance.allCombatNodes.Count - 1;
            allNPCsText.text = "NPC Count: <color=white>" + npccount;
        }


        public static DevUIManager Instance { get; private set; }
    }
}