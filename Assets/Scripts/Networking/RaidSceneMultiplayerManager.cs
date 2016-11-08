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

            // just testing
            int sessionSeed = 0;
            foreach(var player in PhotonNetwork.playerList)
            {
                Random.InitState(player.ID + player.ToString().GetHashCode());
                sessionSeed += Random.Range(0, (int)Mathf.Pow(2, 16));
            }
            Random.InitState(sessionSeed);

            RaidEvents.Initialize();
            currentRaid = new RaidInfo();
            currentRaid.Quest = MultiplayerQuest;
            currentRaid.Dungeon = MultiplayerDungeon;
            currentRaid.RaidParty = new RaidParty(PhotonNetwork.player);

            DarkestDungeonManager.ScreenFader.StartFaded();
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

        if (PhotonNetwork.room.playerCount < 2)
        {
            Raid.Dungeon.StartingRoom.BattleEncounter.Cleared = true;
            Raid.QuestCompleted = true;
            QuestPanel.CompleteQuest();
        }

        currentEvent = RoomLoadingEvent(currentRaid.Dungeon.StartingRoom, RoomTransitionType.Entrance);
        StartCoroutine(currentEvent);
    }
    protected override void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            OnEscapePressed();
    }

    protected override IEnumerator RoomLoadingEvent(DungeonRoom room, RoomTransitionType transitionType, RaidHallSector fromRaidSector = null)
    {
        #region Set restrictions
        QuestPanel.DisableRetreat(false);
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        Formations.LockSelections();
        RaidPanel.heroPanel.equipmentPanel.SetDisabled();
        #endregion

        #region Switch room scene
        SceneState = DungeonSceneState.Room;

        HallwayView.SetActive(false);
        RoomView.SetActive(true);

        partyController.TranseferToPassage(RoomView.hallwayPassage);
        Formations.TransferToRoom(roomView);

        Raid.CurrentLocation = room;
        if (Raid.LastRoom == null)
            Raid.LastRoom = room;
        else if (fromRaidSector != null && transitionType != RoomTransitionType.Teleport)
            Raid.LastRoom = fromRaidSector.HallSector.Hallway.OppositeRoom(room);
        else
            Raid.LastSector = null;

        roomView.LoadRoom(room, fromRaidSector != null ?
            fromRaidSector.HallSector : null,
            (transitionType == RoomTransitionType.CombatLoad ||
            transitionType == RoomTransitionType.Teleport));

        if (transitionType == RoomTransitionType.FromHallway)
            MapPanel.FocusTarget();
        else
        {
            MapPanel.InstantScaleTarget();
            yield return new WaitForEndOfFrame();
            MapPanel.InstantFocusTarget();
        }

        for (int i = 0; i < HeroParty.Units.Count; i++)
            HeroParty.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0);
        for (int i = 0; i < Formations.monsters.overlay.OverlaySlots.Count; i++)
            Formations.monsters.overlay.OverlaySlots[i].RectTransform.pivot = new Vector2(0.5f, 0f);
        #endregion

        #region Load combat save
        if (transitionType == RoomTransitionType.CombatLoad)
        {
            currentEvent = LoadEncounterEvent(RoomView.raidRoom);
            StartCoroutine(currentEvent);
            yield break;
        }
        #endregion

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.5f);
        #region Show dungeon
        DarkestDungeonManager.ScreenFader.Appear(2f);
        #region Check for teleport actions
        if (transitionType == RoomTransitionType.Teleport && !room.HasActiveBattle)
        {
            if (RaidPanel.SelectedUnit != null)
                RaidPanel.SelectedUnit.OverlaySlot.UnitSelected();

            #region Execute Hero Transformations
            for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
            {
                var hero = Formations.heroes.party.Units[i].Character as Hero;
                if (Formations.heroes.party.Units[i].Character.Mode != null
                    && Formations.heroes.party.Units[i].Character.Mode.AfflictionSkillId != null)
                {
                    var battleFinishSkill = hero.SelectedCombatSkills.Find(skill => skill.Id ==
                        Formations.heroes.party.Units[i].Character.Mode.BattleCompleteSkillId);
                    if (battleFinishSkill != null)
                    {
                        SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(Formations.heroes.party.Units[i],
                            Formations.heroes.party.Units[i], battleFinishSkill).
                            UpdateSkillInfo(Formations.heroes.party.Units[i], battleFinishSkill);
                        yield return StartCoroutine(ExecuteHeroSkill(Formations.heroes.party.Units[i],
                            targetInfo, battleFinishSkill));
                    }
                }
            }
            #endregion

            foreach (var hero in Formations.heroes.party.Units)
                hero.SetCombatAnimation(false);

            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return new WaitForSeconds(0.3f);

            if (HeroParty.Units.Count == 0)
            {
                StartCoroutine(RaidResultsEvent());
                yield break;
            }
        }
        else if (transitionType == RoomTransitionType.Teleport && room.HasActiveBattle)
        {
            currentEvent = EncounterEvent(RoomView.raidRoom);
            StartCoroutine(currentEvent);
            yield break;
        }
        else
            yield return new WaitForSeconds(0.5f);
        #endregion

        RaidPanel.SwitchBlocked = false;
        if (fromRaidSector == null)
        {
            Formations.heroes.overlay.Show();
            yield return new WaitForEndOfFrame();
        }
        Formations.ShowHeroOverlay();
        #endregion

        #region Battle encounter
        if (room.HasActiveBattle)
        {
            DisablePartyMovement();

            if (room.Knowledge == Knowledge.Hidden || room.Knowledge == Knowledge.Scouted)
            {
                room.Knowledge = Knowledge.Visited;
                Raid.ExploredRoomCount++;
                MapPanel.UpdateArea(room);
            }
            yield break;
        }
        else
        {
            Formations.UnlockSelections();

            if (room.Knowledge == Knowledge.Hidden || room.Knowledge == Knowledge.Scouted)
            {
                room.Knowledge = Knowledge.Visited;
                Raid.ExploredRoomCount++;
                MapPanel.UpdateArea(room);
            }

            if (!Raid.QuestCompleted && Raid.CheckQuestGoals())
                yield return StartCoroutine(CompletionCrestEvent());
        }
        #endregion

        #region Remove restrictions
        QuestPanel.EnableRetreat();
        MapPanel.ShowAvailableRooms(room);
        Inventory.SetPeacefulState(false);
        RaidPanel.heroPanel.equipmentPanel.SetActive();
        currentEvent = null;
        #endregion
    }
    protected override IEnumerator EncounterEvent(IRaidArea areaView, bool campfireAmbush = false)
    {
        #region Set Combat States and Restrictions
        QuestPanel.UpdateEncounterRetreat();
        QuestPanel.SetCombatState();
        DisableEnviroment();
        DisablePartyMovement();
        Formations.LockSelections();
        Formations.ResetSelections();
        RaidPanel.SetDisabledState();
        Inventory.SetDeactivated();
        RaidPanel.heroPanel.equipmentPanel.SetDisabled();

        foreach (var hero in Formations.heroes.party.Units)
            hero.SetCombatAnimation(true);
        #endregion

        #region Wait For Events
        while (IsUnitEventInProgress)
            yield return null;
        #endregion

        #region Switch Soundtrack
        DarkestSoundManager.PauseDungeonSoundtrack();
        DarkestSoundManager.StartBattleSoundtrack(Raid.Dungeon.Name, SceneState == DungeonSceneState.Room);

        if (areaView.Area.Type == AreaType.Boss || Raid.Quest.Id == "tutorial")
        {
            if (areaView.Area.BattleEncounter.Monsters.Count > 0)
            {
                DarkestSoundManager.ExecuteNarration("combat_start", NarrationPlace.Raid,
                    areaView.Area.BattleEncounter.Monsters.Select(monster => monster.Class).ToArray());
            }
        }
        #endregion

        #region Battle Loop
        BattleGround.InitiateBattle();
        yield return new WaitForSeconds(1f);

        if(PhotonNetwork.room.playerCount < 2)
            BattleGround.SpawnEncounter(areaView.Area.BattleEncounter, campfireAmbush);
        else
        {
            if (PhotonNetwork.isMasterClient)
                BattleGround.SpawnMultiplayerEncounter(PhotonNetwork.otherPlayers[0]);
            else
                BattleGround.SpawnMultiplayerEncounter(PhotonNetwork.player);
        }
        foreach (var hero in Formations.monsters.party.Units)
            hero.SetCombatAnimation(true);

        Formations.monsters.ranks.InstantRelocation();

        #region Starting Sound Effects
        var oneShotStart = FMODUnity.RuntimeManager.CreateInstance("event:/general/combat/start");
        if (oneShotStart != null)
        {
            if (BattleGround.SurpriseStatus == SurpriseStatus.Nothing)
                oneShotStart.setParameterValue("start_condition", 0);
            else if (BattleGround.SurpriseStatus == SurpriseStatus.MonstersSurprised)
                oneShotStart.setParameterValue("start_condition", 1);
            else
                oneShotStart.setParameterValue("start_condition", 2);

            oneShotStart.start();
            oneShotStart.release();
        }
        #endregion

        RaidEvents.ShowBattleAnnouncment();
        yield return new WaitForSeconds(1.8f);
        RaidEvents.HideBattleAnnouncment();
        yield return new WaitForSeconds(1f);

        RaidEvents.roundIndicator.Appear();
        yield return StartCoroutine(BattleRound());
        if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
            yield break;
        while (BattleGround.BattleStatus != BattleStatus.Finished)
        {
            RaidEvents.roundIndicator.UpdateRound(BattleGround.NextRound());
            yield return StartCoroutine(BattleRound());
            if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                yield break;
        }

        #region Execute Hero Transformations
        for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
        {
            var hero = Formations.heroes.party.Units[i].Character as Hero;
            if (Formations.heroes.party.Units[i].Character.Mode != null &&
                Formations.heroes.party.Units[i].Character.Mode.AfflictionSkillId != null)
            {
                var battleFinishSkill = hero.SelectedCombatSkills.Find(skill =>
                skill.Id == Formations.heroes.party.Units[i].Character.Mode.BattleCompleteSkillId);
                if (battleFinishSkill != null)
                {
                    SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(Formations.heroes.party.Units[i],
                        Formations.heroes.party.Units[i], battleFinishSkill).
                        UpdateSkillInfo(Formations.heroes.party.Units[i], battleFinishSkill);
                    yield return StartCoroutine(ExecuteHeroSkill(Formations.heroes.party.Units[i], targetInfo, battleFinishSkill));
                }
            }
        }
        #endregion

        BattleGround.ResetTargetRanks();

        #region Stop Soundtrack
        DarkestSoundManager.StopBattleSoundtrack();
        DarkestSoundManager.ContinueDungeonSoundtrack(Raid.Quest.Dungeon);
        #endregion

        #region Check Game Over
        if (Formations.heroes.party.Units.Count == 0)
        {
            StartCoroutine(RaidResultsEvent());
            yield break;
        }
        else
        {
            DarkestSoundManager.ExecuteNarration("victory", NarrationPlace.Raid);
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");
        }
        #endregion

        #region Destroy Remains
        Formations.HideMonsterOverlay();

        if (BattleGround.MonsterParty.Units.Count > 0)
        {
            foreach (var unit in BattleGround.MonsterParty.Units)
            {
                if (unit.Character.IsMonster)
                {
                    Monster monster = unit.Character as Monster;

                    if (monster.Types.Contains(MonsterType.Corpse))
                        unit.SetCorpseKillAnimation(true);
                    else
                        unit.SetDeathAnimation(true);

                    BattleGround.UnitDestroyed(unit);
                    GameObject deathFx = Instantiate(Resources.Load("Prefabs/Effects/" +
                        monster.CommonEffects.DeathEffect) as GameObject);
                    AnimatedEffect effect = deathFx.GetComponent<AnimatedEffect>();
                    effect.BindToTarget(unit, unit.SkeletonAnimations[1], "fxdeath");

                }
            }
            yield return new WaitForSeconds(1f);
        }
        RaidEvents.MonsterTooltip.Hide();
        RaidEvents.roundIndicator.Disappear();
        yield return new WaitForSeconds(0.4f);
        #endregion

        RaidEvents.roundIndicator.End();
        BattleGround.FinishBattle();
        #endregion

        #region Reset Hero Statuses
        foreach (var hero in Formations.heroes.party.Units)
        {
            hero.ResetHalo();
            hero.Character.GetStatusEffect(StatusType.Stun).ResetStatus();
            hero.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
            hero.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
            hero.Character.UpdateDurations(BuffDurationType.Combat);
            hero.OverlaySlot.UpdateOverlay();
        }
        #endregion

        #region Reset Animation and Selection
        RaidPanel.SelectedUnit.SetPerformerStatus();
        foreach (var hero in Formations.heroes.party.Units)
            hero.SetCombatAnimation(false);
        RaidEvents.MonsterTooltip.Hide();
        #endregion

        #region Check Quest Completion
        areaView.Area.BattleEncounter.Cleared = true;

        if (!Raid.QuestCompleted && Raid.CheckQuestGoals())
            yield return StartCoroutine(CompletionCrestEvent());
        #endregion

        #region Remove Combat States and Restrictions
        QuestPanel.SetPeacefulState();
        Formations.UnlockSelections();
        Formations.ShowHeroOverlay();
        RaidPanel.SetPeacefulState();
        EnablePartyMovement();
        EnableEnviroment();
        #endregion

        #region Room Updates
        if (sceneState == DungeonSceneState.Room)
            MapPanel.ShowAvailableRooms(RoomView.raidRoom.Area as DungeonRoom);
        #endregion

        #region Complete Area Info
        if (areaView is RaidHallSector)
            areaView.CompleteArea();
        else
        {
            if (areaView.Area.Type == AreaType.Battle)
                areaView.Area.Knowledge = Knowledge.Completed;

            MapPanel.UpdateArea(areaView.Area);
        }
        #endregion

        QuestPanel.EnableRetreat();
        BattleGround.LeaveBattleGround();
        Inventory.SetPeacefulState(false);
        RaidPanel.heroPanel.equipmentPanel.SetActive();
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
        currentEvent = null;
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

    public override void AbandonButtonClicked()
    {
        StartCoroutine(RaidResultsEvent());
    }
}
