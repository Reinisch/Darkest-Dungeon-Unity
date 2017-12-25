using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RaidSceneMultiplayerManager : RaidSceneManager
{
    [SerializeField]
    private RaidQuestPanel invaderQuestPanel;

    public new static RaidSceneMultiplayerManager Instanse { get; private set; }

    private readonly List<FormationUnit> pvpDialogUnits = new List<FormationUnit>();

    private Buff DeathsDoorSurvivalDebuff { get; set; }

    protected override void Awake()
    {
        if (Instanse == null)
        {
            RaidSceneManager.Instanse = this;
            Instanse = this;

            SaveLoadManager.WriteStartingSave(new SaveCampaignData(4, "MultiplayerTestSave"));
            DarkestDungeonManager.SaveData = SaveLoadManager.ReadSave(4);
            DarkestDungeonManager.Instanse.LoadSave();

            // just testing
            int sessionSeed = 0;
            foreach(var player in PhotonNetwork.playerList)
            {
                RandomSolver.SetRandomSeed(player.ID + player.ToString().GetHashCode());
                sessionSeed += RandomSolver.Next((int)Mathf.Pow(2, 16));
            }
            RandomSolver.SetRandomSeed(sessionSeed);

            RaidEvents.Initialize();
            CurrentRaid = new RaidInfo();

            CurrentRaid.Quest = new PlotQuest()
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
            CurrentRaid.Dungeon = new Dungeon()
            {
                GridSizeX = 1,
                GridSizeY = 1,
                Name = CurrentRaid.Quest.Dungeon,
                DungeonMash = DarkestDungeonManager.Data.DungeonEnviromentData[CurrentRaid.Quest.Dungeon].
                    BattleMashes.Find(mash => mash.MashId == CurrentRaid.Quest.Difficulty),
                SharedMash = DarkestDungeonManager.Data.DungeonEnviromentData["shared"].
                    BattleMashes.Find(mash => mash.MashId == CurrentRaid.Quest.Difficulty),
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
            CurrentRaid.RaidParty = new RaidParty(PhotonNetwork.masterClient);

            DarkestDungeonManager.ScreenFader.StartFaded();
            DarkestDungeonManager.Data.LoadDungeon(CurrentRaid.Quest.Dungeon, CurrentRaid.Quest.Id);
            Rules = new RaidRuleInfo(CurrentRaid.Quest.Dungeon, BattleGround, TorchMeter);

            #if !(UNITY_ANDROID || UNITY_IOS)
            EscapeButton.gameObject.SetActive(false);
#endif
            DeathsDoorSurvivalDebuff = new Buff()
            {
                Id = "",
                AttributeType = AttributeType.DeathBlow,
                DurationAmount = 3,
                DurationType = BuffDurationType.Combat,
                ModifierValue = -0.1f,
                Type = BuffType.StatAdd,
            };
        }
        else
            Destroy(Instanse.gameObject);
    }

    protected override void Start()
    {
        if (Instanse != this)
            return;

        CharacterWindow.EventWindowClosed += CharacterWindowClosed;
        CharacterWindow.EventNextButtonClicked += CharacterWindowNextButtonClicked;
        CharacterWindow.EventPreviousButtonClick += CharacterWindowPreviousButtonClicked;

        RaidInterface.UpdateRaidScene();
        Inventory.SetDeactivated();
        RaidPanel.BannerPanel.SkillPanel.SetMode(SkillPanelMode.Combat);
        RaidPanel.BannerPanel.SetPeacefulState();
        MapPanel.LoadDungeon(CurrentRaid.Dungeon);
        QuestPanel.UpdateQuest(CurrentRaid.Quest, PhotonNetwork.masterClient, PhotonNetwork.isMasterClient);

        if (PhotonNetwork.room.PlayerCount < 2)
            invaderQuestPanel.gameObject.SetActive(false);
        else if (PhotonNetwork.isMasterClient)
            invaderQuestPanel.UpdateQuest(CurrentRaid.Quest, PhotonNetwork.otherPlayers[0]);
        else
            invaderQuestPanel.UpdateQuest(CurrentRaid.Quest, PhotonNetwork.player, true);

        DarkestSoundManager.StartDungeonSoundtrack(CurrentRaid.Dungeon.Name);
        TorchMeter.Initialize(100);
        Formations.Initialize();

        PhotonGameManager.PlayersPreparedCount = 0;

        if (PhotonNetwork.room.PlayerCount < 2)
        {
            Raid.Dungeon.StartingRoom.BattleEncounter.Cleared = true;
            Raid.QuestCompleted = true;
            QuestPanel.CompleteQuest();
        }

        CurrentEvent = RoomLoadingEvent(CurrentRaid.Dungeon.StartingRoom, RoomTransitionType.Entrance);
        StartCoroutine(CurrentEvent);

        PhotonGameManager.BarkMessages.Clear();
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
        invaderQuestPanel.DisableRetreat(false);
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        Formations.LockSelections();
        RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();
        RaidPanel.SetDisabledState();
        #endregion

        #region Switch room scene
        SceneState = DungeonSceneState.Room;

        HallwayView.SetActive(false);
        RoomView.SetActive(true);

        PartyController.TranseferToPassage(RoomView.HallwayPassage);
        Formations.TransferToRoom(RoomView);

        Raid.CurrentLocation = room;
        if (Raid.LastRoom == null)
            Raid.LastRoom = room;
        else if (fromRaidSector != null && transitionType != RoomTransitionType.Teleport)
            Raid.LastRoom = fromRaidSector.HallSector.Hallway.OppositeRoom(room);
        else
            Raid.LastSector = null;

        RoomView.LoadRoom(room, fromRaidSector != null ?
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
        for (int i = 0; i < Formations.Monsters.Overlay.OverlaySlots.Count; i++)
            Formations.Monsters.Overlay.OverlaySlots[i].RectTransform.pivot = new Vector2(0.5f, 0f);
        #endregion

        #region Load combat save
        if (transitionType == RoomTransitionType.CombatLoad)
        {
            CurrentEvent = LoadEncounterEvent(RoomView.RaidRoom);
            StartCoroutine(CurrentEvent);
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
            for (int i = 0; i < Formations.Heroes.Party.Units.Count; i++)
            {
                var hero = Formations.Heroes.Party.Units[i].Character as Hero;
                if (Formations.Heroes.Party.Units[i].Character.Mode != null
                    && Formations.Heroes.Party.Units[i].Character.Mode.AfflictionSkillId != null)
                {
                    var battleFinishSkill = hero.SelectedCombatSkills.Find(skill => skill.Id ==
                        Formations.Heroes.Party.Units[i].Character.Mode.BattleCompleteSkillId);
                    if (battleFinishSkill != null)
                    {
                        SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(Formations.Heroes.Party.Units[i],
                            Formations.Heroes.Party.Units[i], battleFinishSkill).
                            UpdateSkillInfo(Formations.Heroes.Party.Units[i], battleFinishSkill);
                        yield return StartCoroutine(ExecuteHeroSkill(Formations.Heroes.Party.Units[i],
                            targetInfo, battleFinishSkill));
                    }
                }
            }
            #endregion

            foreach (var hero in Formations.Heroes.Party.Units)
                hero.SetCombatAnimation(false);

            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return new WaitForSeconds(0.3f);
            yield return ProcessRaidFailure();
        }
        else if (transitionType == RoomTransitionType.Teleport && room.HasActiveBattle)
        {
            CurrentEvent = EncounterEvent(RoomView.RaidRoom);
            StartCoroutine(CurrentEvent);
            yield break;
        }
        else
            yield return new WaitForSeconds(0.5f);
        #endregion

        if (PhotonNetwork.room.PlayerCount < 2)
            RaidEvents.ShowAnnouncment("Waiting for opponent to join...");

        RaidPanel.SwitchBlocked = false;
        if (fromRaidSector == null)
        {
            Formations.ShowHeroOverlay();
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
        invaderQuestPanel.EnableRetreat();
        MapPanel.ShowAvailableRooms(room);
        Inventory.SetPeacefulState(false);
        RaidPanel.HeroPanel.EquipmentPanel.SetActive();
        RaidPanel.SetDisabledState();
        CurrentEvent = null;
        #endregion
    }

    protected override IEnumerator EncounterEvent(IRaidArea areaView, bool campfireAmbush = false)
    {
        #region Set Combat States and Restrictions

        QuestPanel.UpdateEncounterRetreat();
        QuestPanel.SetCombatState();
        invaderQuestPanel.UpdateEncounterRetreat();
        invaderQuestPanel.SetCombatState();
        DisableEnviroment();
        DisablePartyMovement();
        Formations.LockSelections();
        Formations.ResetSelections();
        RaidPanel.SetDisabledState();
        Inventory.SetDeactivated();
        RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();

        foreach (var hero in Formations.Heroes.Party.Units)
            hero.SetCombatAnimation(true);

        #endregion

        #region Wait For Events
        while (IsUnitEventInProgress)
            yield return null;
        #endregion

        #region Switch Soundtrack
        DarkestSoundManager.PauseDungeonSoundtrack();
        DarkestSoundManager.StartBattleSoundtrack("town", SceneState == DungeonSceneState.Room);

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

        if(PhotonNetwork.room.PlayerCount < 2)
            BattleGround.SpawnEncounter(areaView.Area.BattleEncounter, campfireAmbush);
        else
            BattleGround.SpawnMultiplayerEncounter(PhotonNetwork.isMasterClient ?
                PhotonNetwork.otherPlayers[0] :
                PhotonNetwork.player);

        foreach (var hero in Formations.Monsters.Party.Units)
            hero.SetCombatAnimation(true);

        Formations.Monsters.Ranks.InstantRelocation();

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

        yield return StartCoroutine(PhotonGameManager.PreparationCheck());

        var stunResForEveryone = new List<FormationUnit>(HeroParty.Units.Concat(BattleGround.MonsterParty.Units));
        var stunResBuff = new Buff()
        {
            AttributeType = AttributeType.Stun,
            DurationAmount = 420,
            DurationType = BuffDurationType.Combat,
            Id = "",
            IsFalseRule = false,
            ModifierValue = 0.4f,
            RuleType = BuffRule.Always,
            Type = BuffType.StatAdd,
        };

        stunResForEveryone = stunResForEveryone.OrderBy(unit => unit.Team == Team.Heroes ? -unit.Rank : unit.Rank).ToList();

        foreach(var unit in stunResForEveryone)
        {
            unit.Character.AddBuff(new BuffInfo(stunResBuff, BuffSourceType.Adventure));
            RaidEvents.ShowPopupMessage(unit, PopupMessageType.Buff);
            unit.OverlaySlot.UpdateOverlay();
            yield return new WaitForSeconds(0.4f);
        }

        yield return StartCoroutine(PhotonGameManager.PreparationCheck());

        RaidEvents.RoundIndicator.Appear();
        yield return StartCoroutine(BattleRound());
        if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
            yield break;
        while (BattleGround.BattleStatus != BattleStatus.Finished)
        {
            RaidEvents.RoundIndicator.UpdateRound(BattleGround.NextRound());
            yield return StartCoroutine(BattleRound());
            if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                yield break;
        }

        #region Execute Hero Transformations
        for (int i = 0; i < Formations.Heroes.Party.Units.Count; i++)
        {
            var hero = Formations.Heroes.Party.Units[i].Character as Hero;
            if (Formations.Heroes.Party.Units[i].Character.Mode != null &&
                Formations.Heroes.Party.Units[i].Character.Mode.AfflictionSkillId != null)
            {
                var battleFinishSkill = hero.SelectedCombatSkills.Find(skill =>
                skill.Id == Formations.Heroes.Party.Units[i].Character.Mode.BattleCompleteSkillId);
                if (battleFinishSkill != null)
                {
                    SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(Formations.Heroes.Party.Units[i],
                        Formations.Heroes.Party.Units[i], battleFinishSkill).
                        UpdateSkillInfo(Formations.Heroes.Party.Units[i], battleFinishSkill);
                    yield return StartCoroutine(ExecuteHeroSkill(Formations.Heroes.Party.Units[i], targetInfo, battleFinishSkill));
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

        yield return new WaitForSeconds(0.5f);
       
        if (HeroParty.Units.Count == 0)
        {
            if (PhotonNetwork.isMasterClient)
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.otherPlayers[0].NickName + " is victorious!");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/retreat");
            }
            else
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.player.NickName + " is victorious!");
                DarkestSoundManager.ExecuteNarration("victory", NarrationPlace.Raid);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");
            }
        }
        else
        {
            if (PhotonNetwork.isMasterClient)
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.player.NickName + " is victorious!");
                DarkestSoundManager.ExecuteNarration("victory", NarrationPlace.Raid);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");
            }
            else
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.masterClient.NickName + " is victorious!");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/retreat");
            }
        }
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(RaidResultsEvent());

        #endregion

        #endregion
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
            if (!CurrentRaid.Quest.IsPlotQuest)
                DarkestSoundManager.ExecuteNarration("quest_end_completed", NarrationPlace.Raid,
                    CurrentRaid.Quest.Type, CurrentRaid.Quest.Dungeon);
        }
        else
        {
            DarkestSoundManager.ExecuteNarration("quest_end_not_completed", NarrationPlace.Raid,
                CurrentRaid.Quest.Type, CurrentRaid.Quest.Dungeon);
        }

        DarkestDungeonManager.ScreenFader.Fade();
        yield return new WaitForSeconds(1f);
        PhotonGameManager.Instanse.LeaveRoom();
    }

    protected override IEnumerator CompletionCrestEvent()
    {
        Raid.QuestCompleted = true;
        CompletionWindow.Appear();
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/quest_goal_complete");
        DungeonCamera.SwitchBlur(true);
        while (CompletionWindow.Action == CompletionAction.Waiting)
            yield return null;

        if (CompletionWindow.Action == CompletionAction.Return)
        {
            yield return StartCoroutine(RaidResultsEvent());
            yield break;
        }

        QuestPanel.CompleteQuest();
        DungeonCamera.SwitchBlur(false);
        yield return new WaitForSeconds(1f);
    }

    protected override IEnumerator BattleRound(bool fromBattleSave = false)
    {
        if (fromBattleSave == false)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/round");
            yield return new WaitForSeconds(1f);

            #region LifeTime Activations
            TempList.Clear();
            for (int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
                if (BattleGround.MonsterParty.Units[i].Character.LifeTime != null)
                    TempList.Add(BattleGround.MonsterParty.Units[i]);

            if (TempList.Count > 0)
            {
                bool someoneExpired = false;
                for (int i = 0; i < TempList.Count; i++)
                {
                    if (TempList[i].Character.LifeTime.AliveRoundLimit <= TempList[i].CombatInfo.RoundsAlive)
                    {
                        PrepareDeath(TempList[i]);
                        someoneExpired = true;
                    }
                }

                if (someoneExpired)
                {
                    yield return new WaitForSeconds(1.2f);
                    for (int i = 0; i < TempList.Count; i++)
                    {
                        if (TempList[i].CombatInfo.IsDead)
                            ExecuteDeath(TempList[i]);
                    }
                }
                TempList.Clear();
            }
            #endregion

            #region Control Activations
            for (int i = BattleGround.Controls.Count - 1; i >= 0; i--)
            {
                if (--BattleGround.Controls[i].DurationLeft < 0 || BattleGround.IsBattleOnesided())
                {
                    var uncontrolledUnit = BattleGround.Controls[i].PrisonerUnit;
                    BattleGround.UncontrolUnit(BattleGround.Controls[i]);
                    yield return new WaitForSeconds(0.175f);

                    uncontrolledUnit.OverlaySlot.StartDialog(LocalizationManager.GetString("str_control_return_siren"));
                    while (uncontrolledUnit.OverlaySlot.IsDoingDialog)
                        yield return null;

                    continue;
                }

                #region Stress Dealing
                if (BattleGround.Controls[i].ControllComponent.StressPerTurn != 0)
                {
                    float initialDamage = BattleGround.Controls[i].ControllComponent.StressPerTurn;

                    int damage = Mathf.RoundToInt(initialDamage * (1 +
                            BattleGround.Controls[i].PrisonerUnit.Character[AttributeType.StressDmgReceivedPercent].ModifiedValue));
                    if (damage < 1) damage = 1;

                    BattleGround.Controls[i].PrisonerUnit.Character.Stress.IncreaseValue(damage);
                    if (BattleGround.Controls[i].PrisonerUnit.Character.IsOverstressed)
                    {
                        if (BattleGround.Controls[i].PrisonerUnit.Character.IsVirtued)
                            BattleGround.Controls[i].PrisonerUnit.Character.Stress.CurrentValue =
                                Mathf.Clamp(BattleGround.Controls[i].PrisonerUnit.Character.Stress.CurrentValue, 0, 100);
                        else if (!BattleGround.Controls[i].PrisonerUnit.Character.IsAfflicted &&
                            BattleGround.Controls[i].PrisonerUnit.Character.IsOverstressed)
                            AddResolveCheck(BattleGround.Controls[i].PrisonerUnit);

                        if (BattleGround.Controls[i].PrisonerUnit.Character.Stress.CurrentValue == 200)
                            RaidSceneManager.Instanse.AddHeartAttackCheck(BattleGround.Controls[i].PrisonerUnit);
                    }
                    BattleGround.Controls[i].PrisonerUnit.OverlaySlot.UpdateOverlay();

                    RaidEvents.ShowPopupMessage(BattleGround.Controls[i].PrisonerUnit, PopupMessageType.Stress, damage.ToString());
                    BattleGround.Controls[i].PrisonerUnit.SetHalo("afflicted");

                    yield return new WaitForSeconds(1.2f);

                    yield return StartCoroutine(ExecuteEffectEvents(true));
                }
                #endregion
            }
            #endregion

            #region Round Start Desires
            TempList.AddRange(BattleGround.MonsterParty.Units);
            while (TempList.Count > 0)
            {
                var monsterUnit = TempList[0];
                TempList.RemoveAt(0);
                if (monsterUnit.Character is Hero)
                    continue;
                var monster = monsterUnit.Character as Monster;

                var desires = monster.Brain.BonusDesireSet.FindAll(desire => desire.IsRoundStart);
                while (desires.Count > 0)
                {
                    var currentDesire = desires[0];
                    desires.RemoveAt(0);

                    if (currentDesire.CheckBonusInitiative(monsterUnit))
                    {
                        yield return StartCoroutine(MonsterTurn(monsterUnit, currentDesire.CombatSkillOverride));
                        break;
                    }
                }
            }
            TempList.Clear();
            #endregion

            #region Mutation Activation
            if (BattleGround.Round.RoundNumber != 1)
            {
                for (int k = 0; k < 1; k++)
                {
                    if (k > 0)
                        yield return new WaitForSeconds(0.2f);

                    TempList.AddRange(BattleGround.MonsterParty.Units.FindAll(unit => unit.Character.IsMonster
                        && (unit.Character as Monster).Data.Shapeshifter != null));
                    if (TempList.Count > 0)
                    {
                        Formations.HideUnitOverlay();
                        yield return new WaitForSeconds(0.2f);
                        DungeonCamera.Zoom(50, 0.05f);
                        yield return new WaitForSeconds(0.05f);
                        DungeonCamera.SwitchBlur(true);
                        foreach (var targetUnit in TempList)
                            Formations.UnitBuffedIntro(targetUnit);
                        yield return new WaitForSeconds(0.05f);
                        List<string> mutations = new List<string>();
                        List<MonsterData> mutationData = new List<MonsterData>();
                        #region Transformation
                        foreach (var targetUnit in TempList)
                        {
                            var monster = targetUnit.Character as Monster;
                            List<int> summonPool = new List<int>();
                            List<int> chancePool = new List<int>(monster.Data.Shapeshifter.MonsterClassChances);
                            for (int i = 0; i < monster.Data.Shapeshifter.MonsterClassIds.Count; i++)
                                summonPool.Add(i);

                            while (summonPool.Count != 0)
                            {
                                if (summonPool.Count == 0)
                                    break;

                                int rolledIndex = RandomSolver.ChooseRandomIndex(chancePool);
                                int summonIndex = summonPool[rolledIndex];

                                if (!monster.Data.Shapeshifter.MonsterClassValidRanks[summonIndex].
                                    IsLaunchableFrom(targetUnit.Rank, targetUnit.Size))
                                {
                                    summonPool.RemoveAt(rolledIndex);
                                    chancePool.RemoveAt(rolledIndex);
                                    continue;
                                }
                                var data = DarkestDungeonManager.Data.Monsters[monster.Data.Shapeshifter.MonsterClassIds[summonIndex]];
                                mutations.Add(data.TypeId);
                                mutationData.Add(data);
                                break;
                            }
                        }
                        #endregion
                        bool mutated = false;
                        for (int i = 0; i < TempList.Count; i++)
                        {
                            if (TempList[i].Character.Class != mutationData[i].TypeId)
                            {
                                TempList[i].SetTargetEffect(TempList[i], "formless_mutate", "root",
                                    TempList[i].Character.Class + "_to_" + mutations[i]);
                                mutated = true;
                            }
                        }
                        if (mutated)
                            FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/_shared/formless_shared_mutate");

                        yield return new WaitForSeconds(0.01f);
                        Formations.PartyBuffPositions.SetUnitTargets(TempList, 0.05f, Vector2.zero);
                        yield return new WaitForSeconds(0.05f);
                        Formations.PartyBuffPositions.SetSpacing(120, 1f);
                        for (int i = 0; i < TempList.Count; i++)
                            if (TempList[i].Character.Class != mutationData[i].TypeId)
                                TempList[i].CurrentState.MeshRenderer.enabled = false;
                        yield return new WaitForSeconds(1.2f);
                        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
                        DungeonCamera.SwitchBlur(false);
                        foreach (var targetUnit in TempList)
                            Formations.UnitBuffedOutro(targetUnit);
                        #region Transformation
                        for (int i = 0; i < TempList.Count; i++)
                        {
                            if (TempList[i].Character.Class != mutationData[i].TypeId)
                            {
                                GameObject summonObject = Resources.Load("Prefabs/Monsters/" + mutationData[i].TypeId) as GameObject;
                                BattleGround.ReplaceUnit(mutationData[i], TempList[i], summonObject, true);
                            }
                        }
                        #endregion
                        yield return new WaitForSeconds(0.175f);
                        Formations.ShowUnitOverlay();
                        Formations.ResetSelections();
                    }
                    TempList.Clear();
                }
            }
            #endregion
        }

        while (BattleGround.Round.OrderedUnits.Count != 0)
        {
            #region Captor Activations
            for (int i = BattleGround.Captures.Count - 1; i >= 0; i--)
            {
                #region Damage Dealing
                if (BattleGround.Captures[i].Component.PerTurnDamagePercent != 0)
                {
                    var captorMonster = BattleGround.Captures[i].CaptorUnit.Character as Monster;
                    int healthDamage = Mathf.RoundToInt(BattleGround.Captures[i].PrisonerUnit.Character.Health.ModifiedValue
                        * BattleGround.Captures[i].Component.PerTurnDamagePercent);
                    BattleGround.Captures[i].PrisonerUnit.Character.Health.DecreaseValue(healthDamage);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + captorMonster.Data.TypeId + "_captor_full_action");

                    if (Mathf.RoundToInt(BattleGround.Captures[i].PrisonerUnit.Character.Health.CurrentValue) != 0)
                    {
                        RaidEvents.ShowPopupMessage(BattleGround.Captures[i].CaptorUnit, PopupMessageType.Damage, healthDamage.ToString());
                        yield return new WaitForSeconds(0.4f);
                        if (RandomSolver.CheckSuccess(0.33f))
                        {
                            BattleGround.Captures[i].CaptorUnit.OverlaySlot.StartDialog(
                                LocalizationManager.GetString("str_prisoner_damage_cauldron_full"));
                            while (BattleGround.Captures[i].CaptorUnit.OverlaySlot.IsDoingDialog)
                                yield return null;
                        }
                    }
                    else
                    {
                        if (PrepareDeath(BattleGround.Captures[i].PrisonerUnit))
                        {
                            RaidEvents.ShowPopupMessage(BattleGround.Captures[i].PrisonerUnit, PopupMessageType.DeathBlow);
                            yield return new WaitForSeconds(1.4f);
                            BattleGround.Round.PostHeroTurn();
                            ExecuteDeath(BattleGround.Captures[i].PrisonerUnit);
                            yield break;
                        }
                        else
                        {
                            if (BattleGround.Captures[i].Component.ReleasePrisonerAtDeathDoor)
                            {
                                RaidEvents.ShowPopupMessage(BattleGround.Captures[i].CaptorUnit,
                                    PopupMessageType.Damage, healthDamage.ToString());
                                yield return new WaitForSeconds(0.4f);
                                Formations.HideUnitOverlay();
                                yield return new WaitForSeconds(0.2f);
                                DungeonCamera.Zoom(50, 0.05f);
                                FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + captorMonster.Data.TypeId + "_vo_death");
                                yield return new WaitForSeconds(0.05f);
                                DungeonCamera.SwitchBlur(true);
                                Formations.UnitSkillIntro(BattleGround.Captures[i].CaptorUnit, "release");
                                yield return new WaitForSeconds(0.05f);
                                Formations.PartyBuffPositions.SetUnitTargets(BattleGround.Captures[i].CaptorUnit, 0.05f, Vector2.zero);
                                yield return new WaitForSeconds(1.2f);
                                DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
                                DungeonCamera.SwitchBlur(false);
                                Formations.UnitSkillOutro(BattleGround.Captures[i].CaptorUnit, "release");
                                var captureRelease = BattleGround.Captures[i];
                                BattleGround.ReleaseUnit(captureRelease);
                                MonsterData emptyCaptorData = DarkestDungeonManager.Data.Monsters[(captureRelease.CaptorUnit.
                                    Character as Monster).Data.FullCaptor.EmptyMonsterClass];
                                GameObject unitObject = Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                                BattleGround.ReplaceUnit(emptyCaptorData, captureRelease.CaptorUnit, unitObject);
                                yield return new WaitForSeconds(0.175f);
                                Formations.ShowUnitOverlay();
                                Formations.ResetSelections();
                                yield return new WaitForSeconds(0.075f);
                                yield return StartCoroutine(ExecuteEffectEvents(true));
                            }
                        }
                    }
                }
                #endregion
            }
            for (int i = BattleGround.Captures.Count - 1; i >= 0; i--)
            {
                #region Stress Dealing
                if (BattleGround.Captures[i].Component.PerTurnStress != 0)
                {
                    if (RandomSolver.CheckSuccess(0.33f))
                    {
                        BattleGround.Captures[i].PrisonerUnit.OverlaySlot.StartDialog(
                            LocalizationManager.GetString("str_prisoner_damage_drowned_anchored"));
                        while (BattleGround.Captures[i].PrisonerUnit.OverlaySlot.IsDoingDialog)
                            yield return null;
                    }

                    float initialDamage = BattleGround.Captures[i].Component.PerTurnStress;

                    int damage = Mathf.RoundToInt(initialDamage * (1 +
                            BattleGround.Captures[i].PrisonerUnit.Character[AttributeType.StressDmgReceivedPercent].ModifiedValue));
                    if (damage < 1) damage = 1;

                    BattleGround.Captures[i].PrisonerUnit.Character.Stress.IncreaseValue(damage);
                    if (BattleGround.Captures[i].PrisonerUnit.Character.IsOverstressed)
                    {
                        if (BattleGround.Captures[i].PrisonerUnit.Character.IsVirtued)
                            BattleGround.Captures[i].PrisonerUnit.Character.Stress.CurrentValue =
                                Mathf.Clamp(BattleGround.Captures[i].PrisonerUnit.Character.Stress.CurrentValue, 0, 100);
                        else if (!BattleGround.Captures[i].PrisonerUnit.Character.IsAfflicted &&
                            BattleGround.Captures[i].PrisonerUnit.Character.IsOverstressed)
                            AddResolveCheck(BattleGround.Captures[i].PrisonerUnit);

                        if (BattleGround.Captures[i].PrisonerUnit.Character.Stress.CurrentValue == 200)
                            AddHeartAttackCheck(BattleGround.Captures[i].PrisonerUnit);
                    }
                    BattleGround.Captures[i].PrisonerUnit.OverlaySlot.UpdateOverlay();

                    RaidEvents.ShowPopupMessage(BattleGround.Captures[i].PrisonerUnit,
                        PopupMessageType.Stress, damage.ToString());
                    BattleGround.Captures[i].PrisonerUnit.SetHalo("afflicted");

                    yield return new WaitForSeconds(1.2f);
                    if (BattleGround.Captures[i].PrisonerUnit.Character.ReadyForAfflictionCheck
                        && BattleGround.Captures[i].Component.ReleaseOnPrisonerAffliction)
                    {
                        var captureRelease = BattleGround.Captures[i];
                        BattleGround.ReleaseUnit(BattleGround.Captures[i]);
                        captureRelease.CaptorUnit.SetReleaseAnimation(true);
                        yield return new WaitForSeconds(0.667f);

                        MonsterData emptyCaptorData = DarkestDungeonManager.Data.Monsters[captureRelease.Component.EmptyMonsterClass];
                        GameObject unitObject = Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                        BattleGround.ReplaceUnit(emptyCaptorData, captureRelease.CaptorUnit, unitObject);
                    }

                    yield return new WaitForSeconds(0.075f);
                    yield return StartCoroutine(ExecuteEffectEvents(true));
                }
                #endregion
            }
            #endregion

            #region Companion Activations
            for (int i = BattleGround.Companions.Count - 1; i >= 0; i--)
            {
                #region Healing
                if (!Mathf.Approximately(BattleGround.Companions[i].CompanionComponent.HealPerTurn, 0))
                {
                    int health = Mathf.RoundToInt(BattleGround.Companions[i].TargetUnit.Character.Health.ModifiedValue
                        * BattleGround.Companions[i].CompanionComponent.HealPerTurn);
                    BattleGround.Companions[i].TargetUnit.Character.Health.IncreaseValue(health);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_enemy");
                    BattleGround.Companions[i].TargetUnit.OverlaySlot.UpdateOverlay();
                    RaidEvents.ShowPopupMessage(BattleGround.Companions[i].TargetUnit, PopupMessageType.Heal, health.ToString());
                    yield return new WaitForSeconds(0.4f);
                }
                #endregion
            }
            #endregion

            #region Life Link Activations
            for (int i = BattleGround.MonsterParty.Units.Count - 1; i >= 0; i--)
            {
                if (BattleGround.MonsterParty.Units[i].Character.IsMonster)
                {
                    var monster = BattleGround.MonsterParty.Units[i].Character as Monster;
                    if (monster.Data.LifeLink != null &&
                        !BattleGround.IsLifeLinked(BattleGround.MonsterParty.Units[i], monster.Data.LifeLink))
                    {
                        PrepareDeath(BattleGround.MonsterParty.Units[i]);
                        yield return new WaitForSeconds(1.2f);
                        ExecuteDeath(BattleGround.MonsterParty.Units[i]);
                        yield return new WaitForSeconds(0.3f);
                    }
                }
            }
            if (BattleGround.IsBattleEnded())
                break;
            #endregion

            #region Next Unit Turn Action
            FormationUnit unit = BattleGround.Round.OrderedUnits[0];
            if (unit.Character.IsMonster == false)
                yield return StartCoroutine(HeroTurn(unit));
            else
                yield return StartCoroutine(MonsterTurn(unit));

            if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                yield break;
            #endregion

            if (BattleGround.IsBattleEnded())
                break;

            #region Turn End Desires
            TempList.AddRange(BattleGround.MonsterParty.Units);
            while (TempList.Count > 0)
            {
                var monsterUnit = TempList[0];
                TempList.RemoveAt(0);
                if (monsterUnit.Character is Hero)
                    continue;

                var monster = monsterUnit.Character as Monster;
                var desires = monster.Brain.BonusDesireSet.FindAll(desire => desire.IsPostTurn && desire.IsRoundInProgress);
                while (desires.Count > 0)
                {
                    var currentDesire = desires[0];
                    desires.RemoveAt(0);

                    if (currentDesire.CheckBonusInitiative(monsterUnit))
                    {
                        yield return WaitForOneTwo;
                        yield return StartCoroutine(MonsterTurn(monsterUnit, currentDesire.CombatSkillOverride));
                        break;
                    }
                }
            }
            TempList.Clear();
            #endregion

            if (BattleGround.IsBattleEnded())
                break;

            yield return WaitForZeroThree;
        }

        #region Round Finish Desires
        TempList.AddRange(BattleGround.MonsterParty.Units);
        while (TempList.Count > 0)
        {
            var monsterUnit = TempList[0];
            TempList.RemoveAt(0);
            if (monsterUnit.Character is Hero)
                continue;

            var monster = monsterUnit.Character as Monster;
            var desires = monster.Brain.BonusDesireSet.FindAll(desire => desire.IsRoundFinish);
            while (desires.Count > 0)
            {
                var currentDesire = desires[0];
                desires.RemoveAt(0);

                if (currentDesire.CheckBonusInitiative(monsterUnit))
                {
                    yield return StartCoroutine(MonsterTurn(monsterUnit, currentDesire.CombatSkillOverride));
                    break;
                }
            }
        }
        TempList.Clear();
        #endregion

        #region Idle units status effects
        TempList.AddRange(BattleGround.MonsterParty.Units.FindAll(targetUnit => targetUnit.CombatInfo.TotalInitiatives == 0));
        bool hasIdleDamage = false, hasIdleDeath = false;
        foreach (var idleUnit in TempList)
        {
            #region Status Effect and Buffs
            if (idleUnit.Character.GetStatusEffect(StatusType.Bleeding).IsApplied)
            {
                var bleedEffect = idleUnit.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect;
                int damage = Mathf.CeilToInt(bleedEffect.CurrentTickDamage * 1.5f);
                idleUnit.Character.Health.DecreaseValue(damage);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
                idleUnit.OverlaySlot.UpdateOverlay();

                #region Damage Activation
                if (Mathf.RoundToInt(idleUnit.Character.Health.CurrentValue) != 0)
                {
                    RaidEvents.ShowPopupMessage(idleUnit, PopupMessageType.Damage, damage.ToString());
                    hasIdleDamage = true;
                }
                else
                {
                    PrepareDeath(idleUnit);
                    hasIdleDeath = true;
                }
                #endregion
            }

            if (idleUnit.Character.GetStatusEffect(StatusType.Poison).IsApplied)
            {
                var poisonEffect = idleUnit.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect;
                int damage = Mathf.CeilToInt(poisonEffect.CurrentTickDamage * 1.5f);
                RaidEvents.ShowPopupMessage(idleUnit, PopupMessageType.Damage, damage.ToString());
                idleUnit.Character.Health.DecreaseValue(damage);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/poison_dot");
                idleUnit.OverlaySlot.UpdateOverlay();

                #region Damage Activation
                if (Mathf.RoundToInt(idleUnit.Character.Health.CurrentValue) != 0)
                {
                    RaidEvents.ShowPopupMessage(idleUnit, PopupMessageType.Damage, damage.ToString());
                    hasIdleDamage = true;
                }
                else
                {
                    PrepareDeath(idleUnit);
                    hasIdleDeath = true;
                }
                #endregion
            }

            if (idleUnit.CombatInfo.IsSurprised)
                idleUnit.SetSurprised(false);

            if (idleUnit.Character.GetStatusEffect(StatusType.Stun).IsApplied)
            {
                var stunStatus = idleUnit.Character.GetStatusEffect(StatusType.Stun) as StunStatusEffect;
                stunStatus.StunApplied = false;
                idleUnit.ResetHalo();
                idleUnit.Character.ApplyStunRecovery();
                idleUnit.Character.UpdateRound();
                idleUnit.OverlaySlot.UpdateOverlay();
            }
            else
            {
                idleUnit.Character.UpdateRound();
                idleUnit.OverlaySlot.UpdateOverlay();
            }
            #endregion
        }
        if (hasIdleDeath)
        {
            yield return new WaitForSeconds(1.4f);
            for (int i = 0; i < TempList.Count; i++)
                ExecuteDeath(TempList[i]);
            yield return new WaitForSeconds(0.2f);
        }
        else if (hasIdleDamage)
        {
            yield return new WaitForSeconds(0.3f);
        }
        TempList.Clear();
        #endregion
    }

    protected override IEnumerator HeroTurn(FormationUnit actionUnit, bool fromBattleSave = false)
    {
        yield return StartCoroutine(PhotonGameManager.PreparationCheck());

        FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/ally_turn");
        RaidPanel.SetDisabledState();
        Formations.ResetSelections();
        yield return new WaitForEndOfFrame();
        BattleGround.Round.PreHeroTurn(actionUnit);
        yield return new WaitForEndOfFrame();

        actionUnit.OverlaySlot.UnitSelected();

        if (actionUnit.Team == Team.Heroes)
            RaidEvents.ShowAnnouncment("Next turn: " + PhotonNetwork.masterClient.NickName);
        else if (PhotonNetwork.isMasterClient && PhotonNetwork.otherPlayers.Length > 0)
            RaidEvents.ShowAnnouncment("Next turn: " + PhotonNetwork.otherPlayers[0].NickName);
        else if (!PhotonNetwork.isMasterClient)
            RaidEvents.ShowAnnouncment("Next turn: " + PhotonNetwork.player.NickName);

        yield return new WaitForSeconds(1.5f);
        RaidEvents.HideAnnouncment();

        #region Status Effect and Buffs
        if (actionUnit.Character.GetStatusEffect(StatusType.Bleeding).IsApplied)
        {
            var bleedEffect = actionUnit.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect;
            actionUnit.Character.Health.DecreaseValue(bleedEffect.CurrentTickDamage);
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
            actionUnit.OverlaySlot.UpdateOverlay();

            #region Damage Activation
            if (Mathf.RoundToInt(actionUnit.Character.Health.CurrentValue) != 0)
            {
                RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Damage, bleedEffect.CurrentTickDamage.ToString());
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                if (actionUnit.Character.AtDeathsDoor)
                {
                    if (PrepareDeath(actionUnit, DeathFactor.BleedMonster))
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathBlow);
                        yield return new WaitForSeconds(1.4f);
                        BattleGround.Round.PostHeroTurn();
                        ExecuteDeath(actionUnit);
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                        yield break;
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathsDoor);
                        yield return new WaitForSeconds(0.6f);
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                    }
                }
                else
                {
                    if (PrepareDeath(actionUnit, DeathFactor.BleedMonster))
                    {
                        DeathDamage deathDamage = actionUnit.Character.DeathDamage;
                        if (actionUnit.Character is Hero)
                            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathBlow);
                        yield return new WaitForSeconds(1.4f);
                        BattleGround.Round.PostMonsterTurn();
                        ExecuteDeath(actionUnit);

                        if (deathDamage != null)
                        {
                            var deathDamageTarget = BattleGround.MonsterParty.Units.Find(unit =>
                                unit.Character.Class == deathDamage.TargetBaseClass);

                            if (deathDamageTarget != null)
                            {
                                deathDamageTarget.Character.Health.DecreaseValue(deathDamage.TargetDamage);
                                if (Mathf.RoundToInt(deathDamageTarget.Character.Health.CurrentValue) != 0)
                                {
                                    RaidEvents.ShowPopupMessage(deathDamageTarget,
                                        PopupMessageType.Damage, deathDamage.TargetDamage.ToString());
                                    yield return new WaitForSeconds(0.4f);
                                }
                            }
                        }
                        yield return StartCoroutine(ExecuteEffectEvents(true));

                        yield break;
                    }
                    else
                    {
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                    }
                }
            }
            #endregion
        }

        if (actionUnit.Character.GetStatusEffect(StatusType.Poison).IsApplied)
        {
            var poisonEffect = actionUnit.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect;
            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Damage, poisonEffect.CurrentTickDamage.ToString());
            actionUnit.Character.Health.DecreaseValue(poisonEffect.CurrentTickDamage);
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/poison_dot");
            actionUnit.OverlaySlot.UpdateOverlay();

            #region Damage Activation
            if (Mathf.RoundToInt(actionUnit.Character.Health.CurrentValue) != 0)
            {
                RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Damage, poisonEffect.CurrentTickDamage.ToString());
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                if (actionUnit.Character.AtDeathsDoor)
                {
                    if (PrepareDeath(actionUnit, DeathFactor.PoisonMonster))
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathBlow);
                        yield return new WaitForSeconds(1.4f);
                        BattleGround.Round.PostHeroTurn();
                        ExecuteDeath(actionUnit);
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                        yield break;
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathsDoor);
                        yield return new WaitForSeconds(0.6f);
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                    }
                }
                else
                {
                    if (PrepareDeath(actionUnit, DeathFactor.PoisonMonster))
                    {
                        DeathDamage deathDamage = actionUnit.Character.DeathDamage;
                        if (actionUnit.Character is Hero)
                            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathBlow);
                        yield return new WaitForSeconds(1.4f);
                        BattleGround.Round.PostMonsterTurn();
                        ExecuteDeath(actionUnit);

                        if (deathDamage != null)
                        {
                            var deathDamageTarget = BattleGround.MonsterParty.Units.Find(unit =>
                            unit.Character.Class == deathDamage.TargetBaseClass);

                            if (deathDamageTarget != null)
                            {
                                deathDamageTarget.Character.Health.DecreaseValue(deathDamage.TargetDamage);
                                if (Mathf.RoundToInt(deathDamageTarget.Character.Health.CurrentValue) != 0)
                                {
                                    RaidEvents.ShowPopupMessage(deathDamageTarget,
                                        PopupMessageType.Damage, deathDamage.TargetDamage.ToString());
                                    yield return new WaitForSeconds(0.4f);
                                }
                            }
                        }
                        yield return StartCoroutine(ExecuteEffectEvents(true));

                        yield break;
                    }
                    else
                    {
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                    }
                }
            }
            #endregion
        }

        if (actionUnit.CombatInfo.IsSurprised)
            actionUnit.SetSurprised(false);

        if (actionUnit.Character.GetStatusEffect(StatusType.Stun).IsApplied)
        {
            var stunStatus = actionUnit.Character.GetStatusEffect(StatusType.Stun) as StunStatusEffect;
            stunStatus.StunApplied = false;
            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Unstun);
            actionUnit.ResetHalo();

            yield return new WaitForSeconds(0.9f);

            actionUnit.Character.ApplyStunRecovery();
            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Buff);
            actionUnit.Character.UpdateRound();
            actionUnit.OverlaySlot.UpdateOverlay();
            BattleGround.Round.PostHeroTurn();

            yield return new WaitForSeconds(0.6f);
            yield break;
        }
        else
        {
            actionUnit.Character.UpdateRound();
            actionUnit.OverlaySlot.UpdateOverlay();
        }
        #endregion

        #region Mode Stress
        if (actionUnit.Character.Mode != null && actionUnit.Character.Mode.StressPerTurn != 0)
        {
            float initialDamage = actionUnit.Character.Mode.StressPerTurn;

            int damage = Mathf.RoundToInt(initialDamage * 2.5f * (1 +
                    actionUnit.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
            if (damage < 1) damage = 1;

            actionUnit.Character.Stress.IncreaseValue(damage);
            if (actionUnit.Character.IsOverstressed)
            {
                if (actionUnit.Character.IsVirtued)
                    actionUnit.Character.Stress.CurrentValue = Mathf.Clamp(actionUnit.Character.Stress.CurrentValue, 0, 100);
                else if (!actionUnit.Character.IsAfflicted && actionUnit.Character.IsOverstressed)
                    RaidSceneManager.Instanse.AddResolveCheck(actionUnit);

                if (Mathf.RoundToInt(actionUnit.Character.Stress.CurrentValue) == 200)
                    RaidSceneManager.Instanse.AddHeartAttackCheck(actionUnit);
            }
            actionUnit.OverlaySlot.UpdateOverlay();

            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Stress, damage.ToString());
            actionUnit.SetHalo("afflicted");

            yield return new WaitForSeconds(1.2f);

            var captureRecord = BattleGround.Captures.Find(capture => capture.PrisonerUnit == actionUnit);
            if (captureRecord != null)
            {
                if (!(captureRecord.PrisonerUnit.Character.IsVirtued || captureRecord.PrisonerUnit.Character.IsAfflicted)
                && captureRecord.PrisonerUnit.Character.IsOverstressed && captureRecord.Component.ReleaseOnPrisonerAffliction)
                {
                    BattleGround.ReleaseUnit(captureRecord);
                    captureRecord.CaptorUnit.SetReleaseAnimation(true);
                    yield return new WaitForSeconds(0.667f);

                    MonsterData emptyCaptorData = DarkestDungeonManager.Data.Monsters[(captureRecord.CaptorUnit.
                                Character as Monster).Data.FullCaptor.EmptyMonsterClass];
                    GameObject unitObject = Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                    BattleGround.ReplaceUnit(emptyCaptorData, captureRecord.CaptorUnit, unitObject);
                }
            }

            yield return new WaitForSeconds(0.075f);
            yield return StartCoroutine(ExecuteEffectEvents(true));
        }
        #endregion

        #region Trait Start Turn Act Out
        Hero actionHero = (Hero)actionUnit.Character;
        //if (actionHero.Trait == null || actionHero.Trait.Id != "selfish")
        //    actionHero.ApplyTrait(DarkestDangeonManager.Data.Traits.Find(trait => trait.Id == "selfish"));

        if (actionHero.Trait != null)
        {
            var actOut = RandomSolver.ChooseByRandom(actionHero.Trait.StartTurnActs);
            switch (actOut.ActType)
            {
                case StartTurnActType.AttackSelf:
                    #region Attack Self
                    yield return new WaitForSeconds(1f);
                    if (actionHero.AtDeathsDoor)
                    {
                        actionUnit.SetDefendAnimation(true);
                        yield return new WaitForSeconds(0.1f);
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");

                        if (PrepareDeath(actionUnit))
                            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathBlow);
                        else
                            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathsDoor);

                        if (actionUnit.CombatInfo.IsDead)
                        {
                            yield return new WaitForSeconds(1.4f);
                            BattleGround.Round.PostHeroTurn();
                            ExecuteDeath(actionUnit);
                            yield break;
                        }
                        else
                        {
                            yield return new WaitForSeconds(0.5f);
                            actionUnit.SetDefendAnimation(false);
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        int damageAmount = Mathf.RoundToInt(actOut.NumberParameter * actionHero.Health.ModifiedValue);
                        actionUnit.SetDefendAnimation(true);
                        yield return new WaitForSeconds(0.1f);

                        actionHero.Health.DecreaseValue(damageAmount);
                        actionUnit.OverlaySlot.UpdateOverlay();
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Damage, damageAmount.ToString());

                        if (Mathf.RoundToInt(actionUnit.Character.Health.CurrentValue) == 0)
                            PrepareDeath(actionUnit);

                        yield return new WaitForSeconds(0.5f);
                        actionUnit.SetDefendAnimation(false);
                        yield return new WaitForSeconds(0.1f);

                        yield return StartCoroutine(ExecuteEffectEvents(false));
                    }
                    break;
                #endregion
                case StartTurnActType.BarkStress:
                    #region Bark Stress
                    var barkStressEffect = DarkestDungeonManager.Data.Effects[actOut.StringParameter];
                    if (actionUnit.Party.Units.Count < 2) break;
                    yield return new WaitForSeconds(1f);
                    TempList.Clear();
                    TempList.AddRange(actionUnit.Party.Units);
                    TempList.Remove(actionUnit);
                    var barkTarget = TempList[RandomSolver.Next(TempList.Count)];
                    TempList.Clear();
                    for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                        barkStressEffect.SubEffects[i].Apply(actionUnit, barkTarget, barkStressEffect);
                    yield return new WaitForSeconds(0.1f);
                    yield return StartCoroutine(ExecuteEffectEvents(false));
                    break;
                #endregion
                case StartTurnActType.BuffAlly:
                    #region Buff Ally
                    var buffAllyEffect = DarkestDungeonManager.Data.Effects[actOut.StringParameter];
                    if (actionUnit.Party.Units.Count < 2) break;
                    yield return new WaitForSeconds(1f);
                    TempList.Clear();
                    TempList.AddRange(actionUnit.Party.Units);
                    TempList.Remove(actionUnit);
                    var buffAllyTarget = TempList[RandomSolver.Next(TempList.Count)];
                    TempList.Clear();
                    for (int i = 0; i < buffAllyEffect.SubEffects.Count; i++)
                        buffAllyEffect.SubEffects[i].Apply(actionUnit, buffAllyTarget, buffAllyEffect);
                    yield return new WaitForSeconds(0.1f);
                    yield return StartCoroutine(ExecuteEffectEvents(false));
                    break;
                #endregion
                case StartTurnActType.BuffParty:
                    #region Buff Party
                    var buffPartyEffect = DarkestDungeonManager.Data.Effects[actOut.StringParameter];
                    if (actionUnit.Party.Units.Count < 2) break;
                    yield return new WaitForSeconds(1f);

                    for (int i = 0; i < buffPartyEffect.SubEffects.Count; i++)
                        for (int j = 0; j < actionUnit.Party.Units.Count; j++)
                            buffPartyEffect.SubEffects[i].Apply(actionUnit, actionUnit.Party.Units[j], buffPartyEffect);

                    yield return new WaitForSeconds(0.1f);
                    yield return StartCoroutine(ExecuteEffectEvents(false));
                    break;
                #endregion
                case StartTurnActType.ChangePosition:
                    #region Change Position
                    if (actionUnit.Party.Units.Count < 2) break;
                    if (actionHero.Trait.Id == "masochistic" && actionUnit.Rank == 1) break;
                    if (actionHero.Trait.Id == "fearful" && actionUnit.Rank == actionUnit.Party.Units.Count) break;
                    TempList.Clear();
                    for (int i = 0; i < actionUnit.Party.Units.Count; i++)
                    {
                        if (actionUnit.Party.Units[i] != actionUnit)
                        {
                            if (actionHero.Trait.Id == "masochistic")
                            {
                                if (actionUnit.Party.Units[i].Rank < actionUnit.Rank)
                                    TempList.Add(actionUnit.Party.Units[i]);
                            }
                            else if (actionHero.Trait.Id == "fearful")
                            {
                                if (actionUnit.Party.Units[i].Rank > actionUnit.Rank)
                                    TempList.Add(actionUnit.Party.Units[i]);
                            }
                            else if (Mathf.Abs(actionUnit.Party.Units[i].Rank - actionUnit.Rank) < 3)
                                TempList.Add(actionUnit.Party.Units[i]);
                        }
                    }
                    if (TempList.Count == 0) break;
                    yield return new WaitForSeconds(1f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/combat_move");
                    yield return new WaitForSeconds(0.1f);
                    var shuffleRoll = TempList[RandomSolver.Next(TempList.Count)];
                    TempList.Clear();

                    if (shuffleRoll.Rank < actionUnit.Rank)
                        actionUnit.Pull(actionUnit.Rank - shuffleRoll.Rank);
                    else
                        actionUnit.Push(shuffleRoll.Rank - actionUnit.Rank);
                    yield return new WaitForSeconds(0.2f);
                    break;
                #endregion
                case StartTurnActType.HealSelf:
                    #region Heal Self
                    if (actionHero.HealthRatio == 1) break;
                    yield return new WaitForSeconds(1f);
                    int healAmount = Mathf.RoundToInt(actOut.NumberParameter * actionHero.Health.ModifiedValue);
                    if (actionHero.AtDeathsDoor)
                        actionHero.RevertDeathsDoor();
                    actionHero.Health.IncreaseValue(healAmount);

                    actionUnit.OverlaySlot.UpdateOverlay();
                    RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Heal, healAmount.ToString());
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                    if (actionHero.AtDeathsDoor)
                        actionHero.RevertDeathsDoor();
                    actionUnit.OverlaySlot.UpdateOverlay();
                    yield return new WaitForSeconds(0.5f);
                    break;
                #endregion
                case StartTurnActType.IgnoreCommand:
                    #region Ignore
                    yield return new WaitForSeconds(1f);
                    RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Pass);
                    yield return new WaitForSeconds(0.8f);
                    BattleGround.Round.PostHeroTurn();
                    yield break;
                #endregion
                case StartTurnActType.MarkSelf:
                    #region Mark Self
                    var markSelfEffect = DarkestDungeonManager.Data.Effects[actOut.StringParameter];
                    yield return new WaitForSeconds(1f);
                    for (int i = 0; i < markSelfEffect.SubEffects.Count; i++)
                        markSelfEffect.SubEffects[i].Apply(actionUnit, actionUnit, markSelfEffect);
                    yield return new WaitForSeconds(0.1f);
                    yield return StartCoroutine(ExecuteEffectEvents(false));
                    break;
                #endregion
                case StartTurnActType.RandomCommand:
                    break;
                case StartTurnActType.StressHealParty:
                    #region Stress Heal Party
                    var stressHealPartyEffect = DarkestDungeonManager.Data.Effects[actOut.StringParameter];
                    yield return new WaitForSeconds(1f);

                    for (int i = 0; i < stressHealPartyEffect.SubEffects.Count; i++)
                        for (int j = 0; j < actionUnit.Party.Units.Count; j++)
                            stressHealPartyEffect.SubEffects[i].Apply(actionUnit, actionUnit.Party.Units[j], stressHealPartyEffect);

                    yield return new WaitForSeconds(0.1f);
                    yield return StartCoroutine(ExecuteEffectEvents(false));
                    break;
                #endregion
                case StartTurnActType.StressHealSelf:
                    #region Stress Heal Self
                    var stressHealSelfEffect = DarkestDungeonManager.Data.Effects[actOut.StringParameter];
                    yield return new WaitForSeconds(1f);
                    for (int i = 0; i < stressHealSelfEffect.SubEffects.Count; i++)
                        stressHealSelfEffect.SubEffects[i].Apply(actionUnit, actionUnit, stressHealSelfEffect);
                    yield return new WaitForSeconds(0.1f);
                    yield return StartCoroutine(ExecuteEffectEvents(false));
                    break;
                #endregion
            }
        }
        #endregion

        actionUnit.Character.ApplyAllBuffRules(Rules.GetIdleUnitRules(actionUnit));
        
        while (true)
        {
            Formations.ShowUnitOverlay();
            BattleGround.Round.OnHeroTurn();

            if ((actionUnit.Team == Team.Heroes && PhotonNetwork.isMasterClient) || (actionUnit.Team == Team.Monsters && !PhotonNetwork.isMasterClient))
            {
                RaidPanel.SetCombatState();
                Inventory.SetCombatState();
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                RaidPanel.SetDisabledState();
                Inventory.SetDeactivated();
            }

            #region Hero Action
            QuestPanel.UpdateCombatRetreat(true);
            invaderQuestPanel.UpdateCombatRetreat(true);

            FormationUnit barkUnit;
            BarkMessage barkMessage;

            while (BattleGround.Round.HeroAction == HeroTurnAction.Waiting)
            {
                while (PhotonGameManager.BarkMessages.Count > 0)
                {
                    barkMessage = PhotonGameManager.BarkMessages.Find(message => message.Team == Team.Heroes);
                    if (barkMessage != null)
                    {
                        barkUnit = BattleGround.HeroParty.Units[Random.Range(0, BattleGround.HeroParty.Units.Count)];
                        pvpDialogUnits.Add(barkUnit);

                        barkUnit.OverlaySlot.StartDialog(barkMessage.Message, PhotonGameManager.SkipMessagesOnClick);
                        PhotonGameManager.BarkMessages.Remove(barkMessage);
                    }

                    barkMessage = PhotonGameManager.BarkMessages.Find(message => message.Team == Team.Monsters);
                    if (barkMessage != null)
                    {
                        barkUnit = BattleGround.MonsterParty.Units[Random.Range(0, BattleGround.MonsterParty.Units.Count)];
                        pvpDialogUnits.Add(barkUnit);

                        barkUnit.OverlaySlot.StartDialog(barkMessage.Message, PhotonGameManager.SkipMessagesOnClick);
                        PhotonGameManager.BarkMessages.Remove(barkMessage);
                    }

                    while (pvpDialogUnits.Count > 0)
                    {
                        for (int i = pvpDialogUnits.Count - 1; i >= 0; i--)
                            if (!pvpDialogUnits[i].OverlaySlot.IsDoingDialog)
                                pvpDialogUnits.RemoveAt(i);

                        yield return null;
                    }
                }

                yield return null;
            }

            QuestPanel.UpdateCombatRetreat(false);
            invaderQuestPanel.UpdateCombatRetreat(false);
            while (IsUnitEventInProgress)
                yield return null;

            Inventory.SetDeactivated();
            var targetUnit = BattleGround.Round.SelectedTarget;
            var usedSkill = RaidPanel.BannerPanel.SkillPanel.SelectedSkill;
            RaidPanel.SetDisabledState();

            yield return StartCoroutine(PhotonGameManager.PreparationCheck());

            switch (BattleGround.Round.HeroAction)
            {
                #region Move
                case HeroTurnAction.Move:
                    if (usedSkill is MoveSkill)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/combat_move");

                        if (actionUnit.Rank > targetUnit.Rank)
                            actionUnit.Pull(actionUnit.Rank - targetUnit.Rank);
                        else
                            actionUnit.Push(targetUnit.Rank - actionUnit.Rank);

                        actionUnit.Formation.UpdateBuffRule(BuffRule.InRank);
                    }
                    break;
                #endregion
                #region Combat Skill
                case HeroTurnAction.Skill:
                    if (usedSkill is CombatSkill)
                    {
                        var usedCombatSkill = usedSkill as CombatSkill;
                        SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(actionUnit,
                            targetUnit, usedCombatSkill).UpdateSkillInfo(actionUnit, usedCombatSkill);

                        yield return StartCoroutine(ExecuteHeroSkill(actionUnit, targetInfo, usedCombatSkill));

                        if (usedCombatSkill.IsContinueTurn)
                        {
                            Formations.ResetSelections();
                            yield return new WaitForEndOfFrame();
                            BattleGround.Round.PreHeroTurn(actionUnit);
                            yield return new WaitForEndOfFrame();
                            actionUnit.OverlaySlot.UnitSelected();
                            yield return StartCoroutine(PhotonGameManager.PreparationCheck());
                            continue;
                        }
                    }
                    break;
                #endregion
                #region Pass
                case HeroTurnAction.Pass:
                    Formations.Heroes.Overlay.ResetSelectionsExcept(actionUnit);
                    RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Pass);
                    yield return new WaitForSeconds(0.8f);
                    break;
                #endregion
                #region Retreat
                case HeroTurnAction.Retreat:
                    Formations.ResetSelections();
                    bool retreatFailed = RandomSolver.CheckSuccess(0.2f);
                    for (int i = 0; i < HeroParty.Units.Count; i++)
                    {
                        #region Trait Block
                        if (HeroParty.Units[i].Character.Trait != null)
                        {
                            if (RandomSolver.CheckSuccess(HeroParty.Units[i].
                                Character.Trait.Reactions[ReactionType.BlockMove].Chance))
                            {
                                retreatFailed = true;
                                string dialogId = HeroParty.Units[i].Character.Class +
                                    "+str_block_combat_retreat_" + HeroParty.Units[i].Character.Trait.Id;
                                HeroParty.Units[i].OverlaySlot.StartDialog(LocalizationManager.GetString(dialogId));
                                while (HeroParty.Units[i].OverlaySlot.IsDoingDialog)
                                    yield return null;
                                break;
                            }
                        }
                        #endregion
                    }

                    if (retreatFailed)
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.RetreatFailed);
                        DarkestSoundManager.ExecuteNarration("battle_retreat_fail", NarrationPlace.Raid);
                        yield return new WaitForSeconds(0.6f);
                        BattleGround.Round.HeroAction = HeroTurnAction.Pass;
                    }
                    else
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/retreat");
                        DarkestSoundManager.ExecuteNarration("battle_retreat", NarrationPlace.Raid);

                        #region Execute Hero Transformations
                        for (int i = 0; i < Formations.Heroes.Party.Units.Count; i++)
                        {
                            var hero = Formations.Heroes.Party.Units[i].Character as Hero;
                            if (Formations.Heroes.Party.Units[i].Character.Mode != null
                                && Formations.Heroes.Party.Units[i].Character.Mode.AfflictionSkillId != null)
                            {
                                var battleFinishSkill = hero.SelectedCombatSkills.Find(skill => skill.Id ==
                                    Formations.Heroes.Party.Units[i].Character.Mode.BattleCompleteSkillId);
                                if (battleFinishSkill != null)
                                {
                                    SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(Formations.Heroes.Party.Units[i],
                                        Formations.Heroes.Party.Units[i], battleFinishSkill).
                                        UpdateSkillInfo(Formations.Heroes.Party.Units[i], battleFinishSkill);
                                    yield return StartCoroutine(ExecuteHeroSkill(Formations.Heroes.Party.Units[i],
                                        targetInfo, battleFinishSkill));
                                }
                            }
                        }
                        #endregion

                        DarkestDungeonManager.ScreenFader.Fade(2);
                        yield return new WaitForSeconds(0.5f);

                        BattleGround.ResetTargetRanks();
                        DarkestSoundManager.StopBattleSoundtrack();
                        DarkestSoundManager.ContinueDungeonSoundtrack(Raid.Quest.Dungeon);

                        #region Destroy Remains
                        Formations.HideMonsterOverlay();
                        RaidEvents.RoundIndicator.Disappear();
                        RaidEvents.RoundIndicator.End();
                        BattleGround.RetreatFromBattle();
                        yield return new WaitForSeconds(0.2f);
                        #endregion

                        #region Reset Hero Statuses
                        foreach (var hero in Formations.Heroes.Party.Units)
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
                        foreach (var hero in Formations.Heroes.Party.Units)
                            hero.SetCombatAnimation(false);
                        RaidEvents.MonsterTooltip.Hide();
                        #endregion

                        if (SceneState == DungeonSceneState.Hall)
                        {
                            HallwayView.CurrentSector.SetInside(false);
                            CurrentEvent = RoomLoadingEvent(HallwayView.StartingRoom,
                                RoomTransitionType.Retreat, HallwayView.CurrentSector);
                        }
                        else if (SceneState == DungeonSceneState.Room)
                        {
                            var currentRoom = Raid.CurrentLocation as DungeonRoom;
                            var targetDoor = currentRoom.Doors.Find(door => door.TargetArea == Raid.LastSector.Hallway.Id);
                            var direction = targetDoor.Direction.OppositeDirection();
                            CurrentEvent = HallwayLoadingEvent(Raid.LastSector, HallTransitionType.Retreat, direction, currentRoom);
                        }

                        #region Remove Combat States and Restrictions
                        QuestPanel.SetPeacefulState();
                        invaderQuestPanel.SetPeacefulState();
                        Formations.ShowHeroOverlay();
                        RaidPanel.SetPeacefulState();
                        EnableEnviroment();
                        BattleGround.LeaveBattleGround();
                        Inventory.SetPeacefulState(false);
                        StartCoroutine(CurrentEvent);
                        #endregion
                        yield break;
                    }
                    break;
                #endregion
            }
            break;
            #endregion
        }

        BattleGround.Round.PostHeroTurn();
    }

    protected override IEnumerator MonsterTurn(FormationUnit actionUnit, string combatSkillOverride = null, bool fromBonusTurn = false)
    {
        yield return StartCoroutine(PhotonGameManager.PreparationCheck());

        yield return StartCoroutine(base.MonsterTurn(actionUnit, combatSkillOverride, fromBonusTurn));
    }

    protected override void ExecuteDeath(FormationUnit targetUnit)
    {
        if (targetUnit.CombatInfo.IsDead)
        {
            if (targetUnit.Character.IsMonster)
            {
                if (RaidEvents.MonsterTooltip.Slot == targetUnit.OverlaySlot)
                    RaidEvents.MonsterTooltip.Hide();
                Monster monster = targetUnit.Character as Monster;

                if (monster.SkillReaction != null && monster.SkillReaction.WasKilledOtherMonstersEffects.Count > 0)
                {
                    for (int i = 0; i < targetUnit.Party.Units.Count; i++)
                        for (int j = 0; j < monster.SkillReaction.WasKilledOtherMonstersEffects.Count; j++)
                            for (int k = 0; k < monster.SkillReaction.WasKilledOtherMonstersEffects[j].SubEffects.Count; k++)
                                monster.SkillReaction.WasKilledOtherMonstersEffects[j].SubEffects[k].
                                    Apply(targetUnit.Party.Units[i], targetUnit.Party.Units[i],
                                    monster.SkillReaction.WasKilledOtherMonstersEffects[j]);
                }

                var companionRecord = BattleGround.Companions.Find(record =>
                record.TargetUnit == targetUnit || record.CompanionUnit == targetUnit);
                if (companionRecord != null)
                {
                    BattleGround.Companions.Remove(companionRecord);

                    if (companionRecord.CompanionUnit == targetUnit)
                    {
                        foreach (var buff in companionRecord.CompanionComponent.Buffs)
                            companionRecord.TargetUnit.Character.RemoveSourceBuff(buff, BuffSourceType.Adventure);
                        companionRecord.TargetUnit.OverlaySlot.UpdateOverlay();
                    }
                }


                if (monster.Data.ControllerCaptor != null)
                {
                    var controlRecord = BattleGround.Controls.Find(control => control.ControllUnit == targetUnit);
                    if (controlRecord != null)
                        BattleGround.UncontrolUnit(controlRecord);
                }
                if (monster.Data.FullCaptor != null)
                {
                    var captureRecord = BattleGround.Captures.Find(capture => capture.CaptorUnit == targetUnit);
                    if (captureRecord != null)
                        BattleGround.ReleaseUnit(captureRecord);

                    if (monster.Data.LifeLink != null && BattleGround.IsLifeLinked(targetUnit, monster.Data.LifeLink))
                    {
                        MonsterData emptyCaptorData = DarkestDungeonManager.Data.Monsters[monster.Data.FullCaptor.EmptyMonsterClass];
                        GameObject unitObject = Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                        BattleGround.ReplaceUnit(emptyCaptorData, targetUnit, unitObject);
                    }
                    else
                    {
                        UnitEventQueue.RemoveAll(item => item == targetUnit);
                        BattleGround.UnitDestroyed(targetUnit);
                        Formations.Monsters.DeleteUnit(targetUnit);
                    }
                }
                else if (monster.Data.DeathClass != null)
                {
                    if (monster.Data.DeathClass.Type == DeathClassType.Corpse)
                    {
                        targetUnit.SetCorpseAnimation(true);
                        BattleGround.UnitCorpsed(targetUnit);
                        Formations.Monsters.SpawnCorpse(targetUnit,
                            new Monster(DarkestDungeonManager.Data.Monsters[monster.Data.DeathClass.CorpseClass]));
                    }
                    else
                    {
                        var deathClass = monster.Data.DeathClass;
                        UnitEventQueue.RemoveAll(item => item == targetUnit);
                        MonsterData replacementData = DarkestDungeonManager.Data.Monsters[monster.Data.DeathClass.CorpseClass];
                        GameObject unitObject = Resources.Load("Prefabs/Monsters/" + replacementData.TypeId) as GameObject;
                        var finalUnit = BattleGround.ReplaceUnit(replacementData, targetUnit, unitObject, false, 1);

                        for (int i = 0; i < deathClass.DeathChangeEffects.Count; i++)
                            for (int j = 0; j < deathClass.DeathChangeEffects[i].SubEffects.Count; j++)
                                deathClass.DeathChangeEffects[i].SubEffects[j].
                                    ApplyInstant(finalUnit, finalUnit, deathClass.DeathChangeEffects[i]);
                    }
                }
                else
                {
                    UnitEventQueue.RemoveAll(item => item == targetUnit);
                    BattleGround.UnitDestroyed(targetUnit);
                    Formations.Monsters.DeleteUnit(targetUnit);

                    if (BattleGround.SharedHealth.IsActive)
                        if (BattleGround.SharedHealth.SharedUnits.Contains(targetUnit))
                            for (int i = 0; i < BattleGround.SharedHealth.SharedUnits.Count; i++)
                                if (BattleGround.MonsterParty.Units.Contains(BattleGround.SharedHealth.SharedUnits[i]))
                                    ExecuteDeath(BattleGround.SharedHealth.SharedUnits[i]);

                    if (BattleGround.SharedHealth.IsActive)
                        BattleGround.SharedHealth.Reset();
                }
            }
            else if (targetUnit.Character is Hero)
            {
                #region Captures and Controls
                var captureRecord = BattleGround.Captures.Find(capture => capture.PrisonerUnit == targetUnit);
                if (captureRecord != null)
                {
                    var monster = captureRecord.CaptorUnit.Character as Monster;
                    BattleGround.ReleaseUnit(captureRecord);

                    if (monster.Data.LifeLink != null && BattleGround.IsLifeLinked(captureRecord.CaptorUnit, monster.Data.LifeLink))
                    {
                        MonsterData emptyCaptorData = DarkestDungeonManager.Data.Monsters[monster.Data.FullCaptor.EmptyMonsterClass];
                        GameObject unitObject = Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                        BattleGround.ReplaceUnit(emptyCaptorData, captureRecord.CaptorUnit, unitObject);
                    }
                    else
                    {
                        UnitEventQueue.RemoveAll(item => item == targetUnit);
                        BattleGround.UnitDestroyed(targetUnit);
                        Formations.Monsters.DeleteUnit(targetUnit);
                    }
                }
                var controlRecord = BattleGround.Controls.Find(control => control.PrisonerUnit == targetUnit);
                if (controlRecord != null)
                    BattleGround.Controls.Remove(controlRecord);
                #endregion

                UnitEventQueue.RemoveAll(item => item == targetUnit);
                ResolveCheckQueue.RemoveAll(item => item == targetUnit);
                HeartAttackCheckQueue.RemoveAll(item => item == targetUnit);
                DeathDoorEnterQueue.RemoveAll(item => item == targetUnit);
                BattleGround.UnitDestroyed(targetUnit);
                targetUnit.Formation.DeleteUnit(targetUnit);
                if (RaidPanel.SelectedUnit == targetUnit)
                {
                    if (Formations.Heroes.Party.Units.Count > 0)
                        Formations.Heroes.Party.Units[0].OverlaySlot.UnitSelected();
                }
                for (int i = 0; i < targetUnit.Party.Units.Count; i++)
                    DarkestDungeonManager.Data.Effects["Stress 3"].ApplyIndependent(targetUnit.Party.Units[i]);
            }
        }
    }

    protected override bool PrepareDeath(FormationUnit targetUnit, DeathFactor deathFactor = DeathFactor.AttackMonster, FormationUnit killer = null)
    {
        if (targetUnit.Character.IsMonster)
        {
            if (targetUnit.CombatInfo.IsDead)
                return true;
            if (targetUnit.Character.DeathClass != null &&
                targetUnit.Character.DeathClass.CanDieFromDamage == false)
                return false;

            targetUnit.CombatInfo.IsDead = true;

            Monster monster = targetUnit.Character as Monster;

            if (BattleGround.SharedHealth.IsActive)
                if (BattleGround.SharedHealth.SharedUnits.Contains(targetUnit))
                    for (int i = 0; i < BattleGround.SharedHealth.SharedUnits.Count; i++)
                        if (BattleGround.SharedHealth.SharedUnits[i].CombatInfo.IsDead == false)
                            PrepareDeath(BattleGround.SharedHealth.SharedUnits[i]);

            if (monster.Data.FullCaptor != null &&
                BattleGround.Captures.Find(capture => capture.CaptorUnit == targetUnit) != null)
            {
                targetUnit.SetReleaseAnimation(true);
                targetUnit.SetDefendAnimation(false);
            }
            else if (monster.Types.Contains(MonsterType.Corpse))
                targetUnit.SetCorpseKillAnimation(true);
            else
                targetUnit.SetDeathAnimation(true);

            if (monster.Data.FullCaptor != null)
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/death_enemy");

            FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + monster.Data.TypeId + "_vo_death");

            if (!monster.MonsterTypes.Contains(MonsterType.Corpse))
                DarkestSoundManager.ExecuteNarration("kill_monster", NarrationPlace.Raid,
                monster.Class, monster.Size > 1 ? "strong" : "weak", monster.Size > 1 ? "big" : "small",
                (deathFactor == DeathFactor.BleedMonster || deathFactor == DeathFactor.PoisonMonster) ? "dot" : "one_shot");

            if (monster.Data.FullCaptor == null)
            {
                GameObject deathFx = Instantiate(Resources.Load("Prefabs/Effects/" +
                    monster.CommonEffects.DeathEffect) as GameObject);
                AnimatedEffect effect = deathFx.GetComponent<AnimatedEffect>();
                effect.gameObject.layer = targetUnit.CurrentState.gameObject.layer;
                effect.BindToTarget(targetUnit, targetUnit.SkeletonAnimations[1], "fxdeath");
            }

            targetUnit.ResetHalo();
            targetUnit.OverlaySlot.Hide();
            return true;
        }
        else
        {
            if (targetUnit.CombatInfo.IsDead)
                return true;

            Hero hero = targetUnit.Character as Hero;
            if (hero.AtDeathsDoor || targetUnit.CombatInfo.MarkedForDeath)
            {
                float resistIgnoreBonus = targetUnit.Team == Team.Heroes ?
                    HeroParty.Units.Count > BattleGround.MonsterParty.Units.Count ? 0.3f : 0 :
                    HeroParty.Units.Count < BattleGround.MonsterParty.Units.Count ? 0.3f : 0 ;

                if (RandomSolver.CheckSuccess(hero.DeathResist - resistIgnoreBonus) && !targetUnit.CombatInfo.MarkedForDeath)
                {
                    hero.AddBuff(new BuffInfo(DeathsDoorSurvivalDebuff, BuffSourceType.Adventure));
                    targetUnit.OverlaySlot.UpdateOverlay();
                    return false;
                }

                targetUnit.SetDeathAnimation(true);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/death_ally");

                var captureRecord = BattleGround.Captures.Find(capture => capture.PrisonerUnit == targetUnit);
                if (captureRecord != null)
                {
                    captureRecord.CaptorUnit.SetReleaseAnimation(true);
                    captureRecord.CaptorUnit.SetDefendAnimation(false);
                }

                GameObject deathFx = Instantiate(Resources.Load("Prefabs/Effects/death_medium") as GameObject);
                AnimatedEffect effect = deathFx.GetComponent<AnimatedEffect>();
                effect.gameObject.layer = targetUnit.CurrentState.gameObject.layer;
                effect.BindToTarget(targetUnit, targetUnit.SkeletonAnimations[1], "fxdeath");
                targetUnit.ResetHalo();
                targetUnit.CombatInfo.IsDead = true;
                targetUnit.OverlaySlot.Hide();

                DarkestSoundManager.ExecuteNarration("kill_hero", NarrationPlace.Raid);
                return true;
            }
            else
            {
                DeathDoorEnterQueue.Add(targetUnit);
                return false;
            }
        }
    }

    #region Player Actions

    public override void AbandonButtonClicked()
    {
        StartCoroutine(RaidResultsEvent());
    }

    public override void HeroPassButtonClicked()
    {
        PhotonGameManager.Instanse.photonView.RPC("HeroPassButtonClicked", PhotonTargets.All);
    }

    public override void HeroSkillTargetSelected(FormationOverlaySlot overlaySlot)
    {
        if (BattleGround.Round.SelectedUnit.Team == Team.Heroes && !PhotonNetwork.isMasterClient)
            return;

        if (BattleGround.Round.SelectedUnit.Team == Team.Monsters && PhotonNetwork.isMasterClient)
            return;

        var primaryTarget = overlaySlot.TargetUnit;

        if (BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            if (RaidPanel.BannerPanel.SkillPanel.SelectedSkill is MoveSkill)
                PhotonGameManager.Instanse.photonView.RPC("HeroMoveButtonClicked", PhotonTargets.All, primaryTarget.CombatInfo.CombatId);
            else if (RaidPanel.BannerPanel.SkillPanel.SelectedSkill is CombatSkill)
                PhotonGameManager.Instanse.photonView.RPC("HeroSkillButtonClicked", PhotonTargets.All, primaryTarget.CombatInfo.CombatId);
        }
        else if (Raid.CampingPhase == CampingPhase.Skill)
        {
            RaidEvents.CampEvent.SelectedTarget = overlaySlot.TargetUnit;
            RaidEvents.CampEvent.ActionType = CampUsageResultType.Skill;
        }
        else
        {
            if (RaidPanel.BannerPanel.SkillPanel.SelectedSkill is MoveSkill)
            {
                var swapper = RaidPanel.SelectedUnit;
                var target = overlaySlot.TargetUnit;
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/combat_move");
                Formations.Heroes.SwapUnits(swapper, target);
                target.OverlaySlot.UnitSelected();
            }
            RaidPanel.BannerPanel.SkillPanel.MoveSlot.Deselect();
        }
    }

    public override void HeroSkillSelected(BattleSkillSlot skillSlot)
    {
        int slotIndex = RaidPanel.BannerPanel.SkillPanel.SkillSlots.IndexOf(skillSlot);
        PhotonGameManager.Instanse.photonView.RPC("HeroSkillSelected", PhotonTargets.All, slotIndex);
    }

    public override void HeroMoveSelected(MoveSkillSlot skillSlot)
    {
        PhotonGameManager.Instanse.photonView.RPC("HeroMoveSelected", PhotonTargets.All);
    }

    public override void HeroMoveDeselected(MoveSkillSlot skillSlot)
    {
        PhotonGameManager.Instanse.photonView.RPC("HeroMoveDeselected", PhotonTargets.All);
    }

    public void HeroSkillSelected(int skillSlotIndex)
    {
        BattleSkillSlot skillSlot = RaidPanel.BannerPanel.SkillPanel.SkillSlots[skillSlotIndex];
        RaidPanel.BannerPanel.SkillPanel.SelectedSkill = skillSlot.Skill;

        BattleFormation allies, enemies;
        if (BattleGround.Round.SelectedUnit.Team == Team.Heroes)
        {
            allies = BattleGround.HeroFormation;
            enemies = BattleGround.MonsterFormation;
        }
        else
        {
            allies = BattleGround.MonsterFormation;
            enemies = BattleGround.HeroFormation;
        }

        if (skillSlot.Skill.TargetRanks.IsSelfFormation || skillSlot.Skill.TargetRanks.IsSelfTarget)
        {
            enemies.Overlay.ResetSelections();

            if (skillSlot.Skill.TargetRanks.IsSelfTarget)
            {
                BattleGround.Round.SelectedUnit.SetFriendlyPerformerStatus(true);
                allies.Overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            }
            else
            {
                for (int i = 0; i < allies.Party.Units.Count; i++)
                {
                    if (allies.Party.Units[i] == BattleGround.Round.SelectedUnit)
                    {
                        if (skillSlot.Skill.IsSelfValid)
                        {
                            if (skillSlot.Skill.Heal != null && BattleGround.Round.SelectedUnit.CombatInfo.
                                BlockedHealUnitIds.Contains(BattleGround.Round.SelectedUnit.CombatInfo.CombatId))
                                BattleGround.Round.SelectedUnit.SetPerformerStatus();
                            else if (skillSlot.Skill.IsBuffSkill && BattleGround.Round.SelectedUnit.CombatInfo.
                                BlockedBuffUnitIds.Contains(BattleGround.Round.SelectedUnit.CombatInfo.CombatId))
                                BattleGround.Round.SelectedUnit.SetPerformerStatus();
                            else if (skillSlot.Skill.TargetRanks.IsTargetableUnit(BattleGround.Round.SelectedUnit))
                                BattleGround.Round.SelectedUnit.SetFriendlyPerformerStatus(true);
                            else
                                BattleGround.Round.SelectedUnit.SetPerformerStatus();
                        }
                    }
                    else
                    {
                        if (skillSlot.Skill.Heal != null && BattleGround.Round.SelectedUnit.CombatInfo.
                            BlockedHealUnitIds.Contains(allies.Party.Units[i].CombatInfo.CombatId))
                            allies.Party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.IsBuffSkill && BattleGround.Round.SelectedUnit.CombatInfo.
                            BlockedBuffUnitIds.Contains(allies.Party.Units[i].CombatInfo.CombatId))
                            allies.Party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.TargetRanks.IsTargetableUnit(allies.Party.Units[i]))
                            allies.Party.Units[i].SetFriendlyTargetStatus(true);
                        else
                            allies.Party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
        else
        {
            allies.Overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            if (BattleGround.Round.SelectedUnit.IsTargetable)
                BattleGround.Round.SelectedUnit.SetPerformerStatus();

            TempList.Clear();

            for (int i = 0; i < enemies.Party.Units.Count; i++)
            {
                if (skillSlot.Skill.TargetRanks.IsTargetableUnit(enemies.Party.Units[i]))
                    TempList.Add(enemies.Party.Units[i]);
                else
                    enemies.Party.Units[i].SetDeactivatedStatus();
            }

            if (skillSlot.Skill.TargetRanks.IsMultitarget && TempList.Count > 0)
            {
                if (BattleGround.Round.SelectedUnit.Team == Team.Heroes)
                    for (int i = 0; i < TempList.Count; i++)
                        TempList[i].SetEnemyTargetStatus(true, i != TempList.Count - 1);
                else
                    for (int i = 0; i < TempList.Count; i++)
                        TempList[i].SetEnemyTargetStatus(true, i != 0);
            }
            else
                foreach (var target in TempList)
                    target.SetEnemyTargetStatus(true, false);

            TempList.Clear();
        }
    }

    public void HeroMoveSelected()
    {
        MoveSkillSlot skillSlot = RaidPanel.BannerPanel.SkillPanel.MoveSlot;
        RaidPanel.BannerPanel.SkillPanel.SelectedSkill = skillSlot.Skill;

        if (BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            BattleFormation allies, enemies;
            if (BattleGround.Round.SelectedUnit.Team == Team.Heroes)
            {
                allies = BattleGround.HeroFormation;
                enemies = BattleGround.MonsterFormation;
            }
            else
            {
                allies = BattleGround.MonsterFormation;
                enemies = BattleGround.HeroFormation;
            }

            enemies.Overlay.ResetSelections();
            for (int i = 0; i < allies.Party.Units.Count; i++)
            {
                if (allies.Party.Units[i] == BattleGround.Round.SelectedUnit)
                    BattleGround.Round.SelectedUnit.SetPerformerStatus();
                else
                {
                    int distance = BattleGround.Round.SelectedUnit.Rank - allies.Party.Units[i].Rank;
                    if (BattleGround.Round.SelectedUnit.CombatInfo.BlockedMoveUnitIds.
                        Contains(allies.Party.Units[i].CombatInfo.CombatId))
                    {
                        allies.Party.Units[i].SetDeactivatedStatus();
                    }
                    else if (distance < 0)
                    {
                        if (skillSlot.Skill.MoveBackward >= -distance && !allies.Party.Units[i].CombatInfo.IsImmobilized)
                            allies.Party.Units[i].SetMoveTargetStatus(true);
                        else
                            allies.Party.Units[i].SetDeactivatedStatus();
                    }
                    else
                    {
                        if (skillSlot.Skill.MoveForward >= distance && !allies.Party.Units[i].CombatInfo.IsImmobilized)
                            allies.Party.Units[i].SetMoveTargetStatus(true);
                        else
                            allies.Party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
    }

    public void HeroMoveDeselected()
    {
        if (BattleGround.BattleStatus == BattleStatus.Peace)
        {
            for (int i = 0; i < Formations.Heroes.Party.Units.Count; i++)
            {
                if (Formations.Heroes.Party.Units[i] != RaidPanel.SelectedUnit)
                    Formations.Heroes.Party.Units[i].SetDeactivatedStatus();
            }
        }
    }

    #endregion
}
