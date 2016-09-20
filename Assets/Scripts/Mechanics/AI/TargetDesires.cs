using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum TargetDesireType
{
    Random, Marked, Health, Stress, Rank, 
    FillEmptyCaptor, AllyClass, Resistance
}
public enum TargetSelectParameter
{
    CanTargetVirtued, CanTargetDeathsDoor,
    CanTargetLastHero, CanTargetAfflicted,
    CanTargetNotOverstressed, 
}

public abstract class TargetSelectionDesire : IProportionValue
{
    public string SpecificCombatSkillId { get; set; }
    public bool IsExclusiveDesire { get; set; }
    public bool IsEnemyTargetDesire { get; set; }
    public bool IsFriendlyTargetDesire { get; set; }

    public TargetDesireType Type { get; set; }
    public int Chance { get; set; }

    protected Dictionary<TargetSelectParameter, bool?> Parameters { get; set; }

    public abstract bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision);
    public abstract void GenerateFromDataSet(Dictionary<string, object> dataSet);
}

public class TargetSelectionRandom : TargetSelectionDesire
{
    public TargetSelectionRandom()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Random;
    }
    public TargetSelectionRandom(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Random;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets);

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();
            
            if(decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                int index = Random.Range(0, availableTargets.Count);
                decision.TargetInfo.Targets.Add(availableTargets[index]);
                availableTargets.RemoveAt(index);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    int sideTargetIndex = Random.Range(0, availableTargets.Count);
                    decision.TargetInfo.Targets.Add(availableTargets[sideTargetIndex]);
                    return true;
                }
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "can_target_deaths_door":
                    Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)dataSet[token.Key];
                    break;
                case "can_target_last_hero":
                    Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)dataSet[token.Key];
                    break;
                case "can_target_not_overstressed":
                    Parameters[TargetSelectParameter.CanTargetNotOverstressed] = (bool)dataSet[token.Key];
                    break;
                case "can_target_afflicted":
                    Parameters[TargetSelectParameter.CanTargetAfflicted] = (bool)dataSet[token.Key];
                    break;
                case "can_target_virtued":
                    Parameters[TargetSelectParameter.CanTargetVirtued] = (bool)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in random target desire: " + token.Key);
                    break;
            }
        }
    }
}
public class TargetSelectionMarked : TargetSelectionDesire
{
    public TargetSelectionMarked()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Marked;
    }
    public TargetSelectionMarked(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Marked;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (decision.TargetInfo.Targets.Find(target => target.Character.GetStatusEffect(StatusType.Marked).IsApplied) == null)
            return false;

        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets);

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                availableTargets.RemoveAll(target => !target.Character[StatusType.Marked].IsApplied);
                if (availableTargets.Count == 0)
                    return false;
                int index = Random.Range(0, availableTargets.Count);
                decision.TargetInfo.Targets.Add(availableTargets[index]);
                availableTargets.RemoveAt(index);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    int sideTargetIndex = Random.Range(0, availableTargets.Count);
                    decision.TargetInfo.Targets.Add(availableTargets[sideTargetIndex]);
                    return true;
                }
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in marked target desire: " + token.Key);
                    break;
            }
        }
    }
}
public class TargetSelectionFillCaptor : TargetSelectionDesire
{
    public TargetSelectionFillCaptor()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.FillEmptyCaptor;
    }
    public TargetSelectionFillCaptor(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.FillEmptyCaptor;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (decision.SelectedSkill.Effects.Find(effect => effect.SubEffects.Find(subeffect => subeffect is CaptureEffect) != null) == null)
            return false;

        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets);

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                int index = Random.Range(0, availableTargets.Count);
                decision.TargetInfo.Targets.Add(availableTargets[index]);
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "can_target_deaths_door":
                    Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)dataSet[token.Key];
                    break;
                case "can_target_last_hero":
                    Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in fill captor target desire: " + token.Key);
                    break;
            }
        }
    }
}
public class TargetSelectionHealth : TargetSelectionDesire
{
    public string AllyBaseClassId { get; set; }
    public bool IsGreaterComparison { get; set; }
    
    public TargetSelectionHealth()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Health;
    }
    public TargetSelectionHealth(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Health;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets);
        if (AllyBaseClassId != null && AllyBaseClassId != "")
            availableTargets.RemoveAll(target => target.Character.Class != AllyBaseClassId);
        if (availableTargets.Count == 0)
            return false;

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                int index = Random.Range(0, availableTargets.Count);
                decision.TargetInfo.Targets.Add(availableTargets[index]);
                availableTargets.RemoveAt(index);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    int sideTargetIndex = Random.Range(0, availableTargets.Count);
                    decision.TargetInfo.Targets.Add(availableTargets[sideTargetIndex]);
                    return true;
                }
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_greater_comparison":
                    IsGreaterComparison = (bool)dataSet[token.Key];
                    break;
                case "ally_base_class_id":
                    AllyBaseClassId = (string)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in health target desire: " + token.Key);
                    break;
            }
        }
    }
}
public class TargetSelectionStress : TargetSelectionDesire
{
    public bool IsGreaterComparison { get; set; }

