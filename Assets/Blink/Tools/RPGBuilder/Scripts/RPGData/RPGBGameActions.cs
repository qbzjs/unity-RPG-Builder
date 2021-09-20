using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Logic
{
    [System.Serializable]
    public class RPGBGameActions
    {
        public ActionType actionType;

        public int assetID = -1;

        public RPGAbility abilityREF;
        public RPGEffect effectREF;
        public int effectRank;
        public RPGItem itemREF;
        public RPGCraftingRecipe craftingRecipeREF;
        public RPGResourceNode resourceNodeREF;
        public RPGBonus bonusREF;
        public RPGTreePoint treePointREF;
        public RPGSkill skillREF;
        public RPGTalentTree talentTreeREF;
        public RPGWeaponTemplate weaponTemplateREF;
        public RPGQuest questREF;
        public RPGCurrency currencyREF;
        public RPGFaction factionREF;
        public RPGGameScene gameSceneREF;
        public GameObject prefab;
        public Vector3 spawnPosition;
        public AudioClip audioClip;

        public RPGDialogueTextNode textNodeREF;
        public int dialogueTextNodeInstanceID;

        public string GameObjectName;
        public string animationName;

        public float chance = 100f;
        public int count = 1;
        public RPGCombatDATA.TARGET_TYPE target;
        public TELEPORT_TYPE teleportType;
    }

    public enum TELEPORT_TYPE
    {
        gameScene,
        position
    }

    public enum ActionType
    {
        UseAbility,
        ApplyEffect,
        GainItem,
        RemoveItem,
        LearnAbility,
        LearnRecipe,
        LearnResourceNode,
        LearnBonus,
        LearnSkill,
        LearnTalentTree,
        GainTreePoint,
        LoseTreePoint,
        GainEXP,
        GainSkillEXP,
        GainWeaponTemplateEXP,
        GainLevel,
        GainSkillLevel,
        ProposeQuest,
        GainCurrency,
        LoseCurrency,
        SpawnGameObject,
        DestroyGameObject,
        ActivateGameObject,
        DeactiveGameObject,
        ToggleWorldNode,
        GainFactionpoints,
        LoseFactionPoints,
        PlaySound,
        Teleport,
        SaveCharacterData,
        RemoveEffect,
        PlayAnimation,
        CompleteDialogueLine
    }
}
