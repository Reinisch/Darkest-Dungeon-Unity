using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionFillEmptyCaptor : SkillSelectionDesire
{
    private bool CanTargetDeathsDoor { get; set; }
    private bool CanTargetLastHero { get; set; }
    private bool FirstInitiativeOnly { get; set; }

    public SkillSelectionFillEmptyCaptor(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
            return false;
        if (CanTargetDeathsDoor == false && RaidSceneManager.BattleGround.NonDeathsDoorHeroes == 0)
            return false;
        if (CanTargetLastHero == false && RaidSceneManager.BattleGround.HeroNumber == 1)
            return false;
        if (performer.Party.Units.Find(unit => unit.Character.IsMonster &&
                                               ((Monster)unit.Character).Data.EmptyCaptor != null) == null)
            return false;

        var monster = performer.Character as Monster;
        var availableSkills = monster.Data.CombatSkills.FindAll(skill => skill.Effects.Find(effect =>
                                                                             effect.SubEffects.Find(subeffect =>
                                                                                 subeffect is CaptureEffect) != null) != null && BattleSolver.IsSkillUsable(performer, skill) &&
                                                                         performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null);

        if (availableSkills.Count > 0)
        {
            decision.Decision = BrainDecisionType.Perform;
            decision.SelectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
            decision.TargetInfo.Targets = BattleSolver.GetSkillAvailableTargets(performer, decision.SelectedSkill);
            if (CanTargetDeathsDoor == false)
                decision.TargetInfo.Targets.RemoveAll(unit => unit.Character.AtDeathsDoor);

            if (decision.TargetInfo.Targets.Count == 0)
                return false;
            decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.IsSelfTarget ?
                SkillTargetType.Self : decision.SelectedSkill.TargetRanks.IsSelfFormation ?
                    SkillTargetType.Party : SkillTargetType.Enemy;

            var availableTargetDesires = new List<TargetSelectionDesire>(monster.Brain.TargetDesireSet)
                .FindAll(desire => desire.Type == TargetDesireType.FillEmptyCaptor);

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
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "can_target_deaths_door":
                    CanTargetDeathsDoor = (bool)dataSet["can_target_deaths_door"];
                    break;
                case "can_target_last_hero":
                    CanTargetLastHero = (bool)dataSet["can_target_last_hero"];
                    break;
                case "first_initiative_only":
                    FirstInitiativeOnly = (bool)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in fill empty captor skill desire: " + token.Key);
                    break;
            }
        }
    }
}