    public TargetSelectionStress()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Stress;
    }
    public TargetSelectionStress(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Stress;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets).FindAll(target => target.Character.IsStressed);

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                int index = Random.Range(0, availableTargets.Count);
                decision.TargetInfo.Targets.Add(availableTargets[index]);
                availableTargets.RemoveAt(index);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    int sideTargetIndex = Random.Range(0, availableTargets.Count);
                    decision.TargetInfo.Targets.Add(availableTargets[sideTargetIndex]);
                    return true;
                }
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_greater_comparison":
                    IsGreaterComparison = (bool)dataSet["is_greater_comparison"];
                    break;
                case "can_target_not_overstressed":
                    Parameters[TargetSelectParameter.CanTargetNotOverstressed] = (bool)dataSet[token.Key];
                    break;
                case "can_target_afflicted":
                    Parameters[TargetSelectParameter.CanTargetAfflicted] = (bool)dataSet[token.Key];
                    break;
                case "can_target_virtued":
                    Parameters[TargetSelectParameter.CanTargetVirtued] = (bool)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in stress target desire: " + token.Key);
                    break;
            }
        }
    }
}
public class TargetSelectionAllyClass : TargetSelectionDesire
{
    public string AllyBaseClass { get; set; }

    public TargetSelectionAllyClass()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.AllyClass;
    }
    public TargetSelectionAllyClass(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.AllyClass;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets).FindAll(target => target.Character.Class == AllyBaseClass);

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                int index = Random.Range(0, availableTargets.Count);
                decision.TargetInfo.Targets.Add(availableTargets[index]);
                availableTargets.RemoveAt(index);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    int sideTargetIndex = Random.Range(0, availableTargets.Count);
                    decision.TargetInfo.Targets.Add(availableTargets[sideTargetIndex]);
                    return true;
                }
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "ally_base_class_id":
                    AllyBaseClass = (string)dataSet["ally_base_class_id"];
                    break;
                default:
                    Debug.LogError("Unknown token in ally base class target desire: " + token.Key);
                    break;
            }
        }
    }
}
public class TargetSelectionResistance : TargetSelectionDesire
{
    public AttributeType ResistanceType { get; set; }
    public bool IsGreaterComparison { get; set; }

    public TargetSelectionResistance()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Resistance;
    }
    public TargetSelectionResistance(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Resistance;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets);

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                float lowestRes = float.MaxValue;
                FormationUnit lowestResTarget = null;
                foreach (var target in availableTargets)
                {
                    if (target.Character[ResistanceType].ModifiedValue < lowestRes)
                    {
                        lowestRes = target.Character[ResistanceType].ModifiedValue;
                        lowestResTarget = target;
                    }
                }
                decision.TargetInfo.Targets.Add(lowestResTarget);
                availableTargets.Remove(lowestResTarget);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    lowestRes = 500f;
                    lowestResTarget = null;
                    foreach (var target in availableTargets)
                    {
                        if (target.Character[ResistanceType].ModifiedValue < lowestRes)
                        {
                            lowestRes = target.Character[ResistanceType].ModifiedValue;
                            lowestResTarget = target;
                        }
                    }
                    if (lowestResTarget != null)
                        decision.TargetInfo.Targets.Add(lowestResTarget);
                    return true;
                }
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_greater_comparison":
                    IsGreaterComparison = (bool)dataSet["is_greater_comparison"];
                    break;
                case "can_target_deaths_door":
                    Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)dataSet[token.Key];
                    break;
                case "can_target_last_hero":
                    Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)dataSet[token.Key];
                    break;
                case "resistance_type_id":
                    ResistanceType = CharacterHelper.StringToAttributeType((string)dataSet["resistance_type_id"]);
                    break;
                default:
                    Debug.LogError("Unknown token in resistance target desire: " + token.Key);
                    break;
            }
        }
    }
}
public class TargetSelectionRank : TargetSelectionDesire
{
    public TargetSelectionRank()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Rank;
    }
    public TargetSelectionRank(Dictionary<string, object> dataSet)
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute
            in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);

        Type = TargetDesireType.Rank;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation || decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        var availableTargets = new List<FormationUnit>(decision.TargetInfo.Targets);
        availableTargets.RemoveAll(target => !target.Formation.rankHolder.MarkedRanks.Contains(target.Rank));

        if (Parameters[TargetSelectParameter.CanTargetDeathsDoor].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetDeathsDoor].Value)
                availableTargets.RemoveAll(unit => unit.Character.AtDeathsDoor);

        if (Parameters[TargetSelectParameter.CanTargetLastHero].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetLastHero].Value)
                availableTargets.RemoveAll(unit => performer.CombatInfo.LastCombatSkillTarget == unit.CombatInfo.CombatId);

        if (Parameters[TargetSelectParameter.CanTargetNotOverstressed].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetNotOverstressed].Value)
                availableTargets.RemoveAll(unit => !unit.Character.IsOverstressed);

        if (Parameters[TargetSelectParameter.CanTargetAfflicted].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetAfflicted].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsAfflicted);

        if (Parameters[TargetSelectParameter.CanTargetVirtued].HasValue)
            if (!Parameters[TargetSelectParameter.CanTargetVirtued].Value)
                availableTargets.RemoveAll(unit => unit.Character.IsVirtued);

        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                int index = Random.Range(0, availableTargets.Count);
                decision.TargetInfo.Targets.Add(availableTargets[index]);
                availableTargets.RemoveAt(index);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    int sideTargetIndex = Random.Range(0, availableTargets.Count);
                    decision.TargetInfo.Targets.Add(availableTargets[sideTargetIndex]);
                    return true;
                }
                return true;
            }
        }
        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "specific_combat_skill_id":
                    SpecificCombatSkillId = (string)dataSet["specific_combat_skill_id"];
                    break;
                case "is_exclusive_desire":
                    IsExclusiveDesire = (bool)dataSet[token.Key];
                    break;
                case "is_enemy_target_desire":
                    IsEnemyTargetDesire = (bool)dataSet[token.Key];
                    break;
                case "is_friendly_target_desire":
                    IsFriendlyTargetDesire = (bool)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in rank target desire: " + token.Key);
                    break;
            }
        }
    }
}
