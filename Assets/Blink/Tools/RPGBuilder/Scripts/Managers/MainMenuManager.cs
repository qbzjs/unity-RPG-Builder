using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Character;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        public GameObject RPGBuilderEssentialsPrefab, LoadingScreenManagerPrefab;

        public CanvasGroup HomeCG, CreateCharCG, ContinueCG, BackHomeButtonCG, errorPopupCG;
        public TMP_InputField characterNameIF;

        public TextMeshProUGUI curSelectedCharacterNameText,
            classTitleText,
            classInfoDescriptionText,
            raceInfoDescriptionText,
            currentAllocationPointsText,
            popupMessageText,
            raceNameText,
            classNameText;

        public GameObject raceSlotPrefab, classSlotPrefab;
        public Transform raceSlotsParent, classSlotsParent;

        private readonly List<RaceSlotHolder> curRaceSlots = new List<RaceSlotHolder>();
        private readonly List<ClassSlotHolder> curClassSlots = new List<ClassSlotHolder>();
        public List<ClassSlotHolder> curGenderSlots = new List<ClassSlotHolder>();
        private readonly List<CharacterSlotHolder> curCharSlots = new List<CharacterSlotHolder>();

        private int currentlySelectedRace;
        private int currentlySelectedClass;
        private RPGRace.RACE_GENDER currentlySelectedGender;

        public Color slotSelectedColor, slotNotSelectedColor;

        public Transform characterModelSpot;

        public GameObject curPlayerModel;

        private List<CharacterData> allCharacters;
        public GameObject characterSlotPrefab;
        public Transform characterSlotsParent;

        public GameObject statAllocationSlotPrefab;
        public Transform statAllocationSlotsParent;
        public List<StatAllocationSlotDataHolder> curStatAllocationSlots = new List<StatAllocationSlotDataHolder>();

        public CanvasGroup modifiersCG;
        public TextMeshProUGUI modifiersPointsText;

        public Transform availablePositiveModifiersParent,
            availableNegativeModifiersParent,
            chosenPositiveModifiersParent,
            chosenNegativeModifiersParent;

        public GameObject gameModifierSlot;
        public Color modifierPositiveColor, modifierNegativeColor;

        public List<GameModifierSlotDataHolder> currentAvailablePositiveModifiersSlots = new List<GameModifierSlotDataHolder>();
        public List<GameModifierSlotDataHolder> currentAvailableNegativeModifiersSlots = new List<GameModifierSlotDataHolder>();
        public List<GameModifierSlotDataHolder> currentChosenPositiveModifiersSlots = new List<GameModifierSlotDataHolder>();
        public List<GameModifierSlotDataHolder> currentChosenNegativeModifiersSlots = new List<GameModifierSlotDataHolder>();
        
        public static MainMenuManager Instance { get; private set; }

        private IEnumerator Start()
        {
            if (!PlayerPrefs.HasKey("MasterVolume"))
            {
                PlayerPrefs.SetFloat("MasterVolume", 0.25f);
                AudioListener.volume =  0.25f;
            }
            else AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume");
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            if (Instance != null) yield break;
            Instance = this;

            if (FindObjectOfType<RPGBuilderEssentials>() == null)
                Instantiate(RPGBuilderEssentialsPrefab, Vector3.zero, Quaternion.identity);
            if (FindObjectOfType<LoadingScreenManager>() == null)
                Instantiate(LoadingScreenManagerPrefab, Vector3.zero, Quaternion.identity);

            
            disableAllCG();
            RPGBuilderUtilities.EnableCG(HomeCG);

            LoadAllCharacter();
        }


        private void LoadAllCharacter()
        {
            allCharacters = DataSavingSystem.LoadAllCharacters();
        }

        public void ClickNewChar()
        {
            CharacterData.Instance.RESET_CHARACTER_DATA(false);
            disableAllCG();
            RPGBuilderUtilities.EnableCG(CreateCharCG);
            RPGBuilderUtilities.EnableCG(BackHomeButtonCG);

            classTitleText.gameObject.SetActive(RPGBuilderEssentials.Instance.combatSettings.useClasses);
            classInfoDescriptionText.gameObject.SetActive(RPGBuilderEssentials.Instance.combatSettings.useClasses);
            classNameText.gameObject.SetActive(RPGBuilderEssentials.Instance.combatSettings.useClasses);
            
            InitCreateNewChar();
        }

        public void ClickContinue()
        {
			if (allCharacters.Count == 0) return;
            disableAllCG();
            RPGBuilderUtilities.EnableCG(ContinueCG);
            RPGBuilderUtilities.EnableCG(BackHomeButtonCG);

            InitContinue();
        }

        public void DeleteCharacter()
        {
            if (allCharacters.Count == 0) return;
            RPGBuilderJsonSaver.DeleteCharacter(CharacterData.Instance.CharacterName);
            allCharacters.Clear();
            LoadAllCharacter();
            if (allCharacters.Count == 0)
                ClickHome();
            else
                InitContinue();
        }

        private void InitContinue()
        {
            clearCharSlots();
            foreach (var t in allCharacters)
            {
                var charSlot = Instantiate(characterSlotPrefab, characterSlotsParent);
                var holder = charSlot.GetComponent<CharacterSlotHolder>();
                holder.Init(t);
                curCharSlots.Add(holder);
            }

            if (allCharacters.Count > 0) SelectCharacter(allCharacters[0].CharacterName);
        }

        public void ClickHome()
        {
            CharacterData.Instance.RESET_CHARACTER_DATA(false);
            disableAllCG();
            RPGBuilderUtilities.EnableCG(HomeCG);

            if (curPlayerModel != null) Destroy(curPlayerModel);
        }

        private void disableAllCG()
        {
            RPGBuilderUtilities.DisableCG(HomeCG);
            RPGBuilderUtilities.DisableCG(ContinueCG);
            RPGBuilderUtilities.DisableCG(CreateCharCG);
            RPGBuilderUtilities.DisableCG(BackHomeButtonCG);
        }

        private void InitCreateNewChar()
        {
            clearAllSlots();

            currentlySelectedRace = 0;
            currentlySelectedClass = 0;

            for (var i = 0; i < RPGBuilderEssentials.Instance.allRaces.Count; i++)
            {
                var raceSlot = Instantiate(raceSlotPrefab, raceSlotsParent);
                var holder = raceSlot.GetComponent<RaceSlotHolder>();
                holder.Init(RPGBuilderEssentials.Instance.allRaces[i], i);
                curRaceSlots.Add(holder);
            }

            GenerateClassSlots();

            resetAllClassBorders();
            resetAllRaceBorders();

            if(RPGBuilderEssentials.Instance.allRaces.Count>0)SelectRace(currentlySelectedRace);
        }

        private void GenerateClassSlots()
        {
            clearClassesSlots();
            if (currentlySelectedRace == -1) return;
            if (RPGBuilderEssentials.Instance.allRaces.Count == 0) return;
            for (var i = 0; i < RPGBuilderEssentials.Instance.allRaces[currentlySelectedRace].availableClasses.Count; i++)
            {
                var classSlot = Instantiate(classSlotPrefab, classSlotsParent);
                var holder = classSlot.GetComponent<ClassSlotHolder>();
                holder.Init(RPGBuilderUtilities.GetClassFromID(RPGBuilderEssentials.Instance.allRaces[currentlySelectedRace].availableClasses[i].classID),
                    i);
                curClassSlots.Add(holder);
            }
        }

        private void clearAllSlots()
        {
            foreach (var t in curRaceSlots)
                Destroy(t.gameObject);

            curRaceSlots.Clear();
            foreach (var t in curClassSlots)
                Destroy(t.gameObject);

            curClassSlots.Clear();
        }

        private void clearCharSlots()
        {
            foreach (var t in curCharSlots)
                Destroy(t.gameObject);

            curCharSlots.Clear();
        }

        private void clearClassesSlots()
        {
            foreach (var t in curClassSlots)
                Destroy(t.gameObject);

            curClassSlots.Clear();
        }

        private void resetAllRaceBorders()
        {
            foreach (var t in curRaceSlots)
                t.selectedBorder.color = slotNotSelectedColor;
        }

        private void resetAllClassBorders()
        {
            foreach (var t in curClassSlots)
                t.selectedBorder.color = slotNotSelectedColor;
        }

        private void resetAllGenderBorders()
        {
            foreach (var t in curGenderSlots)
                t.selectedBorder.color = slotNotSelectedColor;
        }

        

        public void SelectRace(int raceIndex)
        {
            currentlySelectedRace = raceIndex;
            CharacterData.Instance.raceID = RPGBuilderEssentials.Instance.allRaces[raceIndex].ID;
            if(RPGBuilderEssentials.Instance.combatSettings.useClasses)CharacterData.Instance.classDATA.classID = RPGBuilderEssentials.Instance.allRaces[raceIndex].availableClasses[0].classID;
            RPGWorldPosition worldPosREF = RPGBuilderUtilities.GetWorldPositionFromID(RPGBuilderEssentials.Instance
                .allRaces[currentlySelectedRace].startingPositionID);
            CharacterData.Instance.position = worldPosREF.position;
            if (worldPosREF.useRotation)
            {
                CharacterData.Instance.rotation = worldPosREF.rotation;
            }
            CharacterData.Instance.currentGameSceneID =
                RPGBuilderUtilities.GetGameSceneFromID(RPGBuilderEssentials.Instance.allRaces[currentlySelectedRace].startingSceneID).ID;

            SelectGender("Male");
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                SelectClass(0);
                currentlySelectedClass = 0;
                GenerateClassSlots();
                curClassSlots[currentlySelectedClass].selectedBorder.color = slotSelectedColor;
            }
            else
            {
                InitStatAllocation();
            }
            
            resetAllRaceBorders();
            curRaceSlots[raceIndex].selectedBorder.color = slotSelectedColor;

            RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(RPGBuilderEssentials.Instance.allRaces[raceIndex].ID);
            raceInfoDescriptionText.text = raceREF.description;
            raceNameText.text = raceREF.displayName;

            if (curPlayerModel != null)
            {
                PlayerAppearanceHandler appearanceREF = curPlayerModel.GetComponent<PlayerAppearanceHandler>();
                for (var i = 0; i < raceREF.startItems.Count; i++)
                    if (raceREF.startItems[i].itemID != -1 && raceREF.startItems[i].equipped)
                        InventoryManager.Instance.InitEquipClassItemMainMenu(
                            RPGBuilderUtilities.GetItemFromID(raceREF.startItems[i].itemID),
                            appearanceREF, i);
            }
        }

        public void SelectGender(string gender)
        {
            if (curPlayerModel != null) Destroy(curPlayerModel);
            if (RPGBuilderEssentials.Instance.allRaces.Count == 0) return;
            resetAllGenderBorders();
            currentlySelectedGender = (RPGRace.RACE_GENDER) Enum.Parse(typeof(RPGRace.RACE_GENDER), gender);
            if (currentlySelectedGender == RPGRace.RACE_GENDER.Male)
            {
                curGenderSlots[0].selectedBorder.color = slotSelectedColor;
                curPlayerModel = Instantiate(RPGBuilderEssentials.Instance.allRaces[currentlySelectedRace].malePrefab, Vector3.zero,
                    Quaternion.identity);
            }
            else
            {
                curGenderSlots[1].selectedBorder.color = slotSelectedColor;
                curPlayerModel = Instantiate(RPGBuilderEssentials.Instance.allRaces[currentlySelectedRace].femalePrefab, Vector3.zero,
                    Quaternion.identity);
            }


            if (curPlayerModel != null)
            {
                PlayerAppearanceHandler appearanceREF = curPlayerModel.GetComponent<PlayerAppearanceHandler>();
                InventoryManager.Instance.HideAllItemsMainMenu(appearanceREF);
                if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
                {
                    RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                    for (var i = 0; i < classREF.startItems.Count; i++)
                        if (classREF.startItems[i].itemID != -1 && classREF.startItems[i].equipped)
                            InventoryManager.Instance.InitEquipClassItemMainMenu(
                                RPGBuilderUtilities.GetItemFromID(classREF.startItems[i].itemID),
                                appearanceREF, i);
                }

                RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
                foreach (var t in raceREF.weaponTemplates)
                {
                    RPGWeaponTemplate weaponTemplateREF = RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID);
                    for (var i = 0; i < weaponTemplateREF.startItems.Count; i++)
                        if (weaponTemplateREF.startItems[i].itemID != -1 && weaponTemplateREF.startItems[i].equipped)
                            InventoryManager.Instance.InitEquipClassItemMainMenu(
                                RPGBuilderUtilities.GetItemFromID(weaponTemplateREF.startItems[i].itemID),
                                appearanceREF, i);
                }
            }

            CharacterData.Instance.gender = currentlySelectedGender;

            foreach (var raceSlot in curRaceSlots)
            {
                raceSlot.icon.sprite = RPGBuilderUtilities.getRaceIconByID(RPGBuilderEssentials.Instance.allRaces[raceSlot.raceIndex].ID);
            }

            curPlayerModel.transform.SetParent(characterModelSpot);
            curPlayerModel.transform.localPosition = Vector3.zero;
            curPlayerModel.transform.localRotation = Quaternion.identity;

            // Preventing combat and movement actions on the character in main menu
            Destroy(curPlayerModel.GetComponent<CombatNode>());
            Destroy(curPlayerModel.GetComponent<PlayerAnimatorLayerHandler>());

            RPGBCharacterControllerEssentials controllerEssentialREF =
                curPlayerModel.GetComponent<RPGBCharacterControllerEssentials>();
            controllerEssentialREF.MainMenuInit();
        }

        public void SelectClass(int classIndex)
        {
            resetAllClassBorders();
            currentlySelectedClass = classIndex;
            curClassSlots[classIndex].selectedBorder.color = slotSelectedColor;

            CharacterData.Instance.classDATA.classID = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID)
                .availableClasses[currentlySelectedClass].classID;

            if (curPlayerModel == null) return;
            RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
            PlayerAppearanceHandler appearanceREF = curPlayerModel.GetComponent<PlayerAppearanceHandler>();
            InventoryManager.Instance.HideAllItemsMainMenu(appearanceREF);
            for (var i = 0; i < classREF.startItems.Count; i++)
                if (classREF.startItems[i].itemID != -1 && classREF.startItems[i].equipped)
                    InventoryManager.Instance.InitEquipClassItemMainMenu(
                        RPGBuilderUtilities.GetItemFromID(classREF.startItems[i].itemID),
                        appearanceREF, i);
            
            classInfoDescriptionText.text = classREF.description;
            classNameText.text = classREF.displayName;
            
            InitStatAllocation();
        }

        bool IsCharacterNameAvailable(string charName)
        {
            return allCharacters.Any(t => t.CharacterName == charName);
        }

        private void clearAllStatAllocationSlots()
        {
            foreach (var t in curStatAllocationSlots)
                Destroy(t.gameObject);
            
            curStatAllocationSlots.Clear();
        }

        private void InitStatAllocation()
        {
            clearAllStatAllocationSlots();
            CharacterData.Instance.allocatedStatsData.Clear();
            CharacterData.Instance.mainMenuStatAllocationPoints = 0;

            RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
            CharacterData.Instance.mainMenuStatAllocationPoints += raceREF.allocationStatPoints;

            RPGClass classREF = null;
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
            }

            if (RPGBuilderEssentials.Instance.combatSettings.useClasses && classREF != null)
            {
                CharacterData.Instance.mainMenuStatAllocationPoints += classREF.allocationStatPoints;
            }

            CharacterData.Instance.mainMenuStatAllocationMaxPoints =
                CharacterData.Instance.mainMenuStatAllocationPoints;

            UpdateAllocationPointsText();

            foreach (var allocatedStatEntry in raceREF.allocatedStatsEntries)
            {
                StatAllocationManager.Instance.SpawnStatAllocationSlot(allocatedStatEntry, statAllocationSlotPrefab, statAllocationSlotsParent, curStatAllocationSlots, StatAllocationSlotDataHolder.SlotType.Menu);
            }


            if (RPGBuilderEssentials.Instance.combatSettings.useClasses && classREF != null)
            {
                foreach (var allocatedStatEntry in classREF.allocatedStatsEntries)
                {
                    StatAllocationManager.Instance.SpawnStatAllocationSlot(allocatedStatEntry, statAllocationSlotPrefab, statAllocationSlotsParent, curStatAllocationSlots, StatAllocationSlotDataHolder.SlotType.Menu);
                }
            }

            foreach (var allocatedStatSlot in curStatAllocationSlots)
            {
                float currentValue = allocatedStatSlot.thisStat.baseValue;
                foreach (var t in raceREF.stats)
                {
                    if (t.statID == allocatedStatSlot.thisStat.ID)
                    {
                        currentValue += t.amount;
                    }
                }

                if (RPGBuilderEssentials.Instance.combatSettings.useClasses && classREF != null)
                {
                    foreach (var t in classREF.stats)
                    {
                        if (t.statID == allocatedStatSlot.thisStat.ID)
                        {
                            currentValue += t.amount;
                        }
                    }
                }

                float max = StatAllocationManager.Instance.getMaxAllocatedStatValue(allocatedStatSlot.thisStat);
                allocatedStatSlot.curValueText.text = max > 0 ? currentValue + " / " + max : currentValue.ToString();
            }
            
            StatAllocationManager.Instance.HandleStatAllocationButtons(CharacterData.Instance.mainMenuStatAllocationPoints, CharacterData.Instance.mainMenuStatAllocationMaxPoints, curStatAllocationSlots, StatAllocationSlotDataHolder.SlotType.Menu);
        }

        public void UpdateAllocationPointsText()
        {
            currentAllocationPointsText.text = "Points: " + CharacterData.Instance.mainMenuStatAllocationPoints;
        }

        public void CreateCharacter()
        {
            
            if (CharacterData.Instance.raceID == -1)
            {
                ShowPopupMessage("A race must be selected");
                return;
            }
            if (characterNameIF.text == "")
            {
                ShowPopupMessage("The name cannot be empty");
                return;
            }
            if (IsCharacterNameAvailable(characterNameIF.text))
            {
                ShowPopupMessage("This name is already taken");
                return;
            }
            if (CharacterData.Instance.mainMenuStatAllocationPoints > 0 && RPGBuilderEssentials.Instance.combatSettings.spendAllStatPointsToCreateChar)
            {
                ShowPopupMessage("All Stat Points need to be spent");
                return;
            }
            
            if (CharacterData.Instance.mainMenuStatAllocationPoints > 0 && RPGBuilderEssentials.Instance.combatSettings.spendAllStatPointsToCreateChar)
            {
                ShowPopupMessage("All Stat Points need to be spent");
                return;
            }
            
            CharacterData.Instance.CharacterName = characterNameIF.text;

            foreach (var ab in RPGBuilderEssentials.Instance.allAbilities)
            {
                CharacterData.Ability_DATA newAb = new CharacterData.Ability_DATA();
                newAb.name = ab._name;
                newAb.ID = ab.ID;
                newAb.rank = -1;
                newAb.known = false;
                CharacterData.Instance.abilitiesData.Add(newAb);
            }
            foreach (var recipe in RPGBuilderEssentials.Instance.allCraftingRecipes)
            {
                CharacterData.Recipe_DATA newAb = new CharacterData.Recipe_DATA();
                newAb.name = recipe._name;
                newAb.ID = recipe.ID;
                newAb.rank = -1;
                newAb.known = false;
                CharacterData.Instance.recipesData.Add(newAb);
            }
            foreach (var resourceNode in RPGBuilderEssentials.Instance.allResourceNodes)
            {
                CharacterData.ResourceNode_DATA newAb = new CharacterData.ResourceNode_DATA();
                newAb.name = resourceNode._name;
                newAb.ID = resourceNode.ID;
                newAb.rank = -1;
                newAb.known = false;
                CharacterData.Instance.resourceNodeData.Add(newAb);
            }
            foreach (var bonus in RPGBuilderEssentials.Instance.allBonuses)
            {
                CharacterData.Bonus_DATA newAb = new CharacterData.Bonus_DATA();
                newAb.name = bonus._name;
                newAb.ID = bonus.ID;
                newAb.rank = -1;
                newAb.known = false;
                newAb.On = false;
                CharacterData.Instance.bonusesData.Add(newAb);
            }
            
            foreach (var ab in RPGBuilderEssentials.Instance.allFactions)
            {
                CharacterData.Faction_DATA newAb = new CharacterData.Faction_DATA();
                newAb.name = ab._name;
                newAb.ID = ab.ID;
                newAb.currentStance = "";
                foreach (var t in RPGBuilderUtilities.GetFactionFromID(RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).factionID).factionInteractions)
                {
                    if(t.factionID != ab.ID) continue;
                    newAb.currentStance = t.defaultStance;
                    newAb.currentPoint = t.startingPoints;
                }
                CharacterData.Instance.factionsData.Add(newAb);
            }


            CharacterData.Instance.talentTrees.Clear();
            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                var classRef = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                foreach (var t in classRef.talentTrees)
                {
                    AddTalentTree(t.talentTreeID);
                }
            }

            var raceRef = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
            foreach (var t in raceRef.weaponTemplates)
            {
                RPGWeaponTemplate weaponTemplateREF = RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID);
                var newWeaponTemplateDATA = new CharacterData.WeaponTemplatesData();
                newWeaponTemplateDATA.currentWeaponLevel = 1;
                newWeaponTemplateDATA.currentWeaponXP = 0;
                RPGLevelsTemplate lvlTemplateREF = RPGBuilderUtilities.GetLevelTemplateFromID(weaponTemplateREF.levelTemplateID);
                newWeaponTemplateDATA.maxWeaponXP = lvlTemplateREF.allLevels[0].XPRequired;
                newWeaponTemplateDATA.weaponTemplateID = t.weaponTemplateID;
                CharacterData.Instance.weaponTemplates.Add(newWeaponTemplateDATA);
            }

            foreach (var t in CharacterData.Instance.weaponTemplates)
            {
                var weaponTemplateREF = RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID);
                foreach (var t1 in weaponTemplateREF.talentTrees)
                {
                    AddTalentTree(t1.talentTreeID);
                }
            }

            CharacterData.Instance.treePoints.Clear();
            foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
            {
                var newTreePointData = new CharacterData.TreePoints_DATA();
                newTreePointData.treePointID = t.ID;
                newTreePointData.amount = (int)GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.TreePoint + "+" +
                    RPGGameModifier.PointModifierType.Start_At,
                    t.startAmount, t.ID, -1);
                CharacterData.Instance.treePoints.Add(newTreePointData);
            }

            CharacterData.Instance.currencies.Clear();

            foreach (var t in RPGBuilderEssentials.Instance.allCurrencies)
            {
                var newCurrencyData = new CharacterData.Currencies_DATA();
                newCurrencyData.currencyID = t.ID;
                newCurrencyData.amount = t.baseValue;
                CharacterData.Instance.currencies.Add(newCurrencyData);
            }

            for (var i = 0; i < RPGBuilderEssentials.Instance.itemSettings.InventorySlots; i++)
            {
                var newInvItemData = new CharacterData.InventorySlotDATA {itemID = -1, itemStack = 0, itemDataID = -1};
                CharacterData.Instance.inventoryData.baseSlots.Add(newInvItemData);
            }

            CharacterData.Instance.skillsDATA.Clear();
            foreach (var t1 in RPGBuilderEssentials.Instance.allSkills)
                if (t1.automaticlyAdded)
                {
                    var newSkillData = new CharacterData.SkillsDATA();
                    newSkillData.currentSkillLevel = 1;
                    newSkillData.currentSkillXP = 0;
                    newSkillData.skillID = t1.ID;
                    var skillREF = RPGBuilderUtilities.GetSkillFromID(newSkillData.skillID);
                    newSkillData.maxSkillXP = RPGBuilderUtilities.GetLevelTemplateFromID(skillREF.levelTemplateID)
                        .allLevels[0].XPRequired;
                    foreach (var t2 in skillREF.talentTrees)
                    {
                        AddTalentTree(t2.talentTreeID);
                    }

                    CharacterData.Instance.skillsDATA.Add(newSkillData);
                }


            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                CharacterData.Instance.classDATA.currentClassLevel = 1;
                CharacterData.Instance.classDATA.maxClassXP = RPGBuilderUtilities
                    .GetLevelTemplateFromID(RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                        .levelTemplateID).allLevels[0].XPRequired;
            }

            foreach (var t in RPGBuilderEssentials.Instance.generalSettings.actionKeys)
            {
                var newKeybind = new CharacterData.ActionKeyDATA();
                newKeybind.actionKeyName = t.actionName;
                newKeybind.currentKey = t.defaultKey;
                CharacterData.Instance.actionKeys.Add(newKeybind);
            }
            
            RPGBuilderJsonSaver.GenerateCharacterEquippedtemsData();
            RPGBuilderJsonSaver.SaveCharacterData(characterNameIF.text, CharacterData.Instance);
            ClearAllCharactersData();
            LoadingScreenManager.Instance.LoadGameScene(CharacterData.Instance.currentGameSceneID);
        }

        void ClearAllCharactersData()
        {
            foreach (var t in RPGBuilderEssentials.Instance.temporaryDataGO.GetComponents<CharacterData>())
            {
                Destroy(t);
            }
        }

        private void AssignCharacterDATA(CharacterData newCharCbtData)
        {
            CharacterData.Instance.created = newCharCbtData.created;
            CharacterData.Instance.CharacterName = newCharCbtData.CharacterName;
            CharacterData.Instance.raceID = newCharCbtData.raceID;
            CharacterData.Instance.gender = newCharCbtData.gender;

            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                CharacterData.Instance.classDATA.classID = newCharCbtData.classDATA.classID;
                CharacterData.Instance.classDATA.currentClassLevel = newCharCbtData.classDATA.currentClassLevel;
                CharacterData.Instance.classDATA.currentClassXP = newCharCbtData.classDATA.currentClassXP;
                CharacterData.Instance.classDATA.maxClassXP = newCharCbtData.classDATA.maxClassXP;
            }
            
            CharacterData.Instance.inventoryData.baseSlots = newCharCbtData.inventoryData.baseSlots;
            CharacterData.Instance.inventoryData.bags = newCharCbtData.inventoryData.bags;

            CharacterData.Instance.actionBarSlotsDATA = newCharCbtData.actionBarSlotsDATA;
            CharacterData.Instance.stealthedActionBarSlotsDATA = newCharCbtData.stealthedActionBarSlotsDATA;

            CharacterData.Instance.actionKeys = newCharCbtData.actionKeys;

            CharacterData.Instance.currentActionAbilities = newCharCbtData.currentActionAbilities;

            CharacterData.Instance.armorsEquipped = newCharCbtData.armorsEquipped;
            CharacterData.Instance.weaponsEquipped = newCharCbtData.weaponsEquipped;

            CharacterData.Instance.position = newCharCbtData.position;
            CharacterData.Instance.rotation = newCharCbtData.rotation;

            CharacterData.Instance.currentGameSceneID = newCharCbtData.currentGameSceneID;

            CharacterData.Instance.talentTrees = newCharCbtData.talentTrees;
            CharacterData.Instance.treePoints = newCharCbtData.treePoints;
            CharacterData.Instance.currencies = newCharCbtData.currencies;
            CharacterData.Instance.questsData = newCharCbtData.questsData;
            CharacterData.Instance.npcKilled = newCharCbtData.npcKilled;
            CharacterData.Instance.scenesEntered = newCharCbtData.scenesEntered;
            CharacterData.Instance.regionsEntered = newCharCbtData.regionsEntered;
            CharacterData.Instance.abilitiesLearned = newCharCbtData.abilitiesLearned;
            CharacterData.Instance.itemsGained = newCharCbtData.itemsGained;
            CharacterData.Instance.skillsDATA = newCharCbtData.skillsDATA;
            CharacterData.Instance.bonusLearned = newCharCbtData.bonusLearned;
            CharacterData.Instance.itemsDATA = newCharCbtData.itemsDATA;
            CharacterData.Instance.allRandomizedItems = newCharCbtData.allRandomizedItems;
            CharacterData.Instance.nextAvailableItemID = newCharCbtData.nextAvailableItemID;
            CharacterData.Instance.nextAvailableRandomItemID = newCharCbtData.nextAvailableRandomItemID;
            
            CharacterData.Instance.abilitiesData = newCharCbtData.abilitiesData;
            CharacterData.Instance.recipesData = newCharCbtData.recipesData;
            CharacterData.Instance.resourceNodeData = newCharCbtData.resourceNodeData;
            CharacterData.Instance.bonusesData = newCharCbtData.bonusesData;
            
            CharacterData.Instance.factionsData = newCharCbtData.factionsData;
            CharacterData.Instance.weaponTemplates = newCharCbtData.weaponTemplates;
            
            CharacterData.Instance.dialoguesDATA = newCharCbtData.dialoguesDATA;
            
            CharacterData.Instance.allocatedStatsData = newCharCbtData.allocatedStatsData;

            CharacterData.Instance.menuGameModifierPoints = newCharCbtData.menuGameModifierPoints;
            CharacterData.Instance.worldGameModifierPoints = newCharCbtData.worldGameModifierPoints;
            CharacterData.Instance.gameModifiersData = newCharCbtData.gameModifiersData;
        }

        public void PlaySelectedCharacter()
        {
            HandleBackdating();
            ClearAllCharactersData();
            LoadingScreenManager.Instance.LoadGameScene(CharacterData.Instance.currentGameSceneID);
        }

        private bool classHasTalentTree(int ID, RPGClass _class)
        {
            if (!RPGBuilderEssentials.Instance.combatSettings.useClasses) return false;
            foreach (var t in _class.talentTrees)
                if (t.talentTreeID == ID)
                    return true;

            return false;
        }
        private bool weaponTemplateHasTalentTree(int ID)
        {
            RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
            foreach (var t1 in raceREF.weaponTemplates)
            {
                RPGWeaponTemplate REF = RPGBuilderUtilities.GetWeaponTemplateFromID(t1.weaponTemplateID);
                foreach (var t in REF.talentTrees)
                {
                    if (t.talentTreeID == ID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool skillsHaveTalentTree(int ID)
        {
            foreach (var t1 in CharacterData.Instance.skillsDATA)
            {
                RPGSkill skillREF = RPGBuilderUtilities.GetSkillFromID(t1.skillID);
                foreach (var t in skillREF.talentTrees)
                {
                    if (t.talentTreeID == ID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool characterHasTalentTree(int ID, CharacterData charData)
        {
            foreach (var t in charData.talentTrees)
                if (t.treeID == ID)
                    return true;

            return false;
        }

        private bool characterHasAbility(int ID, CharacterData charData)
        {
            foreach (var t in charData.abilitiesData)
                if (t.ID == ID)
                    return true;

            return false;
        }
        private bool characterHasRecipe(int ID, CharacterData charData)
        {
            foreach (var t in charData.recipesData)
                if (t.ID == ID)
                    return true;

            return false;
        }
        private bool characterHasResourceNode(int ID, CharacterData charData)
        {
            foreach (var t in charData.resourceNodeData)
                if (t.ID == ID)
                    return true;

            return false;
        }
        private bool characterHasBonus(int ID, CharacterData charData)
        {
            foreach (var t in charData.bonusesData)
                if (t.ID == ID)
                    return true;

            return false;
        }
        private bool characterHasFaction(int ID, CharacterData charData)
        {
            foreach (var t in charData.factionsData)
                if (t.ID == ID)
                    return true;

            return false;
        }

        private bool abilityFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var abilityData in charData.abilitiesData)
            {
                if(abilityData.ID != ID) continue;
                foreach (var existingAbility in RPGBuilderEssentials.Instance.allAbilities)
                {
                    if (existingAbility.ID == abilityData.ID) return true;
                }
            }
            return false;
        }
        
        private bool recipeFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var recipeData in charData.recipesData)
            {
                if(recipeData.ID != ID) continue;
                foreach (var existingRecipe in RPGBuilderEssentials.Instance.allCraftingRecipes)
                {
                    if (existingRecipe.ID == recipeData.ID) return true;
                }
            }
            return false;
        }
        
        private bool resourceNodeFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var resourceNodeData in charData.resourceNodeData)
            {
                if(resourceNodeData.ID != ID) continue;
                foreach (var existingResourceNode in RPGBuilderEssentials.Instance.allResourceNodes)
                {
                    if (existingResourceNode.ID == resourceNodeData.ID) return true;
                }
            }
            return false;
        }
        private bool bonusFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var bonusData in charData.bonusesData)
            {
                if(bonusData.ID != ID) continue;
                foreach (var existingBonus in RPGBuilderEssentials.Instance.allBonuses)
                {
                    if (existingBonus.ID == bonusData.ID) return true;
                }
            }
            return false;
        }
        private bool factionFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var factionData in charData.factionsData)
            {
                if(factionData.ID != ID) continue;
                foreach (var existingFaction in RPGBuilderEssentials.Instance.allFactions)
                {
                    if (existingFaction.ID == factionData.ID) return true;
                }
            }
            return false;
        }

        private bool characterHasTreePoint(int ID, CharacterData charData)
        {
            foreach (var t in charData.treePoints)
                if (t.treePointID == ID)
                    return true;

            return false;
        }

        private bool treePointFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var treePointData in charData.treePoints)
            {
                if(treePointData.treePointID != ID) continue;
                foreach (var existingTreePoint in RPGBuilderEssentials.Instance.allTreePoints)
                {
                    if (existingTreePoint.ID == treePointData.treePointID) return true;
                }
            }
            return false;
        }

        private bool currencyFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var currencyData in charData.currencies)
            {
                if (currencyData.currencyID != ID) continue;
                foreach (var existingCurrencies in RPGBuilderEssentials.Instance.allCurrencies)
                {
                    if (existingCurrencies.ID == currencyData.currencyID) return true;
                }
            }

            return false;
        }

        private bool skillFromCharStillExist(int ID, CharacterData charData)
        {
            foreach (var skillData in charData.skillsDATA)
            {
                if (skillData.skillID != ID) continue;
                foreach (var existringSkill in RPGBuilderEssentials.Instance.allSkills)
                {
                    if (existringSkill.ID == skillData.skillID) return true;
                }
            }
            return false;
        }
        
        private bool actionKeyStillExist(string actionKeyName)
        {
            foreach (var actionKey in RPGBuilderEssentials.Instance.generalSettings.actionKeys)
            {
                if (actionKey.actionName != actionKeyName) continue;
                return true;
            }
            return false;
        }

        private bool characterHasCurrency(int ID, CharacterData charData)
        {
            foreach (var t in charData.currencies)
                if (t.currencyID == ID)
                    return true;

            return false;
        }

        private bool characterHasSkill(int ID, CharacterData charData)
        {
            foreach (var t in charData.skillsDATA)
                if (t.skillID == ID)
                    return true;

            return false;
        }
        private bool characterHasActionKey(string actionKeyName, CharacterData charData)
        {
            foreach (var t in charData.actionKeys)
                if (t.actionKeyName == actionKeyName)
                    return true;

            return false;
        }
        private bool characterHasWeaponTemplate(int ID, CharacterData charData)
        {
            foreach (var t in charData.weaponTemplates)
                if (t.weaponTemplateID == ID)
                    return true;

            return false;
        }

        private bool characterHasTalentTreeNode(RPGTalentTree tree, RPGTalentTree.Node_DATA nodeDATA,
            CharacterData charData)
        {
            foreach (var t in charData.talentTrees)
            foreach (var t1 in t.nodes)
                if (t1.nodeData == nodeDATA)
                    return true;

            return false;
        }

        private bool talentTreeHasTalentTreeNode(RPGTalentTree tree, RPGTalentTree.Node_DATA nodeDATA,
            CharacterData charData)
        {
            for (var i = 0; i < tree.nodeList.Count; i++)
                foreach (var t in charData.talentTrees)
                    if (RPGBuilderUtilities.GetTalentTreeFromID(t.treeID) == tree)
                        for (var u = 0; u < t.nodes.Count; u++)
                            if (t.nodes[u].nodeData == nodeDATA)
                                return true;

            return false;
        }

        private void RemoveTalentTreeNodeFromCharacter(RPGTalentTree tree, RPGTalentTree.Node_DATA nodeDATA)
        {
            foreach (var t in CharacterData.Instance.talentTrees)
                if (RPGBuilderUtilities.GetTalentTreeFromID(t.treeID) == tree)
                    for (var x = 0; x < t.nodes.Count; x++)
                        if (t.nodes[x].nodeData == nodeDATA)
                            t.nodes.RemoveAt(x);
        }

        private bool TalentTreeHasChanged(RPGTalentTree treeREF, int charTalentTreeIndex)
        {
            for (var i = 0; i < treeREF.nodeList.Count; i++)
            {
                if (treeREF.nodeList.Count != CharacterData.Instance.talentTrees[charTalentTreeIndex].nodes.Count)
                    return true;
                if (treeREF.nodeList[i].nodeType !=
                    CharacterData.Instance.talentTrees[charTalentTreeIndex].nodes[i].nodeData.nodeType) return true;

                if (treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.ability &&
                    treeREF.nodeList[i].abilityID != CharacterData.Instance.talentTrees[charTalentTreeIndex].nodes[i]
                        .nodeData.abilityID
                    || treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.recipe &&
                    treeREF.nodeList[i].recipeID !=
                    CharacterData.Instance.talentTrees[charTalentTreeIndex].nodes[i].nodeData.recipeID
                    || treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                    treeREF.nodeList[i].resourceNodeID != CharacterData.Instance.talentTrees[charTalentTreeIndex].nodes[i]
                        .nodeData.resourceNodeID
                    || treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.bonus &&
                    treeREF.nodeList[i].bonusID !=
                    CharacterData.Instance.talentTrees[charTalentTreeIndex].nodes[i].nodeData.bonusID
                )
                    return true;
            }

            return false;
        }

        private void AddTalentTree(int treeID)
        {
            var newTalentTreeDATA = new CharacterData.TalentTrees_DATA();
            newTalentTreeDATA.treeID = treeID;
            var talentTreeREF = RPGBuilderUtilities.GetTalentTreeFromID(newTalentTreeDATA.treeID);

            foreach (var t in talentTreeREF.nodeList)
            {
                var newNodeDATA = new CharacterData.TalentTrees_DATA.TalentTreeNode_DATA();
                newNodeDATA.nodeData = new RPGTalentTree.Node_DATA();
                var learnedByDefault = false;
                int rank;
                switch (t.nodeType)
                {
                    case RPGTalentTree.TalentTreeNodeType.ability:
                    {
                        newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.ability;
                        newNodeDATA.nodeData.abilityID = t.abilityID;
                            
                        if (RPGBuilderUtilities.GetAbilityFromID(t.abilityID).learnedByDefault)
                        {
                            learnedByDefault = true;
                            rank = 1;
                        }
                        else
                        {
                            rank = 0;
                        }
                        RPGBuilderUtilities.setAbilityData(t.abilityID, rank, learnedByDefault);
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.recipe:
                    {
                        newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.recipe;
                        newNodeDATA.nodeData.recipeID = t.recipeID;
                            
                        if (RPGBuilderUtilities.GetCraftingRecipeFromID(t.recipeID).learnedByDefault)
                        {
                            learnedByDefault = true;
                            rank = 1;
                        }
                        else
                        {
                            rank = 0;
                        }
                        RPGBuilderUtilities.setRecipeData(t.recipeID, rank, learnedByDefault);
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    {
                        newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.resourceNode;
                        newNodeDATA.nodeData.resourceNodeID = t.resourceNodeID;
                            
                        if (RPGBuilderUtilities.GetResourceNodeFromID(t.resourceNodeID).learnedByDefault)
                        {
                            learnedByDefault = true;
                            rank = 1;
                        }
                        else
                        {
                            rank = 0;
                        }
                        RPGBuilderUtilities.setResourceNodeData(t.resourceNodeID, rank, learnedByDefault);
                        break;
                    }
                    case RPGTalentTree.TalentTreeNodeType.bonus:
                    {
                        newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.bonus;
                        newNodeDATA.nodeData.bonusID = t.bonusID;
                            
                        if (RPGBuilderUtilities.GetBonusFromID(t.bonusID).learnedByDefault)
                        {
                            learnedByDefault = true;
                            rank = 1;
                        }
                        else
                        {
                            rank = 0;
                        }
                        RPGBuilderUtilities.setBonusData(t.bonusID, rank, learnedByDefault);
                        break;
                    }
                }

                newTalentTreeDATA.nodes.Add(newNodeDATA);
            }

            CharacterData.Instance.talentTrees.Add(newTalentTreeDATA);
        }

        private void AddAbility(int ID)
        {
            var newEntry = new CharacterData.Ability_DATA();
            RPGAbility entryREF = RPGBuilderUtilities.GetAbilityFromID(ID);
            newEntry.name = entryREF._name;
            newEntry.ID = ID;
            if (entryREF.learnedByDefault)
            {
                newEntry.known = true;
                newEntry.rank = 1;
            }

            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                foreach (var spellbook in RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                    .spellbooks)
                {
                    RPGSpellbook spellbookREF = RPGBuilderUtilities.GetSpellbookFromID(spellbook.spellbookID);
                    foreach (var spellbookNode in spellbookREF.nodeList)
                    {
                        if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.ability) continue;
                        if (spellbookNode.abilityID != ID) continue;
                        if (CharacterData.Instance.classDATA.currentClassLevel < spellbookNode.unlockLevel) continue;
                        newEntry.known = true;
                        newEntry.rank = 1;
                    }
                }
            }

            foreach (var t in CharacterData.Instance.weaponTemplates)
            {
                foreach (var spellbook in RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID).spellbooks)
                {
                    RPGSpellbook spellbookREF = RPGBuilderUtilities.GetSpellbookFromID(spellbook.spellbookID);
                    foreach (var spellbookNode in spellbookREF.nodeList)
                    {
                        if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.ability) continue;
                        if (spellbookNode.abilityID != ID) continue;
                        if (RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID) < spellbookNode.unlockLevel) continue;
                        newEntry.known = true;
                        newEntry.rank = 1;
                    }
                }
            }

            CharacterData.Instance.abilitiesData.Add(newEntry);
        }
        
        private void AddRecipe(int ID)
        {
            var newEntry = new CharacterData.Recipe_DATA();
            RPGCraftingRecipe entryREF = RPGBuilderUtilities.GetCraftingRecipeFromID(ID);
            newEntry.name = entryREF._name;
            newEntry.ID = ID;
            if (entryREF.learnedByDefault)
            {
                newEntry.known = true;
                newEntry.rank = 1;
            }
            CharacterData.Instance.recipesData.Add(newEntry);
        }
        private void AddResourceNode(int ID)
        {
            var newEntry = new CharacterData.ResourceNode_DATA();
            RPGResourceNode entryREF = RPGBuilderUtilities.GetResourceNodeFromID(ID);
            newEntry.name = entryREF._name;
            newEntry.ID = ID;
            if (entryREF.learnedByDefault)
            {
                newEntry.known = true;
                newEntry.rank = 1;
            }
            CharacterData.Instance.resourceNodeData.Add(newEntry);
        }
        private void AddBonus(int ID)
        {
            var newEntry = new CharacterData.Bonus_DATA();
            RPGBonus entryREF = RPGBuilderUtilities.GetBonusFromID(ID);
            newEntry.name = entryREF._name;
            newEntry.ID = ID;
            if (entryREF.learnedByDefault)
            {
                newEntry.known = true;
                newEntry.rank = 1;
            }
            newEntry.On = false;

            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                foreach (var spellbook in RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                    .spellbooks)
                {
                    RPGSpellbook spellbookREF = RPGBuilderUtilities.GetSpellbookFromID(spellbook.spellbookID);
                    foreach (var spellbookNode in spellbookREF.nodeList)
                    {
                        if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.bonus) continue;
                        if (spellbookNode.bonusID != ID) continue;
                        if (CharacterData.Instance.classDATA.currentClassLevel < spellbookNode.unlockLevel) continue;
                        newEntry.known = true;
                        newEntry.rank = 1;
                    }
                }
            }

            foreach (var t in CharacterData.Instance.weaponTemplates)
            {
                foreach (var spellbook in RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID).spellbooks)
                {
                    RPGSpellbook spellbookREF = RPGBuilderUtilities.GetSpellbookFromID(spellbook.spellbookID);
                    foreach (var spellbookNode in spellbookREF.nodeList)
                    {
                        if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.bonus) continue;
                        if (spellbookNode.bonusID != ID) continue;
                        if (RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID) < spellbookNode.unlockLevel) continue;
                        newEntry.known = true;
                        newEntry.rank = 1;
                    }
                }
            }

            CharacterData.Instance.bonusesData.Add(newEntry);
        }
        
        
        private void AddFaction(int ID)
        {
            var newEntry = new CharacterData.Faction_DATA();
            RPGFaction entryREF = RPGBuilderUtilities.GetFactionFromID(ID);
            newEntry.name = entryREF._name;
            newEntry.ID = ID;
            newEntry.currentStance = "";
            foreach (var t in RPGBuilderUtilities.GetFactionFromID(RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).factionID).factionInteractions)
            {
                if(t.factionID != ID) continue;
                newEntry.currentStance = t.defaultStance;
            }
            newEntry.currentPoint = 0;

            CharacterData.Instance.factionsData.Add(newEntry);
        }

        private void AddTreePoint(RPGTreePoint treePoint)
        {
            var newTreePointData = new CharacterData.TreePoints_DATA();
            newTreePointData.treePointID = treePoint.ID;
            newTreePointData.amount = (int)GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.TreePoint + "+" +
                RPGGameModifier.PointModifierType.Start_At,
                treePoint.startAmount, treePoint.ID, -1);
            CharacterData.Instance.treePoints.Add(newTreePointData);
        }

        private void AddCurrency(RPGCurrency currency)
        {
            var newCurrencyData = new CharacterData.Currencies_DATA();
            newCurrencyData.currencyID = currency.ID;
            newCurrencyData.amount = currency.baseValue;
            CharacterData.Instance.currencies.Add(newCurrencyData);
        }

        private void AddSkill(RPGSkill skillREF)
        {
            var newSkillData = new CharacterData.SkillsDATA();
            newSkillData.currentSkillLevel = 1;
            newSkillData.currentSkillXP = 0;
            newSkillData.skillID = skillREF.ID;
            newSkillData.maxSkillXP =
                RPGBuilderUtilities.GetLevelTemplateFromID(skillREF.levelTemplateID).allLevels[0].XPRequired;
            foreach (var t in skillREF.talentTrees)
                AddTalentTree(t.talentTreeID);

            CharacterData.Instance.skillsDATA.Add(newSkillData);
        }
        private void AddWeaponTemplate(RPGWeaponTemplate weaponTemplateREF)
        {
            var newWeaponTemplateData = new CharacterData.WeaponTemplatesData();
            newWeaponTemplateData.currentWeaponLevel = 1;
            newWeaponTemplateData.currentWeaponXP = 0;
            RPGLevelsTemplate lvlTemplateRef = RPGBuilderUtilities.GetLevelTemplateFromID(weaponTemplateREF.levelTemplateID);
            newWeaponTemplateData.maxWeaponXP = lvlTemplateRef.allLevels[0].XPRequired;
            newWeaponTemplateData.weaponTemplateID = weaponTemplateREF.ID;
            CharacterData.Instance.weaponTemplates.Add(newWeaponTemplateData);
        }

        private void AddActionKey(RPGGeneralDATA.ActionKey actionKey)
        {
            var newKeybind = new CharacterData.ActionKeyDATA();
            newKeybind.actionKeyName = actionKey.actionName;
            newKeybind.currentKey = actionKey.defaultKey;
            CharacterData.Instance.actionKeys.Add(newKeybind);
        }

        private void HandleBackdating()
        {
            var classRef = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
            for (var i = 0; i < CharacterData.Instance.talentTrees.Count; i++)
                if (!classHasTalentTree(CharacterData.Instance.talentTrees[i].treeID, classRef) &&
                    !skillsHaveTalentTree(CharacterData.Instance.talentTrees[i].treeID)
                    && !weaponTemplateHasTalentTree(CharacterData.Instance.talentTrees[i].treeID))
                {
                    // If character had a talent tree that was not anymore existing on this class or skills, remove it
                    CharacterData.Instance.talentTrees.RemoveAt(i);
                }
                else
                {
                    // If character had a talent tree that has been modified, remove it and add the modified one instead, also refund points spent
                    var treeREF =
                        RPGBuilderUtilities.GetTalentTreeFromID(CharacterData.Instance.talentTrees[i].treeID);
                    if (!TalentTreeHasChanged(treeREF, i)) continue;
                    TreePointsManager.Instance.AddTreePoint(treeREF.treePointAcceptedID,
                        CharacterData.Instance.talentTrees[i].pointsSpent);
                    CharacterData.Instance.talentTrees.RemoveAt(i);
                    AddTalentTree(treeREF.ID);
                }


            if (RPGBuilderEssentials.Instance.combatSettings.useClasses)
            {
                foreach (var t in classRef.talentTrees)
                    if (!characterHasTalentTree(t.talentTreeID, CharacterData.Instance))
                        // If class has a talent tree that is not on this character, add it
                        AddTalentTree(t.talentTreeID);
            }

            foreach (var t1 in CharacterData.Instance.skillsDATA)
            {
                RPGSkill skillREF = RPGBuilderUtilities.GetSkillFromID(t1.skillID);
                foreach (var t in skillREF.talentTrees)
                    if (!characterHasTalentTree(t.talentTreeID, CharacterData.Instance))
                        // If skill has a talent tree that is not on this character, add it
                        AddTalentTree(t.talentTreeID);
            }

            var raceRef = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
            foreach (var t1 in raceRef.weaponTemplates)
            {
                RPGWeaponTemplate REF = RPGBuilderUtilities.GetWeaponTemplateFromID(t1.weaponTemplateID);
                foreach (var t in REF.talentTrees)
                    if (!characterHasTalentTree(t.talentTreeID, CharacterData.Instance))
                        // If skill has a talent tree that is not on this character, add it
                        AddTalentTree(t.talentTreeID);
            }


            foreach (var t in RPGBuilderEssentials.Instance.allAbilities)
                if (!characterHasAbility(t.ID, CharacterData.Instance))
                    AddAbility(t.ID);
            foreach (var t in RPGBuilderEssentials.Instance.allCraftingRecipes)
                if (!characterHasRecipe(t.ID, CharacterData.Instance))
                    AddRecipe(t.ID);
            foreach (var t in RPGBuilderEssentials.Instance.allResourceNodes)
                if (!characterHasResourceNode(t.ID, CharacterData.Instance))
                    AddResourceNode(t.ID);
            foreach (var t in RPGBuilderEssentials.Instance.allBonuses)
                if (!characterHasBonus(t.ID, CharacterData.Instance))
                    AddBonus(t.ID);

            foreach (var t in RPGBuilderEssentials.Instance.allFactions)
                if (!characterHasFaction(t.ID, CharacterData.Instance))
                    AddFaction(t.ID);

            for (var i = 0; i < CharacterData.Instance.abilitiesData.Count; i++)
                if (!abilityFromCharStillExist(CharacterData.Instance.abilitiesData[i].ID, CharacterData.Instance))
                    CharacterData.Instance.abilitiesData.RemoveAt(i);
            for (var i = 0; i < CharacterData.Instance.recipesData.Count; i++)
                if (!recipeFromCharStillExist(CharacterData.Instance.recipesData[i].ID, CharacterData.Instance))
                    CharacterData.Instance.recipesData.RemoveAt(i);
            for (var i = 0; i < CharacterData.Instance.resourceNodeData.Count; i++)
                if (!resourceNodeFromCharStillExist(CharacterData.Instance.resourceNodeData[i].ID,
                    CharacterData.Instance))
                    CharacterData.Instance.resourceNodeData.RemoveAt(i);
            for (var i = 0; i < CharacterData.Instance.bonusesData.Count; i++)
                if (!bonusFromCharStillExist(CharacterData.Instance.bonusesData[i].ID, CharacterData.Instance))
                    CharacterData.Instance.bonusesData.RemoveAt(i);

            for (var i = 0; i < CharacterData.Instance.factionsData.Count; i++)
                if (!factionFromCharStillExist(CharacterData.Instance.factionsData[i].ID, CharacterData.Instance))
                    CharacterData.Instance.factionsData.RemoveAt(i);

            foreach (var t in RPGBuilderEssentials.Instance.allTreePoints)
                if (!characterHasTreePoint(t.ID, CharacterData.Instance))
                    AddTreePoint(t);

            for (var i = 0; i < CharacterData.Instance.treePoints.Count; i++)
                if (!treePointFromCharStillExist(CharacterData.Instance.treePoints[i].treePointID,
                    CharacterData.Instance))
                    CharacterData.Instance.treePoints.RemoveAt(i);

            foreach (var t in RPGBuilderEssentials.Instance.allCurrencies)
                if (!characterHasCurrency(t.ID, CharacterData.Instance))
                    AddCurrency(t);

            for (var i = 0; i < CharacterData.Instance.currencies.Count; i++)
                if (!currencyFromCharStillExist(CharacterData.Instance.currencies[i].currencyID,
                    CharacterData.Instance))
                    CharacterData.Instance.currencies.RemoveAt(i);

            foreach (var t in RPGBuilderEssentials.Instance.allSkills)
                if (!characterHasSkill(t.ID, CharacterData.Instance))
                    if (t.automaticlyAdded)
                        AddSkill(t);

            foreach (var t in raceRef.weaponTemplates)
                if (!characterHasWeaponTemplate(t.weaponTemplateID, CharacterData.Instance))
                    AddWeaponTemplate(RPGBuilderUtilities.GetWeaponTemplateFromID(t.weaponTemplateID));

            for (var i = 0; i < CharacterData.Instance.skillsDATA.Count; i++)
                if (!skillFromCharStillExist(CharacterData.Instance.skillsDATA[i].skillID, CharacterData.Instance))
                    CharacterData.Instance.skillsDATA.RemoveAt(i);
            
            for (var i = 0; i < CharacterData.Instance.actionKeys.Count; i++)
                if (!actionKeyStillExist(CharacterData.Instance.actionKeys[i].actionKeyName))
                    CharacterData.Instance.actionKeys.RemoveAt(i);
            
            foreach (var t in RPGBuilderEssentials.Instance.generalSettings.actionKeys)
                if (!characterHasActionKey(t.actionName, CharacterData.Instance))
                    AddActionKey(t);
            
            
            if (RPGBuilderEssentials.Instance.itemSettings.InventorySlots > CharacterData.Instance.inventoryData.baseSlots.Count)
            {
                int diff = RPGBuilderEssentials.Instance.itemSettings.InventorySlots - CharacterData.Instance.inventoryData.baseSlots.Count;
                for (int i = 0; i < diff; i++)
                {
                    CharacterData.Instance.inventoryData.baseSlots.Add(new CharacterData.InventorySlotDATA());
                }
            }
            else if(RPGBuilderEssentials.Instance.itemSettings.InventorySlots < CharacterData.Instance.inventoryData.baseSlots.Count)
            {
                int diff = CharacterData.Instance.inventoryData.baseSlots.Count - RPGBuilderEssentials.Instance.itemSettings.InventorySlots;
                for (int i = 0; i < diff; i++)
                {
                    CharacterData.Instance.inventoryData.baseSlots.RemoveAt(CharacterData.Instance.inventoryData.baseSlots.Count-1);
                }  
            }

            RPGBuilderJsonSaver.SaveCharacterData(CharacterData.Instance.CharacterName, CharacterData.Instance);
        }

        public void SelectCharacter(string characterName)
        {
            CharacterData.Instance.RESET_CHARACTER_DATA(false);
            var curCharCbtData = RPGBuilderJsonSaver.LoadCharacterData(characterName);
            curSelectedCharacterNameText.text = curCharCbtData.CharacterName;
            AssignCharacterDATA(curCharCbtData);

            if (curPlayerModel != null) Destroy(curPlayerModel);

            if (CharacterData.Instance.gender == RPGRace.RACE_GENDER.Male)
            {
                curGenderSlots[0].selectedBorder.color = slotSelectedColor;
                curPlayerModel = Instantiate(
                    RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).malePrefab,
                    Vector3.zero, Quaternion.identity);
            }
            else
            {
                curGenderSlots[1].selectedBorder.color = slotSelectedColor;
                curPlayerModel = Instantiate(
                    RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).femalePrefab,
                    Vector3.zero, Quaternion.identity);
            }

            curPlayerModel.transform.SetParent(characterModelSpot);
            curPlayerModel.transform.localPosition = Vector3.zero;
            curPlayerModel.transform.localRotation = Quaternion.identity;
            
            // Preventing combat and movement actions on the character in main menu
            Destroy(curPlayerModel.GetComponent<CombatNode>());
            Destroy(curPlayerModel.GetComponent<PlayerAnimatorLayerHandler>());
            
            RPGBCharacterControllerEssentials controllerEssentialREF =
                curPlayerModel.GetComponent<RPGBCharacterControllerEssentials>();
            controllerEssentialREF.MainMenuInit();

            var appearanceref = curPlayerModel.GetComponent<PlayerAppearanceHandler>();

            for (var i = 0; i < CharacterData.Instance.armorsEquipped.Count; i++)
                if (CharacterData.Instance.armorsEquipped[i].itemID != -1)
                    InventoryManager.Instance.InitEquipItemMainMenu(
                        RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.armorsEquipped[i].itemID),
                        appearanceref, i, CharacterData.Instance.armorsEquipped[i].itemDataID);

            for (var i = 0; i < CharacterData.Instance.weaponsEquipped.Count; i++)
                if (CharacterData.Instance.weaponsEquipped[i].itemID != -1)
                    InventoryManager.Instance.InitEquipItemMainMenu(
                        RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.weaponsEquipped[i].itemID),
                        appearanceref, i, CharacterData.Instance.weaponsEquipped[i].itemDataID);
        }

        private int getRaceIDByName(string raceName)
        {
            for (var i = 0; i < RPGBuilderEssentials.Instance.allRaces.Count; i++)
                if (RPGBuilderEssentials.Instance.allRaces[i]._name == raceName)
                    return i;
            return -1;
        }

        private CharacterData getCharacterDataByName(string characterName)
        {
            foreach (var t in allCharacters)
                if (t.CharacterName == characterName)
                    return t;

            return null;
        }

        private void ShowPopupMessage(string message)
        {
            RPGBuilderUtilities.EnableCG(errorPopupCG);
            popupMessageText.text = message;
        }

        public void HidePopupMessage()
        {
            RPGBuilderUtilities.DisableCG(errorPopupCG);
        }


        private void ClearModifierSlots(List<GameModifierSlotDataHolder> slotList)
        {
            foreach (var slot  in slotList)
            {
                Destroy(slot.gameObject);
            }
            slotList.Clear();
        }
        private void RemoveModifierSlots(List<GameModifierSlotDataHolder> slotList, RPGGameModifier gameModifier)
        {
            for (var index = 0; index < slotList.Count; index++)
            {
                var slot = slotList[index];
                if (slot.thisModifier != gameModifier) continue;
                Destroy(slot.gameObject);
                slotList.Remove(slot);
                return;
            }
        }
        
        public void InitializeModifierPanel()
        {
            ClearAllModifierSlots();
            RPGBuilderUtilities.EnableCG(modifiersCG);

            modifiersPointsText.text = CharacterData.Instance.menuGameModifierPoints.ToString();
            
            foreach (var gameModifier in RPGBuilderEssentials.Instance.allGameModifiers)
            {
                if (RPGBuilderUtilities.isGameModifierOn(gameModifier.ID))
                {
                    if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                    {
                        SpawnGameModifierSlot(chosenNegativeModifiersParent, currentChosenNegativeModifiersSlots, gameModifier);
                    }
                    else
                    {
                        SpawnGameModifierSlot(chosenPositiveModifiersParent, currentChosenPositiveModifiersSlots, gameModifier);
                    }
                }
                else
                {
                    if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                    {
                        SpawnGameModifierSlot(availableNegativeModifiersParent, currentAvailableNegativeModifiersSlots, gameModifier);
                    }
                    else
                    {
                        SpawnGameModifierSlot(availablePositiveModifiersParent, currentAvailablePositiveModifiersSlots, gameModifier);
                    }
                }
            }
        }

        private void SpawnGameModifierSlot(Transform parent, List<GameModifierSlotDataHolder> slotList, RPGGameModifier gameModifierRef)
        {
            GameObject slot = Instantiate(gameModifierSlot, Vector3.zero, Quaternion.identity, parent);
            GameModifierSlotDataHolder slotRef = slot.GetComponent<GameModifierSlotDataHolder>();
            slotList.Add(slotRef);
            slotRef.thisModifier = gameModifierRef;
            slotRef.nameText.text = gameModifierRef.displayName;
            if (gameModifierRef.icon != null)
            {
                slotRef.icon.sprite = gameModifierRef.icon;
            }
            else
            {
                slotRef.icon.enabled = false;
            }
            if (gameModifierRef.gameModifierType == RPGGameModifier.GameModifierType.Negative)
            {
                slotRef.nameText.color = modifierNegativeColor;
                slotRef.costText.text = "+ " + gameModifierRef.gain;
                slotRef.costText.color = modifierPositiveColor;
            }
            else
            {
                slotRef.nameText.color = modifierPositiveColor;
                slotRef.costText.text = "- " + gameModifierRef.cost;
                slotRef.costText.color = modifierNegativeColor;
            }
        }

        private void ClearAllModifierSlots()
        {
            ClearModifierSlots(currentAvailableNegativeModifiersSlots);
            ClearModifierSlots(currentAvailablePositiveModifiersSlots);
            ClearModifierSlots(currentChosenNegativeModifiersSlots);
            ClearModifierSlots(currentChosenPositiveModifiersSlots);
        }
        
        public void ResetModifierPnanel()
        {
            RPGBuilderUtilities.DisableCG(modifiersCG);
            ClearAllModifierSlots();

            modifiersPointsText.text = "" + 0;
        }

        public void ClickGameModifierSlot(RPGGameModifier gameModifier)
        {
            if (RPGBuilderUtilities.isGameModifierOn(gameModifier.ID))
            {
                RemoveModifierFromCharacterData(gameModifier);
            }
            else
            {
                // THIS IS AN AVAILABLE MODIFIER
                if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                {
                    // ADD TO CHARACTER DATA
                    if (RPGBuilderUtilities.isGameModifierAdded(gameModifier.ID))
                    {
                        RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, true);
                        CharacterData.Instance.menuGameModifierPoints += gameModifier.gain;
                    }
                    else
                    {
                        AddModifierToCharacterData(gameModifier);
                    }
                }
                else
                {
                    if (CharacterData.Instance.menuGameModifierPoints < gameModifier.cost)
                    {
                        ShowPopupMessage("You do not have enough points");
                        return;
                    }
                    if (RPGBuilderEssentials.Instance.generalSettings.checkMaxPositiveModifier &&  RPGBuilderUtilities.getPositiveModifiersCount() >= RPGBuilderEssentials.Instance.generalSettings
                        .maximumRequiredPositiveGameModifiers)
                    {
                        ShowPopupMessage("You already have the maximum amounts of positive modifiers");
                        return;
                    }
                    
                    // ADD TO CHARACTER DATA
                    if (RPGBuilderUtilities.isGameModifierAdded(gameModifier.ID))
                    {
                        RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, true);
                        CharacterData.Instance.menuGameModifierPoints -= gameModifier.cost;
                    }
                    else
                    {
                        AddModifierToCharacterData(gameModifier);
                    }
                }
            }
            
            InitializeModifierPanel();
        }

        private void AddModifierToCharacterData(RPGGameModifier gameModifier)
        {
            // ADD TO CHARACTER DATA
            CharacterData.GameModifier_DATA newGameModData = new CharacterData.GameModifier_DATA();
            newGameModData.name = gameModifier._name;
            newGameModData.ID = gameModifier.ID;
            CharacterData.Instance.gameModifiersData.Add(newGameModData);
            RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, true);
            // ADD POINTS
            if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
            {
                CharacterData.Instance.menuGameModifierPoints += gameModifier.gain;
            }
            else
            {
                CharacterData.Instance.menuGameModifierPoints -= gameModifier.cost;
            }
        }

        private void RemoveModifierFromCharacterData(RPGGameModifier gameModifier)
        {
            // ADD TO CHARACTER DATA
            foreach (var gameMod in CharacterData.Instance.gameModifiersData)
            {
                if(gameMod.ID != gameModifier.ID) continue;
                
                // REFUND POINTS
                if (gameModifier.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                {
                    int positivePointsRequired = getTotalPointsRequiredForPositiveModifiers();
                    int negativePointsGain = getTotalGainedPointsFromNegativeModifiers();
                    negativePointsGain -= gameModifier.gain;
                    if (positivePointsRequired == 0 || negativePointsGain >= positivePointsRequired)
                    {
                        CharacterData.Instance.menuGameModifierPoints -= gameModifier.gain;
                        RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, false);
                    }
                    else
                    {
                        ShowPopupMessage("Positive modifiers require these points");
                        return;
                    }
                }
                else
                {
                    CharacterData.Instance.menuGameModifierPoints += gameModifier.cost;
                    RPGBuilderUtilities.setGameModifierOnState(gameModifier.ID, false);
                }
            }
        }

        private int getTotalPointsRequiredForPositiveModifiers()
        {
            int total = 0;
            foreach (var gameMod in CharacterData.Instance.gameModifiersData)
            {
                if(!gameMod.On) continue;
                RPGGameModifier gameModifierRef = RPGBuilderUtilities.GetGameModifierFromID(gameMod.ID);
                if (gameModifierRef.gameModifierType == RPGGameModifier.GameModifierType.Positive)
                {
                    total += gameModifierRef.cost;
                }
            }

            return total;
        }

        private int getTotalGainedPointsFromNegativeModifiers()
        {
            int total = 0;
            foreach (var gameMod in CharacterData.Instance.gameModifiersData)
            {
                if(!gameMod.On) continue;
                RPGGameModifier gameModifierRef = RPGBuilderUtilities.GetGameModifierFromID(gameMod.ID);
                if (gameModifierRef.gameModifierType == RPGGameModifier.GameModifierType.Negative)
                {
                    total += gameModifierRef.gain;
                }
            }

            return total;
        }
        
        public void OpenBlinkStore() {
            Application.OpenURL("https://assetstore.unity.com/publishers/49855");
        }
    }
}