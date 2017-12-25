using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum DungeonSceneState { Room, Hall }
public enum StartingMode { Normal, EntranceEncounter, EntranceCurio }
public enum RoomTransitionType { Entrance, FromHallway, PeacefulLoad, CombatLoad, Retreat, Teleport }
public enum HallTransitionType { FromRoom, PeacefulLoad, CombatLoad, Retreat }

public class RaidSceneManager : MonoBehaviour
{
    public static RaidSceneManager Instanse { get; protected set; }

    #region Raid References

    [SerializeField]
    private StartingMode startingMode;
    [SerializeField]
    private List<string> startingItems;

    [SerializeField]
    private RaidPartyCamera dungeonCamera;
    [SerializeField]
    private RaidHallwayView hallwayView;
    [SerializeField]
    private RaidRoomView roomView;
    [SerializeField]
    private BattleGround battleGround;
    [SerializeField]
    private RaidPartyController partyController;
    [SerializeField]
    private RaidInterface raidInterface;
    [SerializeField]
    private RaidEvents raidEvents;
    [SerializeField]
    private PartyFormationManager formations;
    [SerializeField]
    private RaidResultWindow resultWindow;
    [SerializeField]
    private QuestCompletionWindow completionWindow;
    [SerializeField]
    private TorchMeter torchMeter;
    [SerializeField]
    private CampController campController;
    [SerializeField]
    private CharacterWindow characterWindow;
    [SerializeField]
    private Button escapeButton;

    public static RaidInfo Raid { get { return Instanse.CurrentRaid; } }
    public static RaidPartyController PartyController { get { return Instanse.partyController; } }
    public static RaidPartyCamera DungeonCamera { get { return Instanse.dungeonCamera; } }
    public static RaidInterface RaidInterface { get { return Instanse.raidInterface; } }
    public static RaidEvents RaidEvents { get { return Instanse.raidEvents; } }
    public static PartyFormationManager Formations { get { return Instanse.formations; } }
    public static RaidMapPanel MapPanel { get { return Instanse.raidInterface.RaidPanel.MapPanel; } }
    public static PartyInventory Inventory { get { return Instanse.raidInterface.RaidPanel.InventoryPanel.PartyInventory; } }
    public static RaidPanel RaidPanel { get { return Instanse.raidInterface.RaidPanel; } }
    public static RaidHeroPanel HeroPanel { get { return Instanse.raidInterface.RaidPanel.HeroPanel; } }
    public static RaidQuestPanel QuestPanel { get { return Instanse.raidInterface.QuestPanel; } }
    public static RaidRoomView RoomView { get { return Instanse.roomView; } }
    public static RaidHallwayView HallwayView { get { return Instanse.hallwayView; } }
    public static TorchMeter TorchMeter { get { return Instanse.torchMeter; } }
    public static CampController CampController { get { return Instanse.campController; } }
    public static CharacterWindow CharacterWindow { get { return Instanse.characterWindow; } }
    public static BattleGround BattleGround { get { return Instanse.battleGround; } }
    public static FormationParty HeroParty { get { return Instanse.formations.Heroes.Party; } }
    public static QuestCompletionWindow CompletionWindow { get { return Instanse.completionWindow; } }
    public static Button EscapeButton { get { return Instanse.escapeButton; } }

    public static DungeonSceneState SceneState
    {
        get { return Instanse.sceneState; }
        set { Instanse.sceneState = value; }
    }

    #endregion

    #region Raid Info and Events

    public static bool HasAnyEvents
    {
        get { return IsUnitEventInProgress || Instanse.CurrentEvent != null; }
    }

    public static bool IsUnitEventInProgress
    {
        get { return Instanse.executingEffectEvent || Instanse.itemUsageEvent != null || Instanse.roundAdvanceCounter != 0; }
    }

    public static bool AnyWindowOpened
    {
        get { return DarkestDungeonManager.MainMenu.IsOpened || CharacterWindow.IsOpened; }
    }

    public static RaidRuleInfo Rules { get; protected set; }

    protected RaidInfo CurrentRaid;
    protected IEnumerator CurrentEvent;
    protected readonly List<FormationUnit> UnitEventQueue = new List<FormationUnit>(8);
    protected readonly List<FormationUnit> ResolveCheckQueue = new List<FormationUnit>(4);
    protected readonly List<FormationUnit> HeartAttackCheckQueue = new List<FormationUnit>(4);
    protected readonly List<FormationUnit> DeathDoorEnterQueue = new List<FormationUnit>(4);
    protected readonly List<FormationUnit> TempList = new List<FormationUnit>(4);
    protected readonly WaitForSeconds WaitForZeroThree = new WaitForSeconds(0.3f);
    protected readonly WaitForSeconds WaitForOneTwo = new WaitForSeconds(1.2f);
    protected readonly List<FormationUnit> Riposters = new List<FormationUnit>();
    protected readonly List<SkillResult> RiposteResults = new List<SkillResult>();

    private readonly List<FormationUnit> bleedDeaths = new List<FormationUnit>(4);
    private readonly List<FormationUnit> poisonDeaths = new List<FormationUnit>(4);
    private readonly List<CampEffect> campEffects = new List<CampEffect>(10);
    private readonly List<CampEffect> chosenEffects = new List<CampEffect>(2);
    private readonly Dictionary<DungeonRoom, int> currentScoutedRooms = new Dictionary<DungeonRoom, int>();

    private IEnumerator itemUsageEvent;
    private DungeonSceneState sceneState;
    private bool executingEffectEvent;
    private int scoutingCounter;
    private int roundAdvanceCounter;

    #endregion

    protected virtual void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;

#if !(UNITY_ANDROID || UNITY_IOS)
            EscapeButton.gameObject.SetActive(false);
#endif

            if(DarkestDungeonManager.SaveData.InRaid)
            {
                CurrentRaid = new RaidInfo(DarkestDungeonManager.SaveData);
                DarkestDungeonManager.RaidManager.Quest = CurrentRaid.Quest;
                DarkestDungeonManager.RaidManager.RaidParty = CurrentRaid.RaidParty;
            }
            else
            {
                CurrentRaid = new RaidInfo();
                CurrentRaid.Quest = DarkestDungeonManager.Instanse.RaidingManager.Quest;
                if(CurrentRaid.Quest.IsPlotQuest && (CurrentRaid.Quest as PlotQuest).RaidMap != null)
                    CurrentRaid.Dungeon = SaveLoadManager.LoadDungeonMap((CurrentRaid.Quest as PlotQuest).RaidMap, CurrentRaid.Quest);
                else
                    CurrentRaid.Dungeon = DungeonGenerator.GenerateDungeon(CurrentRaid.Quest);
                CurrentRaid.RaidParty = DarkestDungeonManager.RaidManager.RaidParty;

                if (CurrentRaid.Quest.IsPlotQuest)
                    DarkestSoundManager.ExecuteNarration("quest_start", NarrationPlace.Raid, CurrentRaid.Quest.Id);
                else
                    DarkestSoundManager.ExecuteNarration("quest_start", NarrationPlace.Raid, 
                        CurrentRaid.Quest.Type, CurrentRaid.Quest.Dungeon);
            }

            UpdateExtraStackLimit();

