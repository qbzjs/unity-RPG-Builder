using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGBuilderEditorFilters : ScriptableObject
{
    [System.Serializable]
    public class CategoryFieldData {
        public string fieldBaseName;
        public Type thisType;
        public List<string> fieldNames = new List<string>();
        public bool isStringEnum;
        public bool isIDReference;
        public Attribute customAttribute;
        public bool display = true;
    }
    
    
    [System.Serializable]
    public class ModuleCategory {
        public string categoryName;
        public string categoryDisplayName;
        public string parentType;
        public bool showInEditor = false;
        public bool display = true;
        public Type thisType;
            
        public List<CategoryFieldData> fields = new List<CategoryFieldData>();
    }
    
    [System.Serializable]
    public class ModuleContentData {
        public string moduleName;
        public string moduleDisplayName;
        public Type thisType;
        
        public List<ModuleCategory> categories = new List<ModuleCategory>();
    }
    public List<ModuleContentData> modules = new List<ModuleContentData>();
    
    
    public class EntryField {
        public string fieldName;
        public List<string> parentFieldNames = new List<string>();
        public string mopduleName;
        public string categoryName;
        public Type fieldType;
        public bool display = true;
    }

    public class EntryFieldList {
        public string mopduleName;
        public string categoryName;
        public List<EntryField> fieldList = new List<EntryField>();
        public bool display = true;
    }

    
    [System.Serializable]
    public class FilterEntryData
    {
        public string fieldName;
        public List<string> parentFieldNames = new List<string>();
        public string mopduleName;
        public string categoryName;
        public Type fieldType;
        public string fieldTypeString;
        public float floatValue;
        public int intValue;
        public bool boolValue;
        public string text;
        public Sprite sprite;
        public GameObject gameObject;
        public Material material;
        public Vector3 vector3;
        public NumberConditionType numberConditionType;
        public StringValueType stringValueType;
        public EntryReferenceConditionType entryReferenceConditionType;
        
        public RPGAbility Ability;
        public RPGEffect Effect;
        public RPGNpc NPC;
        public RPGStat Stat;
        public RPGTreePoint TreePoint;
        public RPGSpellbook Spellbook;
        public RPGFaction Faction;
        public RPGWeaponTemplate WeaponTemplate;

        public RPGItem Item;
        public RPGSkill Skill;
        public RPGLevelsTemplate LevelTemplate;
        public RPGRace Race;
        public RPGClass Class;
        public RPGLootTable LootTable;
        public RPGMerchantTable MerchantTable;
        public RPGCurrency Currency;
        public RPGCraftingRecipe CraftingRecipe;
        public RPGCraftingStation CraftingStation;
        public RPGTalentTree TalentTree;
        public RPGBonus Bonus;
        public RPGGearSet GearSet;
        public RPGEnchantment Enchantment;

        public RPGTask Task;
        public RPGQuest Quest;
        public RPGWorldPosition WorldPosition;
        public RPGResourceNode ResourceNode;
        public RPGGameScene GameScene;
        public RPGDialogue Dialogue;

        public bool isRPGDataReference;

        public Enum enumReference;
        public int enumIndex;
    }
    
    public List<FilterEntryData> abilityFilters = new List<FilterEntryData>();
    public List<FilterEntryData> effectFilters = new List<FilterEntryData>();
    public List<FilterEntryData> NPCFilters = new List<FilterEntryData>();
    public List<FilterEntryData> statFilters = new List<FilterEntryData>();
    public List<FilterEntryData> talentPointFilters = new List<FilterEntryData>();
    public List<FilterEntryData> spellbookFilters = new List<FilterEntryData>();
    public List<FilterEntryData> factionFilters = new List<FilterEntryData>();
    public List<FilterEntryData> weaponTemplateFilters = new List<FilterEntryData>();
    public List<FilterEntryData> itemFilters = new List<FilterEntryData>();
    public List<FilterEntryData> skillFilters = new List<FilterEntryData>();
    public List<FilterEntryData> levelTemplateFilters = new List<FilterEntryData>();
    public List<FilterEntryData> raceFilters = new List<FilterEntryData>();
    public List<FilterEntryData> classFilters = new List<FilterEntryData>();
    public List<FilterEntryData> lootTablesFilters = new List<FilterEntryData>();
    public List<FilterEntryData> merchantTableFilters = new List<FilterEntryData>();
    public List<FilterEntryData> currencyFilters = new List<FilterEntryData>();
    public List<FilterEntryData> recipeFilters = new List<FilterEntryData>();
    public List<FilterEntryData> craftingStationFilters = new List<FilterEntryData>();
    public List<FilterEntryData> talentTreeFilters = new List<FilterEntryData>();
    public List<FilterEntryData> bonusFilters = new List<FilterEntryData>();
    public List<FilterEntryData> gearsSetFilters = new List<FilterEntryData>();
    public List<FilterEntryData> enchantmentFilters = new List<FilterEntryData>();
    public List<FilterEntryData> taskFilters = new List<FilterEntryData>();
    public List<FilterEntryData> questFilters = new List<FilterEntryData>();
    public List<FilterEntryData> worldPositionFilters = new List<FilterEntryData>();
    public List<FilterEntryData> resourceNodeFilters = new List<FilterEntryData>();
    public List<FilterEntryData> gameSceneFilters = new List<FilterEntryData>();
    public List<FilterEntryData> dialogueFilters = new List<FilterEntryData>();
    public List<FilterEntryData> gameModifierFilters = new List<FilterEntryData>();
    public List<FilterEntryData> speciesFilters = new List<FilterEntryData>();
    public List<FilterEntryData> comboFilters = new List<FilterEntryData>();
    
    public void ClearALLFilters()
    {
        abilityFilters.Clear();
        effectFilters.Clear();
        NPCFilters.Clear();
        
    }
    
    public enum NumberConditionType
    {
        Equal,
        EqualOrBelow,
        EqualOrAbove,
        Below,
        Above
    }
    
    public enum StringValueType
    {
        Contains,
        DoNotContain,
        Equal,
    }
    
    public enum EntryReferenceConditionType
    {
        Equal,
        NotEqual,
    }
    
    [Serializable]
    public class AbilityModuleSection
    {
        public bool showBaseInfo;
        public bool showRanks;
        public bool showUnlockSettings;
        public bool showUseRequirements;
        public bool showEffectsRequired;
        public bool showActivation;
        public bool showAbilityType;
        public bool showHitSettings;
        public bool showCooldowns;
        public bool showEffectsApplied;
        public bool showCasterEffectsApplied;
        public bool showConditionalEffect;
        public bool showTags;
        public bool showVisualEffects;
        public bool showVisualAnimations;
    }

    public AbilityModuleSection abilityModuleSection = new AbilityModuleSection();


    [Serializable]
    public class EffectModuleSection
    {
        public bool showBaseInfo;
        public bool showType;
        public bool showRanks;
        public bool showStateSettings;
        public bool showVisualEffects;
        public bool showVisualAnimations;
    }

    public EffectModuleSection effectModuleSection = new EffectModuleSection();

    [Serializable]
    public class NPCModuleSection
    {
        public bool showBaseInfo;
        public bool showVisual;
        public bool showFunctions;
        public bool showCombat;
        public bool showRespawn;
        public bool showRewards;
        public bool showLootTables;
        public bool showStats;
        public bool showMovement;
        public bool showMerchant;
        public bool showQuestGiven;
        public bool showQuestCompleted;
        public bool showDialogue;
        public bool showAggroLinks;
    }

    public NPCModuleSection npcModuleSection = new NPCModuleSection();
    
    [Serializable]
    public class StatModuleSection
    {
        public bool showBaseInfo;
        public bool showSetup;
        public bool showBonuses;
        public bool showVitalityActions;
    }

    public StatModuleSection statModuleSection = new StatModuleSection();
    
    [Serializable]
    public class TreePointModuleSection
    {
        public bool showBaseInfo;
        public bool showSetup;
        public bool showGainRequirements;
    }

    public TreePointModuleSection treePointModuleSection = new TreePointModuleSection();
    
    [Serializable]
    public class SpellbookModuleSection
    {
        public bool showBaseInfo;
        public bool showNodes;
    }

    public SpellbookModuleSection spellbookModuleSection = new SpellbookModuleSection();
    
    [Serializable]
    public class FactionModuleSection
    {
        public bool showBaseInfo;
        public bool showStances;
        public bool showInteractions;
    }

    public FactionModuleSection factionModuleSection = new FactionModuleSection();
    
    [Serializable]
    public class WeaponTemplateModuleSection
    {
        public bool showBaseInfo;
        public bool showProgression;
        public bool showWeaponList;
        public bool showStats;
        public bool showTalentTrees;
        public bool showSpellbook;
        public bool showStartingItems;
        public bool showStatAllocationGame;
    }

    public WeaponTemplateModuleSection weaponTemplateModuleSection = new WeaponTemplateModuleSection();
    
    [Serializable]
    public class SpeciesModuleSection
    {
        public bool showBaseInfo;
        public bool showStats;
        public bool showTraits;
    }

    public SpeciesModuleSection speciesModuleSection = new SpeciesModuleSection();
    
    [Serializable]
    public class ComboModuleSection
    {
        public bool showBaseInfo;
        public bool showCombos;
    }

    public ComboModuleSection comboModuleSection = new ComboModuleSection();
    
    [Serializable]
    public class ItemModuleSection
    {
        public bool showBaseInfo;
        public bool showTypes;
        public bool showLootSettings;
        public bool showActionAbilities;
        public bool showSocket;
        public bool showVisuals;
        public bool showCombat;
        public bool showStats;
        public bool showRandomStats;
        public bool showRequirements;
        public bool showOnUseActions;
        public bool showGeneral;
        public bool showEnchantment;
        public bool showGem;
        public bool showWeaponPositions;
    }

    public ItemModuleSection itemModuleSection = new ItemModuleSection();
    
    [Serializable]
    public class SkillModuleSection
    {
        public bool showBaseInfo;
        public bool showStats;
        public bool showTalentTrees;
        public bool showActionAbilities;
        public bool showStartingItems;
        public bool showStatAllocationGame;
    }

    public SkillModuleSection skillModuleSection = new SkillModuleSection();
    
    [Serializable]
    public class LevelTemplateModuleSection
    {
        public bool showBaseInfo;
        public bool showTemplate;
    }

    public LevelTemplateModuleSection levelTemplateModuleSection = new LevelTemplateModuleSection();
    
    [Serializable]
    public class RaceModuleSection
    {
        public bool showBaseInfo;
        public bool showVisual;
        public bool showStartingSettings;
        public bool showClasses;
        public bool showWeaponTemplate;
        public bool showStats;
        public bool showActionAbilities;
        public bool showStartingItems;
        public bool showStatAllocation;
    }

    public RaceModuleSection raceModuleSection = new RaceModuleSection();
    
    [Serializable]
    public class ClassModuleSection
    {
        public bool showBaseInfo;
        public bool showSpellbook;
        public bool showTalentTree;
        public bool showStats;
        public bool showActionAbilities;
        public bool showStartingItems;
        public bool showStatAllocation;
        public bool showStatAllocationGame;
    }

    public ClassModuleSection classModuleSection = new ClassModuleSection();
    
    [Serializable]
    public class LootTableModuleSection
    {
        public bool showBaseInfo;
        public bool showItems;
    }

    public LootTableModuleSection lootTableModuleSection = new LootTableModuleSection();
    
    [Serializable]
    public class MerchantTableModuleSection
    {
        public bool showBaseInfo;
        public bool showItems;
    }

    public MerchantTableModuleSection merchantTableModuleSection = new MerchantTableModuleSection();
    
    [Serializable]
    public class CurrencyModuleSection
    {
        public bool showBaseInfo;
        public bool showSetupSettings;
        public bool showConversion;
        public bool showSuperiorCurrencies;
    }

    public CurrencyModuleSection currencyModuleSection = new CurrencyModuleSection();
    
    [Serializable]
    public class CraftingRecipeModuleSection
    {
        public bool showBaseInfo;
        public bool showRanks;
        public bool showTalentTreeSettings;
        public bool showSettings;
        public bool showCraftedItems;
        public bool showComponentsRequired;
    }

    public CraftingRecipeModuleSection craftingRecipeModuleSection = new CraftingRecipeModuleSection();
    
    [Serializable]
    public class CraftingStationModuleSection
    {
        public bool showBaseInfo;
        public bool showSkills;
    }

    public CraftingStationModuleSection craftingStationModuleSection = new CraftingStationModuleSection();
    
    [Serializable]
    public class TalentTreeModuleSection
    {
        public bool showBaseInfo;
        public bool showNodes;
    }

    public TalentTreeModuleSection talentTreeModuleSection = new TalentTreeModuleSection();
    
    [Serializable]
    public class BonusModuleSection
    {
        public bool showBaseInfo;
        public bool showRanks;
        public bool showTalentTreeSettings;
        public bool showRequirements;
        public bool showBonuses;
    }

    public BonusModuleSection bonusModuleSection = new BonusModuleSection();
    
    [Serializable]
    public class GearSetModuleSection
    {
        public bool showBaseInfo;
        public bool showItems;
        public bool showTiers;
    }

    public GearSetModuleSection gearSetModuleSection = new GearSetModuleSection();
    
    [Serializable]
    public class EnchantmentModuleSection
    {
        public bool showBaseInfo;
        public bool showRequirements;
        public bool showTiers;
    }

    public EnchantmentModuleSection enchantmentModuleSection = new EnchantmentModuleSection();
    
    [Serializable]
    public class QuestModuleSection
    {
        public bool showBaseInfo;
        public bool showUIData;
        public bool showQuestSettings;
        public bool showItemsGiven;
        public bool showObjectives;
        public bool showAutomaticRewards;
        public bool showRewardsToPick;
        public bool showRequirements;
    }

    public QuestModuleSection questModuleSection = new QuestModuleSection();
    
    [Serializable]
    public class TaskModuleSection
    {
        public bool showBaseInfo;
        public bool showTaskData;
    }

    public TaskModuleSection taskModuleSection = new TaskModuleSection();
    
    [Serializable]
    public class WorldPositionModuleSection
    {
        public bool showBaseInfo;
        public bool showSettings;
    }

    public WorldPositionModuleSection worldPositionModuleSection = new WorldPositionModuleSection();
    
    [Serializable]
    public class ResourceNodeModuleSection
    {
        public bool showBaseInfo;
        public bool showRanks;
        public bool showTalentTreeSettings;
        public bool showLootSettings;
        public bool showSkillSettings;
    }

    public ResourceNodeModuleSection resourceNodeModuleSection = new ResourceNodeModuleSection();
    
    [Serializable]
    public class GameSceneModuleSection
    {
        public bool showBaseInfo;
        public bool showSettings;
        public bool showMinimapSettings;
        public bool showLoadingScreenSettings;
        public bool showRegions;
    }

    public GameSceneModuleSection gameSceneModuleSection = new GameSceneModuleSection();
    
    [Serializable]
    public class DialogueModuleSection
    {
        public bool showBaseInfo;
        public bool showSettings;
        public bool showGraphSettings;
    }

    public DialogueModuleSection dialogueModuleSection = new DialogueModuleSection();
    
    [Serializable]
    public class GameModifierModuleSection
    {
        public bool showBaseInfo;
        public bool showSettings;
        public bool showModifiers;
    }

    public GameModifierModuleSection gameModifierModuleSection = new GameModifierModuleSection();
    
    [Serializable]
    public class CombatSettingsModuleSection
    {
        public bool showCombatRules;
        public bool showActionBar;
        public bool showTalentTrees;
        public bool showFactionSettings;
        public bool showStatSettings;
        public bool showNodeSockets;
        public bool showStatFunctions;
        public bool showStatCategory;
        public bool showNPCs;
        public bool showAbilities;
        public bool showSprint;
        public bool showEffects;
    }

    public CombatSettingsModuleSection combatSettingsModuleSection = new CombatSettingsModuleSection();
    
    [Serializable]
    public class GeneralSettingsModuleSection
    {
        public bool showSavingSettings;
        public bool showLoadingScreenSettings;
        public bool showMainMenuSettings;
        public bool showDeveloperSettings;
        public bool showControllerSettings;
        public bool showDialogueSettings;
        public bool showGameModifierSettings;
        public bool showActionKeys;
        public bool showActionKeyCategoryList;
        public bool showActionKeyList;
        public bool showLayers;
    }

    public GeneralSettingsModuleSection generalSettingsModuleSection = new GeneralSettingsModuleSection();
    
    [Serializable]
    public class ItemsSettingsModuleSection
    {
        public bool showInventorySettings;
        public bool showItemSettings;
        public bool showWeaponSettings;
        public bool showArmorSettings;
        public bool showSocketSettings;
        public bool showItemType;
        public bool showItemRarity;
        public bool showItemColor;
        public bool showRarityImage;
        public bool showWeaponType;
        public bool showWeaponAnimatorOverride;
        public bool showWeaponSlots;
        public bool showSlotType;
        public bool showArmorType;
        public bool showArmorSlot;
    }

    public ItemsSettingsModuleSection itemSettingsModuleSection = new ItemsSettingsModuleSection();
    
    [Serializable]
    public class EditorSettingsModuleSection
    {
        public bool showThemeSettings;
        public bool showEditorUtilities;
        public bool showDatabase;
        public bool showsceneLoaderSettings;
        public bool showDatabaseExport;
        public bool showDatabaseImport;
        public bool showEditorInfo;
        public bool showProjectUpgrade;
    }

    public EditorSettingsModuleSection editorSettingsModuleSection = new EditorSettingsModuleSection();
    
    
}
