using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public enum Team { Heroes, Monsters }
public enum TurnType { HeroTurn, MonsterTurn }
public enum RoundStatus { Start, Progress, Finish }
public enum TurnStatus { PreTurn, Progress, PostTurn }
public enum BattleStatus { Peace, Fighting, Finished }
public enum SurpriseStatus { Nothing, MonstersSurprised, HeroesSurprised }

public class BattleGround : MonoBehaviour
{
    [SerializeField]
    private BattleFormation heroFormation;
    [SerializeField]
    private BattleFormation monsterFormation;
    [SerializeField]
    private SharedHealthInfo sharedHealthRecord;
    [SerializeField]
    private RectTransform heroPosition;
    [SerializeField]
    private RectTransform monsterPosition;
    [SerializeField]
    private RectTransform cameraFocus;
    [SerializeField]
    private Backdrop backdrop;

    public BattleFormation HeroFormation { get { return heroFormation; } }
    public BattleFormation MonsterFormation { get { return monsterFormation; } }
    public FormationParty HeroParty { get { return heroFormation.Party; } }
    public FormationParty MonsterParty { get { return monsterFormation.Party; } }
    public SharedHealthInfo SharedHealth { get { return sharedHealthRecord; } }
    public RectTransform Rect { get; set; }

    public string LastSkillUsed { get; set; }
    public int StallingRoundNumber { get; set; }
    public BattleStatus BattleStatus { get; set; }

    public SurpriseStatus SurpriseStatus { get; private set; }
    public RoundStatus RoundStatus { get { return Round.RoundStatus; } }
    public TurnStatus TurnStatus { get { return Round.TurnStatus; } }
    public Round Round { get; private set; }

    public List<int> CombatIds { get; private set; }
    public List<CompanionRecord> Companions { get; private set; }
    public List<CaptureRecord> Captures { get; private set; }
    public List<ControlRecord> Controls { get; private set; }
    public List<LootDefinition> BattleLoot { get; private set; }
    public List<string> LastDamaged { get; private set; }

    public int HeroNumber
    {
        get
        {
            return HeroParty.Units.Count;
        }
    }

    public int MarkedHeroes
    {
        get
        {
            return HeroParty.Units.FindAll(unit => unit.Character.GetStatusEffect(StatusType.Marked).IsApplied).Count;
        }
    }

    public int VirtuedHeroes
    {
        get
        {
            return HeroParty.Units.FindAll(unit => unit.Character.IsVirtued).Count;
        }
    }

    public int NonVirtuedHeroes
    {
        get
        {
            return HeroParty.Units.FindAll(unit => unit.Character.IsVirtued == false).Count;
        }
    }

    public int NonDeathsDoorHeroes
    {
        get
        {
            return HeroParty.Units.FindAll(unit => unit.Character.AtDeathsDoor == false).Count;
        }
    }

    public int ControlCount
    {
        get
        {
            return Controls.Count;
        }
    }

    public int MonsterNumber
    {
        get
        {
            return MonsterParty.Units.FindAll(unit => unit.Character.IsMonster
                && !((Monster)unit.Character).Types.Contains(MonsterType.Corpse)).Count;
        }
    }

    public int GuardedMonsters
    {
        get
        {
            return MonsterParty.Units.FindAll(unit => unit.Character.GetStatusEffect(StatusType.Guarded).IsApplied).Count;
        }
    }

    public int MonsterSize
    {
        get
        {
            return MonsterParty.Units.FindAll(unit => unit.Character.IsMonster
                && !((Monster)unit.Character).Types.Contains(MonsterType.Corpse)).Sum(unit => unit.Size);
        }
    }

    private readonly List<FormationUnit> loadedRemovedPrisoners = new List<FormationUnit>();

    private void Awake()
    {
        BattleLoot = new List<LootDefinition>();
        Companions = new List<CompanionRecord>(4);
        Captures = new List<CaptureRecord>(4);
        Controls = new List<ControlRecord>(4);
        LastDamaged = new List<string>(4);
        Rect = GetComponent<RectTransform>();
        Round = new Round();
        CombatIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
    }

    public bool IsBattleEnded()
    {
        if (Controls.Count != 0 || Captures.Count != 0)
            return false;

        if (HeroFormation.AliveUnitsCount == 0 || MonsterFormation.AliveUnitsCount == 0 || BattleStatus == BattleStatus.Finished)
        {
            BattleStatus = BattleStatus.Finished;
            return true;
        }
        return false;
    }

    public bool IsBattleOnesided()
    {
        return HeroFormation.AliveUnitsCount == 0 || MonsterFormation.AliveUnitsCount == 0;
    }

    #region Initialization

    public void InitiateBattle()
    {
        BattleLoot.Clear();
        StallingRoundNumber = 0;
        BattleStatus = BattleStatus.Fighting;
        Round.HeroAction = HeroTurnAction.Waiting;
        Rect.SetParent(HeroFormation.RectTransform.parent, false);

        HeroFormation.RectTransform.SetParent(Rect, true);
        cameraFocus.position = new Vector3(HeroFormation.Ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x) / 2, 0, 0);
        RaidSceneManager.DungeonCamera.Target = cameraFocus;

        for (int i = 0; i < HeroParty.Units.Count; i++)
            HeroParty.Units[i].CombatInfo.PrepareForBattle(PickId());

