using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum Team { Heroes, Monsters }
public enum TurnType { HeroTurn, MonsterTurn }
public enum RoundStatus { Start, Progress, Finish }
public enum TurnStatus { PreTurn, Progress, PostTurn }
public enum BattleStatus { Peace, Fighting, Finished }
public enum SurpriseStatus { Nothing, MonstersSurprised, HeroesSurprised }

public class CaptureRecord
{
    public FormationUnit PrisonerUnit { get; set; }
    public FormationUnit CaptorUnit { get; set; }
    public bool RemoveFromParty { get; set; }
    public FullCaptor Component
    {
        get
        {
            return CaptorUnit.Character.FullCaptor;
        }
    }

    public CaptureRecord()
    {
    }
    public CaptureRecord(FormationUnit prisoner, FormationUnit captor, bool removeFromParty)
    {
        PrisonerUnit = prisoner;
        CaptorUnit = captor;
        RemoveFromParty = removeFromParty;
    }

    public override int GetHashCode()
    {
        return PrisonerUnit.CombatInfo.CombatId * 100000 + CaptorUnit.CombatInfo.CombatId * 1000 + (RemoveFromParty ? 1 : 0);
    }
    public int GetHashPrisonerId(int hashCode)
    {
        return hashCode / 100000;
    }
    public int GetHashCaptorId(int hashCode)
    {
        return hashCode % 100000 / 1000;
    }
    public bool GetHashRemoveFromParty(int hashCode)
    {
        return hashCode % 2 == 1;
    }
}

public class ControlRecord
{
    public FormationUnit PrisonerUnit { get; set; }
    public FormationUnit ControllUnit { get; set; }
    public int DurationLeft { get; set; }
    public Controller ControllComponent
    {
        get
        {
            return ControllUnit.Character.ControllerCaptor;
        }
    }

    public ControlRecord()
    {
    }
    public ControlRecord(FormationUnit prisoner, FormationUnit controller, int duration)
    {
        PrisonerUnit = prisoner;
        ControllUnit = controller;
        DurationLeft = duration;
    }

    public override int GetHashCode()
    {
        return PrisonerUnit.CombatInfo.CombatId * 100000 + ControllUnit.CombatInfo.CombatId * 1000 + DurationLeft;
    }
    public int GetHashPrisonerId(int hashCode)
    {
        return hashCode / 100000;
    }
    public int GetHashControlId(int hashCode)
    {
        return hashCode % 100000 / 1000;
    }
    public int GetHashDurationLeft(int hashCode)
    {
        return hashCode % 1000;
    }
}

public class CompanionRecord
{
    public FormationUnit TargetUnit { get; set; }
    public FormationUnit CompanionUnit { get; set; }
    public Companion CompanionComponent
    {
        get
        {
            return TargetUnit.Character.Companion;
        }
    }

    public CompanionRecord()
    {
    }
    public CompanionRecord(FormationUnit target, FormationUnit companion)
    {
        TargetUnit = target;
        CompanionUnit = companion;
    }

    public override int GetHashCode()
    {
        return TargetUnit.CombatInfo.CombatId * 1000 + CompanionUnit.CombatInfo.CombatId;
    }
    public int GetHashTargetId(int hashCode)
    {
        return hashCode / 1000;
    }
    public int GetHashCompanionId(int hashCode)
    {
        return hashCode % 1000;
    }
}

public class BattleGround : MonoBehaviour
{
    public BattleFormation heroFormation;
    public BattleFormation monsterFormation;
    public SharedHealthInfo sharedHealthRecord;
    public RectTransform heroPosition;
    public RectTransform monsterPosition;
    public RectTransform cameraFocus;
    public Backdrop backdrop;

    public SharedHealthInfo SharedHealth
    {
        get
        {
            return sharedHealthRecord;
        }
    }
    public List<CompanionRecord> Companions
    {
        get; 
        private set;
    }
    public List<CaptureRecord> Captures { get; private set; }
    public List<ControlRecord> Controls { get; private set; }
    public List<LootDefinition> BattleLoot { get; private set; }
    public List<string> LastDamaged { get; set; }
    public string LastSkillUsed { get; set; }

