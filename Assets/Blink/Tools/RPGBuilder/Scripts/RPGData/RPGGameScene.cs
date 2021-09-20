using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using UnityEngine;

public class RPGGameScene : ScriptableObject
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public string displayName;
    public string description;

    public Sprite loadingBG;
    public Sprite minimapImage;
    public Bounds mapBounds;
    public Vector2 mapSize;

    [CoordinateID] public int startPositionID = -1;
    public RPGWorldPosition startPositionREF;

    public bool isProceduralScene;
    public string SpawnPointName;
    
    [System.Serializable]
    public class REGION_DATA
    {
        public string regionName;
        public bool fogChange, lightningChange, skyboxChange, cameraParticleChange, musicChange, combatModeChange, combatStateChange, welcomeText, gameActions, taskCompletion;

        public bool showInEditor;
        // FOG SETTINGS
        public bool fogEnabled = true;
        public Color fogColor;
        public FogMode fogMode = FogMode.Linear;
        public float fogDensity, fogStartDistance, fogEndDistance;
        public float fogTransitionSpeed = 0.5f;
        
        // LIGHTNING SETTINGS
        public bool lightEnabled = true;
        public Color lightColor;
        public float lightIntensity;
        public string lightGameobjectName;
        public float lightTransitionSpeed = 0.5f;
        
        // LIGHTNING SETTINGS
        //public Material skyboxMaterial;
        public Texture skyboxCubemap;
        public float skyboxTransitionSpeed = 0.5f;
        
        // CAMERA PARTICLE SETTINGS
        public GameObject cameraParticle;
        
        // MUSIC SETTINGS
        [RPGDataList] public List<AudioClip> musicClips = new List<AudioClip>();
        
        // COMBAT MODE SETTINGS
        public bool combatEnabled;
        
        // COMBAT STATE SETTINGS
        public bool inCombat;
        
        // WELCOME MESSAGE SETTINGS
        public string welcomeMessageText;
        public float welcomeMessageDuration;
        
        // GAME ACTIONS SETTINGS
        [RPGDataList] public List<RPGBGameActions> GameActionsList = new List<RPGBGameActions>();

    }

    [RPGDataList] public List<REGION_DATA> regions = new List<REGION_DATA>();

    public void updateThis(RPGGameScene newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        description = newData.description;
        displayName = newData.displayName;
        loadingBG = newData.loadingBG;
        minimapImage = newData.minimapImage;
        mapBounds = newData.mapBounds;
        mapSize = newData.mapSize;
        startPositionID = newData.startPositionID;
        regions = newData.regions;
        isProceduralScene = newData.isProceduralScene;
        SpawnPointName = newData.SpawnPointName;
    }
}