        MonsterFormation.gameObject.SetActive(true);
        MonsterFormation.RectTransform.SetParent(Rect, false);
        MonsterFormation.RectTransform.position = new Vector3(HeroFormation.Ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x),
            HeroFormation.RectTransform.position.y, HeroFormation.RectTransform.position.z);
        MonsterFormation.Ranks.InstantRelocation();

        MonsterFormation.RectTransform.SetAsLastSibling();
        HeroFormation.RectTransform.SetAsLastSibling();
    }

    public void LoadBattle(BattleGroundSaveData battleSaveData)
    {
        InitiateSavedBattle();
        LoadEncounter(battleSaveData.MonsterFormation);

        #region Load Guard Statuses
        for (int i = 0; i < HeroParty.Units.Count; i++)
        {
            var heroGuardedStatus = HeroParty.Units[i].Character[StatusType.Guarded] as GuardedStatusEffect;
            if (heroGuardedStatus.GuardDuration > 0)
            {
                var guardUnit = FindById(heroGuardedStatus.GuardCombatId);
                if (guardUnit != null)
                {
                    heroGuardedStatus.Guard = guardUnit;
                    var guardStatus = guardUnit.Character[StatusType.Guard] as GuardStatusEffect;
                    guardStatus.Targets.Add(HeroParty.Units[i]);
                }
            }
        }
        for (int i = 0; i < MonsterParty.Units.Count; i++)
        {
            var monsterGuardedStatus = MonsterParty.Units[i].Character[StatusType.Guarded] as GuardedStatusEffect;
            if (monsterGuardedStatus.GuardDuration > 0)
            {
                var guardUnit = FindById(monsterGuardedStatus.GuardCombatId);
                if (guardUnit != null)
                {
                    monsterGuardedStatus.Guard = guardUnit;
                    var guardStatus = guardUnit.Character[StatusType.Guard] as GuardStatusEffect;
                    guardStatus.Targets.Add(MonsterParty.Units[i]);
                }
            }
        }
        #endregion

        #region Load Battle Info
        Round.RoundNumber = battleSaveData.RoundNumber;
        Round.RoundStatus = battleSaveData.RoundStatus;
        Round.TurnType = battleSaveData.TurnType;
        Round.TurnStatus = battleSaveData.TurnStatus;
        Round.HeroAction = battleSaveData.HeroAction;
        BattleStatus = battleSaveData.BattleStatus;
        SurpriseStatus = battleSaveData.SurpriseStatus;
        StallingRoundNumber = battleSaveData.StallingRoundNumber;

        Round.SelectedUnit = FindById(battleSaveData.SelectedUnitId);
        Round.SelectedTarget = FindById(battleSaveData.SelectedTargetId);
        LastSkillUsed = battleSaveData.LastSkillUsed;

        Round.OrderedUnits.Clear();
        for (int i = 0; i < battleSaveData.OrderedUnitsCombatIds.Count; i++)
        {
            var newOrderedUnit = FindById(battleSaveData.OrderedUnitsCombatIds[i]);
            if (newOrderedUnit != null)
                Round.OrderedUnits.Add(newOrderedUnit);
        }

        CombatIds = battleSaveData.CombatIds;
        LastDamaged = battleSaveData.LastDamaged;
        BattleLoot = battleSaveData.BattleLoot;
        #endregion

        #region Load Removed Captured Units
        for (int i = 0; i < battleSaveData.RemovedUnits.Count; i++)
        {
            var prisonerSaveData = battleSaveData.RemovedUnits[i];

            if (prisonerSaveData.IsHero)
            {
                Hero hero = DarkestDungeonManager.Campaign.Heroes.Find(estateHero =>
                    estateHero.RosterId == prisonerSaveData.RosterId);
                FormationUnit unit = Instantiate(Resources.Load<GameObject>("Prefabs/Heroes/" +
                    hero.Class)).GetComponent<FormationUnit>();
                unit.transform.SetParent(HeroParty.transform, false);
                unit.Party = HeroParty;
                unit.Formation = HeroFormation;
                unit.Initialize(hero, prisonerSaveData);
                unit.ResetAnimations();

                if (hero.HeroClass.Modes.Count > 0)
                {
                    var currentMode = hero.HeroClass.Modes.Find(mode => mode.Id == prisonerSaveData.CurrentMode);
                    if (currentMode != null)
                        hero.CurrentMode = currentMode;
                    else
                        hero.CurrentMode = hero.HeroClass.Modes.Find(mode => mode.IsRaidDefault);
                }
                loadedRemovedPrisoners.Add(unit);
                unit.gameObject.SetActive(false);
            }
            else
            {
                GameObject unitObject = Resources.Load("Prefabs/Monsters/" + prisonerSaveData.Class) as GameObject;
                FormationUnit unit = Instantiate(unitObject).GetComponent<FormationUnit>();
                unit.transform.SetParent(HeroParty.transform, false);
                unit.Party = HeroParty;
                unit.Formation = HeroFormation;
                var monsterSave = new Monster(prisonerSaveData);
                unit.Initialize(monsterSave, prisonerSaveData);
                unit.ResetAnimations();
                loadedRemovedPrisoners.Add(unit);
                unit.gameObject.SetActive(false);
            }
        }
        #endregion
    }

    public void LoadEffects(BattleGroundSaveData battleSaveData)
    {
        #region Load Statuses and Immobilize
        for (int i = 0; i < HeroParty.Units.Count; i++)
        {
            if (HeroParty.Units[i].Character[StatusType.Stun].IsApplied)
                HeroParty.Units[i].SetHalo("stunned");
            else if (HeroParty.Units[i].CombatInfo.IsSurprised)
                HeroParty.Units[i].SetHalo("surprised");

            if (HeroParty.Units[i].CombatInfo.IsImmobilized)
                HeroParty.Units[i].SetDefendAnimation(true);

            if (HeroParty.Units[i].Character.IsMonster == false)
            {
                var hero = HeroParty.Units[i].Character as Hero;
                if (hero[StatusType.DeathsDoor].IsApplied)
                    hero.ApplyDeathDoor();
                else if (hero[StatusType.DeathRecovery].IsApplied)
                    hero.ApplyMortality();
            }
        }
        for (int i = 0; i < MonsterParty.Units.Count; i++)
        {
            if (MonsterParty.Units[i].Character[StatusType.Stun].IsApplied)
                MonsterParty.Units[i].SetHalo("stunned");
            else if (MonsterParty.Units[i].CombatInfo.IsSurprised)
                MonsterParty.Units[i].SetHalo("surprised");

            if (MonsterParty.Units[i].CombatInfo.IsImmobilized)
                MonsterParty.Units[i].SetDefendAnimation(true);

            if (MonsterParty.Units[i].Character.IsMonster == false)
            {
                MonsterParty.Units[i].SetCombatAnimation(true);
                var hero = MonsterParty.Units[i].Character as Hero;
                if (hero[StatusType.DeathsDoor].IsApplied)
                    hero.ApplyDeathDoor();
                else if (hero[StatusType.DeathRecovery].IsApplied)
                    hero.ApplyMortality();
            }
        }
        #endregion

        #region Load Captures
        for (int i = 0; i < battleSaveData.Captures.Count; i++)
        {
            CaptureRecord newCaptureRecord = new CaptureRecord();
            int prisonerId = newCaptureRecord.GetHashPrisonerId(battleSaveData.Captures[i]);
            int captorId = newCaptureRecord.GetHashCaptorId(battleSaveData.Captures[i]);
            newCaptureRecord.RemoveFromParty = newCaptureRecord.GetHashRemoveFromParty(battleSaveData.Captures[i]);

            if (newCaptureRecord.RemoveFromParty == false)
            {
                FormationUnit prisoner = FindById(prisonerId);
                FormationUnit captor = FindById(captorId);
                newCaptureRecord.PrisonerUnit = prisoner;
                newCaptureRecord.CaptorUnit = captor;
                prisoner.SetCaptureEffect(captor);
                Captures.Add(newCaptureRecord);
            }
            else
            {
                FormationUnit prisoner = loadedRemovedPrisoners.Find(removedUnit => removedUnit.CombatInfo.CombatId == prisonerId);
                FormationUnit captor = FindById(captorId);
                prisoner.RectTransform.position = captor.RectTransform.position;
                newCaptureRecord.PrisonerUnit = prisoner;
                newCaptureRecord.CaptorUnit = captor;
                Captures.Add(newCaptureRecord);
            }
        }
        #endregion

        #region Load Companions
        for (int i = 0; i < battleSaveData.Companions.Count; i++)
        {
            CompanionRecord newCompanionRecord = new CompanionRecord();
            int companionId = newCompanionRecord.GetHashCompanionId(battleSaveData.Companions[i]);
            int targetId = newCompanionRecord.GetHashTargetId(battleSaveData.Companions[i]);

            FormationUnit companion = FindById(companionId);
            FormationUnit target = FindById(targetId);
            newCompanionRecord.CompanionUnit = companion;
            newCompanionRecord.TargetUnit = target;
            Companions.Add(newCompanionRecord);
        }
        #endregion

        #region Load Controls
        for (int i = 0; i < battleSaveData.Controls.Count; i++)
        {
            ControlRecord newControlRecord = new ControlRecord();
            int prisonerId = newControlRecord.GetHashPrisonerId(battleSaveData.Controls[i]);
            int controllerId = newControlRecord.GetHashControlId(battleSaveData.Controls[i]);
            newControlRecord.DurationLeft = newControlRecord.GetHashDurationLeft(battleSaveData.Controls[i]);
            FormationUnit prisoner = FindById(prisonerId);
            FormationUnit controller = FindById(controllerId);
            newControlRecord.PrisonerUnit = prisoner;
            newControlRecord.ControllUnit = controller;
            Controls.Add(newControlRecord);
        }
        #endregion

        loadedRemovedPrisoners.Clear();

        RaidSceneManager.Formations.HeroOverlay.UpdateOverlay();
        RaidSceneManager.Formations.Monsters.Overlay.UpdateOverlay();
    }

    public void SpawnEncounter(BattleEncounter encounter, bool campfireSurprise)
    {
        MonsterFormation.LoadParty(encounter);
        for (int i = 0; i < MonsterParty.Units.Count; i++)
        {
            var monster = MonsterParty.Units[i].Character as Monster;
            MonsterParty.Units[i].CombatInfo.PrepareForBattle(PickId(), monster, true);
            if (monster.TorchlightModifier != null)
                RaidSceneManager.TorchMeter.Modify(monster.TorchlightModifier);
            RaidSceneManager.TorchMeter.ApplyBuffsForUnit(MonsterParty.Units[i]);
            if (monster.Data.BattleBackdrop != null)
                backdrop.Activate(monster.Data.BattleBackdrop);
            if (monster.Data.Spawn != null)
            {
                for (int k = 0; k < monster.Data.Spawn.Effects.Count; k++)
                    for (int j = 0; j < monster.Data.Spawn.Effects[k].SubEffects.Count; j++)
                        monster.Data.Spawn.Effects[k].SubEffects[j].ApplyInstant(MonsterParty.Units[i],
                            MonsterParty.Units[i], monster.Data.Spawn.Effects[k]);
                MonsterParty.Units[i].OverlaySlot.UpdateOverlay();
            }
        }
        var shared = MonsterParty.Units.Find(unit => unit.Character.SharedHealth != null);
        if (shared != null)
            SharedHealth.Initialize(MonsterParty.Units, shared.Character.SharedHealth);
        MonsterFormation.Overlay.UpdateOverlay();

        SurpriseStatus = SurpriseStatus.Nothing;

        if (MonsterFormation.AlwaysSurprises || campfireSurprise)
            SurpriseStatus = SurpriseStatus.HeroesSurprised;
        else if (MonsterFormation.AlwaysBeSurprised)
            SurpriseStatus = SurpriseStatus.MonstersSurprised;
        else
        {
            if (MonsterFormation.CanBeSurprised)
            {
                float monstersSurprised = 0.1f + RaidSceneManager.TorchMeter.CurrentRange.MonstersSurprised;
                for (int i = 0; i < HeroParty.Units.Count; i++)
                    monstersSurprised += HeroParty.Units[i].Character[AttributeType.MonsterSurpirseChance].ModifiedValue;
                monstersSurprised = Mathf.Clamp(monstersSurprised, 0, 0.65f);
                if (RandomSolver.CheckSuccess(monstersSurprised))
                    SurpriseStatus = SurpriseStatus.MonstersSurprised;
            }

            if (MonsterFormation.CanSurprise && SurpriseStatus == SurpriseStatus.Nothing)
            {
                float heroesSurprised = 0.1f + RaidSceneManager.TorchMeter.CurrentRange.HeroesSurprised;
                for (int i = 0; i < HeroParty.Units.Count; i++)
                    heroesSurprised += HeroParty.Units[i].Character[AttributeType.PartySurpriseChance].ModifiedValue;
                heroesSurprised = Mathf.Clamp(heroesSurprised, 0, 0.65f);
                if (RandomSolver.CheckSuccess(heroesSurprised))
                    SurpriseStatus = SurpriseStatus.HeroesSurprised;
            }
        }
        Round.StartBattle(this);
    }

    public void SpawnMultiplayerEncounter(PhotonPlayer invader)
    {
        MonsterFormation.LoadParty(new RaidParty(invader), invader);
        for (int i = 0; i < MonsterParty.Units.Count; i++)
        {
            MonsterParty.Units[i].CombatInfo.PrepareForBattle(PickId());
            RaidSceneManager.TorchMeter.ApplyBuffsForUnit(MonsterParty.Units[i]);
        }
        
        MonsterFormation.Overlay.UpdateOverlay();

        SurpriseStatus = SurpriseStatus.Nothing;
        
        Round.StartBattle(this);
    }

    public void FinishBattle()
    {
        StallingRoundNumber = 0;
        var darkenessLoot = DarkestDungeonManager.Data.LootDatabase.DarknessLoot["battle"];

        for (int i = darkenessLoot.Count - 1; i >= 0; i--)
        {
            if (darkenessLoot[i].DarknessLevel == RaidSceneManager.TorchMeter.CurrentRange.Min)
            {
                if (RandomSolver.CheckSuccess(darkenessLoot[i].Chance))
                {
                    for (int j = 0; j < darkenessLoot[i].Codes.Count; j++)
                        BattleLoot.Add(new LootDefinition() { Code = darkenessLoot[i].Codes[j], Count = 1 });
                }
                break;
            }
        }
        for (int i = 0; i < HeroParty.Units.Count; i++)
        {
            var hero = (Hero)HeroParty.Units[i].Character;
            if (hero.HeroClass.ExtraBattleLoot != null)
                BattleLoot.Add(hero.HeroClass.ExtraBattleLoot);
        }

        HeroFormation.RectTransform.SetParent(Rect.parent, false);
        RaidSceneManager.DungeonCamera.Target = HeroFormation.Ranks.RectTransform;
        MonsterFormation.Overlay.ResetOverlay();
        MonsterParty.DeleteFormation();
        MonsterFormation.gameObject.SetActive(false);
        ResetIds();
        backdrop.Deactivate();
        Round.OrderedUnits.Clear();
        RaidSceneManager.TorchMeter.ClearModifier();

        if (SharedHealth.IsActive)
            SharedHealth.Reset();
    }

    public void LeaveBattleGround()
    {
        BattleStatus = BattleStatus.Peace;
    }

    public void RetreatFromBattle()
    {
        for (int i = 0; i < Controls.Count; i++)
        {
            if (Controls[i].PrisonerUnit.Character.IsMonster == false)
            {
                var heroInfo = RaidSceneManager.Raid.RaidParty.HeroInfo.Find(info =>
                    info.Hero == Controls[i].PrisonerUnit.Character as Hero);
                heroInfo.IsAlive = false;
                heroInfo.DeathRecord = new DeathRecord()
                {
                    HeroClassIndex = heroInfo.Hero.ClassIndexId,
                    HeroName = heroInfo.Hero.Name,
                    KillerName = Controls[i].ControllUnit.Character.Name,
                    ResolveLevel = heroInfo.Hero.Resolve.Level,
                    Factor = DeathFactor.AttackMonster,
                };
                Controls[i].PrisonerUnit.Formation.DeleteUnit(Controls[i].PrisonerUnit);
                if (RaidSceneManager.RaidPanel.SelectedUnit == Controls[i].PrisonerUnit)
                {
                    if (RaidSceneManager.Formations.Heroes.Party.Units.Count > 0)
                        RaidSceneManager.Formations.Heroes.Party.Units[0].OverlaySlot.UnitSelected();
                }
            }
        }
        Controls.Clear();

        for (int i = 0; i < Captures.Count; i++)
        {
            if (Captures[i].PrisonerUnit.Character.IsMonster == false)
            {
                var heroInfo = RaidSceneManager.Raid.RaidParty.HeroInfo.Find(info =>
                    info.Hero == Captures[i].PrisonerUnit.Character as Hero);
                heroInfo.IsAlive = false;
                heroInfo.DeathRecord = new DeathRecord()
                {
                    HeroClassIndex = heroInfo.Hero.ClassIndexId,
                    HeroName = heroInfo.Hero.Name,
                    KillerName = Captures[i].CaptorUnit.Character.Name,
                    ResolveLevel = heroInfo.Hero.Resolve.Level,
                    Factor = DeathFactor.CaptorMonster,
                };
                Captures[i].PrisonerUnit.Formation.DeleteUnit(Captures[i].PrisonerUnit);
                if (RaidSceneManager.RaidPanel.SelectedUnit == Captures[i].PrisonerUnit)
                {
                    if (RaidSceneManager.Formations.Heroes.Party.Units.Count > 0)
                        RaidSceneManager.Formations.Heroes.Party.Units[0].OverlaySlot.UnitSelected();
                }
            }
        }
        Captures.Clear();
        Companions.Clear();

        FinishBattle();
        Round.HeroAction = HeroTurnAction.Retreat;
    }

    private void InitiateSavedBattle()
    {
        Rect.SetParent(HeroFormation.RectTransform.parent, false);

        HeroFormation.RectTransform.SetParent(Rect, true);
        cameraFocus.position = new Vector3(HeroFormation.Ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x) / 2, 0, 0);
        RaidSceneManager.DungeonCamera.Target = cameraFocus;

        MonsterFormation.gameObject.SetActive(true);
        MonsterFormation.RectTransform.SetParent(Rect, false);
        MonsterFormation.RectTransform.position = new Vector3(HeroFormation.Ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x),
            HeroFormation.RectTransform.position.y, HeroFormation.RectTransform.position.z);
        MonsterFormation.Ranks.InstantRelocation();

        MonsterFormation.RectTransform.SetAsLastSibling();
        HeroFormation.RectTransform.SetAsLastSibling();
    }

    private void LoadEncounter(BattleFormationSaveData encounterSaveData)
    {
        MonsterFormation.LoadParty(encounterSaveData, false);
        for (int i = 0; i < MonsterParty.Units.Count; i++)
        {
            RaidSceneManager.TorchMeter.ApplyBuffsForUnit(MonsterParty.Units[i]);
            if (MonsterParty.Units[i].Character.IsMonster)
            {
                if (((Monster)MonsterParty.Units[i].Character).Data.BattleBackdrop != null)
                    backdrop.Activate(((Monster)MonsterParty.Units[i].Character).Data.BattleBackdrop);
            }
        }
        var shared = MonsterParty.Units.Find(unit => unit.Character.SharedHealth != null);
        if (shared != null)
            SharedHealth.Initialize(MonsterParty.Units, shared.Character.SharedHealth);
        MonsterFormation.Overlay.UpdateOverlay();
    }

    #endregion

    #region Battle Effect Functions

    public void CaptureUnit(FormationUnit prisoner, FormationUnit captor, bool removeFromParty)
    {
        RaidSceneManager.BattleGround.Captures.Add(new CaptureRecord(prisoner, captor, removeFromParty));
        if (removeFromParty)
        {
            prisoner.Formation.DeleteUnit(prisoner, false);
            prisoner.gameObject.SetActive(false);
            prisoner.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
            prisoner.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
            Round.OrderedUnits.RemoveAll(unit => unit == prisoner);
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + captor.Character.Class + "_capture_fade_in");          
        }
    }

    public void ControlUnit(FormationUnit prisoner, FormationUnit controller, int duration)
    {
        RaidSceneManager.BattleGround.Controls.Add(new ControlRecord(prisoner, controller, duration));
        prisoner.Formation.DeleteUnit(prisoner, false);
        prisoner.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
        prisoner.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
        Round.OrderedUnits.RemoveAll(unit => unit == prisoner);

        prisoner.Team = controller.Team;
        prisoner.transform.SetParent(controller.Party.transform, true);
        prisoner.Party = controller.Party;
        prisoner.Formation = controller.Formation;
        prisoner.CombatInfo.PrepareForBattle(prisoner.CombatInfo.CombatId);
        prisoner.InstantFlip();
        prisoner.SetMoveSmoothTime(0.1f);

        if (prisoner.Formation.AvailableFreeSpace >= prisoner.Size)
            prisoner.Formation.SpawnUnit(prisoner, 4);
        else if (prisoner.Formation.AvailableSummonSpace >= prisoner.Size)
        {
            var purgingCandidates = new List<FormationUnit>(prisoner.Formation.Party.Units);
            purgingCandidates.Sort((x, y) =>
            {
                if (Mathf.Abs(x.Rank - 4) == Mathf.Abs(y.Rank - 4)) return 0;
                if (Mathf.Abs(x.Rank - 4) < Mathf.Abs(y.Rank - 4)) return 1; else return -1;
            });

            for (int i = purgingCandidates.Count - 1; i >= 0; i--)
            {
                if (purgingCandidates[i].Character.IsMonster && purgingCandidates[i].Character.BattleModifiers.CanBeSummonRank)
                {
                    RaidSceneManager.Instanse.SummonPurging(purgingCandidates[i]);
                    if (prisoner.Formation.AvailableFreeSpace >= prisoner.Size)
                    {
                        prisoner.Formation.SpawnUnit(prisoner, 4);
                        prisoner.OverlaySlot.UpdateOverlay();
                        return;
                    }
                }
            }
            Debug.LogError("Not enough control space for " + prisoner.name);
        }
        else
        {
            Debug.LogError("Not enough control space for " + prisoner.name);
        }
        prisoner.OverlaySlot.UpdateOverlay();
    }

    public void ReleaseUnit(CaptureRecord captureRecord)
    {
        RaidSceneManager.BattleGround.Captures.Remove(captureRecord);
        if (captureRecord.RemoveFromParty)
        {
            HeroFormation.SpawnUnit(captureRecord.PrisonerUnit, 1);
            captureRecord.PrisonerUnit.gameObject.SetActive(true);
            captureRecord.PrisonerUnit.InstantRelocation();
            captureRecord.PrisonerUnit.SetCombatAnimation(true);
            captureRecord.PrisonerUnit.OverlaySlot.UpdateOverlay();
        }
        else
        {
            if (captureRecord.Component.ReleaseEffects.Count > 0)
            {
                foreach (var captorEffectString in captureRecord.Component.ReleaseEffects)
                {
                    var captorEffect = DarkestDungeonManager.Data.Effects[captorEffectString];
                    foreach (var subEffect in captorEffect.SubEffects)
                        subEffect.ApplyInstant(captureRecord.CaptorUnit, captureRecord.PrisonerUnit, captorEffect);
                }
                captureRecord.PrisonerUnit.RemoveCaptureEffect();
                FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" +
                    captureRecord.CaptorUnit.Character.Class + "_capture_fade_out");
            }
        }
    }

    public bool IsLifeLinked(FormationUnit unit, LifeLink link)
    {
        return unit.Formation.ContainsBaseClass(link.LinkBaseClass);
    }

    public void UncontrolUnit(ControlRecord controlRecord)
    {
        RaidSceneManager.BattleGround.Controls.Remove(controlRecord);
        var prisoner = controlRecord.PrisonerUnit;
        prisoner.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
        prisoner.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
        prisoner.Formation.DeleteUnit(prisoner, false);
        Round.OrderedUnits.RemoveAll(unit => unit == prisoner);

        var targetFormation = prisoner.Team == Team.Monsters? HeroFormation : MonsterFormation;
        prisoner.Team = prisoner.Team == Team.Monsters? Team.Heroes : Team.Monsters;
        prisoner.transform.SetParent(targetFormation.Party.transform, true);
        prisoner.SetMoveSmoothTime(0.1f);
        prisoner.Party = targetFormation.Party;
        prisoner.Formation = targetFormation;
        prisoner.CombatInfo.PrepareForBattle(prisoner.CombatInfo.CombatId);
        prisoner.InstantFlip();
        prisoner.Formation.SpawnUnit(prisoner, 4);
        if (controlRecord.ControllComponent.UncontrollEffects.Count > 0)
        {
            foreach (var controlEffectString in controlRecord.ControllComponent.UncontrollEffects)
            {
                var captorEffect = DarkestDungeonManager.Data.Effects[controlEffectString];
                foreach (var subEffect in captorEffect.SubEffects)
                    subEffect.ApplyInstant(controlRecord.ControllUnit, controlRecord.PrisonerUnit, captorEffect);
            }
        }
        prisoner.OverlaySlot.UpdateOverlay();
    }

    public void SummonUnit(MonsterData monsterData, GameObject unitObject, int targetRank, bool initiativeRoll, bool canSpawnLoot)
    {
        FormationUnit newUnit = Instantiate(unitObject).GetComponent<FormationUnit>();

        newUnit.Initialize(new Monster(monsterData), targetRank, Team.Monsters);
        newUnit.transform.SetParent(MonsterParty.transform, false);
        newUnit.RectTransform.position = MonsterFormation.RankHolder.RankMarkSlots[targetRank - 1].transform.position;
        newUnit.Party = MonsterParty;
        newUnit.Formation = MonsterFormation;
        newUnit.CombatInfo.PrepareForBattle(PickId(), newUnit.Character as Monster, canSpawnLoot);
        RaidSceneManager.TorchMeter.ApplyBuffsForUnit(newUnit);

        if (!MonsterFormation.Ranks.FacingRight)
            newUnit.InstantFlip();

        newUnit.ResetAnimations();

        if (newUnit.Formation.AvailableFreeSpace >= newUnit.Size)
        {
            newUnit.Formation.SpawnUnit(newUnit, targetRank);
            if(initiativeRoll)
                Round.InsertInitiativeRoll(newUnit);
            #region Spawn Check
            if(monsterData.Spawn != null)
            {
                for (int i = 0; i < monsterData.Spawn.Effects.Count; i++)
                    for (int j = 0; j < monsterData.Spawn.Effects[i].SubEffects.Count; j++)
                        monsterData.Spawn.Effects[i].SubEffects[j].ApplyInstant(newUnit, newUnit, monsterData.Spawn.Effects[i]);
                newUnit.OverlaySlot.UpdateOverlay();
            }
            #endregion
            #region Companion Check
            if (monsterData.Companion != null)
            {
                var companion = newUnit.Formation.Party.Units.Find(unit =>
                    unit.Character.Class == monsterData.Companion.MonsterClass);
                if(companion != null)
                {
                    Companions.Add(new CompanionRecord(newUnit, companion));
                    foreach (var buff in monsterData.Companion.Buffs)
                        newUnit.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Raid,
                                BuffSourceType.Adventure, 3));
                    newUnit.OverlaySlot.UpdateOverlay();
                }
            }
            for(int i = 0; i < newUnit.Party.Units.Count; i++)
            {
                if(newUnit.Party.Units[i] != newUnit && newUnit.Party.Units[i].Character.IsMonster)
                {
                    var monster = newUnit.Party.Units[i].Character as Monster;
                    if(monster.Data.Companion != null)
                    {
                        var companion = newUnit.Party.Units.Find(unit =>
                        unit.Character.Class == monster.Data.Companion.MonsterClass);
                        if (companion != null)
                        {
                            Companions.Add(new CompanionRecord(newUnit.Party.Units[i], companion));
                            foreach (var buff in monster.Data.Companion.Buffs)
                                newUnit.Party.Units[i].Character.AddBuff(new BuffInfo(buff, BuffDurationType.Raid,
                                        BuffSourceType.Adventure, 3));
                            newUnit.Party.Units[i].OverlaySlot.UpdateOverlay();
                        }
                    }
                }
            }
            #endregion
        }
        else if (newUnit.Formation.AvailableSummonSpace >= newUnit.Size)
        {
            var purgingCandidates = new List<FormationUnit>(newUnit.Formation.Party.Units);
            purgingCandidates.Sort((x, y) =>
            {
                if (Mathf.Abs(x.Rank - targetRank) == Mathf.Abs(y.Rank - targetRank)) return 0;
                if (Mathf.Abs(x.Rank - targetRank) < Mathf.Abs(y.Rank - targetRank)) return 1; else return -1;
            });

            for (int i = purgingCandidates.Count - 1; i >= 0; i--)
            {
                if (purgingCandidates[i].Character.IsMonster && purgingCandidates[i].Character.BattleModifiers.CanBeSummonRank)
                {
                    RaidSceneManager.Instanse.SummonPurging(purgingCandidates[i]);
                    if (newUnit.Formation.AvailableFreeSpace >= newUnit.Size)
                    {
                        newUnit.Formation.SpawnUnit(newUnit, targetRank);
                        if(initiativeRoll)
                            Round.InsertInitiativeRoll(newUnit);
                        #region Spawn Check
                        if (monsterData.Spawn != null)
                        {
                            for (int k = 0; k < monsterData.Spawn.Effects.Count; k++)
                                for (int j = 0; j < monsterData.Spawn.Effects[k].SubEffects.Count; j++)
                                    monsterData.Spawn.Effects[k].SubEffects[j].ApplyInstant(newUnit, newUnit, monsterData.Spawn.Effects[k]);
                            newUnit.OverlaySlot.UpdateOverlay();
                        }
                        #endregion
                        #region Companion Check
                        if (monsterData.Companion != null)
                        {
                            var companion = newUnit.Formation.Party.Units.Find(unit => 
                                unit.Character.Class == monsterData.Companion.MonsterClass);
                            if (companion != null)
                            {
                                Companions.Add(new CompanionRecord(newUnit, companion));
                                foreach (var buff in monsterData.Companion.Buffs)
                                    newUnit.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Raid,
                                            BuffSourceType.Adventure, 3));
                                newUnit.OverlaySlot.UpdateOverlay();
                            }
                        }
                        for (int j = 0; j < newUnit.Party.Units.Count; j++)
                        {
                            if (newUnit.Party.Units[j] != newUnit && newUnit.Party.Units[j].Character.IsMonster)
                            {
                                var monster = newUnit.Party.Units[j].Character as Monster;
                                if (monster.Data.Companion != null)
                                {
                                    var companion = newUnit.Party.Units.Find(unit =>
                                        unit.Character.Class == monster.Data.Companion.MonsterClass);
                                    if (companion != null)
                                    {
                                        Companions.Add(new CompanionRecord(newUnit.Party.Units[j], companion));
                                        foreach (var buff in monster.Data.Companion.Buffs)
                                            newUnit.Party.Units[j].Character.AddBuff(new BuffInfo(buff, BuffDurationType.Raid,
                                                    BuffSourceType.Adventure, 3));
                                        newUnit.Party.Units[j].OverlaySlot.UpdateOverlay();
                                    }
                                }
                            }
                        }
                        #endregion
                        return;
                    }
                }
            }
            ReturnId(newUnit.CombatInfo.CombatId);
            Debug.LogError("Not enough summon space for " + newUnit.name);
            Destroy(newUnit.gameObject);
        }
        else
        {
            ReturnId(newUnit.CombatInfo.CombatId);
            Debug.LogError("Not enough summon space for " + newUnit.name);
            Destroy(newUnit.gameObject);
        }
    }

    public FormationUnit ReplaceUnit(MonsterData data, FormationUnit oldUnit, GameObject newObject, bool roll = false, float carryHp = 0)
    {
        #region Companion
        var companionRecord = Companions.Find(record => record.TargetUnit == oldUnit || record.CompanionUnit == oldUnit);
        if (companionRecord != null)
        {
            Companions.Remove(companionRecord);

            if (companionRecord.CompanionUnit == oldUnit)
            {
                foreach (var buff in companionRecord.CompanionComponent.Buffs)
                    companionRecord.TargetUnit.Character.RemoveSourceBuff(buff, BuffSourceType.Adventure);
                companionRecord.TargetUnit.OverlaySlot.UpdateOverlay();
            }
        }
        #endregion

        FormationUnit newUnit = Instantiate(newObject).GetComponent<FormationUnit>();
        newUnit.Initialize(new Monster(data), oldUnit.Rank, Team.Monsters);
        newUnit.transform.SetParent(oldUnit.transform.parent, false);
        newUnit.transform.position = oldUnit.transform.position;
        newUnit.Party = oldUnit.Party;
        newUnit.Party.Units[newUnit.Party.Units.IndexOf(oldUnit)] = newUnit;
        newUnit.Formation = oldUnit.Formation;
        RaidSceneManager.TorchMeter.ApplyBuffsForUnit(newUnit);
        oldUnit.OverlaySlot.LockOnUnit(newUnit);
        oldUnit.RankSlot.PutInSlot(newUnit);
        if (carryHp != 0)
            newUnit.Character.Health.ValueRatio = carryHp;
        newUnit.CombatInfo.PrepareForBattle(oldUnit.CombatInfo.CombatId, newUnit.Character as Monster, true);

        DarkestSoundManager.ExecuteNarration("change_monster_class", NarrationPlace.Raid,
            oldUnit.Character.Class, newUnit.Character.Class);

        if (!newUnit.RankSlot.Ranks.FacingRight)
            newUnit.InstantFlip();

        newUnit.ResetAnimations();
        if (roll)
        {
            for (int i = 0; i < Round.OrderedUnits.Count; i++)
                if (Round.OrderedUnits[i] == oldUnit)
                    Round.OrderedUnits[i] = newUnit;
        }
        else
        {
            Round.OrderedUnits.RemoveAll(unit => unit == oldUnit);
        }

        if (SharedHealth.IsActive && data.SharedHealth != null)
        {
            SharedHealth.SharedUnits.Remove(oldUnit);
            SharedHealth.SharedUnits.Add(newUnit);
            newUnit.Character[AttributeType.HitPoints, true] = SharedHealth.Health;
        }

        Destroy(oldUnit.gameObject);

        #region Companion Check
        if (data.Companion != null)
        {
            var companion = newUnit.Formation.Party.Units.Find(unit => unit.Character.Class == data.Companion.MonsterClass);
            if (companion != null)
            {
                Companions.Add(new CompanionRecord(newUnit, companion));
                foreach (var buff in data.Companion.Buffs)
                    newUnit.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Raid,
                            BuffSourceType.Adventure, 3));
                newUnit.OverlaySlot.UpdateOverlay();
            }
        }
        for (int i = 0; i < newUnit.Party.Units.Count; i++)
        {
            if (newUnit.Party.Units[i] != newUnit && newUnit.Party.Units[i].Character.IsMonster)
            {
                var monster = newUnit.Party.Units[i].Character as Monster;
                if (monster.Data.Companion != null)
                {
                    var companion = newUnit.Party.Units.Find(unit => unit.Character.Class == monster.Data.Companion.MonsterClass);
                    if (companion != null)
                    {
                        Companions.Add(new CompanionRecord(newUnit.Party.Units[i], companion));
                        foreach (var buff in monster.Data.Companion.Buffs)
                            newUnit.Party.Units[i].Character.AddBuff(new BuffInfo(buff, BuffDurationType.Raid,
                                    BuffSourceType.Adventure, 3));
                        newUnit.Party.Units[i].OverlaySlot.UpdateOverlay();
                    }
                }
            }
        }
        #endregion

        return newUnit;
    }

    #endregion

    public FormationUnit FindById(int id)
    {
        var unitFound = HeroParty.Units.Find(unit => unit.CombatInfo.CombatId == id);
        if (unitFound != null)
            return unitFound;
        else
            return MonsterParty.Units.Find(unit => unit.CombatInfo.CombatId == id);
    }

    public int NextRound()
    {
        return Round.NextRound(this);
    }

    public void ResetTargetRanks()
    {
        HeroFormation.RankHolder.ClearMarks();
        MonsterFormation.RankHolder.ClearMarks();
    }

    public void UnitDestroyed(FormationUnit deadUnit)
    {
        if (deadUnit.Character.Loot != null)
            BattleLoot.AddRange(deadUnit.Character.Loot);

        Controls.RemoveAll(record => record.PrisonerUnit == deadUnit);
        deadUnit.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
        deadUnit.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
        ReturnId(deadUnit.CombatInfo.CombatId);
        Round.OrderedUnits.RemoveAll(unit => unit == deadUnit);
        if (deadUnit.Character.IsMonster)
            RaidSceneManager.Raid.KilledMonsters.Add(deadUnit.Character.Name);
    }

    public void UnitCorpsed(FormationUnit deadUnit)
    {
        if (deadUnit.Character.Loot != null)
            BattleLoot.AddRange(deadUnit.Character.Loot);

        deadUnit.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
        deadUnit.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
        Round.OrderedUnits.RemoveAll(unit => unit == deadUnit);
        if (deadUnit.Character.IsMonster)
            RaidSceneManager.Raid.KilledMonsters.Add(deadUnit.Character.Name);
    }

    private int PickId()
    {
        if (CombatIds.Count > 0)
        {
            int newId = CombatIds[0];
            CombatIds.Remove(newId);
            return newId;
        }
        return -1;
    }

    private void ReturnId(int id)
    {
        if (!CombatIds.Contains(id))
            CombatIds.Add(id);
    }

    private void ResetIds()
    {
        CombatIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
    }
}