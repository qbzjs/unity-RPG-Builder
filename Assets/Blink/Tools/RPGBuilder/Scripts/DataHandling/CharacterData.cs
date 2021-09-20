using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    private static CharacterData instance;

    public bool created;
    public int raceID = -1;
    public string CharacterName;
    public RPGRace.RACE_GENDER gender;
    public Vector3 position;
    public Vector3 rotation;
    public int currentGameSceneID = -1;

    public int mainMenuStatAllocationPoints = 0;
    public int mainMenuStatAllocationMaxPoints = 0;
    
    [System.Serializable]
    public class AllocatedStatEntry
    {
        public RPGStat statREF;
        public int statID = -1;
        public int maxValue = -1;
        public int cost = 1;
        public int valueAdded = 1;
    }
    
    [System.Serializable]
    public class AllocatedStatData
    {
        public string statName;
        public int statID = -1;
        public int value;
        public int maxValue;
        public int valueGame;
        public int maxValueGame;
    }
    public List<AllocatedStatData> allocatedStatsData;
    
    [System.Serializable]
    public class ActionKeyDATA
    {
        public string actionKeyName;
        public KeyCode currentKey;
    }

    public List<ActionKeyDATA> actionKeys;

    [System.Serializable]
    public class ClassDATA
    {
        public int classID = -1;
        public int currentClassLevel;
        public int currentClassXP, maxClassXP;

    }

    public ClassDATA classDATA;
    
    
    [System.Serializable]
    public class WeaponTemplatesData
    {
        public int weaponTemplateID;
        public int currentWeaponLevel;
        public int currentWeaponXP, maxWeaponXP;

    }

    public List<WeaponTemplatesData> weaponTemplates;

    [System.Serializable]
    public class TalentTrees_DATA
    {
        public int treeID;
        public int pointsSpent;

        [System.Serializable]
        public class TalentTreeNode_DATA
        {
            public RPGTalentTree.Node_DATA nodeData;
        }

        public List<TalentTreeNode_DATA> nodes = new List<TalentTreeNode_DATA>();

    }

    public List<TalentTrees_DATA> talentTrees;

    [System.Serializable]
    public class Ability_DATA
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
        public float NextTimeUse, CDLeft;
        public bool comboActive;
    }
    public List<Ability_DATA> abilitiesData;
    
    
    [System.Serializable]
    public class Recipe_DATA
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
    }
    public List<Recipe_DATA> recipesData;
    
    [System.Serializable]
    public class ResourceNode_DATA
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
    }
    public List<ResourceNode_DATA> resourceNodeData;

    [System.Serializable]
    public class Bonus_DATA
    {
        public string name;
        public int ID;
        public bool known;
        public int rank;
        public bool On;
    }
    public List<Bonus_DATA> bonusesData;
    
    [System.Serializable]
    public class GameModifier_DATA
    {
        public string name;
        public int ID;
        public bool On;
    }
    public List<GameModifier_DATA> gameModifiersData;
    public int menuGameModifierPoints;
    public int worldGameModifierPoints;
    
    [System.Serializable]
    public class Faction_DATA
    {
        public string name;
        public int ID;
        public string currentStance;
        public int currentPoint;
    }
    public List<Faction_DATA> factionsData;

    public enum ActionBarSlotContentType
    {
        None,
        Ability,
        Item
    }
    public enum ActionBarType
    {
        Main,
        Extra
    }
    [System.Serializable]
    public class ActionBarSlotDATA
    {
        public ActionBarSlotContentType contentType;
        public ActionBarType slotType;
        public int ID = -1;
    }

    public List<ActionBarSlotDATA> actionBarSlotsDATA;
    public List<ActionBarSlotDATA> stealthedActionBarSlotsDATA;
    

    [System.Serializable]
    public class SkillsDATA
    {
        public int skillID;
        public int currentSkillLevel;
        public int currentSkillXP, maxSkillXP;
    }

    public List<SkillsDATA> skillsDATA;

    [System.Serializable]
    public class InventoryBagDATA
    {
        public int bagItemID;
        public List<InventorySlotDATA> slots = new List<InventorySlotDATA>();

    }
    [System.Serializable]
    public class InventorySlotDATA
    {
        public int itemID;
        public int itemStack;
        public int itemDataID = -1;
    }
    
    [System.Serializable]
    public class InventoryDATA
    {
        public List<InventorySlotDATA> baseSlots = new List<InventorySlotDATA>();
        public List<InventoryBagDATA> bags = new List<InventoryBagDATA>();
    }

    public InventoryDATA inventoryData;

    [System.Serializable]
    public class ArmorsEquippedDATA
    {
        public int itemID = -1;
        public int itemDataID = -1;
    }
    public List<ArmorsEquippedDATA> armorsEquipped;
    
    [System.Serializable]
    public class WeaponsEquippedDATA
    {
        public int itemID = -1;
        public int itemDataID = -1;
    }
    public List<WeaponsEquippedDATA> weaponsEquipped;

    [System.Serializable]
    public class QuestDATA
    {
        public int questID;
        public QuestManager.questState state;

        [System.Serializable]
        public class Quest_ObjectiveDATA
        {
            public int taskID;
            public QuestManager.questObjectiveState state;
            public int currentProgressValue, maxProgressValue;
        }

        public List<Quest_ObjectiveDATA> objectives = new List<Quest_ObjectiveDATA>();
    }

    public List<QuestDATA> questsData;

    [System.Serializable]
    public class TreePoints_DATA
    {
        public int treePointID;
        public int amount;
    }

    public List<TreePoints_DATA> treePoints;

    [System.Serializable]
    public class Currencies_DATA
    {
        public int currencyID;
        public int amount;
    }

    public List<Currencies_DATA> currencies;

    [System.Serializable]
    public class NPC_KilledDATA
    {
        public int npcID;
        public int killedAmount;
    }

    public List<NPC_KilledDATA> npcKilled;

    [System.Serializable]
    public class SCENE_EnteredDATA
    {
        public string sceneName;
    }

    public List<SCENE_EnteredDATA> scenesEntered;

    [System.Serializable]
    public class REGION_EnteredDATA
    {
        public string regionName;
    }

    public List<REGION_EnteredDATA> regionsEntered;

    [System.Serializable]
    public class ABILITY_LearnedDATA
    {
        public int abilityID;
    }

    public List<ABILITY_LearnedDATA> abilitiesLearned;

    [System.Serializable]
    public class BONUS_LearnedDATA
    {
        public int bonusID;
    }

    public List<BONUS_LearnedDATA> bonusLearned;

    [System.Serializable]
    public class RECIPE_LearnedDATA
    {
        public int recipeID;
    }

    public List<RECIPE_LearnedDATA> recipeslearned;

    [System.Serializable]
    public class RESOURCENODE_LearnedDATA
    {
        public int resourceNodeID;
    }

    public List<RESOURCENODE_LearnedDATA> resourcenodeslearned;

    [System.Serializable]
    public class ITEM_GainedDATA
    {
        public int itemID;
    }

    public List<ITEM_GainedDATA> itemsGained;

    public int nextAvailableItemID = 0;
    [System.Serializable]
    public class ItemDATA
    {
        public string itemName = "";
        public int itemID = -1;
        public int id = 0;
        public int rdmItemID = -1;
        public ItemDataState state;
        
        // Enchanting Data
        public int enchantmentID = -1;
        public int enchantmentTierIndex = -1;
        
        // Socketing Data
        public List<ItemSocketData> sockets = new List<ItemSocketData>();
    }
    
    [System.Serializable]
    public class ItemSocketData
    {
        public string socketType;
        public int gemItemID = -1;
    }
    
    public enum ItemDataState
    {
        world,
        inBag,
        equipped
    }

    public List<ItemDATA> itemsDATA;
    public int nextAvailableRandomItemID = 0;

    [System.Serializable]
    public class RandomizedItems
    {
        public string itemName = "";
        public int itemID = -1;
        public int id = 0;
        public List<RPGItemDATA.RandomizedStat> randomStats = new List<RPGItemDATA.RandomizedStat>();
    }

    public List<RandomizedItems> allRandomizedItems;

    public static CharacterData Instance => instance;

    [System.Serializable]
    public class ActionAbility
    {
        public ActionAbilityType type;
        public int sourceID;
        public RPGAbility ability;
        public RPGCombatDATA.ActionAbilityKeyType keyType;
        public KeyCode key;
        public string actionKeyName;
        public float NextTimeUse, CDLeft;
    }

    public enum ActionAbilityType
    {
        fromRace,
        fromSkill,
        fromClass,
        fromItem
    }

    public List<ActionAbility> currentActionAbilities;
    
    [System.Serializable]
    public class DialoguesData
    {
        public int ID;
        
        [System.Serializable]
        public class DialogueNodeData
        {
            public RPGDialogueTextNode textNode;
            public int currentlyViewedCount;
            public int currentlyClickedCount;
            public bool lineCompleted;
        }
        public List<DialogueNodeData> nodesData = new List<DialogueNodeData>();
        
    }
    public List<DialoguesData> dialoguesDATA;

    private void Start()
    {
        if (instance != null) return;
        instance = this;
        
        currentActionAbilities = new List<ActionAbility>();
        allRandomizedItems = new List<RandomizedItems>();
        actionKeys = new List<ActionKeyDATA>();
        talentTrees = new List<TalentTrees_DATA>();
        actionBarSlotsDATA = new List<ActionBarSlotDATA>();
        stealthedActionBarSlotsDATA = new List<ActionBarSlotDATA>();
        skillsDATA = new List<SkillsDATA>();
        inventoryData = new InventoryDATA();
        armorsEquipped = new List<ArmorsEquippedDATA>();
        weaponsEquipped = new List<WeaponsEquippedDATA>();
        questsData = new List<QuestDATA>();
        treePoints = new List<TreePoints_DATA>();
        currencies = new List<Currencies_DATA>();
        npcKilled = new List<NPC_KilledDATA>();
        scenesEntered = new List<SCENE_EnteredDATA>();
        regionsEntered = new List<REGION_EnteredDATA>();
        abilitiesLearned = new List<ABILITY_LearnedDATA>();
        bonusLearned = new List<BONUS_LearnedDATA>();
        recipeslearned = new List<RECIPE_LearnedDATA>();
        resourcenodeslearned = new List<RESOURCENODE_LearnedDATA>();
        itemsGained = new List<ITEM_GainedDATA>();
        itemsDATA = new List<ItemDATA>();
        abilitiesData = new List<Ability_DATA>();
        recipesData = new List<Recipe_DATA>();
        resourceNodeData = new List<ResourceNode_DATA>();
        bonusesData = new List<Bonus_DATA>();
        factionsData = new List<Faction_DATA>();
        weaponTemplates = new List<WeaponTemplatesData>();
        dialoguesDATA = new List<DialoguesData>();
        allocatedStatsData = new List<AllocatedStatData>();
    }

    public int getCurrencyAmount(RPGCurrency currency)
    {
        foreach (var t in currencies)
            if (t.currencyID == currency.ID)
                return t.amount;

        return -1;
    }

    public int getCurrencyIndex(RPGCurrency currency)
    {
        for (var i = 0; i < currencies.Count; i++)
            if (currencies[i].currencyID == currency.ID)
                return i;
        return -1;
    }


    public Ability_DATA getAbilityData(int ID)
    {
        foreach (var t in abilitiesData)
            if (t.ID == ID)
                return t;

        return null;
    }
    
    public TalentTrees_DATA.TalentTreeNode_DATA getTalentTreeNodeData(int ID)
    {
        foreach (var t in talentTrees)
        foreach (var t1 in t.nodes)
            if (t1.nodeData.abilityID == ID)
                return t1;

        return null;
    }

    public TalentTrees_DATA.TalentTreeNode_DATA getTalentTreeNodeData(RPGCraftingRecipe ab)
    {
        foreach (var t in talentTrees)
        foreach (var t1 in t.nodes)
            if (t1.nodeData.recipeID == ab.ID)
                return t1;

        return null;
    }

    public TalentTrees_DATA.TalentTreeNode_DATA getTalentTreeNodeData(RPGResourceNode ab)
    {
        foreach (var t in talentTrees)
        foreach (var t1 in t.nodes)
            if (t1.nodeData.resourceNodeID == ab.ID)
                return t1;

        return null;
    }

    public TalentTrees_DATA.TalentTreeNode_DATA getTalentTreeNodeData(RPGBonus ab)
    {
        foreach (var t in talentTrees)
        foreach (var t1 in t.nodes)
            if (t1.nodeData.bonusID == ab.ID)
                return t1;

        return null;
    }

    public int getTreePointsAmountByPoint(int ID)
    {
        foreach (var t in treePoints)
            if (t.treePointID == ID)
                return t.amount;

        return -1;
    }

    public QuestDATA getQuestDATA(RPGQuest quest)
    {
        foreach (var t in questsData)
            if (t.questID == quest.ID)
                return t;

        return null;
    }

    public int getQuestINDEX(RPGQuest quest)
    {
        for (var i = 0; i < questsData.Count; i++)
            if (questsData[i].questID == quest.ID)
                return i;
        return -1;
    }

    public bool isAbilityCDReady(RPGAbility ab)
    {
        if (ab.abilityType == RPGAbility.AbilityType.PlayerAutoAttack)
            return CombatManager.playerCombatNode.autoAttackIsReady();
        if (ab.abilityType == RPGAbility.AbilityType.PlayerActionAbility)
            return CombatManager.Instance.actionAbIsReady(ab);
            
        /*foreach (var t in talentTrees)
        foreach (var t1 in t.nodes)
            if (t1.nodeData.nodeType == RPGTalentTree.TalentTreeNodeType.ability && t1.nodeData.abilityID == ab.ID)
                if (t1.NextTimeUse == 0)
                    return true;
        */
        foreach (var t in abilitiesData)
        {
            if (t.ID != ab.ID) continue;
            if (t.NextTimeUse == 0)
                return true;
        }
        
        return false;
    }

    public void InitAbilityCooldown(int ID, float duration)
    {
        RPGAbility ab = RPGBuilderUtilities.GetAbilityFromID(ID);
        switch (ab.abilityType)
        {
            case RPGAbility.AbilityType.PlayerAutoAttack:
                CombatManager.playerCombatNode.InitAACooldown(duration);
                break;
            case RPGAbility.AbilityType.PlayerActionAbility:
                CombatManager.playerCombatNode.InitActionAbilityCooldown(ID, duration);
                break;
            default:
            {
                foreach (var t in abilitiesData)
                {
                    if (t.ID != ID) continue;
                    t.NextTimeUse = duration;
                    t.CDLeft = duration;
                }

                break;
            }
        }
    }

    public class AbilityCDState
    {
        public float NextUse;
        public float CDLeft;
        public bool canUseDuringGCD;
    }
    
    public AbilityCDState getAbilityCDState(RPGAbility ab)
    {
        AbilityCDState cdState = new AbilityCDState();
        foreach (var t in abilitiesData)
        {
            if (t.ID != ab.ID) continue;
            int rank = t.known ? t.rank - 1 : t.rank;
            if (rank == -1) rank = 0;
            cdState.canUseDuringGCD = RPGBuilderUtilities.GetAbilityFromID(t.ID).ranks[rank].CanUseDuringGCD;
            cdState.NextUse = t.NextTimeUse;
            cdState.CDLeft = t.CDLeft;
            return cdState;
        }

        return null;
    }

    private void FixedUpdate()
    {
        if (Time.frameCount < 10)
            return;

        if (!RPGBuilderEssentials.Instance.isInGame) return;

        foreach (var t in abilitiesData)
        {
            if (!(t.NextTimeUse > 0)) continue;
            t.CDLeft -= Time.deltaTime;
            if (!(t.CDLeft <= 0)) continue;
            t.CDLeft = 0;
            t.NextTimeUse = 0;
        }
    }

    public void RESET_CHARACTER_DATA(bool destroyEssentials)
    {
        created = false;
        raceID = -1;
        CharacterName = "";
        gender = RPGRace.RACE_GENDER.Male;
        position = Vector3.zero;
        rotation = Vector3.zero;
        currentGameSceneID = -1;

        actionKeys.Clear();
        
        classDATA.classID = -1;
        classDATA.currentClassLevel = -1;
        classDATA.currentClassXP = 0;
        classDATA.maxClassXP = 0;
        
        inventoryData.baseSlots.Clear();
        inventoryData.bags.Clear();

        armorsEquipped.Clear();
        weaponsEquipped.Clear();
        talentTrees.Clear();
        actionBarSlotsDATA.Clear();
        stealthedActionBarSlotsDATA.Clear();
        skillsDATA.Clear();
        questsData.Clear();
        treePoints.Clear();
        currencies.Clear();
        npcKilled.Clear();
        scenesEntered.Clear();
        regionsEntered.Clear();
        abilitiesLearned.Clear();
        bonusLearned.Clear();
        recipeslearned.Clear();
        resourcenodeslearned.Clear();
        itemsGained.Clear();
        itemsDATA.Clear();
        allRandomizedItems.Clear();
        abilitiesData.Clear();
        recipesData.Clear();
        resourceNodeData.Clear();
        bonusesData.Clear();
        factionsData.Clear();
        weaponTemplates.Clear();
        dialoguesDATA.Clear();
        allocatedStatsData.Clear();
        
        nextAvailableItemID = 0;
        nextAvailableRandomItemID = 0;
        
        menuGameModifierPoints = RPGBuilderEssentials.Instance.generalSettings.baseGameModifierPointsInMenu;
        worldGameModifierPoints = RPGBuilderEssentials.Instance.generalSettings.baseGameModifierPointsInWorld;
        gameModifiersData.Clear();

        if (destroyEssentials) Destroy(RPGBuilderEssentials.Instance.gameObject);
    }
}
