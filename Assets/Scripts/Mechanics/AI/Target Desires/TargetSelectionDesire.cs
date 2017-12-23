using UnityEngine;
using System.Collections.Generic;

public abstract class TargetSelectionDesire : IProportionValue
{
    public TargetDesireType Type { get; protected set; }
    public int Chance { get; set; }

    protected Dictionary<TargetSelectParameter, bool?> Parameters { get; private set; }

    private string SpecificCombatSkillId { get; set; }
    private bool IsEnemyTargetDesire { get; set; }
    private bool IsFriendlyTargetDesire { get; set; }

    protected TargetSelectionDesire()
    {
        Parameters = new Dictionary<TargetSelectParameter, bool?>();
        foreach (TargetSelectParameter selectionAttribute in System.Enum.GetValues(typeof(TargetSelectParameter)))
            Parameters.Add(selectionAttribute, null);
    }

    public virtual bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (!(SpecificCombatSkillId == "" || SpecificCombatSkillId == decision.SelectedSkill.Id))
            return false;

        if (decision.SelectedSkill.TargetRanks.IsSelfFormation && !IsFriendlyTargetDesire)
            return false;

        if (!(decision.SelectedSkill.TargetRanks.IsSelfFormation ||
            decision.SelectedSkill.TargetRanks.IsSelfTarget) && !IsEnemyTargetDesire)
            return false;

        return ChooseTargets(FilterTargets(performer, decision.TargetInfo.Targets), decision);
    }

    protected virtual List<FormationUnit> FilterTargets(FormationUnit performer, List<FormationUnit> possibleTargets)
    {
        var availableTargets = new List<FormationUnit>(possibleTargets);

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

        return availableTargets;
    }

    protected virtual bool ChooseTargets(List<FormationUnit> availableTargets, MonsterBrainDecision decision)
    {
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

    protected virtual void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
            ProcessBaseDataToken(token);
    }

    protected void ProcessBaseDataToken(KeyValuePair<string, object> token)
    {
        switch (token.Key)
        {
            case "base_chance":
                Chance = (int)(double)token.Value;
                break;
            case "specific_combat_skill_id":
                SpecificCombatSkillId = (string)token.Value;
                break;
            case "is_exclusive_desire":
                break;
            case "is_enemy_target_desire":
                IsEnemyTargetDesire = (bool)token.Value;
                break;
            case "is_friendly_target_desire":
                IsFriendlyTargetDesire = (bool)token.Value;
                break;
            default:
                Debug.LogError("Unknown token in target desire: " + token.Key + " Type: " + this);
                break;
        }
    }
}