    public int HeroNumber
    {
        get
        {
            return heroFormation.party.Units.Count;
        }
    }
    public int MarkedHeroes
    {
        get
        {
            return heroFormation.party.Units.FindAll(unit => unit.Character.GetStatusEffect(StatusType.Marked).IsApplied).Count;
        }
    }
    public int VirtuedHeroes
    {
        get
        {
            return heroFormation.party.Units.FindAll(unit => unit.Character.IsVirtued).Count;
        }
    }
    public int NonVirtuedHeroes
    {
        get
        {
            return heroFormation.party.Units.FindAll(unit => unit.Character.IsVirtued == false).Count;
        }
    }
    public int NonDeathsDoorHeroes
    {
        get
        {
            return heroFormation.party.Units.FindAll(unit => unit.Character.AtDeathsDoor == false).Count;
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
            return monsterFormation.party.Units.FindAll(unit => unit.Character.IsMonster
                && !(unit.Character as Monster).Types.Contains(MonsterType.Corpse)).Count;
        }
    }
    public int GuardedMonsters
    {
        get
        {
            return monsterFormation.party.Units.FindAll(unit => unit.Character.GetStatusEffect(StatusType.Guarded).IsApplied).Count;
        }
    }
    public int MonsterSize
    {
        get
        {
            return monsterFormation.party.Units.FindAll(unit => unit.Character.IsMonster
                && !(unit.Character as Monster).Types.Contains(MonsterType.Corpse)).Sum(unit => unit.Size);
        }
    }

    public Round Round { get; set; }
    public int StallingRoundNumber { get; set; }

    public BattleStatus BattleStatus
    {
        get;
        set;
    }
    public RoundStatus RoundStatus
    {
        get 
        {
            return Round.RoundStatus;
        }
        set
        {
            Round.RoundStatus = value;
        }
    }
    public TurnStatus TurnStatus
    {
        get
        {
            return Round.TurnStatus;
        }
    }
    public SurpriseStatus SurpriseStatus
    {
        get;
        set;
    }

    public RectTransform Rect { get; set; }
    public List<int> CombatIds { get; set; }

    private List<FormationUnit> loadedRemovedPrisoners = new List<FormationUnit>();

    void Awake()
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

    int PickId()
    {
        if(CombatIds.Count > 0)
        {
            int index = Random.Range(0, CombatIds.Count);
            int newId = CombatIds[index];
            CombatIds.Remove(newId);
            return newId;
        }
        return -1;
    }
    void ReturnId(int id)
    {
        if (!CombatIds.Contains(id))
            CombatIds.Add(id);
    }
    void ResetIdSet()
    {
        CombatIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
    }

    public int NextRound()
    {
        return Round.NextRound(this);
    }

    public static bool IsPerformerSkillTargetable(CombatSkill skill, BattleFormation allies, BattleFormation enemies, FormationUnit performer)
    {
        if (skill.TargetRanks.IsSelfTarget)
        {
            if (skill.Heal != null && performer.CombatInfo.BlockedHealUnitIds.Contains(performer.CombatInfo.CombatId))
                return false;
            if (skill.IsBuffSkill && performer.CombatInfo.BlockedBuffUnitIds.Contains(performer.CombatInfo.CombatId))
                return false;
            return true;
        }

        if(skill.TargetRanks.IsSelfFormation)
        {
            if (skill.IsSelfValid)
            {
                if (skill.Heal != null)
                {
                    if (performer.CombatInfo.BlockedHealUnitIds.Contains(performer.CombatInfo.CombatId) == false)
                        return true;
                }
                else if (skill.IsBuffSkill)
                {
                    if (performer.CombatInfo.BlockedBuffUnitIds.Contains(performer.CombatInfo.CombatId) == false)
                        return true;
                }
                else
                    return true;
            }

            for(int i = 0; i < allies.party.Units.Count; i++)
            {
                if (skill.Heal != null && performer.CombatInfo.BlockedHealUnitIds.Contains(allies.party.Units[i].CombatInfo.CombatId))
                    continue;
                if (skill.IsBuffSkill && performer.CombatInfo.BlockedBuffUnitIds.Contains(allies.party.Units[i].CombatInfo.CombatId))
                    continue;

                if(allies.party.Units[i] != performer && skill.TargetRanks.IsTargetableUnit(allies.party.Units[i]))
                    return true;
            }
        }
        else
        {
            for(int i = 0; i < enemies.party.Units.Count; i++)
                if(skill.TargetRanks.IsTargetableUnit(enemies.party.Units[i]))
                    return true;
        }

        return false;
    }

