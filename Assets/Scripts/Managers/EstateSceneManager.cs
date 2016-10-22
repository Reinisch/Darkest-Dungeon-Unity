using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

enum EstateSceneState { EstateScreen, QuestScreen, ProvisionScreen }

public class EstateSceneManager : MonoBehaviour
{
    public static EstateSceneManager Instanse { get; set; }

    public GameObject estateUI;
    public GameObject questUI;
    public GameObject provisionUI;

    public CanvasGroup sharedUICanvasGroup;

    public Text estateTitle;
    public CharacterWindow characterWindow;
    public ActivityLogWindow activityLogWindow;
    public GlossaryWindow glossaryWindow;
    public RealmInventoryWindow realmInventoryWindow;
    public TownEventWindow townEventWindow;

    public EstateCurrencyPanel currencyPanel;
    public HeroRosterPanel rosterPanel;
    public EstateBottomPanel bottomPanel;
    public EstateTopPanel topPanel;
    public RaidPartyPanel raidPartyPanel;

    public RaidPreparationManager raidPreparationManager;
    public ShopManager shopManager;
    public TownManager townManager;

    bool menuOpened = false;
    bool realmInventoryOpened = false;
    bool activityLogOpened = false;
    bool characterWindowOpened = false;
    bool transitionsEnabled = true;
    EstateSceneState estateSceneState;

    public bool AnyWindowsOpened
    {
        get
        {
            return menuOpened || realmInventoryOpened || activityLogOpened ||
                characterWindowOpened || glossaryWindow.isActiveAndEnabled || townEventWindow.isActiveAndEnabled;
        }
    }

    void Awake()
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

        shopManager.partyInventory.Initialize();
        DarkestDungeonManager.SkipTransactions = false;
        (townManager.buildingWindows[8] as NomadWagonWindow).wagonInventory.onTrinketSell += realmInventoryWindow.AddTrinket;

        characterWindow.onWindowClose += RosterCharacterInfoClose;
        activityLogWindow.onWindowClose += ActivityLogClose;
        realmInventoryWindow.onWindowClose += RealmInventoryClose;
        rosterPanel.onHeroResurrection += RosterPanel_onHeroResurrection;

        DarkestDungeonManager.MainMenu.onWindowClose += MainMenuClose;

        rosterPanel.onHeroInspect += CharacterWindowSwitch;

        bottomPanel.onActivityIconClick += ActivityLogClick;
        bottomPanel.onRealmInventoryIconClick += RealmInventoryClick;
        bottomPanel.onMainMenuIconClick += MainMenuClick;
        bottomPanel.onGlossaryIconClick += GlossaryClick;
        bottomPanel.onTownEventIconClick += TownEventClick;

        bottomPanel.onPrepareEmbarkButtonClick += EmbarkButtonClick;
        bottomPanel.onProvisionButtonClick += ProvisionClick;
        bottomPanel.onFinalEmbarkButtonClick += FinalEmbarkClick;

        topPanel.onReturnButtonClick += ReturnButtonClick;

        raidPartyPanel.onPartyAssembled += EnableEmbarkToProvision;
        raidPartyPanel.onPartyDisassembled += DisableEmbarkToProvision;

