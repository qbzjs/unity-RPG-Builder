using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGGameModifier : ScriptableObject
{
    public int ID = -1;
    public Sprite icon;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;

    public enum GameModifierUnlockType
    {
        MainMenu,
        World,
        Both
    }

    public GameModifierUnlockType unlockType;
    
    public enum CategoryType
    {
        Combat,
        General,
        World,
        Settings
    }

    public enum CombatModuleType
    {
        Ability,
        Effect,
        NPC,
        Stat,
        TreePoint,
        Spellbook,
        Faction,
        WeaponTemplate
    }

    public enum GeneralModuleType
    {
        Item,
        Skill,
        LevelTemplate,
        Race,
        Class,
        TalentTree,
        LootTable,
        MerchantTable,
        Currency,
        CraftingRecipe,
        CraftingStation,
        Bonus,
        GearSet,
        Enchantment,
    }

    public enum WorldModuleType
    {
        Task,
        Quest,
        WorldPosition,
        ResourceNode,
        GameScene,
        Dialogue
    }

    public enum SettingsModuleType
    {
        CombatSettings,
        GeneralSettings,
        SceneSettings
    }

    public enum GameModifierType
    {
        Positive,
        Negative
    }

    public GameModifierType gameModifierType;

    public int cost;
    public int gain;


    [System.Serializable]
    public class ModuleAmountModifier
    {
        public int entryID = -1;
        public string entryName;
        public float alterAmount;
        public bool isPercent;
        public DataModifierType dataModifierType;
    }

    [System.Serializable]
    public class StatDataModifier
    {
        [StatID] public int statID = -1;
        public DataModifierType dataModifierType;
        public UnitType unitType;
        public bool checkMin, checkMax;
        public float valueMin, valueMax;
        public float valueDefault;
        public bool restShifting, CombatShifting;
        public float restShiftAmount, restShiftInterval, combatShiftAmount, combatShiftInterval;
    }


    public enum UnitType
    {
        Player,
        NPC,
        All
    }
    
    public enum DataModifierType
    {
        Add,
        Override
    }


    public enum AbilityModifierType
    {
        Unlock_Cost,
        No_Use_Requirement,
        No_Effect_Requirement
    }

    public enum PointModifierType
    {
        Start_At,
        Max,
        Gain_Value
    }

    public enum FactionModifierType
    {
        Stance_Point_Required,
        Interaction_Start_Point
    }

    public enum SpellbookModifierType
    {
        Ability_Level_Required,
        Bonus_Level_Required
    }

    public enum StatModifierType
    {
        Settings,
        MinOverride,
        MaxOverride
    }

    public enum NPCModifierType
    {
        Aggro_Range,
        Exp,
        Level,
        Reset_Target_Distance,
        Respawn_Time,
        Faction_Reward,
        Loot_Table_Chance,
        Roam_Range,
        Faction
    }

    public enum WeaponTemplateModifierType
    {
        Exp_Mod,
        No_Starting_Items,
        Stat_Amount,
    }

    public enum ItemModifierType
    {
        Gem_Bonus_Amount,
        Attack_Speed,
        Min_Damage,
        Max_Damage,
        Overriden_Auto_Attack,
        Stat_Amount,
        Max_Random_Stat_Amount,
        Random_Stats_Chance,
        Random_Stat_Min,
        Random_Stat_Max,
        Sell_Price,
        Stack_Amount,
        No_Requirement,
    }

    public enum SkillModifierType
    {
        Alloc_Points,
        No_Starting_Items,
        Stat_Amount,
    }

    public enum LevelTemplateModifierType
    {
        MaxEXPToLevel,
    }

    public enum RaceModifierType
    {
        Male_Prefab,
        Female_Prefab,
        Start_Scene,
        Start_Position,
        Faction,
        Stat_Amount,
        No_Starting_Items,
        Alloc_Points
    }

    public enum ClassModifierType
    {
        Alloc_Points,
        Alloc_Points_Menu,
        No_Starting_Items,
        Stat_Amount
    }

    public enum MerchantTableModifierType
    {
        Cost,
        Currency,
    }

    public enum CurrencyModifierType
    {
        Min,
        Max,
        Start_At,
        Amount_For_Convertion
    }

    public enum RecipeModifierType
    {
        Unlock_Cost,
        EXP,
        Crafted_Chance,
        Crafted_Count,
        Component_Required_Count
    }
    public enum BonusModifierType
    {
        Unlock_Cost,
        No_Requirement,
    }
    public enum GearSetModifierType
    {
        Equipped_Amount,
        Stat_Bonuses,
    }
    public enum EnchantmentModifierType
    {
        No_Requirement,
        Time,
        Price,
    }
    public enum QuestModifierType
    {
        No_Requirement,
    }

    public enum ResourceNodeModifierType
    {
        Unlock_Cost,
        EXP,
        Gather_Time,
        Respawn_Time,
        Level_Required
    }

    public enum GeneralSettingModifierType
    {
        No_Auto_Save,
    }

    public enum CombatSettingModifierType
    {
        Health_Stat,
        Alloc_Tree_Point,
        Can_Decrease_Alloc_Point,
        Critical_Bonus,
        Combat_Reset_Timer,
        Action_Bar_Slots
    }

    public enum WorldSettingModifierType
    {
        Light_Intensity,
        Camera_FOV,
        Game_Audio
    }

    [System.Serializable]
    public class GameModifierDATA
    {
        public bool showModifier;
        public CategoryType categoryType;
        public CombatModuleType combatModuleType;
        public GeneralModuleType generalModuleType;
        public WorldModuleType worldModuleType;
        public SettingsModuleType settingsModuleType;
        public string modifierTypeName;

        public bool showEntryList;
        public ModuleAmountModifier amountModifier = new ModuleAmountModifier();
        public List<ModuleAmountModifier> amountModifierList = new List<ModuleAmountModifier>();
        public List<int> entryIDs = new List<int>();
        public bool boolValue;
        public bool isGlobal = true;
        public int intValue;
        public float floatValue;
        public GameObject gameObjectValue;
        
        // ABILITY
        public AbilityModifierType abilityModifierType;
        
        // POINTS
        public PointModifierType treePointModifierType;
        
        // FACTIONS
        public FactionModifierType factionModifierType;
        
        // SPELLBOOK
        public SpellbookModifierType spellbookModifierType;
        
        // STATS
        public StatModifierType statModifierType;
        [RPGDataList] public List<StatDataModifier> statModifierData = new List<StatDataModifier>();
        
        // NPC
        public NPCModifierType npcModifierType;
        
        // WEAPON TEMPLATES
        public WeaponTemplateModifierType weaponTemplateModifierType;
        
        // ITEMS
        public ItemModifierType itemModifierType;
        
        // SKILLS
        public SkillModifierType skillModifierType;
        
        // LEVEL TEMPLATES
        public LevelTemplateModifierType levelTemplateModifierType;
        
        // RACES
        public RaceModifierType raceModifierType;
        public GameObject raceOverridenMalePrefab;
        public GameObject raceOverridenFemalePrefab;
        
        // CLASSES
        public ClassModifierType classModifierType;
        
        // MERCHANT TABLES
        public MerchantTableModifierType merchantTableModifierType;
        
        // CURRENCIES
        public CurrencyModifierType currencyModifierType;
        
        // RECIPE
        public RecipeModifierType recipeModifierType;
        
        // BONUS
        public BonusModifierType bonusModifierType;
        
        // GEAR SETS
        public GearSetModifierType gearSetModifierType;
        
        // ENCHANTMENT
        public EnchantmentModifierType enchantmentModifierType;
        
        // QUEST
        public QuestModifierType questModifierType;
        
        // RESOURCE NODE
        public ResourceNodeModifierType resourceNodeModifierType;
        
        // GENERAL SETTINGS
        public GeneralSettingModifierType generalSettingModifierType;
        
        // COMBAT SETTINGS
        public CombatSettingModifierType combatSettingModifierType;
        
        // WORLD SETTINGS
        public WorldSettingModifierType worldSettingModifierType;
    }

    [RPGDataList] public List<GameModifierDATA> gameModifiersList = new List<GameModifierDATA>();
    
    
    public void updateThis(RPGGameModifier newData)
    {
        ID = newData.ID;
        _name = newData._name;
        displayName = newData.displayName;
        _fileName = newData._fileName;
        description = newData.description;
        icon = newData.icon;
        gameModifierType = newData.gameModifierType;
        
        cost = newData.cost;
        gain = newData.gain;
        
        gameModifiersList = newData.gameModifiersList;
    }
}