            DarkestDungeonManager.Data.LoadDungeon(CurrentRaid.Quest.Dungeon, CurrentRaid.Quest.Id);
            Rules = new RaidRuleInfo(CurrentRaid.Quest.Dungeon, BattleGround, TorchMeter);
            RaidEvents.Initialize();
        }
        else
            Destroy(Instanse.gameObject);
    }

    protected virtual void Start()
    {
        if (Instanse != this)
            return;

        CharacterWindow.EventWindowClosed += CharacterWindowClosed;
        CharacterWindow.EventNextButtonClicked += CharacterWindowNextButtonClicked;
        CharacterWindow.EventPreviousButtonClick += CharacterWindowPreviousButtonClicked;
        
        if (DarkestDungeonManager.SaveData.InRaid)
        {
            RaidInterface.UpdateRaidScene();
            MapPanel.LoadDungeon(CurrentRaid.Dungeon);
            Inventory.LoadItems(DarkestDungeonManager.SaveData.InventoryItems);
#if UNITY_EDITOR
            Inventory.DistributeItem(new ItemDefinition("supply", "holy_water", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "shovel", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "medicinal_herbs", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "antivenom", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "bandage", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "torch", 12));
            Inventory.DistributeItem(new ItemDefinition("provision", "", 12));
            Inventory.DistributeItem(new ItemDefinition("supply", "firewood", 2));
#endif
            Inventory.SetDeactivated();
            RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();
            RaidPanel.BannerPanel.SkillPanel.SetMode(SkillPanelMode.Combat);
            RaidPanel.BannerPanel.SetPeacefulState();

            QuestPanel.UpdateQuest(CurrentRaid.Quest, DarkestDungeonManager.SaveData.QuestCompleted);
            Formations.Initialize(DarkestDungeonManager.SaveData.HeroFormationData);
            DarkestSoundManager.StartDungeonSoundtrack(CurrentRaid.Dungeon.Name);

            if (CurrentRaid.CurrentLocation is DungeonRoom)
                CurrentEvent = RoomLoadingEvent(CurrentRaid.CurrentLocation as DungeonRoom, 
                    DarkestDungeonManager.SaveData.InBattle ? RoomTransitionType.CombatLoad : RoomTransitionType.PeacefulLoad);
            else
                CurrentEvent = HallwayLoadingEvent(CurrentRaid.CurrentLocation as HallSector,
                    DarkestDungeonManager.SaveData.InBattle ? HallTransitionType.CombatLoad : HallTransitionType.PeacefulLoad,
                    CurrentRaid.RaidParty.IsMovingLeft ? Direction.Left : Direction.Right);

            TorchMeter.Initialize(DarkestDungeonManager.SaveData.TorchAmount);

            if (DarkestDungeonManager.SaveData.ModifiedMinTorch != -1)
                TorchMeter.Modify(new TorchlightModifier(DarkestDungeonManager.SaveData.ModifiedMinTorch,
                    DarkestDungeonManager.SaveData.ModifiedMaxTorch));

            StartCoroutine(CurrentEvent);
        }
        else
        {
            RaidInterface.UpdateRaidScene();
            MapPanel.LoadDungeon(CurrentRaid.Dungeon);
            Inventory.LoadItems(DarkestDungeonManager.RaidManager.InventorySlotData);
#if UNITY_EDITOR
            Inventory.DistributeItem(new ItemDefinition("supply", "holy_water", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "medicinal_herbs", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "antivenom", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "bandage", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "shovel", 4));
            Inventory.DistributeItem(new ItemDefinition("supply", "torch", 12));
            Inventory.DistributeItem(new ItemDefinition("provision", "", 12));
            Inventory.DistributeItem(new ItemDefinition("supply", "firewood", 2));
#endif
            Inventory.SetDeactivated();
            RaidPanel.BannerPanel.SkillPanel.SetMode(SkillPanelMode.Combat);
            RaidPanel.BannerPanel.SetPeacefulState();
            QuestPanel.UpdateQuest(CurrentRaid.Quest);
            Formations.Initialize();
            DarkestSoundManager.StartDungeonSoundtrack(CurrentRaid.Dungeon.Name);

            if (startingMode == StartingMode.EntranceEncounter)
            {
                if (startingItems.Count > 0)
                {
                    CurrentRaid.Dungeon.StartingRoom.BattleEncounter = new BattleEncounter(startingItems);
                    CurrentRaid.Dungeon.StartingRoom.Type = AreaType.Battle;
                }
            }
            else if (startingMode == StartingMode.EntranceCurio)
            {
                if (startingItems.Count > 0)
                {
                    CurrentRaid.Dungeon.StartingRoom.Prop = DarkestDungeonManager.Data.Curios[startingItems[0]];
                    CurrentRaid.Dungeon.StartingRoom.Type = AreaType.Curio;
                }
            }

            TorchMeter.Initialize(100);

            CurrentEvent = RoomLoadingEvent(CurrentRaid.Dungeon.StartingRoom, RoomTransitionType.Entrance);
            StartCoroutine(CurrentEvent);
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            OnEscapePressed();

        if (DarkestDungeonManager.GamePaused || AnyWindowOpened)
            return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (CurrentRaid.CurrentLocation != null && sceneState == DungeonSceneState.Hall && CurrentEvent == null &&
                HallwayView.CurrentSector.Area.Type == AreaType.Obstacle && !PartyController.ForwardMovementAllowed)
            {
                if (!(HallwayView.CurrentSector.Prop as RaidObstacle).Removed)
                {
                    EncounterObstacle(HallwayView.CurrentSector);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            if (BattleGround.BattleStatus == BattleStatus.Peace && CurrentRaid.CurrentLocation != null)
            {
                if(sceneState == DungeonSceneState.Hall)
                {
                    if (HallwayView.CurrentSector.Area.Type == AreaType.Curio)
                    {
                        if (!(HallwayView.CurrentSector.Prop as RaidCurio).Investigated)
                        {
                            ActivateCurio(HallwayView.CurrentSector);
                        }
                    }
                    else if (HallwayView.CurrentSector.Area.Type == AreaType.Obstacle)
                    {
                        if (!(HallwayView.CurrentSector.Prop as RaidObstacle).Removed)
                        {
                            EncounterObstacle(HallwayView.CurrentSector);
                        }
                    }
                    else if (HallwayView.CurrentSector.Area.Type == AreaType.Door)
                        ActivateDoor(HallwayView.CurrentSector);
                }  
                else if (sceneState == DungeonSceneState.Room)
                {
                    if (RoomView.RaidRoom.Area.Type == AreaType.BattleCurio
                        || RoomView.RaidRoom.Area.Type == AreaType.BattleTresure)
                    {
                        if (!(RoomView.RaidRoom.Prop as RaidCurio).Investigated)
                        {
                            ActivateCurio(RoomView.RaidRoom);
                        }
                    }
                }
            }
        }

        if(sceneState == DungeonSceneState.Room && CurrentEvent == null)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                Door door = (RoomView.RaidRoom.Area as DungeonRoom).Doors.Find(item => item.Direction == Direction.Top);
                if (door != null)
                {
                    CurrentEvent = HallwayLoadingEvent(CurrentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Top, RoomView.RaidRoom.Area as DungeonRoom);
                    StartCoroutine(CurrentEvent);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Door door = (RoomView.RaidRoom.Area as DungeonRoom).Doors.Find(item => item.Direction == Direction.Bot);
                if (door != null)
                {
                    CurrentEvent = HallwayLoadingEvent(CurrentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Bot, RoomView.RaidRoom.Area as DungeonRoom);
                    StartCoroutine(CurrentEvent);
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Door door = (RoomView.RaidRoom.Area as DungeonRoom).Doors.Find(item => item.Direction == Direction.Left);
                if (door != null)
                {
                    CurrentEvent = HallwayLoadingEvent(CurrentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Left, RoomView.RaidRoom.Area as DungeonRoom);
                    StartCoroutine(CurrentEvent);
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Door door = (RoomView.RaidRoom.Area as DungeonRoom).Doors.Find(item => item.Direction == Direction.Right);
                if (door != null)
                {
                    CurrentEvent = HallwayLoadingEvent(CurrentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Right, RoomView.RaidRoom.Area as DungeonRoom);
                    StartCoroutine(CurrentEvent);
                }
            }
        }
    }

    #region Transitions

    public static Vector3 DungeonPositionToScreen(Vector3 position)
    {
        Vector3 screenPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(RaidInterface.OverlayRect,
            RectTransformUtility.WorldToScreenPoint(DungeonCamera.Camera, position),
            RaidInterface.OverlayCamera, out screenPoint);
        return screenPoint;
    }

    protected virtual IEnumerator ExecuteCampEffect(CampEffect currentEffect, FormationUnit target, bool skipNotification)
    {
        switch (currentEffect.Type)
        {
            case CampEffectType.Buff:
                var campBuff = DarkestDungeonManager.Data.Buffs[currentEffect.Subtype];
                target.Character.AddBuff(new BuffInfo(campBuff, currentEffect.Amount, BuffSourceType.Adventure));
                if(!skipNotification)
                    RaidEvents.ShowPopupMessage(target, PopupMessageType.Buff);
                break;
            case CampEffectType.HealthHealMaxHealthPercent:
                int heal = target.Character.HealPercent(currentEffect.Amount, true);
                RaidEvents.ShowPopupMessage(target, PopupMessageType.Heal, heal.ToString());
                target.OverlaySlot.UpdateOverlay();
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                break;
            case CampEffectType.Loot:
                RaidEvents.LoadSingleLoot(currentEffect.Subtype, (int)currentEffect.Amount);
                if (RaidEvents.LootEvent.HasSomething)
                    yield return StartCoroutine(LootEvent());
                break;
            case CampEffectType.ReduceAmbushChance:
                Raid.NightAmbushReduced = currentEffect.Amount;
                break;
            case CampEffectType.ReduceTorch:
                TorchMeter.DecreaseTorch((int)currentEffect.Amount);
                break;
            case CampEffectType.RemoveBleed:
            case CampEffectType.RemovePoison:
                if (currentEffect.Type == CampEffectType.RemoveBleed && target.Character[StatusType.Bleeding].IsApplied)
                {
                    target.Character[StatusType.Bleeding].ResetStatus();
                    if (!skipNotification)
                        RaidEvents.ShowPopupMessage(target, PopupMessageType.Cured);
                }
                else if (currentEffect.Type == CampEffectType.RemovePoison && target.Character[StatusType.Poison].IsApplied)
                {
                    target.Character[StatusType.Poison].ResetStatus();
                    if (!skipNotification)
                        RaidEvents.ShowPopupMessage(target, PopupMessageType.Cured);
                }
                break;
            case CampEffectType.RemoveDeathRecovery:
                target.Character[StatusType.DeathRecovery].ResetStatus();
                target.OverlaySlot.UpdateOverlay();
                break;
            case CampEffectType.RemoveDisease:
                if (currentEffect.Type == CampEffectType.RemoveDisease)
                {
                    var heroWithDisease = target.Character as Hero;
                    if (heroWithDisease.Diseases.Count > 0)
                    {
                        var disease = heroWithDisease.RemoveDiseaseQuirk();
                        if (disease != null)
                            RaidEvents.ShowPopupMessage(target, PopupMessageType.DiseaseCured,
                                LocalizationManager.GetString("str_quirk_name_" + disease.Id));
                    }
                }
                break;
            case CampEffectType.StressDamageAmount:
                float initialDamage = currentEffect.Amount;

                int damage = Mathf.RoundToInt(initialDamage * (1 +
                        target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
                if (damage < 1) damage = 1;

                target.Character.Stress.IncreaseValue(damage);
                if (target.Character.IsOverstressed)
                {
                    if (target.Character.IsVirtued)
                        target.Character.Stress.CurrentValue = Mathf.Clamp(target.Character.Stress.CurrentValue, 0, 100);
                    else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                        AddResolveCheck(target);

                    if (target.Character.Stress.CurrentValue == 200)
                        AddHeartAttackCheck(target);
                }

                target.OverlaySlot.UpdateOverlay();
                RaidEvents.ShowPopupMessage(target, PopupMessageType.Stress, damage.ToString());
                target.SetHalo("afflicted");
                yield return new WaitForSeconds(0.2f);
                break;
            case CampEffectType.StressHealAmount:
                float initialStressHeal = currentEffect.Amount;
                var hero = target.Character as Hero;
                int stressHeal = Mathf.RoundToInt(initialStressHeal * (1 +
                        target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
                if (stressHeal < 1) stressHeal = 1;

                target.Character.Stress.DecreaseValue(stressHeal);
                if (Mathf.RoundToInt(hero.Stress.CurrentValue) == 0 && hero.IsAfflicted)
                    hero.RevertTrait();
                target.OverlaySlot.UpdateOverlay();
                RaidEvents.ShowPopupMessage(target, PopupMessageType.StressHeal, stressHeal.ToString());
                target.SetHalo("heroic");
                yield return new WaitForSeconds(0.2f);
                break;
        }
    }

    protected virtual IEnumerator ExecuteCampEffectGroup(bool allowSkipping, float waitTime, List<FormationUnit> targets,  Predicate<CampEffectType> nextTypeSelector)
    {
        bool skipNotification = false;
        CampEffect currentEffect = null;

        while (true)
        {
            for (int j = 0; j < targets.Count; j++)
            {
                currentEffect = RandomSolver.ChooseBySingleRandom(chosenEffects);

                if (chosenEffects.Count == 1 && chosenEffects[0].Chance != 1)
                    if (!RandomSolver.CheckSuccess(chosenEffects[0].Chance))
                        continue;

                if (!BattleSolver.IsRequirementFulfilled(targets[j], currentEffect.Requirement))
                    continue;

                yield return ExecuteCampEffect(currentEffect, targets[j], skipNotification);
            }

            if (nextTypeSelector == null)
                break;

            currentEffect = campEffects.Find(effect => nextTypeSelector(effect.Type) 
                && effect.Selection == currentEffect.Selection && effect.Chance == 1);

            if (currentEffect != null)
            {
                campEffects.Remove(currentEffect);
                chosenEffects.Clear();
                chosenEffects.Add(currentEffect);
                if(allowSkipping)
                    skipNotification = true;
            }
            else
            {
                if (campEffects.Count == 0 || chosenEffects[0].Selection.IsWaitingForNext(campEffects[0].Selection))
                    yield return new WaitForSeconds(waitTime);

                break;
            }
        }
    }

    protected virtual IEnumerator HallwayLoadingEvent(HallSector hallSector, HallTransitionType transitionType, Direction direction, DungeonRoom fromRoom = null)
    {
        #region Set restrictions
        QuestPanel.DisableRetreat(false);
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        Formations.LockSelections();
        RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();
        Formations.HideHeroOverlay();
        #endregion

        #region If from room : fade screen
        if (transitionType == HallTransitionType.FromRoom)
        {
            DarkestDungeonManager.ScreenFader.Fade();
            yield return new WaitForSeconds(1f);
        }
        else
            DarkestDungeonManager.ScreenFader.StartFaded();
        #endregion

        #region Switch hallway state
        SceneState = DungeonSceneState.Hall;

        RoomView.SetActive(false);
        HallwayView.SetActive(true);

        if (transitionType == HallTransitionType.FromRoom)
            Raid.ResetRoundSector(hallSector);
        hallwayView.LoadHallway(hallSector.Hallway, direction, fromRoom, transitionType == HallTransitionType.CombatLoad);

        yield return new WaitForEndOfFrame();
        PartyController.TranseferToPassage(HallwayView.HallwayPassage);
        PartyController.enabled = false;
        DisablePartyMovement();
        yield return new WaitForEndOfFrame();

        var targetRaidSector = HallwayView.RaidHallway.HallSectors.Find(raidSector => raidSector.Area == hallSector);

        if (transitionType == HallTransitionType.FromRoom)
        {
            Formations.TransferToHallway(hallwayView);
            PartyController.enabled = true;
            MapPanel.FocusTarget();
        }
        else
        {
            Formations.TransferToHallwaySector(HallwayView, targetRaidSector);
            HallwayView.CurrentSector = targetRaidSector;
            targetRaidSector.SetInside(true);

            if(transitionType == HallTransitionType.CombatLoad || transitionType == HallTransitionType.PeacefulLoad)
            {
                if (Raid.CurrentLocation is DungeonRoom)
                    MapPanel.SetCurrentIndicator((DungeonRoom)Raid.CurrentLocation);
                else
                {
                    MapPanel.SetCurrentIndicator(Raid.LastRoom);
                    if (Raid.CurrentLocation.Type != AreaType.Door)
                        MapPanel.SetCurrentIndicator(Raid.CurrentLocation as HallSector);
                }
            }

            yield return new WaitForEndOfFrame();
            MapPanel.InstantScaleTarget();
            yield return new WaitForEndOfFrame();
            MapPanel.InstantFocusTarget();
        }

        PartyController.enabled = true;

        for (int i = 0; i < HeroParty.Units.Count; i++)
            HeroParty.Units[i].OverlaySlot.RectTransform.pivot = new Vector2(0.5f, 0.1f);
        for (int i = 0; i < Formations.Monsters.Overlay.OverlaySlots.Count; i++)
            Formations.Monsters.Overlay.OverlaySlots[i].RectTransform.pivot = new Vector2(0.5f, 0.1f);
        #endregion

        #region Load combat save
        if (transitionType == HallTransitionType.CombatLoad)
        {
            CurrentEvent = LoadEncounterEvent(targetRaidSector);
            StartCoroutine(CurrentEvent);
            yield break;
        }
        #endregion

        #region Show dungeon
        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.1f);
        if(transitionType != HallTransitionType.Retreat)
            EnablePartyMovement();
        Formations.ShowHeroOverlay();
        yield return new WaitForSeconds(0.4f);
        #endregion

        #region Apply retreat effects
        if (transitionType == HallTransitionType.Retreat)
        {
            for (int i = 0; i < HeroParty.Units.Count; i++)
                DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(HeroParty.Units[i]);

            for (int i = 0; i < HeroParty.Units.Count; i++)
                yield return StartCoroutine(ExecuteRandomDialog(HeroParty.Units[i], "str_bark_increasingstress"));

            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return new WaitForSeconds(0.3f);
            yield return ProcessRaidFailure();
            EnablePartyMovement();
        }
        #endregion

        #region Remove restrictions
        QuestPanel.EnableRetreat();
        RaidPanel.SwitchBlocked = false;
        Formations.UnlockSelections();
        Formations.ShowHeroOverlay();
        Inventory.SetPeacefulState(false);
        RaidPanel.HeroPanel.EquipmentPanel.SetActive();
        CurrentEvent = null;
        #endregion
    }

    protected virtual IEnumerator RoomLoadingEvent(DungeonRoom room, RoomTransitionType transitionType, RaidHallSector fromRaidSector = null)
    {
        #region Set restrictions
        QuestPanel.DisableRetreat(false);
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        Formations.LockSelections();
        RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();
        #endregion

        #region From hallway : open and close doors
        if(transitionType == RoomTransitionType.FromHallway)
        {
            if (fromRaidSector != null)
            {
                RaidDoor raidDoor = fromRaidSector.Prop as RaidDoor;

                while (UnitEventQueue.Count > 0)
                    yield return null;

                fromRaidSector.LeaveSector();
                Formations.HideHeroOverlay();
                DarkestDungeonManager.ScreenFader.Fade();

                DungeonCamera.Zoom(30, 1);
                raidDoor.Open();
                DisablePartyMovement();
                yield return new WaitForSeconds(0.3f);
                for (int i = Formations.Heroes.Party.Units.Count - 1; i >= 0; i--)
                {
                    Formations.SendUnitIntoDoor(fromRaidSector, i);
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForSeconds(0.7f);

                for (int i = Formations.Heroes.Party.Units.Count - 1; i >= 0; i--)
                {
                    Formations.GetUnitOutOfDoor(fromRaidSector, i);
                }
                DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
                EnablePartyMovement();
            }
        }
        else
            DarkestDungeonManager.ScreenFader.StartFaded();
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

        if(transitionType == RoomTransitionType.FromHallway)
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
        
        #region Show dungeon
        DarkestDungeonManager.ScreenFader.Appear(2);
        #region Check for teleport actions
        if (transitionType == RoomTransitionType.Teleport && !room.HasActiveBattle)
        {
            if (RaidPanel.SelectedUnit != null)
                RaidPanel.SelectedUnit.OverlaySlot.UnitSelected();

            yield return StartCoroutine(ProcessTransformationsAfterBattle());

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

        RaidPanel.SwitchBlocked = false;
        if (fromRaidSector == null)
        {
            Formations.ShowHeroOverlay();
            yield return new WaitForEndOfFrame();
        }
        Formations.ShowHeroOverlay();
        #endregion

        #region Apply retreat effects
        if (transitionType == RoomTransitionType.Retreat)
        {
            for (int i = 0; i < HeroParty.Units.Count; i++)
                DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(HeroParty.Units[i]);

            for (int i = 0; i < HeroParty.Units.Count; i++)
                yield return StartCoroutine(ExecuteRandomDialog(HeroParty.Units[i], "str_bark_increasingstress"));

            yield return StartCoroutine(ExecuteEffectEvents(false, 0.3f));
            yield return ProcessRaidFailure();
        }
        EnablePartyMovement();
        #endregion

        #region Battle encounter and scouting
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

            if (Raid.Quest.IsScoutingEnabled && room.Knowledge == Knowledge.Hidden)
            {
                room.Knowledge = Knowledge.Visited;
                Raid.ExploredRoomCount++;
                MapPanel.UpdateArea(room);
                yield return StartCoroutine(ScoutingEvent(room));
            }
            else
            {
                if (room.Knowledge == Knowledge.Hidden || room.Knowledge == Knowledge.Scouted)
                {
                    room.Knowledge = Knowledge.Visited;
                    Raid.ExploredRoomCount++;
                    MapPanel.UpdateArea(room);
                }
            }

            if (!Raid.QuestCompleted && Raid.CheckQuestGoals())
                yield return StartCoroutine(CompletionCrestEvent());
        }
        #endregion

        #region Remove restrictions
        QuestPanel.EnableRetreat();
        MapPanel.ShowAvailableRooms(room);
        Inventory.SetPeacefulState(false);
        RaidPanel.HeroPanel.EquipmentPanel.SetActive();
        CurrentEvent = null;
        #endregion
    }

    protected virtual IEnumerator CampingEvent(DungeonRoom room)
    {
        if (SceneState != DungeonSceneState.Room)
            yield break;

        #region Transition To Camping

        DarkestSoundManager.StartCampingSoundtrack();
        DisableEnviroment();
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        RaidPanel.SetDisabledState();
        RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();
        MapPanel.HideAvailableRooms(RoomView.RaidRoom.Area as DungeonRoom);

        DarkestDungeonManager.ScreenFader.Fade();
        TorchMeter.Hide();
        TorchMeter.IncreaseTorch(100);
        QuestPanel.DisableRetreat(true);
        Formations.LockSelections();
        Formations.ResetSelections();
        yield return new WaitForSeconds(1f);
        DungeonCamera.Zoom(50, 0);
        DungeonCamera.Transform.Rotate(3, 0, 0);
        DungeonCamera.SetCampingLight();
        RaidPanel.BannerPanel.SkillPanel.SetMode(SkillPanelMode.Camping);
        RaidPanel.BannerPanel.SkillPanel.UpdateSkillPanel();
        RaidPanel.BannerPanel.SetDisabledState();
        DisablePartyMovement();
        yield return new WaitForSeconds(0.1f);

        foreach (var hero in Formations.Heroes.Party.Units)
            hero.SetCampingAnimation(true);
        CampController.SwitchCamping(true);
        yield return new WaitForSeconds(0.1f);

        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        DarkestSoundManager.ExecuteNarration("camp", NarrationPlace.Raid);

        #endregion

        #region Dialog Box Test

#if UNITY_EDITOR
        for (int barkLoops = 0; barkLoops < 2; barkLoops++)
        {
            for (int i = 0; i < HeroParty.Units.Count; i++)
            {
                HeroParty.Units[i].OverlaySlot.StartDialog("I'm gonna eat all the fucking chicken in this room!");
                while (HeroParty.Units[i].OverlaySlot.IsDoingDialog)
                    yield return null;
            }
        }
#endif

        #endregion

        #region Meal Phase

        RaidEvents.LoadCampingMeal();
        yield return new WaitForEndOfFrame();
        RaidEvents.MealEvent.Show();

        while (RaidEvents.MealEvent.MealResult == CampMealResultType.Wait)
            yield return null;

        Inventory.DiscardItemType("provision", RaidEvents.MealEvent.SelectedMealSlot.Amount);

        RaidEvents.MealEvent.Hide();
        yield return new WaitForSeconds(0.2f);
        RaidEvents.MealEvent.SetActive(false);

        switch (RaidEvents.MealEvent.SelectedMealSlot.FoodRank)
        {
            case 0:
                yield return StartCoroutine(ProcessStarvation());
                break;
            case 1:
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");
                yield return new WaitForSeconds(0.6f);
                break;
            case 2:
            case 3:
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");
                float healPercent = RaidEvents.MealEvent.SelectedMealSlot.FoodRank == 2 ? 0.1f : 0.25f;

                for (int i = 0; i < HeroParty.Units.Count; i++)
                {
                    int mealHeal = HeroParty.Units[i].Character.HealPercent(healPercent, false);
                    HeroParty.Units[i].OverlaySlot.UpdateOverlay();
                    RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Heal, mealHeal.ToString());

                    if (RaidEvents.MealEvent.SelectedMealSlot.FoodRank == 3)
                        DarkestDungeonManager.Data.Effects["HealSelfStress 1"].ApplyIndependent(HeroParty.Units[i], HeroParty.Units[i]);
                }

                if (RaidEvents.MealEvent.SelectedMealSlot.FoodRank == 3)
                    yield return new WaitForSeconds(0.2f);

                yield return StartCoroutine(ExecuteEffectEvents(false, 0.6f));
                yield return ProcessRaidFailure();
                break;
        }

        yield return new WaitForSeconds(0.1f);

        #endregion

        #region Skill Phase

        RaidEvents.LoadCampingSkillEvent();
        yield return new WaitForSeconds(0.1f);
        RaidEvents.CampEvent.Show();

        Formations.UnlockSelections();

        if (RaidPanel.SelectedUnit != null)
            RaidPanel.SelectedUnit.OverlaySlot.UnitSelected();
        else
            HeroParty.Units[0].OverlaySlot.UnitSelected();

        RaidPanel.BannerPanel.SetCombatReady();
        Raid.CampingPhase = CampingPhase.Skill;

        while (true)
        {
            if (RaidEvents.CampEvent.ActionType == CampUsageResultType.Wait)
                yield return null;
            else
            {
                if (RaidEvents.CampEvent.ActionType != CampUsageResultType.Skill)
                {
                    RaidEvents.CampEvent.Hide();
                    yield return new WaitForSeconds(0.2f);
                    RaidEvents.CampEvent.ScrollClosed();
                    break;
                }
                CampingSkill skill = RaidPanel.BannerPanel.SkillPanel.SelectedSkill as CampingSkill;

                RaidPanel.SelectedUnit.CombatInfo.SkillsUsedThisTurn.Add(skill.Id);
                RaidEvents.CampEvent.SpendTime(skill.TimeCost);
                Formations.ResetSelections();
                RaidPanel.SkillPanel.SetUsable();

                campEffects.Clear();
                campEffects.AddRange(skill.Effects);
                FMODUnity.RuntimeManager.PlayOneShot("event:/camp/skill/" + skill.Id, DungeonCamera.Transform.position);
                yield return new WaitForSeconds(0.5f);

                foreach (var damageEffect in campEffects.FindAll(effect => effect.Type == CampEffectType.HealthDamageMaxHealthPercent))
                {
                    campEffects.Remove(damageEffect);
                    BattleSolver.FindTargets(RaidPanel.SelectedUnit, RaidEvents.CampEvent.SelectedTarget, damageEffect, TempList);
                    TempList.ForEach(unit => ProcessDamage(unit, Mathf.RoundToInt(unit.Character.MaxHealth * 0.2f)));
                    yield return StartCoroutine(ProcessHeroDeaths(0.6f, 0.8f, 0.3f));
                }

                while (campEffects.Count > 0)
                {
                    chosenEffects.Clear();
                    chosenEffects.Add(campEffects[0]);
                    campEffects.RemoveAt(0);

                    if (chosenEffects[0].Chance != 1)
                    {
                        chosenEffects.AddRange(campEffects.FindAll(effect => effect.Code == chosenEffects[0].Code));
                        campEffects.RemoveAll(effect => effect.Code == chosenEffects[0].Code);
                    }
                    BattleSolver.FindTargets(RaidPanel.SelectedUnit, RaidEvents.CampEvent.SelectedTarget, chosenEffects[0], TempList);

                    switch (chosenEffects[0].Type)
                    {
                        case CampEffectType.Buff:
                        case CampEffectType.HealthHealMaxHealthPercent:
                        case CampEffectType.RemoveDisease:
                            yield return StartCoroutine(ExecuteCampEffectGroup(true, 0.6f, TempList, effectType => effectType == chosenEffects[0].Type));
                            break;
                        case CampEffectType.Loot:
                        case CampEffectType.ReduceAmbushChance:
                        case CampEffectType.ReduceTorch:
                        case CampEffectType.RemoveDeathRecovery:
                            yield return StartCoroutine(ExecuteCampEffectGroup(false, 0.0f, TempList, null));
                            break;
                        case CampEffectType.RemoveBleed:
                        case CampEffectType.RemovePoison:
                            yield return StartCoroutine(ExecuteCampEffectGroup(false, 0.6f, TempList, effectType => 
                                effectType == CampEffectType.RemoveBleed || effectType == CampEffectType.RemovePoison));
                            break;
                        case CampEffectType.StressDamageAmount:
                        case CampEffectType.StressHealAmount:
                            yield return StartCoroutine(ExecuteCampEffectGroup(false, 1.2f, TempList, effectType => effectType == chosenEffects[0].Type));
                            break;
                    }
                }
                RaidEvents.CampEvent.Show();

                Formations.HeroOverlay.UpdateOverlay();
                Formations.UnlockSelections();
                RaidEvents.CampEvent.SkillExecuted();
                RaidPanel.SelectedUnit.SetPerformerStatus();
            }
        }

        #endregion

        #region Transition From Camping

        DarkestSoundManager.StopCampingSoundtrack();
        Formations.ResetSelections();
        DarkestDungeonManager.ScreenFader.Fade();
        yield return new WaitForSeconds(1f);

        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0);
        DungeonCamera.Transform.Rotate(-3, 0, 0);
        DungeonCamera.SetRaidingLight(TorchMeter.CurrentRange.RangeType);
        RaidPanel.SkillPanel.SetMode(SkillPanelMode.Combat);
        RaidPanel.SkillPanel.UpdateSkillPanel();
        RaidPanel.SetDisabledState();
        TorchMeter.Show();

        HeroParty.Units.ForEach(hero => { hero.SetCampingAnimation(false); hero.DeleteTarget(0); });
        CampController.SwitchCamping(false);

        if(RandomSolver.CheckSuccess(0.5f - Raid.NightAmbushReduced))
        {
            var battleEncounter = RandomSolver.ChooseByRandom(Raid.Dungeon.DungeonMash.RoomEncounters);
            if(battleEncounter != null)
            {
                RaidPanel.SwitchBlocked = false;
                Raid.NightAmbushReduced = 0;
                room.BattleEncounter = new BattleEncounter(battleEncounter.MonsterSet);
                CurrentEvent = EncounterEvent(RoomView.RaidRoom, true);
                int torchBefore = TorchMeter.TorchAmount;
                TorchMeter.DecreaseTorch(100);
                DarkestDungeonManager.ScreenFader.Appear(2);
                yield return new WaitForSeconds(0.3f);
                yield return StartCoroutine(CurrentEvent);
                if (TorchMeter.TorchAmount < torchBefore)
                    TorchMeter.IncreaseTorch(torchBefore - TorchMeter.TorchAmount);
                yield break;
            }
        }

        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        QuestPanel.EnableRetreat();
        EnablePartyMovement();
        RaidPanel.SelectedUnit.SetPerformerStatus();

        MapPanel.ShowAvailableRooms(RoomView.RaidRoom.Area as DungeonRoom);
        Formations.UnlockSelections();
        EnableEnviroment();
        RaidPanel.SwitchBlocked = false;
        Inventory.SetPeacefulState(false);
        HeroPanel.EquipmentPanel.SetActive();
        RaidPanel.SetPeacefulState();
        CurrentEvent = null;
        Raid.NightAmbushReduced = 0;

        #endregion
    }

    protected virtual IEnumerator RaidResultsEvent()
    {
        RaidInterface.CanvasGroup.blocksRaycasts = false;

        if (Raid.QuestCompleted || Raid.CheckQuestGoals())
            DarkestDungeonManager.RaidManager.Status = RaidStatus.Success;
        else
        {
            DarkestDungeonManager.RaidManager.Status = Formations.Heroes.Party.Units.Count > 0 ?
                RaidStatus.Abandon : RaidStatus.Defeat;
        }
        ToolTipManager.Instanse.Hide();
        Formations.HideHeroOverlay();
        RoomView.DisableInteraction();
        HallwayView.DisableInteraction();
        DarkestDungeonManager.ScreenFader.Fade();
        yield return new WaitForSeconds(1f);
        RoomView.gameObject.SetActive(false);
        hallwayView.gameObject.SetActive(false);
        resultWindow.gameObject.SetActive(true);
        resultWindow.ProceedToItems();
        resultWindow.DisableInteraction();
        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        if (DarkestDungeonManager.RaidManager.Status == RaidStatus.Success)
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
        resultWindow.EnableInteraction();
    }

    protected virtual IEnumerator RaidResultsHeroTransition()
    {
        DarkestDungeonManager.ScreenFader.Fade();
        yield return new WaitForSeconds(1f);
        resultWindow.ProceedToHeroes();
        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        resultWindow.EnableInteraction();
    }

    protected virtual IEnumerator RaidResultsTownTransition()
    {
        DarkestDungeonManager.ScreenFader.Fade();
        DarkestSoundManager.StopDungeonSoundtrack();
        DarkestSoundManager.StopCampingSoundtrack();
        yield return new WaitForSeconds(1f);
        DarkestDungeonManager.ScreenFader.Appear(2);
        DarkestDungeonManager.LoadingInfo.SetNextScene("EstateManagement", "Screen/loading_screen.town_visit");
        SceneManager.LoadScene("LoadingScreen");
    }

    #endregion

    #region Encounters and Activations

    public void AdvanceThroughDungeon()
    {
        StartCoroutine(ExecuteRoundAdvance());
    }

    public void EncounterObstacle(RaidHallSector sector)
    {
        QuestPanel.DisableRetreat(false);
        DisableForwardPartyMovement();
        RaidEvents.LoadInteraction(sector.Area.Prop as Obstacle, sector);
    }

    public void LeaveObstacle(RaidHallSector sector)
    {
        QuestPanel.EnableRetreat();
        EnableForwardPartyMovement();
    }

    public void EncounterMonsters(IRaidArea areaView)
    {
        CurrentEvent = EncounterEvent(areaView);
        StartCoroutine(CurrentEvent);
    }

    public void ActivateCurio(IRaidArea area)
    {
        if (CurrentEvent != null)
            return;

        CurrentEvent = CurioEvent(area);
        StartCoroutine(CurrentEvent);
    }

    public void ActivateCurio(IRaidArea area, Quirk quirk)
    {
        if (CurrentEvent != null)
            return;

        CurrentEvent = CurioEvent(area, quirk);
        StartCoroutine(CurrentEvent);
    }

    public void ActivateCurio(IRaidArea area, Trait trait)
    {
        if (CurrentEvent != null)
            return;
        CurrentEvent = CurioEvent(area, null, trait);
        StartCoroutine(CurrentEvent);
    }

    public void ActivateHunger(RaidHallSector sector)
    {
        CurrentEvent = HungerEvent();
        StartCoroutine(CurrentEvent);
    }

    public void ActivateTrap(RaidHallSector sector, bool handActivation)
    {
        CurrentEvent = TrapEvent(sector, handActivation);
        StartCoroutine(CurrentEvent);
    }

    public void ActivateObstacle(RaidHallSector sector, bool handActivation)
    {
        CurrentEvent = ObstacleEvent(sector, handActivation);
        StartCoroutine(CurrentEvent);
    }

    public void ActivateDoor(RaidHallSector sector)
    {
        if (CurrentEvent != null)
            return;

        var door = sector.Area.Prop as Door;
        CurrentEvent = RoomLoadingEvent(CurrentRaid.Dungeon.Rooms[door.TargetArea], RoomTransitionType.FromHallway, sector);
        StartCoroutine(CurrentEvent);
    }

    #endregion

    #region Player Actions

    public void OnSceneLeave()
    {
        DarkestSoundManager.StopDungeonSoundtrack();
        DarkestSoundManager.StopCampingSoundtrack();
        DarkestSoundManager.StopBattleSoundtrack();
        ResetExtraStackLimit();
    }

    public void OnEscapePressed()
    {
        if (DarkestDungeonManager.MainMenu.gameObject.activeSelf)
            DarkestDungeonManager.MainMenu.WindowClosed();
        else
            DarkestDungeonManager.MainMenu.OpenMenu();
    }

    public virtual void NextRaidResultButtonClicked()
    {
        if (resultWindow.State == ResultWindowState.Items)
        {
            resultWindow.DisableInteraction();
            StartCoroutine(RaidResultsHeroTransition());
        }
        else if (resultWindow.State == ResultWindowState.Heroes)
        {
            resultWindow.DisableInteraction();
            StartCoroutine(RaidResultsTownTransition());
        }
    }

    public virtual void AbandonButtonClicked()
    {
        if (BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            if (BattleGround.Round.HeroAction == HeroTurnAction.Waiting && BattleGround.Round.TurnStatus == TurnStatus.Progress)
            {
                BattleGround.Round.HeroAction = HeroTurnAction.Retreat;
            }
        }
        else if (CurrentEvent == null)
        {
            StartCoroutine(RaidResultsEvent());
        }
    }

    public virtual void ReturnToEstateClicked()
    {
        StartCoroutine(RaidResultsEvent());
    }

    public void HeroCampingSkillSelected(BattleCampingSlot skillSlot)
    {
        Formations.LockSelections();

        for (int i = 0; i < skillSlot.Skill.Effects.Count; i++)
        {
            if (skillSlot.Skill.Effects[i].Selection == CampTargetType.Individual)
            {
                for (int j = 0; j < HeroParty.Units.Count; j++)
                {
                    if (RaidPanel.SelectedUnit != HeroParty.Units[j])
                        Formations.Heroes.Party.Units[j].SetFriendlyTargetStatus(true);
                }
                return;
            }
        }
        RaidEvents.CampEvent.ActionType = CampUsageResultType.Skill;
        Formations.ResetSelections();
    }

    public void HeroCampingSkillDeselected(BattleCampingSlot skillSlot)
    {
        Formations.UnlockSelections();
        Formations.Heroes.Overlay.ResetSelectionsExcept(RaidPanel.SelectedUnit);
    }

    public virtual void HeroSkillSelected(BattleSkillSlot skillSlot)
    {
        if (skillSlot.Skill.TargetRanks.IsSelfFormation || skillSlot.Skill.TargetRanks.IsSelfTarget)
        {
            Formations.Monsters.Overlay.ResetSelections();

            if(skillSlot.Skill.TargetRanks.IsSelfTarget)
            {
                BattleGround.Round.SelectedUnit.SetFriendlyPerformerStatus(true);
                Formations.Heroes.Overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            }
            else
            {
                for (int i = 0; i < Formations.Heroes.Party.Units.Count; i++)
                {
                    if(Formations.Heroes.Party.Units[i] == BattleGround.Round.SelectedUnit)
                    {
                        if(skillSlot.Skill.IsSelfValid)
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
                            BlockedHealUnitIds.Contains(Formations.Heroes.Party.Units[i].CombatInfo.CombatId))
                                Formations.Heroes.Party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.IsBuffSkill && BattleGround.Round.SelectedUnit.CombatInfo.
                            BlockedBuffUnitIds.Contains(Formations.Heroes.Party.Units[i].CombatInfo.CombatId))
                                Formations.Heroes.Party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.TargetRanks.IsTargetableUnit(Formations.Heroes.Party.Units[i]))
                            Formations.Heroes.Party.Units[i].SetFriendlyTargetStatus(true);
                        else
                            Formations.Heroes.Party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
        else
        {
            Formations.Heroes.Overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            TempList.Clear();

            for (int i = 0; i < Formations.Monsters.Party.Units.Count; i++)
            {
                if (skillSlot.Skill.TargetRanks.IsTargetableUnit(Formations.Monsters.Party.Units[i]))
                    TempList.Add(Formations.Monsters.Party.Units[i]);
                else
                    Formations.Monsters.Party.Units[i].SetDeactivatedStatus();
            }

            if (skillSlot.Skill.TargetRanks.IsMultitarget && TempList.Count > 0)
            {
                for (int i = 0; i < TempList.Count; i++)
                    TempList[i].SetEnemyTargetStatus(true, i != TempList.Count - 1);
            }
            else
                foreach (var target in TempList)
                    target.SetEnemyTargetStatus(true, false);

            TempList.Clear();
        }
    }

    public virtual void HeroMoveSelected(MoveSkillSlot skillSlot)
    {
        if (BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            Formations.Monsters.Overlay.ResetSelections();
            for (int i = 0; i < Formations.Heroes.Party.Units.Count; i++)
            {
                if (Formations.Heroes.Party.Units[i] == BattleGround.Round.SelectedUnit)
                    BattleGround.Round.SelectedUnit.SetPerformerStatus();
                else
                {
                    int distance = BattleGround.Round.SelectedUnit.Rank - Formations.Heroes.Party.Units[i].Rank;
                    if(BattleGround.Round.SelectedUnit.CombatInfo.BlockedMoveUnitIds.
                        Contains(Formations.Heroes.Party.Units[i].CombatInfo.CombatId))
                    {
                        Formations.Heroes.Party.Units[i].SetDeactivatedStatus();
                    }
                    else if(distance < 0)
                    {
                        if (skillSlot.Skill.MoveBackward >= -distance && !Formations.Heroes.Party.Units[i].CombatInfo.IsImmobilized)
                            Formations.Heroes.Party.Units[i].SetMoveTargetStatus(true);
                        else
                            Formations.Heroes.Party.Units[i].SetDeactivatedStatus();
                    }
                    else
                    {
                        if (skillSlot.Skill.MoveForward >= distance && !Formations.Heroes.Party.Units[i].CombatInfo.IsImmobilized)
                            Formations.Heroes.Party.Units[i].SetMoveTargetStatus(true);
                        else
                            Formations.Heroes.Party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < Formations.Heroes.Party.Units.Count; i++)
            {
                if (Formations.Heroes.Party.Units[i] == RaidPanel.SelectedUnit)
                    RaidPanel.SelectedUnit.SetPerformerStatus();
                else
                    Formations.Heroes.Party.Units[i].SetMoveTargetStatus(true);
            }
        }
    }

    public virtual void HeroMoveDeselected(MoveSkillSlot skillSlot)
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

    public void HeroItemActivated(InventorySlot slot)
    {
        if (itemUsageEvent != null)
            return;

        if (BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            if (BattleGround.Round.SelectedUnit == null && BattleGround.Round.SelectedUnit.Team == Team.Monsters)
                return;
            if (BattleGround.Round.HeroAction != HeroTurnAction.Waiting)
                return;
        }

        itemUsageEvent = ExecuteHeroItemUsage(RaidPanel.SelectedUnit, slot);
        StartCoroutine(itemUsageEvent);
    }

    public virtual void HeroSkillTargetSelected(FormationOverlaySlot overlaySlot)
    {
        var primaryTarget = overlaySlot.TargetUnit;

        if(BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            if(RaidPanel.BannerPanel.SkillPanel.SelectedSkill is MoveSkill)
            {
                BattleGround.Round.HeroActionSelected(HeroTurnAction.Move, primaryTarget);
            }
            else if (RaidPanel.BannerPanel.SkillPanel.SelectedSkill is CombatSkill)
            {
                BattleGround.Round.HeroActionSelected(HeroTurnAction.Skill, primaryTarget);
            }
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

    public virtual void HeroPassButtonClicked()
    {
        BattleGround.Round.HeroActionSelected(HeroTurnAction.Pass, BattleGround.Round.SelectedUnit);
    }

    public virtual void HeroCharacterWindowOpened(FormationOverlaySlot overlaySlot)
    {
        if(BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            if (BattleGround.Round.HeroAction == HeroTurnAction.Waiting && BattleGround.Round.TurnStatus == TurnStatus.Progress)
            {
                CharacterWindow.WindowOpened();
                CharacterWindow.UpdateRaidCharacterInfo(overlaySlot.TargetUnit.Character as Hero, false);
            }
        }
        else if (overlaySlot.IsSelectionLocked == false)
        {
            CharacterWindow.WindowOpened();
            if (Raid.CampingPhase == CampingPhase.None)
                CharacterWindow.UpdateRaidCharacterInfo(overlaySlot.TargetUnit.Character as Hero, true);
            else
                CharacterWindow.UpdateRaidCharacterInfo(overlaySlot.TargetUnit.Character as Hero, false);
        }
    }

    public void TargetRoomSelected(DungeonRoom room)
    {
        if (sceneState == DungeonSceneState.Room && CurrentEvent == null)
        {
            Door door = null;
            var currentRoom = RoomView.RaidRoom.Area as DungeonRoom;
            for(int i = 0; i < currentRoom.Doors.Count; i++)
                for(int j = 0; j < room.Doors.Count; j++)
                    if(currentRoom.Doors[i].TargetArea == room.Doors[j].TargetArea)
                        door = currentRoom.Doors[i];

            if (door != null)
            {
                CurrentEvent = HallwayLoadingEvent(CurrentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                    HallTransitionType.FromRoom, door.Direction, RoomView.RaidRoom.Area as DungeonRoom);
                StartCoroutine(CurrentEvent);
            }
        }
    }

    #endregion

    #region Battle and Hero Events

    public virtual void SummonPurging(FormationUnit targetUnit)
    {
        targetUnit.SetSortingOrder(4);
        PrepareDeath(targetUnit);
        UnitEventQueue.RemoveAll(item => item == targetUnit);
        BattleGround.UnitDestroyed(targetUnit);
        Formations.Monsters.DeleteUnitDelayed(targetUnit, 1.867f);
    }

    public virtual void AddResolveCheck(FormationUnit unit)
    {
        if (!ResolveCheckQueue.Contains(unit))
            ResolveCheckQueue.Add(unit);
    }

    public virtual void AddHeartAttackCheck(FormationUnit unit)
    {
        if (!HeartAttackCheckQueue.Contains(unit))
            HeartAttackCheckQueue.Add(unit);
    }

    protected virtual bool PrepareDeath(FormationUnit targetUnit, DeathFactor deathFactor = DeathFactor.AttackMonster, FormationUnit killer = null)
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
                if (RandomSolver.CheckSuccess(hero.DeathResist) && !targetUnit.CombatInfo.MarkedForDeath)
                    return false;
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

    protected virtual void ExecuteDeath(FormationUnit targetUnit)
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

                var heroInfo = CurrentRaid.RaidParty.HeroInfo.Find(info => info.Hero == targetUnit.Character as Hero);
                heroInfo.IsAlive = false;
                heroInfo.DeathRecord = new DeathRecord()
                {
                    HeroClassIndex = heroInfo.Hero.ClassIndexId,
                    HeroName = heroInfo.Hero.Name,
                    KillerName = "necromancer_A",
                    ResolveLevel = heroInfo.Hero.Resolve.Level,
                    Factor = DeathFactor.AttackMonster,
                };
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
                for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
                    DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(BattleGround.HeroParty.Units[i]);
            }
        }
    }

    protected bool ProcessDamage(FormationUnit unit, int damage)
    {
        unit.Character.TakeDamage(damage);
        unit.OverlaySlot.UpdateOverlay();

        if (!unit.Character.HasZeroHealth)
            RaidEvents.ShowPopupMessage(unit, PopupMessageType.Damage, damage.ToString());
        else
        {
            bool atDeathDoor = unit.Character.AtDeathsDoor;
            bool isDead = PrepareDeath(unit);

            RaidEvents.ShowPopupMessage(unit, atDeathDoor ? (isDead ? PopupMessageType.DeathBlow :
                PopupMessageType.DeathsDoor) : PopupMessageType.Damage, damage.ToString());

            return isDead;
        }
        return false;
    }

    protected void ProcessStress(FormationUnit unit, int stress)
    {
        int damage = Mathf.RoundToInt(stress * (1 + unit.Character[AttributeType.StressDmgReceivedPercent].ModifiedValue));
        if (damage < 1)
            damage = 1;

        unit.Character.Stress.IncreaseValue(damage);

        if (unit.Character.IsOverstressed)
        {
            if (unit.Character.IsVirtued)
                unit.Character.Stress.CurrentValue = Mathf.Clamp(unit.Character.Stress.CurrentValue, 0, 100);
            else if (!unit.Character.IsAfflicted && unit.Character.IsOverstressed)
                Instanse.AddResolveCheck(unit);

            if (Mathf.RoundToInt(unit.Character.Stress.CurrentValue) == 200)
                Instanse.AddHeartAttackCheck(unit);
        }

        unit.OverlaySlot.UpdateOverlay();
        RaidEvents.ShowPopupMessage(unit, PopupMessageType.Stress, damage.ToString());
        unit.SetHalo("afflicted");
    }

    protected bool ProcessDeathDamage(DeathDamage deathDamage)
    {
        if (deathDamage == null)
            return false;

        var damageTarget = BattleGround.MonsterParty.Units.Find(target =>
            target.Character.Class == deathDamage.TargetBaseClass);

        if (damageTarget == null)
            return false;

        int damage = damageTarget.Character.TakeDamage(deathDamage.TargetDamage);
        RaidEvents.ShowPopupMessage(damageTarget, PopupMessageType.Damage, damage.ToString());

        return true;
    }

    protected IEnumerator ProcessRaidFailure()
    {
        if (HeroParty.Units.Count == 0)
        {
            StopAllCoroutines();
            StartCoroutine(RaidResultsEvent());
        }
        yield break;
    }

    protected IEnumerator ProcessHeroDeaths(float prepTime, float waitBefore, float waitAfter)
    {
        yield return new WaitForSeconds(prepTime);

        if (!HeroParty.Units.Any(unit => unit.CombatInfo.IsDead))
            yield break;

        yield return new WaitForSeconds(waitBefore);

        for (int i = HeroParty.Units.Count - 1; i >= 0; i--)
            ExecuteDeath(HeroParty.Units[i]);

        yield return new WaitForSeconds(waitAfter);

        yield return StartCoroutine(ExecuteEffectEvents(false));

        yield return ProcessRaidFailure();
    }

    protected IEnumerator ProcessTransformationsAfterBattle()
    {
        foreach (FormationUnit unit in Formations.Heroes.Party.Units)
        {
            var hero = (Hero)unit.Character;
            if (hero.Mode == null || hero.Mode.AfflictionSkillId == null)
                continue;

            var skill = hero.SelectedCombatSkills.Find(s => s.Id == hero.Mode.BattleCompleteSkillId);
            if (skill == null)
                continue;

            SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(unit, unit, skill).UpdateSkillInfo(unit, skill);
            yield return StartCoroutine(ExecuteHeroSkill(unit, targetInfo, skill));
        }
    }

    #region Battle Round

    protected virtual IEnumerator LoadEncounterEvent(IRaidArea areaView)
    {
        SetEncounterState();

        #region Switch Soundtrack
        DarkestSoundManager.PauseDungeonSoundtrack();
        DarkestSoundManager.StartBattleSoundtrack(Raid.Dungeon.Name, SceneState == DungeonSceneState.Room);
        #endregion

        #region Battle Loop
        BattleGround.LoadBattle(DarkestDungeonManager.SaveData.BattleGroundSaveData);
        yield return new WaitForEndOfFrame();
        BattleGround.LoadEffects(DarkestDungeonManager.SaveData.BattleGroundSaveData);

        RaidEvents.RoundIndicator.Appear();

        yield return new WaitForSeconds(1f);
        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        RaidPanel.SwitchBlocked = false;
        Formations.ShowHeroOverlay();

        yield return StartCoroutine(BattleRound(true));
        if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
            yield break;
        while (BattleGround.BattleStatus != BattleStatus.Finished)
        {
            RaidEvents.RoundIndicator.UpdateRound(BattleGround.NextRound());
            yield return StartCoroutine(BattleRound());
            if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                yield break;
        }

        #endregion

        yield return StartCoroutine(FinishEncouter(areaView));
    }

    protected virtual IEnumerator EncounterEvent(IRaidArea areaView, bool campfireAmbush = false)
    {
        SetEncounterState();

        while (IsUnitEventInProgress)
            yield return null;

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
        BattleGround.SpawnEncounter(areaView.Area.BattleEncounter, campfireAmbush);

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

        foreach (var unit in Formations.Monsters.Party.Units)
            FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + unit.Character.Class + "_vo_aggro");
        #endregion

        RaidEvents.ShowBattleAnnouncment();
        yield return new WaitForSeconds(1.8f);
        RaidEvents.HideBattleAnnouncment();
        yield return new WaitForSeconds(0.2f);

        #region Check Suprises
        if (BattleGround.SurpriseStatus == SurpriseStatus.HeroesSurprised)
        {
            RaidEvents.ShowAnnouncment(LocalizationManager.GetString("surprise_announcement"), AnnouncmentPosition.Left);
            TempList.Clear();
            TempList.AddRange(BattleGround.HeroParty.Units);

            foreach (var unit in TempList)
            {
                unit.SetSurprised(true);
                var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                var shuffleRoll = shuffleTargets[RandomSolver.Next(shuffleTargets.Count)];

                if (shuffleRoll.Rank < unit.Rank)
                    unit.Pull(unit.Rank - shuffleRoll.Rank);
                else
                    unit.Push(shuffleRoll.Rank - unit.Rank);
            }
            TempList.Clear();
            yield return new WaitForSeconds(1.2f);
            RaidEvents.HideAnnouncment();
        }
        else if (BattleGround.SurpriseStatus == SurpriseStatus.MonstersSurprised)
        {
            RaidEvents.ShowAnnouncment(LocalizationManager.GetString("surprise_announcement"), AnnouncmentPosition.Right);
            for (int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
                if (BattleGround.MonsterParty.Units[i].Character.BattleModifiers != null)
                    if (BattleGround.MonsterParty.Units[i].Character.BattleModifiers.CanBeSurprised)
                        BattleGround.MonsterParty.Units[i].SetSurprised(true);

            yield return new WaitForSeconds(1.2f);
            RaidEvents.HideAnnouncment();
        }
        else
            yield return new WaitForSeconds(0.8f);
        #endregion

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

        #endregion

        yield return StartCoroutine(FinishEncouter(areaView));
    }

    protected virtual IEnumerator FinishEncouter(IRaidArea areaView)
    {
        yield return StartCoroutine(ProcessTransformationsAfterBattle());

        BattleGround.ResetTargetRanks();
        DarkestSoundManager.StopBattleSoundtrack();
        DarkestSoundManager.ContinueDungeonSoundtrack(Raid.Quest.Dungeon);

        yield return ProcessRaidFailure();

        DarkestSoundManager.ExecuteNarration("victory", NarrationPlace.Raid);
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");

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
        RaidEvents.RoundIndicator.Disappear();
        yield return new WaitForSeconds(0.4f);

        #endregion

        RaidEvents.RoundIndicator.End();
        BattleGround.FinishBattle();

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

        RaidPanel.SelectedUnit.SetPerformerStatus();
        foreach (var hero in Formations.Heroes.Party.Units)
            hero.SetCombatAnimation(false);
        RaidEvents.MonsterTooltip.Hide();

        #endregion

        #region Provide Loot

        if (BattleGround.BattleLoot.Count > 0)
        {
            Formations.UnlockSelections();
            RaidEvents.LoadBattleLoot(BattleGround.BattleLoot);
            if (RaidEvents.LootEvent.HasSomething)
                yield return StartCoroutine(LootEvent());
        }

        #endregion

        #region Check Quest Completion

        areaView.Area.BattleEncounter.Cleared = true;

        if (!Raid.QuestCompleted && Raid.CheckQuestGoals())
            yield return StartCoroutine(CompletionCrestEvent());

        #endregion

        #region Force Curio Tag

        if (areaView.Area.Type == AreaType.BattleCurio || areaView.Area.Type == AreaType.BattleTresure)
        {
            Curio curio = (Curio)areaView.Area.Prop;
            if (curio.IsQuestCurio == false)
            {
                foreach (var triggeredHero in Formations.Heroes.Party.Units)
                {
                    if (triggeredHero.Character.Trait != null)
                    {
                        if (triggeredHero.Character.Trait.CurioTag == "All" ||
                            curio.Tags.Contains(triggeredHero.Character.Trait.CurioTag))
                        {
                            if (RandomSolver.CheckSuccess(triggeredHero.Character.Trait.TagChance))
                            {
                                triggeredHero.OverlaySlot.UnitSelected();
                                if (!(areaView.Prop as RaidCurio).Investigated)
                                    yield return StartCoroutine(CurioEvent(areaView, null, triggeredHero.Character.Trait));
                                break;
                            }
                        }
                    }
                    var hero = triggeredHero.Character as Hero;
                    foreach (var triggerQuirk in hero.Quirks)
                    {
                        if (triggerQuirk.Quirk.CurioTag == "All" || curio.Tags.Contains(triggerQuirk.Quirk.CurioTag))
                        {
                            if (RandomSolver.CheckSuccess(triggerQuirk.Quirk.CurioTagChance))
                            {
                                triggeredHero.OverlaySlot.UnitSelected();
                                if (!(areaView.Prop as RaidCurio).Investigated)
                                    yield return StartCoroutine(CurioEvent(areaView, triggerQuirk.Quirk));
                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Remove Combat States and Restrictions

        QuestPanel.SetPeacefulState();
        Formations.UnlockSelections();
        Formations.ShowHeroOverlay();
        RaidPanel.SetPeacefulState();
        EnablePartyMovement();
        EnableEnviroment();

        #endregion

        #region Scouting and Room Updates

        if (sceneState == DungeonSceneState.Room && Raid.Quest.IsScoutingEnabled)
        {
            DungeonRoom room = RoomView.RaidRoom.Area as DungeonRoom;
            yield return StartCoroutine(ScoutingEvent(room));
        }
        if (sceneState == DungeonSceneState.Room)
            MapPanel.ShowAvailableRooms(RoomView.RaidRoom.Area as DungeonRoom);

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
        RaidPanel.HeroPanel.EquipmentPanel.SetActive();
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
        CurrentEvent = null;
    }

    protected virtual IEnumerator CompletionCrestEvent()
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

    protected virtual IEnumerator BattleRound(bool fromBattleSave = false)
    {
        if (fromBattleSave == false)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/round");
            yield return new WaitForSeconds(1f);

            #region Stalling

            if (BattleGround.MonsterFormation.IsStallingActive)
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
                            Instanse.AddHeartAttackCheck(BattleGround.Controls[i].PrisonerUnit);
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

            yield return StartCoroutine(BonusTurn(desire => desire.IsRoundStart));

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
            if (BattleGround.IsBattleEnded())
                break;

            if (fromBattleSave == false)
            {
                #region Captor Activations
                for (int i = BattleGround.Captures.Count - 1; i >= 0; i--)
                {
                    #region Damage Dealing
                    if (BattleGround.Captures[i].Component.PerTurnDamagePercent != 0)
                    {
                        var captorMonster = BattleGround.Captures[i].CaptorUnit.Character as Monster;
                        var prisonerUnit = BattleGround.Captures[i].PrisonerUnit;

                        int healthDamage = prisonerUnit.Character.TakeDamagePercent(BattleGround.Captures[i].Component.PerTurnDamagePercent);
                        FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + captorMonster.Data.TypeId + "_captor_full_action");

                        if (!prisonerUnit.Character.HasZeroHealth)
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

                foreach (var companionEntry in BattleGround.Companions)
                {
                    if (!Mathf.Approximately(companionEntry.CompanionComponent.HealPerTurn, 0))
                    {
                        int health = companionEntry.TargetUnit.Character.HealPercent(companionEntry.CompanionComponent.HealPerTurn, false);
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_enemy");
                        companionEntry.TargetUnit.OverlaySlot.UpdateOverlay();
                        RaidEvents.ShowPopupMessage(companionEntry.TargetUnit, PopupMessageType.Heal, health.ToString());
                        yield return new WaitForSeconds(0.4f);
                    }
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
                if (unit.Team == Team.Heroes)
                {
                    yield return StartCoroutine(HeroTurn(unit));

                    if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                        yield break;
                }
                else if (unit.Team == Team.Monsters)
                {
                    yield return StartCoroutine(MonsterTurn(unit));

                    if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                        yield break;
                }
                #endregion
            }
            else
            {
                fromBattleSave = false;

                if (BattleGround.Round.SelectedUnit.Team == Team.Heroes)
                {
                    yield return StartCoroutine(HeroTurn(BattleGround.Round.SelectedUnit, true));
                    if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                        yield break;
                }
                else if (BattleGround.Round.SelectedUnit.Team == Team.Monsters)
                {
                    yield return StartCoroutine(MonsterTurn(BattleGround.Round.SelectedUnit));
                    if (BattleGround.Round.HeroAction == HeroTurnAction.Retreat)
                        yield break;
                }
            }

            if (!BattleGround.IsBattleEnded())
                yield return StartCoroutine(BonusTurn(desire => desire.IsPostTurn && desire.IsRoundInProgress));

            if (!BattleGround.IsBattleEnded())
                yield return WaitForZeroThree;
        }

        yield return StartCoroutine(BonusTurn(desire => desire.IsRoundFinish));

        #region Idle units status effects
        TempList.AddRange(BattleGround.MonsterParty.Units.FindAll(targetUnit => targetUnit.CombatInfo.TotalInitiatives == 0));
        bool hasIdleDamage = false, hasIdleDeath = false;
        foreach (var idleUnit in TempList)
        {
            #region Status Effect and Buffs
            if (idleUnit.Character.GetStatusEffect(StatusType.Bleeding).IsApplied)
            {
                var bleedEffect = idleUnit.Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect;
                int damage = idleUnit.Character.TakeDamage(Mathf.CeilToInt(bleedEffect.CurrentTickDamage * 1.5f));
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
                idleUnit.OverlaySlot.UpdateOverlay();

                if (!idleUnit.Character.HasZeroHealth)
                {
                    RaidEvents.ShowPopupMessage(idleUnit, PopupMessageType.Damage, damage.ToString());
                    hasIdleDamage = true;
                }
                else
                {
                    PrepareDeath(idleUnit);
                    hasIdleDeath = true;
                }
            }

            if (idleUnit.Character.GetStatusEffect(StatusType.Poison).IsApplied)
            {
                var poisonEffect = idleUnit.Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect;
                int damage = idleUnit.Character.TakeDamage(Mathf.CeilToInt(poisonEffect.CurrentTickDamage * 1.5f));
                RaidEvents.ShowPopupMessage(idleUnit, PopupMessageType.Damage, damage.ToString());
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/poison_dot");
                idleUnit.OverlaySlot.UpdateOverlay();

                if (!idleUnit.Character.HasZeroHealth)
                {
                    RaidEvents.ShowPopupMessage(idleUnit, PopupMessageType.Damage, damage.ToString());
                    hasIdleDamage = true;
                }
                else
                {
                    PrepareDeath(idleUnit);
                    hasIdleDeath = true;
                }
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

    protected virtual IEnumerator HeroTurn(FormationUnit actionUnit, bool fromBattleSave = false)
    {
        if(fromBattleSave == false)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/ally_turn");
            RaidPanel.SetDisabledState();
            Formations.ResetSelections();
            yield return new WaitForEndOfFrame();
            BattleGround.Round.PreHeroTurn(actionUnit);
            yield return new WaitForEndOfFrame();
            actionUnit.OverlaySlot.UnitSelected();

            #region Status Effect and Buffs
            if (actionUnit.Character.GetStatusEffect(StatusType.Bleeding).IsApplied)
            {
                var bleedEffect = (BleedingStatusEffect)actionUnit.Character.GetStatusEffect(StatusType.Bleeding);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");

                if (ProcessDamage(actionUnit, bleedEffect.CurrentTickDamage))
                {
                    yield return new WaitForSeconds(1.4f);
                    BattleGround.Round.PostHeroTurn();
                    ExecuteDeath(actionUnit);
                    yield return StartCoroutine(ExecuteEffectEvents(true));
                    yield break;
                }

                yield return new WaitForSeconds(actionUnit.Character.AtDeathsDoor ? 0.6f : 0.3f);
                yield return StartCoroutine(ExecuteEffectEvents(true));
            }

            if (actionUnit.Character.GetStatusEffect(StatusType.Poison).IsApplied)
            {
                var poisonEffect = (PoisonStatusEffect)actionUnit.Character.GetStatusEffect(StatusType.Poison);
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/poison_dot");

                if (ProcessDamage(actionUnit, poisonEffect.CurrentTickDamage))
                {
                    yield return new WaitForSeconds(1.4f);
                    BattleGround.Round.PostHeroTurn();
                    ExecuteDeath(actionUnit);
                    yield return StartCoroutine(ExecuteEffectEvents(true));
                    yield break;
                }

                yield return new WaitForSeconds(actionUnit.Character.AtDeathsDoor ? 0.6f : 0.3f);
                yield return StartCoroutine(ExecuteEffectEvents(true));
            }

            if (actionUnit.CombatInfo.IsSurprised)
                actionUnit.SetSurprised(false);

            if (actionUnit.Character.GetStatusEffect(StatusType.Stun).IsApplied)
            {
                var stunStatus = (StunStatusEffect)actionUnit.Character.GetStatusEffect(StatusType.Stun);
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
                ProcessStress(actionUnit, actionUnit.Character.Mode.StressPerTurn);
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
            Hero actionHero = actionUnit.Character as Hero;
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
                            actionUnit.SetDefendAnimation(true);
                            yield return new WaitForSeconds(0.1f);

                            int damageAmount = actionHero.TakeDamagePercent(actOut.NumberParameter);
                            actionUnit.OverlaySlot.UpdateOverlay();
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
                            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Damage, damageAmount.ToString());

                            if (actionUnit.Character.HasZeroHealth)
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
                        BattleGround.Round.PostHeroTurn();
                        yield break;
                        #endregion
                    case StartTurnActType.HealSelf:
                        #region Heal Self
                        if (actionHero.HealthRatio == 1)
                            break;

                        yield return new WaitForSeconds(1f);
                        int healAmount = actionHero.HealPercent(actOut.NumberParameter, true);
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
                            Formations.Heroes.Overlay.ResetSelectionsExcept(actionUnit);
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
                }
            }
            #endregion
        }
        else
        {
            RaidPanel.SetDisabledState();
            Formations.ResetSelections();
            yield return new WaitForEndOfFrame();
            actionUnit.OverlaySlot.UnitSelected();
        }

        actionUnit.Character.ApplyAllBuffRules(Rules.GetIdleUnitRules(actionUnit));
        if (fromBattleSave == false)
        {
            DarkestDungeonManager.SaveData.UpdateFromRaid();
            DarkestDungeonManager.Instanse.SaveGame();
        }
        while (true)
        {
            Formations.ShowUnitOverlay();
            BattleGround.Round.OnHeroTurn();
            RaidPanel.SetCombatState();
            Inventory.SetCombatState();

            #region Hero Action
            QuestPanel.UpdateCombatRetreat(true);
            while (BattleGround.Round.HeroAction == HeroTurnAction.Waiting)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    BattleGround.BattleStatus = BattleStatus.Finished;
                    BattleGround.Round.HeroActionSelected(HeroTurnAction.Pass, BattleGround.Round.SelectedUnit);
                    break;
                }

                yield return null;
            }
            QuestPanel.UpdateCombatRetreat(false);
            while (IsUnitEventInProgress)
                yield return null;

            Inventory.SetDeactivated();
            var targetUnit = BattleGround.Round.SelectedTarget;
            var usedSkill = RaidPanel.BannerPanel.SkillPanel.SelectedSkill;
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
                            if (
                                RandomSolver.CheckSuccess(
                                    targetUnit.Character.Trait.Reactions[ReactionType.BlockMove].Chance))
                            {
                                actionUnit.CombatInfo.BlockedMoveUnitIds.Add(targetUnit.CombatInfo.CombatId);
                                yield return new WaitForSeconds(1f);
                                Formations.ResetSelections();
                                yield return new WaitForEndOfFrame();
                                BattleGround.Round.PreHeroTurn(actionUnit);
                                yield return new WaitForEndOfFrame();
                                actionUnit.OverlaySlot.UnitSelected();
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
                        if(targetInfo.Targets.Count == 0)
                        {
                            Formations.ResetSelections();
                            yield return new WaitForEndOfFrame();
                            BattleGround.Round.PreHeroTurn(actionUnit);
                            yield return new WaitForEndOfFrame();
                            actionUnit.OverlaySlot.UnitSelected();
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
                case HeroTurnAction.Retreat:
                    Formations.ResetSelections();
                    bool retreatFailed = RandomSolver.CheckSuccess(0.2f);
                    for(int i = 0; i < HeroParty.Units.Count; i++)
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

                        yield return StartCoroutine(ProcessTransformationsAfterBattle());

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

                        if(SceneState == DungeonSceneState.Hall)
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
            }
            break;

            #endregion
        }

        BattleGround.Round.PostHeroTurn();
    }

    protected virtual IEnumerator MonsterTurn(FormationUnit actionUnit, string combatSkillOverride = null, bool fromBonusTurn = false)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/enemy_turn");
        Formations.ResetSelections();
        yield return new WaitForEndOfFrame();

        if(!fromBonusTurn)
            BattleGround.Round.PreMonsterTurn(actionUnit);

        yield return new WaitForEndOfFrame();
        Formations.ShowUnitOverlay();
        yield return new WaitForEndOfFrame();
        actionUnit.SetPerformerStatus();

        if (!fromBonusTurn)
        {
            #region Status Effects and Buffs

            if (actionUnit.Character[StatusType.Bleeding].IsApplied)
            {
                var bleedEffect = (BleedingStatusEffect)actionUnit.Character[StatusType.Bleeding];
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");

                if (ProcessDamage(actionUnit, bleedEffect.CurrentTickDamage))
                {
                    DeathDamage deathDamage = actionUnit.Character.DeathDamage;
                    yield return new WaitForSeconds(1.4f);
                    BattleGround.Round.PostMonsterTurn();
                    ExecuteDeath(actionUnit);

                    if (ProcessDeathDamage(deathDamage))
                        yield return new WaitForSeconds(0.4f);

                    yield return StartCoroutine(ExecuteEffectEvents(true));
                    yield break;
                }

                yield return new WaitForSeconds(actionUnit.Character.AtDeathsDoor ? 0.6f : 0.3f);
            }

            if (actionUnit.Character[StatusType.Poison].IsApplied)
            {
                var poisonEffect = (PoisonStatusEffect)actionUnit.Character[StatusType.Poison];
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/poison_dot");

                if (ProcessDamage(actionUnit, poisonEffect.CurrentTickDamage))
                {
                    DeathDamage deathDamage = actionUnit.Character.DeathDamage;
                    yield return new WaitForSeconds(1.4f);
                    BattleGround.Round.PostMonsterTurn();
                    ExecuteDeath(actionUnit);

                    if (ProcessDeathDamage(deathDamage))
                        yield return new WaitForSeconds(0.4f);

                    yield return StartCoroutine(ExecuteEffectEvents(true));
                    yield break;
                }

                yield return new WaitForSeconds(actionUnit.Character.AtDeathsDoor ? 0.6f : 0.3f);
            }

            if (actionUnit.CombatInfo.IsSurprised)
                actionUnit.SetSurprised(false);

            if (actionUnit.Character[StatusType.Stun].IsApplied)
            {
                var stunStatus = (StunStatusEffect)actionUnit.Character[StatusType.Stun];
                stunStatus.StunApplied = false;
                RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Unstun);
                actionUnit.ResetHalo();

                yield return new WaitForSeconds(0.9f);

                actionUnit.Character.ApplyStunRecovery();
                RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Buff);
                actionUnit.Character.UpdateRound();
                actionUnit.OverlaySlot.UpdateOverlay();
                BattleGround.Round.PostMonsterTurn();

                yield return new WaitForSeconds(0.6f);
                yield break;
            }

            actionUnit.Character.UpdateRound();
            actionUnit.OverlaySlot.UpdateOverlay();

            #endregion

            BattleGround.Round.OnMonsterTurn();
        }
        
        yield return StartCoroutine(ExecuteMonsterSkill(actionUnit, combatSkillOverride));

        if (!fromBonusTurn)
            if (BattleGround.Round.HeroAction != HeroTurnAction.Retreat)
                BattleGround.Round.PostMonsterTurn();
    }

    protected virtual IEnumerator BonusTurn(Predicate<BonusInitiativeDesire> desireSelector)
    {
        TempList.AddRange(BattleGround.MonsterParty.Units);
        while (TempList.Count > 0)
        {
            var monsterUnit = TempList[0];
            TempList.RemoveAt(0);
            if (monsterUnit.Character is Hero)
                continue;

            var monster = monsterUnit.Character as Monster;
            var desires = monster.Brain.BonusDesireSet.FindAll(desireSelector);
            while (desires.Count > 0)
            {
                var currentDesire = desires[0];
                desires.RemoveAt(0);

                if (currentDesire.CheckBonusInitiative(monsterUnit))
                {
                    yield return StartCoroutine(MonsterTurn(monsterUnit, currentDesire.CombatSkillOverride, true));
                    break;
                }
            }
        }
        TempList.Clear();
    }

    private void SetEncounterState()
    {
        QuestPanel.UpdateEncounterRetreat();
        QuestPanel.SetCombatState();
        DisableEnviroment();
        DisablePartyMovement();
        Formations.LockSelections();
        Formations.ResetSelections();
        RaidPanel.SetDisabledState();
        Inventory.SetDeactivated();
        RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();

        foreach (var hero in Formations.Heroes.Party.Units)
            hero.SetCombatAnimation(true);
    }

    #endregion

    #region Skill Usage

    protected virtual void ExecuteSkillInstants(FormationUnit performer, SkillTargetInfo targetInfo, SkillResult skillResult)
    {
        foreach (var skillEntry in skillResult.SkillEntries)
        {
            if (skillEntry.IsTargetHit)
                skillEntry.Target.SetTargetSkillEffect(targetInfo.SkillArtInfo, performer);

            skillEntry.Target.OverlaySlot.UpdateOverlay();

            if (targetInfo.Type == SkillTargetType.Enemy && skillEntry.Target.Character.AtDeathsDoor)
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
                switch (skillEntry.Type)
                {
                    case SkillResultType.Miss:
                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Miss);
                        break;
                    case SkillResultType.Dodge:
                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Dodge);
                        break;
                    case SkillResultType.Hit:
                        if (performer.Character.IsMonster && targetInfo.Skill.DamageMax == 0)
                            break;
                        if (!performer.Character.IsMonster && targetInfo.Skill.DamageMod == -1)
                            break;

                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Damage, skillEntry.Amount.ToString());
                        break;
                    case SkillResultType.Crit:
                        if (performer.Character.IsMonster && targetInfo.Skill.DamageMax == 0)
                            break;
                        if (!performer.Character.IsMonster && targetInfo.Skill.DamageMod == -1)
                            break;

                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.CritDamage, skillEntry.Amount.ToString());
                        break;
                    case SkillResultType.Heal:
                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Heal, skillEntry.Amount.ToString());
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                        break;
                    case SkillResultType.CritHeal:
                        RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.CritHeal, skillEntry.Amount.ToString());
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally_crit");
                        break;
                }

                if (skillEntry.IsTargetHit && skillEntry.Target.Character.SkillReaction != null &&
                    skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects.Count > 0)
                {
                    for (int i = 0; i < skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects.Count; i++)
                        for (int j = 0; j < skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects[i].SubEffects.Count; j++)
                            skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects[i].SubEffects[j].Apply(skillEntry.Target,
                                performer, skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects[i]);
                }

                if (skillEntry.Target.Character.IsMonster && skillEntry.Target.Character.HasZeroHealth)
                    PrepareDeath(skillEntry.Target);
                else if (skillEntry.Target.Character.IsMonster == false && skillEntry.Target.Character.AtDeathsDoor == false)
                    if (skillEntry.Target.Character.HasZeroHealth)
                        PrepareDeath(skillEntry.Target);
            }
        }

        for(int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
            if (BattleGround.MonsterParty.Units[i].CombatInfo.MarkedForDeath)
                PrepareDeath(BattleGround.MonsterParty.Units[i]);

        performer.OverlaySlot.UpdateOverlay();
    }

    protected virtual void ExecuteSlidingSetup(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        if(performer.Team == Team.Monsters)
        {
            if (targetInfo.Type == SkillTargetType.Party)
                Formations.PartyBuffPositions.SetSpacing(120, 1f);
            else if (targetInfo.Type == SkillTargetType.Enemy)
            {
                if (targetInfo.Skill.Type == "melee")
                    Formations.MonstersAttackMeleePosition.SetSliding(-150, 1f);
                else
                    Formations.MonstersAttackRangePosition.SetSliding(120, 1f);

                Formations.HeroesDefencePositions.SetSliding(-120, 1f);
            }
            else if (targetInfo.Type == SkillTargetType.Self)
                Formations.PartyBuffPositions.SetSpacing(120, 1f);
        }
        else
        {
            if (targetInfo.Type == SkillTargetType.Party)
                Formations.PartyBuffPositions.SetSpacing(120, 1f);
            else if (targetInfo.Type == SkillTargetType.Enemy)
            {
                if (targetInfo.Skill.Type == "melee")
                    Formations.HeroesAttackMeleePosition.SetSliding(150, 1f);
                else
                    Formations.HeroesAttackRangePosition.SetSliding(-120, 1f);

                Formations.MonstersDefencePositions.SetSliding(120, 1f);
            }
            else if (targetInfo.Type == SkillTargetType.Self)
                Formations.PartyBuffPositions.SetSpacing(120, 1f);
        }
    }

    protected virtual void ExecuteRiposteAnimationIntro(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        if (RiposteResults.Count > 0)
        {
            performer.SetPerformerSkillAnimation(targetInfo.SkillArtInfo, false);
            performer.SetDefendAnimation(true);

            for (int i = 0; i < RiposteResults.Count; i++)
            {
                Riposters[i].SetDefendAnimation(false);
                Riposters[i].SetPerformerSkillAnimation(RiposteResults[i].ArtInfo, true);
            }
        }
    }

    protected virtual void ExecuteSkillAnimationIntro(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        if (targetInfo.Skill.ValidModes.Count > 1 && targetInfo.Mode != null)
            Formations.UnitSkillIntroOverriden(performer, targetInfo.SkillArtInfo, targetInfo.Mode.Id);
        else
            Formations.UnitSkillIntro(performer, targetInfo.SkillArtInfo);

        if (targetInfo.Type == SkillTargetType.Party)
        {
            foreach (var targetUnit in targetInfo.Targets)
                if (performer != targetUnit)
                    Formations.UnitBuffedIntro(targetUnit);

            if (targetInfo.Targets.Contains(performer))
            {
                if(performer.Team == Team.Heroes)
                    Formations.PartyBuffPositions.SetUnitTargets(targetInfo.Targets.OrderByDescending(unit =>
                    unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                else
                    Formations.PartyBuffPositions.SetUnitTargets(targetInfo.Targets.OrderBy(unit => 
                    unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
            }
            else
            {
                var positionTargets = new List<FormationUnit>(targetInfo.Targets);
                positionTargets.Insert(0, performer);
                if (performer.Team == Team.Heroes)
                    Formations.PartyBuffPositions.SetUnitTargets(positionTargets.OrderByDescending(unit =>
                    unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                else
                    Formations.PartyBuffPositions.SetUnitTargets(positionTargets.OrderBy(unit =>
                    unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
            }
        }
        else if (targetInfo.Type == SkillTargetType.Enemy)
        {
            foreach (var targetUnit in targetInfo.Targets)
                Formations.UnitDefendIntro(targetUnit);

            if(performer.Team == Team.Monsters)
            {
                if (targetInfo.Skill.Type == "melee")
                    Formations.MonstersAttackMeleePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);
                else
                    Formations.MonstersAttackRangePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);

                Formations.HeroesDefencePositions.SetUnitTargets(targetInfo.Targets.OrderByDescending(unit =>
                    unit.Rank).ToList(), 0.01f, targetInfo.SkillArtInfo.TargetAreaOffset);
            }
            else
            {
                if (targetInfo.Skill.Type == "melee")
                    Formations.HeroesAttackMeleePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);
                else
                    Formations.HeroesAttackRangePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);

                Formations.MonstersDefencePositions.SetUnitTargets(targetInfo.Targets.OrderBy(unit =>
                    unit.Rank).ToList(), 0.01f, targetInfo.SkillArtInfo.TargetAreaOffset);
            }
        }
        else if (targetInfo.Type == SkillTargetType.Self)
        {
            Formations.PartyBuffPositions.SetUnitTargets(performer, 0.01f, Vector2.zero);
        }
    }

    protected virtual void ExecuteSkillAnimationOutro(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        if (RiposteResults.Count > 0)
        {
            performer.SetPerformerSkillAnimation(targetInfo.SkillArtInfo, false);
            performer.SetDefendAnimation(false);

            for (int i = 0; i < RiposteResults.Count; i++)
            {
                Riposters[i].SetDefendAnimation(false);
                Riposters[i].SetPerformerSkillAnimation(RiposteResults[i].ArtInfo, false);
            }
        }

        if (targetInfo.Skill.ValidModes.Count > 1 && targetInfo.Mode != null)
            Formations.UnitSkillOutroOverriden(performer, targetInfo.SkillArtInfo, targetInfo.Mode.Id);
        else
            Formations.UnitSkillOutro(performer, targetInfo.SkillArtInfo);

        if (targetInfo.Type == SkillTargetType.Party)
        {
            foreach (var targetUnit in targetInfo.Targets)
                if (performer != targetUnit)
                    Formations.UnitBuffedOutro(targetUnit);
        }
        else if (targetInfo.Type == SkillTargetType.Enemy)
        {
            foreach (var targetUnit in targetInfo.Targets)
                Formations.UnitDefendOutro(targetUnit);
        }

        RiposteResults.Clear();
        Riposters.Clear();
    }

    protected virtual void ExecuteGuardRedirection(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        if (targetInfo.Type == SkillTargetType.Enemy)
            for (int i = targetInfo.Targets.Count - 1; i >= 0; i--)
                if (targetInfo.Targets[i].Character.GetStatusEffect(StatusType.Guarded).IsApplied)
                {
                    var guardedStatus = targetInfo.Targets[i].Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;
                    if (!targetInfo.Targets.Contains(guardedStatus.Guard))
                        targetInfo.Targets[i] = guardedStatus.Guard;
                }
    }

    protected virtual void ExecuteRiposteSkillActivation(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        Riposters.Clear();
        RiposteResults.Clear();

        if (targetInfo.Type != SkillTargetType.Enemy)
            return;

        foreach (var target in targetInfo.Targets)
        {
            if (!target.Character.GetStatusEffect(StatusType.Riposte).IsApplied)
                continue;
            if (target.CombatInfo.IsDead)
                continue;

            var riposteSkill = target.Character.RiposteSkill;

            if (riposteSkill == null)
                continue;

            var riposteArt = target.Character.SkillArtInfo.Find(art => art.SkillId == riposteSkill.Id);
            if (riposteArt == null)
                continue;

            BattleSolver.SkillResult.Reset();
            BattleSolver.ExecuteSkill(target, performer, riposteSkill, riposteArt);

            Riposters.Add(target);
            RiposteResults.Add(BattleSolver.SkillResult.Copy());

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
        }
    }

    protected virtual void ExecuteRiposteInstants(FormationUnit performer)
    {
        for (int i = 0; i < Riposters.Count; i++)
        {
            foreach (var skillEntry in RiposteResults[i].SkillEntries)
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
                    }

                    if (skillEntry.IsZeroed && (skillEntry.Type == SkillResultType.Hit || skillEntry.Type == SkillResultType.Crit))
                    {
                        if(skillEntry.Target.Character.IsMonster)
                            PrepareDeath(skillEntry.Target);
                        else if (!skillEntry.Target.Character.AtDeathsDoor && !DeathDoorEnterQueue.Contains(skillEntry.Target))
                            PrepareDeath(skillEntry.Target);
                    }
                }
            }
            performer.OverlaySlot.UpdateOverlay();
        }
    }

    protected virtual void SetBrainDecisionMarkings(FormationUnit performer, MonsterBrainDecision brainDecision)
    {
        if (brainDecision.TargetInfo.Targets.Contains(performer))
            performer.SetFriendlyPerformerStatus(false);

        if (brainDecision.TargetInfo.Type == SkillTargetType.Party)
        {
            foreach (var target in brainDecision.TargetInfo.Targets)
                if (target != performer)
                    target.SetFriendlyTargetStatus(false);
        }
        else if (brainDecision.TargetInfo.Type == SkillTargetType.Enemy)
        {
            if (brainDecision.SelectedSkill.TargetRanks.IsMultitarget && brainDecision.TargetInfo.Targets.Count > 0)
            {
                for (int i = 0; i < brainDecision.TargetInfo.Targets.Count; i++)
                    brainDecision.TargetInfo.Targets[i].SetEnemyTargetStatus(false, i != 0);
            }
            else
                foreach (var target in brainDecision.TargetInfo.Targets)
                    target.SetEnemyTargetStatus(false, false);
        }
    }

    protected virtual SkillResult ExecuteSkillBase(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        BattleSolver.SkillResult.Reset();
        BattleGround.LastDamaged.Clear();

        foreach (var targetUnit in targetInfo.Targets)
        {
            BattleSolver.ExecuteSkill(performer, targetUnit, targetInfo.Skill, targetInfo.SkillArtInfo);
            if (BattleSolver.SkillResult.Current.IsTargetHit)
                BattleGround.LastDamaged.Add(targetUnit.Character.Class);
        }

        var skillResult = BattleSolver.SkillResult.Copy();

        string playSkillEvent;
        string playSkillMissEvent;

        if (performer.Character.IsMonster)
        {
            playSkillEvent = "event:/char/enemy/" + performer.Character.Class + "_" + targetInfo.Skill.Id;
            playSkillMissEvent = "event:/char/enemy/" + performer.Character.Class + "_" + targetInfo.Skill.Id + "_miss";
        }
        else if (performer.Character.Mode != null)
        {
            playSkillEvent = "event:/char/ally/" + performer.Character.Class + "_" +
                targetInfo.Skill.Id + "_" + performer.Character.Mode.Id;
            playSkillMissEvent = "event:/char/ally/" + performer.Character.Class + "_" + 
                targetInfo.Skill.Id + "_miss" + "_" + performer.Character.Mode.Id;
        }
        else
        {
            playSkillEvent = "event:/char/ally/" + performer.Character.Class + "_" + targetInfo.Skill.Id;
            playSkillMissEvent = playSkillEvent + "_miss";
        }

        if(skillResult.HasHit && FMODUnity.RuntimeManager.GetEventDescription(playSkillEvent) != null)
            FMODUnity.RuntimeManager.PlayOneShot(playSkillEvent, DungeonCamera.Transform.position);
        else if (FMODUnity.RuntimeManager.GetEventDescription(playSkillMissEvent) != null)
            FMODUnity.RuntimeManager.PlayOneShot(playSkillMissEvent, DungeonCamera.Transform.position);

        if (skillResult.HasCritEffect && targetInfo.Type == SkillTargetType.Enemy)
            DarkestSoundManager.ExecuteNarration(performer.Character.IsMonster ? "crit_hero" : "crit_monster", NarrationPlace.Raid);

        return skillResult;
    }

    protected virtual List<DeathDamage> ExecuteBattlegroundDeaths(FormationUnit peformer)
    {
        List<DeathDamage> deathDamages = new List<DeathDamage>();

        if (peformer.CombatInfo.MarkedForDeath)
        {
            peformer.CombatInfo.IsDead = true;
            List<FormationUnit> lifeLinkedUnits = new List<FormationUnit>();
            for (int i = 0; i < peformer.Party.Units.Count; i++)
            {
                if (peformer.Party.Units[i].Character.LifeLink != null)
                {
                    if (peformer.Party.Units[i].Character.LifeLink.LinkBaseClass == peformer.Character.Class)
                        lifeLinkedUnits.Add(peformer.Party.Units[i]);
                }
            }
            lifeLinkedUnits.ForEach(SummonPurging);
            lifeLinkedUnits.Clear();

            if (peformer.CombatInfo.IsDead)
                if (peformer.Character.DeathDamage != null)
                    deathDamages.Add(peformer.Character.DeathDamage);

            ExecuteDeath(peformer);
        }

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

        return deathDamages;
    }

    protected virtual IEnumerator ExecuteDeathDamages(List<DeathDamage> deathDamages)
    {
        for (int i = 0; i < deathDamages.Count; i++)
        {
            var deathDamageTarget = BattleGround.MonsterParty.Units.Find(unit => unit.Character.Class == deathDamages[i].TargetBaseClass) ??
                BattleGround.HeroParty.Units.Find(unit => unit.Character.Class == deathDamages[i].TargetBaseClass);

            if (deathDamageTarget != null)
            {
                int damage = deathDamageTarget.Character.TakeDamage(deathDamages[i].TargetDamage);
                deathDamageTarget.OverlaySlot.UpdateOverlay();
                RaidEvents.ShowPopupMessage(deathDamageTarget, PopupMessageType.Damage, damage.ToString());
                deathDamageTarget.SetDefendAnimation(true);
                yield return new WaitForSeconds(0.8f);
                deathDamageTarget.SetDefendAnimation(false);
            }
        }
    }

    protected virtual IEnumerator ExecuteMonsterSkill(FormationUnit actionUnit, string combatSkillOverride = null)
    {
        RaidEvents.MonsterTooltip.IsDisabled = true;
        RaidEvents.MonsterTooltip.Hide();
        if(actionUnit.Character.IsMonster == false)
        {
            actionUnit.OverlaySlot.StartDialog(LocalizationManager.GetString("str_control_before_turn_siren"));
            while (actionUnit.OverlaySlot.IsDoingDialog)
                yield return null;
        }
        var brainDecision = BattleSolver.UseMonsterBrain(actionUnit, combatSkillOverride);
        if(brainDecision.Decision == BrainDecisionType.Pass)
        {
            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Pass);
            yield return new WaitForSeconds(0.9f);
            yield break;
        }
        yield return new WaitForSeconds(0.1f);
        SetBrainDecisionMarkings(actionUnit, brainDecision);
        ExecuteGuardRedirection(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.1f);
        brainDecision.TargetInfo.UpdateSkillInfo(actionUnit, brainDecision.SelectedSkill);
        if (brainDecision.TargetInfo.SkillArtInfo.CanDisplaySelection != false)
            RaidEvents.ShowMonsterSkillAnnouncement(actionUnit.Character, brainDecision.TargetInfo.SkillArtInfo.SkillId);
        yield return new WaitForSeconds(0.75f);
        Formations.HideUnitOverlay();
        TorchMeter.Hide();
        if (brainDecision.TargetInfo.SkillArtInfo.CanDisplaySelection != false)
            RaidEvents.HideAnnouncment();
        yield return new WaitForSeconds(0.2f);
        DungeonCamera.Zoom(50, 0.05f);
        var skillResult = ExecuteSkillBase(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.05f);
        DungeonCamera.SwitchBlur(true);
        ExecuteSkillAnimationIntro(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.01f);
        ExecuteSkillInstants(actionUnit, brainDecision.TargetInfo, skillResult);
        yield return new WaitForSeconds(0.01f);
        ExecuteSlidingSetup(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.70f);
        #region Teleport Skill
        if(brainDecision.SelectedSkill.Type == "teleport")
        {
            DarkestDungeonManager.ScreenFader.Fade(2);
            yield return new WaitForSeconds(0.6f);

            BattleGround.ResetTargetRanks();
            DarkestSoundManager.StopBattleSoundtrack();
            DarkestSoundManager.ContinueDungeonSoundtrack(Raid.Quest.Dungeon);

            #region Destroy Remains
            Formations.HideMonsterOverlay();
            RaidEvents.RoundIndicator.Disappear();
            RaidEvents.RoundIndicator.End();
            BattleGround.RetreatFromBattle();
            Formations.ResetSelections();
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

            RaidEvents.MonsterTooltip.Hide();

            if (brainDecision.TargetInfo.Type == SkillTargetType.Party)
            {
                foreach (var targetUnit in brainDecision.TargetInfo.Targets)
                    if (actionUnit != targetUnit)
                        Formations.UnitBuffedOutro(targetUnit);
            }
            else if (brainDecision.TargetInfo.Type == SkillTargetType.Enemy)
            {
                foreach (var targetUnit in brainDecision.TargetInfo.Targets)
                    Formations.UnitDefendOutro(targetUnit);
            }

            DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
            DungeonCamera.SwitchBlur(false);
            TorchMeter.Show();
            yield return new WaitForSeconds(0.2f);
            BattleGround.Round.HeroAction = HeroTurnAction.Retreat;
            var targetRooms = Raid.Dungeon.Rooms.Values.ToList().
                FindAll(targetRoom => targetRoom.Doors.Count == 1 && targetRoom != Raid.CurrentLocation);
            if (targetRooms.Count == 0)
                targetRooms = Raid.Dungeon.Rooms.Values.ToList().
                FindAll(targetRoom => targetRoom.Doors.Count == 2 && targetRoom != Raid.CurrentLocation);

            if(targetRooms.Count == 0)
                targetRooms.Add(Raid.Dungeon.StartingRoom);
            if (SceneState == DungeonSceneState.Hall)
            {
                HallwayView.CurrentSector.SetInside(false);
                CurrentEvent = RoomLoadingEvent(targetRooms[RandomSolver.Next(targetRooms.Count)],
                    RoomTransitionType.Teleport, HallwayView.CurrentSector);
            }
            else if (SceneState == DungeonSceneState.Room)
            {
                CurrentEvent = RoomLoadingEvent(targetRooms[RandomSolver.Next(targetRooms.Count)],
                    RoomTransitionType.Teleport);
            }
            #region Remove Combat States and Restrictions
            QuestPanel.SetPeacefulState();
            Formations.ShowHeroOverlay();
            RaidPanel.SetPeacefulState();
            EnableEnviroment();
            BattleGround.LeaveBattleGround();
            Inventory.SetPeacefulState(false);
            StartCoroutine(CurrentEvent);
            #endregion
            yield break;
        }
        #endregion
        ExecuteRiposteSkillActivation(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.05f);
        ExecuteRiposteAnimationIntro(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.05f);
        ExecuteRiposteInstants(actionUnit);
        yield return new WaitForSeconds(Riposters.Count > 0 ? 1.2f : 0.7f);
        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
        DungeonCamera.SwitchBlur(false);
        ExecuteSkillAnimationOutro(actionUnit, brainDecision.TargetInfo);

        List<DeathDamage> deathDamages = ExecuteBattlegroundDeaths(actionUnit);
        if (deathDamages.Count > 0)
            yield return StartCoroutine(ExecuteDeathDamages(deathDamages));
        
        yield return new WaitForSeconds(0.175f);
        Formations.ShowUnitOverlay();
        TorchMeter.Show();
        Formations.ResetSelections();
        yield return new WaitForSeconds(0.075f);

        if (brainDecision.TargetInfo.Type == SkillTargetType.Enemy && skillResult.HasCritEffect)
            for (int j = 0; j < BattleGround.HeroParty.Units.Count; j++)
                DarkestDungeonManager.Data.Effects["AfflictedAllyStress"].
                    ApplyIndependent(BattleGround.HeroParty.Units[j]);

        yield return StartCoroutine(ExecuteEffectEvents(true));

        for (int i = 0; i < brainDecision.TargetInfo.Targets.Count; i++)
            BattleSolver.RemoveConditions(brainDecision.TargetInfo.Targets[i]);
        BattleSolver.RemoveConditions(actionUnit);

        RaidEvents.MonsterTooltip.IsDisabled = false;

        if (!string.IsNullOrEmpty(combatSkillOverride))
            yield break;

        #region Trait Comment Self and Ally

        if(brainDecision.TargetInfo.Type == SkillTargetType.Enemy)
        {
            foreach (var skillEntry in skillResult.SkillEntries)
            {
                if (skillEntry.Target.CombatInfo.IsDead)
                    continue;
                if (skillEntry.Target.Party.Units.Count < 2)
                    continue;

                #region Comment on Self

                if (skillEntry.Target.Character.Trait != null)
                {
                    ReactionType reactionType = skillEntry.IsTargetHit
                        ? ReactionType.CommentSelfHit
                        : ReactionType.CommentSelfMissed;

                    if (RandomSolver.CheckSuccess(skillEntry.Target.Character.Trait.Reactions[reactionType].Chance))
                    {
                        var barkStressEffect = skillEntry.Target.Character.Trait.Reactions[reactionType].Effect;
                        yield return new WaitForSeconds(1f);
                        var barkTarget = RandomSolver.ChooseAnyExcept(skillEntry.Target.Party.Units, skillEntry.Target);
                        foreach (SubEffect subEffect in barkStressEffect.SubEffects)
                            subEffect.Apply(skillEntry.Target, barkTarget, barkStressEffect);
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                        break;
                    }
                }

                #endregion

                #region Comment on Ally

                foreach (var ally in skillEntry.Target.Party.Units)
                {
                    if (ally == skillEntry.Target)
                        continue;
                    if (ally.Character.Trait == null)
                        continue;

                    ReactionType reactionType = skillEntry.IsTargetHit
                        ? ReactionType.CommentAllyHit
                        : ReactionType.CommentAllyMissed;

                    if (RandomSolver.CheckSuccess(ally.Character.Trait.Reactions[reactionType].Chance))
                    {
                        var barkStressEffect = ally.Character.Trait.Reactions[reactionType].Effect;
                        yield return new WaitForSeconds(1f);
                        foreach (SubEffect subEffect in barkStressEffect.SubEffects)
                            subEffect.Apply(ally, skillEntry.Target, barkStressEffect);
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(ExecuteEffectEvents(true));
                        break;
                    }
                }

                #endregion
            }
        }

        #endregion
    }

    protected virtual IEnumerator ExecuteHeroSkill(FormationUnit actionUnit, SkillTargetInfo targetInfo, CombatSkill skill)
    {
        RaidEvents.MonsterTooltip.IsDisabled = true;
        RaidEvents.MonsterTooltip.Hide();
        ExecuteGuardRedirection(actionUnit, targetInfo);
        Formations.HideUnitOverlay();
        TorchMeter.Hide();
        yield return new WaitForSeconds(0.2f);
        DungeonCamera.Zoom(50, 0.05f);
        var skillResult = ExecuteSkillBase(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.05f);
        DungeonCamera.SwitchBlur(true);
        ExecuteSkillAnimationIntro(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.01f);
        ExecuteSkillInstants(actionUnit, targetInfo, skillResult);
        yield return new WaitForSeconds(0.01f);
        ExecuteSlidingSetup(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.70f);
        ExecuteRiposteSkillActivation(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.05f);
        ExecuteRiposteAnimationIntro(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.05f);
        ExecuteRiposteInstants(actionUnit);
        yield return new WaitForSeconds(Riposters.Count > 0 ? 1.2f : 0.7f);
        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
        DungeonCamera.SwitchBlur(false);
        ExecuteSkillAnimationOutro(actionUnit, targetInfo);

        List<DeathDamage> deathDamages = ExecuteBattlegroundDeaths(actionUnit);
        if (deathDamages.Count > 0)
            yield return StartCoroutine(ExecuteDeathDamages(deathDamages));

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

        if (BattleGround.HeroParty.Units.Contains(actionUnit) && BattleGround.HeroParty.Units.Count > 1)
        {
            for (int i = 0; i < actionUnit.Party.Units.Count; i++)
            {
                if (actionUnit == actionUnit.Party.Units[i] || actionUnit.Party.Units[i].Character.Trait == null)
                    continue;

                if (targetInfo.Type != SkillTargetType.Enemy)
                    continue;

                ReactionType reactionType = skillResult.HasHit
                    ? ReactionType.CommentAllyAttackHit
                    : ReactionType.CommentAllyAttackMiss;

                if (RandomSolver.CheckSuccess(actionUnit.Party.Units[i].Character.Trait.Reactions[reactionType].Chance))
                {
                    var barkStressEffect = actionUnit.Party.Units[i].Character.Trait.Reactions[reactionType].Effect;
                    yield return new WaitForSeconds(1f);
                    foreach (SubEffect subEffect in barkStressEffect.SubEffects)
                        subEffect.Apply(actionUnit.Party.Units[i], actionUnit, barkStressEffect);
                    yield return new WaitForSeconds(0.1f);
                    yield return StartCoroutine(ExecuteEffectEvents(false));
                    break;
                }
            }
        }

        #endregion
    }

    protected virtual IEnumerator ExecuteHeroItemUsage(FormationUnit actionUnit, InventorySlot slot)
    {
        if (actionUnit == null || slot.HasItem == false)
        {
            itemUsageEvent = null;
            yield break;
        }

        switch (slot.SlotItem.ItemData.Type)
        {
            case "provision":
                if (actionUnit.Character.HealthRatio < 1)
                {
                    Inventory.DiscardSingleItem(slot);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                    yield return new WaitForSeconds(0.1f);
                    int healthRestored = actionUnit.Character.HealPercent(0.05f, false);
                    RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Heal, healthRestored.ToString());
                    actionUnit.OverlaySlot.UpdateOverlay();
                    Inventory.UpdateState();
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");
                    yield return new WaitForSeconds(0.3f);
                }
                break;
            case "supply":
                switch (slot.SlotItem.ItemData.Id)
                {
                    case "firewood":
                        if(SceneState == DungeonSceneState.Room && CurrentEvent == null)
                        {
                            Inventory.DiscardSingleItem(slot);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            CurrentEvent = CampingEvent(RoomView.RaidRoom.Area as DungeonRoom);
                            StartCoroutine(CurrentEvent);
                        }
                        break;
                    case "bandage":
                        if (actionUnit.Character[StatusType.Bleeding].IsApplied)
                        {
                            #region Trait Block
                            if (actionUnit.Character.Trait != null)
                            {
                                if (RandomSolver.CheckSuccess(actionUnit.Character.Trait.Reactions[ReactionType.BlockItem].Chance))
                                {
                                    actionUnit.CombatInfo.BlockedItems.Add(slot.SlotItem.ItemData.Id);
                                    Inventory.UpdateState();
                                    yield return new WaitForSeconds(1f);
                                    break;
                                }
                            }
                            #endregion
                            Inventory.DiscardSingleItem(slot);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            yield return new WaitForSeconds(0.1f);
                            actionUnit.Character[StatusType.Bleeding].ResetStatus();
                            actionUnit.OverlaySlot.UpdateOverlay();
                            actionUnit.SetTargetItemEffect("bandage");
                            Inventory.UpdateState();
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/bandage");
                            yield return new WaitForSeconds(0.8f);
                        }
                        break;
                    case "antivenom":
                        if (actionUnit.Character[StatusType.Poison].IsApplied)
                        {
                            #region Trait Block
                            if (actionUnit.Character.Trait != null)
                            {
                                if (RandomSolver.CheckSuccess(actionUnit.Character.Trait.Reactions[ReactionType.BlockItem].Chance))
                                {
                                    actionUnit.CombatInfo.BlockedItems.Add(slot.SlotItem.ItemData.Id);
                                    Inventory.UpdateState();
                                    yield return new WaitForSeconds(1f);
                                    break;
                                }
                            }
                            #endregion
                            Inventory.DiscardSingleItem(slot);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            yield return new WaitForSeconds(0.1f);
                            actionUnit.Character[StatusType.Poison].ResetStatus();
                            actionUnit.OverlaySlot.UpdateOverlay();
                            actionUnit.SetTargetItemEffect("antivenom");
                            Inventory.UpdateState();
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/antivenom");
                            yield return new WaitForSeconds(0.8f);
                        }
                        break;
                    case "medicinal_herbs":
                        if (actionUnit.Character.HasDebuffs())
                        {
                            #region Trait Block
                            if (actionUnit.Character.Trait != null)
                            {
                                if (RandomSolver.CheckSuccess(actionUnit.Character.Trait.Reactions[ReactionType.BlockItem].Chance))
                                {
                                    actionUnit.CombatInfo.BlockedItems.Add(slot.SlotItem.ItemData.Id);
                                    Inventory.UpdateState();
                                    yield return new WaitForSeconds(1f);
                                    break;
                                }
                            }
                            #endregion
                            Inventory.DiscardSingleItem(slot);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            yield return new WaitForSeconds(0.1f);
                            RaidPanel.SelectedHero.RemoveCombatDebuffs();
                            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Cured);
                            actionUnit.OverlaySlot.UpdateOverlay();
                            actionUnit.SetTargetItemEffect("medicinal_herbs");
                            Inventory.UpdateState();
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/medicinal_herbs");
                            yield return new WaitForSeconds(0.8f);
                        }
                        break;
                    case "torch":
                        if (TorchMeter.TorchAmount < TorchMeter.MaxAmount)
                        {
                            Inventory.DiscardSingleItem(slot);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            yield return new WaitForSeconds(0.1f);
                            TorchMeter.IncreaseTorch(25);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/torch");
                            Inventory.UpdateState();
                            yield return new WaitForSeconds(0.1f);
                        }
                        break;
                    case "holy_water":
                        #region Trait Block
                        if (actionUnit.Character.Trait != null)
                        {
                            if (RandomSolver.CheckSuccess(actionUnit.Character.Trait.Reactions[ReactionType.BlockItem].Chance))
                            {
                                actionUnit.CombatInfo.BlockedItems.Add(slot.SlotItem.ItemData.Id);
                                Inventory.UpdateState();
                                yield return new WaitForSeconds(1f);
                                break;
                            }
                        }
                        #endregion
                        Inventory.DiscardSingleItem(slot);
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                        yield return new WaitForSeconds(0.1f);

                        var holyWaterEffect = DarkestDungeonManager.Data.Effects["holy_water"];
                        for (int i = 0; i < holyWaterEffect.SubEffects.Count; i++)
                            holyWaterEffect.SubEffects[i].ApplyQueued(RaidPanel.SelectedUnit, RaidPanel.SelectedUnit, holyWaterEffect);
                        actionUnit.OverlaySlot.UpdateOverlay();
                        actionUnit.SetTargetItemEffect("holy_water");

                        Inventory.UpdateState();
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/holy_water");
                        yield return new WaitForSeconds(0.8f);
                        break;
                    case "dog_treats":
                        if (actionUnit.Character.Class == "hound_master")
                        {
                            #region Trait Block
                            if (actionUnit.Character.Trait != null)
                            {
                                if (RandomSolver.CheckSuccess(actionUnit.Character.Trait.Reactions[ReactionType.BlockItem].Chance))
                                {
                                    actionUnit.CombatInfo.BlockedItems.Add(slot.SlotItem.ItemData.Id);
                                    Inventory.UpdateState();
                                    yield return new WaitForSeconds(1f);
                                    break;
                                }
                            }
                            #endregion
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            yield return new WaitForSeconds(0.1f);

                            var dogEffect = DarkestDungeonManager.Data.Effects["dog_treats"];
                            for (int i = 0; i < dogEffect.SubEffects.Count; i++)
                                dogEffect.SubEffects[i].ApplyQueued(RaidPanel.SelectedUnit, RaidPanel.SelectedUnit, dogEffect);
                            actionUnit.OverlaySlot.UpdateOverlay();
                            actionUnit.SetTargetItemEffect("dog_treat");

                            Inventory.UpdateState();
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/dog_treats");
                            yield return new WaitForSeconds(0.8f);
                        }
                        break;
                }
                break;
        }
        actionUnit.Character.ApplyAllBuffRules(Rules.GetIdleUnitRules(actionUnit));
        itemUsageEvent = null;
    }

    #endregion

    protected virtual IEnumerator ExecuteResolveChecks()
    {
        while(ResolveCheckQueue.Count > 0)
        {
            var resolveUnit = ResolveCheckQueue[0];
            var resolveHero = resolveUnit.Character as Hero;
            ResolveCheckQueue.RemoveAt(0);
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

            RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("resolve_test"), resolveUnit.Character.Name));

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

            if(isVirtue)
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
            Formations.PartyBuffPositions.SetUnitTarget(resolveUnit, 0.05f, Vector2.zero);
            RaidEvents.ShowAnnouncment(isVirtue ? 
                LocalizationManager.GetString("str_virtue_name_" + resolveTrait.Id):
                LocalizationManager.GetString("str_affliction_name_" + resolveTrait.Id), AnnouncmentPosition.Bottom);
            if(!Rules.IsDoingCamping)
                DungeonCamera.Zoom(45, 0.1f);

            yield return new WaitForSeconds(2.45f);
            if(!Rules.IsDoingCamping)
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
    }

    protected virtual IEnumerator ExecuteRoundAdvance()
    {
        roundAdvanceCounter++;

        while (UnitEventQueue.Count > 0)
            yield return null;

        UnitEventQueue.AddRange(Formations.Heroes.Party.Units);

        bleedDeaths.Clear();
        poisonDeaths.Clear();
        float timeWasted = 0;
        bool executedBleed = false;
        bool executedPoison = false;
        bool movementStopped = false;

        #region Bleeding

        for (int i = UnitEventQueue.Count - 1; i >= 0; i--)
        {
            if (UnitEventQueue[i].CombatInfo.IsDead)
                continue;

            if (!UnitEventQueue[i].Character[StatusType.Bleeding].IsApplied)
                continue;

            var bleedEffect = (BleedingStatusEffect)UnitEventQueue[i].Character[StatusType.Bleeding];
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
            executedBleed = true;

            if (ProcessDamage(UnitEventQueue[i], bleedEffect.CurrentTickDamage))
            {
                if (PartyController.MovementAllowed)
                {
                    movementStopped = true;
                    DisablePartyMovement();
                }
                bleedDeaths.Add(UnitEventQueue[i]);
            }
        }

        if(executedBleed)
        {
            yield return new WaitForSeconds(0.6f);
            timeWasted += 0.6f;

            float coroutineTime = Time.time;
            yield return StartCoroutine(ExecuteEffectEvents(false));
            UnitEventQueue.AddRange(Formations.Heroes.Party.Units);
            timeWasted += Time.time - coroutineTime;
        }

        #endregion

        #region Poisoning

        for (int i = UnitEventQueue.Count - 1; i >= 0; i--)
        {
            if (UnitEventQueue[i].CombatInfo.IsDead)
                continue;

            if (!UnitEventQueue[i].Character[StatusType.Poison].IsApplied)
                continue;

            var poisonEffect = (BleedingStatusEffect)UnitEventQueue[i].Character[StatusType.Poison];
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/poison_dot");
            executedPoison = true;

            if (ProcessDamage(UnitEventQueue[i], poisonEffect.CurrentTickDamage))
            {
                if (PartyController.MovementAllowed)
                {
                    movementStopped = true;
                    DisablePartyMovement();
                }
                poisonDeaths.Add(UnitEventQueue[i]);
            }
        }

        if (executedPoison)
        {
            yield return new WaitForSeconds(0.6f);
            timeWasted += 0.6f;

            float coroutineTime = Time.time;
            yield return StartCoroutine(ExecuteEffectEvents(false));
            UnitEventQueue.AddRange(Formations.Heroes.Party.Units);
            timeWasted += Time.time - coroutineTime;
        }

        #endregion

        #region Deaths

        for (int i = 0; i < UnitEventQueue.Count; i++)
        {
            UnitEventQueue[i].Character.UpdateRound();
            UnitEventQueue[i].OverlaySlot.UpdateOverlay();
        }

        if(bleedDeaths.Count > 0)
        {
            yield return new WaitForSeconds(1.4f - timeWasted);
            timeWasted += 1.4f - timeWasted;
            for (int i = 0; i < bleedDeaths.Count; i++)
                ExecuteDeath(bleedDeaths[i]);
            bleedDeaths.Clear();
        }
        if(poisonDeaths.Count > 0)
        {
            yield return new WaitForSeconds(2 - timeWasted);
            for (int i = 0; i < poisonDeaths.Count; i++)
                ExecuteDeath(poisonDeaths[i]);
            poisonDeaths.Clear();
        }

        #endregion

        yield return StartCoroutine(ExecuteEffectEvents(false));
        UnitEventQueue.Clear();

        #region Comment on Movement
        if (Formations.Heroes.Party.Units.Count > 1)
        {
            foreach (var heroBarker in Formations.Heroes.Party.Units)
            {
                if (heroBarker.Character.Trait != null)
                {
                    if (RandomSolver.CheckSuccess(heroBarker.Character.Trait.Reactions[ReactionType.CommentMove].Chance))
                    {
                        var barkStressEffect = heroBarker.Character.Trait.Reactions[ReactionType.CommentMove].Effect;
                        yield return new WaitForSeconds(1f);
                        TempList.Clear();
                        TempList.AddRange(Formations.Heroes.Party.Units);
                        TempList.Remove(heroBarker);
                        var barkTarget = TempList[RandomSolver.Next(TempList.Count)];
                        TempList.Clear();
                        for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                            barkStressEffect.SubEffects[i].Apply(heroBarker, barkTarget, barkStressEffect);
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(ExecuteEffectEvents(false));
                        break;
                    }
                }
            }
        }
        #endregion

        if (movementStopped)
            EnablePartyMovement();

        roundAdvanceCounter--;
    }

    protected virtual IEnumerator ExecuteEffectEvents(bool includeMonsters, float waitAfter = 0.0f)
    {
        executingEffectEvent = true;
        bool executedEvent;

        for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
            BattleGround.HeroParty.Units[i].StackEvents();
        if (includeMonsters)
            for (int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
                BattleGround.MonsterParty.Units[i].StackEvents();

        do
        {
            #region Death Doors
            if (DeathDoorEnterQueue.Count > 0)
            {
                if (RaidEvents.CampEvent.ActionType == CampUsageResultType.Skill)
                    RaidEvents.CampEvent.Hide();

                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/deaths_door");
                DarkestSoundManager.ExecuteNarration("deaths_door", NarrationPlace.Raid);

                foreach (var deathDoorUnit in DeathDoorEnterQueue)
                {
                    if (deathDoorUnit.Character.AtDeathsDoor)
                        Debug.LogError("Already at deaths door!");
                    (deathDoorUnit.Character as Hero).ApplyDeathDoor();
                    deathDoorUnit.Character.ApplySingleBuffRule(Rules.GetIdleUnitRules(deathDoorUnit), BuffRule.DeathsDoor);
                    deathDoorUnit.SetHalo("deaths_door");
                    deathDoorUnit.OverlaySlot.UpdateOverlay();
                    DarkestDungeonManager.Data.Effects["BarkStress"].ApplyIndependent(deathDoorUnit);
                }

                string deathDoorHeroes = null;
                switch (DeathDoorEnterQueue.Count)
                {
                    case 1:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_1_death"),
                            DeathDoorEnterQueue[0].Character.Name);
                        break;
                    case 2:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_2_death"),
                            DeathDoorEnterQueue[0].Character.Name, DeathDoorEnterQueue[1].Character.Name);
                        break;
                    case 3:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_3_death"),
                            DeathDoorEnterQueue[0].Character.Name, DeathDoorEnterQueue[1].Character.Name,
                            DeathDoorEnterQueue[2].Character.Name);
                        break;
                    case 4:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_4_death"),
                            DeathDoorEnterQueue[0].Character.Name, DeathDoorEnterQueue[1].Character.Name,
                            DeathDoorEnterQueue[2].Character.Name, DeathDoorEnterQueue[3].Character.Name);
                        break;
                    default:
                        Debug.LogError("Too much deathdoors!");
                        break;
                }
                if (DeathDoorEnterQueue.Count > 1)
                    RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("str_ui_deathdoor_multy"), deathDoorHeroes));
                else
                    RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("str_ui_deathdoor"), deathDoorHeroes));

                DeathDoorEnterQueue.Clear();
                yield return new WaitForSeconds(1.6f);
                RaidEvents.HideAnnouncment();
            }
            #endregion

            #region Effects
            do
            {
                executedEvent = false;
                UnitEventQueue.Clear();
                UnitEventQueue.AddRange(BattleGround.HeroParty.Units);
                if (includeMonsters)
                    UnitEventQueue.AddRange(BattleGround.MonsterParty.Units);

                while (UnitEventQueue.Count > 0)
                {
                    var eventUnit = UnitEventQueue[0];
                    UnitEventQueue.Remove(eventUnit);

                    if (eventUnit != null && eventUnit.EventQueue.Count > 0)
                    {
                        var eventEffect = eventUnit.EventQueue[0];
                        eventUnit.EventQueue.RemoveAt(0);

                        eventEffect.Execute();
                        executedEvent = true;
                        if (eventEffect.SubEffect is StressEffect || eventEffect.SubEffect is StressHealEffect)
                            yield return new WaitForSeconds(0.25f);

                        if (eventUnit.CombatInfo.MarkedForDeath)
                        {
                            eventUnit.CombatInfo.IsDead = true;
                            ExecuteDeath(eventUnit);
                        }
                    }
                }

                if (executedEvent)
                    yield return new WaitForSeconds(1f);
            } 
            while (executedEvent);
            #endregion

            #region Resolve Checks
            if (ResolveCheckQueue.Count != 0)
            {
                executedEvent = true;
                if (RaidEvents.CampEvent.ActionType == CampUsageResultType.Skill)
                    RaidEvents.CampEvent.Hide();
                yield return StartCoroutine(ExecuteResolveChecks());
            }
            #endregion

            #region Heart Attacks
            while (HeartAttackCheckQueue.Count > 0)
            {
                executedEvent = true;
                var heartAttackedUnit = HeartAttackCheckQueue[0];
                HeartAttackCheckQueue.RemoveAt(0);

                if (heartAttackedUnit.Character.AtDeathsDoor)
                {
                    heartAttackedUnit.CombatInfo.MarkedForDeath = true;
                    PrepareDeath(heartAttackedUnit);
                    RaidEvents.ShowPopupMessage(heartAttackedUnit, PopupMessageType.HeartAttack, "", 100);
                    yield return new WaitForSeconds(0.2f);
                    RaidEvents.ShowPopupMessage(heartAttackedUnit, PopupMessageType.DeathBlow);
                    yield return new WaitForSeconds(1.2f);
                    ExecuteDeath(heartAttackedUnit);
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    RaidEvents.ShowPopupMessage(heartAttackedUnit, PopupMessageType.HeartAttack, "", 100);
                    heartAttackedUnit.Character.TakeDamagePercent(1.0f);
                    heartAttackedUnit.Character.Stress.ValueRatio = 0.75f;
                    heartAttackedUnit.OverlaySlot.UpdateOverlay();
                    yield return new WaitForSeconds(0.2f);
                    DeathDoorEnterQueue.Add(heartAttackedUnit);
                }
            }
            #endregion

            if (DeathDoorEnterQueue.Count != 0 || ResolveCheckQueue.Count != 0 || HeartAttackCheckQueue.Count != 0)
                executedEvent = true;
        } 
        while (executedEvent);

        for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
            BattleGround.HeroParty.Units[i].Character.ApplyAllBuffRules(
                Rules.GetIdleUnitRules(BattleGround.HeroParty.Units[i]));

        if (waitAfter > 0.0f)
            yield return new WaitForSeconds(waitAfter);

        executingEffectEvent = false;
    }

    protected virtual IEnumerator ExecuteRandomDialog(FormationUnit unit, string dialogId)
    {
        if (RandomSolver.CheckSuccess(DarkestDungeonManager.RandomBarkChance))
        {
            unit.OverlaySlot.StartDialog(LocalizationManager.GetString(dialogId));
            while (unit.OverlaySlot.IsDoingDialog)
                yield return null;
        }
    }

    private IEnumerator HungerEvent()
    {
        DisableEnviroment();
        Inventory.SetDeactivated();
        RaidPanel.SetDisabledState();
        RaidPanel.HeroPanel.EquipmentPanel.SetDisabled();
        QuestPanel.DisableRetreat(false);
        RaidPanel.BannerPanel.SetDisabledState();
        DisablePartyMovement();

        RaidEvents.LoadHungerMeal();
        yield return new WaitForEndOfFrame();
        RaidEvents.HungerEvent.Show();

        while (true)
        {
            if (RaidEvents.HungerEvent.ActionType == HungerResultType.Wait)
                yield return null;
            else
                break;
        }

        if(RaidEvents.HungerEvent.ActionType == HungerResultType.Eat)
            Inventory.DiscardItemType("provision", RaidEvents.HungerEvent.MealAmount);

        RaidEvents.HungerEvent.Hide();
        yield return new WaitForSeconds(0.2f);

        if(RaidEvents.HungerEvent.ActionType == HungerResultType.Starve)
            yield return StartCoroutine(ProcessStarvation());
        else if (RaidEvents.HungerEvent.ActionType == HungerResultType.Eat)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");

            for (int i = 0; i < HeroParty.Units.Count; i++)
            {
                int mealHeal = HeroParty.Units[i].Character.HealPercent(0.1f, false);
                HeroParty.Units[i].OverlaySlot.UpdateOverlay();
                RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Heal, mealHeal.ToString());
            }

            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return new WaitForSeconds(0.6f);

            yield return ProcessRaidFailure();
        }

        yield return new WaitForSeconds(0.1f);
        QuestPanel.EnableRetreat();
        EnablePartyMovement();
        EnableEnviroment();
        Inventory.SetPeacefulState(false);
        RaidPanel.HeroPanel.EquipmentPanel.SetActive();
        RaidPanel.SetPeacefulState();
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
        CurrentEvent = null;
    }

    private IEnumerator LootEvent()
    {
        if (RaidEvents.LootEvent.KeepLoot)
        {
            yield return new WaitForSeconds(3f);
            RaidEvents.LootEvent.Close();
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            DarkestSoundManager.ExecuteNarration("loot", NarrationPlace.Raid);

            Inventory.SetPeacefulState(true);
            float announcementTimer = -1;
            while (true)
            {
                if (announcementTimer > 0)
                {
                    announcementTimer -= Time.deltaTime;
                    if (announcementTimer <= 0)
                    {
                        if (RaidEvents.Announcment.Animator.isInitialized)
                            RaidEvents.HideAnnouncment();
                        announcementTimer = -1;
                    }
                }

                if (RaidEvents.LootEvent.ActionType == LootResultType.NotEnoughSpace)
                {
                    RaidEvents.LootEvent.ResetWaiting();
                    RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_discard_items_from_your_inventory"));
                    announcementTimer = 1;
                }

                if (RaidEvents.LootEvent.ActionType == LootResultType.Waiting)
                    yield return null;
                else
                {
                    if (RaidEvents.Announcment.Animator.isInitialized)
                        RaidEvents.HideAnnouncment();
                    break;
                }
            }
        }

        if (!Raid.QuestCompleted && Raid.CheckQuestGoals())
            yield return StartCoroutine(CompletionCrestEvent());
    }
    
    private IEnumerator ScoutingEvent(DungeonRoom room)
    {
        currentScoutedRooms.Clear();

        float scoutingChance = 0.25f + TorchMeter.CurrentRange.ScoutingChance + 
            Formations.Heroes.Party.Units.Sum(unit => unit.Character[AttributeType.ScoutingChance].ModifiedValue);

        int scoutingTiles;
        if (scoutingChance > 1)
            scoutingTiles = RandomSolver.CheckSuccess(scoutingChance / 2) ? 12 : 6;
        else
            scoutingTiles = RandomSolver.CheckSuccess(scoutingChance) ? RandomSolver.CheckSuccess(0.5f) ? 12 : 6 : 0;

        if(scoutingTiles > 0)
        {
            RaidPanel.SwitchBlocked = true;
            if (!RaidPanel.IsMapActive)
                RaidPanel.RightPanelSwitched(true);
            MapPanel.FocusTarget(1.2f);
            yield return new WaitForSeconds(0.6f);

            MapPanel.SetScoutingRadar();
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_start");
            yield return new WaitForSeconds(1f);
            
            foreach (Door door in room.Doors)
                StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[door.TargetArea], door.Direction, scoutingTiles));

            while (scoutingCounter > 0)
                yield return null;

            RaidPanel.SwitchBlocked = false;
        }
        currentScoutedRooms.Clear();
    }

    private IEnumerator CurioScoutingEvent(Area area, CurioResult result)
    {
        RaidPanel.LockOnMap();
        MapPanel.FocusTarget(1f);
        yield return new WaitForSeconds(0.6f);

        MapPanel.SetScoutingRadar();
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_start");
        yield return new WaitForSeconds(1f);

        if (result.Item.EndsWith("all"))
        {
            int scoutingTiles;
            if (result.Item.StartsWith("0"))
            {
                scoutingTiles = 1000;
            }
            else
                scoutingTiles = (int.Parse(result.Item.Substring(0, 1)) - 1) * 6;

            if (scoutingTiles > 0)
            {
                var sector = area as HallSector;
                if(sector != null)
                {
                    StartCoroutine(ScoutingHallway(sector.Hallway, Direction.Left, scoutingTiles, sector));
                    StartCoroutine(ScoutingHallway(sector.Hallway, Direction.Right, scoutingTiles, sector));
                }
                else
                {
                    var room = (DungeonRoom)area;
                    foreach (Door door in room.Doors)
                        StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[door.TargetArea], door.Direction, scoutingTiles));
                }

                while (scoutingCounter > 0)
                    yield return null;
            }
        }
        else if (result.Item.EndsWith("traps"))
        {
            MapPanel.FocusTarget(0.8f);
            yield return new WaitForSeconds(0.3f);

            foreach (var hallway in Raid.Dungeon.Hallways.Values)
            {
                foreach(var hallwaySlot in hallway.Halls)
                {
                    if (hallwaySlot.Type == AreaType.Trap && hallwaySlot.Knowledge == Knowledge.Hidden)
                    {
                        hallwaySlot.Scout();
                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
        }
        else if (result.Item.EndsWith("obstacles"))
        {
            MapPanel.FocusTarget(0.8f);
            yield return new WaitForSeconds(0.3f);

            foreach (var hallway in Raid.Dungeon.Hallways.Values)
            {
                foreach (var hallwaySlot in hallway.Halls)
                {
                    if (hallwaySlot.Type == AreaType.Obstacle && hallwaySlot.Knowledge == Knowledge.Hidden)
                    {
                        hallwaySlot.Scout();
                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
        }
        else if (result.Item.EndsWith("hall_battles"))
        {
            MapPanel.FocusTarget(0.8f);
            yield return new WaitForSeconds(0.3f);

            foreach (var hallway in Raid.Dungeon.Hallways.Values)
            {
                foreach (var hallwaySlot in hallway.Halls)
                {
                    if (hallwaySlot.HasActiveBattle && hallwaySlot.Knowledge == Knowledge.Hidden)
                    {
                        hallwaySlot.Scout();
                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
        }
        else if (result.Item.EndsWith("room_battles"))
        {
            MapPanel.FocusTarget(0.8f);
            yield return new WaitForSeconds(0.3f);

            foreach (var room in Raid.Dungeon.Rooms.Values)
            {
                if (room.HasActiveBattle && room.Knowledge == Knowledge.Hidden)
                {
                    room.Scout();
                    yield return new WaitForSeconds(0.6f);
                }
            }
        }
        else if (result.Item.EndsWith("curios"))
        {
            foreach (var room in Raid.Dungeon.Rooms.Values)
            {
                if ((room.Type == AreaType.Curio || room.Type == AreaType.BattleCurio) && room.Knowledge == Knowledge.Hidden)
                {
                    room.Scout();
                    yield return new WaitForSeconds(0.6f);
                }
            }
            foreach (var hallway in Raid.Dungeon.Hallways.Values)
            {
                foreach (var hallwaySlot in hallway.Halls)
                {
                    if (hallwaySlot.Type == AreaType.Curio && hallwaySlot.Knowledge == Knowledge.Hidden)
                    {
                        hallwaySlot.Scout();
                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
        }

        RaidPanel.SwitchBlocked = false;
        currentScoutedRooms.Clear();
    }

    private IEnumerator CurioEvent(IRaidArea areaView, Quirk triggerQuirk = null, Trait triggerTrait = null)
    {
        QuestPanel.DisableRetreat(false);
        RaidPanel.SetDisabledState();
        Inventory.SetDeactivated();
        DisablePartyMovement();
        DisableEnviroment();
        if (triggerQuirk != null || triggerTrait != null)
        {
            yield return new WaitForSeconds(1f);
            Formations.LockSelections();
        }

        while (IsUnitEventInProgress)
            yield return null;

        DisablePartyMovement();
        Curio curio = areaView.Area.Prop as Curio;
        CurioInteraction curioInteraction = null;
        CurioResult curioResult = null;

        if(curio.IsQuestCurio || (curio.IsFullCurio && triggerQuirk == null && triggerTrait == null))
        {
            Inventory.SetInteractionState(curio.IsQuestCurio);
            yield return StartCoroutine(WaitCurioChoise(areaView, curio));

            switch (RaidEvents.InteractionEvent.ActionType)
            {
                case InteractionResultType.Cancel:
                    QuestPanel.EnableRetreat();
                    Formations.UnlockSelections();
                    RaidPanel.SetPeacefulState();
                    Inventory.SetPeacefulState(false);
                    EnablePartyMovement();
                    EnableEnviroment();
                    Formations.UnlockSelections();

                    CurrentEvent = null;
                    yield break;
                case InteractionResultType.ItemInteraction:
                    curioInteraction = curio.ItemInteractions.Find(action =>
                        action.ItemId == RaidEvents.InteractionEvent.SelectedItem.Id);
                    curioResult = RandomSolver.ChooseByRandom(curioInteraction.Results);
                    break;
                case InteractionResultType.ManualInteraction:
                    curioInteraction = RandomSolver.ChooseByRandom(curio.Results);
                    curioResult = RandomSolver.ChooseByRandom(curioInteraction.Results);
                    break;
            }
        }
        else
        {
            curioInteraction = RandomSolver.ChooseByRandom(curio.Results);
            curioResult = RandomSolver.ChooseByRandom(curioInteraction.Results);
        }

        Inventory.SetDeactivated();

        var oneShotCurio = FMODUnity.RuntimeManager.CreateInstance("event:/props/curios/" + curio.OriginalId);
        if (oneShotCurio != null)
        {
            if (curioInteraction is ItemInteraction)
            {
                oneShotCurio.setParameterValue("item_index", curio.ItemInteractions.IndexOf(curioInteraction as ItemInteraction) + 1);
                oneShotCurio.setParameterValue("result_category", 2);
            }
            else
            {
                oneShotCurio.setParameterValue("item_index", 0);
                if (curio.IsQuestCurio)
                    oneShotCurio.setParameterValue("result_category", 2);
                else if (curio.ResultTypes == "mixed")
                    oneShotCurio.setParameterValue("result_category", 1);
                else if (curio.ResultTypes == "good")
                    oneShotCurio.setParameterValue("result_category", 2);
                else
                    oneShotCurio.setParameterValue("result_category", 0);
            }
            oneShotCurio.start();
            oneShotCurio.release();
        }

        Formations.LockSelections();

        #region Curio Investigation
        string stringId = "str_curio_" + curio.OriginalId + "_" + curioInteraction.ResultString();
        string message = LocalizationManager.GetString(stringId);

        if (stringId == message && curioResult != null)
        {
            stringId = stringId + "_" + curioResult.Item;
            message = LocalizationManager.GetString(stringId + "_" + curioResult.Item);
        }

        areaView.CompleteArea();
        if(curio.OriginalId == "beacon" || curio.OriginalId == "teleporter")
            (areaView.Prop as RaidCurio).SkeletonAnimation.state.AddAnimation(0, "disturbed", true, 1.8f);
        
        Formations.InvestigateCurioIntro(areaView.Prop);
        if(stringId != message)
            RaidEvents.ShowAnnouncment(message);
        DungeonCamera.Zoom(50, 0.1f);

        GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/interaction_curio");
        if (animationObject != null)
        {
            AnimatedEffect effect = Instantiate(animationObject).GetComponent<AnimatedEffect>();
            if (effect != null)
            {
                effect.SkeletonAnimation.state.SetAnimation(0, "heroic", false);
                effect.BindToTarget(areaView.Prop.RectTransform, (areaView.Prop as RaidCurio).SkeletonAnimation, "root");
                effect.SkeletonAnimation.MeshRenderer.sortingOrder = (areaView.Prop as RaidCurio).
                    SkeletonAnimation.MeshRenderer.sortingOrder - 1;
            }
        }
        yield return new WaitForSeconds(2f);
        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
        Formations.InvestigateCurioOutro(areaView);
        yield return new WaitForSeconds(0.10f);
        areaView.Prop.SetSortingOrder(PartyFormationManager.BackgroundOrder);
        yield return new WaitForSeconds(0.05f);
        if (stringId != message)
            RaidEvents.HideAnnouncment();
        Formations.ShowHeroOverlay();
        #endregion
        var interactorUnit = RaidPanel.SelectedUnit;

        switch (curioInteraction.ResultType)
        {
            case "nothing":
            case "teleport":
                break;
            case "summon":
                #region Summon
                if (Raid.Dungeon.DungeonMash.NamedEncounters.ContainsKey(curioResult.Item))
                {
                    var summonMash = RandomSolver.ChooseByRandom(Raid.Dungeon.DungeonMash.NamedEncounters[curioResult.Item]);
                    if (summonMash != null)
                    {
                        areaView.Area.BattleEncounter = new BattleEncounter(summonMash.MonsterSet);
                        CurrentEvent = EncounterEvent(areaView);
                        StartCoroutine(CurrentEvent);
                        yield break;
                    }
                }
                #endregion
                break;
            case "scouting":
                yield return StartCoroutine(CurioScoutingEvent(areaView.Area, curioResult));
                break;
            case "loot":
                #region Curio Loot Event
                if(triggerQuirk != null)
                    RaidEvents.LoadCurioLoot(curio, curioInteraction, curioResult, triggerQuirk.KeepLoot);
                else if (triggerTrait != null)
                    RaidEvents.LoadCurioLoot(curio, curioInteraction, curioResult, triggerTrait.KeepLoot);
                else
                    RaidEvents.LoadCurioLoot(curio, curioInteraction, curioResult);

                Formations.UnlockSelections();
                if (RaidEvents.LootEvent.HasSomething)
                    yield return StartCoroutine(LootEvent());

                if (curio.IsQuestCurio && !Raid.QuestCompleted && Raid.CheckQuestGoals())
                    yield return StartCoroutine(CompletionCrestEvent());
                #endregion
                break;
            case "quirk":
                #region Curio Quirk Event
                if (curioResult.Item == "positive")
                {
                    var newPositiveQuirk = (RaidPanel.SelectedUnit.Character as Hero).AddPositiveQuirk();
                    if(newPositiveQuirk != null)
                    {
                        RaidPanel.SelectedUnit.SetHalo("quirk");
                        RaidEvents.ShowPopupMessage(RaidPanel.SelectedUnit, PopupMessageType.PositiveQuirk,
                            LocalizationManager.GetString("str_quirk_name_" + newPositiveQuirk.Id));
                        yield return new WaitForSeconds(1f);
                    }
                }
                else if (curioResult.Item == "negative")
                {
                    var newNegativeQuirk = (RaidPanel.SelectedUnit.Character as Hero).AddNegativeQuirk();
                    if (newNegativeQuirk != null)
                    {
                        RaidPanel.SelectedUnit.SetHalo("quirk");
                        RaidEvents.ShowPopupMessage(RaidPanel.SelectedUnit, PopupMessageType.NegativeQuirk,
                            LocalizationManager.GetString("str_quirk_name_" + newNegativeQuirk.Id));
                        yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    Quirk newQuirk;
                    if (DarkestDungeonManager.Data.Quirks.ContainsKey(curioResult.Item))
                        newQuirk = DarkestDungeonManager.Data.Quirks[curioResult.Item];
                    else
                    {
                        Debug.LogError("Quirk " + curioResult.Item + " in curio " + curio.StringId + " not found!");
                        break;
                    }
                    if ((RaidPanel.SelectedUnit.Character as Hero).AddQuirk(newQuirk))
                    {
                        RaidPanel.SelectedUnit.SetHalo("quirk");
                        RaidEvents.ShowPopupMessage(RaidPanel.SelectedUnit, newQuirk.IsPositive ?
                            PopupMessageType.PositiveQuirk : PopupMessageType.NegativeQuirk,
                            LocalizationManager.GetString("str_quirk_name_" + newQuirk.Id));
                        yield return new WaitForSeconds(1f);
                    }
                }
                #endregion
                break;
            case "effect":
                #region Curio Effect Event
                Effect effect;
                if (DarkestDungeonManager.Data.Effects.ContainsKey(curioResult.Item))
                    effect = DarkestDungeonManager.Data.Effects[curioResult.Item];
                else
                {
                    Debug.LogError("Effect " + curioResult.Item + " in curio " + curio.StringId + " not found!");
                    break;
                }

                BattleSolver.SkillResult.Reset();
                BattleSolver.SkillResult.AddResultEntry(new SkillResultEntry(RaidPanel.SelectedUnit, SkillResultType.Hit));

                if(effect.TargetType == EffectTargetType.Performer)
                    effect.Apply(RaidPanel.SelectedUnit, RaidPanel.SelectedUnit, BattleSolver.SkillResult);
                else
                    effect.Apply(null, RaidPanel.SelectedUnit, BattleSolver.SkillResult);

                yield return StartCoroutine(ExecuteEffectEvents(false));
                #endregion
                break;
            case "purge":
                #region Curio Purge Effect
                if (curioResult.Item != "negative")
                    Debug.LogError("Purge type " + curioResult.Item + " in curio " + curio.StringId + " is unknown!");

                Quirk removedQuirk = (RaidPanel.SelectedUnit.Character as Hero).RemoveNegativeQuirk();
                if(removedQuirk != null)
                {
                    RaidPanel.SelectedUnit.SetHalo("quirk");
                    RaidEvents.ShowPopupMessage(RaidPanel.SelectedUnit, PopupMessageType.QuirkRemoved,
                        LocalizationManager.GetString("str_quirk_name_" + removedQuirk.Id));
                    yield return new WaitForSeconds(1f);
                }
                #endregion
                break;
            case "disease":
                #region Curio Disease Effect
                Quirk disease;
                if (curioResult.Item == "random")
                    disease = (RaidPanel.SelectedUnit.Character as Hero).AddRandomDisease();
                else
                {
                    if (DarkestDungeonManager.Data.Quirks.ContainsKey(curioResult.Item))
                        disease = DarkestDungeonManager.Data.Quirks[curioResult.Item];
                    else
                    {
                        Debug.LogError("Disease " + curioResult.Item + " in curio " + curio.StringId + " not found!");
                        break;
                    }
                }
                if (disease != null)
                {
                    RaidPanel.SelectedUnit.SetHalo("disease");
                    RaidEvents.ShowPopupMessage(RaidPanel.SelectedUnit, PopupMessageType.Disease,
                        LocalizationManager.GetString("str_quirk_name_" + disease.Id));
                    yield return new WaitForSeconds(1f);
                }
                #endregion
                break;
        }

        #region Comment on Curio
        if (Formations.Heroes.Party.Units.Count > 1 && Formations.Heroes.Party.Units.Contains(interactorUnit))
        {
            foreach (var heroBarker in Formations.Heroes.Party.Units)
            {
                if (heroBarker != interactorUnit && heroBarker.Character.Trait != null)
                {
                    if (RandomSolver.CheckSuccess(heroBarker.Character.Trait.Reactions[ReactionType.CommentCurioInteraction].Chance))
                    {
                        var barkStressEffect = heroBarker.Character.Trait.Reactions[ReactionType.CommentCurioInteraction].Effect;
                        yield return new WaitForSeconds(1f);
                        for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                            barkStressEffect.SubEffects[i].Apply(heroBarker, interactorUnit, barkStressEffect);
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(ExecuteEffectEvents(false));
                        break;
                    }
                }
            }
        }
        #endregion

        QuestPanel.EnableRetreat();
        Inventory.SetPeacefulState(false);
        Formations.UnlockSelections();
        RaidPanel.SetPeacefulState();
        EnablePartyMovement();
        EnableEnviroment();
        if (triggerQuirk != null || triggerTrait != null)
            Formations.UnlockSelections();
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
        CurrentEvent = null;
    }

    private IEnumerator TrapEvent(RaidHallSector sector, bool handActivation)
    {
        QuestPanel.DisableRetreat(false);
        DisablePartyMovement();
        RaidPanel.BannerPanel.SetDisabledState();
        Inventory.SetDeactivated();
        Formations.HideUnitOverlay();
        Formations.LockSelections();

        while (IsUnitEventInProgress)
            yield return null;

        RaidTrap raidTrap = sector.Prop as RaidTrap;
        Trap trap = sector.Area.Prop as Trap;
        FormationUnit trapTarget = RaidPanel.SelectedUnit;
        bool isDisarmed = false;
        sector.CompleteArea();

        yield return new WaitForSeconds(0.30f);

        if(handActivation)
        {
            float disarmChance = RaidPanel.SelectedUnit.Character.GetSingleAttribute(AttributeType.Trap).ModifiedValue;
            disarmChance -= Mathf.RoundToInt(CurrentRaid.Quest.Difficulty / 2.0f) * 0.2f;

            if (RandomSolver.CheckSuccess(disarmChance))
                isDisarmed = true;
            else
                DarkestSoundManager.ExecuteNarration("trap", NarrationPlace.Raid);
        }
        DungeonCamera.Zoom(50, 0.1f);
        yield return new WaitForSeconds(0.10f);
        raidTrap.SkeletonAnimation.MeshRenderer.enabled = true;
        Formations.InvestigateTrapIntro(raidTrap, isDisarmed);
        Formations.HeroesAttackMeleePosition.SetUnitTarget(trapTarget, 0.05f, new Vector2(200, 0));
        Formations.MonstersAttackMeleePosition.SetTrap(raidTrap, 0.05f, new Vector2(-200, -60));

        if (isDisarmed)
        {
            #region Animation
            GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/interaction_curio");
            if (animationObject != null)
            {
                AnimatedEffect effect = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                if (effect != null)
                {
                    effect.SkeletonAnimation.MeshRenderer.sortingOrder = raidTrap.SkeletonAnimation.MeshRenderer.sortingOrder - 1;
                    effect.SkeletonAnimation.state.SetAnimation(0, "heroic", false);
                    effect.BindToTarget(raidTrap.RectTransform, raidTrap.SkeletonAnimation, "root");
                }
            }
            #endregion
        }
        yield return new WaitForSeconds(0.05f);
        #region Effects Execution
        BattleSolver.SkillResult.Reset();

        if (isDisarmed)
        {
            BattleSolver.SkillResult.AddResultEntry(new SkillResultEntry(trapTarget, SkillResultType.Hit));
            FMODUnity.RuntimeManager.PlayOneShot("event:/props/traps/" + trap.StringId + "_disarm");

            if(CurrentRaid.Quest.Difficulty == 1)
            {
                foreach(var succesEffect in trap.SuccessEffects)
                    succesEffect.Apply(null, trapTarget, BattleSolver.SkillResult);
            }
            else
            {
                TrapVariation variation;
                if(trap.Variations.ContainsKey(CurrentRaid.Quest.Difficulty))
                    variation = trap.Variations[CurrentRaid.Quest.Difficulty];
                else
                    variation = trap.Variations[5];

                foreach (var succesEffect in variation.SuccessEffects)
                    succesEffect.Apply(null, trapTarget, BattleSolver.SkillResult);
            }
        }
        else
        {
            if (RandomSolver.CheckSuccess(Mathf.Clamp(trapTarget.Character.Dodge, 0, 0.9f)))
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/props/traps/" + trap.StringId + "_dodge");
                BattleSolver.SkillResult.AddResultEntry(new SkillResultEntry(trapTarget, SkillResultType.Dodge));
            }
            else
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/props/traps/" + trap.StringId);
                BattleSolver.SkillResult.AddResultEntry(new SkillResultEntry(trapTarget, SkillResultType.Hit));
            }

            if (BattleSolver.SkillResult.Current.Type == SkillResultType.Hit)
            {
                if (CurrentRaid.Quest.Difficulty == 1)
                {
                    int damage = trapTarget.Character.TakeDamagePercent(Mathf.Abs(trap.HealthPenalty));
                    RaidEvents.ShowPopupMessage(trapTarget, PopupMessageType.Damage, damage.ToString());

                    foreach (var failEffect in trap.FailEffects)
                        failEffect.Apply(null, trapTarget, BattleSolver.SkillResult);
                }
                else
                {
                    TrapVariation variation;
                    if (trap.Variations.ContainsKey(CurrentRaid.Quest.Difficulty))
                        variation = trap.Variations[CurrentRaid.Quest.Difficulty];
                    else
                        variation = trap.Variations[5];

                    int damage = trapTarget.Character.TakeDamagePercent(Mathf.Abs(variation.HealthPenalty));
                    RaidEvents.ShowPopupMessage(trapTarget, PopupMessageType.Damage, damage.ToString());

                    foreach (var failEffect in variation.FailEffects)
                        failEffect.Apply(null, trapTarget, BattleSolver.SkillResult);
                }
            }
            else
            {
                RaidEvents.ShowPopupMessage(trapTarget, PopupMessageType.Dodge);
            }
        }
        trapTarget.OverlaySlot.UpdateOverlay();
        #endregion
        yield return new WaitForSeconds(1.4f);
        DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
        Formations.InvestigateTrapOutro(sector, isDisarmed);
        yield return new WaitForSeconds(0.10f);
        raidTrap.SetSortingOrder(3);
        yield return new WaitForSeconds(0.35f);
        Formations.ShowHeroOverlay();
        raidTrap.gameObject.SetActive(false);

        yield return StartCoroutine(ExecuteEffectEvents(false));

        if(Formations.Heroes.Party.Units.Contains(trapTarget))
            trapTarget.OverlaySlot.UpdateOverlay();

        yield return ProcessRaidFailure();

        #region Comment on Trigger
        if (isDisarmed == false && Formations.Heroes.Party.Units.Count > 1 &&
            Formations.Heroes.Party.Units.Contains(trapTarget))
        {
            foreach (var heroBarker in Formations.Heroes.Party.Units)
            {
                if (heroBarker != trapTarget && heroBarker.Character.Trait != null)
                {
                    if (RandomSolver.CheckSuccess(heroBarker.Character.
                        Trait.Reactions[ReactionType.CommentTrapTriggered].Chance))
                    {
                        var barkStressEffect = heroBarker.Character.
                            Trait.Reactions[ReactionType.CommentTrapTriggered].Effect;
                        yield return new WaitForSeconds(1f);
                        for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                            barkStressEffect.SubEffects[i].Apply(heroBarker, trapTarget, barkStressEffect);
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(ExecuteEffectEvents(false));
                        break;
                    }
                }
            }
        }
        #endregion

        QuestPanel.EnableRetreat();
        Formations.UnlockSelections();
        RaidPanel.BannerPanel.SetPeacefulState();
        Inventory.SetPeacefulState(false);
        EnablePartyMovement();
        CurrentEvent = null;
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
    }

    private IEnumerator ObstacleEvent(RaidHallSector sector, bool handActivation)
    {
        QuestPanel.DisableRetreat(false);
        DisablePartyMovement();
        DisableEnviroment();
        RaidPanel.SetDisabledState();
        Inventory.SetDeactivated();
        Formations.LockSelections();

        while (IsUnitEventInProgress)
            yield return null;

        RaidObstacle raidObstacle = sector.Prop as RaidObstacle;
        Obstacle obstacle = sector.Area.Prop as Obstacle;

        if (obstacle.AncestorTalk)
        {
            DarkestSoundManager.ExecuteNarration("ancestor_talk", NarrationPlace.Raid, "ancestor_talk_" + Raid.AncestorTalk);
            Raid.AncestorTalk++;

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            while (DarkestSoundManager.NarrationQueue.Count > 0 && DarkestSoundManager.CurrentNarration != null)
                yield return null;
        }

        sector.CompleteArea();
        yield return new WaitForEndOfFrame();

        float timeWasted = 0;
        if(handActivation)
        {
            DarkestSoundManager.ExecuteNarration("obstacle_clear_no_item", NarrationPlace.Raid);
            FMODUnity.RuntimeManager.PlayOneShot("event:/props/obstacles/" + obstacle.StringId + "_by_hand");
            if (obstacle.TorchlightPenalty < 0)
                TorchMeter.DecreaseTorch(Mathf.Abs(Mathf.RoundToInt(obstacle.TorchlightPenalty)));

            if(obstacle.HealthPenalty != 0)
            {
                foreach (var heroUnit in Formations.Heroes.Party.Units)
                {
                    int damage = heroUnit.Character.TakeDamagePercent(Mathf.Abs(obstacle.HealthPenalty));
                    RaidEvents.ShowPopupMessage(heroUnit, PopupMessageType.Damage, damage.ToString());
                    heroUnit.OverlaySlot.UpdateOverlay();
                }
                yield return new WaitForSeconds(0.6f);
                timeWasted += 0.6f;
            }

            BattleSolver.SkillResult.Reset();

            if(obstacle.FailEffects.Count > 0)
            {
                foreach (var heroUnit in Formations.Heroes.Party.Units)
                {
                    BattleSolver.SkillResult.AddResultEntry(new SkillResultEntry(heroUnit, SkillResultType.Hit));
                    foreach (var failEffect in obstacle.FailEffects)
                        failEffect.Apply(null, heroUnit, BattleSolver.SkillResult);
                }
            }
            
            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return ProcessRaidFailure();
        }
        else
        {
            DarkestSoundManager.ExecuteNarration("obstacle", NarrationPlace.Raid, Raid.Quest.Dungeon);
            FMODUnity.RuntimeManager.PlayOneShot("event:/props/obstacles/" + obstacle.StringId);
        }

        if (raidObstacle.SkeletonAnimation.state.GetCurrent(0) != null)
            yield return new WaitForSeconds(raidObstacle.SkeletonAnimation.state.GetCurrent(0).EndTime - timeWasted);

        QuestPanel.EnableRetreat();
        Inventory.SetPeacefulState(false);
        Formations.UnlockSelections();
        RaidPanel.SetPeacefulState();
        EnableEnviroment();
        EnableMovement();

        CurrentEvent = null;
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
    }

    private IEnumerator WaitCurioChoise(IRaidArea areaView, Curio curio)
    {
        float announcementTimer = -1;
        RaidEvents.LoadInteraction(curio, areaView);
        while (true)
        {
            if (announcementTimer > 0)
            {
                announcementTimer -= Time.deltaTime;
                if (announcementTimer <= 0)
                {
                    if (RaidEvents.Announcment.Animator.isInitialized)
                        RaidEvents.HideAnnouncment();
                    announcementTimer = -1;
                }
            }

            if (RaidEvents.InteractionEvent.ActionType == InteractionResultType.ItemInteraction)
            {
                if (!curio.ItemInteractions.Exists(action => action.ItemId == RaidEvents.InteractionEvent.SelectedItem.Id))
                {
                    RaidEvents.InteractionEvent.ResetInteraction(curio);
                    RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_curio_item_had_no_effect"));

                    announcementTimer = 1;
                }
            }

            if (RaidEvents.InteractionEvent.ActionType == InteractionResultType.Waiting)
                yield return null;
            else
                break;
        }
    }

    private IEnumerator ScoutingHallway(Hallway hallway, Direction direction, int tiles, HallSector fromSector = null)
    {
        scoutingCounter++;
        bool scoutNextRoom = false;

        if (direction == Direction.Bot || direction == Direction.Left)
        {
            int hallIndex = fromSector == null ? 0 : hallway.Halls.IndexOf(fromSector);
            int tilesScouted = Mathf.Min(hallway.Halls.Count - hallIndex, tiles);
            int lastHallIndex = hallIndex + tilesScouted - 1;
            scoutNextRoom = lastHallIndex == hallway.Halls.Count - 1;

            for (int i = hallIndex; i <= lastHallIndex; i++)
            {
                hallway.Halls[i].Scout();

                if (hallway.Halls[i].Type != AreaType.Door)
                    yield return new WaitForSeconds(0.4f);
            }

            tiles -= tilesScouted;
        }
        else if (direction == Direction.Top || direction == Direction.Right)
        {
            int hallIndex = fromSector == null ? hallway.HallCount - 1 : hallway.Halls.IndexOf(fromSector);
            int tilesScouted = Mathf.Min(hallIndex + 1, tiles);
            int lastHallIndex = hallIndex - tilesScouted + 1;
            scoutNextRoom = lastHallIndex == 0;

            for (int i = hallIndex; i >= lastHallIndex; i--)
            {
                hallway.Halls[i].Scout();

                if (hallway.Halls[i].Type != AreaType.Door)
                    yield return new WaitForSeconds(0.4f);
            }

            tiles -= tilesScouted;
        }

        DungeonRoom nextRoom = direction == Direction.Bot || direction == Direction.Left ? hallway.RoomB : hallway.RoomA;
        if (scoutNextRoom)
        {
            nextRoom.Scout();
            yield return new WaitForSeconds(0.5f);

            if (currentScoutedRooms.ContainsKey(nextRoom))
            {
                if (currentScoutedRooms[nextRoom] < tiles)
                    currentScoutedRooms[nextRoom] = tiles;
                else
                    tiles = 0;
            }
            else
                currentScoutedRooms.Add(nextRoom, tiles);

            if (tiles > 0)
            {
                for (int i = 0; i < nextRoom.Doors.Count; i++)
                {
                    if (nextRoom.Doors[i].TargetArea == hallway.Id)
                        continue;

                    StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[nextRoom.Doors[i].TargetArea], nextRoom.Doors[i].Direction, tiles));
                }
            }
        }

        scoutingCounter--;
    }

    private IEnumerator ProcessStarvation()
    {
        DarkestSoundManager.ExecuteNarration("hunger_starve", NarrationPlace.Raid);
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");

        HeroParty.Units.ForEach(unit => ProcessDamage(unit, unit.Character.TakeDamagePercent(0.2f)));
        HeroParty.Units.ForEach(unit => DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(unit));
        yield return StartCoroutine(ProcessHeroDeaths(0.6f, 0.8f, 0.3f));
        yield return StartCoroutine(ExecuteEffectEvents(false, 0.3f));
        yield return ProcessRaidFailure();
    }

    #endregion

    #region Restrictions

    public void DisablePartyMovement()
    {
        PartyController.MovementAllowed = false;
    }

    public void EnablePartyMovement()
    {
        PartyController.MovementAllowed = true;
    }

    private void DisableForwardPartyMovement()
    {
        PartyController.ForwardMovementAllowed = false;
    }

    private void EnableForwardPartyMovement()
    {
        PartyController.ForwardMovementAllowed = true;
    }

    private void EnableMovement()
    {
        PartyController.MovementAllowed = true;
        PartyController.ForwardMovementAllowed = true;
    }

    protected void DisableEnviroment()
    {
        RoomView.DisableInteraction();
        HallwayView.DisableInteraction();
    }

    protected void EnableEnviroment()
    {
        RoomView.EnableInteraction();
        HallwayView.EnableInteraction();
    }

    #endregion

    protected void CharacterWindowClosed()
    {
        if (BattleGround.BattleStatus != BattleStatus.Fighting)
            RaidPanel.UpdateSelection();
    }

    protected void CharacterWindowNextButtonClicked()
    {
        var formationUnit = HeroParty.Units.Find(unit => unit.Character == CharacterWindow.CurrentHero);
        if (formationUnit == null || HeroParty.Units.Count < 2)
            return;

        int unitIndex = HeroParty.Units.IndexOf(formationUnit) + 1;
        if (unitIndex > HeroParty.Units.Count - 1)
            HeroCharacterWindowOpened(HeroParty.Units[0].OverlaySlot);
        else
            HeroCharacterWindowOpened(HeroParty.Units[unitIndex].OverlaySlot);
    }

    protected void CharacterWindowPreviousButtonClicked()
    {
        var formationUnit = HeroParty.Units.Find(unit => unit.Character == CharacterWindow.CurrentHero);
        if (formationUnit == null || HeroParty.Units.Count < 2)
            return;

        int unitIndex = HeroParty.Units.IndexOf(formationUnit) - 1;
        if (unitIndex < 0)
            HeroCharacterWindowOpened(HeroParty.Units[HeroParty.Units.Count - 1].OverlaySlot);
        else
            HeroCharacterWindowOpened(HeroParty.Units[unitIndex].OverlaySlot);
    }

    private void UpdateExtraStackLimit()
    {
        ResetExtraStackLimit();

        for (int i = 0; i < CurrentRaid.RaidParty.HeroInfo.Count; i++)
            if (CurrentRaid.RaidParty.HeroInfo[i].Hero.HeroClass.ExtraStackLimit != null)
            {
                switch (CurrentRaid.RaidParty.HeroInfo[i].Hero.HeroClass.ExtraStackLimit)
                {
                    case "antiquarian_gold":
                        DarkestDungeonManager.Data.Items["gold"][""].ExtraStackLimit += 500;
                        break;
                }
            }
    }

    private void ResetExtraStackLimit()
    {
        DarkestDungeonManager.Data.Items["gold"][""].ExtraStackLimit = 0;
    }
}