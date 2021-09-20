using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.LogicMono
{
    public class RPGBuilderEssentials : MonoBehaviour
    {
        private static RPGBuilderEssentials instance;

        public GameObject temporaryDataGO;
        public Canvas mainGameCanvas;

        private float nextAutomaticSave = 0;
    
        public RPGBuilderEditorDATA editorDATA;
        public RPGGeneralDATA generalSettings;
        public RPGItemDATA itemSettings;
        public RPGCombatDATA combatSettings;

        public List<RPGAbility> allAbilities;
        public List<RPGEffect> allEffects;
        public List<RPGNpc> allNPCs;
        public List<RPGStat> allStats;
        public List<RPGTreePoint> allTreePoints;
        public List<RPGSpellbook> allSpellbooks;
        public List<RPGFaction> allFactions;
        public List<RPGWeaponTemplate> allWeaponTemplates;
        public List<RPGSpecies> allSpecies;
        public List<RPGCombo> allCombos;

        public List<RPGItem> allItems;
        public List<RPGSkill> allSkills;
        public List<RPGLevelsTemplate> allLevelTemplates;
        public List<RPGRace> allRaces;
        public List<RPGClass> allClasses;
        public List<RPGLootTable> allLootTables;
        public List<RPGMerchantTable> allMerchantTables;
        public List<RPGCurrency> allCurrencies;
        public List<RPGCraftingRecipe> allCraftingRecipes;
        public List<RPGCraftingStation> allCraftingStation;
        public List<RPGTalentTree> allTalentTrees;
        public List<RPGBonus> allBonuses;
        public List<RPGGearSet> allGearSets;
        public List<RPGEnchantment> allEnchantments;

        public List<RPGTask> allTasks;
        public List<RPGQuest> allQuests;
        public List<RPGWorldPosition> allWorldPositions;
        public List<RPGResourceNode> allResourceNodes;
        public List<RPGGameScene> allGameScenes;
        public List<RPGDialogue> allDialogues;
        public List<RPGGameModifier> allGameModifiers;

        public List<RPGAbility.RPGAbilityRankData> allAbilityRanks;

        public RPGStat healthStatReference;
        public RPGStat sprintStatDrainReference;

        public bool isInGame;
    
        public static RPGBuilderEssentials Instance => instance;

        private void LoadDATA()
        {
            editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
            allAbilities = Resources.LoadAll<RPGAbility>(  editorDATA.RPGBDatabasePath + "Abilities").ToList();
            allEffects = Resources.LoadAll<RPGEffect>(editorDATA.RPGBDatabasePath + "Effects").ToList();
            allNPCs = Resources.LoadAll<RPGNpc>(editorDATA.RPGBDatabasePath + "NPCs").ToList();
            allStats = Resources.LoadAll<RPGStat>(editorDATA.RPGBDatabasePath + "Stats").ToList();
            allTreePoints = Resources.LoadAll<RPGTreePoint>(editorDATA.RPGBDatabasePath + "TreePoints").ToList();
            allSpellbooks = Resources.LoadAll<RPGSpellbook>(editorDATA.RPGBDatabasePath + "Spellbooks").ToList();
            allFactions = Resources.LoadAll<RPGFaction>(editorDATA.RPGBDatabasePath + "Factions").ToList();
            allWeaponTemplates = Resources.LoadAll<RPGWeaponTemplate>(editorDATA.RPGBDatabasePath + "WeaponTemplates").ToList();
            allSpecies = Resources.LoadAll<RPGSpecies>(editorDATA.RPGBDatabasePath + "Species").ToList();
            allCombos = Resources.LoadAll<RPGCombo>(editorDATA.RPGBDatabasePath + "Combos").ToList();

            allItems = Resources.LoadAll<RPGItem>(editorDATA.RPGBDatabasePath + "Items").ToList();
            allSkills = Resources.LoadAll<RPGSkill>(editorDATA.RPGBDatabasePath + "Skills").ToList();
            allLevelTemplates = Resources.LoadAll<RPGLevelsTemplate>(editorDATA.RPGBDatabasePath + "LevelsTemplate").ToList();
            allRaces = Resources.LoadAll<RPGRace>(editorDATA.RPGBDatabasePath + "Races").ToList();
            allClasses = Resources.LoadAll<RPGClass>(editorDATA.RPGBDatabasePath + "Classes").ToList();
            allLootTables = Resources.LoadAll<RPGLootTable>(editorDATA.RPGBDatabasePath + "LootTables").ToList();
            allMerchantTables = Resources.LoadAll<RPGMerchantTable>(editorDATA.RPGBDatabasePath + "MerchantTables").ToList();
            allCurrencies = Resources.LoadAll<RPGCurrency>(editorDATA.RPGBDatabasePath + "Currencies").ToList();
            allCraftingRecipes = Resources.LoadAll<RPGCraftingRecipe>(editorDATA.RPGBDatabasePath + "CraftingRecipes").ToList();
            allCraftingStation = Resources.LoadAll<RPGCraftingStation>(editorDATA.RPGBDatabasePath + "CraftingStation").ToList();
            allTalentTrees = Resources.LoadAll<RPGTalentTree>(editorDATA.RPGBDatabasePath + "TalentTrees").ToList();
            allBonuses = Resources.LoadAll<RPGBonus>(editorDATA.RPGBDatabasePath + "Bonuses").ToList();
            allGearSets = Resources.LoadAll<RPGGearSet>(editorDATA.RPGBDatabasePath + "GearSets").ToList();
            allEnchantments = Resources.LoadAll<RPGEnchantment>(editorDATA.RPGBDatabasePath + "Enchantments").ToList();

            allTasks = Resources.LoadAll<RPGTask>(editorDATA.RPGBDatabasePath + "Tasks").ToList();
            allQuests = Resources.LoadAll<RPGQuest>(editorDATA.RPGBDatabasePath + "Quests").ToList();
            allWorldPositions = Resources.LoadAll<RPGWorldPosition>(editorDATA.RPGBDatabasePath + "WorldPositions").ToList();
            allResourceNodes = Resources.LoadAll<RPGResourceNode>(editorDATA.RPGBDatabasePath + "ResourceNodes").ToList();
            allGameScenes = Resources.LoadAll<RPGGameScene>(editorDATA.RPGBDatabasePath + "GameScenes").ToList();
            allDialogues = Resources.LoadAll<RPGDialogue>(editorDATA.RPGBDatabasePath + "Dialogues").ToList();
            allGameModifiers = Resources.LoadAll<RPGGameModifier>(editorDATA.RPGBDatabasePath + "GameModifiers").ToList();

            itemSettings = Resources.Load<RPGItemDATA>(editorDATA.RPGBDatabasePath + "Settings/ItemSettings");
            generalSettings = Resources.Load<RPGGeneralDATA>(editorDATA.RPGBDatabasePath + "Settings/GeneralSettings");
            combatSettings = Resources.Load<RPGCombatDATA>(editorDATA.RPGBDatabasePath + "Settings/CombatSettings");

            healthStatReference = RPGBuilderUtilities.GetStatFromID(combatSettings.healthStatID);
            sprintStatDrainReference = RPGBuilderUtilities.GetStatFromID(combatSettings.sprintStatDrainID);
        }

        private void Start()
        {
            mainGameCanvas.enabled = false;
        }

        private void Awake()
        {
            if (instance != null) return;
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDATA();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    
        public Scene getCurrentScene()
        {
            return SceneManager.GetActiveScene();
        }

        public void TeleportToGameScene(int GameSceneID, Vector3 teleportPosition)
        {
            CharacterData.Instance.position = teleportPosition;
            LoadingScreenManager.Instance.LoadGameScene(GameSceneID);
        }

        public void HandleDATAReset ()
        {
            InventoryDisplayManager.Instance.ResetSlots();
            CombatManager.playerCombatNode = null;
            CombatManager.Instance.PlayerTargetData.currentTarget = null;
            CombatManager.Instance.ResetPlayerTarget();
            CombatManager.Instance.allCombatNodes.Clear();

            CursorManager.Instance.ResetCursor();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            QuestTrackerDisplayManager.Instance.trackedQuest.Clear();
        }

        private bool isGameScene(string sceneName)
        {
            foreach (var gameScene in allGameScenes)
            {
                if(gameScene._name != sceneName) continue;
                return true;
            }
            return false;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(InitializeGameState(scene));
        }

        private IEnumerator InitializeGameState(Scene scene)
        {
            yield return new WaitForSeconds(generalSettings.DelayAfterSceneLoad);
            if (scene.name != generalSettings.mainMenuSceneName && isGameScene(scene.name))
            {
                RPGGameScene gameSceneREF = RPGBuilderUtilities.GetGameSceneFromName(scene.name);
                CharacterData.Instance.currentGameSceneID = gameSceneREF.ID;
                mainGameCanvas.enabled = true;
                GameObject playerCharacter;
                Vector3 spawnPos = Vector3.zero;
                if (gameSceneREF.isProceduralScene && !string.IsNullOrEmpty(gameSceneREF.SpawnPointName))
                {
                    spawnPos = GameObject.Find(gameSceneREF.SpawnPointName).transform.position;
                }
                else
                {
                    spawnPos = CharacterData.Instance.position;
                }
                    
                playerCharacter = Instantiate(
                    CharacterData.Instance.gender == RPGRace.RACE_GENDER.Male ? RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).malePrefab :
                        RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).femalePrefab,
                    spawnPos, Quaternion.Euler(CharacterData.Instance.rotation));
                    
                CombatManager.playerCombatNode = playerCharacter.GetComponent<CombatNode>();
                if(combatSettings.useClasses)CombatManager.playerCombatNode.AutoAttackData.currentAutoAttackAbilityID = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID).autoAttackAbilityID;
                CombatManager.Instance.allCombatNodes.Add(CombatManager.playerCombatNode);
                CombatManager.playerCombatNode.InitStats();
                CombatManager.Instance.allGraveyards.Clear();
                CombatManager.Instance.allGraveyards = FindObjectsOfType<GraveyardHandler>().ToList();

                ScreenSpaceNameplates.Instance.InitCamera();
                ScreenSpaceWorldDroppedItems.Instance.InitCamera();
                
                PlayerInfoDisplayManager.Instance.Init();

                ActionBarManager.Instance.InitActionBar();
                if (CharacterData.Instance.actionBarSlotsDATA.Count == 0)
                {
                    foreach (var t in ActionBarManager.Instance.actionBarSlots)
                    {
                        CharacterData.Instance.actionBarSlotsDATA.Add(new CharacterData.ActionBarSlotDATA());
                        if (t.actionBarType ==
                            CharacterData.ActionBarType.Main)
                        {
                            CharacterData.Instance.stealthedActionBarSlotsDATA.Add(new CharacterData.ActionBarSlotDATA());
                        }
                    }
                }
                else
                {
                    if (ActionBarManager.Instance.actionBarSlots.Count > CharacterData.Instance.actionBarSlotsDATA.Count)
                    {
                        int diff = ActionBarManager.Instance.actionBarSlots.Count - CharacterData.Instance.actionBarSlotsDATA.Count;
                        for (int i = 0; i < diff; i++)
                        {
                            CharacterData.Instance.actionBarSlotsDATA.Add(new CharacterData.ActionBarSlotDATA());
                        }
                    }
                    else if(ActionBarManager.Instance.actionBarSlots.Count < CharacterData.Instance.actionBarSlotsDATA.Count)
                    {
                        int diff = CharacterData.Instance.actionBarSlotsDATA.Count - ActionBarManager.Instance.actionBarSlots.Count;
                        for (int i = 0; i < diff; i++)
                        {
                            CharacterData.Instance.actionBarSlotsDATA.RemoveAt(CharacterData.Instance.actionBarSlotsDATA.Count-1);
                        }  
                    }

                    int currentMainSlotCount = ActionBarManager.Instance.actionBarSlots.Count(t => t.actionBarType == CharacterData.ActionBarType.Main);

                    if (currentMainSlotCount != CharacterData.Instance.stealthedActionBarSlotsDATA.Count)
                    {
                        if (currentMainSlotCount > CharacterData.Instance.stealthedActionBarSlotsDATA.Count)
                        {
                            int diff = currentMainSlotCount - CharacterData.Instance.stealthedActionBarSlotsDATA.Count;
                            for (int i = 0; i < diff; i++)
                            {
                                CharacterData.Instance.stealthedActionBarSlotsDATA.Add(new CharacterData.ActionBarSlotDATA());
                            }
                        } else if (currentMainSlotCount < CharacterData.Instance.stealthedActionBarSlotsDATA.Count)
                        {
                            int diff = CharacterData.Instance.stealthedActionBarSlotsDATA.Count - currentMainSlotCount;
                            for (int i = 0; i < diff; i++)
                            {
                                CharacterData.Instance.stealthedActionBarSlotsDATA.RemoveAt(CharacterData.Instance.stealthedActionBarSlotsDATA.Count-1);
                            }  
                        }
                    }

                    for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
                    {
                        switch (CharacterData.Instance.actionBarSlotsDATA[i].contentType)
                        {
                            case CharacterData.ActionBarSlotContentType.None:
                                ActionBarManager.Instance.actionBarSlots[i].Reset();
                                continue;
                            case CharacterData.ActionBarSlotContentType.Ability:
                                if(!RPGBuilderUtilities.isAbilityKnown(CharacterData.Instance.actionBarSlotsDATA[i].ID))continue;
                                ActionBarManager.Instance.actionBarSlots[i].contentType = CharacterData.ActionBarSlotContentType.Ability;
                                ActionBarManager.Instance.actionBarSlots[i].ThisAbility = RPGBuilderUtilities.GetAbilityFromID(CharacterData.Instance.actionBarSlotsDATA[i].ID);
                                break;
                            case CharacterData.ActionBarSlotContentType.Item:
                                if(RPGBuilderUtilities.getItemCount(RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.actionBarSlotsDATA[i].ID)) == 0)continue;
                                ActionBarManager.Instance.actionBarSlots[i].contentType = CharacterData.ActionBarSlotContentType.Item;
                                ActionBarManager.Instance.actionBarSlots[i].ThisItem = RPGBuilderUtilities.GetItemFromID(CharacterData.Instance.actionBarSlotsDATA[i].ID);
                                break;
                        }
                    }
                }
                
                ActionBarManager.Instance.InitializeSlots();
                InventoryDisplayManager.Instance.InitInventory();
                
                MusicManager.Instance.InitializeSceneMusic();
                RegionManager.Instance.StartCoroutine(RegionManager.Instance.InitializeDefaultRegion());

                CharacterEventsManager.Instance.SceneEntered(scene.name);
                BonusManager.Instance.ResetAllOnBonuses();
                BonusManager.Instance.InitBonuses();

                MinimapDisplayManager.Instance.InitializeMinimap(RPGBuilderUtilities.GetGameSceneFromName(scene.name));
                Toolbar.Instance.InitToolbar();
                
                CombatManager.Instance.ResetPlayerTarget();
                
                
                foreach (var ab in CharacterData.Instance.abilitiesData)
                {
                    ab.comboActive = false;
                }

                bool isNewCharacter = !CharacterData.Instance.created;
                if (!CharacterData.Instance.created)
                {
                    InitializeNewCharacter();
                }
                else
                {
                    InventoryManager.Instance.InitEquippedItems();
                    LevelingManager.Instance.HandleSpellbookAfterLevelUp();
                }
                
                StatCalculator.SetVitalityToMax();
                CombatManager.playerCombatNode.appearanceREF.HandleAnimatorOverride();
                InitActionAbilities();
                ShapeshiftingSlotsDisplayManager.Instance.DisplaySlots();
                CombatManager.playerCombatNode.playerControllerEssentials.StartCoroutine(CombatManager.playerCombatNode.playerControllerEssentials.InitControllers());
                if(isNewCharacter) RPGBDemoTutorialDisplayManager.Instance.StartCoroutine(RPGBDemoTutorialDisplayManager.Instance.InitTutorial());
                
                isInGame = true;
            } else
            {
                isInGame = false;
            }
        }


        private void InitActionAbilities()
        {
            CharacterData.Instance.currentActionAbilities.Clear();

            foreach (var actionAb in RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID).actionAbilities)
            {
                AddCurrentActionAb(actionAb, CharacterData.ActionAbilityType.fromRace, CharacterData.Instance.raceID);
            }

            foreach (var skill in CharacterData.Instance.skillsDATA)
            {
                foreach (var actionAb in RPGBuilderUtilities.GetSkillFromID(skill.skillID).actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterData.ActionAbilityType.fromSkill, skill.skillID);
                }
            }

            if (combatSettings.useClasses)
            {
                foreach (var actionAb in RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID)
                    .actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterData.ActionAbilityType.fromClass,
                        CharacterData.Instance.classDATA.classID);
                }
            }

            foreach (var equippedItem in CharacterData.Instance.armorsEquipped)
            {
                if(equippedItem.itemID == -1) continue;
                foreach (var actionAb in RPGBuilderUtilities.GetItemFromID(equippedItem.itemID).actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterData.ActionAbilityType.fromItem, equippedItem.itemID);
                }
            }

            foreach (var equippedItem in CharacterData.Instance.weaponsEquipped)
            {
                if(equippedItem.itemID == -1) continue;
                foreach (var actionAb in RPGBuilderUtilities.GetItemFromID(equippedItem.itemID).actionAbilities)
                {
                    AddCurrentActionAb(actionAb, CharacterData.ActionAbilityType.fromItem, equippedItem.itemID);
                }
            }
        }

        public void AddCurrentActionAb(RPGCombatDATA.ActionAbilityDATA actionAb, CharacterData.ActionAbilityType type, int sourceID)
        {
            CharacterData.ActionAbility newActionAb = new CharacterData.ActionAbility();
            newActionAb.type = type;
            newActionAb.sourceID = sourceID;
            newActionAb.ability = RPGBuilderUtilities.GetAbilityFromID(actionAb.abilityID);
            if (actionAb.keyType == RPGCombatDATA.ActionAbilityKeyType.ActionKey)
            {
                newActionAb.keyType = actionAb.keyType;
                newActionAb.actionKeyName = actionAb.actionKeyName;
            }
            else
            {
                newActionAb.keyType = actionAb.keyType;
                newActionAb.key = actionAb.key;
            }
            newActionAb.CDLeft = 0;
            newActionAb.NextTimeUse = 0;
            CharacterData.Instance.currentActionAbilities.Add(newActionAb);
        }

        private void InitializeNewCharacter()
        {
            CharacterData.Instance.created = true;

            RPGRace raceREF = RPGBuilderUtilities.GetRaceFromID(CharacterData.Instance.raceID);
            foreach (var t in raceREF.startItems)
            {
                RPGBuilderUtilities.HandleItemLooting(t.itemID, t.count, t.equipped, false);
            }

            if (combatSettings.useClasses)
            {
                RPGClass classREF = RPGBuilderUtilities.GetClassFromID(CharacterData.Instance.classDATA.classID);
                foreach (var t in classREF.startItems)
                {
                    RPGBuilderUtilities.HandleItemLooting(t.itemID, t.count, t.equipped, false);
                }
            }

            foreach (var t1 in CharacterData.Instance.skillsDATA)
            {
                RPGSkill skillREF = RPGBuilderUtilities.GetSkillFromID(t1.skillID);
                foreach (var t in skillREF.startItems)
                {
                    RPGBuilderUtilities.HandleItemLooting(t.itemID, t.count, t.equipped, false);
                }
            }
            
            foreach (var t1 in CharacterData.Instance.weaponTemplates)
            {
                RPGWeaponTemplate weaponTemplateREF = RPGBuilderUtilities.GetWeaponTemplateFromID(t1.weaponTemplateID);
                foreach (var t in weaponTemplateREF.startItems)
                {
                    RPGBuilderUtilities.HandleItemLooting(t.itemID, t.count, t.equipped, false);
                }
            }

            List<RPGAbility> knownAbilities = new List<RPGAbility>();
            List<int> slots = new List<int>();
            int curAb = 0;

            LevelingManager.Instance.HandleSpellbookAfterLevelUp();
            CraftingManager.Instance.HandleStartingRecipes();
            GatheringManager.Instance.HandleStartingResourceNodes();

            foreach (CharacterData.Ability_DATA t in CharacterData.Instance.abilitiesData)
            {
                RPGAbility abREF = RPGBuilderUtilities.GetAbilityFromID(t.ID);
                if (!t.known) continue;
                knownAbilities.Add(abREF);
                slots.Add(curAb);
                curAb++;
            }

            for (int i = 0; i < knownAbilities.Count; i++)
            {
                ActionBarManager.Instance.SetAbilityToSlot(knownAbilities[i], slots[i]);
            }
        }

        private void Update()
        {
            if (generalSettings == null || CombatManager.playerCombatNode == null) return;
            if (!generalSettings.automaticSave) return;
            if (!(Time.time >= nextAutomaticSave)) return;
            nextAutomaticSave += generalSettings.automaticSaveDelay;
            RPGBuilderJsonSaver.SaveCharacterData(CharacterData.Instance.CharacterName, CharacterData.Instance);
        }

        private void OnApplicationQuit()
        {
            if (generalSettings == null || !generalSettings.automaticSaveOnQuit || CombatManager.playerCombatNode == null) return;
            ClearAllWorldItemData();
            if (CombatManager.playerCombatNode.appearanceREF.isShapeshifted)
            {
                CombatManager.Instance.ResetPlayerShapeshift();
            }
            RPGBuilderJsonSaver.SaveCharacterData(CharacterData.Instance.CharacterName, CharacterData.Instance);
        }

        public void ClearAllWorldItemData()
        {
            List<CharacterData.ItemDATA> worldItems = new List<CharacterData.ItemDATA>();
            foreach (var itemData in CharacterData.Instance.itemsDATA)
            {
                if (itemData.state == CharacterData.ItemDataState.world)
                    worldItems.Add(itemData);
            }

            foreach (var worldItem in worldItems)
            {
                if (CharacterData.Instance.itemsDATA.Contains(worldItem))
                    CharacterData.Instance.itemsDATA.Remove(worldItem);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

    }
}