    public FormationUnit ReplaceUnit(MonsterData monsterData, FormationUnit replacementUnit, GameObject unitObject, bool replaceRoll = false, float carryHp = 0)
    {
        #region Companion
        var companionRecord = Companions.Find(record => record.TargetUnit == replacementUnit || record.CompanionUnit == replacementUnit);
        if (companionRecord != null)
        {
            Companions.Remove(companionRecord);

            if (companionRecord.CompanionUnit == replacementUnit)
            {
                foreach (var buff in companionRecord.CompanionComponent.Buffs)
                    companionRecord.TargetUnit.Character.RemoveSourceBuff(buff, BuffSourceType.Adventure);
                companionRecord.TargetUnit.OverlaySlot.UpdateOverlay();
            }
        }
        #endregion

        FormationUnit newUnit = Instantiate(unitObject).GetComponent<FormationUnit>();
        newUnit.Initialize(new Monster(monsterData), replacementUnit.Rank, Team.Monsters);
        newUnit.transform.SetParent(replacementUnit.transform.parent, false);
        newUnit.transform.position = replacementUnit.transform.position;
        newUnit.Party = replacementUnit.Party;
        newUnit.Party.Units[newUnit.Party.Units.IndexOf(replacementUnit)] = newUnit;
        newUnit.Formation = replacementUnit.Formation;
        RaidSceneManager.TorchMeter.ApplyBuffsForUnit(newUnit);
        replacementUnit.OverlaySlot.LockOnUnit(newUnit);
        replacementUnit.RankSlot.PutInSlot(newUnit);
        if (carryHp != 0)
            newUnit.Character.Health.ValueRatio = carryHp;
        newUnit.CombatInfo.PrepareForBattle(replacementUnit.CombatInfo.CombatId, newUnit.Character as Monster, true);

        if (!newUnit.RankSlot.Ranks.facingRight)
            newUnit.InstantFlip();

        newUnit.ResetAnimations();
        if(replaceRoll)
        {
            for (int i = 0; i < Round.OrderedUnits.Count; i++)
                if (Round.OrderedUnits[i] == replacementUnit)
                    Round.OrderedUnits[i] = newUnit;
        }
        else
        {
            Round.OrderedUnits.RemoveAll(unit => unit == replacementUnit);
        }

        if(sharedHealthRecord.IsActive && monsterData.SharedHealth != null)
        {
            sharedHealthRecord.SharedUnits.Remove(replacementUnit);
            sharedHealthRecord.SharedUnits.Add(newUnit);
            newUnit.Character[AttributeType.HitPoints, true] = sharedHealthRecord.Health;
        }

        Destroy(replacementUnit.gameObject);

        #region Companion Check
        if (monsterData.Companion != null)
        {
            var companion = newUnit.Formation.party.Units.Find(unit => unit.Character.Class == monsterData.Companion.MonsterClass);
            if (companion != null)
            {
                Companions.Add(new CompanionRecord(newUnit, companion));
                foreach (var buff in monsterData.Companion.Buffs)
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
            var purgingCandidates = new List<FormationUnit>(prisoner.Formation.party.Units);
            purgingCandidates.Sort((x, y) =>
            {
                if (Mathf.Abs(x.Rank - 4) == Mathf.Abs(y.Rank - 4)) return 0;
                if (Mathf.Abs(x.Rank - 4) < Mathf.Abs(y.Rank - 4)) return 1; else return -1;
            });

            for (int i = purgingCandidates.Count - 1; i >= 0; i--)
            {
                if (purgingCandidates[i].Character.IsMonster && (purgingCandidates[i].Character as Monster).Data.Modifiers.CanBeSummonRank)
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
            heroFormation.SpawnUnit(captureRecord.PrisonerUnit, 1);
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
                FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + captureRecord.CaptorUnit.Character.Class + "_capture_fade_out");
            }
        }
    }
    public void UncontrolUnit(ControlRecord controlRecord)
    {
        RaidSceneManager.BattleGround.Controls.Remove(controlRecord);
        var prisoner = controlRecord.PrisonerUnit;
        prisoner.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
        prisoner.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
        prisoner.Formation.DeleteUnit(prisoner, false);
        Round.OrderedUnits.RemoveAll(unit => unit == prisoner);

        var targetFormation = prisoner.Team == Team.Monsters? heroFormation : monsterFormation;
        prisoner.Team = prisoner.Team == Team.Monsters? Team.Heroes : Team.Monsters;
        prisoner.transform.SetParent(targetFormation.party.transform, true);
        prisoner.SetMoveSmoothTime(0.1f);
        prisoner.Party = targetFormation.party;
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
        newUnit.transform.SetParent(monsterFormation.party.transform, false);
        newUnit.RectTransform.position = monsterFormation.rankHolder.rankMarkSlots[targetRank - 1].transform.position;
        newUnit.Party = monsterFormation.party;
        newUnit.Formation = monsterFormation;
        newUnit.CombatInfo.PrepareForBattle(PickId(), newUnit.Character as Monster, canSpawnLoot);
        RaidSceneManager.TorchMeter.ApplyBuffsForUnit(newUnit);

        if (!monsterFormation.ranks.facingRight)
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
                var companion = newUnit.Formation.party.Units.Find(unit => unit.Character.Class == monsterData.Companion.MonsterClass);
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
        }
        else if (newUnit.Formation.AvailableSummonSpace >= newUnit.Size)
        {
            var purgingCandidates = new List<FormationUnit>(newUnit.Formation.party.Units);
            purgingCandidates.Sort((x, y) =>
            {
                if (Mathf.Abs(x.Rank - targetRank) == Mathf.Abs(y.Rank - targetRank)) return 0;
                if (Mathf.Abs(x.Rank - targetRank) < Mathf.Abs(y.Rank - targetRank)) return 1; else return -1;
            });

            for (int i = purgingCandidates.Count - 1; i >= 0; i--)
            {
                if (purgingCandidates[i].Character.IsMonster && (purgingCandidates[i].Character as Monster).Data.Modifiers.CanBeSummonRank)
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
                            var companion = newUnit.Formation.party.Units.Find(unit => unit.Character.Class == monsterData.Companion.MonsterClass);
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
                                    var companion = newUnit.Party.Units.Find(unit => unit.Character.Class == monster.Data.Companion.MonsterClass);
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
    public void SpawnEncounter(BattleEncounter encounter, bool campfireSurprise)
    {
        monsterFormation.LoadParty(encounter);
        for (int i = 0; i < monsterFormation.party.Units.Count; i++)
        {
            var monster = monsterFormation.party.Units[i].Character as Monster;
            monsterFormation.party.Units[i].CombatInfo.PrepareForBattle(PickId(), monster, true);
            if (monster.TorchlightModifier != null)
                RaidSceneManager.TorchMeter.Modify(monster.TorchlightModifier);
            RaidSceneManager.TorchMeter.ApplyBuffsForUnit(monsterFormation.party.Units[i]);
            if (monster.Data.BattleBackdrop != null)
                backdrop.Activate(monster.Data.BattleBackdrop);
            #region Spawn Check
            if (monster.Data.Spawn != null)
            {
                for (int k = 0; k < monster.Data.Spawn.Effects.Count; k++)
                    for (int j = 0; j < monster.Data.Spawn.Effects[k].SubEffects.Count; j++)
                        monster.Data.Spawn.Effects[k].SubEffects[j].ApplyInstant(monsterFormation.party.Units[i], monsterFormation.party.Units[i], monster.Data.Spawn.Effects[k]);
                monsterFormation.party.Units[i].OverlaySlot.UpdateOverlay();
            }
            #endregion
        }
        var shared = monsterFormation.party.Units.Find(unit => unit.Character.SharedHealth != null);
        if(shared != null)
            sharedHealthRecord.Initialize(monsterFormation.party.Units, shared.Character.SharedHealth);
        monsterFormation.overlay.UpdateOverlay();

        SurpriseStatus = SurpriseStatus.Nothing;

        if (monsterFormation.AlwaysSurprises() || campfireSurprise)
            SurpriseStatus = SurpriseStatus.HeroesSurprised;
        else if (monsterFormation.AlwaysBeSurprised())
            SurpriseStatus = SurpriseStatus.MonstersSurprised;
        else
        {
            if(monsterFormation.CanBeSurprised())
            {
                float monstersSurprised = 0.1f + RaidSceneManager.TorchMeter.CurrentRange.MonstersSurprised;
                for (int i = 0; i < heroFormation.party.Units.Count; i++)
                    monstersSurprised += heroFormation.party.Units[i].Character[AttributeType.MonsterSurpirseChance].ModifiedValue;
                monstersSurprised = Mathf.Clamp(monstersSurprised, 0, 0.65f);
                if (RandomSolver.CheckSuccess(monstersSurprised))
                    SurpriseStatus = SurpriseStatus.MonstersSurprised;
            }

            if(monsterFormation.CanSurprise() && SurpriseStatus == SurpriseStatus.Nothing)
            {
                float heroesSurprised = 0.1f + RaidSceneManager.TorchMeter.CurrentRange.HeroesSurprised;
                for (int i = 0; i < heroFormation.party.Units.Count; i++)
                    heroesSurprised += heroFormation.party.Units[i].Character[AttributeType.PartySurpriseChance].ModifiedValue;
                heroesSurprised = Mathf.Clamp(heroesSurprised, 0, 0.65f);
                if (RandomSolver.CheckSuccess(heroesSurprised))
                    SurpriseStatus = SurpriseStatus.HeroesSurprised;
            }
        }
        Round.StartBattle(this);
    }
    public void LoadEncounter(BattleFormationSaveData encounterSaveData)
    {
        monsterFormation.LoadParty(encounterSaveData, false);
        for (int i = 0; i < monsterFormation.party.Units.Count; i++)
        {
            RaidSceneManager.TorchMeter.ApplyBuffsForUnit(monsterFormation.party.Units[i]);
            if(monsterFormation.party.Units[i].Character.IsMonster)
            {
                if ((monsterFormation.party.Units[i].Character as Monster).Data.BattleBackdrop != null)
                    backdrop.Activate((monsterFormation.party.Units[i].Character as Monster).Data.BattleBackdrop);
            }
        }
        var shared = monsterFormation.party.Units.Find(unit => unit.Character.SharedHealth != null);
        if(shared != null)
            sharedHealthRecord.Initialize(monsterFormation.party.Units, shared.Character.SharedHealth);
        monsterFormation.overlay.UpdateOverlay();
    }

    public void InitiateBattle()
    {
        BattleLoot.Clear();
        StallingRoundNumber = 0;
        BattleStatus = BattleStatus.Fighting;
        Round.HeroAction = HeroTurnAction.Waiting;
        Rect.SetParent(heroFormation.RectTransform.parent, false);

        heroFormation.RectTransform.SetParent(Rect, true);
        cameraFocus.position = new Vector3(heroFormation.ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x) / 2, 0, 0);
        RaidSceneManager.DungeonCamera.target = cameraFocus;

        for (int i = 0; i < heroFormation.party.Units.Count; i++)
            heroFormation.party.Units[i].CombatInfo.PrepareForBattle(PickId());

        monsterFormation.gameObject.SetActive(true);
        monsterFormation.RectTransform.SetParent(Rect, false);
        monsterFormation.RectTransform.position = new Vector3(heroFormation.ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x),
            heroFormation.RectTransform.position.y, heroFormation.RectTransform.position.z);
        monsterFormation.ranks.InstantRelocation();

        monsterFormation.RectTransform.SetAsLastSibling();
        heroFormation.RectTransform.SetAsLastSibling();
    }
    public void InitiateSavedBattle()
    {
        Rect.SetParent(heroFormation.RectTransform.parent, false);

        heroFormation.RectTransform.SetParent(Rect, true);
        cameraFocus.position = new Vector3(heroFormation.ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x) / 2, 0, 0);
        RaidSceneManager.DungeonCamera.target = cameraFocus;

