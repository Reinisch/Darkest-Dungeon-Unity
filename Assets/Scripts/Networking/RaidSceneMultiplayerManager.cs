using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class RaidSceneMultiplayerManager : RaidSceneManager
{
    #region Multiplayer Setup

    private static Quest MultiplayerQuest = new PlotQuest()
    {
        IsPlotQuest = true,
        Id = "tutorial",
        Type = "tutorial_room",
        Dungeon = "weald",
        Difficulty = 1,
        Length = 1,
        Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals["tutorial_final_room"],
        Reward = new CompletionReward(),
        CanRetreat = false,
        AlwaysRetreatFromRaid = false,
        CompletionDungeonXp = false,
        HasStatueContents = false,
        IsProgression = false,
        IsScoutingEnabled = false,
        IsStressClearedOnCompletion = false,
        IsSurpriseEnabled = false,
        RetreatKillCount = 0,
        RosterBuffOnFailureMinimumPartyResolveLevel = 0,
    };

    private Dungeon MultiplayerDungeon = new Dungeon()
    {
        GridSizeX = 1,
        GridSizeY = 1,
        Name = MultiplayerQuest.Dungeon,
        DungeonMash = DarkestDungeonManager.Data.DungeonEnviromentData[MultiplayerQuest.Dungeon].
            BattleMashes.Find(mash => mash.MashId == MultiplayerQuest.Difficulty),
        SharedMash = DarkestDungeonManager.Data.DungeonEnviromentData["shared"].
            BattleMashes.Find(mash => mash.MashId == MultiplayerQuest.Difficulty),
        Rooms = new Dictionary<string, DungeonRoom>()
        {
            { "room2_1", new DungeonRoom("room2_1", 1, 1)
                {
                    TextureId = "effigy_1", Type = AreaType.Entrance,
                    BattleEncounter = new BattleEncounter() { Cleared = false },
                }
            },
        },
        StartingRoomId = "room2_1",
    };

    #endregion

    protected override void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;

            SaveLoadManager.WriteStartingSave(new SaveCampaignData(4, "MultiplayerTestSave"));
            DarkestDungeonManager.SaveData = SaveLoadManager.ReadSave(4);
            DarkestDungeonManager.Instanse.LoadSave();

            RaidEvents.Initialize();
            currentRaid = new RaidInfo();
            currentRaid.Quest = MultiplayerQuest;
            currentRaid.Dungeon = MultiplayerDungeon;
            currentRaid.RaidParty = new RaidParty(PhotonNetwork.player);

            DarkestDungeonManager.Data.LoadDungeon(currentRaid.Quest.Dungeon, currentRaid.Quest.Id);
            Rules = new RaidRuleInfo(currentRaid.Quest.Dungeon, BattleGround, TorchMeter);

            #if !(UNITY_ANDROID || UNITY_IOS)
            escapeButton.gameObject.SetActive(false);
            #endif
        }
        else
            Destroy(Instanse.gameObject);
    }
    protected override void Start()
    {
        if (Instanse != this)
            return;

        CharacterWindow.onWindowClose += CharacterWindow_onWindowClose;
        CharacterWindow.onNextButtonClick += CharacterWindow_onNextButtonClick;
        CharacterWindow.onPreviousButtonClick += CharacterWindow_onPreviousButtonClick;

        RaidInterface.UpdateRaidScene();
        Inventory.SetDeactivated();
        RaidPanel.bannerPanel.skillPanel.SetMode(SkillPanelMode.Combat);
        RaidPanel.bannerPanel.SetPeacefulState();
        MapPanel.LoadDungeon(currentRaid.Dungeon);
        QuestPanel.UpdateQuest(currentRaid.Quest);
        DarkestSoundManager.StartDungeonSoundtrack(currentRaid.Dungeon.Name);
        TorchMeter.Initialize(100);

        Formations.Initialize();

        currentEvent = RoomLoadingEvent(currentRaid.Dungeon.StartingRoom, RoomTransitionType.Entrance);
        StartCoroutine(currentEvent);
    }
    protected override void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            OnEscapePressed();
    }

    protected override IEnumerator RaidResultsEvent()
    {
        RaidInterface.CanvasGroup.blocksRaycasts = false;
        ToolTipManager.Instanse.Hide();
        Formations.HideHeroOverlay();
        RoomView.DisableInteraction();
        HallwayView.DisableInteraction();

        if (HeroParty.Units.Count > 0)
        {
            if (!currentRaid.Quest.IsPlotQuest)
                DarkestSoundManager.ExecuteNarration("quest_end_completed", NarrationPlace.Raid,
                    currentRaid.Quest.Type, currentRaid.Quest.Dungeon);
        }
        else
        {
            DarkestSoundManager.ExecuteNarration("quest_end_not_completed", NarrationPlace.Raid,
                currentRaid.Quest.Type, currentRaid.Quest.Dungeon);
        }

        DarkestDungeonManager.ScreenFader.Fade(1);
        yield return new WaitForSeconds(1f);
        PhotonGameManager.Instanse.LeaveRoom();
        yield break;
    }
    protected override IEnumerator CompletionCrestEvent()
    {
        Raid.QuestCompleted = true;
        completionWindow.Appear();
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/quest_goal_complete");
        DungeonCamera.blur.enabled = true;
        while (completionWindow.Action == CompletionAction.Waiting)
            yield return null;

        if (completionWindow.Action == CompletionAction.Return)
        {
            yield return StartCoroutine(RaidResultsEvent());
            yield break;
        }

        QuestPanel.CompleteQuest();
        DungeonCamera.blur.enabled = false;
        yield return new WaitForSeconds(1f);
    }
}
