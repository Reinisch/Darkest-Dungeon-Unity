using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class DarkestDungeonManager : MonoBehaviour
{
    public ScreenFader screenFader;
    public MainMenuWindow mainMenu;

    public Material sharedHighlight;
    public Material sharedGrayscale;
    public Material sharedGrayhighlight;
    public Material sharedGrayDark;
    public Material sharedFullDark;
    public Material sharedDeactivated;
    public Material sharedDeactHighlight;

    public static bool SkipTransactions { get; set; }
    public static bool GamePaused { get; set; }

    public static DarkestDungeonManager Instanse
    {
        get;
        private set;
    }

    public static Material HighlightMaterial
    {
        get
        {
            return Instanse.sharedHighlight;
        }
    }
    public static Material GrayMaterial
    {
        get
        {
            return Instanse.sharedGrayscale;
        }
    }
    public static Material GrayHighlightMaterial
    {
        get
        {
            return Instanse.sharedGrayhighlight;
        }
    }
    public static Material GrayDarkMaterial
    {
        get
        {
            return Instanse.sharedGrayDark;
        }
    }
    public static Material FullGrayDarkMaterial
    {
        get
        {
            return Instanse.sharedFullDark;
        }
    }
    public static Material DeactivatedMaterial
    {
        get
        {
            return Instanse.sharedDeactivated;
        }
    }
    public static Material DeactivatedHighlightedMaterial
    {
        get
        {
            return Instanse.sharedDeactHighlight;
        }
    }

    public static Campaign Campaign
    {
        get
        {
            return Instanse.campaign;
        }
    }
    public static DarkestDatabase Data
    {
        get
        {
            return Instanse.database;
        }
    }
    public static ScreenFader ScreenFader
    {
        get
        {
            return Instanse.screenFader;
        }
    }
    public static RaidManager RaidManager
    {
        get
        {
            return Instanse.RaidingManager;
        }
    }
    public static HeroSpriteDatabase HeroSprites
    {
        get
        {
            return Instanse.database.HeroSprites;
        }
    }
    public static LoadingScreenInfo LoadingInfo
    {
        get
        {
            return Instanse.nextSceneInfo;
        }
    }
    public static SaveCampaignData SaveData
    {
        get
        {
            return Instanse.loadedSaveData;
        }
        set
        {
            Instanse.loadedSaveData = value;
        }
    }

    public static float RandomBarkChance { get; set; }

    public Canvas MainMenuUI { get; set; }
    public Camera MainUICamera { get; set; }

    public DragManager DragManager { get; private set; }
    public RaidManager RaidingManager { get; private set; }
    public ToolTipManager ToolTipManager { get; private set; }
    public LocalizationManager LocalizationManager { get; private set; }

    private Campaign campaign;
    private DarkestDatabase database;
    private SaveCampaignData loadedSaveData;
    private LoadingScreenInfo nextSceneInfo = new LoadingScreenInfo();

    void Awake()
    {
        if (Instanse == null)
        {
            RandomBarkChance = 0.2f;

            Instanse = this;
            DontDestroyOnLoad(gameObject);

            RaidingManager = GetComponent<RaidManager>();
            ToolTipManager = GetComponent<ToolTipManager>();
            DragManager = GetComponent<DragManager>();
            LocalizationManager = GetComponent<LocalizationManager>();

            MainMenuUI = GetComponentInChildren<Canvas>();
            MainUICamera = GameObject.FindGameObjectWithTag("Main UI Camera").GetComponent<Camera>();
            UpdateSceneOverlay(MainUICamera);

            database = GetComponent<DarkestDatabase>();
            database.Load();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (SceneManager.GetActiveScene().name == "Dungeon")
        {
            LoadSave();
            if (SaveData.InRaid == false)
            {
                var raidPanel = FindObjectOfType<RaidPanel>();
                raidPanel.inventoryPanel.partyInventory.Initialize();
                Instanse.RaidingManager.QuickStart(raidPanel.inventoryPanel.partyInventory);
            }
        }
        else if (SceneManager.GetActiveScene().name == "EstateManagement")
        {
            LoadSave();
        }
    }
    void Start()
    {
        if (database == null)
            return;
    }

    public void LoadSave()
    {
        if (SaveData == null)
        {
            SaveLoadManager.WriteStartingSave(new SaveCampaignData(1, "Darkest"));
            SaveLoadManager.WriteTestingSave(new SaveCampaignData(2, "Middle"));
            SaveData = SaveLoadManager.ReadSave(2);
        }
        campaign = new Campaign();
        campaign.Load(SaveData);
    }
    public void SaveGame()
    {
        SaveLoadManager.WriteSave(SaveData);
    }
    public void UpdateSceneOverlay(Camera screenOverlayCam)
    {
        var screenOverlayRect = MainMenuUI.GetComponent<RectTransform>();
        ToolTipManager.OverlayCamera = screenOverlayCam;
        ToolTipManager.OverlayRect = screenOverlayRect;

        DragManager.OverlayCamera = screenOverlayCam;
        DragManager.OverlayRect = screenOverlayRect;

        MainMenuUI.worldCamera = screenOverlayCam;
        MainUICamera = screenOverlayCam;
    }
}