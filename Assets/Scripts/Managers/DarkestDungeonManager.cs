using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class DarkestDungeonManager : MonoBehaviour
{
    [SerializeField]
    private ScreenFader screenFader;
    [SerializeField]
    private MainMenuWindow mainMenu;

    [SerializeField]
    private Material sharedHighlight;
    [SerializeField]
    private Material sharedGrayscale;
    [SerializeField]
    private Material sharedGrayhighlight;
    [SerializeField]
    private Material sharedGrayDark;
    [SerializeField]
    private Material sharedFullDark;
    [SerializeField]
    private Material sharedDeactivated;
    [SerializeField]
    private Material sharedDeactHighlight;

    public static bool SkipTransactions { get; set; }
    public static bool GamePaused { get; set; }

    public static DarkestDungeonManager Instanse { get; private set; }

    public static Material HighlightMaterial { get { return Instanse.sharedHighlight; } }
    public static Material GrayMaterial { get { return Instanse.sharedGrayscale; } }
    public static Material GrayHighlightMaterial { get { return Instanse.sharedGrayhighlight; } }
    public static Material GrayDarkMaterial { get { return Instanse.sharedGrayDark; } }
    public static Material FullGrayDarkMaterial { get { return Instanse.sharedFullDark; } }
    public static Material DeactivatedMaterial { get { return Instanse.sharedDeactivated; } }
    public static Material DeactivatedHighlightedMaterial { get { return Instanse.sharedDeactHighlight; } }

    public static Campaign Campaign { get { return Instanse.campaign; } }
    public static DarkestDatabase Data { get { return Instanse.database; } }
    public static ScreenFader ScreenFader { get { return Instanse.screenFader; } }
    public static RaidManager RaidManager { get { return Instanse.RaidingManager; } }
    public static HeroSpriteDatabase HeroSprites { get { return Instanse.database.HeroSprites; } }
    public static LoadingScreenInfo LoadingInfo { get { return Instanse.nextSceneInfo; } }
    public static MainMenuWindow MainMenu { get { return Instanse.mainMenu; } }
    public static float RandomBarkChance { get; private set; }
    public RaidManager RaidingManager { get; private set; }
    public Camera MainUICamera { get; private set; }

    public static SaveCampaignData SaveData
    {
        get { return Instanse.loadedSaveData; }
        set { Instanse.loadedSaveData = value; }
    }

    private Canvas MainMenuUI { get; set; }
    private DragManager DragManager { get; set; }

    private Campaign campaign;
    private DarkestDatabase database;
    private SaveCampaignData loadedSaveData;
    private readonly LoadingScreenInfo nextSceneInfo = new LoadingScreenInfo();

    private void Awake()
    {
        if (Instanse == null)
        {
            RandomBarkChance = 0.2f;

            Instanse = this;
            DontDestroyOnLoad(gameObject);

            RaidingManager = GetComponent<RaidManager>();
            DragManager = GetComponent<DragManager>();

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
                raidPanel.InventoryPanel.PartyInventory.Initialize();
                Instanse.RaidingManager.QuickStart(raidPanel.InventoryPanel.PartyInventory);
            }
        }
        else if (SceneManager.GetActiveScene().name == "EstateManagement")
        {
            LoadSave();
        }
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

        DragManager.OverlayCamera = screenOverlayCam;
        DragManager.OverlayRect = screenOverlayRect;

        MainMenuUI.worldCamera = screenOverlayCam;
        MainUICamera = screenOverlayCam;
    }
}