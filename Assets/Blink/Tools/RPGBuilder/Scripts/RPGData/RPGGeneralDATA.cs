using System;
using System.Collections.Generic;
using UnityEngine;

public class RPGGeneralDATA : ScriptableObject
{
    public bool automaticSave;
    public float automaticSaveDelay;
    public bool automaticSaveOnQuit;

    public bool clickToLoadScene;
    public float DelayAfterSceneLoad;
    public float LoadingScreenEndDelay;
    public bool enableDevPanel = true;

    public Sprite mainMenuLoadingImage;
    public string mainMenuSceneName, mainMenuLoadingName, mainMenuLoadingDescription;

    public bool useOldController;

    public string[] dialogueKeywords;
    public List<string> dialogueKeywordsList = new List<string>();

    public bool useGameModifiers;
    public int negativePointsRequired;
    public bool checkMinNegativeModifier, checkMaxPositiveModifier;
    public int minimumRequiredNegativeGameModifiers;
    public int maximumRequiredPositiveGameModifiers;
    public int baseGameModifierPointsInMenu;
    public int baseGameModifierPointsInWorld;

    public enum ControllerTypes
    {
        ThirdPerson,
        ThirdPersonShooter,
        TopDownClickToMove,
        TopDownWASD,
        FirstPerson
    }
    
    public List<string> ActionKeyCategoryList = new List<string>();
    
    [System.Serializable]
    public class ActionKey
    {
        public string actionName;
        public string actionDisplayName;
        public KeyCode defaultKey;
        public bool isUnique;
        public string category;
    }

    public List<ActionKey> actionKeys = new List<ActionKey>();

    public LayerMask worldInteractableLayer;
    
    public void updateThis(RPGGeneralDATA newData)
    {
        automaticSave = newData.automaticSave;
        automaticSaveDelay = newData.automaticSaveDelay;
        automaticSaveOnQuit = newData.automaticSaveOnQuit;
        clickToLoadScene = newData.clickToLoadScene;
        mainMenuSceneName = newData.mainMenuSceneName;
        mainMenuLoadingImage = newData.mainMenuLoadingImage;
        mainMenuLoadingName = newData.mainMenuLoadingName;
        mainMenuLoadingDescription = newData.mainMenuLoadingDescription;
        enableDevPanel = newData.enableDevPanel;
        useOldController = newData.useOldController;
        dialogueKeywordsList = newData.dialogueKeywordsList;
        useGameModifiers = newData.useGameModifiers;
        negativePointsRequired = newData.negativePointsRequired;
        minimumRequiredNegativeGameModifiers = newData.minimumRequiredNegativeGameModifiers;
        maximumRequiredPositiveGameModifiers = newData.maximumRequiredPositiveGameModifiers;
        baseGameModifierPointsInMenu = newData.baseGameModifierPointsInMenu;
        baseGameModifierPointsInWorld = newData.baseGameModifierPointsInWorld;
        checkMinNegativeModifier = newData.checkMinNegativeModifier;
        checkMaxPositiveModifier = newData.checkMaxPositiveModifier;
        actionKeys = newData.actionKeys;
        ActionKeyCategoryList = newData.ActionKeyCategoryList;
        worldInteractableLayer = newData.worldInteractableLayer;
        DelayAfterSceneLoad = newData.DelayAfterSceneLoad;
        LoadingScreenEndDelay = newData.LoadingScreenEndDelay;
    }
}