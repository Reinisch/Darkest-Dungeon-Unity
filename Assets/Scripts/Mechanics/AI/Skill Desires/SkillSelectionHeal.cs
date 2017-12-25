using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionHeal : SkillSelectionDesire
{
    private string CombatSkillId { get; set; }
    private float HpRatioThreshold { get; set; }
    private bool FirstInitiativeOnly { get; set; }

    public SkillSelectionHeal(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
            return false;

        if (Restrictions[SkillSelectRestriction.MonstersSizeMin] != null)
            if (Restrictions[SkillSelectRestriction.MonstersSizeMin].Value > RaidSceneManager.BattleGround.MonsterSize)
                return false;
        if (Restrictions[SkillSelectRestriction.MonstersSizeMax] != null)
            if (Restrictions[SkillSelectRestriction.MonstersSizeMax].Value < RaidSceneManager.BattleGround.MonsterSize)
                return false;

        var monster = performer.Character as Monster;

        var availableSkills = CombatSkillId == null || CombatSkillId == "" ?
            monster.Data.CombatSkills.FindAll(skill => skill.Heal != null && BattleSolver.IsSkillUsable(performer, skill)
                                                       && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null) :
            monster.Data.CombatSkills.FindAll(skill => skill.Id == CombatSkillId
                                                       && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null);

        if (availableSkills.Count > 0)
        {
            decision.Decision = BrainDecisionType.Perform;
            decision.SelectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
            decision.TargetInfo.Targets = BattleSolver.GetSkillAvailableTargets(performer, decision.SelectedSkill).
                FindAll(target => target.Character.HealthRatio < HpRatioThreshold);
            if (decision.TargetInfo.Targets.Count == 0)
                return false;

            decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.IsSelfTarget ?
                SkillTargetType.Self : decision.SelectedSkill.TargetRanks.IsSelfFormation ?
                    SkillTargetType.Party : SkillTargetType.Enemy;

            var availableTargetDesires = monster.Brain.TargetDesireSet.FindAll(desire => desire.Type == TargetDesireType.Health);

            while (availableTargetDesires.Count > 0)
            {
                TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                if (desire.SelectTarget(performer, decision))
                    return true;
                else
                    availableTargetDesires.Remove(desire);
            }
            return false;
        }
        return false;
    }

    private void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)((double)dataSet["base_chance"] * 100);
                    break;
                case "hp_ratio_treshold":
                    HpRatioThreshold = (float)(double)dataSet["hp_ratio_treshold"];
                    break;
                case "first_initiative_only":
                    FirstInitiativeOnly = (bool)dataSet[token.Key];
                    break;
                case "monsters_size_min":
                    Restrictions[SkillSelectRestriction.MonstersSizeMin] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_size_max":
                    Restrictions[SkillSelectRestriction.MonstersSizeMax] = (int)(long)dataSet[token.Key];
                    break;
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in heal skill desire: " + token.Key);
                    break;
            }
        }
    }
}