        monsterFormation.gameObject.SetActive(true);
        monsterFormation.RectTransform.SetParent(Rect, false);
        monsterFormation.RectTransform.position = new Vector3(heroFormation.ranks.RectTransform.position.x
            + Mathf.Abs(monsterPosition.position.x - heroPosition.position.x),
            heroFormation.RectTransform.position.y, heroFormation.RectTransform.position.z);
        monsterFormation.ranks.InstantRelocation();

        monsterFormation.RectTransform.SetAsLastSibling();
        heroFormation.RectTransform.SetAsLastSibling();
    }

    public void FinishBattle()
    {
        StallingRoundNumber = 0;

        for (int i = DarkestDungeonManager.Data.LootDatabase.DarknessLoot["battle"].Count - 1; i >= 0; i--)
        {
            if (DarkestDungeonManager.Data.LootDatabase.DarknessLoot["battle"][i].DarknessLevel == RaidSceneManager.TorchMeter.CurrentRange.Min)
            {
                if (RandomSolver.CheckSuccess(DarkestDungeonManager.Data.LootDatabase.DarknessLoot["battle"][i].Chance))
                {
                    for (int j = 0; j < DarkestDungeonManager.Data.LootDatabase.DarknessLoot["battle"][i].Codes.Count; j++)
                        BattleLoot.Add(new LootDefinition() { Code = DarkestDungeonManager.Data.LootDatabase.DarknessLoot["battle"][i].Codes[j], Count = 1 });
                }
                break;
            }
        }

        heroFormation.RectTransform.SetParent(Rect.parent, false);
        RaidSceneManager.DungeonCamera.target = heroFormation.ranks.RectTransform;
        monsterFormation.overlay.ResetOverlay();
        monsterFormation.party.DeleteFormation();
        monsterFormation.gameObject.SetActive(false);
        ResetIdSet();
        backdrop.Deactivate();
        Round.OrderedUnits.Clear();
        RaidSceneManager.TorchMeter.ClearModifier();
    }
    public void LeaveBattleGround()
    {
        BattleStatus = BattleStatus.Peace;
    }

    public void RetreatFromBattle()
    {
        for(int i = 0; i < Controls.Count; i++)
        {
            if(Controls[i].PrisonerUnit.Character.IsMonster == false)
            {
                var heroInfo = RaidSceneManager.Raid.RaidParty.HeroInfo.Find(info => info.Hero == Controls[i].PrisonerUnit.Character as Hero);
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
                    if (RaidSceneManager.Formations.heroes.party.Units.Count > 0)
                        RaidSceneManager.Formations.heroes.party.Units[0].OverlaySlot.UnitSelected();
                }
            }
        }
        Controls.Clear();

        for (int i = 0; i < Captures.Count; i++)
        {
            if (Captures[i].PrisonerUnit.Character.IsMonster == false)
            {
                var heroInfo = RaidSceneManager.Raid.RaidParty.HeroInfo.Find(info => info.Hero == Captures[i].PrisonerUnit.Character as Hero);
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
                    if (RaidSceneManager.Formations.heroes.party.Units.Count > 0)
                        RaidSceneManager.Formations.heroes.party.Units[0].OverlaySlot.UnitSelected();
                }
            }
        }
        Captures.Clear();
        Companions.Clear();

        FinishBattle();
        Round.HeroAction = HeroTurnAction.Retreat;
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
            RaidSceneManager.Raid.KilledMonsters.Add(deadUnit.Character.Class);
    }
    public void UnitCorpsed(FormationUnit deadUnit)
    {
        if (deadUnit.Character.Loot != null)
            BattleLoot.AddRange(deadUnit.Character.Loot);

        deadUnit.Character.GetStatusEffect(StatusType.Guarded).ResetStatus();
        deadUnit.Character.GetStatusEffect(StatusType.Guard).ResetStatus();
        Round.OrderedUnits.RemoveAll(unit => unit == deadUnit);
        if (deadUnit.Character.IsMonster)
            RaidSceneManager.Raid.KilledMonsters.Add(deadUnit.Character.Class);
    }
    public bool IsLifeLinked(FormationUnit unit, LifeLink link)
    {
        return unit.Formation.ContainsBaseClass(link.LinkBaseClass);
    }
    public bool IsBattleEnded()
    {
        if(Controls.Count != 0 || Captures.Count != 0)
            return false;

        if (heroFormation.AliveUnitsCount == 0 || monsterFormation.AliveUnitsCount == 0 || BattleStatus == BattleStatus.Finished)
        {
            BattleStatus = BattleStatus.Finished;
            return true;
        }
        return false;
    }
    public bool IsBattleOnesided()
    {
        if (heroFormation.AliveUnitsCount == 0 || monsterFormation.AliveUnitsCount == 0)
        {
            return true;
        }
        return false;
    }
    public void LoadBattle(BattleGroundSaveData battleSaveData)
    {
        InitiateSavedBattle();
        LoadEncounter(battleSaveData.MonsterFormation);

        #region Load Guard Statuses
        for(int i = 0; i < heroFormation.party.Units.Count; i++)
        {
            var heroGuardedStatus = heroFormation.party.Units[i].Character[StatusType.Guarded] as GuardedStatusEffect;
            if(heroGuardedStatus.GuardDuration > 0)
            {
                var guardUnit = FindUnitByCombatId(heroGuardedStatus.GuardCombatId);
                if(guardUnit != null)
                {
                    heroGuardedStatus.Guard = guardUnit;
                    var guardStatus = guardUnit.Character[StatusType.Guard] as GuardStatusEffect;
                    guardStatus.Targets.Add(heroFormation.party.Units[i]);
                }
            }
        }
        for (int i = 0; i < monsterFormation.party.Units.Count; i++)
        {
            var monsterGuardedStatus = monsterFormation.party.Units[i].Character[StatusType.Guarded] as GuardedStatusEffect;
            if (monsterGuardedStatus.GuardDuration > 0)
            {
                var guardUnit = FindUnitByCombatId(monsterGuardedStatus.GuardCombatId);
                if (guardUnit != null)
                {
                    monsterGuardedStatus.Guard = guardUnit;
                    var guardStatus = guardUnit.Character[StatusType.Guard] as GuardStatusEffect;
                    guardStatus.Targets.Add(monsterFormation.party.Units[i]);
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

        Round.SelectedUnit = FindUnitByCombatId(battleSaveData.SelectedUnitId);
        Round.SelectedTarget = FindUnitByCombatId(battleSaveData.SelectedTargetId);
        LastSkillUsed = battleSaveData.LastSkillUsed;

        Round.OrderedUnits.Clear();
        for(int i = 0; i < battleSaveData.OrderedUnitsCombatIds.Count; i++)
        {
            var newOrderedUnit = FindUnitByCombatId(battleSaveData.OrderedUnitsCombatIds[i]);
            if (newOrderedUnit != null)
                Round.OrderedUnits.Add(newOrderedUnit);
        }

        CombatIds = battleSaveData.CombatIds;
        LastDamaged = battleSaveData.LastDamaged;
        BattleLoot = battleSaveData.BattleLoot;
        #endregion

        #region Load Removed Captured Units
        for(int i = 0; i < battleSaveData.RemovedUnits.Count; i++)
        {
            var prisonerSaveData = battleSaveData.RemovedUnits[i];

            if (prisonerSaveData.IsHero)
            {
                Hero hero = DarkestDungeonManager.Campaign.Heroes.Find(estateHero => estateHero.RosterId == prisonerSaveData.RosterId);
                FormationUnit unit = Instantiate(Resources.Load<GameObject>("Prefabs/Heroes/" + hero.Class)).GetComponent<FormationUnit>();
                unit.transform.SetParent(heroFormation.party.transform, false);
                unit.Party = heroFormation.party;
                unit.Formation = heroFormation;
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
                unit.transform.SetParent(heroFormation.party.transform, false);
                unit.Party = heroFormation.party;
                unit.Formation = heroFormation;
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
        for(int i = 0; i < heroFormation.party.Units.Count; i++)
        {
            if (heroFormation.party.Units[i].Character[StatusType.Stun].IsApplied)
                heroFormation.party.Units[i].SetHalo("stunned");
            else if (heroFormation.party.Units[i].CombatInfo.IsSurprised)
                heroFormation.party.Units[i].SetHalo("surprised");

            if (heroFormation.party.Units[i].CombatInfo.IsImmobilized)
                heroFormation.party.Units[i].SetDefendAnimation(true);

            if (heroFormation.party.Units[i].Character.IsMonster == false)
            {
                var hero = heroFormation.party.Units[i].Character as Hero;
                if (hero[StatusType.DeathsDoor].IsApplied)
                    hero.ApplyDeathDoor();
                else if (hero[StatusType.DeathRecovery].IsApplied)
                    hero.ApplyMortality();
            }
        }
        for (int i = 0; i < monsterFormation.party.Units.Count; i++)
        {
            if (monsterFormation.party.Units[i].Character[StatusType.Stun].IsApplied)
                monsterFormation.party.Units[i].SetHalo("stunned");
            else if (monsterFormation.party.Units[i].CombatInfo.IsSurprised)
                monsterFormation.party.Units[i].SetHalo("surprised");

            if (monsterFormation.party.Units[i].CombatInfo.IsImmobilized)
                monsterFormation.party.Units[i].SetDefendAnimation(true);

            if (monsterFormation.party.Units[i].Character.IsMonster == false)
            {
                monsterFormation.party.Units[i].SetCombatAnimation(true);
                var hero = monsterFormation.party.Units[i].Character as Hero;
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
                FormationUnit prisoner = FindUnitByCombatId(prisonerId);
                FormationUnit captor = FindUnitByCombatId(captorId);
                newCaptureRecord.PrisonerUnit = prisoner;
                newCaptureRecord.CaptorUnit = captor;
                prisoner.SetCaptureEffect(captor);
                Captures.Add(newCaptureRecord);
            }
            else
            {
                FormationUnit prisoner = loadedRemovedPrisoners.Find(removedUnit => removedUnit.CombatInfo.CombatId == prisonerId);
                FormationUnit captor = FindUnitByCombatId(captorId);
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

            FormationUnit companion = FindUnitByCombatId(companionId);
            FormationUnit target = FindUnitByCombatId(targetId);
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
            FormationUnit prisoner = FindUnitByCombatId(prisonerId);
            FormationUnit controller = FindUnitByCombatId(controllerId);
            newControlRecord.PrisonerUnit = prisoner;
            newControlRecord.ControllUnit = controller;
            Controls.Add(newControlRecord);
        }
        #endregion

        loadedRemovedPrisoners.Clear();

        RaidSceneManager.Formations.HeroOverlay.UpdateOverlay();
        RaidSceneManager.Formations.monsters.overlay.UpdateOverlay();
    }
    public FormationUnit FindUnitByCombatId(int id)
    {
        var unitFound = heroFormation.party.Units.Find(unit => unit.CombatInfo.CombatId == id);
        if (unitFound != null)
            return unitFound;
        else
            return monsterFormation.party.Units.Find(unit => unit.CombatInfo.CombatId == id);
    }
}