        DarkestDungeonManager.Instanse.UpdateSceneOverlay(GameObject.FindGameObjectWithTag("Main UI Camera").GetComponent<Camera>());
        DarkestDungeonManager.Instanse.mainMenu.uiCanvasGroup = GameObject.Find("UI_Shared").GetComponent<CanvasGroup>();
    }
    void Start()
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
                        "Screen/loading_screen.plot_" + (DarkestDungeonManager.SaveData.Quest as PlotQuest).Id);

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
            bottomPanel.townEventButton.gameObject.SetActive(false);
        else
        {
            bottomPanel.townEventButton.gameObject.SetActive(true);
            townEventWindow.UpdateEvent(DarkestDungeonManager.Campaign.TriggeredEvent);
        }

        currencyPanel.UpdateCurrency();
        rosterPanel.InitializeRoster();
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
    }
    void OnDestroy()
    {
        DarkestDungeonManager.MainMenu.onWindowClose -= MainMenuClose;
    }

    IEnumerator LoadEstateEvent()
    {
        sharedUICanvasGroup.blocksRaycasts = false;
        DarkestDungeonManager.Instanse.screenFader.StartFaded();
        yield return new WaitForEndOfFrame();
        DarkestDungeonManager.Instanse.screenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        sharedUICanvasGroup.blocksRaycasts = true;
    }

    void EnableEmbarkToProvision()
    {
        bottomPanel.provisionButton.interactable = true;
    }
    void DisableEmbarkToProvision()
    {
        bottomPanel.provisionButton.interactable = false;
    }
    void RosterPanel_onHeroResurrection(DeathRecord record)
    {
        (townManager.buildingWindows[2] as GraveyardWindow).HeroResurrected(record);
        townEventWindow.eventRecruits.UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.EventHeroes);
    }

    #region Quik Start Generation Options
    public List<string> Difficulties = new List<string>() { "1", "3", "5", "6" };
    public List<string> Lengths = new List<string>() { "1", "2", "3" };
    public List<string> Dungeons = new List<string>()
    {
        "crypts",
        "weald",
        "warrens",
        "cove",
    };
    public List<string> Types = new List<string>()
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

    public void QuickStart()
    {
        DarkestDungeonManager.SkipTransactions = true;
        DarkestDungeonManager.Instanse.RaidingManager.QuickStart(shopManager.partyInventory);
        if(DarkestDungeonManager.Instanse.RaidingManager.Quest.IsPlotQuest)
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen.plot_" +
                (DarkestDungeonManager.Instanse.RaidingManager.Quest as PlotQuest).Id);
        else
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen." +
                DarkestDungeonManager.Instanse.RaidingManager.Quest.Dungeon + "_0");

        DarkestDungeonManager.Campaign.CheckEmbarkBuffs(DarkestDungeonManager.RaidManager.RaidParty);

        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
    }
    public void QuickProgress()
    {
        if (characterWindowOpened)
            characterWindow.WindowClosed();
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

        (townManager.buildingWindows[0] as AbbeyWindow).UpdateSlots();
        (townManager.buildingWindows[3] as SanitariumWindow).UpdateSlots();
        (townManager.buildingWindows[4] as TavernWindow).UpdateSlots();
        (townManager.buildingWindows[8] as NomadWagonWindow).wagonInventory.UpdateShop();
        (townManager.buildingWindows[9] as StageCoachWindow).recruitPanel.
            UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.Heroes);

        rosterPanel.UpdateRoster();


        var returnRecord = new PartyActivityRecord(partyRecord,
            new bool[] { Random.value > 0.05f ? true : false,
                    Random.value > 0.05f ? true : false,
                    Random.value > 0.05f ? true : false,
                    Random.value > -1 ? true : false, },
                Random.value > 0.3f ? true : false);

        DarkestDungeonManager.Campaign.CurrentLog().ReturnRecord = returnRecord;
        activityLogWindow.RecalculateHeight();

        if (DarkestDungeonManager.Campaign.TriggeredEvent == null)
            bottomPanel.townEventButton.gameObject.SetActive(false);
        else
        {
            bottomPanel.townEventButton.gameObject.SetActive(true);
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

            var returnRecord = new PartyActivityRecord(partyRecord,
                new bool[] { Random.value > 0.05f ? true : false,
                    Random.value > 0.05f ? true : false,
                    Random.value > 0.05f ? true : false,
                    Random.value > -1 ? true : false, },
                    Random.value > 0.3f ? true : false);

            DarkestDungeonManager.Campaign.CurrentLog().ReturnRecord = returnRecord;
            activityLogWindow.RecalculateHeight();
        }
        (townManager.buildingWindows[0] as AbbeyWindow).UpdateSlots();
        (townManager.buildingWindows[3] as SanitariumWindow).UpdateSlots();
        (townManager.buildingWindows[4] as TavernWindow).UpdateSlots();
        (townManager.buildingWindows[8] as NomadWagonWindow).wagonInventory.UpdateShop();
        (townManager.buildingWindows[9] as StageCoachWindow).recruitPanel.
            UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.Heroes);

        rosterPanel.UpdateRoster();
    }

    public void RosterCharacterInfoClose()
    {
        characterWindowOpened = false;
        characterWindow.gameObject.SetActive(false);
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
    }
    public void TownEventClose()
    {
        bottomPanel.TownEventAnimator.state.SetAnimation(0, "idle", false);
        townEventWindow.gameObject.SetActive(false);
    }

    public void CharacterWindowSwitch(Hero hero, bool interactable)
    {
        if (estateSceneState == EstateSceneState.ProvisionScreen)
            return;

        if (characterWindowOpened)
            characterWindow.UpdateCharacterInfo(hero, interactable);
        else
        {
            characterWindowOpened = true;
            characterWindow.gameObject.SetActive(true);
            characterWindow.UpdateCharacterInfo(hero, interactable);
            if (activityLogOpened)
                ActivityLogClose();
        }
    }
    public void RealmInventoryClick()
    {
        realmInventoryOpened = !realmInventoryOpened;
        if(realmInventoryOpened)
            bottomPanel.RealmInventoryAnimator.state.SetAnimation(0, "selected", true);
        else
        {
            bottomPanel.RealmInventoryAnimator.state.ClearTracks();
            bottomPanel.RealmInventoryAnimator.state.SetAnimation(0, "idle", false);
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
            bottomPanel.ActivityLogAnimator.state.SetAnimation(0, "selected", false);
        else
            bottomPanel.ActivityLogAnimator.state.SetAnimation(0, "idle", false);
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
        }
        else
        {
            bottomPanel.GlossaryAnimator.state.SetAnimation(0, "selected", true);
            glossaryWindow.gameObject.SetActive(true);
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
        }
        else
        {
            bottomPanel.TownEventAnimator.state.SetAnimation(0, "selected", true);
            townEventWindow.gameObject.SetActive(true);
        }
    }

    public void EmbarkButtonClick()
    {
        if (transitionsEnabled && !townManager.BuildingWindowActive)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.onFadeEnded += EmbarkTransitionFadeComplete;
            DarkestDungeonManager.ScreenFader.onAppearEnded += EmbarkTransitionAppearComplete;
            DarkestDungeonManager.ScreenFader.Fade();
        }
    }
    void EmbarkTransitionFadeComplete()
    {
        provisionUI.SetActive(false);
        estateUI.SetActive(false);
        questUI.SetActive(true);

        bottomPanel.activityLogButton.interactable = false;
        raidPartyPanel.ActivateDragManagerBehaviour();
        
        if (raidPartyPanel.IsPartyPrepared)
            EnableEmbarkToProvision();
        else
            DisableEmbarkToProvision();

        raidPreparationManager.Initialize();
        raidPreparationManager.UpdateUI();

        ToolTipManager.Instanse.Hide();
        estateSceneState = EstateSceneState.QuestScreen;
        DarkestDungeonManager.ScreenFader.Appear();
    }
    void EmbarkTransitionAppearComplete()
    {
        DarkestDungeonManager.ScreenFader.onFadeEnded -= EmbarkTransitionFadeComplete;
        DarkestDungeonManager.ScreenFader.onAppearEnded -= EmbarkTransitionAppearComplete;
        DarkestSoundManager.ExecuteNarration("enter_quest_select", NarrationPlace.Town);
        transitionsEnabled = true;
    }

    public void ProvisionClick()
    {
        if (transitionsEnabled)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.onFadeEnded += ProvisionFadeComplete;
            DarkestDungeonManager.ScreenFader.onAppearEnded += ProvisionAppearComplete;
            DarkestDungeonManager.ScreenFader.Fade();
        }
    }
    void ProvisionFadeComplete()
    {
        estateUI.SetActive(false);
        questUI.SetActive(false);
        provisionUI.SetActive(true);

        raidPartyPanel.DeactivateDragManagerBehaviour();
        bottomPanel.activityLogButton.interactable = false;
        bottomPanel.realmInventoryButton.interactable = false;

        RosterCharacterInfoClose();
        RealmInventoryClose();
        ActivityLogClose();


        ToolTipManager.Instanse.Hide();
        shopManager.LoadInitialSetup(raidPreparationManager.SelectedQuestSlot.Quest, raidPreparationManager.raidPartyPanel);
        shopManager.ActivateShopBehaviour();

        estateSceneState = EstateSceneState.ProvisionScreen;
        DarkestDungeonManager.ScreenFader.Appear();
    }
    void ProvisionAppearComplete()
    {
        DarkestDungeonManager.ScreenFader.onFadeEnded -= ProvisionFadeComplete;
        DarkestDungeonManager.ScreenFader.onAppearEnded -= ProvisionAppearComplete;
        DarkestSoundManager.ExecuteNarration("enter_provision_select", NarrationPlace.Town);
        transitionsEnabled = true;
    }

    public void FinalEmbarkClick()
    {
        if (transitionsEnabled)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.onFadeEnded += FinalEmbarkFadeComplete;
            DarkestDungeonManager.ScreenFader.Fade();
            DarkestDungeonManager.Instanse.RaidingManager.DeployFromPreparation(raidPreparationManager, shopManager);      
        }
    }
    void FinalEmbarkFadeComplete()
    {
        DarkestDungeonManager.ScreenFader.Appear();
        DarkestDungeonManager.ScreenFader.onFadeEnded -= FinalEmbarkFadeComplete;
        transitionsEnabled = true;
        shopManager.DeactivateShopBehaviour();
        ToolTipManager.Instanse.Hide();
        DarkestDungeonManager.Campaign.CurrentLog().EmbarkRecord = 
            new PartyActivityRecord(PartyActionType.Embark, DarkestDungeonManager.RaidManager);

        if (DarkestDungeonManager.Instanse.RaidingManager.Quest.IsPlotQuest)
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen." +
                (DarkestDungeonManager.Instanse.RaidingManager.Quest as PlotQuest).Id);
        else
            DarkestDungeonManager.LoadingInfo.SetNextScene("Dungeon", "Screen/loading_screen." + 
                DarkestDungeonManager.Instanse.RaidingManager.Quest.Dungeon + "_0");

        DarkestDungeonManager.Campaign.CheckEmbarkBuffs(DarkestDungeonManager.RaidManager.RaidParty);
        DarkestDungeonManager.Campaign.NarrationTownInfo.Clear();

        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
    }

    public void ReturnButtonClick()
    {
        if (transitionsEnabled)
        {
            transitionsEnabled = false;
            DarkestDungeonManager.ScreenFader.onFadeEnded += ReturnFadeComplete;
            DarkestDungeonManager.ScreenFader.onAppearEnded += ReturnAppearComplete;
            DarkestDungeonManager.ScreenFader.Fade();
        }
    }
    void ReturnFadeComplete()
    {
        switch (estateSceneState)
        {
            case EstateSceneState.QuestScreen:
                estateUI.SetActive(true);
                questUI.SetActive(false);
                bottomPanel.activityLogButton.interactable = true;
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
                bottomPanel.activityLogButton.interactable = true;
                bottomPanel.realmInventoryButton.interactable = true;
                ToolTipManager.Instanse.Hide();
                break;
        }
        DarkestDungeonManager.ScreenFader.Appear();
    }
    void ReturnAppearComplete()
    {
        switch (estateSceneState)
        {
            case EstateSceneState.QuestScreen:
                estateSceneState = EstateSceneState.EstateScreen;
                break;
            case EstateSceneState.ProvisionScreen:
                estateSceneState = EstateSceneState.QuestScreen;
                break;
        }
        DarkestDungeonManager.ScreenFader.onFadeEnded -= ReturnFadeComplete;
        DarkestDungeonManager.ScreenFader.onAppearEnded -= ReturnAppearComplete;
        transitionsEnabled = true;
    }
}