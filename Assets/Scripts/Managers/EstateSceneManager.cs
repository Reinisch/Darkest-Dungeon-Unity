using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public enum EstateSceneState { EstateScreen, QuestScreen, ProvisionScreen }

public class EstateSceneManager : MonoBehaviour
{
    public static EstateSceneManager Instanse { get; private set; }

    [SerializeField]
    private GameObject estateUI;
    [SerializeField]
    private GameObject questUI;
    [SerializeField]
    private GameObject provisionUI;
    [SerializeField]
    private CanvasGroup sharedUICanvasGroup;

    [SerializeField]
    private Text estateTitle;
    [SerializeField]
    private CharacterWindow characterWindow;
    [SerializeField]
    private ActivityLogWindow activityLogWindow;
    [SerializeField]
    private GlossaryWindow glossaryWindow;
    [SerializeField]
    private RealmInventoryWindow realmInventoryWindow;
    [SerializeField]
    private TownEventWindow townEventWindow;

    [SerializeField]
    private EstateCurrencyPanel currencyPanel;
    [SerializeField]
    private HeroRosterPanel rosterPanel;
    [SerializeField]
    private EstateBottomPanel bottomPanel;
    [SerializeField]
    private EstateTopPanel topPanel;
    [SerializeField]
    private RaidPartyPanel raidPartyPanel;
    [SerializeField]
    private HeirloomExchangePanel exchangePanel;

    [SerializeField]
    private RaidPreparationManager raidPreparationManager;
    [SerializeField]
    private ShopManager shopManager;
    [SerializeField]
    private TownManager townManager;

    public TownManager TownManager { get { return townManager; } }
    public CharacterWindow CharacterWindow { get { return characterWindow; } }
    public EstateCurrencyPanel CurrencyPanel { get { return currencyPanel; } }
    public HeroRosterPanel RosterPanel { get { return rosterPanel; } }

    public bool AnyWindowsOpened
    {
        get
        {
            return menuOpened || realmInventoryOpened || activityLogOpened ||
                characterWindowOpened || glossaryWindow.isActiveAndEnabled || townEventWindow.isActiveAndEnabled;
        }
    }

    private EstateSceneState EstateSceneState { get; set; }

    private bool menuOpened;
    private bool realmInventoryOpened;
    private bool activityLogOpened;
    private bool characterWindowOpened;
    private bool transitionsEnabled = true;

    #region Quik Start Generation Options

    public List<string> Difficulties = new List<string> { "1", "3", "5", "6" };

    public List<string> Lengths = new List<string> { "1", "2", "3" };

    public List<string> Dungeons = new List<string>
    {
        "crypts",
        "weald",
        "warrens",
        "cove",
    };

    public List<string> Types = new List<string>
    {
        "explore", "explore", "explore", "explore", "explore", "explore", "explore", "explore", "explore",
        "cleanse", "cleanse", "cleanse", "cleanse", "cleanse", "cleanse", "cleanse", "cleanse", "cleanse",
        "gather", "gather", "gather", "gather", "gather", "gather", "gather", "gather", "gather", "gather",
        "inventory_activate","inventory_activate","inventory_activate","inventory_activate","inventory_activate",
        "plot_kill_necromancer_1",
        "plot_kill_necromancer_2",
        "plot_kill_necromancer_3",
        "plot_kill_hag_1",
        "plot_kill_hag_2",
        "plot_kill_hag_3",
        "plot_kill_swine_prince_1",
        "plot_kill_swine_prince_2",
        "plot_kill_swine_prince_3",
        "plot_kill_prophet_1",
        "plot_kill_prophet_2",
        "plot_kill_prophet_3",
        "plot_kill_brigand_cannon_1",
        "plot_kill_brigand_cannon_2",
        "plot_kill_brigand_cannon_3",
        "plot_kill_formless_flesh_1",
        "plot_kill_formless_flesh_2",
        "plot_kill_formless_flesh_3",
        "plot_kill_siren_1",
        "plot_kill_siren_2",
        "plot_kill_siren_3",
        "plot_kill_drowned_crew_1",
        "plot_kill_drowned_crew_2",
        "plot_kill_drowned_crew_3",
    };

    #endregion

