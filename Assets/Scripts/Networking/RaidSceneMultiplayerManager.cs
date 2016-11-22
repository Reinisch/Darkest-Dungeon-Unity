using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RaidSceneMultiplayerManager : RaidSceneManager
{
    public static new RaidSceneMultiplayerManager Instanse { get; set; }

    public RaidQuestPanel invaderQuestPanel;

    public static RaidQuestPanel InvaderQuestPanel
    {
        get
        {
            return Instanse.invaderQuestPanel;
        }
    }

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
            currentRaid = new RaidInfo();
            currentRaid.Quest = MultiplayerQuest;
            currentRaid.Dungeon = MultiplayerDungeon;
            currentRaid.RaidParty = new RaidParty(PhotonNetwork.masterClient);

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
        QuestPanel.UpdateQuest(currentRaid.Quest, PhotonNetwork.masterClient, PhotonNetwork.isMasterClient);

        if (PhotonNetwork.room.playerCount < 2)
            InvaderQuestPanel.gameObject.SetActive(false);
        else if (PhotonNetwork.isMasterClient)
            InvaderQuestPanel.UpdateQuest(currentRaid.Quest, PhotonNetwork.otherPlayers[0], false);
        else
            InvaderQuestPanel.UpdateQuest(currentRaid.Quest, PhotonNetwork.player, true);

        DarkestSoundManager.StartDungeonSoundtrack(currentRaid.Dungeon.Name);
        TorchMeter.Initialize(100);
        Formations.Initialize();

        PhotonGameManager.PlayersPreparedCount = 0;

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
        InvaderQuestPanel.DisableRetreat(false);
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        Formations.LockSelections();
        RaidPanel.heroPanel.equipmentPanel.SetDisabled();
        RaidPanel.SetDisabledState();
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

        if (PhotonNetwork.room.playerCount < 2)
            RaidEvents.ShowAnnouncment("Waiting for opponent to join...");

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
        InvaderQuestPanel.EnableRetreat();
        MapPanel.ShowAvailableRooms(room);
        Inventory.SetPeacefulState(false);
        RaidPanel.heroPanel.equipmentPanel.SetActive();
        RaidPanel.SetDisabledState();
        currentEvent = null;
        #endregion
    }
    protected override IEnumerator EncounterEvent(IRaidArea areaView, bool campfireAmbush = false)
    {
        #region Set Combat States and Restrictions
        QuestPanel.UpdateEncounterRetreat();
        QuestPanel.SetCombatState();
        InvaderQuestPanel.UpdateEncounterRetreat();
        InvaderQuestPanel.SetCombatState();
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
        yield return new WaitForSeconds(0.5f);
       
        if (HeroParty.Units.Count == 0)
        {
            if (PhotonNetwork.isMasterClient)
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.otherPlayers[0].name + " is victorious!");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/retreat");
            }
            else
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.player.name + " is victorious!");
                DarkestSoundManager.ExecuteNarration("victory", NarrationPlace.Raid);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");
            }
        }
        else
        {
            if (PhotonNetwork.isMasterClient)
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.player.name + " is victorious!");
                DarkestSoundManager.ExecuteNarration("victory", NarrationPlace.Raid);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");
            }
            else
            {
                RaidEvents.ShowAnnouncment("Player " + PhotonNetwork.masterClient.name + " is victorious!");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/retreat");
            }
        }
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(RaidResultsEvent());
        yield break;
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
        DungeonCamera.SwitchBlur(true);
        while (completionWindow.Action == CompletionAction.Waiting)
            yield return null;

        if (completionWindow.Action == CompletionAction.Return)
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
            tempList.Clear();
            for (int i = 0; i < battleGround.MonsterParty.Units.Count; i++)
                if (battleGround.MonsterParty.Units[i].Character.LifeTime != null)
                    tempList.Add(battleGround.MonsterParty.Units[i]);

            if (tempList.Count > 0)
            {
                bool someoneExpired = false;
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i].Character.LifeTime.AliveRoundLimit <= tempList[i].CombatInfo.RoundsAlive)
                    {
                        PrepareDeath(tempList[i]);
                        someoneExpired = true;
                    }
                }

                if (someoneExpired)
                {
                    yield return new WaitForSeconds(1.2f);
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        if (tempList[i].CombatInfo.IsDead)
                            ExecuteDeath(tempList[i]);
                    }
                }
                tempList.Clear();
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
            tempList.AddRange(battleGround.MonsterParty.Units);
            while (tempList.Count > 0)
            {
                var monsterUnit = tempList[0];
                tempList.RemoveAt(0);
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
                        if (currentDesire.CombatSkillOverride == "")
                            yield return StartCoroutine(MonsterTurn(monsterUnit));
                        else
                            yield return StartCoroutine(MonsterOverriddenTurn(monsterUnit, currentDesire.CombatSkillOverride));
                        break;
                    }
                }
            }
            tempList.Clear();
            #endregion

            #region Mutation Activation
            if (BattleGround.Round.RoundNumber != 1)
            {
                for (int k = 0; k < 1; k++)
                {
                    if (k > 0)
                        yield return new WaitForSeconds(0.2f);

                    tempList.AddRange(battleGround.MonsterParty.Units.FindAll(unit => unit.Character.IsMonster
                        && (unit.Character as Monster).Data.Shapeshifter != null));
                    if (tempList.Count > 0)
                    {
                        Formations.HideUnitOverlay();
                        yield return new WaitForSeconds(0.2f);
                        DungeonCamera.Zoom(50, 0.05f);
                        yield return new WaitForSeconds(0.05f);
                        DungeonCamera.SwitchBlur(true);
                        foreach (var targetUnit in tempList)
                            Formations.UnitBuffedIntro(targetUnit);
                        yield return new WaitForSeconds(0.05f);
                        List<string> mutations = new List<string>();
                        List<MonsterData> mutationData = new List<MonsterData>();
                        #region Transformation
                        foreach (var targetUnit in tempList)
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
                        for (int i = 0; i < tempList.Count; i++)
                        {
                            if (tempList[i].Character.Class != mutationData[i].TypeId)
                            {
                                tempList[i].SetTargetEffect(tempList[i], "formless_mutate", "root",
                                    tempList[i].Character.Class + "_to_" + mutations[i]);
                                mutated = true;
                            }
                        }
                        if (mutated)
                            FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/_shared/formless_shared_mutate");

                        yield return new WaitForSeconds(0.01f);
                        Formations.partyBuffPositions.SetUnitTargets(tempList, 0.05f, Vector2.zero);
                        yield return new WaitForSeconds(0.05f);
                        Formations.partyBuffPositions.SetSpacing(120, 1f);
                        for (int i = 0; i < tempList.Count; i++)
                            if (tempList[i].Character.Class != mutationData[i].TypeId)
                                tempList[i].CurrentState.MeshRenderer.enabled = false;
                        yield return new WaitForSeconds(1.2f);
                        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
                        DungeonCamera.SwitchBlur(false);
                        foreach (var targetUnit in tempList)
                            Formations.UnitBuffedOutro(targetUnit);
                        #region Transformation
                        for (int i = 0; i < tempList.Count; i++)
                        {
                            if (tempList[i].Character.Class != mutationData[i].TypeId)
                            {
                                GameObject summonObject = Resources.Load("Prefabs/Monsters/" + mutationData[i].TypeId) as GameObject;
                                BattleGround.ReplaceUnit(mutationData[i], tempList[i], summonObject, true);
                            }
                        }
                        #endregion
                        yield return new WaitForSeconds(0.175f);
                        Formations.ShowUnitOverlay();
                        Formations.ResetSelections();
                    }
                    tempList.Clear();
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
                                Formations.partyBuffPositions.SetUnitTargets(BattleGround.Captures[i].CaptorUnit, 0.05f, Vector2.zero);
                                yield return new WaitForSeconds(1.2f);
                                DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
                                DungeonCamera.SwitchBlur(false);
                                Formations.UnitSkillOutro(BattleGround.Captures[i].CaptorUnit, "release");
                                var captureRelease = BattleGround.Captures[i];
                                BattleGround.ReleaseUnit(captureRelease);
                                MonsterData emptyCaptorData = DarkestDungeonManager.Data.Monsters[(captureRelease.CaptorUnit.
                                    Character as Monster).Data.FullCaptor.EmptyMonsterClass];
                                GameObject unitObject = Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                                RaidSceneManager.BattleGround.ReplaceUnit(emptyCaptorData, captureRelease.CaptorUnit, unitObject);
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
            tempList.AddRange(battleGround.MonsterParty.Units);
            while (tempList.Count > 0)
            {
                var monsterUnit = tempList[0];
                tempList.RemoveAt(0);
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
                        yield return waitForOneTwo;

                        if (currentDesire.CombatSkillOverride == "")
                            yield return StartCoroutine(MonsterTurn(monsterUnit));
                        else
                            yield return StartCoroutine(MonsterOverriddenTurn(monsterUnit, currentDesire.CombatSkillOverride));
                        break;
                    }
                }
            }
            tempList.Clear();
            #endregion

            if (BattleGround.IsBattleEnded())
                break;

            yield return waitForZeroThree;
        }

        #region Round Finish Desires
        tempList.AddRange(battleGround.MonsterParty.Units);
        while (tempList.Count > 0)
        {
            var monsterUnit = tempList[0];
            tempList.RemoveAt(0);
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
                    if (currentDesire.CombatSkillOverride == "")
                        yield return StartCoroutine(MonsterTurn(monsterUnit));
                    else
                        yield return StartCoroutine(MonsterOverriddenTurn(monsterUnit, currentDesire.CombatSkillOverride));
                    break;
                }
            }
        }
        tempList.Clear();
        #endregion

        #region Idle units status effects
        tempList.AddRange(BattleGround.MonsterParty.Units.FindAll(targetUnit => targetUnit.CombatInfo.TotalInitiatives == 0));
        bool hasIdleDamage = false, hasIdleDeath = false;
        foreach (var idleUnit in tempList)
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
            for (int i = 0; i < tempList.Count; i++)
                ExecuteDeath(tempList[i]);
            yield return new WaitForSeconds(0.2f);
        }
        else if (hasIdleDamage)
        {
            yield return new WaitForSeconds(0.3f);
        }
        tempList.Clear();
        #endregion

        yield break;
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
            RaidEvents.ShowAnnouncment("Next turn: " + PhotonNetwork.masterClient.name);
        else if (PhotonNetwork.isMasterClient && PhotonNetwork.otherPlayers.Length > 0)
            RaidEvents.ShowAnnouncment("Next turn: " + PhotonNetwork.otherPlayers[0].name);
        else if (!PhotonNetwork.isMasterClient)
            RaidEvents.ShowAnnouncment("Next turn: " + PhotonNetwork.player.name);

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
                        yield break;
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathsDoor);
                        yield return new WaitForSeconds(0.6f);
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
                        yield break;
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathsDoor);
                        yield return new WaitForSeconds(0.6f);
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

            int damage = Mathf.RoundToInt(initialDamage * (1 +
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
            actionUnit.OverlaySlot.stressBar.UpdateStress(actionUnit.Character.Stress.ValueRatio);

            RaidSceneManager.RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Stress, damage.ToString());
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
                    RaidSceneManager.BattleGround.ReplaceUnit(emptyCaptorData, captureRecord.CaptorUnit, unitObject);
                }
            }

            yield return new WaitForSeconds(0.075f);
            yield return StartCoroutine(ExecuteEffectEvents(true));
        }
        #endregion

        #region Trait Start Turn Act Out
        Hero actionHero = actionUnit.Character as Hero;
        //if (actionHero.Trait == null || actionHero.Trait.Id != "selfish")
        //    actionHero.ApplyTrait(DarkestDangeonManager.Data.Traits.Find(trait => trait.Id == "selfish"));

        if (actionHero.Trait != null)
        {
            var actOut = RandomSolver.ChooseByRandom<CombatStartTurnActOut>(actionHero.Trait.StartTurnActs);
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
                        actionUnit.OverlaySlot.healthBar.UpdateHealth(actionHero);
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
                    tempList.Clear();
                    tempList.AddRange(actionUnit.Party.Units);
                    tempList.Remove(actionUnit);
                    var barkTarget = tempList[RandomSolver.Next(tempList.Count)];
                    tempList.Clear();
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
                    tempList.Clear();
                    tempList.AddRange(actionUnit.Party.Units);
                    tempList.Remove(actionUnit);
                    var buffAllyTarget = tempList[RandomSolver.Next(tempList.Count)];
                    tempList.Clear();
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
                    tempList.Clear();
                    for (int i = 0; i < actionUnit.Party.Units.Count; i++)
                    {
                        if (actionUnit.Party.Units[i] != actionUnit)
                        {
                            if (actionHero.Trait.Id == "masochistic")
                            {
                                if (actionUnit.Party.Units[i].Rank < actionUnit.Rank)
                                    tempList.Add(actionUnit.Party.Units[i]);
                            }
                            else if (actionHero.Trait.Id == "fearful")
                            {
                                if (actionUnit.Party.Units[i].Rank > actionUnit.Rank)
                                    tempList.Add(actionUnit.Party.Units[i]);
                            }
                            else if (Mathf.Abs(actionUnit.Party.Units[i].Rank - actionUnit.Rank) < 3)
                                tempList.Add(actionUnit.Party.Units[i]);
                        }
                    }
                    if (tempList.Count == 0) break;
                    yield return new WaitForSeconds(1f);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/combat_move");
                    yield return new WaitForSeconds(0.1f);
                    var shuffleRoll = tempList[RandomSolver.Next(tempList.Count)];
                    tempList.Clear();

                    if (shuffleRoll.Rank < actionUnit.Rank)
                        actionUnit.Pull(actionUnit.Rank - shuffleRoll.Rank);
                    else
                        actionUnit.Push(shuffleRoll.Rank - actionUnit.Rank);
                    yield return new WaitForSeconds(0.2f);
                    break;
                #endregion
                case StartTurnActType.HealSelf:
                    #region Heal Self
                    if (actionHero.Health.ValueRatio == 11) break;
                    yield return new WaitForSeconds(1f);
                    float healAmount = Mathf.RoundToInt(actOut.NumberParameter * actionHero.Health.ModifiedValue);
                    if (actionHero.AtDeathsDoor)
                        actionHero.RevertDeathsDoor();
                    actionHero.Health.IncreaseValue(healAmount);

                    actionUnit.OverlaySlot.healthBar.UpdateHealth(actionHero);
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
                default:
                    break;
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
            InvaderQuestPanel.UpdateCombatRetreat(true);
            while (BattleGround.Round.HeroAction == HeroTurnAction.Waiting)
            {
                yield return null;
            }
            QuestPanel.UpdateCombatRetreat(false);
            InvaderQuestPanel.UpdateCombatRetreat(false);
            while (IsUnitEventInProgress)
                yield return null;

            Inventory.SetDeactivated();
            var targetUnit = BattleGround.Round.SelectedTarget;
            var usedSkill = RaidPanel.bannerPanel.skillPanel.SelectedSkill;
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
                            usedSkill = null;
                            yield return StartCoroutine(PhotonGameManager.PreparationCheck());
                            continue;
                        }
                    }
                    break;
                #endregion
                #region Pass
                case HeroTurnAction.Pass:
                    Formations.heroes.overlay.ResetSelectionsExcept(actionUnit);
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

                        DarkestDungeonManager.ScreenFader.Fade(2);
                        yield return new WaitForSeconds(0.5f);

                        BattleGround.ResetTargetRanks();
                        DarkestSoundManager.StopBattleSoundtrack();
                        DarkestSoundManager.ContinueDungeonSoundtrack(Raid.Quest.Dungeon);

                        #region Destroy Remains
                        Formations.HideMonsterOverlay();
                        RaidEvents.roundIndicator.Disappear();
                        RaidEvents.roundIndicator.End();
                        BattleGround.RetreatFromBattle();
                        yield return new WaitForSeconds(0.2f);
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
                        foreach (var hero in Formations.heroes.party.Units)
                            hero.SetCombatAnimation(false);
                        RaidEvents.MonsterTooltip.Hide();
                        #endregion

                        if (SceneState == DungeonSceneState.Hall)
                        {
                            HallwayView.CurrentSector.SetInside(false);
                            currentEvent = RoomLoadingEvent(HallwayView.StartingRoom,
                                RoomTransitionType.Retreat, HallwayView.CurrentSector);
                        }
                        else if (SceneState == DungeonSceneState.Room)
                        {
                            var currentRoom = Raid.CurrentLocation as DungeonRoom;
                            var targetDoor = currentRoom.Doors.Find(door => door.TargetArea == Raid.LastSector.Hallway.Id);
                            var direction = currentRoom.OppositeDirection(targetDoor.Direction);
                            currentEvent = HallwayLoadingEvent(Raid.LastSector, HallTransitionType.Retreat, direction, currentRoom);
                        }

                        #region Remove Combat States and Restrictions
                        QuestPanel.SetPeacefulState();
                        InvaderQuestPanel.SetPeacefulState();
                        Formations.ShowHeroOverlay();
                        RaidPanel.SetPeacefulState();
                        EnableEnviroment();
                        BattleGround.LeaveBattleGround();
                        Inventory.SetPeacefulState(false);
                        StartCoroutine(currentEvent);
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
    protected override IEnumerator MonsterTurn(FormationUnit actionUnit)
    {
        yield return StartCoroutine(PhotonGameManager.PreparationCheck());

        FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/enemy_turn");
        Formations.ResetSelections();
        yield return new WaitForEndOfFrame();
        BattleGround.Round.PreMonsterTurn(actionUnit);
        yield return new WaitForEndOfFrame();
        Formations.ShowUnitOverlay();
        yield return new WaitForEndOfFrame();
        actionUnit.SetPerformerStatus();

        #region Status Effects and Buffs
        if (actionUnit.Character.GetStatusEffect(StatusType.Bleeding).IsApplied)
        {
            var bleedEffect = actionUnit.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect;
            actionUnit.Character.Health.DecreaseValue(bleedEffect.CurrentTickDamage);
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
            actionUnit.OverlaySlot.UpdateOverlay();

            #region Damage Activation
            if (Mathf.RoundToInt(actionUnit.Character.Health.CurrentValue) != 0)
            {
                RaidEvents.ShowPopupMessage(actionUnit,
                    PopupMessageType.Damage, bleedEffect.CurrentTickDamage.ToString());
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
                        yield break;
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathsDoor);
                        yield return new WaitForSeconds(0.6f);
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
                        yield break;
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.DeathsDoor);
                        yield return new WaitForSeconds(0.6f);
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

        BattleGround.Round.OnMonsterTurn();

        yield return StartCoroutine(ExecuteMonsterSkill(actionUnit));
        if (BattleGround.Round.HeroAction != HeroTurnAction.Retreat)
            BattleGround.Round.PostMonsterTurn();
    }
    protected override IEnumerator MonsterOverriddenTurn(FormationUnit actionUnit, string combatSkillOverride)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/enemy_turn");
        Formations.ResetSelections();
        yield return new WaitForEndOfFrame();
        Formations.ShowUnitOverlay();
        yield return new WaitForEndOfFrame();
        actionUnit.SetPerformerStatus();

        yield return StartCoroutine(ExecuteMonsterOverridenSkill(actionUnit, combatSkillOverride));
    }

    protected override IEnumerator ExecuteHeroSkill(FormationUnit actionUnit, SkillTargetInfo targetInfo, CombatSkill skill)
    {
        RaidEvents.MonsterTooltip.IsDisabled = true;
        RaidEvents.MonsterTooltip.Hide();
        ExecuteGuardRedirection(actionUnit, targetInfo);
        Formations.HideUnitOverlay();
        TorchMeter.Hide();
        yield return new WaitForSeconds(0.2f);
        DungeonCamera.Zoom(50, 0.05f);
        var skillResult = ExecuteSkillBase(actionUnit, targetInfo);
        if (skillResult.HasCritEffect && targetInfo.Type == SkillTargetType.Enemy)
            DarkestSoundManager.ExecuteNarration("crit_monster", NarrationPlace.Raid);
        yield return new WaitForSeconds(0.05f);
        DungeonCamera.SwitchBlur(true);
        ExecuteSkillAnimationIntro(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.01f);
        ExecuteSkillInstants(actionUnit, targetInfo, skillResult);
        yield return new WaitForSeconds(0.01f);
        ExecuteSlidingSetup(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.70f);
        #region Riposte Skill Activation
        List<FormationUnit> riposters = new List<FormationUnit>();
        List<SkillResult> riposteResults = new List<SkillResult>();
        if (targetInfo.Type == SkillTargetType.Enemy)
            foreach (var target in targetInfo.Targets)
            {
                if (target.Character.GetStatusEffect(StatusType.Riposte).IsApplied)
                {
                    if (target.CombatInfo.IsDead == false)
                    {
                        var riposteSkill = target.Character.RiposteSkill;

                        if (riposteSkill != null)
                        {
                            var riposteArt = target.Character.SkillArtInfo.Find(art => art.SkillId == riposteSkill.Id);
                            if (riposteArt != null)
                            {
                                #region Skill Execution
                                BattleSolver.SkillResult.Reset();
                                BattleSolver.ExecuteSkill(target, actionUnit, riposteSkill, riposteArt);

                                riposters.Add(target);
                                riposteResults.Add(BattleSolver.SkillResult.Copy());

                                if (target.Character is Hero)
                                {
                                    if (target.Character.Mode != null)
                                        FMODUnity.RuntimeManager.PlayOneShot("event:/char/ally/" +
                                            target.Character.Class + "_" + riposteSkill.Id + "_" + target.Character.Mode.Id);
                                    else
                                        FMODUnity.RuntimeManager.PlayOneShot("event:/char/ally/" +
                                            target.Character.Class + "_" + riposteSkill.Id);
                                }
                                else
                                {
                                    FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" +
                                        target.Character.Class + "_" + riposteSkill.Id);
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
        #endregion
        yield return new WaitForSeconds(0.05f);
        #region Riposte Animation Intro
        if (riposteResults.Count > 0)
        {
            actionUnit.SetPerformerSkillAnimation(targetInfo.SkillArtInfo, false);
            actionUnit.SetDefendAnimation(true);

            for (int i = 0; i < riposteResults.Count; i++)
            {
                riposters[i].SetDefendAnimation(false);
                riposters[i].SetPerformerSkillAnimation(riposteResults[i].ArtInfo, true);
            }
        }
        #endregion
        yield return new WaitForSeconds(0.05f);
        #region Execute Riposte Instants
        for (int i = 0; i < riposters.Count; i++)
        {
            foreach (var skillEntry in riposteResults[i].SkillEntries)
            {
                if (skillEntry.Target.CombatInfo.IsDead)
                    break;

                skillEntry.Target.OverlaySlot.UpdateOverlay();

                if (skillEntry.IsHarmful && skillEntry.Target.Character.AtDeathsDoor)
                {
                    if (PrepareDeath(skillEntry.Target))
                    {
                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.DeathBlow);
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.DeathsDoor);
                    }
                }
                else
                {
                    if (!skillEntry.Target.Character.IsMonster && skillEntry.IsZeroed)
                    {
                        if (skillEntry.Type == SkillResultType.Hit || skillEntry.Type == SkillResultType.Crit)
                            if (!skillEntry.Target.Character.AtDeathsDoor && !deathDoorEnterQueue.Contains(skillEntry.Target))
                                PrepareDeath(skillEntry.Target);
                    }
                    
                    switch (skillEntry.Type)
                    {
                        case SkillResultType.Miss:
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Miss, "", 40 * i);
                            break;
                        case SkillResultType.Dodge:
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Dodge, "", 40 * i);
                            break;
                        case SkillResultType.Hit:
                            if (skillEntry.Amount < 1)
                                RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.ZeroDamage, "", 40 * i);
                            else
                                RaidEvents.ShowPopupMessage(skillEntry.Target,
                                    PopupMessageType.Damage, skillEntry.Amount.ToString(), 40 * i);
                            break;
                        case SkillResultType.Crit:
                            if (skillEntry.Amount < 1)
                                RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.ZeroDamage, "", 40 * i);
                            else
                                RaidEvents.ShowPopupMessage(skillEntry.Target,
                                    PopupMessageType.CritDamage, skillEntry.Amount.ToString(), 40 * i);
                            break;
                        case SkillResultType.Heal:
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Heal, skillEntry.Amount.ToString());
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                            break;
                        case SkillResultType.CritHeal:
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.CritHeal, skillEntry.Amount.ToString());
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally_crit");
                            break;
                        case SkillResultType.Utility:
                        default:
                            break;
                    }
                    if (skillEntry.Target.Character.IsMonster && skillEntry.IsZeroed)
                        PrepareDeath(skillEntry.Target);
                }
            }
            actionUnit.OverlaySlot.UpdateOverlay();
        }
        #endregion
        if (riposters.Count > 0)
            yield return new WaitForSeconds(1.2f);
        else
            yield return new WaitForSeconds(0.7f);
        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
        DungeonCamera.SwitchBlur(false);
        #region Animation Outro
        #region Riposte Results
        if (riposteResults.Count > 0)
        {
            actionUnit.SetPerformerSkillAnimation(targetInfo.SkillArtInfo, false);
            actionUnit.SetDefendAnimation(false);

            for (int i = 0; i < riposteResults.Count; i++)
            {
                riposters[i].SetDefendAnimation(false);
                riposters[i].SetPerformerSkillAnimation(riposteResults[i].ArtInfo, false);
            }
        }
        #endregion

        #region Skill Outro
        if (skill.ValidModes.Count > 1 && targetInfo.Mode != null)
            Formations.UnitSkillOutroOverriden(actionUnit, targetInfo.SkillArtInfo, targetInfo.Mode.Id);
        else
            Formations.UnitSkillOutro(actionUnit, targetInfo.SkillArtInfo);
        #endregion

        #region Defend Outro
        if (targetInfo.Type == SkillTargetType.Party)
        {
            foreach (var targetUnit in targetInfo.Targets)
                if (actionUnit != targetUnit)
                    Formations.UnitBuffedOutro(targetUnit);
        }
        else if (targetInfo.Type == SkillTargetType.Enemy)
        {
            foreach (var targetUnit in targetInfo.Targets)
                Formations.UnitDefendOutro(targetUnit);
        }
        #endregion

        #region Execute Deaths
        List<DeathDamage> deathDamages = new List<DeathDamage>();
        for (int i = BattleGround.HeroParty.Units.Count - 1; i >= 0; i--)
        {
            if (BattleGround.HeroParty.Units[i].CombatInfo.IsDead)
                if (BattleGround.HeroParty.Units[i].Character.DeathDamage != null)
                    deathDamages.Add(BattleGround.HeroParty.Units[i].Character.DeathDamage);

            ExecuteDeath(BattleGround.HeroParty.Units[i]);
        }
        for (int i = BattleGround.MonsterParty.Units.Count - 1; i >= 0; i--)
        {
            if (BattleGround.MonsterParty.Units[i].CombatInfo.IsDead)
                if (BattleGround.MonsterParty.Units[i].Character.DeathDamage != null)
                    deathDamages.Add(BattleGround.MonsterParty.Units[i].Character.DeathDamage);

            ExecuteDeath(BattleGround.MonsterParty.Units[i]);
        }
        #endregion

        #region Execute Death Damages
        for (int i = 0; i < deathDamages.Count; i++)
        {
            var deathDamageTarget = actionUnit.Team == Team.Heroes ?
                BattleGround.MonsterParty.Units.Find(unit =>
                unit.Character.Class == deathDamages[i].TargetBaseClass) :
                BattleGround.HeroParty.Units.Find(unit =>
                unit.Character.Class == deathDamages[i].TargetBaseClass);

            if (deathDamageTarget != null)
            {
                deathDamageTarget.Character.Health.DecreaseValue(deathDamages[i].TargetDamage);
                deathDamageTarget.OverlaySlot.UpdateOverlay();
                RaidEvents.ShowPopupMessage(deathDamageTarget,
                    PopupMessageType.Damage, deathDamages[i].TargetDamage.ToString());
                deathDamageTarget.SetDefendAnimation(true);
                yield return new WaitForSeconds(0.8f);
                deathDamageTarget.SetDefendAnimation(false);
            }
        }
        #endregion

        riposteResults.Clear();
        riposters.Clear();
        #endregion
        yield return new WaitForSeconds(0.175f);
        Formations.ShowUnitOverlay();
        TorchMeter.Show();
        Formations.ResetSelections();
        yield return new WaitForSeconds(0.075f);

        if (targetInfo.Type == SkillTargetType.Enemy && skillResult.HasCritEffect)
        {
            DarkestDungeonManager.Data.Effects["Heal Stress 1"].ApplyIndependent(actionUnit);

            for (int j = 0; j < BattleGround.HeroParty.Units.Count; j++)
                if (BattleGround.HeroParty.Units[j] != actionUnit && RandomSolver.CheckSuccess(0.33f))
                    DarkestDungeonManager.Data.Effects["Heal Stress 1"].
                        ApplyIndependent(BattleGround.HeroParty.Units[j]);
        }
        else if (skillResult.HasDeadEffect)
            DarkestDungeonManager.Data.Effects["Heal Stress Chance 1"].ApplyIndependent(actionUnit);

        yield return StartCoroutine(ExecuteEffectEvents(true));

        for (int i = 0; i < targetInfo.Targets.Count; i++)
            BattleSolver.RemoveConditions(targetInfo.Targets[i]);
        BattleSolver.RemoveConditions(actionUnit);

        RaidEvents.MonsterTooltip.IsDisabled = false;

        #region Trait Comment Attack Result
        if (BattleGround.HeroParty.Units.Contains(actionUnit)
            && BattleGround.HeroParty.Units.Count > 1)
        {
            for (int i = 0; i < actionUnit.Party.Units.Count; i++)
            {
                if (actionUnit != actionUnit.Party.Units[i] && actionUnit.Party.Units[i].Character.Trait != null)
                {
                    if (targetInfo.Type == SkillTargetType.Enemy && skillResult.HasHit)
                    {
                        if (RandomSolver.CheckSuccess(actionUnit.Party.Units[i].Character.
                            Trait.Reactions[ReactionType.CommentAllyAttackHit].Chance))
                        {
                            var barkStressEffect = actionUnit.Party.Units[i].Character.
                                Trait.Reactions[ReactionType.CommentAllyAttackHit].Effect;
                            yield return new WaitForSeconds(1f);
                            for (int j = 0; j < barkStressEffect.SubEffects.Count; j++)
                                barkStressEffect.SubEffects[j].Apply(actionUnit.Party.Units[i], actionUnit, barkStressEffect);
                            yield return new WaitForSeconds(0.1f);
                            yield return StartCoroutine(ExecuteEffectEvents(false));
                            break;
                        }
                    }
                    if (targetInfo.Type == SkillTargetType.Enemy && !skillResult.HasHit)
                    {
                        if (RandomSolver.CheckSuccess(actionUnit.Party.Units[i].Character.
                            Trait.Reactions[ReactionType.CommentAllyAttackMiss].Chance))
                        {
                            var barkStressEffect = actionUnit.Party.Units[i].Character.
                                Trait.Reactions[ReactionType.CommentAllyAttackMiss].Effect;
                            yield return new WaitForSeconds(1f);
                            for (int j = 0; j < barkStressEffect.SubEffects.Count; j++)
                                barkStressEffect.SubEffects[j].Apply(actionUnit.Party.Units[i], actionUnit, barkStressEffect);
                            yield return new WaitForSeconds(0.1f);
                            yield return StartCoroutine(ExecuteEffectEvents(false));
                            break;
                        }
                    }
                }
            }
        }
        #endregion
    }
    protected override IEnumerator ExecuteResolveChecks()
    {
        while (resolveCheckQueue.Count > 0)
        {
            var resolveUnit = resolveCheckQueue[0];
            var resolveHero = resolveUnit.Character as Hero;
            resolveCheckQueue.RemoveAt(0);
            float virtueChance = 0.25f + resolveUnit.Character[AttributeType.ResolveCheckPercent].ModifiedValue;
            virtueChance = Mathf.Clamp(virtueChance, 0.01f, 0.6f);
            bool isVirtue = RandomSolver.CheckSuccess(virtueChance);
            var availableTraits = isVirtue ? DarkestDungeonManager.Data.Traits.FindAll(trait => trait.Type == OverstressType.Virtue) :
                DarkestDungeonManager.Data.Traits.FindAll(trait => trait.Type == OverstressType.Affliction);
            Trait resolveTrait = availableTraits[RandomSolver.Next(availableTraits.Count)];

            if (!isVirtue)
                for (int i = 0; i < resolveUnit.Party.Units.Count; i++)
                    if (resolveUnit.Party.Units[i] != resolveUnit)
                        DarkestDungeonManager.Data.Effects["AfflictedAllyStress"].
                            ApplyIndependent(resolveUnit.Party.Units[i]);

            if (!isVirtue && resolveUnit.Character.Mode != null && resolveUnit.Character.Mode.AfflictionSkillId != null)
            {
                var resolveSkill = resolveHero.SelectedCombatSkills.Find(skill =>
                skill.Id == resolveUnit.Character.Mode.AfflictionSkillId);
                if (resolveSkill != null)
                {
                    SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(resolveUnit,
                        resolveUnit, resolveSkill).UpdateSkillInfo(resolveUnit, resolveSkill);
                    yield return StartCoroutine(ExecuteHeroSkill(resolveUnit, targetInfo, resolveSkill));
                }
            }

            RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("resolve_test"),
                resolveUnit.Character.Name), AnnouncmentPosition.Top);

            FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_test");
            yield return new WaitForSeconds(1.6f);
            RaidEvents.HideAnnouncment();
            Formations.HideUnitOverlay();
            yield return new WaitForSeconds(0.1f);
            DungeonCamera.SwitchBlur(true);

            Rules.GetIdleUnitRules(resolveUnit);
            resolveHero.ApplyTrait(resolveTrait);
            resolveHero.ApplySingleBuffRule(Rules, BuffRule.Afflicted);
            resolveHero.ApplySingleBuffRule(Rules, BuffRule.Virtued);

            if (isVirtue)
            {
                DarkestSoundManager.ExecuteNarration("virtue", NarrationPlace.Raid, resolveTrait.Id);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_virtue");
            }
            else
            {
                DarkestSoundManager.ExecuteNarration("afflicted", NarrationPlace.Raid, resolveTrait.Id);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_afflict");
            }

            Formations.HeroResolveCheckIntro(resolveUnit, isVirtue);
            Formations.partyBuffPositions.SetUnitTarget(resolveUnit, 0.05f, Vector2.zero);
            RaidEvents.ShowAnnouncment(isVirtue ?
                LocalizationManager.GetString("str_virtue_name_" + resolveTrait.Id) :
                LocalizationManager.GetString("str_affliction_name_" + resolveTrait.Id), AnnouncmentPosition.Bottom);
            if (!Rules.IsDoingCamping)
                DungeonCamera.Zoom(45, 0.1f);

            yield return new WaitForSeconds(2.45f);
            if (!Rules.IsDoingCamping)
                DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);

            Formations.HeroResolveCheckOutro(resolveUnit, isVirtue);
            DungeonCamera.SwitchBlur(false);
            yield return new WaitForSeconds(0.15f);
            if (isVirtue)
                resolveUnit.Character.Stress.CurrentValue = RandomSolver.Next(20, 40);
            resolveUnit.OverlaySlot.UpdateOverlay();

            RaidEvents.HideAnnouncment();
            Formations.ShowUnitOverlay();
            yield return new WaitForSeconds(0.15f);
        }
        yield break;
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
                        unitEventQueue.RemoveAll(item => item == targetUnit);
                        BattleGround.UnitDestroyed(targetUnit);
                        Formations.monsters.DeleteUnit(targetUnit);
                    }
                }
                else if (monster.Data.DeathClass != null)
                {
                    if (monster.Data.DeathClass.Type == DeathClassType.Corpse)
                    {
                        targetUnit.SetCorpseAnimation(true);
                        BattleGround.UnitCorpsed(targetUnit);
                        Formations.monsters.SpawnCorpse(targetUnit,
                            new Monster(DarkestDungeonManager.Data.Monsters[monster.Data.DeathClass.CorpseClass]));
                    }
                    else
                    {
                        var deathClass = monster.Data.DeathClass;
                        unitEventQueue.RemoveAll(item => item == targetUnit);
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
                    unitEventQueue.RemoveAll(item => item == targetUnit);
                    BattleGround.UnitDestroyed(targetUnit);
                    Formations.monsters.DeleteUnit(targetUnit);

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
                        unitEventQueue.RemoveAll(item => item == targetUnit);
                        BattleGround.UnitDestroyed(targetUnit);
                        Formations.monsters.DeleteUnit(targetUnit);
                    }
                }
                var controlRecord = BattleGround.Controls.Find(control => control.PrisonerUnit == targetUnit);
                if (controlRecord != null)
                    BattleGround.Controls.Remove(controlRecord);
                #endregion

                unitEventQueue.RemoveAll(item => item == targetUnit);
                resolveCheckQueue.RemoveAll(item => item == targetUnit);
                heartAttackCheckQueue.RemoveAll(item => item == targetUnit);
                deathDoorEnterQueue.RemoveAll(item => item == targetUnit);
                BattleGround.UnitDestroyed(targetUnit);
                targetUnit.Formation.DeleteUnit(targetUnit);
                if (RaidPanel.SelectedUnit == targetUnit)
                {
                    if (Formations.heroes.party.Units.Count > 0)
                        Formations.heroes.party.Units[0].OverlaySlot.UnitSelected();
                }
                for (int i = 0; i < targetUnit.Party.Units.Count; i++)
                    DarkestDungeonManager.Data.Effects["Stress 3"].ApplyIndependent(targetUnit.Party.Units[i]);
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
        //BattleGround.Round.HeroActionSelected(HeroTurnAction.Pass, BattleGround.Round.SelectedUnit);
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
            if (RaidPanel.bannerPanel.skillPanel.SelectedSkill is MoveSkill)
            {
                PhotonGameManager.Instanse.photonView.RPC("HeroMoveButtonClicked", PhotonTargets.All, primaryTarget.CombatInfo.CombatId);
                //BattleGround.Round.HeroActionSelected(HeroTurnAction.Move, primaryTarget);
            }
            else if (RaidPanel.bannerPanel.skillPanel.SelectedSkill is CombatSkill)
            {
                PhotonGameManager.Instanse.photonView.RPC("HeroSkillButtonClicked", PhotonTargets.All, primaryTarget.CombatInfo.CombatId);
                //BattleGround.Round.HeroActionSelected(HeroTurnAction.Skill, primaryTarget);
            }
        }
        else if (Raid.CampingPhase == CampingPhase.Skill)
        {
            RaidEvents.CampEvent.SelectedTarget = overlaySlot.TargetUnit;
            RaidEvents.CampEvent.ActionType = CampUsageResultType.Skill;
        }
        else
        {
            if (RaidPanel.bannerPanel.skillPanel.SelectedSkill is MoveSkill)
            {
                var swapper = RaidPanel.SelectedUnit;
                var target = overlaySlot.TargetUnit;
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/combat_move");
                Formations.heroes.SwapUnits(swapper, target);
                target.OverlaySlot.UnitSelected();
            }
            RaidPanel.bannerPanel.skillPanel.moveSlot.Deselect();
        }
    }

    public override void HeroSkillSelected(BattleSkillSlot skillSlot)
    {
        int slotIndex = RaidPanel.bannerPanel.skillPanel.skillSlots.IndexOf(skillSlot);
        PhotonGameManager.Instanse.photonView.RPC("HeroSkillSelected", PhotonTargets.All, slotIndex);
    }
    public void HeroSkillSelected(int skillSlotIndex)
    {
        BattleSkillSlot skillSlot = RaidPanel.bannerPanel.skillPanel.skillSlots[skillSlotIndex];
        RaidPanel.bannerPanel.skillPanel.SelectedSkill = skillSlot.Skill;

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
            enemies.overlay.ResetSelections();

            if (skillSlot.Skill.TargetRanks.IsSelfTarget)
            {
                BattleGround.Round.SelectedUnit.SetFriendlyPerformerStatus(true);
                allies.overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            }
            else
            {
                for (int i = 0; i < allies.party.Units.Count; i++)
                {
                    if (allies.party.Units[i] == BattleGround.Round.SelectedUnit)
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
                            BlockedHealUnitIds.Contains(allies.party.Units[i].CombatInfo.CombatId))
                            allies.party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.IsBuffSkill && BattleGround.Round.SelectedUnit.CombatInfo.
                            BlockedBuffUnitIds.Contains(allies.party.Units[i].CombatInfo.CombatId))
                            allies.party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.TargetRanks.IsTargetableUnit(allies.party.Units[i]))
                            allies.party.Units[i].SetFriendlyTargetStatus(true);
                        else
                            allies.party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
        else
        {
            allies.overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            if (BattleGround.Round.SelectedUnit.IsTargetable)
                BattleGround.Round.SelectedUnit.SetPerformerStatus();

            tempList.Clear();

            for (int i = 0; i < enemies.party.Units.Count; i++)
            {
                if (skillSlot.Skill.TargetRanks.IsTargetableUnit(enemies.party.Units[i]))
                    tempList.Add(enemies.party.Units[i]);
                else
                    enemies.party.Units[i].SetDeactivatedStatus();
            }

            if (skillSlot.Skill.TargetRanks.IsMultitarget && tempList.Count > 0)
            {
                if (BattleGround.Round.SelectedUnit.Team == Team.Heroes)
                    for (int i = 0; i < tempList.Count; i++)
                        tempList[i].SetEnemyTargetStatus(true, i != tempList.Count - 1);
                else
                    for (int i = 0; i < tempList.Count; i++)
                        tempList[i].SetEnemyTargetStatus(true, i != 0);
            }
            else
                foreach (var target in tempList)
                    target.SetEnemyTargetStatus(true, false);

            tempList.Clear();
        }
    }

    public override void HeroMoveSelected(MoveSkillSlot skillSlot)
    {
        PhotonGameManager.Instanse.photonView.RPC("HeroMoveSelected", PhotonTargets.All);
    }
    public void HeroMoveSelected()
    {
        MoveSkillSlot skillSlot = RaidPanel.bannerPanel.skillPanel.moveSlot;
        RaidPanel.bannerPanel.skillPanel.SelectedSkill = skillSlot.Skill;

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

            enemies.overlay.ResetSelections();
            for (int i = 0; i < allies.party.Units.Count; i++)
            {
                if (allies.party.Units[i] == BattleGround.Round.SelectedUnit)
                    BattleGround.Round.SelectedUnit.SetPerformerStatus();
                else
                {
                    int distance = BattleGround.Round.SelectedUnit.Rank - allies.party.Units[i].Rank;
                    if (BattleGround.Round.SelectedUnit.CombatInfo.BlockedMoveUnitIds.
                        Contains(allies.party.Units[i].CombatInfo.CombatId))
                    {
                        allies.party.Units[i].SetDeactivatedStatus();
                    }
                    else if (distance < 0)
                    {
                        if (skillSlot.Skill.MoveBackward >= -distance && !allies.party.Units[i].CombatInfo.IsImmobilized)
                            allies.party.Units[i].SetMoveTargetStatus(true);
                        else
                            allies.party.Units[i].SetDeactivatedStatus();
                    }
                    else
                    {
                        if (skillSlot.Skill.MoveForward >= distance && !allies.party.Units[i].CombatInfo.IsImmobilized)
                            allies.party.Units[i].SetMoveTargetStatus(true);
                        else
                            allies.party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
    }

    public override void HeroMoveDeselected(MoveSkillSlot skillSlot)
    {
        PhotonGameManager.Instanse.photonView.RPC("HeroMoveDeselected", PhotonTargets.All);
    }
    public void HeroMoveDeselected()
    {
        if (BattleGround.BattleStatus == BattleStatus.Peace)
        {
            for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
            {
                if (Formations.heroes.party.Units[i] != RaidPanel.SelectedUnit)
                    Formations.heroes.party.Units[i].SetDeactivatedStatus();
            }
        }
    }

    #endregion
}
