using System.IO;
using BLINK.RPGBuilder;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;


public static class RPGBuilderJsonSaver
{
    
    public static void GenerateCharacterEquippedtemsData()
    {
        if (CharacterData.Instance.armorsEquipped.Count == 0)
            for (var i = 0; i < RPGBuilderEssentials.Instance.itemSettings.armorSlotsList.Count; i++)
                CharacterData.Instance.armorsEquipped.Add(new CharacterData.ArmorsEquippedDATA());
        
        if (CharacterData.Instance.weaponsEquipped.Count == 0)
            for (var i = 0; i < RPGBuilderEssentials.Instance.itemSettings.weaponSlotsList.Count; i++)
                CharacterData.Instance.weaponsEquipped.Add(new CharacterData.WeaponsEquippedDATA());
    }
    
    private static void SaveEquippedItems()
    {

        if (CharacterData.Instance.armorsEquipped.Count <
            RPGBuilderEssentials.Instance.itemSettings.armorSlotsList.Count)
        {
            int diff = RPGBuilderEssentials.Instance.itemSettings.armorSlotsList.Count -
                       CharacterData.Instance.armorsEquipped.Count;

            for (int i = 0; i < diff; i++)
            {
                CharacterData.Instance.armorsEquipped.Add(new CharacterData.ArmorsEquippedDATA());
            }
        }
        if (CharacterData.Instance.armorsEquipped.Count >
            RPGBuilderEssentials.Instance.itemSettings.armorSlotsList.Count)
        {
            int diff = CharacterData.Instance.armorsEquipped.Count - RPGBuilderEssentials.Instance.itemSettings.armorSlotsList.Count;

            for (int i = 0; i < diff; i++)
            {
                CharacterData.Instance.armorsEquipped.RemoveAt(CharacterData.Instance.armorsEquipped.Count-1);
            }
        }

        for (var i = 0; i < CharacterData.Instance.armorsEquipped.Count; i++)
        {
            var itemID = -1;
            var itemDataID = -1;
            if (InventoryManager.Instance.equippedArmors[i].itemEquipped != null)
            {
                itemID = InventoryManager.Instance.equippedArmors[i].itemEquipped.ID;
                itemDataID = InventoryManager.Instance.equippedArmors[i].temporaryItemDataID;
            }

            CharacterData.Instance.armorsEquipped[i].itemID = itemID;
            CharacterData.Instance.armorsEquipped[i].itemDataID = itemDataID;
        }

        if (CharacterData.Instance.weaponsEquipped.Count <
            RPGBuilderEssentials.Instance.itemSettings.weaponSlotsList.Count)
        {
            int diff = RPGBuilderEssentials.Instance.itemSettings.weaponSlotsList.Count -
                       CharacterData.Instance.weaponsEquipped.Count;

            for (int i = 0; i < diff; i++)
            {
                CharacterData.Instance.weaponsEquipped.Add(new CharacterData.WeaponsEquippedDATA());
            }
        }
        if (CharacterData.Instance.weaponsEquipped.Count >
            RPGBuilderEssentials.Instance.itemSettings.weaponSlotsList.Count)
        {
            int diff = CharacterData.Instance.weaponsEquipped.Count - RPGBuilderEssentials.Instance.itemSettings.weaponSlotsList.Count;

            for (int i = 0; i < diff; i++)
            {
                CharacterData.Instance.weaponsEquipped.RemoveAt(CharacterData.Instance.weaponsEquipped.Count-1);
            }
        }

        for (var i = 0; i < CharacterData.Instance.weaponsEquipped.Count; i++)
        {
            var itemID = -1;
            var itemDataID = -1;
            if (InventoryManager.Instance.equippedWeapons[i].itemEquipped != null)
            {
                itemID = InventoryManager.Instance.equippedWeapons[i].itemEquipped.ID;
                itemDataID = InventoryManager.Instance.equippedWeapons[i].temporaryItemDataID;
            }

            CharacterData.Instance.weaponsEquipped[i].itemID = itemID;
            CharacterData.Instance.weaponsEquipped[i].itemDataID = itemDataID;
        }
    }

    public static void SaveCharacterData(string charName, CharacterData charCombatData)
    {
        if (CombatManager.playerCombatNode)
        {
            if (!LoadingScreenManager.Instance.isSceneLoading && !CombatManager.playerCombatNode.dead)
            {
                charCombatData.position = CombatManager.playerCombatNode.transform.position;
                charCombatData.rotation.y = CombatManager.playerCombatNode.transform.eulerAngles.y;
            }
            SaveEquippedItems();
        }

        var json = JsonUtility.ToJson(charCombatData);
        WriteToFile(charName + "_CharacterData.txt", json);
    }

    public static CharacterData LoadCharacterData(string charName)
    {
        foreach (var t in RPGBuilderEssentials.Instance.temporaryDataGO.GetComponents<CharacterData>())
        {
            if (t.CharacterName != charName) continue;
            var json2 = ReadFromFile(t.CharacterName + "_CharacterData.txt");
            JsonUtility.FromJsonOverwrite(json2, t);
            return t;
        }
        CharacterData curCharCombatData = RPGBuilderEssentials.Instance.temporaryDataGO.AddComponent<CharacterData>();
        var json = ReadFromFile(charName + "_CharacterData.txt");
        JsonUtility.FromJsonOverwrite(json, curCharCombatData);
        return curCharCombatData;
    }

    private static void WriteToFile(string fileName, string json)
    {
        var path = GetFilePath(fileName);
        var fileStream = new FileStream(path, FileMode.Create);

        using (var writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }
    }

    public static void DeleteCharacter(string characterName)
    {

        var filePath = Application.persistentDataPath + "/" + characterName + "_CharacterData.txt";

        // check if file exists
        if (!File.Exists(filePath))
            Debug.LogError("This character save file does not exist");
        else
            File.Delete(filePath);
    }

    private static string ReadFromFile(string fileName)
    {
        var path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            using (var reader = new StreamReader(path))
            {
                var json = reader.ReadToEnd();
                return json;
            }
        }
        else
        {
            return "";
        }
    }

    private static string GetFilePath(string fileName)
    {
        return Application.persistentDataPath + "/" + fileName;
    }
}

