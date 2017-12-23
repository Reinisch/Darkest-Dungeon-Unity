using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionSpecific : SkillSelectionDesire
{
    public override int Chance
    {
        get
        {
            if (RaidSceneManager.Instanse != null)
                return base.Chance + PerRoundChance * (RaidSceneManager.BattleGround.Round.RoundNumber - 1);
            return base.Chance;
        }
        set
        {
            base.Chance = value;
        }
    }

    private string CombatSkillId { get; set; }
    private bool FirstInitiativeOnly { get; set; }
    private int PerRoundChance { get; set; }

    public SkillSelectionSpecific(Dictionary<string, object> dataSet)
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
        if (Restrictions[SkillSelectRestriction.MonstersSizeMin] != null)
            if (Restrictions[SkillSelectRestriction.MonstersSizeMin].Value > RaidSceneManager.BattleGround.MonsterSize)
                return false;
        if (Restrictions[SkillSelectRestriction.MonstersSizeMax] != null)
            if (Restrictions[SkillSelectRestriction.MonstersSizeMax].Value < RaidSceneManager.BattleGround.MonsterSize)
                return false;
        if (Restrictions[SkillSelectRestriction.MarkedHeroesMin] != null)
            if (Restrictions[SkillSelectRestriction.MarkedHeroesMin].Value > RaidSceneManager.BattleGround.MarkedHeroes)
                return false;
        if (Restrictions[SkillSelectRestriction.MarkedHeroesMax] != null)
            if (Restrictions[SkillSelectRestriction.MarkedHeroesMax].Value < RaidSceneManager.BattleGround.MarkedHeroes)
                return false;
        if (Restrictions[SkillSelectRestriction.NonVirtuedHeroesMin] != null)
            if (Restrictions[SkillSelectRestriction.NonVirtuedHeroesMin].Value > RaidSceneManager.BattleGround.NonVirtuedHeroes)
                return false;
        if (Restrictions[SkillSelectRestriction.ControlCountMin] != null)
            if (Restrictions[SkillSelectRestriction.ControlCountMin].Value > RaidSceneManager.BattleGround.ControlCount)
                return false;
        if (Restrictions[SkillSelectRestriction.ControlCountMax] != null)
            if (Restrictions[SkillSelectRestriction.ControlCountMax].Value < RaidSceneManager.BattleGround.ControlCount)
                return false;
        if (Restrictions[SkillSelectRestriction.HeroesMin] != null)
            if (Restrictions[SkillSelectRestriction.HeroesMin].Value > RaidSceneManager.BattleGround.HeroNumber)
                return false;
        if (Restrictions[SkillSelectRestriction.VirtuedHeroesMax] != null)
            if (Restrictions[SkillSelectRestriction.VirtuedHeroesMax].Value < RaidSceneManager.BattleGround.VirtuedHeroes)
                return false;
        if (Restrictions[SkillSelectRestriction.GuardedMonstersMin] != null)
            if (Restrictions[SkillSelectRestriction.GuardedMonstersMin].Value > RaidSceneManager.BattleGround.GuardedMonsters)
                return false;
        if (Restrictions[SkillSelectRestriction.GuardedMonstersMax] != null)
            if (Restrictions[SkillSelectRestriction.GuardedMonstersMax].Value < RaidSceneManager.BattleGround.GuardedMonsters)
                return false;

        var monster = performer.Character as Monster;
        var availableSkills = monster.Data.CombatSkills.FindAll(skill =>
            skill.Id == CombatSkillId && BattleSolver.IsSkillUsable(performer, skill) &&
            performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null);

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
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet["combat_skill_id"];
                    break;
                case "hp_ratio_treshold":
                    break;
                case "first_initiative_only":
                    FirstInitiativeOnly = (bool)dataSet[token.Key];
                    break;
                case "marked_heroes_min":
                    Restrictions[SkillSelectRestriction.MarkedHeroesMin] = (int)(long)dataSet[token.Key];
                    break;
                case "marked_heroes_max":
                    Restrictions[SkillSelectRestriction.MarkedHeroesMax] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_size_min":
                    Restrictions[SkillSelectRestriction.MonstersSizeMin] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_size_max":
                    Restrictions[SkillSelectRestriction.MonstersSizeMax] = (int)(long)dataSet[token.Key];
                    break;
                case "non_virtued_heroes_min":
                    Restrictions[SkillSelectRestriction.NonVirtuedHeroesMin] = (int)(long)dataSet[token.Key];
                    break;
                case "virtued_heroes_max":
                    Restrictions[SkillSelectRestriction.VirtuedHeroesMax] = (int)(long)dataSet[token.Key];
                    break;
                case "non_deaths_door_heroes_min":
                    Restrictions[SkillSelectRestriction.NonDeathsDorrHeroesMin] = (int)(long)dataSet[token.Key];
                    break;
                case "control_count_min":
                    Restrictions[SkillSelectRestriction.ControlCountMin] = (int)(long)dataSet[token.Key];
                    break;
                case "control_count_max":
                    Restrictions[SkillSelectRestriction.ControlCountMax] = (int)(long)dataSet[token.Key];
                    break;
                case "heroes_min":
                    Restrictions[SkillSelectRestriction.HeroesMin] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_min":
                    Restrictions[SkillSelectRestriction.MonstersMin] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_max":
                    Restrictions[SkillSelectRestriction.MonstersMax] = (int)(long)dataSet[token.Key];
                    break;
                case "guarded_monsters_min":
                    Restrictions[SkillSelectRestriction.GuardedMonstersMin] = (int)(long)dataSet[token.Key];
                    break;
                case "guarded_monsters_max":
                    Restrictions[SkillSelectRestriction.GuardedMonstersMax] = (int)(long)dataSet[token.Key];
                    break;
                case "per_round_chance":
                    PerRoundChance = (int)(double)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in specific skill desire: " + token.Key);
                    break;
            }
        }
    }
}