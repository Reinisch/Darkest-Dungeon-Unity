using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SkillSelectionDesire : IProportionValue
{
    public virtual int Chance { get; set; }

    private readonly Dictionary<SkillSelectRestriction, int?> restrictions;

    protected SkillSelectionDesire()
    {
        restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction attribute in Enum.GetValues(typeof(SkillSelectRestriction)))
            restrictions.Add(attribute, null);
    }

    public bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (IsRestricted(performer))
            return false;

        var monster = (Monster)performer.Character;
        var availableSkills = monster.Data.CombatSkills.FindAll(skill => IsValidSkill(performer, skill));

        if (availableSkills.Count > 0)
        {
            decision.Decision = BrainDecisionType.Perform;
            decision.SelectedSkill = availableSkills[RandomSolver.Next(availableSkills.Count)];
            decision.TargetInfo.Targets = BattleSolver.GetSkillAvailableTargets(performer, decision.SelectedSkill);
            decision.TargetInfo.Targets.RemoveAll(target => !IsValidTarget(target));
            decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.SkillTargetType;

            if (decision.TargetInfo.Targets.Count == 0)
                return false;

            var availableTargetDesires = monster.Brain.TargetDesireSet.FindAll(IsValidTargetDesire);
            while (availableTargetDesires.Count > 0)
            {
                TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                if (desire.SelectTarget(performer, decision))
                    return true;

                availableTargetDesires.Remove(desire);
            }
            return false;
        }
        return false;
    }

    protected virtual bool IsRestricted(FormationUnit performer)
    {
        if (restrictions[SkillSelectRestriction.MonstersMin] != null)
            if (restrictions[SkillSelectRestriction.MonstersMin].Value > RaidSceneManager.BattleGround.MonsterNumber)
                return true;
        if (restrictions[SkillSelectRestriction.MonstersMax] != null)
            if (restrictions[SkillSelectRestriction.MonstersMax].Value < RaidSceneManager.BattleGround.MonsterNumber)
                return true;
        if (restrictions[SkillSelectRestriction.MonstersSizeMin] != null)
            if (restrictions[SkillSelectRestriction.MonstersSizeMin].Value > RaidSceneManager.BattleGround.MonsterSize)
                return true;
        if (restrictions[SkillSelectRestriction.MonstersSizeMax] != null)
            if (restrictions[SkillSelectRestriction.MonstersSizeMax].Value < RaidSceneManager.BattleGround.MonsterSize)
                return true;
        if (restrictions[SkillSelectRestriction.MarkedHeroesMin] != null)
            if (restrictions[SkillSelectRestriction.MarkedHeroesMin].Value > RaidSceneManager.BattleGround.MarkedHeroes)
                return true;
        if (restrictions[SkillSelectRestriction.MarkedHeroesMax] != null)
            if (restrictions[SkillSelectRestriction.MarkedHeroesMax].Value < RaidSceneManager.BattleGround.MarkedHeroes)
                return true;
        if (restrictions[SkillSelectRestriction.NonVirtuedHeroesMin] != null)
            if (restrictions[SkillSelectRestriction.NonVirtuedHeroesMin].Value > RaidSceneManager.BattleGround.NonVirtuedHeroes)
                return true;
        if (restrictions[SkillSelectRestriction.ControlCountMin] != null)
            if (restrictions[SkillSelectRestriction.ControlCountMin].Value > RaidSceneManager.BattleGround.ControlCount)
                return true;
        if (restrictions[SkillSelectRestriction.ControlCountMax] != null)
            if (restrictions[SkillSelectRestriction.ControlCountMax].Value < RaidSceneManager.BattleGround.ControlCount)
                return true;
        if (restrictions[SkillSelectRestriction.HeroesMin] != null)
            if (restrictions[SkillSelectRestriction.HeroesMin].Value > RaidSceneManager.BattleGround.HeroNumber)
                return true;
        if (restrictions[SkillSelectRestriction.VirtuedHeroesMax] != null)
            if (restrictions[SkillSelectRestriction.VirtuedHeroesMax].Value < RaidSceneManager.BattleGround.VirtuedHeroes)
                return true;
        if (restrictions[SkillSelectRestriction.GuardedMonstersMin] != null)
            if (restrictions[SkillSelectRestriction.GuardedMonstersMin].Value > RaidSceneManager.BattleGround.GuardedMonsters)
                return true;
        if (restrictions[SkillSelectRestriction.GuardedMonstersMax] != null)
            if (restrictions[SkillSelectRestriction.GuardedMonstersMax].Value < RaidSceneManager.BattleGround.GuardedMonsters)
                return true;

        return false;
    }

    protected virtual bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if(!BattleSolver.IsSkillUsable(performer, skill))
            return false;

        if (performer.CombatInfo.SkillCooldowns.Any(cooldown => cooldown.SkillId == skill.Id))
            return false;

        return true;
    }

    protected virtual bool IsValidTarget(FormationUnit target)
    {
        return true;
    }

    protected virtual bool IsValidTargetDesire(TargetSelectionDesire desire)
    {
        return true;
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
                Chance = (int)((double)token.Value * 100);
                break;
            case "marked_heroes_min":
                restrictions[SkillSelectRestriction.MarkedHeroesMin] = (int)(long)token.Value;
                break;
            case "marked_heroes_max":
                restrictions[SkillSelectRestriction.MarkedHeroesMax] = (int)(long)token.Value;
                break;
            case "monsters_size_min":
                restrictions[SkillSelectRestriction.MonstersSizeMin] = (int)(long)token.Value;
                break;
            case "monsters_size_max":
                restrictions[SkillSelectRestriction.MonstersSizeMax] = (int)(long)token.Value;
                break;
            case "non_virtued_heroes_min":
                restrictions[SkillSelectRestriction.NonVirtuedHeroesMin] = (int)(long)token.Value;
                break;
            case "virtued_heroes_max":
                restrictions[SkillSelectRestriction.VirtuedHeroesMax] = (int)(long)token.Value;
                break;
            case "non_deaths_door_heroes_min":
                restrictions[SkillSelectRestriction.NonDeathsDorrHeroesMin] = (int)(long)token.Value;
                break;
            case "control_count_min":
                restrictions[SkillSelectRestriction.ControlCountMin] = (int)(long)token.Value;
                break;
            case "control_count_max":
                restrictions[SkillSelectRestriction.ControlCountMax] = (int)(long)token.Value;
                break;
            case "heroes_min":
                restrictions[SkillSelectRestriction.HeroesMin] = (int)(long)token.Value;
                break;
            case "monsters_min":
                restrictions[SkillSelectRestriction.MonstersMin] = (int)(long)token.Value;
                break;
            case "monsters_max":
                restrictions[SkillSelectRestriction.MonstersMax] = (int)(long)token.Value;
                break;
            case "guarded_monsters_min":
                restrictions[SkillSelectRestriction.GuardedMonstersMin] = (int)(long)token.Value;
                break;
            case "guarded_monsters_max":
                restrictions[SkillSelectRestriction.GuardedMonstersMax] = (int)(long)token.Value;
                break;
            default:
                Debug.LogError("Unknown token in skill desire: " + token.Key + " Type: " + this);
                break;
        }
    }
}