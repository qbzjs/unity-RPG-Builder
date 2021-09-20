using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RPGBuilderEditorDATA : ScriptableObject
{
    public bool disableStartupWindow = false;

    public string getRPGBVersion()
    {
        return "1.1.0.5";
    }
    
    public enum ThemeTypes
    {
        Dark,
        Light
    }

    public ThemeTypes curEditorTheme;
    public bool increasedEditorUpdates = true;
    
    public string ResourcePath = "Assets/Blink/Tools/RPGBuilder/Resources/", RPGBDatabasePath = "Database/", RPGBEditorDataPath = "EditorData/";
    public string exportDirectoryPath = "", exportDirectoryName = "", exportDirectoryFullPath = "";
    public string importDirectoryPath = "";
    public string upgradeDirectoryPath = "";
    public bool overrideDatabaseWhenImporting;

    public float MinEditorWidth = 1050;
    public float MinEditorHeight = 550;
        
    public Sprite RPGBuilderLogo, RPGBuilderLogoHover, BlinkLogoOff, BlinkLogoOn, BlinkBanner, BlinkSmallLogoOff, BlinkSmallLogoOn;
    
    public float CategoryWidthPercent, CategoryWidthPercentHover, SubCategoryWidthPercent, SubCategoryWidthPercentHover, ElementListWidthPercent, ViewWidthPercent, FilterWidthPercent, TopBarHeightPercent;
    public float viewSmallFieldHeight, smallButtonHeight;
    public float labelFieldWidth = 100f, fieldContentWidth = 400f, filterLabelFieldWidth = 50;
    public float SmallActionButtonWidth, MediumActionButtonWidth, BigActionButtonWidth;
    
    public Texture2D bannerBegin, bannerMiddle, bannerEnd;
    public Color bannerColor, bannerColorHover, bannerCollapsedColor, addButtonColor, removeButtonColor;

    public float CategoriesY;
    public float actionButtonsY;
    public float ModuleButtonsY;


    public Texture2D DarkThemeBackground, LightThemeBackground;
    public Texture2D DarkThemeBackgroundHover, LightThemeBackgroundHover;
    public Texture2D DarkThemeModuleSearchBackground, DarkThemeModuleSearchBackgroundHover, LightThemeModuleSearchBackground, LightThemeModuleSearchBackgroundHover;
    public Texture2D searchIcon;
    public Sprite defaultElementIcon;
    public Texture2D  abilityNullSprite,
        gearSetsSeparator,
        smallSeparator;


    public List<GameObject> polytopeStudioAssets_GO = new List<GameObject>();
    public List<AudioClip> cafofoAssets_AUDIO = new List<AudioClip>();
    public List<GameObject> GabbrielAguiarAsset_GO = new List<GameObject>();
    public List<GameObject> RDRAssets_GO = new List<GameObject>();
    public List<GameObject> TitanForgeAssets_GO = new List<GameObject>();
    public List<Sprite> PONETIAssets_SPRITE = new List<Sprite>();
    public List<GameObject> MalbersAnimationAssets_GO = new List<GameObject>();

    public Sprite polytopePartnerImage,
        cafofoPartnerImage,
        GabrielAguiarPartnerImage,
        RDRPartnerImage,
        TitanForgePartnerImage,
        PONETIPartnerImage,
        MalbersAnimationParterImage,
        BOXOPHOBICPartnerImage,
        PolyartStudioPartnerImage,
        StaggartPartnerImage,
        InfinityPBRPartnerImage;



    [Serializable]
    public class SceneLoaderData
    {
        public string sceneName;

#if UNITY_EDITOR
        public SceneAsset scene;
#endif
    }

    public List<SceneLoaderData> sceneLoaderList = new List<SceneLoaderData>();
    
    [Serializable]
    public class CategoriesDATA
    {
        public string CategoryName;

        [Serializable]
        public class SubCategoriesDATA
        {
            public string SubCategoryName;
            public string assetType;
            public AssetIDHandler.ASSET_TYPE_ID assetIDType;
            public string folderName;
            public bool Active = true;
            public bool exportOn = true;
            public bool importOn = true;
        }

        public SubCategoriesDATA[] subCategoriesData;

        public bool Active = true;
    }

    public CategoriesDATA[] categoriesData;

    public Mesh npcSpawnerMesh;
}