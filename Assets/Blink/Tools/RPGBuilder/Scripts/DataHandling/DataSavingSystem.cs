using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.VersionControl;
#endif
using UnityEngine;
using FileMode = System.IO.FileMode;

public static class DataSavingSystem
{

    public static List<CharacterData> LoadAllCharacters()
    {
        var path = Application.persistentDataPath;
        var di = new DirectoryInfo(path);
        var files = di.GetFiles().Where(o => o.Name.Contains("_CharacterData.txt")).ToArray();
        var allCharacters = new List<CharacterData>();
        foreach (var t in files)
        {
            var charname = t.Name.Replace("_CharacterData.txt", "");
            allCharacters.Add(RPGBuilderJsonSaver.LoadCharacterData(charname));
        }

        return allCharacters;
    }

    public static void DeleteAssetIDFile(AssetIDHandler.ASSET_TYPE_ID assetType)
    {
        var path = getPath(assetType);
        File.Delete(path);
        
#if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }

    public static void SaveAssetID(AssetIDHandler assetIDHandler)
    {
        var formatter = new BinaryFormatter();
        var path = getPath(assetIDHandler.assetType);
        var stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, assetIDHandler);
        stream.Close();
    }

#if UNITY_EDITOR
    public static AssetIDHandler LoadAssetID(AssetIDHandler.ASSET_TYPE_ID assetType)
    {
        var path = getPath(assetType);
        
        Asset thisAsset = Provider.GetAssetByPath(getPath(assetType));
        if (!Provider.isActive) return !File.Exists(path) ? null : HandleFileActions(path);
        if(thisAsset == null) return null;
        if (Provider.IsOpenForEdit(thisAsset)) return !File.Exists(path) ? null : HandleFileActions(path);
        Provider.Checkout(thisAsset, CheckoutMode.Both);

        FileInfo fileInfo = new FileInfo(path);
        fileInfo.IsReadOnly = false;

        return !File.Exists(path) ? null : HandleFileActions(path);
    }
#endif

    private static AssetIDHandler HandleFileActions(string path)
    {
        var formatter = new BinaryFormatter();
        var stream = new FileStream(path, FileMode.Open);
        var data = formatter.Deserialize(stream) as AssetIDHandler;
        stream.Close();
        return data;
    }
    
    private static string getPath(AssetIDHandler.ASSET_TYPE_ID assetType)
    {
        RPGBuilderEditorDATA editorDATA = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
        switch (assetType)
        {
            case AssetIDHandler.ASSET_TYPE_ID.ability:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/AbilitiesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.effect:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/EffectsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.npc:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/NPCsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.stat:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/StatIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.treePoint:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/TreePointsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.item:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/ItemIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.skill:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/SkillsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.levelTemplate:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/LevelTemplatesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.race:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/RacesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID._class:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/ClassesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.lootTable:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/LootTablesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.merchantTable:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/MerchantTableIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.currency:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/CurrenciesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.craftingRecipe:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/CraftingRecipesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.craftingStation:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/CraftingStationsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.talentTree:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/TalentTreesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.bonus:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/BonusesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.task:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/TasksIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.quest:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/QuestsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.worldPosition:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/WorldPositionsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.resourceNode:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/ResourceNodesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.gameScene:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/GameSceneIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.gearSet:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/GearSetsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.enchantment:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/EnchantmentsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.spellbook:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/SpellbooksIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.faction:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/FactionsIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.weaponTemplate:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/WeaponTemplatesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.dialogue:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/DialoguesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.gameModifier:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/GameModifierIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.species:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/SpeciesIDs.THMSV";
            case AssetIDHandler.ASSET_TYPE_ID.combo:
                return editorDATA.ResourcePath + editorDATA.RPGBDatabasePath + "PersistentData/CombosIDs.THMSV";
            default:
                return "";
        }
    }
    
}