    private void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;
        }
        else
        {
            Destroy(Instanse.gameObject);
            return;
        }

        shopManager.PartyInventory.Initialize();
        DarkestDungeonManager.SkipTransactions = false;
        ((NomadWagonWindow) townManager.BuildingWindows[8]).Inventory.EventTrinketSell += realmInventoryWindow.AddTrinket;

        CharacterWindow.EventWindowClosed += RosterCharacterInfoClose;
        activityLogWindow.EventWindowClosed += ActivityLogClose;
        realmInventoryWindow.EventWindowClosed += RealmInventoryClose;
        RosterPanel.EventHeroResurrected += RosterPanelHeroResurrected;

        DarkestDungeonManager.MainMenu.EventWindowClosed += MainMenuClose;

        RosterPanel.EventHeroInspected += CharacterWindowSwitch;

        bottomPanel.EventActivityIconClick += ActivityLogClick;
        bottomPanel.EventRealmInventoryIconClick += RealmInventoryClick;
        bottomPanel.EventMainMenuIconClick += MainMenuClick;
        bottomPanel.EventGlossaryIconClick += GlossaryClick;
        bottomPanel.EventTownEventIconClick += TownEventClick;

        bottomPanel.EventPrepareEmbarkButtonClick += EmbarkButtonClick;
        bottomPanel.EventProvisionButtonClick += ProvisionClick;
        bottomPanel.EventFinalEmbarkButtonClick += FinalEmbarkClick;

        topPanel.EventReturnButtonClick += ReturnButtonClick;

        raidPartyPanel.EventPartyAssembled += EnableEmbarkToProvision;
        raidPartyPanel.EventPartyDisassembled += DisableEmbarkToProvision;

        CurrencyPanel.EventCurrencyIncreased += exchangePanel.CurrencyUpdated;
        CurrencyPanel.EventCurrencyDecreased += exchangePanel.CurrencyUpdated;

        DarkestDungeonManager.Instanse.UpdateSceneOverlay(GameObject.FindGameObjectWithTag("Main UI Camera").GetComponent<Camera>());
        DarkestDungeonManager.MainMenu.UICanvasGroup = GameObject.Find("UI_Shared").GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (Instanse != this)
            return;

        if (DarkestDungeonManager.RaidManager.Status != RaidStatus.Preparation)
        {
            DarkestDungeonManager.Campaign.NarrationRaidInfo.Clear();

            foreach (var heroInfo in DarkestDungeonManager.RaidManager.RaidParty.HeroInfo)
            {
                if (heroInfo.IsAlive)
                {
                    heroInfo.Hero.TownReset();
                }
                else
                {
                    DarkestDungeonManager.Campaign.Heroes.Remove(heroInfo.Hero);
                    DarkestDungeonManager.Campaign.Estate.Graveyard.Records.Add(heroInfo.DeathRecord);
                }
            }

            if (DarkestDungeonManager.RaidManager.Quest.Goal.Id == "tutorial_final_room")
            {
                DarkestDungeonManager.Campaign.ExecuteProgress();
                DarkestDungeonManager.Campaign.CurrentLog().ReturnRecord =
                    new PartyActivityRecord(PartyActionType.Tutorial, DarkestDungeonManager.RaidManager);
            }
            else
            {
                DarkestDungeonManager.Campaign.ExecuteProgress();
                DarkestDungeonManager.Campaign.AdvanceNextWeek();
                DarkestDungeonManager.Campaign.CurrentLog().ReturnRecord = 
                    new PartyActivityRecord(PartyActionType.Result, DarkestDungeonManager.RaidManager);
            }

            DarkestSoundManager.ExecuteNarration("town_visit_start", NarrationPlace.Town,
                DarkestDungeonManager.RaidManager.Status == RaidStatus.Success ? "successes" : "fail",
                DarkestDungeonManager.Campaign.TriggeredEvent != null ?
                DarkestDungeonManager.Campaign.TriggeredEvent.Id : "not_triggered",
                DarkestDungeonManager.Campaign.GuaranteedEvent != null ?
                DarkestDungeonManager.Campaign.GuaranteedEvent.Id : "not_guaranteed");
        }
        else
        {
            if (DarkestDungeonManager.SaveData.InRaid)
            {
                DarkestDungeonManager.SkipTransactions = true;
                if (!DarkestDungeonManager.SaveData.Quest.IsPlotQuest ||
                    DarkestDungeonManager.SaveData.Quest.Goal.Id == "tutorial_final_room")
                    DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon",
                        "Screen/loading_screen." + DarkestDungeonManager.SaveData.Quest.Dungeon + "_0");
                else
                    DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon",
                        "Screen/loading_screen.plot_" + ((PlotQuest) DarkestDungeonManager.SaveData.Quest).Id);

                SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
                return;
            }
            else
            {
                DarkestSoundManager.ExecuteNarration("town_visit_start", NarrationPlace.Town);
            }
        }

        glossaryWindow.Initialize();
        activityLogWindow.Initialize();

        if (DarkestDungeonManager.Campaign.TriggeredEvent == null)
            bottomPanel.TownEventButton.gameObject.SetActive(false);
        else
        {
            bottomPanel.TownEventButton.gameObject.SetActive(true);
            townEventWindow.UpdateEvent(DarkestDungeonManager.Campaign.TriggeredEvent);
        }

        CurrencyPanel.UpdateCurrency();
        RosterPanel.InitializeRoster();
        realmInventoryWindow.Populate();
        estateTitle.text = string.Format(LocalizationManager.GetString("estate_title_format"),
            DarkestDungeonManager.Campaign.Estate.EstateTitle);

        townManager.InitializeBuildings();

        StartCoroutine(LoadEstateEvent());

        if(DarkestDungeonManager.RaidManager.Status != RaidStatus.Preparation)
        {
            DarkestDungeonManager.SaveData.UpdateFromEstate();
            DarkestDungeonManager.Instanse.SaveGame();
            DarkestDungeonManager.RaidManager.Status = RaidStatus.Preparation;
        }

        DarkestSoundManager.StartTownSoundtrack();
    }

    private void OnDestroy()
    {
        DarkestDungeonManager.MainMenu.EventWindowClosed -= MainMenuClose;

        CurrencyPanel.EventCurrencyIncreased -= exchangePanel.CurrencyUpdated;
        CurrencyPanel.EventCurrencyDecreased -= exchangePanel.CurrencyUpdated;
    }

    private IEnumerator LoadEstateEvent()
    {
        sharedUICanvasGroup.blocksRaycasts = false;
        DarkestDungeonManager.ScreenFader.StartFaded();
        yield return new WaitForEndOfFrame();
        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        sharedUICanvasGroup.blocksRaycasts = true;
    }

    private void EnableEmbarkToProvision()
    {
        bottomPanel.ProvisionButton.interactable = true;
    }

    private void DisableEmbarkToProvision()
    {
        bottomPanel.ProvisionButton.interactable = false;
    }

    private void RosterPanelHeroResurrected(DeathRecord record)
    {
        ((GraveyardWindow) townManager.BuildingWindows[2]).HeroResurrected(record);
        townEventWindow.EventRecruits.UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.EventHeroes);
        DarkestSoundManager.PlayOneShot("event:/town/graveyard_resurrect");
    }

    public void OnHeirloomExchange()
    {
        if (townManager.BuildingWindowActive)
        {
            BuildingWindow openedWindow = townManager.BuildingWindows.Find(window => window.gameObject.activeSelf);
            if (openedWindow != null)
                openedWindow.UpdateUpgradeTrees(true);
        }
    }

    public void OnSceneLeave(bool embarking = false)
    {
        DarkestSoundManager.StopTownSoundtrack();

        if(!embarking)
        {
            if (raidPartyPanel.PartySlots != null)
                foreach (var slot in raidPartyPanel.PartySlots)
                    if (slot.SelectedHero != null)
                        slot.SelectedHero.SetStatus(HeroStatus.Available);

            if (EstateSceneState == EstateSceneState.ProvisionScreen)
                shopManager.SellOutEverything();
        }
    }

    public void QuickStart()
    {
        DarkestDungeonManager.SkipTransactions = true;
        DarkestDungeonManager.Instanse.RaidingManager.QuickStart(shopManager.PartyInventory);
        if(DarkestDungeonManager.Instanse.RaidingManager.Quest.IsPlotQuest)
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen.plot_" +
                ((PlotQuest) DarkestDungeonManager.Instanse.RaidingManager.Quest).Id);
        else
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen." +
                DarkestDungeonManager.Instanse.RaidingManager.Quest.Dungeon + "_0");

        DarkestDungeonManager.Campaign.CheckEmbarkBuffs(DarkestDungeonManager.RaidManager.RaidParty);
        OnSceneLeave();
        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
    }

    public void QuickProgress()
    {
        if (characterWindowOpened)
            CharacterWindow.WindowClosed();
        if (activityLogOpened)
            activityLogWindow.WindowClosed();
        if (realmInventoryOpened)
            realmInventoryWindow.WindowClosed();
        if (townManager.BuildingWindowActive)
            townManager.CloseBuildingWindow();
        if (glossaryWindow.isActiveAndEnabled)
            GlossaryClose();
        if (townEventWindow.isActiveAndEnabled)
            TownEventClose();

        var partyRecord = new PartyActivityRecord(PartyActionType.Embark,
            Types[Random.Range(0, Types.Count)], Difficulties[Random.Range(0, Difficulties.Count)],
            Lengths[Random.Range(0, Lengths.Count)], Dungeons[Random.Range(0, Dungeons.Count)],
            DarkestDungeonManager.Campaign.Heroes.Where(item => true).OrderBy(x => Random.value).Take(4).ToList());

        DarkestDungeonManager.Campaign.CurrentLog().EmbarkRecord = partyRecord;

        activityLogWindow.RecalculateHeight();
        DarkestDungeonManager.Campaign.ExecuteProgress();
        activityLogWindow.RecalculateHeight();
        DarkestDungeonManager.Campaign.AdvanceNextWeek();
        activityLogWindow.ProgressWeek();

        UpdateTownInfo();

        var returnRecord = new PartyActivityRecord(partyRecord, new[] { Random.value > 0.05f,
            Random.value > 0.05f, Random.value > 0.05f, Random.value > -1, }, Random.value > 0.3f);

        DarkestDungeonManager.Campaign.CurrentLog().ReturnRecord = returnRecord;
        activityLogWindow.RecalculateHeight();

        if (DarkestDungeonManager.Campaign.TriggeredEvent == null)
            bottomPanel.TownEventButton.gameObject.SetActive(false);
        else
        {
            bottomPanel.TownEventButton.gameObject.SetActive(true);
            townEventWindow.UpdateEvent(DarkestDungeonManager.Campaign.TriggeredEvent);
        }

        //ProgressLoop();
    }

    public void ProgressLoop()
    {
        for (int i = 0; i < 10000; i++)
        {
            var partyRecord = new PartyActivityRecord(PartyActionType.Embark,
                Types[Random.Range(0, Types.Count)], Difficulties[Random.Range(0, Difficulties.Count)],
                Lengths[Random.Range(0, Lengths.Count)], Dungeons[Random.Range(0, Dungeons.Count)],
                DarkestDungeonManager.Campaign.Heroes.Where(item => true).OrderBy(x => Random.value).Take(4).ToList());

            DarkestDungeonManager.Campaign.CurrentLog().EmbarkRecord = partyRecord;

            activityLogWindow.RecalculateHeight();
            DarkestDungeonManager.Campaign.ExecuteProgress();
            activityLogWindow.ProgressWeek();

            var returnRecord = new PartyActivityRecord(partyRecord, new[] { Random.value > 0.05f,
                    Random.value > 0.05f, Random.value > 0.05f, Random.value > -1, }, Random.value > 0.3f);

            DarkestDungeonManager.Campaign.CurrentLog().ReturnRecord = returnRecord;
            activityLogWindow.RecalculateHeight();
        }

        UpdateTownInfo();
    }

    private void UpdateTownInfo()
    {
        ((AbbeyWindow)townManager.BuildingWindows[0]).UpdateSlots();
        ((SanitariumWindow)townManager.BuildingWindows[3]).UpdateSlots();
        ((TavernWindow)townManager.BuildingWindows[4]).UpdateSlots();
        ((NomadWagonWindow)townManager.BuildingWindows[8]).Inventory.UpdateShop();
        ((StageCoachWindow)townManager.BuildingWindows[9]).Panel.
            UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.Heroes);

        RosterPanel.UpdateRoster();
    }

    #region Estate Window Actions

    public void RosterCharacterInfoClose()
    {
        characterWindowOpened = false;
        CharacterWindow.gameObject.SetActive(false);
    }

    public void RealmInventoryClose()
    {
        realmInventoryOpened = false;
        bottomPanel.RealmInventoryAnimator.state.ClearTracks();
        bottomPanel.RealmInventoryAnimator.state.SetAnimation(0, "idle", false);
        realmInventoryWindow.gameObject.SetActive(realmInventoryOpened);
    }

    public void ActivityLogClose()
    {
        activityLogOpened = false;
        bottomPanel.ActivityLogAnimator.state.SetAnimation(0, "idle", false);
        activityLogWindow.gameObject.SetActive(activityLogOpened);
        DarkestSoundManager.PlayOneShot("event:/ui/town/page_close");
    }

    public void MainMenuClose()
    {
        menuOpened = false;
        bottomPanel.SettingsAnimator.state.SetAnimation(0, "idle_loop", true);
    }

    public void GlossaryClose()
    {
        bottomPanel.GlossaryAnimator.state.SetAnimation(0, "idle", false);
        glossaryWindow.gameObject.SetActive(false);
        DarkestSoundManager.PlayOneShot("event:/ui/town/page_close");
    }

    public void TownEventClose()
    {
        bottomPanel.TownEventAnimator.state.SetAnimation(0, "idle", false);
        townEventWindow.gameObject.SetActive(false);
        DarkestSoundManager.PlayOneShot("event:/ui/town/page_close");
    }

    public void CharacterWindowSwitch(Hero hero, bool interactable)
    {
        if (EstateSceneState == EstateSceneState.ProvisionScreen)
            return;

        DarkestSoundManager.PlayOneShot("event:/ui/town/page_open");

        if (characterWindowOpened)
            CharacterWindow.UpdateCharacterInfo(hero, interactable);
        else
        {
            characterWindowOpened = true;
            CharacterWindow.gameObject.SetActive(true);
            CharacterWindow.UpdateCharacterInfo(hero, interactable);
            if (activityLogOpened)
                ActivityLogClose();
        }
    }

    public void RealmInventoryClick()
    {
        realmInventoryOpened = !realmInventoryOpened;
        if(realmInventoryOpened)
        {
            bottomPanel.RealmInventoryAnimator.state.SetAnimation(0, "selected", true);
            DarkestSoundManager.PlayOneShot("event:/ui/town/trinket_open");
        }
        else
        {
            bottomPanel.RealmInventoryAnimator.state.ClearTracks();
            bottomPanel.RealmInventoryAnimator.state.SetAnimation(0, "idle", false);
            DarkestSoundManager.PlayOneShot("event:/ui/town/trinket_close");
        }

        realmInventoryWindow.gameObject.SetActive(realmInventoryOpened);
        if (activityLogOpened)
        {
            ActivityLogClose();
        }
    }

    public void MainMenuClick()
    {
        menuOpened = !menuOpened;
        if (menuOpened)
        {
            bottomPanel.SettingsAnimator.state.SetAnimation(0, "selected", false);
            bottomPanel.SettingsAnimator.state.AddAnimation(0, "selected_loop", true, 0.2f);
            DarkestDungeonManager.MainMenu.gameObject.SetActive(menuOpened);
            DarkestDungeonManager.MainMenu.OpenMenu();
        }
        else
        {
            DarkestDungeonManager.MainMenu.WindowClosed();
        }
    }

    public void ActivityLogClick()
    {
        activityLogOpened = !activityLogOpened;
        if(activityLogOpened)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/page_open");
            bottomPanel.ActivityLogAnimator.state.SetAnimation(0, "selected", false);
        }
        else
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/page_close");
            bottomPanel.ActivityLogAnimator.state.SetAnimation(0, "idle", false);
        }
        activityLogWindow.gameObject.SetActive(activityLogOpened);
        if (activityLogOpened)
        {
            if (realmInventoryOpened)
                RealmInventoryClose();
            if (characterWindowOpened)
                RosterCharacterInfoClose();
        }
    }

    public void GlossaryClick()
    {
        if (townManager.BuildingWindowActive)
            return;

        if(glossaryWindow.isActiveAndEnabled)
        {
            bottomPanel.GlossaryAnimator.state.SetAnimation(0, "idle", false);
            glossaryWindow.gameObject.SetActive(false);
            DarkestSoundManager.PlayOneShot("event:/ui/town/page_close");
        }
        else
        {
            bottomPanel.GlossaryAnimator.state.SetAnimation(0, "selected", true);
            glossaryWindow.gameObject.SetActive(true);
            DarkestSoundManager.PlayOneShot("event:/ui/town/page_open");
        }
    }

    public void TownEventClick()
    {
        if (townManager.BuildingWindowActive)
            return;

        if (townEventWindow.isActiveAndEnabled)
        {
            bottomPanel.TownEventAnimator.state.SetAnimation(0, "idle", false);
            townEventWindow.gameObject.SetActive(false);
            DarkestSoundManager.PlayOneShot("event:/ui/town/page_close");
        }
        else
        {
            bottomPanel.TownEventAnimator.state.SetAnimation(0, "selected", true);
            townEventWindow.gameObject.SetActive(true);
            DarkestSoundManager.PlayOneShot("event:/ui/town/page_open");
        }
    }

    #endregion

    #region Raid Preparation Transitions

    public void EmbarkButtonClick()
    {
        if (transitionsEnabled && !townManager.BuildingWindowActive)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.EventFadeEnded += EmbarkTransitionFadeEnded;
            DarkestDungeonManager.ScreenFader.EventAppearEnded += EmbarkTransitionAppearEnded;
            DarkestDungeonManager.ScreenFader.Fade();
            DarkestSoundManager.PlayOneShot("event:/ui/town/embark_button");
    }
    }

    public void ProvisionClick()
    {
        if (transitionsEnabled)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.EventFadeEnded += ProvisionFadeEnded;
            DarkestDungeonManager.ScreenFader.EventAppearEnded += ProvisionAppearEnded;
            DarkestDungeonManager.ScreenFader.Fade();
            DarkestSoundManager.PlayOneShot("event:/ui/town/provision_button");
        }
    }

    public void FinalEmbarkClick()
    {
        if (transitionsEnabled)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.EventFadeEnded += FinalEmbarkFadeEnded;
            DarkestDungeonManager.ScreenFader.Fade();
            DarkestDungeonManager.Instanse.RaidingManager.DeployFromPreparation(raidPreparationManager, shopManager);
            DarkestSoundManager.PlayOneShot("event:/ui/town/set_off_button");
            OnSceneLeave(true);
        }
    }

    public void ReturnButtonClick()
    {
        if (transitionsEnabled)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.EventFadeEnded += ReturnFadeEnded;
            DarkestDungeonManager.ScreenFader.EventAppearEnded += ReturnAppearEnded;
            DarkestDungeonManager.ScreenFader.Fade();
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_click_back");
        }
    }

    private void EmbarkTransitionFadeEnded()
    {
        provisionUI.SetActive(false);
        estateUI.SetActive(false);
        questUI.SetActive(true);

        bottomPanel.ActivityLogButton.interactable = false;
        raidPartyPanel.ActivateDragManagerBehaviour();
        
        if (raidPartyPanel.IsPartyPrepared)
            EnableEmbarkToProvision();
        else
            DisableEmbarkToProvision();

        raidPreparationManager.Initialize();
        raidPreparationManager.UpdateUI();

        ToolTipManager.Instanse.Hide();
        EstateSceneState = EstateSceneState.QuestScreen;
        DarkestDungeonManager.ScreenFader.Appear();
    }

    private void EmbarkTransitionAppearEnded()
    {
        DarkestDungeonManager.ScreenFader.EventFadeEnded -= EmbarkTransitionFadeEnded;
        DarkestDungeonManager.ScreenFader.EventAppearEnded -= EmbarkTransitionAppearEnded;
        DarkestSoundManager.ExecuteNarration("enter_quest_select", NarrationPlace.Town);
        transitionsEnabled = true;
        raidPartyPanel.CheckComposition();
    }

    private void ProvisionFadeEnded()
    {
        estateUI.SetActive(false);
        questUI.SetActive(false);
        provisionUI.SetActive(true);

        raidPartyPanel.DeactivateDragManagerBehaviour();
        bottomPanel.ActivityLogButton.interactable = false;
        bottomPanel.RealmInventoryButton.interactable = false;

        RosterCharacterInfoClose();
        RealmInventoryClose();
        ActivityLogClose();


        ToolTipManager.Instanse.Hide();
        shopManager.LoadInitialSetup(raidPreparationManager.SelectedQuestSlot.Quest, raidPreparationManager.RaidPartyPanel);
        shopManager.ActivateShopBehaviour();

        EstateSceneState = EstateSceneState.ProvisionScreen;
        DarkestDungeonManager.ScreenFader.Appear();
    }

    private void ProvisionAppearEnded()
    {
        DarkestDungeonManager.ScreenFader.EventFadeEnded -= ProvisionFadeEnded;
        DarkestDungeonManager.ScreenFader.EventAppearEnded -= ProvisionAppearEnded;
        DarkestSoundManager.ExecuteNarration("enter_provision_select", NarrationPlace.Town);
        transitionsEnabled = true;
    }

    private void FinalEmbarkFadeEnded()
    {
        DarkestDungeonManager.ScreenFader.Appear();
        DarkestDungeonManager.ScreenFader.EventFadeEnded -= FinalEmbarkFadeEnded;
        transitionsEnabled = true;
        shopManager.DeactivateShopBehaviour();
        ToolTipManager.Instanse.Hide();
        DarkestDungeonManager.Campaign.CurrentLog().EmbarkRecord = 
            new PartyActivityRecord(PartyActionType.Embark, DarkestDungeonManager.RaidManager);

        if (DarkestDungeonManager.Instanse.RaidingManager.Quest.IsPlotQuest)
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen." +
                ((PlotQuest)DarkestDungeonManager.Instanse.RaidingManager.Quest).Id);
        else
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen." + 
                DarkestDungeonManager.Instanse.RaidingManager.Quest.Dungeon + "_0");

        DarkestDungeonManager.Campaign.CheckEmbarkBuffs(DarkestDungeonManager.RaidManager.RaidParty);
        DarkestDungeonManager.Campaign.NarrationTownInfo.Clear();

        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
    }

    private void ReturnFadeEnded()
    {
        switch (EstateSceneState)
        {
            case EstateSceneState.QuestScreen:
                estateUI.SetActive(true);
                questUI.SetActive(false);
                bottomPanel.ActivityLogButton.interactable = true;
                raidPartyPanel.DeactivateDragManagerBehaviour();
                ToolTipManager.Instanse.Hide();
                break;
            case EstateSceneState.ProvisionScreen:
                shopManager.SellOutEverything();
                shopManager.DeactivateShopBehaviour();

                provisionUI.SetActive(false);
                questUI.SetActive(true);
                raidPartyPanel.ActivateDragManagerBehaviour();
                
                if (raidPartyPanel.IsPartyPrepared)
                    EnableEmbarkToProvision();
                else
                    DisableEmbarkToProvision();

                raidPreparationManager.UpdateUI();
                bottomPanel.ActivityLogButton.interactable = true;
                bottomPanel.RealmInventoryButton.interactable = true;
                ToolTipManager.Instanse.Hide();
                break;
        }
        DarkestDungeonManager.ScreenFader.Appear();
    }

    private void ReturnAppearEnded()
    {
        switch (EstateSceneState)
        {
            case EstateSceneState.QuestScreen:
                EstateSceneState = EstateSceneState.EstateScreen;
                break;
            case EstateSceneState.ProvisionScreen:
                EstateSceneState = EstateSceneState.QuestScreen;
                raidPartyPanel.CheckComposition();
                break;
        }
        DarkestDungeonManager.ScreenFader.EventFadeEnded -= ReturnFadeEnded;
        DarkestDungeonManager.ScreenFader.EventAppearEnded -= ReturnAppearEnded;
        transitionsEnabled = true;
    }

    #endregion
}