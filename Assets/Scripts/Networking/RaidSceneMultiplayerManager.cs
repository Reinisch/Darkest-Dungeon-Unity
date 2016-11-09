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
        QuestPanel.UpdateQuest(currentRaid.Quest, PhotonNetwork.player);
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

    protected override IEnumerator BattleRound(bool fromBattleSave = false)
    {
        if (fromBattleSave == false)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/round");
            yield return new WaitForSeconds(1f);

            #region Stalling
            if (BattleGround.MonsterFormation.IsStallingActive())
            {
                BattleGround.StallingRoundNumber++;

                if (BattleGround.StallingRoundNumber == 3)
                {
                    var stallEffect = DarkestDungeonManager.Data.Effects["stall_stress"];
                    for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
                        for (int j = 0; j < stallEffect.SubEffects.Count; j++)
                            stallEffect.SubEffects[j].Apply(BattleGround.HeroParty.Units[i],
                                BattleGround.HeroParty.Units[i], stallEffect);

                    yield return StartCoroutine(ExecuteEffectEvents(false));
                }
                else if (BattleGround.StallingRoundNumber == 4)
                {
                    var stallSet = RandomSolver.ChooseByRandom(Raid.Dungeon.DungeonMash.StallEncounters);
                    if (stallSet != null)
                    {
                        for (int i = 0; i < stallSet.MonsterSet.Count; i++)
                        {
                            MonsterData summonData = DarkestDungeonManager.Data.Monsters[stallSet.MonsterSet[i]];
                            if (summonData.Size <= BattleGround.MonsterFormation.AvailableSummonSpace)
                            {
                                GameObject summonObject = Resources.Load("Prefabs/Monsters/" + summonData.TypeId) as GameObject;
                                BattleGround.SummonUnit(summonData, summonObject, i + 1, true, false);
                            }
                        }
                    }
                    BattleGround.StallingRoundNumber = 0;
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
                BattleGround.StallingRoundNumber = 0;
            #endregion

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
                        dungeonCamera.Zoom(50, 0.05f);
                        yield return new WaitForSeconds(0.05f);
                        DungeonCamera.blur.enabled = true;
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
                        dungeonCamera.Zoom(dungeonCamera.StandardFOV, 0.1f);
                        DungeonCamera.blur.enabled = false;
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
                                dungeonCamera.Zoom(50, 0.05f);
                                FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + captorMonster.Data.TypeId + "_vo_death");
                                yield return new WaitForSeconds(0.05f);
                                DungeonCamera.blur.enabled = true;
                                Formations.UnitSkillIntro(BattleGround.Captures[i].CaptorUnit, "release");
                                yield return new WaitForSeconds(0.05f);
                                Formations.partyBuffPositions.SetUnitTargets(BattleGround.Captures[i].CaptorUnit, 0.05f, Vector2.zero);
                                yield return new WaitForSeconds(1.2f);
                                dungeonCamera.Zoom(dungeonCamera.StandardFOV, 0.1f);
                                DungeonCamera.blur.enabled = false;
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
            yield return StartCoroutine(HeroTurn(unit));

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
                    var barkTarget = tempList[Random.Range(0, tempList.Count)];
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
                    var buffAllyTarget = tempList[Random.Range(0, tempList.Count)];
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
                    var shuffleRoll = tempList[Random.Range(0, tempList.Count)];
                    tempList.Clear();

                    if (shuffleRoll.Rank < actionUnit.Rank)
                        actionUnit.Pull(actionUnit.Rank - shuffleRoll.Rank);
                    else
                        actionUnit.Push(shuffleRoll.Rank - actionUnit.Rank);
                    yield return new WaitForSeconds(0.2f);
                    BattleGround.Round.PostHeroTurn();
                    yield break;
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
                    yield return new WaitForSeconds(0.5f);
                    break;
                #endregion
                case StartTurnActType.IgnoreCommand:
                    #region Ignore
                    yield return new WaitForSeconds(1f);
                    RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Pass);
                    yield return new WaitForSeconds(0.8f);
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
                    #region Random Command
                    yield return new WaitForSeconds(1f);
                    var brainDesicion = BattleSolver.UseMonsterBrain(actionUnit);
                    Formations.ShowUnitOverlay();
                    BattleGround.Round.OnHeroTurn();
                    yield return new WaitForSeconds(0.2f);
                    if (brainDesicion.Decision == BrainDecisionType.Pass)
                    {
                        Formations.heroes.overlay.ResetSelectionsExcept(actionUnit);
                        RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Pass);
                        yield return new WaitForSeconds(0.6f);
                        yield break;
                    }
                    else
                    {
                        if (brainDesicion.TargetInfo.Targets.Count > 0)
                        {
                            SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(actionUnit, brainDesicion.TargetInfo.Targets[0],
                                brainDesicion.SelectedSkill).UpdateSkillInfo(actionUnit, brainDesicion.SelectedSkill);
                            yield return StartCoroutine(ExecuteHeroSkill(actionUnit, targetInfo, brainDesicion.SelectedSkill));
                        }

                        if (brainDesicion.SelectedSkill.IsContinueTurn)
                        {
                            Formations.ResetSelections();
                            yield return new WaitForEndOfFrame();
                            BattleGround.Round.PreHeroTurn(actionUnit);
                            yield return new WaitForEndOfFrame();
                            actionUnit.OverlaySlot.UnitSelected();
                        }
                        else
                            yield break;
                    }
                    break;
                #endregion
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
            }
            else
            {
                RaidPanel.SetDisabledState();
                Inventory.SetDeactivated();
            }

            #region Hero Action
            QuestPanel.UpdateCombatRetreat(true);
            while (BattleGround.Round.HeroAction == HeroTurnAction.Waiting)
            {
                yield return null;
            }
            QuestPanel.UpdateCombatRetreat(false);
            while (IsUnitEventInProgress)
                yield return null;

            Inventory.SetDeactivated();
            var targetUnit = BattleGround.Round.SelectedTarget;
            var usedSkill = RaidPanel.bannerPanel.skillPanel.SelectedSkill;
            RaidPanel.SetDisabledState();

            switch (BattleGround.Round.HeroAction)
            {
                #region Move
                case HeroTurnAction.Move:
                    if (usedSkill is MoveSkill)
                    {
                        #region Trait Block
                        if (targetUnit.Character.Trait != null)
                        {
                            if (RandomSolver.CheckSuccess(targetUnit.Character.Trait.Reactions[ReactionType.BlockMove].Chance))
                            {
                                actionUnit.CombatInfo.BlockedMoveUnitIds.Add(targetUnit.CombatInfo.CombatId);
                                yield return new WaitForSeconds(1f);
                                Formations.ResetSelections();
                                yield return new WaitForEndOfFrame();
                                BattleGround.Round.PreHeroTurn(actionUnit);
                                yield return new WaitForEndOfFrame();
                                actionUnit.OverlaySlot.UnitSelected();
                                usedSkill = null;
                                continue;
                            }
                        }
                        #endregion

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

                        #region Trait Block
                        if (targetInfo.Type != SkillTargetType.Enemy)
                        {
                            if (usedCombatSkill.Heal != null)
                            {
                                bool blockedHeal = false;
                                for (int i = targetInfo.Targets.Count - 1; i >= 0; i--)
                                {
                                    if (targetInfo.Targets[i].Character.Trait != null)
                                    {
                                        if (RandomSolver.CheckSuccess(targetInfo.Targets[i].
                                            Character.Trait.Reactions[ReactionType.BlockHeal].Chance))
                                        {
                                            actionUnit.CombatInfo.BlockedHealUnitIds.Add(targetInfo.Targets[i].CombatInfo.CombatId);
                                            targetInfo.Targets.RemoveAt(i);
                                            blockedHeal = true;
                                        }
                                    }
                                }
                                if (blockedHeal)
                                    yield return new WaitForSeconds(1f);
                            }
                            else if (usedCombatSkill.Effects.Find(effect => effect.SubEffects.Find(subEffect =>
                                subEffect.Type == EffectSubType.Buff || subEffect.Type == EffectSubType.StatBuff) != null) != null)
                            {
                                bool blockedBuff = false;
                                for (int i = targetInfo.Targets.Count - 1; i >= 0; i--)
                                {
                                    if (targetInfo.Targets[i].Character.Trait != null)
                                    {
                                        if (RandomSolver.CheckSuccess(targetInfo.Targets[i].
                                            Character.Trait.Reactions[ReactionType.BlockBuff].Chance))
                                        {
                                            actionUnit.CombatInfo.BlockedBuffUnitIds.Add(targetInfo.Targets[i].CombatInfo.CombatId);
                                            targetInfo.Targets.RemoveAt(i);
                                            blockedBuff = true;
                                        }
                                    }
                                }
                                if (blockedBuff)
                                    yield return new WaitForSeconds(1f);
                            }
                        }
                        if (targetInfo.Targets.Count == 0)
                        {
                            Formations.ResetSelections();
                            yield return new WaitForEndOfFrame();
                            BattleGround.Round.PreHeroTurn(actionUnit);
                            yield return new WaitForEndOfFrame();
                            actionUnit.OverlaySlot.UnitSelected();
                            usedSkill = null;
                            continue;
                        }
                        #endregion

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
            }
            break;
            #endregion
        }

        BattleGround.Round.PostHeroTurn();
    }
    protected override IEnumerator MonsterTurn(FormationUnit actionUnit)
    {
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
        PhotonGameManager.Instanse.photonView.RPC("HeroSkillSelected", PhotonTargets.Others, slotIndex);
        HeroSkillSelected(slotIndex);
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
        PhotonGameManager.Instanse.photonView.RPC("HeroMoveSelected", PhotonTargets.Others);
        HeroMoveSelected();
    }
    public void HeroMoveSelected()
    {
        MoveSkillSlot skillSlot = RaidPanel.bannerPanel.skillPanel.moveSlot;

        if (BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            Formations.monsters.overlay.ResetSelections();
            for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
            {
                if (Formations.heroes.party.Units[i] == BattleGround.Round.SelectedUnit)
                    BattleGround.Round.SelectedUnit.SetPerformerStatus();
                else
                {
                    int distance = BattleGround.Round.SelectedUnit.Rank - Formations.heroes.party.Units[i].Rank;
                    if (BattleGround.Round.SelectedUnit.CombatInfo.BlockedMoveUnitIds.
                        Contains(Formations.heroes.party.Units[i].CombatInfo.CombatId))
                    {
                        Formations.heroes.party.Units[i].SetDeactivatedStatus();
                    }
                    else if (distance < 0)
                    {
                        if (skillSlot.Skill.MoveBackward >= -distance && !Formations.heroes.party.Units[i].CombatInfo.IsImmobilized)
                            Formations.heroes.party.Units[i].SetMoveTargetStatus(true);
                        else
                            Formations.heroes.party.Units[i].SetDeactivatedStatus();
                    }
                    else
                    {
                        if (skillSlot.Skill.MoveForward >= distance && !Formations.heroes.party.Units[i].CombatInfo.IsImmobilized)
                            Formations.heroes.party.Units[i].SetMoveTargetStatus(true);
                        else
                            Formations.heroes.party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
            {
                if (Formations.heroes.party.Units[i] == RaidPanel.SelectedUnit)
                    RaidPanel.SelectedUnit.SetPerformerStatus();
                else
                    Formations.heroes.party.Units[i].SetMoveTargetStatus(true);
            }
        }
    }

    public override void HeroMoveDeselected(MoveSkillSlot skillSlot)
    {
        PhotonGameManager.Instanse.photonView.RPC("HeroMoveDeselected", PhotonTargets.Others);
        HeroMoveDeselected();
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
