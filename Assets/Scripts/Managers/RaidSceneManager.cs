using UnityEngine;
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
    public static RaidSceneManager Instanse { get; set; }

    #region Raid References
    public StartingMode startingMode;
    public List<string> startingItems;

    public RaidPartyCamera dungeonCamera;
    public RaidHallwayView hallwayView;
    public RaidRoomView roomView;
    public BattleGround battleGround;
    public RaidPartyController partyController;
    public RaidInterface raidInterface;
    public RaidEvents raidEvents;
    public PartyFormationManager formations;
    public RaidResultWindow resultWindow;
    public QuestCompletionWindow completionWindow;
    public TorchMeter torchMeter;
    public CampController campController;
    public CharacterWindow characterWindow;

    public static RaidInfo Raid
    {
        get
        {
            return Instanse.currentRaid;
        }
    }
    public static RaidPartyController PartyController
    {
        get
        {
            return Instanse.partyController;
        }
    }
    public static RaidPartyCamera DungeonCamera
    {
        get
        {
            return Instanse.dungeonCamera;
        }
    }
    public static RaidInterface RaidInterface
    {
        get
        {
            return Instanse.raidInterface;
        }
    }
    public static RaidEvents RaidEvents
    { 
        get
        {
            return Instanse.raidEvents;
        }
    }
    public static PartyFormationManager Formations
    {
        get
        {
            return Instanse.formations;
        }
    }
    public static RaidMapPanel MapPanel
    {
        get
        {
            return Instanse.raidInterface.RaidPanel.mapPanel;
        }
    }
    public static PartyInventory Inventory
    {
        get
        {
            return Instanse.raidInterface.RaidPanel.inventoryPanel.partyInventory;
        }
    }
    public static RaidPanel RaidPanel
    {
        get
        {
            return Instanse.raidInterface.RaidPanel;
        }
    }
    public static RaidHeroPanel HeroPanel
    {
        get
        {
            return Instanse.raidInterface.RaidPanel.heroPanel;
        }
    }
    public static RaidQuestPanel QuestPanel
    {
        get
        {
            return Instanse.raidInterface.QuestPanel;
        }
    }
    public static RaidRoomView RoomView
    {
        get
        {
            return Instanse.roomView;
        }
    }
    public static RaidHallwayView HallwayView
    {
        get
        {
            return Instanse.hallwayView;
        }
    }
    public static TorchMeter TorchMeter
    {
        get
        {
            return Instanse.torchMeter;
        }
    }
    public static CampController CampController
    {
        get
        {
            return Instanse.campController;
        }
    }
    public static CharacterWindow CharacterWindow
    {
        get
        {
            return Instanse.characterWindow;
        }
    }
    public static BattleGround BattleGround
    {
        get
        {
            return Instanse.battleGround;
        }
    }
    public static FormationParty HeroParty
    {
        get
        {
            return Instanse.formations.heroes.party;
        }
    }
    public static DungeonSceneState SceneState
    {
        get
        {
            return Instanse.sceneState;
        }
        set
        {
            Instanse.sceneState = value;
        }
    }
    #endregion

    #region Raid Info and Events
    public static bool HasAnyEvents
    {
        get
        {
            return IsUnitEventInProgress || Instanse.currentEvent != null;
        }
    }
    public static bool IsUnitEventInProgress
    {
        get
        {
            return Instanse.effectEvent != null || Instanse.itemUsageEvent != null || Instanse.roundAdvanceCounter != 0;
        }
    }
    public static bool AnyWindowOpened
    {
        get
        {
            return DarkestDungeonManager.Instanse.mainMenu.IsOpened || CharacterWindow.IsOpened;
        }
    }
    public static RaidRuleInfo Rules { get; set; }

    private RaidInfo currentRaid;
    private IEnumerator currentEvent;
    private IEnumerator itemUsageEvent;
    private IEnumerator effectEvent;
    private DungeonSceneState sceneState;
    private int scoutingCounter = 0;
    private int roundAdvanceCounter = 0;

    private List<FormationUnit> unitEventQueue = new List<FormationUnit>(8); 
    private List<FormationUnit> resolveCheckQueue = new List<FormationUnit>(4);
    private List<FormationUnit> heartAttackCheckQueue = new List<FormationUnit>(4);
    private List<FormationUnit> deathDoorEnterQueue = new List<FormationUnit>(4);
    private List<FormationUnit> bleedDeaths = new List<FormationUnit>(4);
    private List<FormationUnit> poisonDeaths = new List<FormationUnit>(4);
    private List<FormationUnit> tempList = new List<FormationUnit>(4);
    private List<CampEffect> campEffects = new List<CampEffect>(10);
    private List<CampEffect> chosenEffects = new List<CampEffect>(2);
    private Dictionary<Room, int> currentScoutedRooms = new Dictionary<Room, int>();
    
    private WaitForSeconds waitForZeroThree = new WaitForSeconds(0.3f);
    private WaitForSeconds waitForOneTwo = new WaitForSeconds(1.2f);
    #endregion

    private void CharacterWindow_onWindowClose()
    {
        if(BattleGround.BattleStatus != BattleStatus.Fighting)
            RaidPanel.UpdateSelection();
    }
    private void CharacterWindow_onNextButtonClick()
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
    private void CharacterWindow_onPreviousButtonClick()
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

    void Awake()
    {
        if (Instanse == null)
        {
            Instanse = this;

            if(DarkestDungeonManager.SaveData.InRaid)
            {
                currentRaid = new RaidInfo(DarkestDungeonManager.SaveData);
                DarkestDungeonManager.RaidManager.Quest = currentRaid.Quest;
                DarkestDungeonManager.RaidManager.RaidParty = currentRaid.RaidParty;
            }
            else
            {
                currentRaid = new RaidInfo();
                currentRaid.Quest = DarkestDungeonManager.Instanse.RaidingManager.Quest;
                if(currentRaid.Quest.IsPlotQuest && (currentRaid.Quest as PlotQuest).RaidMap != null)
                    currentRaid.Dungeon = SaveLoadManager.LoadDungeonMap((currentRaid.Quest as PlotQuest).RaidMap, currentRaid.Quest);
                else
                    currentRaid.Dungeon = DungeonGenerator.GenerateDungeon(currentRaid.Quest);
                currentRaid.RaidParty = DarkestDungeonManager.RaidManager.RaidParty;
            }

            UpdateExtraStackLimit();

            DarkestDungeonManager.Data.LoadDungeon(currentRaid.Quest.Dungeon, currentRaid.Quest.Id);
            Rules = new RaidRuleInfo(currentRaid.Quest.Dungeon, BattleGround, TorchMeter);
            RaidEvents.Initialize();
        }
        else
            Destroy(Instanse.gameObject);
    }
    void Start()
    {
        CharacterWindow.onWindowClose += CharacterWindow_onWindowClose;
        CharacterWindow.onNextButtonClick += CharacterWindow_onNextButtonClick;
        CharacterWindow.onPreviousButtonClick += CharacterWindow_onPreviousButtonClick;

        if (Instanse != this)
            return;
        
        if (DarkestDungeonManager.SaveData.InRaid)
        {
            RaidInterface.UpdateRaidScene();
            MapPanel.LoadDungeon(currentRaid.Dungeon);
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
            RaidPanel.heroPanel.equipmentPanel.SetDisabled();
            RaidPanel.bannerPanel.skillPanel.SetMode(SkillPanelMode.Combat);
            RaidPanel.bannerPanel.SetPeacefulState();

            QuestPanel.UpdateQuest(currentRaid.Quest, DarkestDungeonManager.SaveData.QuestCompleted);
            Formations.Initialize(DarkestDungeonManager.SaveData.HeroFormationData);
            DarkestSoundManager.StartDungeonSoundtrack(currentRaid.Dungeon.Name);

            if (currentRaid.CurrentLocation is Room)
                currentEvent = RoomLoadingEvent(currentRaid.CurrentLocation as Room, 
                    DarkestDungeonManager.SaveData.inBattle ? RoomTransitionType.CombatLoad : RoomTransitionType.PeacefulLoad);
            else
                currentEvent = HallwayLoadingEvent(currentRaid.CurrentLocation as HallSector,
                    DarkestDungeonManager.SaveData.inBattle ? HallTransitionType.CombatLoad : HallTransitionType.PeacefulLoad,
                    currentRaid.RaidParty.IsMovingLeft ? Direction.Left : Direction.Right);

            TorchMeter.Initialize(DarkestDungeonManager.SaveData.TorchAmount);

            if (DarkestDungeonManager.SaveData.ModifiedMinTorch != -1)
                TorchMeter.Modify(new TorchlightModifier(DarkestDungeonManager.SaveData.ModifiedMinTorch,
                    DarkestDungeonManager.SaveData.ModifiedMaxTorch));

            StartCoroutine(currentEvent);
        }
        else
        {
            RaidInterface.UpdateRaidScene();
            MapPanel.LoadDungeon(currentRaid.Dungeon);
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
            RaidPanel.bannerPanel.skillPanel.SetMode(SkillPanelMode.Combat);
            RaidPanel.bannerPanel.SetPeacefulState();
            QuestPanel.UpdateQuest(currentRaid.Quest);
            Formations.Initialize();
            DarkestSoundManager.StartDungeonSoundtrack(currentRaid.Dungeon.Name);

            if (startingMode == StartingMode.EntranceEncounter)
            {
                if (startingItems.Count > 0)
                {
                    currentRaid.Dungeon.StartingRoom.BattleEncounter = new BattleEncounter(startingItems);
                    currentRaid.Dungeon.StartingRoom.Type = AreaType.Battle;
                }
            }
            else if (startingMode == StartingMode.EntranceCurio)
            {
                if (startingItems.Count > 0)
                {
                    currentRaid.Dungeon.StartingRoom.Prop = DarkestDungeonManager.Data.Curios[startingItems[0]];
                    currentRaid.Dungeon.StartingRoom.Type = AreaType.Curio;
                }
            }

            TorchMeter.Initialize(100);

            currentEvent = RoomLoadingEvent(currentRaid.Dungeon.StartingRoom, RoomTransitionType.Entrance);
            StartCoroutine(currentEvent);
        }
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (DarkestDungeonManager.Instanse.mainMenu.gameObject.activeSelf)
                DarkestDungeonManager.Instanse.mainMenu.WindowClosed();
            else
                DarkestDungeonManager.Instanse.mainMenu.OpenMenu();
        }

        if (DarkestDungeonManager.GamePaused || AnyWindowOpened)
            return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentRaid.CurrentLocation != null && sceneState == DungeonSceneState.Hall && currentEvent == null &&
                HallwayView.CurrentSector.Area.Type == AreaType.Obstacle && !partyController.ForwardMovementAllowed)
            {
                if (!(HallwayView.CurrentSector.Prop as RaidObstacle).Removed)
                {
                    EncounterObstacle(HallwayView.CurrentSector);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            if (BattleGround.BattleStatus == BattleStatus.Peace && currentRaid.CurrentLocation != null)
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
                    if (RoomView.raidRoom.Area.Type == AreaType.BattleCurio
                        || RoomView.raidRoom.Area.Type == AreaType.BattleTresure)
                    {
                        if (!(RoomView.raidRoom.Prop as RaidCurio).Investigated)
                        {
                            ActivateCurio(RoomView.raidRoom);
                        }
                    }
                }
            }
        }

        if(sceneState == DungeonSceneState.Room && currentEvent == null)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                Door door = (roomView.raidRoom.Area as Room).Doors.Find(item => item.Direction == Direction.Top);
                if (door != null)
                {
                    currentEvent = HallwayLoadingEvent(currentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Top, roomView.raidRoom.Area as Room);
                    StartCoroutine(currentEvent);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Door door = (roomView.raidRoom.Area as Room).Doors.Find(item => item.Direction == Direction.Bot);
                if (door != null)
                {
                    currentEvent = HallwayLoadingEvent(currentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Bot, roomView.raidRoom.Area as Room);
                    StartCoroutine(currentEvent);
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Door door = (roomView.raidRoom.Area as Room).Doors.Find(item => item.Direction == Direction.Left);
                if (door != null)
                {
                    currentEvent = HallwayLoadingEvent(currentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Left, roomView.raidRoom.Area as Room);
                    StartCoroutine(currentEvent);
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Door door = (roomView.raidRoom.Area as Room).Doors.Find(item => item.Direction == Direction.Right);
                if (door != null)
                {
                    currentEvent = HallwayLoadingEvent(currentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                        HallTransitionType.FromRoom, Direction.Right, roomView.raidRoom.Area as Room);
                    StartCoroutine(currentEvent);
                }
            }
        }
    }
    void UpdateExtraStackLimit()
    {
        ResetExtraStackLimit();

        for (int i = 0; i < currentRaid.RaidParty.HeroInfo.Count; i++)
            if(currentRaid.RaidParty.HeroInfo[i].Hero.HeroClass.ExtraStackLimit != null)
            {
                switch(currentRaid.RaidParty.HeroInfo[i].Hero.HeroClass.ExtraStackLimit)
                {
                    case "antiquarian_gold":
                        DarkestDungeonManager.Data.Items["gold"][""].ExtraStackLimit += 500;
                        break;
                    default:
                        break;
                }
            }
    }
    void ResetExtraStackLimit()
    {
        DarkestDungeonManager.Data.Items["gold"][""].ExtraStackLimit = 0;
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

    public void OnSceneLeave()
    {
        DarkestDungeonManager.ScreenFader.Fade(1);
        DarkestSoundManager.StopDungeonSoundtrack();
        DarkestSoundManager.StopCampingSoundtrack();
        DarkestSoundManager.StopBattleSoundtrack();
        ResetExtraStackLimit();
    }
    IEnumerator ExecuteCampEffect(CampEffect currentEffect, FormationUnit target, bool skipNotification)
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
                float initialHeal = target.Character.Health.ModifiedValue * currentEffect.Amount;
                int heal = Mathf.CeilToInt(initialHeal * (1 + target.Character[AttributeType.HpHealReceivedPercent].ModifiedValue));
                if (heal < 1) heal = 1;
                target.Character.Health.IncreaseValue(heal);
                if (target.Character.AtDeathsDoor)
                    (target.Character as Hero).RevertDeathsDoor();
                RaidEvents.ShowPopupMessage(target, PopupMessageType.Heal, heal.ToString());
                target.OverlaySlot.UpdateOverlay();
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                break;
            case CampEffectType.Loot:
                RaidEvents.LoadSingleLoot(currentEffect.Subtype, (int)currentEffect.Amount);
                if (RaidEvents.loot.partyInventory.HasSomething())
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

                target.OverlaySlot.stressBar.UpdateStress(target.Character.Stress.ValueRatio);
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
            default:
                break;
        }
    }
    IEnumerator HallwayLoadingEvent(HallSector hallSector, HallTransitionType transitionType, Direction direction, Room fromRoom = null)
    {
        #region Set restrictions
        QuestPanel.DisableRetreat(false);
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        Formations.LockSelections();
        RaidPanel.heroPanel.equipmentPanel.SetDisabled();
        Formations.HideHeroOverlay();
        #endregion

        #region If from room : fade screen
        if (transitionType == HallTransitionType.FromRoom)
        {
            DarkestDungeonManager.Instanse.screenFader.Fade(1);
            yield return new WaitForSeconds(1f);
        }
        else
            DarkestDungeonManager.Instanse.screenFader.StartFaded();
        #endregion

        #region Switch hallway state
        SceneState = DungeonSceneState.Hall;

        RoomView.SetActive(false);
        HallwayView.SetActive(true);

        if (transitionType == HallTransitionType.FromRoom)
            Raid.ResetRoundSector(hallSector);
        hallwayView.LoadHallway(hallSector.Hallway, direction, fromRoom, transitionType == HallTransitionType.CombatLoad);

        yield return new WaitForEndOfFrame();
        PartyController.TranseferToPassage(HallwayView.hallwayPassage);
        PartyController.enabled = false;
        DisablePartyMovement();
        yield return new WaitForEndOfFrame();

        var targetRaidSector = HallwayView.raidHallway.HallSectors.Find(raidSector => raidSector.Area == hallSector);

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
                if (Raid.CurrentLocation is Room)
                {
                    MapPanel.SetCurrentIndicator(Raid.CurrentLocation as Room);
                }
                else
                {
                    MapPanel.SetCurrentIndicator(Raid.LastRoom as Room);
                    if ((Raid.CurrentLocation as HallSector).Type != AreaType.Door)
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
        for (int i = 0; i < Formations.monsters.overlay.OverlaySlots.Count; i++)
            Formations.monsters.overlay.OverlaySlots[i].RectTransform.pivot = new Vector2(0.5f, 0.1f);
        #endregion

        #region Load combat save
        if (transitionType == HallTransitionType.CombatLoad)
        {
            currentEvent = LoadEncounterEvent(targetRaidSector);
            StartCoroutine(currentEvent);
            yield break;
        }
        #endregion

        #region Show dungeon
        DarkestDungeonManager.Instanse.screenFader.Appear(2);
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

            if (HeroParty.Units.Count == 0)
            {
                yield return StartCoroutine(RaidResultsEvent());
                yield break;
            }
            EnablePartyMovement();
        }
        #endregion

        #region Remove restrictions
        QuestPanel.EnableRetreat();
        RaidPanel.SwitchBlocked = false;
        Formations.UnlockSelections();
        Formations.heroes.overlay.Show();
        Inventory.SetPeacefulState(false);
        RaidPanel.heroPanel.equipmentPanel.SetActive();
        currentEvent = null;
        #endregion
    }
    IEnumerator RoomLoadingEvent(Room room, RoomTransitionType transitionType, RaidHallSector fromRaidSector = null)
    {
        #region Set restrictions
        QuestPanel.DisableRetreat(false);
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        Formations.LockSelections();
        RaidPanel.heroPanel.equipmentPanel.SetDisabled();
        #endregion

        #region From hallway : open and close doors
        if(transitionType == RoomTransitionType.FromHallway)
        {
            if (fromRaidSector != null)
            {
                RaidDoor raidDoor = fromRaidSector.Prop as RaidDoor;

                while (unitEventQueue.Count > 0)
                    yield return null;

                fromRaidSector.LeaveSector();
                Formations.HideHeroOverlay();
                DarkestDungeonManager.Instanse.screenFader.Fade(1);

                dungeonCamera.TargetFOV = 30;
                dungeonCamera.SmoothTimeFOV = 1;
                raidDoor.Open();
                DisablePartyMovement();
                yield return new WaitForSeconds(0.3f);
                for (int i = Formations.heroes.party.Units.Count - 1; i >= 0; i--)
                {
                    Formations.SendUnitIntoDoor(fromRaidSector, i);
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForSeconds(0.7f);

                for (int i = Formations.heroes.party.Units.Count - 1; i >= 0; i--)
                {
                    Formations.GetUnitOutOfDoor(fromRaidSector, i);
                }
                dungeonCamera.SmoothTimeFOV = 0.1f;
                dungeonCamera.TargetFOV = 60;
                EnablePartyMovement();
            }
        }
        else
        {
            DarkestDungeonManager.Instanse.screenFader.StartFaded();
        }
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

        #region Show dungeon
        DarkestDungeonManager.Instanse.screenFader.Appear(2);
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

        #region Apply retreat effects
        if (transitionType == RoomTransitionType.Retreat)
        {
            for (int i = 0; i < HeroParty.Units.Count; i++)
                DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(HeroParty.Units[i]);

            for (int i = 0; i < HeroParty.Units.Count; i++)
                yield return StartCoroutine(ExecuteRandomDialog(HeroParty.Units[i], "str_bark_increasingstress"));

            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return new WaitForSeconds(0.3f);

            if (HeroParty.Units.Count == 0)
            {
                StartCoroutine(RaidResultsEvent());
                yield break;
            }
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
        RaidPanel.heroPanel.equipmentPanel.SetActive();
        currentEvent = null;
        #endregion
    }
    IEnumerator CampingEvent(Room room)
    {
        if (SceneState != DungeonSceneState.Room)
            yield break;

        #region Transition To Camping
        DarkestSoundManager.StartCampingSoundtrack();
        DisableEnviroment();
        RaidPanel.SwitchBlocked = true;
        Inventory.SetDeactivated();
        RaidPanel.SetDisabledState();
        RaidPanel.heroPanel.equipmentPanel.SetDisabled();
        MapPanel.HideAvailableRooms(RoomView.raidRoom.Area as Room);

        DarkestDungeonManager.Instanse.screenFader.Fade(1);
        TorchMeter.Hide();
        TorchMeter.IncreaseTorch(100);
        QuestPanel.DisableRetreat(true);
        Formations.LockSelections();
        Formations.ResetSelections();
        yield return new WaitForSeconds(1f);
        DungeonCamera.Zoom(50, 0);
        DungeonCamera.Transform.Rotate(3, 0, 0);
        DungeonCamera.SetCampingLight();
        RaidPanel.bannerPanel.skillPanel.SetMode(SkillPanelMode.Camping);
        RaidPanel.bannerPanel.skillPanel.UpdateSkillPanel();
        RaidPanel.bannerPanel.SetDisabledState();
        DisablePartyMovement();
        yield return new WaitForSeconds(0.1f);

        foreach (var hero in Formations.heroes.party.Units)
            hero.SetCampingAnimation(true);
        CampController.SwitchCamping(true);
        yield return new WaitForSeconds(0.1f);

        DarkestDungeonManager.Instanse.screenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        #endregion

        #if UNITY_EDITOR
        for(int barkLoops = 0; barkLoops < 2; barkLoops++)
        {
            for (int i = 0; i < HeroParty.Units.Count; i++)
            {
                HeroParty.Units[i].OverlaySlot.StartDialog("I'm gonna eat all the fucking chicken in this room!");
                while (HeroParty.Units[i].OverlaySlot.IsDoingDialog)
                    yield return null;
            }
        }
        #endif

        #region Meal Phase
        RaidEvents.LoadCampingMeal();
        yield return new WaitForEndOfFrame();
        RaidEvents.MealEvent.Show();

        while (true)
        {
            if (RaidEvents.MealEvent.MealResult == CampMealResultType.Wait)
                yield return null;
            else
                break;
        }

        Inventory.DiscardItemType("provision", RaidEvents.MealEvent.SelectedMealSlot.Amount);
        RaidEvents.MealEvent.Hide();
        yield return new WaitForSeconds(0.2f);
        RaidEvents.MealEvent.ScrollClosed();


        switch (RaidEvents.MealEvent.SelectedMealSlot.FoodRank)
        {
            case 0:
                bool someOneStarved = false;
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
                for (int i = 0; i < HeroParty.Units.Count; i++)
                {
                    int starveDamage = Mathf.RoundToInt(HeroParty.Units[i].Character.Health.ModifiedValue * 0.2f);
                    HeroParty.Units[i].Character.Health.DecreaseValue(starveDamage);
                    DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(HeroParty.Units[i]);

                    HeroParty.Units[i].OverlaySlot.UpdateOverlay();

                    #region Damage Activation
                    if (Mathf.RoundToInt(HeroParty.Units[i].Character.Health.CurrentValue) != 0)
                    {
                        RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Damage, starveDamage.ToString());
                    }
                    else
                    {
                        if (HeroParty.Units[i].Character.AtDeathsDoor)
                        {
                            if (PrepareDeath(HeroParty.Units[i]))
                            {
                                RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.DeathBlow);
                                someOneStarved = true;
                            }
                            else
                                RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.DeathsDoor);
                        }
                        else
                        {
                            PrepareDeath(HeroParty.Units[i]);
                            RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Damage, starveDamage.ToString());
                        }
                    }
                    #endregion
                }
                if (someOneStarved)
                {
                    yield return new WaitForSeconds(1.4f);
                    for (int i = HeroParty.Units.Count - 1; i >= 0; i--)
                        ExecuteDeath(HeroParty.Units[i]);
                    yield return new WaitForSeconds(0.3f);
                }
                else
                    yield return new WaitForSeconds(0.6f);

                yield return StartCoroutine(ExecuteEffectEvents(false));
                yield return new WaitForSeconds(0.3f);

                if (HeroParty.Units.Count == 0)
                {
                    StartCoroutine(RaidResultsEvent());
                    yield break;
                }
                break;
            case 1:
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");
                yield return new WaitForSeconds(0.6f);
                break;
            case 2:
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");

                for (int i = 0; i < HeroParty.Units.Count; i++)
                {
                    int mealHeal = Mathf.RoundToInt(HeroParty.Units[i].Character.Health.ModifiedValue * 0.1f);
                    HeroParty.Units[i].Character.Health.IncreaseValue(mealHeal);
                    if (HeroParty.Units[i].Character.AtDeathsDoor)
                        (HeroParty.Units[i].Character as Hero).RevertDeathsDoor();
                    HeroParty.Units[i].OverlaySlot.UpdateOverlay();
                    RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Heal, mealHeal.ToString());
                }

                yield return StartCoroutine(ExecuteEffectEvents(false));
                yield return new WaitForSeconds(0.6f);

                if (HeroParty.Units.Count == 0)
                {
                    StartCoroutine(RaidResultsEvent());
                    yield break;
                }
                break;
            case 3:
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");

                for (int i = 0; i < HeroParty.Units.Count; i++)
                {
                    int mealHeal = Mathf.RoundToInt(HeroParty.Units[i].Character.Health.ModifiedValue * 0.25f);
                    HeroParty.Units[i].Character.Health.IncreaseValue(mealHeal);
                    if (HeroParty.Units[i].Character.AtDeathsDoor)
                        (HeroParty.Units[i].Character as Hero).RevertDeathsDoor();
                    DarkestDungeonManager.Data.Effects["HealSelfStress 1"].ApplyIndependent(HeroParty.Units[i], HeroParty.Units[i]);
                    HeroParty.Units[i].OverlaySlot.UpdateOverlay();
                    RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Heal, mealHeal.ToString());
                }
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(ExecuteEffectEvents(false));
                yield return new WaitForSeconds(0.6f);

                if (HeroParty.Units.Count == 0)
                {
                    StartCoroutine(RaidResultsEvent());
                    yield break;
                }
                break;
            default:
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

        RaidPanel.bannerPanel.SetCombatReady();
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
                CampingSkill skill = RaidPanel.bannerPanel.skillPanel.SelectedSkill as CampingSkill;

                RaidPanel.SelectedUnit.CombatInfo.SkillsUsedThisTurn.Add(skill.Id);
                RaidEvents.CampEvent.SpendTime(skill.TimeCost);
                Formations.ResetSelections();
                RaidPanel.SkillPanel.SetUsable();

                campEffects.Clear();
                campEffects.AddRange(skill.Effects);
                FMODUnity.RuntimeManager.PlayOneShot("event:/camp/skill/" + skill.Id, DungeonCamera.Transform.position);
                yield return new WaitForSeconds(0.5f);

                #region Damage Effect
                var damageEffect = campEffects.Find(effect => effect.Type == CampEffectType.HealthDamageMaxHealthPercent);
                bool someOneDied = false;

                while (damageEffect != null)
                {
                    campEffects.Remove(damageEffect);
                    BattleSolver.GetTargetsForCampEffect(RaidPanel.SelectedUnit, 
                        RaidEvents.CampEvent.SelectedTarget, damageEffect, tempList);

                    #region Damage Activation
                    for (int i = 0; i < tempList.Count; i++)
                    {
                        int effectDamage = Mathf.RoundToInt(tempList[i].Character.Health.ModifiedValue * 0.2f);
                        tempList[i].Character.Health.DecreaseValue(effectDamage);
                        tempList[i].OverlaySlot.UpdateOverlay();

                        if (Mathf.RoundToInt(tempList[i].Character.Health.CurrentValue) != 0)
                        {
                            RaidEvents.ShowPopupMessage(tempList[i], PopupMessageType.Damage, effectDamage.ToString());
                        }
                        else
                        {
                            if (tempList[i].Character.AtDeathsDoor)
                            {
                                if (PrepareDeath(tempList[i]))
                                {
                                    someOneDied = true;
                                    RaidEvents.ShowPopupMessage(tempList[i], PopupMessageType.DeathBlow);
                                }
                                else
                                    RaidEvents.ShowPopupMessage(tempList[i], PopupMessageType.DeathsDoor);
                            }
                            else
                            {
                                PrepareDeath(tempList[i]);
                                RaidEvents.ShowPopupMessage(tempList[i], PopupMessageType.Damage, effectDamage.ToString());
                            }
                        }
                    }
                    #endregion

                    #region Death Execution
                    if (someOneDied)
                    {
                        yield return new WaitForSeconds(1.4f);
                        for (int i = HeroParty.Units.Count - 1; i >= 0; i--)
                            ExecuteDeath(HeroParty.Units[i]);
                        yield return new WaitForSeconds(0.3f);
                        yield return StartCoroutine(ExecuteEffectEvents(false));
                        if (HeroParty.Units.Count == 0)
                        {
                            StartCoroutine(RaidResultsEvent());
                            yield break;
                        }
                    }
                    else
                        yield return new WaitForSeconds(0.6f);
                    #endregion

                    damageEffect = campEffects.Find(effect => effect.Type == CampEffectType.HealthDamageMaxHealthPercent);
                }
                #endregion

                while (campEffects.Count > 0)
                {
                    #region Choose Effect and Targets
                    chosenEffects.Clear();
                    chosenEffects.Add(campEffects[0]);
                    campEffects.RemoveAt(0);

                    if (chosenEffects[0].Chance != 1)
                    {
                        chosenEffects.AddRange(campEffects.FindAll(effect => effect.Code == chosenEffects[0].Code));
                        campEffects.RemoveAll(effect => effect.Code == chosenEffects[0].Code);
                    }
                    BattleSolver.GetTargetsForCampEffect(RaidPanel.SelectedUnit,
                        RaidEvents.CampEvent.SelectedTarget, chosenEffects[0], tempList);
                    #endregion

                    #region Effect Execution
                    CampEffect currentEffect = null;
                    bool skipNotification = false;

                    switch (chosenEffects[0].Type)
                    {
                        case CampEffectType.Buff:
                        case CampEffectType.HealthHealMaxHealthPercent:
                        case CampEffectType.RemoveDisease:
                            #region Standard Wait Effects
                            while (true)
                            {
                                for (int j = 0; j < tempList.Count; j++)
                                {
                                    currentEffect = RandomSolver.ChooseBySingleRandom(chosenEffects);

                                    if (chosenEffects.Count == 1 && chosenEffects[0].Chance != 1)
                                        if(!RandomSolver.CheckSuccess(chosenEffects[0].Chance))
                                            continue;

                                    if (!BattleSolver.IsRequirementFulfilled(tempList[j], currentEffect.Requirement))
                                        continue;

                                    yield return ExecuteCampEffect(currentEffect, tempList[j], skipNotification);
                                }

                                currentEffect = campEffects.Find(effect => effect.Type == currentEffect.Type
                                    && effect.Selection == currentEffect.Selection && effect.Chance == 1);

                                if (currentEffect != null)
                                {
                                    campEffects.Remove(currentEffect);
                                    chosenEffects.Clear();
                                    chosenEffects.Add(currentEffect);
                                    skipNotification = true;
                                    continue;
                                }
                                else
                                {
                                    if (campEffects.Count == 0)
                                        yield return new WaitForSeconds(0.6f);
                                    else switch (chosenEffects[0].Selection)
                                        {
                                            case CampTargetType.Individual:
                                                if (campEffects[0].Selection != CampTargetType.Self)
                                                    yield return new WaitForSeconds(0.6f);
                                                break;
                                            case CampTargetType.PartyOther:
                                                if (campEffects[0].Selection != CampTargetType.Self)
                                                    yield return new WaitForSeconds(0.6f);
                                                break;
                                            case CampTargetType.Self:
                                                if (campEffects[0].Selection == CampTargetType.Self)
                                                    yield return new WaitForSeconds(0.6f);
                                                break;
                                            default:
                                                break;
                                        }
                                    break;
                                }
                            }
                            break;
                        #endregion
                        case CampEffectType.Loot:
                        case CampEffectType.ReduceAmbushChance:
                        case CampEffectType.ReduceTorch:
                        case CampEffectType.RemoveDeathRecovery:
                            #region Instant Effects
                            for (int j = 0; j < tempList.Count; j++)
                            {
                                currentEffect = RandomSolver.ChooseBySingleRandom(chosenEffects);

                                if (chosenEffects.Count == 1 && chosenEffects[0].Chance != 1)
                                    if(!RandomSolver.CheckSuccess(chosenEffects[0].Chance))
                                        continue;

                                if (!BattleSolver.IsRequirementFulfilled(tempList[j], currentEffect.Requirement))
                                    continue;

                                yield return ExecuteCampEffect(currentEffect, tempList[j], skipNotification);
                            }
                            break;
                            #endregion
                        case CampEffectType.RemoveBleed:
                        case CampEffectType.RemovePoison:
                            #region Combined Effects
                            while (true)
                            {
                                for (int j = 0; j < tempList.Count; j++)
                                {
                                    currentEffect = RandomSolver.ChooseBySingleRandom(chosenEffects);

                                    if (chosenEffects.Count == 1 && chosenEffects[0].Chance != 1)
                                        if(!RandomSolver.CheckSuccess(chosenEffects[0].Chance))
                                            continue;

                                    if (!BattleSolver.IsRequirementFulfilled(tempList[j], currentEffect.Requirement))
                                        continue;

                                    yield return ExecuteCampEffect(currentEffect, tempList[j], skipNotification);
                                }

                                currentEffect = campEffects.Find(effect => (effect.Type == CampEffectType.RemoveBleed 
                                    || effect.Type == CampEffectType.RemovePoison)
                                    && effect.Selection == currentEffect.Selection && effect.Chance == 1);

                                if (currentEffect != null)
                                {
                                    campEffects.Remove(currentEffect);
                                    chosenEffects.Clear();
                                    chosenEffects.Add(currentEffect);
                                    continue;
                                }
                                else
                                {
                                    if (campEffects.Count == 0)
                                        yield return new WaitForSeconds(0.6f);
                                    else switch (chosenEffects[0].Selection)
                                        {
                                            case CampTargetType.Individual:
                                                if (campEffects[0].Selection != CampTargetType.Self)
                                                    yield return new WaitForSeconds(0.6f);
                                                break;
                                            case CampTargetType.PartyOther:
                                                if (campEffects[0].Selection != CampTargetType.Self)
                                                    yield return new WaitForSeconds(0.6f);
                                                break;
                                            case CampTargetType.Self:
                                                if (campEffects[0].Selection == CampTargetType.Self)
                                                    yield return new WaitForSeconds(0.6f);
                                                break;
                                            default:
                                                break;
                                        }
                                    break;
                                }
                            }
                            break;
                        #endregion
                        case CampEffectType.StressDamageAmount:
                        case CampEffectType.StressHealAmount:
                            #region Effects with Stress
                            while (true)
                            {
                                for (int j = 0; j < tempList.Count; j++)
                                {
                                    currentEffect = RandomSolver.ChooseBySingleRandom(chosenEffects);

                                    if (chosenEffects.Count == 1 && chosenEffects[0].Chance != 1)
                                        if(!RandomSolver.CheckSuccess(chosenEffects[0].Chance))
                                            continue;

                                    if (!BattleSolver.IsRequirementFulfilled(tempList[j], currentEffect.Requirement))
                                        continue;

                                    yield return ExecuteCampEffect(currentEffect, tempList[j], skipNotification);
                                }

                                currentEffect = campEffects.Find(effect => effect.Type == currentEffect.Type
                                    && effect.Selection == currentEffect.Selection && effect.Chance == 1);

                                if (currentEffect != null)
                                {
                                    campEffects.Remove(currentEffect);
                                    chosenEffects.Clear();
                                    chosenEffects.Add(currentEffect);
                                    continue;
                                }
                                else
                                {
                                    if (campEffects.Count == 0)
                                        yield return new WaitForSeconds(1.2f);
                                    else switch (chosenEffects[0].Selection)
                                        {
                                            case CampTargetType.Individual:
                                                if (campEffects[0].Selection != CampTargetType.Self)
                                                    yield return new WaitForSeconds(1.2f);
                                                break;
                                            case CampTargetType.PartyOther:
                                                if (campEffects[0].Selection != CampTargetType.Self)
                                                    yield return new WaitForSeconds(1.2f);
                                                break;
                                            case CampTargetType.Self:
                                                if (campEffects[0].Selection == CampTargetType.Self)
                                                    yield return new WaitForSeconds(1.2f);
                                                break;
                                            default:
                                                break;
                                        }
                                    break;
                                }
                            }
                            yield return StartCoroutine(ExecuteEffectEvents(false));
                            if (HeroParty.Units.Count == 0)
                            {
                                StartCoroutine(RaidResultsEvent());
                                yield break;
                            }
                            break;
                        #endregion
                        default:
                            break;
                    }
                    #endregion
                }
                RaidEvents.CampEvent.Show();

                #region Post Skill Usage
                Formations.HeroOverlay.UpdateOverlay();
                Formations.UnlockSelections();
                RaidEvents.CampEvent.SkillExecuted();
                RaidPanel.SelectedUnit.SetPerformerStatus();
                #endregion
            }
        }
        #endregion

        #region Transition From Camping
        DarkestSoundManager.StopCampingSoundtrack();
        Formations.ResetSelections();
        DarkestDungeonManager.Instanse.screenFader.Fade(1);
        yield return new WaitForSeconds(1f);
        DungeonCamera.Zoom(60, 0);
        DungeonCamera.Transform.Rotate(-3, 0, 0);
        DungeonCamera.SetRaidingLight(TorchMeter.CurrentRange.RangeType);
        RaidPanel.bannerPanel.skillPanel.SetMode(SkillPanelMode.Combat);
        RaidPanel.bannerPanel.skillPanel.UpdateSkillPanel();
        RaidPanel.bannerPanel.SetDisabledState();
        TorchMeter.Show();
        foreach (var hero in Formations.heroes.party.Units)
        {
            hero.SetCampingAnimation(false);
            hero.DeleteTarget(0);
        }
        CampController.SwitchCamping(false);

        if(RandomSolver.CheckSuccess(0.5f - Raid.NightAmbushReduced))
        {
            var battleEncounter = RandomSolver.ChooseByRandom(Raid.Dungeon.DungeonMash.RoomEncounters);
            if(battleEncounter != null)
            {
                RaidPanel.SwitchBlocked = false;
                Raid.NightAmbushReduced = 0;
                room.BattleEncounter = new BattleEncounter(battleEncounter.MonsterSet);
                currentEvent = EncounterEvent(RoomView.raidRoom, true);
                int torchBefore = TorchMeter.TorchAmount;
                TorchMeter.DecreaseTorch(100);
                DarkestDungeonManager.Instanse.screenFader.Appear(2);
                yield return new WaitForSeconds(0.3f);
                yield return StartCoroutine(currentEvent);
                if (TorchMeter.TorchAmount < torchBefore)
                    TorchMeter.IncreaseTorch(torchBefore - TorchMeter.TorchAmount);
                yield break;
            }
        }

        DarkestDungeonManager.Instanse.screenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        QuestPanel.EnableRetreat();
        EnablePartyMovement();
        RaidPanel.SelectedUnit.SetPerformerStatus();

        MapPanel.ShowAvailableRooms(RoomView.raidRoom.Area as Room);
        Formations.UnlockSelections();
        EnableEnviroment();
        RaidPanel.SwitchBlocked = false;
        Inventory.SetPeacefulState(false);
        RaidPanel.heroPanel.equipmentPanel.SetActive();
        RaidPanel.SetPeacefulState();
        currentEvent = null;
        Raid.NightAmbushReduced = 0;
        yield break;
        #endregion
    }
    IEnumerator RaidResultsEvent()
    {
        RaidInterface.CanvasGroup.blocksRaycasts = false;

        if(Raid.QuestCompleted == true || Raid.CheckQuestGoals())
            DarkestDungeonManager.RaidManager.Status = RaidStatus.Success;
        else if (Formations.heroes.party.Units.Count > 0)
        {
            DarkestDungeonManager.RaidManager.Status = RaidStatus.Abandon;
        }
        else
            DarkestDungeonManager.RaidManager.Status = RaidStatus.Defeat;

        ToolTipManager.Instanse.Hide();
        Formations.HideHeroOverlay();
        RoomView.DisableInteraction();
        HallwayView.DisableInteraction();
        DarkestDungeonManager.Instanse.screenFader.Fade(1);
        yield return new WaitForSeconds(1f);
        roomView.gameObject.SetActive(false);
        hallwayView.gameObject.SetActive(false);
        resultWindow.gameObject.SetActive(true);
        resultWindow.ProceedToItems();
        resultWindow.DisableInteraction();
        DarkestDungeonManager.Instanse.screenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        resultWindow.EnableInteraction();
    }
    IEnumerator RaidResultsHeroTransition()
    {
        DarkestDungeonManager.ScreenFader.Fade(1);
        yield return new WaitForSeconds(1f);
        resultWindow.ProceedToHeroes();
        DarkestDungeonManager.ScreenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        resultWindow.EnableInteraction();
    }
    IEnumerator RaidResultsTownTransition()
    {
        DarkestDungeonManager.Instanse.screenFader.Fade(1);
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
        currentEvent = EncounterEvent(areaView);
        StartCoroutine(currentEvent);
    }

    public void ActivateCurio(IRaidArea area)
    {
        if (currentEvent != null)
            return;

        currentEvent = CurioEvent(area);
        StartCoroutine(currentEvent);
    }
    public void ActivateCurio(IRaidArea area, Quirk quirk)
    {
        if (currentEvent != null)
            return;

        currentEvent = CurioEvent(area, quirk);
        StartCoroutine(currentEvent);
    }
    public void ActivateCurio(IRaidArea area, Trait trait)
    {
        if (currentEvent != null)
            return;
        currentEvent = CurioEvent(area, null, trait);
        StartCoroutine(currentEvent);
    }

    public void ActivateHunger(RaidHallSector sector)
    {
        currentEvent = HungerEvent();
        StartCoroutine(currentEvent);
    }
    public void ActivateTrap(RaidHallSector sector, bool handActivation)
    {
        currentEvent = TrapEvent(sector, handActivation);
        StartCoroutine(currentEvent);
    }
    public void ActivateObstacle(RaidHallSector sector, bool handActivation)
    {
        currentEvent = ObstacleEvent(sector, handActivation);
        StartCoroutine(currentEvent);
    }
    public void ActivateDoor(RaidHallSector sector)
    {
        if (currentEvent != null)
            return;

        var door = sector.Area.Prop as Door;
        currentEvent = RoomLoadingEvent(currentRaid.Dungeon.Rooms[door.TargetArea], RoomTransitionType.FromHallway, sector);
        StartCoroutine(currentEvent);
    }
    #endregion 

    #region Player Actions
    public void NextRaidResultButtonClicked()
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
    public void AbandonButtonClicked()
    {
        if (BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            if (BattleGround.Round.HeroAction == HeroTurnAction.Waiting && BattleGround.Round.TurnStatus == TurnStatus.Progress)
            {
                BattleGround.Round.HeroAction = HeroTurnAction.Retreat;
            }
        }
        else if (currentEvent == null)
        {
            StartCoroutine(RaidResultsEvent());
        }
    }
    public void ReturnToEstateClicked()
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
                        Formations.heroes.party.Units[j].SetFriendlyTargetStatus(true);
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
        Formations.heroes.overlay.ResetSelectionsExcept(RaidPanel.SelectedUnit);
    }
    public void HeroSkillSelected(BattleSkillSlot skillSlot)
    {
        if (skillSlot.Skill.TargetRanks.IsSelfFormation || skillSlot.Skill.TargetRanks.IsSelfTarget)
        {
            Formations.monsters.overlay.ResetSelections();

            if(skillSlot.Skill.TargetRanks.IsSelfTarget)
            {
                BattleGround.Round.SelectedUnit.SetFriendlyPerformerStatus(true);
                Formations.heroes.overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            }
            else
            {
                for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
                {
                    if(Formations.heroes.party.Units[i] == BattleGround.Round.SelectedUnit)
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
                            BlockedHealUnitIds.Contains(Formations.heroes.party.Units[i].CombatInfo.CombatId))
                                Formations.heroes.party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.IsBuffSkill && BattleGround.Round.SelectedUnit.CombatInfo.
                            BlockedBuffUnitIds.Contains(Formations.heroes.party.Units[i].CombatInfo.CombatId))
                                Formations.heroes.party.Units[i].SetDeactivatedStatus();
                        else if (skillSlot.Skill.TargetRanks.IsTargetableUnit(Formations.heroes.party.Units[i]))
                            Formations.heroes.party.Units[i].SetFriendlyTargetStatus(true);
                        else
                            Formations.heroes.party.Units[i].SetDeactivatedStatus();
                    }
                }
            }
        }
        else
        {
            Formations.heroes.overlay.ResetSelectionsExcept(BattleGround.Round.SelectedUnit);
            tempList.Clear();

            for (int i = 0; i < Formations.monsters.party.Units.Count; i++)
            {
                if (skillSlot.Skill.TargetRanks.IsTargetableUnit(Formations.monsters.party.Units[i]))
                    tempList.Add(Formations.monsters.party.Units[i]);
                else
                    Formations.monsters.party.Units[i].SetDeactivatedStatus();
            }

            if (skillSlot.Skill.TargetRanks.IsMultitarget && tempList.Count > 0)
            {
                for (int i = 0; i < tempList.Count; i++)
                    tempList[i].SetEnemyTargetStatus(true, i != tempList.Count - 1);
            }
            else
                foreach (var target in tempList)
                    target.SetEnemyTargetStatus(true, false);

            tempList.Clear();
        }
    }
    public void HeroMoveSelected(MoveSkillSlot skillSlot)
    {
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
                    if(BattleGround.Round.SelectedUnit.CombatInfo.BlockedMoveUnitIds.
                        Contains(Formations.heroes.party.Units[i].CombatInfo.CombatId))
                    {
                        Formations.heroes.party.Units[i].SetDeactivatedStatus();
                    }
                    else if(distance < 0)
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
    public void HeroMoveDeselected(MoveSkillSlot skillSlot)
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

        itemUsageEvent = ExecuteHeroItemUsage(RaidSceneManager.RaidPanel.SelectedUnit, slot);
        StartCoroutine(itemUsageEvent);
    }
    public void HeroSkillTargetSelected(FormationOverlaySlot overlaySlot)
    {
        var primaryTarget = overlaySlot.TargetUnit;

        if(BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            if(RaidPanel.bannerPanel.skillPanel.SelectedSkill is MoveSkill)
            {
                BattleGround.Round.HeroActionSelected(HeroTurnAction.Move, primaryTarget);
            }
            else if (RaidPanel.bannerPanel.skillPanel.SelectedSkill is CombatSkill)
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
    public void HeroPassButtonClicked()
    {
        BattleGround.Round.HeroActionSelected(HeroTurnAction.Pass, BattleGround.Round.SelectedUnit);
    }
    public void HeroCharacterWindowOpened(FormationOverlaySlot overlaySlot)
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
    public void TargetRoomSelected(Room room)
    {
        if (sceneState == DungeonSceneState.Room && currentEvent == null)
        {
            Door door = null;
            var currentRoom = roomView.raidRoom.Area as Room;
            for(int i = 0; i < currentRoom.Doors.Count; i++)
                for(int j = 0; j < room.Doors.Count; j++)
                    if(currentRoom.Doors[i].TargetArea == room.Doors[j].TargetArea)
                        door = currentRoom.Doors[i];

            if (door != null)
            {
                currentEvent = HallwayLoadingEvent(currentRaid.Dungeon.Hallways[door.TargetArea].Halls[0],
                    HallTransitionType.FromRoom, door.Direction, roomView.raidRoom.Area as Room);
                StartCoroutine(currentEvent);
            }
        }
    }
    #endregion

    #region Battle and Hero Events
    IEnumerator BattleRound(bool fromBattleSave = false)
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
                        dungeonCamera.Zoom(60, 0.1f);
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
            if (fromBattleSave == false)
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
                                    dungeonCamera.Zoom(60, 0.1f);
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
                    if(currentDesire.CombatSkillOverride == "")
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
    IEnumerator HeroTurn(FormationUnit actionUnit, bool fromBattleSave = false)
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
                        if (PrepareDeath(actionUnit))
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
                        if (PrepareDeath(actionUnit))
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
                        if (PrepareDeath(actionUnit))
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
                        if (PrepareDeath(actionUnit))
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
                        if(targetInfo.Targets.Count == 0)
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
                        yield return new WaitForSeconds(0.6f);
                        BattleGround.Round.HeroAction = HeroTurnAction.Pass;
                    }
                    else
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/retreat");

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

                        if(SceneState == DungeonSceneState.Hall)
                        {
                            HallwayView.CurrentSector.SetInside(false);
                            currentEvent = RoomLoadingEvent(HallwayView.StartingRoom, 
                                RoomTransitionType.Retreat, HallwayView.CurrentSector);
                        }
                        else if (SceneState == DungeonSceneState.Room)
                        {
                            var currentRoom = Raid.CurrentLocation as Room;
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
    IEnumerator MonsterTurn(FormationUnit actionUnit)
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
                    if (PrepareDeath(actionUnit))
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
                    if(PrepareDeath(actionUnit))
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
                    if (PrepareDeath(actionUnit))
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
                    if (PrepareDeath(actionUnit))
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
        if(BattleGround.Round.HeroAction != HeroTurnAction.Retreat)
            BattleGround.Round.PostMonsterTurn();
    }
    IEnumerator MonsterOverriddenTurn(FormationUnit actionUnit, string combatSkillOverride)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/enemy_turn");
        Formations.ResetSelections();
        yield return new WaitForEndOfFrame();
        Formations.ShowUnitOverlay();
        yield return new WaitForEndOfFrame();
        actionUnit.SetPerformerStatus();

        yield return StartCoroutine(ExecuteMonsterOverridenSkill(actionUnit, combatSkillOverride));
    }

    void ExecuteSkillInstants(FormationUnit performer, SkillTargetInfo targetInfo, SkillResult skillResult)
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
                    case SkillResultType.Utility:
                    default:
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

                if (skillEntry.Target.Character.IsMonster && Mathf.RoundToInt(skillEntry.Target.Character.Health.CurrentValue) == 0)
                    PrepareDeath(skillEntry.Target);
                else if (skillEntry.Target.Character.IsMonster == false && skillEntry.Target.Character.AtDeathsDoor == false)
                    if (Mathf.RoundToInt(skillEntry.Target.Character.Health.CurrentValue) == 0)
                        PrepareDeath(skillEntry.Target);
            }
        }

        for(int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
            if (BattleGround.MonsterParty.Units[i].CombatInfo.MarkedForDeath)
                PrepareDeath(BattleGround.MonsterParty.Units[i]);

        performer.OverlaySlot.UpdateOverlay();
    }
    void ExecuteSlidingSetup(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        if(performer.Team == Team.Monsters)
        {
            if (targetInfo.Type == SkillTargetType.Party)
                Formations.partyBuffPositions.SetSpacing(120, 1f);
            else if (targetInfo.Type == SkillTargetType.Enemy)
            {
                if (targetInfo.Skill.Type == "melee")
                    Formations.monstersAttackMeleePosition.SetSliding(-150, 1f);
                else
                    Formations.monstersAttackRangePosition.SetSliding(120, 1f);

                Formations.heroesDefencePositions.SetSliding(-120, 1f);
            }
            else if (targetInfo.Type == SkillTargetType.Self)
                Formations.partyBuffPositions.SetSpacing(120, 1f);
        }
        else
        {
            if (targetInfo.Type == SkillTargetType.Party)
                Formations.partyBuffPositions.SetSpacing(120, 1f);
            else if (targetInfo.Type == SkillTargetType.Enemy)
            {
                if (targetInfo.Skill.Type == "melee")
                    Formations.heroesAttackMeleePosition.SetSliding(150, 1f);
                else
                    Formations.heroesAttackRangePosition.SetSliding(-120, 1f);

                Formations.monstersDefencePositions.SetSliding(120, 1f);
            }
            else if (targetInfo.Type == SkillTargetType.Self)
                Formations.partyBuffPositions.SetSpacing(120, 1f);
        }
    }
    void ExecuteSkillAnimationIntro(FormationUnit performer, SkillTargetInfo targetInfo)
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
                    Formations.partyBuffPositions.SetUnitTargets(targetInfo.Targets.OrderByDescending(unit =>
                    unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                else
                    Formations.partyBuffPositions.SetUnitTargets(targetInfo.Targets.OrderBy(unit => 
                    unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
            }
            else
            {
                var positionTargets = new List<FormationUnit>(targetInfo.Targets);
                positionTargets.Insert(0, performer);
                if (performer.Team == Team.Heroes)
                    Formations.partyBuffPositions.SetUnitTargets(positionTargets.OrderByDescending(unit =>
                    unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                else
                    Formations.partyBuffPositions.SetUnitTargets(positionTargets.OrderBy(unit =>
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
                    Formations.monstersAttackMeleePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);
                else
                    Formations.monstersAttackRangePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);

                Formations.heroesDefencePositions.SetUnitTargets(targetInfo.Targets.OrderByDescending(unit =>
                    unit.Rank).ToList(), 0.01f, targetInfo.SkillArtInfo.TargetAreaOffset);
            }
            else
            {
                if (targetInfo.Skill.Type == "melee")
                    Formations.heroesAttackMeleePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);
                else
                    Formations.heroesAttackRangePosition.SetUnitTargets(performer, 0.01f, targetInfo.SkillArtInfo.AreaOffset);

                Formations.monstersDefencePositions.SetUnitTargets(targetInfo.Targets.OrderBy(unit =>
                    unit.Rank).ToList(), 0.01f, targetInfo.SkillArtInfo.TargetAreaOffset);
            }
            
        }
        else if (targetInfo.Type == SkillTargetType.Self)
        {
            Formations.partyBuffPositions.SetUnitTargets(performer, 0.01f, Vector2.zero);
        }
    }
    void ExecuteGuardRedirection(FormationUnit performer, SkillTargetInfo targetInfo)
    {
        if (targetInfo.Type == SkillTargetType.Enemy)
            for (int i = targetInfo.Targets.Count - 1; i >= 0; i--)
            {
                if (targetInfo.Targets[i].Character.GetStatusEffect(StatusType.Guarded).IsApplied)
                {
                    var guardedStatus = targetInfo.Targets[i].Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;
                    if (!targetInfo.Targets.Contains(guardedStatus.Guard))
                        targetInfo.Targets[i] = guardedStatus.Guard;
                }
            }
    }
    void SetBrainDecisionMarkings(FormationUnit performer, MonsterBrainDecision brainDecision)
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
    SkillResult ExecuteSkillBase(FormationUnit performer, SkillTargetInfo targetInfo)
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

        string playSkillEvent = null;
        string playSkillMissEvent = null;

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

        return skillResult;
    }
    
    IEnumerator ExecuteMonsterSkill(FormationUnit actionUnit)
    {
        RaidEvents.MonsterTooltip.IsDisabled = true;
        RaidEvents.MonsterTooltip.Hide();
        if(actionUnit.Character.IsMonster == false)
        {
            actionUnit.OverlaySlot.StartDialog(LocalizationManager.GetString("str_control_before_turn_siren"));
            while (actionUnit.OverlaySlot.IsDoingDialog)
                yield return null;
        }
        #region Get Brain Decision
        var brainDecision = BattleSolver.UseMonsterBrain(actionUnit);
        if(brainDecision.Decision == BrainDecisionType.Pass)
        {
            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Pass);
            yield return new WaitForSeconds(0.9f);
            yield break;
        }
        #endregion
        yield return new WaitForSeconds(0.1f);
        SetBrainDecisionMarkings(actionUnit, brainDecision);
        ExecuteGuardRedirection(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.1f);
        brainDecision.TargetInfo.UpdateSkillInfo(actionUnit, brainDecision.SelectedSkill);
        #region Dipslay Announcment
        if (!(brainDecision.TargetInfo.SkillArtInfo.CanDisplaySelection == false))
        {
            if (actionUnit.Character.IsMonster)
            {
                if (actionUnit.Character.DisplayModifier != null && actionUnit.Character.DisplayModifier.UseCentreSkillAnnouncment)
                    RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_monster_skill_" +
                        brainDecision.TargetInfo.SkillArtInfo.SkillId), AnnouncmentPosition.Top);
                else
                    RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_monster_skill_" +
                        brainDecision.TargetInfo.SkillArtInfo.SkillId), AnnouncmentPosition.Right);
            }
            else
                RaidEvents.ShowAnnouncment(LocalizationManager.GetString("combat_skill_name_"
                    + actionUnit.Character.Class + "_" + brainDecision.TargetInfo.SkillArtInfo.SkillId), AnnouncmentPosition.Right);
        }
        #endregion
        yield return new WaitForSeconds(0.75f);
        Formations.HideUnitOverlay();
        TorchMeter.Hide();

        #region Hide Announcment
        if (!(brainDecision.TargetInfo.SkillArtInfo.CanDisplaySelection == false))
            RaidEvents.HideAnnouncment();
        #endregion
        yield return new WaitForSeconds(0.2f);
        dungeonCamera.Zoom(50, 0.05f);
        var skillResult = ExecuteSkillBase(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.05f);
        DungeonCamera.blur.enabled = true;
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
            RaidEvents.roundIndicator.Disappear();
            RaidEvents.roundIndicator.End();
            BattleGround.RetreatFromBattle();
            Formations.ResetSelections();
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
            RaidEvents.MonsterTooltip.Hide();
            #endregion

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

            dungeonCamera.Zoom(60, 0.1f);
            DungeonCamera.blur.enabled = false;
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
                currentEvent = RoomLoadingEvent(targetRooms[Random.Range(0, targetRooms.Count)],
                    RoomTransitionType.Teleport, HallwayView.CurrentSector);
            }
            else if (SceneState == DungeonSceneState.Room)
            {
                currentEvent = RoomLoadingEvent(targetRooms[Random.Range(0, targetRooms.Count)],
                    RoomTransitionType.Teleport);
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
        #endregion

        #region Riposte Skill Activation
        List<FormationUnit> riposters = new List<FormationUnit>();
        List<SkillResult> riposteResults = new List<SkillResult>();
        if (brainDecision.TargetInfo.Type == SkillTargetType.Enemy)
            foreach (var target in brainDecision.TargetInfo.Targets)
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

                                if(target.Character is Hero)
                                {
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
            actionUnit.SetPerformerSkillAnimation(brainDecision.TargetInfo.SkillArtInfo, false);
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
        dungeonCamera.Zoom(60, 0.1f);
        DungeonCamera.blur.enabled = false;
        #region Animation Outro
        if (riposteResults.Count > 0)
        {
            actionUnit.SetPerformerSkillAnimation(brainDecision.TargetInfo.SkillArtInfo, false);
            actionUnit.SetDefendAnimation(false);

            for (int i = 0; i < riposteResults.Count; i++)
            {
                riposters[i].SetDefendAnimation(false);
                riposters[i].SetPerformerSkillAnimation(riposteResults[i].ArtInfo, false);
            }
        }

        if (brainDecision.SelectedSkill.ValidModes.Count > 1 && brainDecision.TargetInfo.Mode != null)
            Formations.UnitSkillOutroOverriden(actionUnit,
                brainDecision.TargetInfo.SkillArtInfo, brainDecision.TargetInfo.Mode.Id);
        else
            Formations.UnitSkillOutro(actionUnit, brainDecision.TargetInfo.SkillArtInfo);

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

        for (int i = BattleGround.HeroParty.Units.Count - 1; i >= 0; i--)
            ExecuteDeath(BattleGround.HeroParty.Units[i]);

        List<DeathDamage> deathDamages = new List<DeathDamage>();
        for (int i = BattleGround.MonsterParty.Units.Count - 1; i >= 0; i--)
        {
            if (BattleGround.MonsterParty.Units[i].CombatInfo.IsDead)
                if (BattleGround.MonsterParty.Units[i].Character.DeathDamage != null)
                    deathDamages.Add(BattleGround.MonsterParty.Units[i].Character.DeathDamage);

            ExecuteDeath(BattleGround.MonsterParty.Units[i]);
        }

        #region Execute Death Damages
        for (int i = 0; i < deathDamages.Count; i++)
        {
            var deathDamageTarget = BattleGround.MonsterParty.Units.Find(unit =>
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

        if (brainDecision.TargetInfo.Type == SkillTargetType.Enemy && skillResult.HasCritEffect)
            for (int j = 0; j < BattleGround.HeroParty.Units.Count; j++)
                DarkestDungeonManager.Data.Effects["AfflictedAllyStress"].
                    ApplyIndependent(BattleGround.HeroParty.Units[j]);

        yield return StartCoroutine(ExecuteEffectEvents(true));
        for (int i = 0; i < brainDecision.TargetInfo.Targets.Count; i++)
            BattleSolver.RemoveConditions(brainDecision.TargetInfo.Targets[i]);
        BattleSolver.RemoveConditions(actionUnit);
        RaidEvents.MonsterTooltip.IsDisabled = false;
        #region Trait Comment Self and Ally
        if(brainDecision.TargetInfo.Type == SkillTargetType.Enemy)
        {
            foreach (var skillEntry in skillResult.SkillEntries)
            {
                if (skillEntry.Target.CombatInfo.IsDead == false)
                {
                    #region Comment on Self
                    if (skillEntry.Target.Character.Trait != null)
                    {
                        if (skillEntry.IsTargetHit)
                        {
                            #region Self Hit
                            if (RandomSolver.CheckSuccess(skillEntry.Target.Character.
                                Trait.Reactions[ReactionType.CommentSelfHit].Chance))
                            {
                                var barkStressEffect = skillEntry.Target.Character.
                                    Trait.Reactions[ReactionType.CommentSelfHit].Effect;
                                if (skillEntry.Target.Party.Units.Count > 1)
                                {
                                    yield return new WaitForSeconds(1f);
                                    tempList.Clear();
                                    tempList.AddRange(skillEntry.Target.Party.Units);
                                    tempList.Remove(skillEntry.Target);
                                    var barkTarget = tempList[Random.Range(0, tempList.Count)];
                                    tempList.Clear();
                                    for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                                        barkStressEffect.SubEffects[i].Apply(skillEntry.Target, barkTarget, barkStressEffect);
                                    yield return new WaitForSeconds(0.1f);
                                    yield return StartCoroutine(ExecuteEffectEvents(false));
                                    break;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region Self Miss
                            if (RandomSolver.CheckSuccess(skillEntry.Target.Character.
                                Trait.Reactions[ReactionType.CommentSelfMissed].Chance))
                            {
                                var barkStressEffect = skillEntry.Target.Character.
                                    Trait.Reactions[ReactionType.CommentSelfHit].Effect;
                                if (skillEntry.Target.Party.Units.Count > 1)
                                {
                                    yield return new WaitForSeconds(1f);
                                    tempList.Clear();
                                    tempList.AddRange(skillEntry.Target.Party.Units);
                                    tempList.Remove(skillEntry.Target);
                                    var barkTarget = tempList[Random.Range(0, tempList.Count)];
                                    tempList.Clear();
                                    for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                                        barkStressEffect.SubEffects[i].Apply(skillEntry.Target, barkTarget, barkStressEffect);
                                    yield return new WaitForSeconds(0.1f);
                                    yield return StartCoroutine(ExecuteEffectEvents(false));
                                    break;
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion

                    #region Comment on Ally
                    if (skillEntry.Target.Party.Units.Count > 1)
                    {
                        foreach(var ally in skillEntry.Target.Party.Units)
                        {
                            if (ally == skillEntry.Target)
                                continue;

                            if(ally.Character.Trait != null)
                            {
                                if (skillEntry.IsTargetHit)
                                {
                                    #region Ally Hit
                                    if (RandomSolver.CheckSuccess(ally.Character.
                                        Trait.Reactions[ReactionType.CommentAllyHit].Chance))
                                    {
                                        var barkStressEffect = ally.Character.
                                            Trait.Reactions[ReactionType.CommentAllyHit].Effect;
                                        if (skillEntry.Target.Party.Units.Count > 1)
                                        {
                                            yield return new WaitForSeconds(1f);
                                            for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                                                barkStressEffect.SubEffects[i].Apply(ally, skillEntry.Target, barkStressEffect);
                                            yield return new WaitForSeconds(0.1f);
                                            yield return StartCoroutine(ExecuteEffectEvents(false));
                                            break;
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region Ally Miss
                                    if (RandomSolver.CheckSuccess(ally.Character.
                                        Trait.Reactions[ReactionType.CommentAllyMissed].Chance))
                                    {
                                        var barkStressEffect = ally.Character.
                                            Trait.Reactions[ReactionType.CommentAllyMissed].Effect;
                                        if (skillEntry.Target.Party.Units.Count > 1)
                                        {
                                            yield return new WaitForSeconds(1f);
                                            for (int i = 0; i < barkStressEffect.SubEffects.Count; i++)
                                                barkStressEffect.SubEffects[i].Apply(ally, skillEntry.Target, barkStressEffect);
                                            yield return new WaitForSeconds(0.1f);
                                            yield return StartCoroutine(ExecuteEffectEvents(false));
                                            break;
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }
        #endregion
    }
    IEnumerator ExecuteMonsterOverridenSkill(FormationUnit actionUnit, string combatSkillOverride)
    {
        RaidEvents.MonsterTooltip.IsDisabled = true;
        RaidEvents.MonsterTooltip.Hide();
        #region Get Brain Decision
        var brainDecision = BattleSolver.UseMonsterBrain(actionUnit, combatSkillOverride);
        if (brainDecision.Decision == BrainDecisionType.Pass)
        {
            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Pass);
            yield return new WaitForSeconds(0.9f);
            yield break;
        }
        #endregion
        yield return new WaitForSeconds(0.1f);
        SetBrainDecisionMarkings(actionUnit, brainDecision);
        yield return new WaitForSeconds(0.1f);
        brainDecision.TargetInfo.UpdateSkillInfo(actionUnit, brainDecision.SelectedSkill);
        #region Dipslay Announcment
        if (!(brainDecision.TargetInfo.SkillArtInfo.CanDisplaySelection == false))
        {
            if (actionUnit.Character.IsMonster)
            {
                if(actionUnit.Character.DisplayModifier != null &&
                    actionUnit.Character.DisplayModifier.UseCentreSkillAnnouncment)
                    RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_monster_skill_" +
                        brainDecision.TargetInfo.SkillArtInfo.SkillId), AnnouncmentPosition.Top);
                else
                    RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_monster_skill_" +
                        brainDecision.TargetInfo.SkillArtInfo.SkillId), AnnouncmentPosition.Right);
            }
            else
                RaidEvents.ShowAnnouncment(LocalizationManager.GetString("combat_skill_name_"
                    + actionUnit.Character.Class + "_" + brainDecision.TargetInfo.SkillArtInfo.SkillId), AnnouncmentPosition.Right);
        }
        #endregion
        yield return new WaitForSeconds(0.75f);
        Formations.HideUnitOverlay();
        TorchMeter.Hide();
        #region Hide Announcment
        if (!(brainDecision.TargetInfo.SkillArtInfo.CanDisplaySelection == false))
            RaidEvents.HideAnnouncment();
        #endregion
        yield return new WaitForSeconds(0.2f);
        dungeonCamera.Zoom(50, 0.05f);
        var skillResult = ExecuteSkillBase(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.05f);
        DungeonCamera.blur.enabled = true;
        ExecuteSkillAnimationIntro(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(0.01f);
        ExecuteSkillInstants(actionUnit, brainDecision.TargetInfo, skillResult);
        yield return new WaitForSeconds(0.01f);
        ExecuteSlidingSetup(actionUnit, brainDecision.TargetInfo);
        yield return new WaitForSeconds(1.5f);
        dungeonCamera.Zoom(60, 0.1f);
        DungeonCamera.blur.enabled = false;
        #region Animation Outro
        Formations.UnitSkillOutro(actionUnit, brainDecision.TargetInfo.SkillArtInfo);

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

        if (actionUnit.CombatInfo.MarkedForDeath)
        {
            actionUnit.CombatInfo.IsDead = true;
            List<FormationUnit> summonPurging = new List<FormationUnit>();
            for (int i = 0; i < actionUnit.Party.Units.Count; i++)
            {
                if (actionUnit.Party.Units[i].Character.LifeLink != null)
                {
                    if (actionUnit.Party.Units[i].Character.LifeLink.LinkBaseClass == actionUnit.Character.Class)
                        summonPurging.Add(actionUnit.Party.Units[i]);
                }
            }
            for (int i = 0; i < summonPurging.Count; i++)
            {
                SummonPurging(summonPurging[i]);
            }
            summonPurging.Clear();
            ExecuteDeath(actionUnit);
        }


        for (int i = BattleGround.HeroParty.Units.Count - 1; i >= 0; i--)
            ExecuteDeath(BattleGround.HeroParty.Units[i]);

        List<DeathDamage> deathDamages = new List<DeathDamage>();
        for (int i = BattleGround.MonsterParty.Units.Count - 1; i >= 0; i--)
        {
            if (BattleGround.MonsterParty.Units[i].CombatInfo.IsDead)
                if (BattleGround.MonsterParty.Units[i].Character.DeathDamage != null)
                    deathDamages.Add(BattleGround.MonsterParty.Units[i].Character.DeathDamage);

            ExecuteDeath(BattleGround.MonsterParty.Units[i]);
        }

        #region Execute Death Damages
        for (int i = 0; i < deathDamages.Count; i++)
        {
            var deathDamageTarget = BattleGround.MonsterParty.Units.Find(unit =>
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

        #endregion
        yield return new WaitForSeconds(0.175f);
        Formations.ShowUnitOverlay();
        TorchMeter.Show();
        Formations.ResetSelections();
        yield return new WaitForSeconds(0.075f);
        yield return StartCoroutine(ExecuteEffectEvents(true));
        for (int i = 0; i < brainDecision.TargetInfo.Targets.Count; i++)
            BattleSolver.RemoveConditions(brainDecision.TargetInfo.Targets[i]);
        BattleSolver.RemoveConditions(actionUnit);
        RaidEvents.MonsterTooltip.IsDisabled = false;
    }
    IEnumerator ExecuteHeroSkill(FormationUnit actionUnit, SkillTargetInfo targetInfo, CombatSkill skill)
    {
        RaidEvents.MonsterTooltip.IsDisabled = true;
        RaidEvents.MonsterTooltip.Hide();
        ExecuteGuardRedirection(actionUnit, targetInfo);
        Formations.HideUnitOverlay();
        TorchMeter.Hide();
        yield return new WaitForSeconds(0.2f);
        dungeonCamera.Zoom(50, 0.05f);
        var skillResult = ExecuteSkillBase(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.05f);
        DungeonCamera.blur.enabled = true;
        ExecuteSkillAnimationIntro(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.01f);
        ExecuteSkillInstants(actionUnit, targetInfo, skillResult);
        yield return new WaitForSeconds(0.01f);
        ExecuteSlidingSetup(actionUnit, targetInfo);
        yield return new WaitForSeconds(0.70f);
        #region Riposte Skill Activation
        List<FormationUnit> riposters = new List<FormationUnit>();
        List<SkillResult> riposteResults = new List<SkillResult>();
        if(targetInfo.Type == SkillTargetType.Enemy)
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
        if(riposteResults.Count > 0)
        {
            actionUnit.SetPerformerSkillAnimation(targetInfo.SkillArtInfo, false);
            actionUnit.SetDefendAnimation(true);

            for(int i = 0; i < riposteResults.Count; i++)
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
        dungeonCamera.Zoom(60, 0.1f);
        DungeonCamera.blur.enabled = false;
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
        for (int i = BattleGround.MonsterParty.Units.Count - 1; i >= 0; i--)
        {
            if (BattleGround.MonsterParty.Units[i].CombatInfo.IsDead)
                if (BattleGround.MonsterParty.Units[i].Character.DeathDamage != null)
                    deathDamages.Add(BattleGround.MonsterParty.Units[i].Character.DeathDamage);

            ExecuteDeath(BattleGround.MonsterParty.Units[i]);
        }
        ExecuteDeath(actionUnit);
        #endregion

        #region Execute Death Damages
        for (int i = 0; i < deathDamages.Count; i++)
        {
            var deathDamageTarget = BattleGround.MonsterParty.Units.Find(unit =>
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
    IEnumerator ExecuteHeroItemUsage(FormationUnit actionUnit, InventorySlot slot)
    {
        if (actionUnit == null || slot.HasItem == false)
        {
            itemUsageEvent = null;
            yield break;
        }

        switch (slot.SlotItem.ItemData.Type)
        {
            case "provision":
                if (actionUnit.Character.Health.ValueRatio < 1)
                {
                    Inventory.DiscardSingleItem(slot);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                    yield return new WaitForSeconds(0.1f);
                    int healthRestored = Mathf.CeilToInt(actionUnit.Character.Health.ModifiedValue * 0.05f);
                    actionUnit.Character.Health.IncreaseValue(healthRestored);
                    if (actionUnit.Character.AtDeathsDoor)
                        (actionUnit.Character as Hero).RevertDeathsDoor();
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
                        if(SceneState == DungeonSceneState.Room && currentEvent == null)
                        {
                            Inventory.DiscardSingleItem(slot);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            currentEvent = CampingEvent(RoomView.raidRoom.Area as Room);
                            StartCoroutine(currentEvent);
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
                            RaidSceneManager.RaidPanel.SelectedHero.RemoveCombatDebuffs();
                            RaidEvents.ShowPopupMessage(actionUnit, PopupMessageType.Cured);
                            actionUnit.OverlaySlot.UpdateOverlay();
                            actionUnit.SetTargetItemEffect("medicinal_herbs");
                            Inventory.UpdateState();
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/medicinal_herbs");
                            yield return new WaitForSeconds(0.8f);
                        }
                        break;
                    case "torch":
                        if (RaidSceneManager.TorchMeter.TorchAmount < RaidSceneManager.TorchMeter.MaxAmount)
                        {
                            Inventory.DiscardSingleItem(slot);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/items/discard");
                            yield return new WaitForSeconds(0.1f);
                            RaidSceneManager.TorchMeter.IncreaseTorch(25);
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
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        actionUnit.Character.ApplyAllBuffRules(Rules.GetIdleUnitRules(actionUnit));
        itemUsageEvent = null;
        yield break;
    }

    IEnumerator ExecuteResolveChecks()
    {
        while(resolveCheckQueue.Count > 0)
        {
            var resolveUnit = resolveCheckQueue[0];
            var resolveHero = resolveUnit.Character as Hero;
            resolveCheckQueue.RemoveAt(0);
            float virtueChance = 0.25f + resolveUnit.Character[AttributeType.ResolveCheckPercent].ModifiedValue;
            virtueChance = Mathf.Clamp(virtueChance, 0.01f, 0.6f);
            bool isVirtue = RandomSolver.CheckSuccess(virtueChance);
            var availableTraits = isVirtue ? DarkestDungeonManager.Data.Traits.FindAll(trait => trait.Type == OverstressType.Virtue) :
                DarkestDungeonManager.Data.Traits.FindAll(trait => trait.Type == OverstressType.Affliction);
            Trait resolveTrait = availableTraits[Random.Range(0, availableTraits.Count)];

            if (!isVirtue)
                for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
                    if (Formations.heroes.party.Units[i] != resolveUnit)
                        DarkestDungeonManager.Data.Effects["AfflictedAllyStress"].
                            ApplyIndependent(Formations.heroes.party.Units[i]);

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
            DungeonCamera.blur.enabled = true;

            Rules.GetIdleUnitRules(resolveUnit);
            resolveHero.ApplyTrait(resolveTrait);
            resolveHero.ApplySingleBuffRule(Rules, BuffRule.Afflicted);
            resolveHero.ApplySingleBuffRule(Rules, BuffRule.Virtued);

            if(isVirtue)
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_virtue");
            else
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_afflict");

            Formations.HeroResolveCheckIntro(resolveUnit, isVirtue);
            Formations.partyBuffPositions.SetUnitTarget(resolveUnit, 0.05f, Vector2.zero);
            RaidEvents.ShowAnnouncment(isVirtue ? 
                LocalizationManager.GetString("str_virtue_name_" + resolveTrait.Id):
                LocalizationManager.GetString("str_affliction_name_" + resolveTrait.Id), AnnouncmentPosition.Bottom);
            if(!Rules.IsDoingCamping)
                dungeonCamera.Zoom(45, 0.1f);

            yield return new WaitForSeconds(2.45f);
            if(!Rules.IsDoingCamping)
                dungeonCamera.Zoom(60, 0.1f);

            Formations.HeroResolveCheckOutro(resolveUnit, isVirtue);
            DungeonCamera.blur.enabled = false;
            yield return new WaitForSeconds(0.15f);
            if (isVirtue)
                resolveUnit.Character.Stress.CurrentValue = Random.Range(20, 40);
            resolveUnit.OverlaySlot.UpdateOverlay();

            RaidEvents.HideAnnouncment();
            Formations.ShowUnitOverlay();
            yield return new WaitForSeconds(0.15f);
        }
        yield break;
    }
    IEnumerator ExecuteRoundAdvance()
    {
        roundAdvanceCounter++;

        while (unitEventQueue.Count > 0)
            yield return null;

        unitEventQueue.AddRange(Formations.heroes.party.Units);

        bleedDeaths.Clear();
        poisonDeaths.Clear();
        float timeWasted = 0;
        bool executedBleed = false;
        bool executedPoison = false;
        bool movementStopped = false;

        #region Bleeding
        for (int i = unitEventQueue.Count - 1; i >= 0; i--)
        {
            if (unitEventQueue[i].CombatInfo.IsDead)
                continue;

            var bleedEffect = unitEventQueue[i].Character.GetStatusEffect(StatusType.Bleeding) as BleedingStatusEffect;
            if(bleedEffect.IsApplied)
            {
                unitEventQueue[i].Character.Health.DecreaseValue(bleedEffect.CurrentTickDamage);
                unitEventQueue[i].OverlaySlot.UpdateOverlay();
                executedBleed = true;

                if (Mathf.RoundToInt(unitEventQueue[i].Character.Health.CurrentValue) != 0)
                    RaidEvents.ShowPopupMessage(unitEventQueue[i], PopupMessageType.Damage, bleedEffect.CurrentTickDamage.ToString());
                else
                    if (PrepareDeath(unitEventQueue[i]))
                    {
                        if (partyController.MovementAllowed)
                        {
                            movementStopped = true;
                            DisablePartyMovement();
                        }
                        bleedDeaths.Add(unitEventQueue[i]);
                        RaidEvents.ShowPopupMessage(unitEventQueue[i], PopupMessageType.DeathBlow);
                    }
                    else
                        RaidEvents.ShowPopupMessage(unitEventQueue[i], PopupMessageType.DeathsDoor);
            }
        }

        if(executedBleed)
        {
            yield return new WaitForSeconds(0.6f);
            timeWasted += 0.6f;
        }
        #endregion

        #region Poisoning
        for (int i = unitEventQueue.Count - 1; i >= 0; i--)
        {
            if (unitEventQueue[i].CombatInfo.IsDead)
                continue;

            var poisonEffect = unitEventQueue[i].Character.GetStatusEffect(StatusType.Poison) as PoisonStatusEffect;
            if (poisonEffect.IsApplied)
            {
                unitEventQueue[i].Character.Health.DecreaseValue(poisonEffect.CurrentTickDamage);
                unitEventQueue[i].OverlaySlot.UpdateOverlay();
                executedPoison = true;

                if (Mathf.RoundToInt(unitEventQueue[i].Character.Health.CurrentValue) != 0)
                    RaidEvents.ShowPopupMessage(unitEventQueue[i], PopupMessageType.Damage, poisonEffect.CurrentTickDamage.ToString());
                else
                    if (PrepareDeath(unitEventQueue[i]))
                    {
                        if (partyController.MovementAllowed)
                        {
                            movementStopped = true;
                            DisablePartyMovement();
                        }
                        poisonDeaths.Add(unitEventQueue[i]);
                        RaidEvents.ShowPopupMessage(unitEventQueue[i], PopupMessageType.DeathBlow);
                    }
                    else
                        RaidEvents.ShowPopupMessage(unitEventQueue[i], PopupMessageType.DeathsDoor);
            }
        }

        if (executedPoison)
        {
            yield return new WaitForSeconds(0.6f);
            timeWasted += 0.6f;
        }
        #endregion

        #region Deaths
        for (int i = 0; i < unitEventQueue.Count; i++)
        {
            unitEventQueue[i].Character.UpdateRound();
            unitEventQueue[i].OverlaySlot.UpdateOverlay();
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

        unitEventQueue.Clear();

        #region Comment on Movement
        if (Formations.heroes.party.Units.Count > 1)
        {
            foreach (var heroBarker in Formations.heroes.party.Units)
            {
                if (heroBarker.Character.Trait != null)
                {
                    if (RandomSolver.CheckSuccess(heroBarker.Character.Trait.Reactions[ReactionType.CommentMove].Chance))
                    {
                        var barkStressEffect = heroBarker.Character.Trait.Reactions[ReactionType.CommentMove].Effect;
                        yield return new WaitForSeconds(1f);
                        tempList.Clear();
                        tempList.AddRange(Formations.heroes.party.Units);
                        tempList.Remove(heroBarker);
                        var barkTarget = tempList[Random.Range(0, tempList.Count)];
                        tempList.Clear();
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
    IEnumerator ExecuteEffectEvents(bool includeMonsters)
    {
        effectEvent = ExecuteEffectEvents(includeMonsters);

        bool executedEvent = false;

        for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
            BattleGround.HeroParty.Units[i].StackEvents();
        if (includeMonsters)
            for (int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
                BattleGround.MonsterParty.Units[i].StackEvents();

        do
        {
            #region Death Doors
            if (deathDoorEnterQueue.Count > 0)
            {
                if (RaidEvents.CampEvent.ActionType == CampUsageResultType.Skill)
                    RaidEvents.CampEvent.Hide();

                FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/deaths_door");

                foreach (var deathDoorUnit in deathDoorEnterQueue)
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
                switch (deathDoorEnterQueue.Count)
                {
                    case 1:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_1_death"),
                            deathDoorEnterQueue[0].Character.Name);
                        break;
                    case 2:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_2_death"),
                            deathDoorEnterQueue[0].Character.Name, deathDoorEnterQueue[1].Character.Name);
                        break;
                    case 3:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_3_death"),
                            deathDoorEnterQueue[0].Character.Name, deathDoorEnterQueue[1].Character.Name,
                            deathDoorEnterQueue[2].Character.Name);
                        break;
                    case 4:
                        deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_4_death"),
                            deathDoorEnterQueue[0].Character.Name, deathDoorEnterQueue[1].Character.Name,
                            deathDoorEnterQueue[2].Character.Name, deathDoorEnterQueue[3].Character.Name);
                        break;
                    default:
                        Debug.LogError("Too much deathdoors!");
                        break;
                }
                if (deathDoorEnterQueue.Count > 1)
                    RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("str_ui_deathdoor_multy"), deathDoorHeroes));
                else
                    RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("str_ui_deathdoor"), deathDoorHeroes));

                deathDoorEnterQueue.Clear();
                yield return new WaitForSeconds(1.6f);
                RaidEvents.HideAnnouncment();
            }
            #endregion

            #region Effects
            do
            {
                executedEvent = false;

                unitEventQueue.AddRange(BattleGround.HeroParty.Units);
                if (includeMonsters)
                    unitEventQueue.AddRange(BattleGround.MonsterParty.Units);

                while (unitEventQueue.Count > 0)
                {
                    var eventUnit = unitEventQueue[0];
                    unitEventQueue.Remove(eventUnit);

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
            if (resolveCheckQueue.Count != 0)
            {
                executedEvent = true;
                if (RaidEvents.CampEvent.ActionType == CampUsageResultType.Skill)
                    RaidEvents.CampEvent.Hide();
                yield return StartCoroutine(ExecuteResolveChecks());
            }
            #endregion

            #region Heart Attacks
            while (heartAttackCheckQueue.Count > 0)
            {
                executedEvent = true;
                var heartAttackedUnit = heartAttackCheckQueue[0];
                heartAttackCheckQueue.RemoveAt(0);

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
                    heartAttackedUnit.Character.Health.ValueRatio = 0;
                    heartAttackedUnit.Character.Stress.ValueRatio = 0.75f;
                    heartAttackedUnit.OverlaySlot.UpdateOverlay();
                    yield return new WaitForSeconds(0.2f);
                    deathDoorEnterQueue.Add(heartAttackedUnit);
                }
            }
            #endregion

            if (deathDoorEnterQueue.Count != 0 || resolveCheckQueue.Count != 0 || heartAttackCheckQueue.Count != 0)
                executedEvent = true;
        } 
        while (executedEvent);

        for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
            BattleGround.HeroParty.Units[i].Character.ApplyAllBuffRules(
                Rules.GetIdleUnitRules(BattleGround.HeroParty.Units[i]));
        effectEvent = null;
    }
    IEnumerator ExecuteRandomDialog(FormationUnit unit, string dialogId)
    {
        if (RandomSolver.CheckSuccess(DarkestDungeonManager.RandomBarkChance))
        {
            unit.OverlaySlot.StartDialog(LocalizationManager.GetString(dialogId));
            while (unit.OverlaySlot.IsDoingDialog)
                yield return null;
        }
    }
    IEnumerator HungerEvent()
    {
        DisableEnviroment();
        Inventory.SetDeactivated();
        RaidPanel.SetDisabledState();
        RaidPanel.heroPanel.equipmentPanel.SetDisabled();
        QuestPanel.DisableRetreat(false);
        RaidPanel.bannerPanel.SetDisabledState();
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
        RaidEvents.HungerEvent.ScrollClosed();

        if(RaidEvents.HungerEvent.ActionType == HungerResultType.Starve)
        {
            #region Starving
            bool someOneStarved = false;
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/bleed_dot");
            for (int i = 0; i < HeroParty.Units.Count; i++)
            {
                int starveDamage = Mathf.RoundToInt(HeroParty.Units[i].Character.Health.ModifiedValue * 0.2f);
                HeroParty.Units[i].Character.Health.DecreaseValue(starveDamage);
                DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(HeroParty.Units[i]);

                HeroParty.Units[i].OverlaySlot.UpdateOverlay();

                #region Damage Activation
                if (Mathf.RoundToInt(HeroParty.Units[i].Character.Health.CurrentValue) != 0)
                {
                    RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Damage, starveDamage.ToString());
                }
                else
                {
                    if (HeroParty.Units[i].Character.AtDeathsDoor)
                    {
                        if (PrepareDeath(HeroParty.Units[i]))
                        {
                            RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.DeathBlow);
                            someOneStarved = true;
                        }
                        else
                            RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.DeathsDoor);
                    }
                    else
                    {
                        PrepareDeath(HeroParty.Units[i]);
                        RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Damage, starveDamage.ToString());
                    }
                }
                #endregion
            }
            if (someOneStarved)
            {
                yield return new WaitForSeconds(1.4f);
                for (int i = HeroParty.Units.Count - 1; i >= 0; i--)
                    ExecuteDeath(HeroParty.Units[i]);
                yield return new WaitForSeconds(0.3f);
            }
            else
                yield return new WaitForSeconds(0.6f);

            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return new WaitForSeconds(0.3f);

            if (HeroParty.Units.Count == 0)
            {
                StartCoroutine(RaidResultsEvent());
                yield break;
            }
            #endregion
        }
        else if (RaidEvents.HungerEvent.ActionType == HungerResultType.Eat)
        {
            #region Eating
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/party/eat");

            for (int i = 0; i < HeroParty.Units.Count; i++)
            {
                int mealHeal = Mathf.RoundToInt(HeroParty.Units[i].Character.Health.ModifiedValue * 0.1f);
                HeroParty.Units[i].Character.Health.IncreaseValue(mealHeal);
                if (HeroParty.Units[i].Character.AtDeathsDoor)
                    (HeroParty.Units[i].Character as Hero).RevertDeathsDoor();
                HeroParty.Units[i].OverlaySlot.UpdateOverlay();
                RaidEvents.ShowPopupMessage(HeroParty.Units[i], PopupMessageType.Heal, mealHeal.ToString());
            }

            yield return StartCoroutine(ExecuteEffectEvents(false));
            yield return new WaitForSeconds(0.6f);

            if (HeroParty.Units.Count == 0)
            {
                StartCoroutine(RaidResultsEvent());
                yield break;
            }
            #endregion
        }

        yield return new WaitForSeconds(0.1f);
        QuestPanel.EnableRetreat();
        EnablePartyMovement();
        EnableEnviroment();
        Inventory.SetPeacefulState(false);
        RaidPanel.heroPanel.equipmentPanel.SetActive();
        RaidPanel.SetPeacefulState();
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
        currentEvent = null;
    }
    IEnumerator LootEvent()
    {
        if (RaidEvents.LootEvent.KeepLoot)
        {
            yield return new WaitForSeconds(3f);
            RaidEvents.LootEvent.Close();
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            Inventory.SetPeacefulState(true);
            float announcementTimer = -1;
            while (true)
            {
                if (announcementTimer > 0)
                {
                    announcementTimer -= Time.deltaTime;
                    if (announcementTimer <= 0)
                    {
                        if (RaidEvents.announcment.animator.isInitialized)
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
                    if (RaidEvents.announcment.animator.isInitialized)
                        RaidEvents.HideAnnouncment();
                    break;
                }
            }
        }

        if (!Raid.QuestCompleted && Raid.CheckQuestGoals())
            yield return StartCoroutine(CompletionCrestEvent());
    }
    IEnumerator LoadEncounterEvent(IRaidArea areaView)
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

        #region Switch Soundtrack
        DarkestSoundManager.PauseDungeonSoundtrack();
        DarkestSoundManager.StartBattleSoundtrack(Raid.Dungeon.Name, SceneState == DungeonSceneState.Room);
        #endregion

        #region Battle Loop
        BattleGround.LoadBattle(DarkestDungeonManager.SaveData.battleGroundSaveData);
        yield return new WaitForEndOfFrame();
        BattleGround.LoadEffects(DarkestDungeonManager.SaveData.battleGroundSaveData);

        RaidEvents.roundIndicator.Appear();

        yield return new WaitForSeconds(1f);
        DarkestDungeonManager.Instanse.screenFader.Appear(2);
        yield return new WaitForSeconds(0.5f);
        RaidPanel.SwitchBlocked = false;
        Formations.ShowHeroOverlay();

        yield return StartCoroutine(BattleRound(true));
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
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");
        DarkestSoundManager.ContinueDungeonSoundtrack(Raid.Quest.Dungeon);
        #endregion

        #region Check Game Over
        if (Formations.heroes.party.Units.Count == 0)
        {
            StartCoroutine(RaidResultsEvent());
            yield break;
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

        #region Provide Loot
        if (BattleGround.BattleLoot.Count > 0)
        {
            Formations.UnlockSelections();
            RaidEvents.LoadBattleLoot(BattleGround.BattleLoot);
            if (RaidEvents.loot.partyInventory.HasSomething())
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
            Curio curio = areaView.Area.Prop as Curio;
            if (curio.IsQuestCurio == false)
            {
                foreach (var triggeredHero in RaidSceneManager.Formations.heroes.party.Units)
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
            Room room = RoomView.raidRoom.Area as Room;
            yield return StartCoroutine(ScoutingEvent(room));
        }
        if (sceneState == DungeonSceneState.Room)
            MapPanel.ShowAvailableRooms(RoomView.raidRoom.Area as Room);
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
    IEnumerator EncounterEvent(IRaidArea areaView, bool campfireAmbush = false)
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
        #endregion

        #region Battle Loop
        BattleGround.InitiateBattle();
        yield return new WaitForSeconds(1f);
        BattleGround.SpawnEncounter(areaView.Area.BattleEncounter, campfireAmbush);

        #region Starting Sound Effects
        var oneShotStart = FMODUnity.RuntimeManager.CreateInstance("event:/general/combat/start");
        if (BattleGround.SurpriseStatus == SurpriseStatus.Nothing)
            oneShotStart.setParameterValue("start_condition", 0);
        else if (BattleGround.SurpriseStatus == SurpriseStatus.MonstersSurprised)
            oneShotStart.setParameterValue("start_condition", 1);
        else
            oneShotStart.setParameterValue("start_condition", 2);

        oneShotStart.start();
        oneShotStart.release();

        foreach(var unit in Formations.monsters.party.Units)
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
            tempList.Clear();
            tempList.AddRange(BattleGround.HeroParty.Units);

            foreach (var unit in tempList)
            {
                unit.SetSurprised(true);
                var shuffleTargets = unit.Party.Units.FindAll(shuffle => shuffle != unit);
                var shuffleRoll = shuffleTargets[Random.Range(0, shuffleTargets.Count)];

                if (shuffleRoll.Rank < unit.Rank)
                    unit.Pull(unit.Rank - shuffleRoll.Rank);
                else
                    unit.Push(shuffleRoll.Rank - unit.Rank);
            }
            tempList.Clear();
            yield return new WaitForSeconds(1.2f);
            RaidEvents.HideAnnouncment();
        }
        else if (BattleGround.SurpriseStatus == SurpriseStatus.MonstersSurprised)
        {
            RaidEvents.ShowAnnouncment(LocalizationManager.GetString("surprise_announcement"), AnnouncmentPosition.Right);
            for (int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
                if (BattleGround.MonsterParty.Units[i].Character.BattleModifiers != null)
                    if (BattleGround.MonsterParty.Units[i].Character.BattleModifiers.CanBeSurprised == true)
                        BattleGround.MonsterParty.Units[i].SetSurprised(true);

            yield return new WaitForSeconds(1.2f);
            RaidEvents.HideAnnouncment();
        }
        else
            yield return new WaitForSeconds(0.8f);
        #endregion

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
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/victory");
        DarkestSoundManager.ContinueDungeonSoundtrack(Raid.Quest.Dungeon);
        #endregion

        #region Check Game Over
        if (Formations.heroes.party.Units.Count == 0)
        {
            StartCoroutine(RaidResultsEvent());
            yield break;
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

        #region Provide Loot
        if (BattleGround.BattleLoot.Count > 0)
        {
            Formations.UnlockSelections();
            RaidEvents.LoadBattleLoot(BattleGround.BattleLoot);
            if(RaidEvents.loot.partyInventory.HasSomething())
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
            Curio curio = areaView.Area.Prop as Curio;
            if (curio.IsQuestCurio == false)
            {
                foreach (var triggeredHero in RaidSceneManager.Formations.heroes.party.Units)
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
            Room room = RoomView.raidRoom.Area as Room;
            yield return StartCoroutine(ScoutingEvent(room));
        }
        if (sceneState == DungeonSceneState.Room)
            MapPanel.ShowAvailableRooms(RoomView.raidRoom.Area as Room);
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

    IEnumerator ScoutingHallway(Hallway hallway, Direction direction, int tiles)
    {
        int scoutingSectors = Mathf.Min(hallway.HallCount, tiles);
        tiles -= scoutingSectors;

        if (direction == Direction.Bot || direction == Direction.Left)
        {
            for (int i = 0; i < scoutingSectors; i++)
            {
                if (hallway.Halls[i].Knowledge == Knowledge.Hidden)
                {
                    hallway.Halls[i].Knowledge = Knowledge.Scouted;

                    if (hallway.Halls[i].Type != AreaType.Door)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                        MapPanel.UpdateArea(hallway.Halls[i]);
                        yield return new WaitForSeconds(0.4f);
                    }
                }
                else if (hallway.Halls[i].Type != AreaType.Door)
                    yield return new WaitForSeconds(0.4f);
            }

            if(scoutingSectors == hallway.HallCount)
            {
                if(hallway.RoomB.Knowledge == Knowledge.Hidden)
                {
                    hallway.RoomB.Knowledge = Knowledge.Scouted;
                    MapPanel.UpdateArea(hallway.RoomB);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_room");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                    yield return new WaitForSeconds(0.5f);

                if (currentScoutedRooms.ContainsKey(hallway.RoomB))
                {
                    if (currentScoutedRooms[hallway.RoomB] < tiles)
                        currentScoutedRooms[hallway.RoomB] = tiles;
                    else
                        tiles = 0;
                }
                else
                    currentScoutedRooms.Add(hallway.RoomB, tiles);

                if (tiles > 0)
                {
                    for (int i = 0; i < hallway.RoomB.Doors.Count; i++)
                    {
                        if (hallway.RoomB.Doors[i].TargetArea == hallway.Id)
                            continue;

                        StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[hallway.RoomB.Doors[i].TargetArea],
                            hallway.RoomB.Doors[i].Direction, tiles));
                        scoutingCounter++;
                    }
                }
            }
        }
        else if (direction == Direction.Top || direction == Direction.Right)
        {
            for (int i = hallway.HallCount - 1; i >= hallway.HallCount - scoutingSectors; i--)
            {
                if (hallway.Halls[i].Knowledge == Knowledge.Hidden)
                {
                    hallway.Halls[i].Knowledge = Knowledge.Scouted;

                    if (hallway.Halls[i].Type != AreaType.Door)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                        MapPanel.UpdateArea(hallway.Halls[i]);
                        yield return new WaitForSeconds(0.4f);
                    }
                }
                else if (hallway.Halls[i].Type != AreaType.Door)
                    yield return new WaitForSeconds(0.4f);
            }

            if (scoutingSectors == hallway.HallCount)
            {
                if (hallway.RoomA.Knowledge == Knowledge.Hidden)
                {
                    hallway.RoomA.Knowledge = Knowledge.Scouted;
                    MapPanel.UpdateArea(hallway.RoomA);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_room");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                    yield return new WaitForSeconds(0.5f);

                if (currentScoutedRooms.ContainsKey(hallway.RoomA))
                {
                    if (currentScoutedRooms[hallway.RoomA] < tiles)
                        currentScoutedRooms[hallway.RoomA] = tiles;
                    else
                        tiles = 0;
                }
                else
                    currentScoutedRooms.Add(hallway.RoomA, tiles);

                if (tiles > 0)
                {
                    for (int i = 0; i < hallway.RoomA.Doors.Count; i++)
                    {
                        if (hallway.RoomA.Doors[i].TargetArea == hallway.Id)
                            continue;

                        StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[hallway.RoomA.Doors[i].TargetArea],
                            hallway.RoomA.Doors[i].Direction, tiles));
                        scoutingCounter++;
                    }
                }
            }
        }
        scoutingCounter--;
    }
    IEnumerator CurioScoutingHallway(HallSector hallSector, Direction direction, int tiles)
    {
        if (direction == Direction.Bot || direction == Direction.Left)
        {
            int hallIndex = hallSector.Hallway.Halls.IndexOf(hallSector);
            int tilesScouted = Mathf.Min(hallIndex, tiles);
            int lastHallIndex = hallIndex - tilesScouted;

            for (int i = hallIndex - 1; i >= lastHallIndex; i--)
            {
                if (hallSector.Hallway.Halls[i].Knowledge == Knowledge.Hidden)
                {
                    hallSector.Hallway.Halls[i].Knowledge = Knowledge.Scouted;

                    if (hallSector.Hallway.Halls[i].Type != AreaType.Door)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                        MapPanel.UpdateArea(hallSector.Hallway.Halls[i]);
                        if(hallSector.Hallway.Halls[i].Type == AreaType.Trap)
                        {
                            var raidSector = HallwayView.raidHallway.HallSectors.
                                Find(trapSector => trapSector.HallSector == hallSector.Hallway.Halls[i]);
                            if(raidSector != null)
                                (raidSector.Prop as RaidTrap).SkeletonAnimation.MeshRenderer.enabled = true;
                        }
                        yield return new WaitForSeconds(0.4f);
                    }
                }
                else if (hallSector.Hallway.Halls[i].Type != AreaType.Door)
                    yield return new WaitForSeconds(0.4f);
            }

            tiles -= tilesScouted;

            if (tiles >= 0)
            {
                if (hallSector.Hallway.RoomA.Knowledge == Knowledge.Hidden)
                {
                    hallSector.Hallway.RoomA.Knowledge = Knowledge.Scouted;

                    MapPanel.UpdateArea(hallSector.Hallway.RoomA);
                    if (HallwayView.TargetRoom == hallSector.Hallway.RoomA)
                        MapPanel.SetMovingRoom(hallSector.Hallway.RoomA);

                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_room");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                    yield return new WaitForSeconds(0.5f);

                if (currentScoutedRooms.ContainsKey(hallSector.Hallway.RoomA))
                {
                    if (currentScoutedRooms[hallSector.Hallway.RoomA] < tiles)
                        currentScoutedRooms[hallSector.Hallway.RoomA] = tiles;
                    else
                        tiles = 0;
                }
                else
                    currentScoutedRooms.Add(hallSector.Hallway.RoomA, tiles);

                if (tiles > 0)
                {
                    for (int i = 0; i < hallSector.Hallway.RoomA.Doors.Count; i++)
                    {
                        if (hallSector.Hallway.RoomA.Doors[i].TargetArea == hallSector.Hallway.Id)
                            continue;

                        StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[hallSector.Hallway.RoomA.Doors[i].TargetArea],
                            hallSector.Hallway.RoomA.Doors[i].Direction, tiles));
                        scoutingCounter++;
                    }
                }
            }
        }
        else if(direction == Direction.Top || direction == Direction.Right)
        {
            int hallIndex = hallSector.Hallway.Halls.IndexOf(hallSector);
            int tilesScouted = Mathf.Min(hallSector.Hallway.Halls.Count - hallSector.Hallway.Halls.IndexOf(hallSector) - 1, tiles);
            int lastHallIndex = hallIndex + tilesScouted;

            for (int i = hallIndex + 1; i <= lastHallIndex; i++)
            {
                if (hallSector.Hallway.Halls[i].Knowledge == Knowledge.Hidden)
                {
                    hallSector.Hallway.Halls[i].Knowledge = Knowledge.Scouted;

                    if (hallSector.Hallway.Halls[i].Type != AreaType.Door)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                        MapPanel.UpdateArea(hallSector.Hallway.Halls[i]);
                        if (hallSector.Hallway.Halls[i].Type == AreaType.Trap)
                        {
                            var raidSector = HallwayView.raidHallway.HallSectors.
                                Find(trapSector => trapSector.HallSector == hallSector.Hallway.Halls[i]);
                            if (raidSector != null)
                                (raidSector.Prop as RaidTrap).SkeletonAnimation.MeshRenderer.enabled = true;
                        }
                        yield return new WaitForSeconds(0.4f);
                    }
                }
                else if (hallSector.Hallway.Halls[i].Type != AreaType.Door)
                    yield return new WaitForSeconds(0.4f);
            }
            tiles -= tilesScouted;

            if (tiles >= 0)
            {
                if (hallSector.Hallway.RoomB.Knowledge == Knowledge.Hidden)
                {
                    hallSector.Hallway.RoomB.Knowledge = Knowledge.Scouted;

                    MapPanel.UpdateArea(hallSector.Hallway.RoomB);
                    if (HallwayView.TargetRoom == hallSector.Hallway.RoomB)
                        MapPanel.SetMovingRoom(hallSector.Hallway.RoomB);

                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_room");
                    yield return new WaitForSeconds(0.5f);
                }
                else
                    yield return new WaitForSeconds(0.5f);

                if (currentScoutedRooms.ContainsKey(hallSector.Hallway.RoomB))
                {
                    if (currentScoutedRooms[hallSector.Hallway.RoomB] < tiles)
                        currentScoutedRooms[hallSector.Hallway.RoomB] = tiles;
                    else
                        tiles = 0;
                }
                else
                    currentScoutedRooms.Add(hallSector.Hallway.RoomB, tiles);

                if (tiles > 0)
                {
                    for (int i = 0; i < hallSector.Hallway.RoomB.Doors.Count; i++)
                    {
                        if (hallSector.Hallway.RoomB.Doors[i].TargetArea == hallSector.Hallway.Id)
                            continue;

                        StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[hallSector.Hallway.RoomB.Doors[i].TargetArea],
                            hallSector.Hallway.RoomB.Doors[i].Direction, tiles));
                        scoutingCounter++;
                    }
                }
            }
        }
        scoutingCounter--;
    }
    IEnumerator ScoutingEvent(Room room)
    {
        currentScoutedRooms.Clear();

        float scoutingChance = 0.25f + TorchMeter.CurrentRange.ScoutingChance;
        for (int i = 0; i < Formations.heroes.party.Units.Count; i++)
            scoutingChance += Formations.heroes.party.Units[i].Character[AttributeType.ScoutingChance].ModifiedValue;

        int scoutingTiles = 0;
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
            
            scoutingCounter = 0;

            for (int i = 0; i < room.Doors.Count; i++)
            {
                StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[room.Doors[i].TargetArea],
                    room.Doors[i].Direction, scoutingTiles));
                scoutingCounter++;
            }

            while (scoutingCounter > 0)
                yield return null;

            RaidPanel.SwitchBlocked = false;
        }
        currentScoutedRooms.Clear();
        yield break;
    }
    IEnumerator CurioScoutingEvent(Area area, CurioResult result)
    {
        RaidPanel.LockOnMap();
        MapPanel.FocusTarget(1f);
        yield return new WaitForSeconds(0.6f);

        MapPanel.SetScoutingRadar();
        FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_start");
        yield return new WaitForSeconds(1f);

        if (result.Item.EndsWith("all"))
        {
            int scoutingTiles = 0;
            if (result.Item.StartsWith("0"))
            {
                scoutingTiles = 1000;
            }
            else
                scoutingTiles = (int.Parse(result.Item.Substring(0, 1)) - 1) * 6;

            if (scoutingTiles > 0)
            {
                if(area is HallSector)
                {
                    scoutingCounter = 2;

                    StartCoroutine(CurioScoutingHallway(area as HallSector, Direction.Left, scoutingTiles));
                    StartCoroutine(CurioScoutingHallway(area as HallSector, Direction.Right, scoutingTiles));
                }
                else
                {
                    scoutingCounter = 0;
                    var room = area as Room;

                    for (int i = 0; i < room.Doors.Count; i++)
                    {
                        StartCoroutine(ScoutingHallway(Raid.Dungeon.Hallways[room.Doors[i].TargetArea],
                            room.Doors[i].Direction, scoutingTiles));
                        scoutingCounter++;
                    }
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
                    if (hallwaySlot.Type == AreaType.Trap && 
                        (hallwaySlot.Knowledge == Knowledge.Hidden || hallwaySlot.Knowledge == Knowledge.Visited))
                    {
                        hallwaySlot.Knowledge = Knowledge.Scouted;

                        if (hallwaySlot.Type != AreaType.Door)
                        {
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                            MapPanel.UpdateArea(hallwaySlot);
                            if (SceneState == DungeonSceneState.Hall && HallwayView.Hallway == hallway)
                            {
                                var raidSector = HallwayView.raidHallway.HallSectors.
                                    Find(trapSector => trapSector.HallSector == hallwaySlot);
                                if (raidSector != null)
                                    (raidSector.Prop as RaidTrap).SkeletonAnimation.MeshRenderer.enabled = true;
                            }
                        }
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
                        hallwaySlot.Knowledge = Knowledge.Scouted;

                        if (hallwaySlot.Type != AreaType.Door)
                        {
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                            MapPanel.UpdateArea(hallwaySlot);
                        }
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
                        hallwaySlot.Knowledge = Knowledge.Scouted;

                        if (hallwaySlot.Type != AreaType.Door)
                        {
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                            MapPanel.UpdateArea(hallwaySlot);
                        }
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
                    room.Knowledge = Knowledge.Scouted;

                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_room");
                    MapPanel.UpdateArea(room);
                    
                    yield return new WaitForSeconds(0.6f);
                }
            }
        }
        else if (result.Item.EndsWith("curios"))
        {
            foreach (var room in Raid.Dungeon.Rooms.Values)
            {
                if ( (room.Type == AreaType.Curio || room.Type == AreaType.BattleCurio) && room.Knowledge == Knowledge.Hidden)
                {
                    room.Knowledge = Knowledge.Scouted;

                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_room");
                    MapPanel.UpdateArea(room);
                    yield return new WaitForSeconds(0.6f);
                }
            }
            foreach (var hallway in Raid.Dungeon.Hallways.Values)
            {
                foreach (var hallwaySlot in hallway.Halls)
                {
                    if (hallwaySlot.Type == AreaType.Curio && hallwaySlot.Knowledge == Knowledge.Hidden)
                    {
                        hallwaySlot.Knowledge = Knowledge.Scouted;

                        if (hallwaySlot.Type != AreaType.Door)
                        {
                            FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_hallway");
                            MapPanel.UpdateArea(hallwaySlot);
                        }
                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
        }

        RaidPanel.SwitchBlocked = false;
        currentScoutedRooms.Clear();
        yield break;
    }

    IEnumerator CompletionCrestEvent()
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
    IEnumerator CurioEvent(IRaidArea areaView, Quirk triggerQuirk = null, Trait triggerTrait = null)
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

        if(curio.IsQuestCurio)
        {
            Inventory.SetInteractionState(true);
            #region Interaction Event
            float announcementTimer = -1;
            RaidEvents.LoadInteraction(curio, areaView);
            while (true)
            {
                if (announcementTimer > 0)
                {
                    announcementTimer -= Time.deltaTime;
                    if (announcementTimer <= 0)
                    {
                        if (RaidEvents.announcment.animator.isInitialized)
                            RaidEvents.HideAnnouncment();
                        announcementTimer = -1;
                    }
                }

                if (RaidEvents.itemInteraction.ActionType == InteractionResultType.ItemInteraction)
                {
                    curioInteraction = curio.ItemInteractions.Find(itemInteraction =>
                        itemInteraction.ItemId == RaidEvents.itemInteraction.SelectedItem.Id);
                    if (curioInteraction == null)
                    {
                        RaidEvents.itemInteraction.ResetInteraction(curio);
                        RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_curio_item_had_no_effect"));
                        announcementTimer = 1;
                    }
                }

                if (RaidEvents.itemInteraction.ActionType == InteractionResultType.Waiting)
                    yield return null;
                else
                    break;
            }


            switch (RaidEvents.itemInteraction.ActionType)
            {
                case InteractionResultType.Cancel:
                    QuestPanel.EnableRetreat();
                    Formations.UnlockSelections();
                    RaidPanel.SetPeacefulState();
                    Inventory.SetPeacefulState(false);
                    EnablePartyMovement();
                    EnableEnviroment();
                    if (triggerQuirk != null || triggerTrait != null)
                        Formations.UnlockSelections();
                    currentEvent = null;
                    yield break;
                case InteractionResultType.ItemInteraction:
                    curioInteraction = curio.ItemInteractions.Find(itemInteraction =>
                    itemInteraction.ItemId == RaidEvents.itemInteraction.SelectedItem.Id);
                    if (curioInteraction == null)
                    {
                        QuestPanel.EnableRetreat();
                        Formations.UnlockSelections();
                        RaidPanel.SetPeacefulState();
                        Inventory.SetPeacefulState(false);
                        EnablePartyMovement();
                        EnableEnviroment();
                        currentEvent = null;
                        if (triggerQuirk != null || triggerTrait != null)
                            Formations.UnlockSelections();
                        yield break;
                    }
                    curioResult = RandomSolver.ChooseByRandom<CurioResult>(curioInteraction.Results);
                    break;
                case InteractionResultType.ManualInteraction:
                    curioInteraction = RandomSolver.ChooseByRandom<CurioInteraction>(curio.Results);
                    curioResult = RandomSolver.ChooseByRandom<CurioResult>(curioInteraction.Results);
                    break;
            }
            #endregion
        }
        else if (curio.IsFullCurio && triggerQuirk == null && triggerTrait == null)
        {
            Inventory.SetInteractionState(false);
            #region Interaction Event
            float announcementTimer = -1;
            RaidEvents.LoadInteraction(curio, areaView);
            while (true)
            {
                if (announcementTimer > 0)
                {
                    announcementTimer -= Time.deltaTime;
                    if (announcementTimer <= 0)
                    {
                        if (RaidEvents.announcment.animator.isInitialized)
                            RaidEvents.HideAnnouncment();
                        announcementTimer = -1;
                    }
                }

                if (RaidEvents.itemInteraction.ActionType == InteractionResultType.ItemInteraction)
                {
                    curioInteraction = curio.ItemInteractions.Find(itemInteraction =>
                        itemInteraction.ItemId == RaidEvents.itemInteraction.SelectedItem.Id);
                    if (curioInteraction == null)
                    {
                        RaidEvents.itemInteraction.ResetInteraction(curio);
                        RaidEvents.ShowAnnouncment(LocalizationManager.GetString("str_curio_item_had_no_effect"));
                        announcementTimer = 1;
                    }
                }

                if (RaidEvents.itemInteraction.ActionType == InteractionResultType.Waiting)
                    yield return null;
                else
                    break;
            }


            switch (RaidEvents.itemInteraction.ActionType)
            {
                case InteractionResultType.Cancel:
                    QuestPanel.EnableRetreat();
                    Formations.UnlockSelections();
                    RaidPanel.SetPeacefulState();
                    Inventory.SetPeacefulState(false);
                    EnablePartyMovement();
                    EnableEnviroment();
                    if (triggerQuirk != null || triggerTrait != null)
                        Formations.UnlockSelections();
                    currentEvent = null;
                    yield break;
                case InteractionResultType.ItemInteraction:
                    curioInteraction = curio.ItemInteractions.Find(itemInteraction =>
                    itemInteraction.ItemId == RaidEvents.itemInteraction.SelectedItem.Id);
                    if (curioInteraction == null)
                    {
                        QuestPanel.EnableRetreat();
                        Formations.UnlockSelections();
                        RaidPanel.SetPeacefulState();
                        Inventory.SetPeacefulState(false);
                        EnablePartyMovement();
                        EnableEnviroment();
                        if (triggerQuirk != null || triggerTrait != null)
                            Formations.UnlockSelections();
                        currentEvent = null;
                        yield break;
                    }
                    curioResult = RandomSolver.ChooseByRandom<CurioResult>(curioInteraction.Results);
                    break;
                case InteractionResultType.ManualInteraction:
                    curioInteraction = RandomSolver.ChooseByRandom<CurioInteraction>(curio.Results);
                    curioResult = RandomSolver.ChooseByRandom<CurioResult>(curioInteraction.Results);
                    break;
            }
            #endregion
        }
        else
        {
            curioInteraction = RandomSolver.ChooseByRandom<CurioInteraction>(curio.Results);
            curioResult = RandomSolver.ChooseByRandom<CurioResult>(curioInteraction.Results);
        }
        Inventory.SetDeactivated();

        var oneShotCurio = FMODUnity.RuntimeManager.CreateInstance("event:/props/curios/" + curio.OriginalId);
        if(curioInteraction is ItemInteraction)
        {
            oneShotCurio.setParameterValue("item_index", curio.ItemInteractions.IndexOf(curioInteraction as ItemInteraction) + 1);
            oneShotCurio.setParameterValue("result_category", 2);
        }
        else
        {
            oneShotCurio.setParameterValue("item_index", 0);
            if(curio.IsQuestCurio)
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
        dungeonCamera.TargetFOV = 50;

        GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/interaction_curio");
        if (animationObject != null)
        {
            AnimatedEffect animation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
            if (animation != null)
            {
                animation.skeletonAnimation.state.SetAnimation(0, "heroic", false);
                animation.BindToTarget(areaView.Prop.RectTransform, (areaView.Prop as RaidCurio).SkeletonAnimation, "root");
                animation.skeletonAnimation.MeshRenderer.sortingOrder = (areaView.Prop as RaidCurio).
                    SkeletonAnimation.MeshRenderer.sortingOrder - 1;
            }
        }
        yield return new WaitForSeconds(2f);
        dungeonCamera.TargetFOV = 60;
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
                        currentEvent = EncounterEvent(areaView);
                        StartCoroutine(currentEvent);
                        yield break;
                    }
                }
                #endregion
                break;
            case "scouting":
                #region Scouting
                yield return StartCoroutine(CurioScoutingEvent(areaView.Area, curioResult));
                #endregion
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
                if (RaidEvents.loot.partyInventory.HasSomething())
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
                    Quirk newQuirk = null;
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
        if (Formations.heroes.party.Units.Count > 1 && Formations.heroes.party.Units.Contains(interactorUnit))
        {
            foreach (var heroBarker in Formations.heroes.party.Units)
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
        currentEvent = null;
    }
    IEnumerator TrapEvent(RaidHallSector sector, bool handActivation)
    {
        QuestPanel.DisableRetreat(false);
        DisablePartyMovement();
        RaidPanel.bannerPanel.SetDisabledState();
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
            disarmChance -= currentRaid.Quest.Difficulty / 2 * 0.2f;

            if (RandomSolver.CheckSuccess(disarmChance))
                isDisarmed = true;
        }
        dungeonCamera.TargetFOV = 50;
        yield return new WaitForSeconds(0.10f);
        raidTrap.SkeletonAnimation.MeshRenderer.enabled = true;
        Formations.InvestigateTrapIntro(raidTrap, isDisarmed);
        Formations.heroesAttackMeleePosition.SetUnitTarget(trapTarget, 0.05f, new Vector2(200, 0));
        Formations.monstersAttackMeleePosition.SetTrap(raidTrap, 0.05f, new Vector2(-200, -60));

        if (isDisarmed)
        {
            #region Animation
            GameObject animationObject = Resources.Load<GameObject>("Prefabs/Effects/interaction_curio");
            if (animationObject != null)
            {
                AnimatedEffect animation = Instantiate(animationObject).GetComponent<AnimatedEffect>();
                if (animation != null)
                {
                    animation.skeletonAnimation.MeshRenderer.sortingOrder = raidTrap.SkeletonAnimation.MeshRenderer.sortingOrder - 1;
                    animation.skeletonAnimation.state.SetAnimation(0, "heroic", false);
                    animation.BindToTarget(raidTrap.RectTransform, raidTrap.SkeletonAnimation, "root");
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

            if(currentRaid.Quest.Difficulty == 1)
            {
                foreach(var succesEffect in trap.SuccessEffects)
                    succesEffect.Apply(null, trapTarget, BattleSolver.SkillResult);
            }
            else
            {
                TrapVariation variation;
                if(trap.Variations.ContainsKey(currentRaid.Quest.Difficulty))
                    variation = trap.Variations[currentRaid.Quest.Difficulty];
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
                if (currentRaid.Quest.Difficulty == 1)
                {
                    int damage = Mathf.RoundToInt(Mathf.Abs(trap.HealthPenalty) * trapTarget.Character.Health.ModifiedValue);
                    trapTarget.Character.Health.DecreaseValue(damage);
                    RaidEvents.ShowPopupMessage(trapTarget, PopupMessageType.Damage, damage.ToString());

                    foreach (var failEffect in trap.FailEffects)
                        failEffect.Apply(null, trapTarget, BattleSolver.SkillResult);
                }
                else
                {
                    TrapVariation variation;
                    if (trap.Variations.ContainsKey(currentRaid.Quest.Difficulty))
                        variation = trap.Variations[currentRaid.Quest.Difficulty];
                    else
                        variation = trap.Variations[5];

                    int damage = Mathf.RoundToInt(Mathf.Abs(variation.HealthPenalty) * trapTarget.Character.Health.ModifiedValue);
                    trapTarget.Character.Health.DecreaseValue(damage);
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
        dungeonCamera.TargetFOV = 60;
        Formations.InvestigateTrapOutro(sector, isDisarmed);
        yield return new WaitForSeconds(0.10f);
        raidTrap.SetSortingOrder(3);
        yield return new WaitForSeconds(0.35f);
        Formations.ShowHeroOverlay();
        raidTrap.gameObject.SetActive(false);

        yield return StartCoroutine(ExecuteEffectEvents(false));

        if(Formations.heroes.party.Units.Contains(trapTarget))
            trapTarget.OverlaySlot.UpdateOverlay();

        #region Check Game Over
        if (Formations.heroes.party.Units.Count == 0)
        {
            StartCoroutine(RaidResultsEvent());
            yield break;
        }
        #endregion

        #region Comment on Trigger
        if (isDisarmed == false && Formations.heroes.party.Units.Count > 1 &&
            Formations.heroes.party.Units.Contains(trapTarget))
        {
            foreach (var heroBarker in Formations.heroes.party.Units)
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
        RaidPanel.bannerPanel.SetPeacefulState();
        Inventory.SetPeacefulState(false);
        EnablePartyMovement();
        currentEvent = null;
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
        yield break;
    }
    IEnumerator ObstacleEvent(RaidHallSector sector, bool handActivation)
    {
        QuestPanel.DisableRetreat(false);
        DisablePartyMovement();
        DisableEnviroment();
        RaidPanel.SetDisabledState();
        Inventory.SetDeactivated();
        Formations.LockSelections();

        while (IsUnitEventInProgress)
            yield return null;

        sector.CompleteArea();
        yield return new WaitForEndOfFrame();
        RaidObstacle raidObstacle = sector.Prop as RaidObstacle;
        Obstacle obstacle = sector.Area.Prop as Obstacle;

        float timeWasted = 0;
        if(handActivation)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/props/obstacles/" + obstacle.StringId + "_by_hand");
            if (obstacle.TorchlightPenalty < 0)
                TorchMeter.DecreaseTorch(Mathf.Abs(Mathf.RoundToInt(obstacle.TorchlightPenalty)));

            if(obstacle.HealthPenalty != 0)
            {
                foreach (var heroUnit in Formations.heroes.party.Units)
                {
                    int damage = Mathf.RoundToInt(Mathf.Abs(obstacle.HealthPenalty) * heroUnit.Character.Health.ModifiedValue);
                    heroUnit.Character.Health.DecreaseValue(damage);
                    RaidEvents.ShowPopupMessage(heroUnit, PopupMessageType.Damage, damage.ToString());
                    heroUnit.OverlaySlot.UpdateOverlay();
                }
                yield return new WaitForSeconds(0.6f);
                timeWasted += 0.6f;
            }

            BattleSolver.SkillResult.Reset();

            if(obstacle.FailEffects.Count > 0)
            {
                foreach (var heroUnit in Formations.heroes.party.Units)
                {
                    BattleSolver.SkillResult.AddResultEntry(new SkillResultEntry(heroUnit, SkillResultType.Hit));
                    foreach (var failEffect in obstacle.FailEffects)
                        failEffect.Apply(null, heroUnit, BattleSolver.SkillResult);
                }
            }
            
            yield return StartCoroutine(ExecuteEffectEvents(false));

            #region Check Game Over
            if (Formations.heroes.party.Units.Count == 0)
            {
                StartCoroutine(RaidResultsEvent());
                yield break;
            }
            #endregion
        }
        else
            FMODUnity.RuntimeManager.PlayOneShot("event:/props/obstacles/" + obstacle.StringId);

        if (raidObstacle.SkeletonAnimation.state.GetCurrent(0) != null)
            yield return new WaitForSeconds(raidObstacle.SkeletonAnimation.state.GetCurrent(0).EndTime - timeWasted);

        QuestPanel.EnableRetreat();
        Inventory.SetPeacefulState(false);
        Formations.UnlockSelections();
        RaidPanel.SetPeacefulState();
        EnableEnviroment();
        EnableMovement();

        currentEvent = null;
        DarkestDungeonManager.SaveData.UpdateFromRaid();
        DarkestDungeonManager.Instanse.SaveGame();
    }

    public void SummonPurging(FormationUnit targetUnit)
    {
        targetUnit.SetSortingOrder(4);
        PrepareDeath(targetUnit);
        unitEventQueue.RemoveAll(item => item == targetUnit);
        BattleGround.UnitDestroyed(targetUnit);
        Formations.monsters.DeleteUnitDelayed(targetUnit, 1.867f);
    }
    bool PrepareDeath(FormationUnit targetUnit)
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

            if(BattleGround.SharedHealth.IsActive)
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
                if(captureRecord != null)
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
                return true;
            }
            else
            {
                deathDoorEnterQueue.Add(targetUnit);
                return false;
            }
        }
    }
    void ExecuteDeath(FormationUnit targetUnit)
    {
        if (targetUnit.CombatInfo.IsDead)
        {
            if (targetUnit.Character.IsMonster)
            {
                if (RaidEvents.MonsterTooltip.Slot == targetUnit.OverlaySlot)
                    RaidEvents.MonsterTooltip.Hide();
                Monster monster = targetUnit.Character as Monster;

                if(monster.SkillReaction != null && monster.SkillReaction.WasKilledOtherMonstersEffects.Count > 0)
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
                if(companionRecord != null)
                {
                    BattleGround.Companions.Remove(companionRecord);

                    if(companionRecord.CompanionUnit == targetUnit)
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
                    if(captureRecord != null)
                        BattleGround.ReleaseUnit(captureRecord);

                    if(monster.Data.LifeLink != null && BattleGround.IsLifeLinked(targetUnit, monster.Data.LifeLink))
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
                    if(monster.Data.DeathClass.Type == DeathClassType.Corpse)
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

                var heroInfo = currentRaid.RaidParty.HeroInfo.Find(info => info.Hero == targetUnit.Character as Hero);
                heroInfo.IsAlive = false;
                heroInfo.DeathRecord = new DeathRecord()
                {
                    HeroClassIndex = heroInfo.Hero.ClassIndexId,
                    HeroName = heroInfo.Hero.Name,
                    KillerName = "necromancer_A",
                    ResolveLevel = heroInfo.Hero.Resolve.Level,
                    Factor = DeathFactor.AttackMonster,
                };
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
                for(int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
                    DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(BattleGround.HeroParty.Units[i]);
            }
        }
    }

    public void AddResolveCheck(FormationUnit unit)
    {
        if (!resolveCheckQueue.Contains(unit))
            resolveCheckQueue.Add(unit);
    }
    public void AddHeartAttackCheck(FormationUnit unit)
    {
        if (!heartAttackCheckQueue.Contains(unit))
            heartAttackCheckQueue.Add(unit);
    }
    #endregion

    #region Restrictions
    public void DisablePartyMovement()
    {
        partyController.MovementAllowed = false;
    }
    public void EnablePartyMovement()
    {
        partyController.MovementAllowed = true;
    }

    public void DisableForwardPartyMovement()
    {
        partyController.ForwardMovementAllowed = false;
    }
    public void EnableForwardPartyMovement()
    {
        partyController.ForwardMovementAllowed = true;
    }

    public void EnableMovement()
    {
        partyController.MovementAllowed = true;
        partyController.ForwardMovementAllowed = true;
    }

    public void DisableEnviroment()
    {
        RoomView.DisableInteraction();
        HallwayView.DisableInteraction();
    }
    public void EnableEnviroment()
    {
        RoomView.EnableInteraction();
        HallwayView.EnableInteraction();
    }
    #endregion
}