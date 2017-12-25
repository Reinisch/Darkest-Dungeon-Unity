using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionAllyDead : SkillSelectionDesire
{
    private bool FirstInitiativeOnly { get; set; }
    private string CombatSkillId { get; set; }
    private string AllyBaseClassId { get; set; }

    public SkillSelectionAllyDead(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
            return false;

        if (Restrictions[SkillSelectRestriction.MonstersMin] != null)
            if (Restrictions[SkillSelectRestriction.MonstersMin].Value > RaidSceneManager.BattleGround.MonsterNumber)
                return false;
        if (Restrictions[SkillSelectRestriction.MonstersMax] != null)
            if (Restrictions[SkillSelectRestriction.MonstersMax].Value < RaidSceneManager.BattleGround.MonsterNumber)
                return false;

        if (performer.Party.Units.Find(unit => unit.Character.Class == AllyBaseClassId) != null)
            return false;

        var monster = performer.Character as Monster;
        var availableSkills = monster.Data.CombatSkills.FindAll(skill =>
            BattleSolver.IsSkillUsable(performer, skill) && skill.Id == CombatSkillId
            && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null);
        if (availableSkills.Count > 0)
        {
            decision.Decision = BrainDecisionType.Perform;
            decision.SelectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
            decision.TargetInfo.Targets = BattleSolver.GetSkillAvailableTargets(performer, decision.SelectedSkill);
            decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.IsSelfTarget ?
                SkillTargetType.Self : decision.SelectedSkill.TargetRanks.IsSelfFormation ?
                    SkillTargetType.Party : SkillTargetType.Enemy;

            var availableTargetDesires = new List<TargetSelectionDesire>(monster.Brain.TargetDesireSet);

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
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet["combat_skill_id"];
                    break;
                case "ally_base_class_id":
                    AllyBaseClassId = (string)dataSet["ally_base_class_id"];
                    break;
                case "first_initiative_only":
                    FirstInitiativeOnly = (bool)dataSet[token.Key];
                    break;
                case "monsters_min":
                    Restrictions[SkillSelectRestriction.MonstersMin] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_max":
                    Restrictions[SkillSelectRestriction.MonstersMax] = (int)(long)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in ally dead skill desire: " + token.Key);
                    break;
            }
        }
    }
}