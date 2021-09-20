using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameActionsManager : MonoBehaviour
{
    
    void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public static GameActionsManager Instance { get; private set; }

    public void UseAbility(CombatNode caster, int abilityID)
    {
        CombatManager.Instance.InitAbility(caster, RPGBuilderUtilities.GetAbilityFromID(abilityID), false);
    }
    
    public void ApplyEffect(RPGCombatDATA.TARGET_TYPE targetType, CombatNode caster, int effectID)
    {
        CombatNode target = targetType == RPGCombatDATA.TARGET_TYPE.Caster ? caster : CombatManager.playerCombatNode;
        if (target == null)
        {
            ErrorEventsDisplayManager.Instance.ShowErrorEvent("The target is not valid", 3);
            return;
        }
        CombatManager.Instance.ExecuteEffect(caster, target, RPGBuilderUtilities.GetEffectFromID(effectID), 0, null, 0);
    }
    
    public void GainItem(int itemID, int count)
    {
        int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(itemID, count, false, false);
        if (itemsLeftOver != 0)
        {
            ErrorEventsDisplayManager.Instance.ShowErrorEvent("The inventory is full", 3);
        }
    }
    
    public void LoseItem(int itemID, int count)
    {
        InventoryManager.Instance.RemoveItem(itemID, count, -1, -1, false);
    }
    
    public void LearnAbility(int ID)
    {
        if(!RPGBuilderUtilities.isAbilityKnown(ID))RPGBuilderUtilities.setAbilityData(ID, 1, true);
    }
    
    public void LearnRecipe(int ID)
    {
        if(!RPGBuilderUtilities.isRecipeKnown(ID))RPGBuilderUtilities.setRecipeData(ID, 1, true);
    }
    
    public void LearnResourceNode(int ID)
    {
        if(!RPGBuilderUtilities.isResourceNodeKnown(ID))RPGBuilderUtilities.setResourceNodeData(ID, 1, true);
    }
    
    public void LearnBonus(int ID)
    {
        if(!RPGBuilderUtilities.isBonusKnown(ID))RPGBuilderUtilities.setBonusData(ID, 1, true);
    }
    
    public void LearnSkill()
    {
        
    }
    
    public void LearnTalentTree()
    {
        
    }
    
    public void GainTreePoint(int ID, int count)
    {
        TreePointsManager.Instance.AddTreePoint(ID, count);
    }
    
    public void LoseTreePoint(int ID, int count)
    {
        TreePointsManager.Instance.RemoveTreePoint(ID, count);
    }
    
    public void GainEXP (int amount)
    {
        LevelingManager.Instance.AddClassXP(amount);
    }
    
    public void GainSkillEXP(int ID, int amount)
    {
        LevelingManager.Instance.AddSkillXP(ID, amount);
    }
    
    public void GainWeaponTemplateEXP(int ID, int amount)
    {
        LevelingManager.Instance.AddWeaponTemplateXP(ID, amount);
    }
    
    public void GainLevel(int amount)
    {
        LevelingManager.Instance.AddClassLevel(amount);
    }
    
    public void GainSkillLevel(int ID, int amount)
    {
        LevelingManager.Instance.AddSkillLevel(ID, amount);
    }
    
    public void ProposeQuest(int questID)
    {
        QuestInteractionDisplayManager.Instance.InitializeQuestContent(RPGBuilderUtilities.GetQuestFromID(questID), false);
    }
    
    public void GainCurrency(int ID, int amount)
    {
        InventoryManager.Instance.AddCurrency(ID, amount);
    }
    
    public void LoseCurrency(int ID, int amount)
    {
        InventoryManager.Instance.RemoveCurrency(ID, amount);
    }
    
    public void SpawnGameobject(GameObject prefab, Vector3 position)
    {
        Instantiate(prefab, position, Quaternion.identity);
    }
    
    public void DestroyGameobject(string GOName)
    {
        Destroy(GameObject.Find(GOName));
    }
    
    public void ToggleWorldNode()
    {
        
    }
    
    public void GainFactionpoint(int ID, int amount)
    {
        FactionManager.Instance.AddFactionPoint(ID, amount);
    }
    public void LoseFactionpoint(int ID, int amount)
    {
        FactionManager.Instance.RemoveFactionPoint(ID, amount);
    }
    
    public void ActivateGameObject(string GOName)
    {
        GameObject.Find(GOName).SetActive(true);
    }
    public void DeactivateGameObject(string GOName)
    {
        GameObject.Find(GOName).SetActive(false);
    }
    public void PlaySound(AudioClip audioClip)
    {
        if (!(Camera.main is null)) Camera.main.GetComponent<AudioSource>().PlayOneShot(audioClip);
    }
    public void TeleportPlayer(int gameSceneID, Vector3 position, TELEPORT_TYPE teleportType)
    {
        switch (teleportType)
        {
            case TELEPORT_TYPE.gameScene:
                    RPGBuilderEssentials.Instance.TeleportToGameScene(gameSceneID, position);
                break;
            case TELEPORT_TYPE.position:
                CombatManager.playerCombatNode.playerControllerEssentials.TeleportToTarget(position);
                break;
        }
    }

    public void SaveCharacterData()
    {
        RPGBuilderJsonSaver.SaveCharacterData(CharacterData.Instance.CharacterName, CharacterData.Instance);
    }
    
    public void RemoveCharacterEffect(int ID)
    {
        CombatManager.playerCombatNode.RemoveEffect(ID);
    }

    public void PlayAnimation(CombatNode NPC, RPGDialogueTextNode.IdentityType identityType, string animationParameter)
    {
        switch (identityType)
        {
            case RPGDialogueTextNode.IdentityType.NPC:
                NPC.GetComponent<Animator>().SetTrigger(animationParameter);
                break;
            case RPGDialogueTextNode.IdentityType.Player:
                CombatManager.playerCombatNode.playerControllerEssentials.anim.SetTrigger(animationParameter);
                break;
        }
    }
    
    public void CompleteDialogueLine(int dialogueID, RPGDialogueTextNode textNode)
    {
       RPGBuilderUtilities.completeDialogueLine(dialogueID, textNode);
    }
    
    public void TriggerGameActions(List<RPGBGameActions> gameActionsList)
        {
            foreach (var gameAction in gameActionsList)
            {
                var chance = Random.Range(0, 100f);
                if (gameAction.chance != 0 && !(chance <= gameAction.chance)) continue;
                switch (gameAction.actionType)
                {
                    case ActionType.UseAbility:
                        UseAbility(CombatManager.playerCombatNode, gameAction.assetID);
                        break;
                    case ActionType.ApplyEffect:
                        ApplyEffect(gameAction.target, CombatManager.playerCombatNode, gameAction.assetID);
                        break;
                    case ActionType.GainItem:
                        GainItem(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.RemoveItem:
                        LoseItem(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LearnAbility:
                        LearnAbility(gameAction.assetID);
                        break;
                    case ActionType.LearnRecipe:
                        LearnRecipe(gameAction.assetID);
                        break;
                    case ActionType.LearnResourceNode:
                        LearnResourceNode(gameAction.assetID);
                        break;
                    case ActionType.LearnBonus:
                        LearnBonus(gameAction.assetID);
                        break;
                    case ActionType.LearnSkill:
                        Debug.LogError("Learn Skill option not implemented yet");
                        break;
                    case ActionType.LearnTalentTree:
                        Debug.LogError("Learn Talent Tree option not implemented yet");
                        break;
                    case ActionType.GainTreePoint:
                        GainTreePoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LoseTreePoint:
                        LoseTreePoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.GainEXP:
                        GainEXP(gameAction.count);
                        break;
                    case ActionType.GainSkillEXP:
                        GainSkillEXP(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.GainWeaponTemplateEXP:
                        GainWeaponTemplateEXP(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.GainLevel:
                        GainLevel(gameAction.count);
                        break;
                    case ActionType.GainSkillLevel:
                        GainSkillLevel(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.ProposeQuest:
                        ProposeQuest(gameAction.assetID);
                        break;
                    case ActionType.GainCurrency:
                        GainCurrency(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LoseCurrency:
                        LoseCurrency(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.SpawnGameObject:
                        SpawnGameobject(gameAction.prefab, gameAction.spawnPosition);
                        break;
                    case ActionType.DestroyGameObject:
                        DestroyGameobject(gameAction.GameObjectName);
                        break;
                    case ActionType.ActivateGameObject:
                        ActivateGameObject(gameAction.GameObjectName);
                        break;
                    case ActionType.DeactiveGameObject:
                        DeactivateGameObject(gameAction.GameObjectName);
                        break;
                    case ActionType.ToggleWorldNode:
                        Debug.LogError("Toggle World Node option not implemented yet");
                        break;
                    case ActionType.GainFactionpoints:
                        GainFactionpoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.LoseFactionPoints:
                        LoseFactionpoint(gameAction.assetID, gameAction.count);
                        break;
                    case ActionType.PlaySound:
                        PlaySound(gameAction.audioClip);
                        break;
                    case ActionType.Teleport:
                        TeleportPlayer(gameAction.assetID, gameAction.spawnPosition, gameAction.teleportType);
                        break;
                    case ActionType.SaveCharacterData:
                        SaveCharacterData();
                        break;
                    case ActionType.RemoveEffect:
                        RemoveCharacterEffect(gameAction.assetID);
                        break;
                    case ActionType.PlayAnimation:
                        PlayAnimation(CombatManager.playerCombatNode, RPGDialogueTextNode.IdentityType.Player, gameAction.animationName);
                        break;
                }
            }
        }
    
}
