using UnityEngine;
using System.Collections.Generic;

public enum SkillDesireType
{
    Preferred, Specific, Status,AllyDead, AllyAlive,
    FillEmptyCaptor, PerformingTurn, Guard, Heal, Random
}

public enum SkillSelectRestriction
{ 
    MarkedHeroesMin, MarkedHeroesMax,
    MonstersSizeMin, MonstersSizeMax,
    HeroesMin,
    VirtuedHeroesMax,
    NonVirtuedHeroesMin,
    NonDeathsDorrHeroesMin,
    ControlCountMin, ControlCountMax,
    MonstersMin, MonstersMax,
    GuardedMonstersMin,
    GuardedMonstersMax,
}

public abstract class SkillSelectionDesire : IProportionValue
{
    public SkillDesireType Type { get; set; }
    public virtual int Chance { get; set; }

    protected Dictionary<SkillSelectRestriction, int?> Restrictions { get; set; }

    public abstract bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision);
    public abstract void GenerateFromDataSet(Dictionary<string, object> dataSet);
}

public class SkillSelectionRandom : SkillSelectionDesire
{
    public SkillSelectionRandom()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute 
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Random;
    }
    public SkillSelectionRandom(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Random;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        var monster = performer.Character as Monster;
        var availableSkills = monster.Data.CombatSkills.FindAll(skill => BattleSolver.IsSkillUsable(performer, skill)
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

            while(availableTargetDesires.Count > 0)
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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                default:
                    Debug.LogError("Unknown token in random skill desire: " + token.Key);
                    break;
            }
        }
    }
}

public class SkillSelectionPreferred : SkillSelectionDesire
{
    public SkillSelectionPreferred()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Preferred;
    }
    public SkillSelectionPreferred(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Preferred;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        var monster = performer.Character as Monster;
        if(monster.Data.PreferableSkill >= 0)
        {
            if(monster.Data.CombatSkills.Count > monster.Data.PreferableSkill)
            {
                var skill = monster.Data.CombatSkills[monster.Data.PreferableSkill];
                if (performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) != null)
                    return false;
                if(BattleSolver.IsSkillUsable(performer, skill))
                {
                    decision.Decision = BrainDecisionType.Perform;
                    decision.SelectedSkill = skill;
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
                }
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
                default:
                    Debug.LogError("Unknown token in preferred skill desire: " + token.Key);
                    break;
            }
        }
    }
}

public class SkillSelectionHeal : SkillSelectionDesire
{
    public string CombatSkillId { get; set; }
    public float HpRatioThreshold { get; set; }
    public bool FirstInitiativeOnly { get; set; }

    public SkillSelectionHeal()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Heal;
    }
    public SkillSelectionHeal(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Heal;

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
            && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null)
            ;

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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
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

public class SkillSelectionSpecific : SkillSelectionDesire
{
    public override int Chance
    {
        get
        {
            if (RaidSceneManager.Instanse != null)
                return base.Chance + PerRoundChance * (RaidSceneManager.BattleGround.Round.RoundNumber - 1);
            else
                return base.Chance;
        }
        set
        {
            base.Chance = value;
        }
    }

    public string CombatSkillId { get; set; }
    public float HpRatioThreshold { get; set; }
    public bool FirstInitiativeOnly { get; set; }
    public int PerRoundChance { get; set; }

    public SkillSelectionSpecific()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Specific;
    }
    public SkillSelectionSpecific(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Specific;

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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
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
                    HpRatioThreshold = (float)(double)dataSet["hp_ratio_treshold"];
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

public class SkillSelectionPerformingTurn : SkillSelectionDesire
{
    public string CombatSkillId { get; set; }
    public int PerformingTurn { get; set; }

    public SkillSelectionPerformingTurn()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.PerformingTurn;
    }
    public SkillSelectionPerformingTurn(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.PerformingTurn;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (PerformingTurn != RaidSceneManager.BattleGround.Round.RoundNumber)
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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
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
                case "performing_turn":
                    PerformingTurn = (int)(long)dataSet["performing_turn"];
                    break;
                case "marked_heroes_min":
                    Restrictions[SkillSelectRestriction.MarkedHeroesMin] = (int)(long)dataSet[token.Key];
                    break;
                case "marked_heroes_max":
                    Restrictions[SkillSelectRestriction.MarkedHeroesMax] = (int)(long)dataSet[token.Key];
                    break;
                case "virtued_heroes_max":
                    Restrictions[SkillSelectRestriction.VirtuedHeroesMax] = (int)(long)dataSet[token.Key];
                    break;
                case "guarded_monsters_min":
                    Restrictions[SkillSelectRestriction.GuardedMonstersMin] = (int)(long)dataSet[token.Key];
                    break;
                case "guarded_monsters_max":
                    Restrictions[SkillSelectRestriction.GuardedMonstersMax] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_size_min":
                    Restrictions[SkillSelectRestriction.MonstersSizeMin] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_size_max":
                    Restrictions[SkillSelectRestriction.MonstersSizeMax] = (int)(long)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in performing turn skill desire: " + token.Key);
                    break;
            }
        }
    }
}

public class SkillSelectionAllyAlive : SkillSelectionDesire
{
    public string CombatSkillId { get; set; }
    public string AllyBaseClassId { get; set; }

    public SkillSelectionAllyAlive()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.AllyAlive;
    }
    public SkillSelectionAllyAlive(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.AllyAlive;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (performer.Party.Units.Find(unit => unit.Character.Class == AllyBaseClassId) == null)
            return false;

        if (Restrictions[SkillSelectRestriction.MonstersSizeMin] != null)
            if (Restrictions[SkillSelectRestriction.MonstersSizeMin].Value > RaidSceneManager.BattleGround.MonsterSize)
                return false;
        if (Restrictions[SkillSelectRestriction.MonstersSizeMax] != null)
            if (Restrictions[SkillSelectRestriction.MonstersSizeMax].Value < RaidSceneManager.BattleGround.MonsterSize)
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

            var availableTargetDesires = monster.Brain.TargetDesireSet.FindAll(desire => desire.Type == TargetDesireType.AllyClass);
            while (availableTargetDesires.Count > 0)
            {
                TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                if (desire.SelectTarget(performer, decision))
                    return true;
                else
                    availableTargetDesires.Remove(desire);
            }

            availableTargetDesires = new List<TargetSelectionDesire>(monster.Brain.TargetDesireSet);
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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
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
                case "ally_base_class_id":
                    AllyBaseClassId = (string)dataSet["ally_base_class_id"];
                    break;
                case "guarded_monsters_min":
                    Restrictions[SkillSelectRestriction.GuardedMonstersMin] = (int)(long)dataSet[token.Key];
                    break;
                case "guarded_monsters_max":
                    Restrictions[SkillSelectRestriction.GuardedMonstersMax] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_size_min":
                    Restrictions[SkillSelectRestriction.MonstersSizeMin] = (int)(long)dataSet[token.Key];
                    break;
                case "monsters_size_max":
                    Restrictions[SkillSelectRestriction.MonstersSizeMax] = (int)(long)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in ally alive skill desire: " + token.Key);
                    break;
            }
        }
    }
}

public class SkillSelectionFillEmptyCaptor : SkillSelectionDesire
{
    public bool CanTargetDeathsDoor { get; set; }
    public bool CanTargetLastHero { get; set; }
    public bool FirstInitiativeOnly { get; set; }

    public SkillSelectionFillEmptyCaptor()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.FillEmptyCaptor;
    }
    public SkillSelectionFillEmptyCaptor(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.FillEmptyCaptor;

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
            (unit.Character as Monster).Data.EmptyCaptor != null) == null)
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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
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

public class SkillSelectionStatus : SkillSelectionDesire
{
    public StatusType EffectStatus { get; set; }
    public string CombatSkillId { get; set; }

    public SkillSelectionStatus()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Status;
    }
    public SkillSelectionStatus(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.Status;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        var monster = performer.Character as Monster;
        var availableSkills = CombatSkillId != null ? monster.Data.CombatSkills.FindAll(skill => skill.Id == CombatSkillId
            && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null) :
            monster.Data.CombatSkills.FindAll(skill => BattleSolver.IsSkillUsable(performer, skill)
            && skill.Effects.Count > 0 && skill.Effects.Find(effect =>
            effect.SubEffects.Find(subEffect => subEffect.Type == EffectSubType.StatBuff
            && (subEffect as CombatStatBuffEffect).TargetStatus == EffectStatus) != null) != null
            && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null);

        if (availableSkills.Count > 0)
        {
            decision.Decision = BrainDecisionType.Perform;
            decision.SelectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
            decision.TargetInfo.Targets = BattleSolver.GetSkillAvailableTargets(performer, decision.SelectedSkill).
                FindAll(unit => unit.Character.GetStatusEffect(EffectStatus).IsApplied);
            if (decision.TargetInfo.Targets.Count == 0)
                return false;

            decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.IsSelfTarget ?
                SkillTargetType.Self : decision.SelectedSkill.TargetRanks.IsSelfFormation ?
                SkillTargetType.Party : SkillTargetType.Enemy;

            var availableTargetDesires = new List<TargetSelectionDesire>(monster.Brain.TargetDesireSet)
                .FindAll(desire => desire.Type == TargetDesireType.Marked);

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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
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
                case "effect_key_status":
                    switch ((string)dataSet["effect_key_status"])
                    {
                        case "tagged":
                            EffectStatus = StatusType.Marked;
                            break;
                        case "poisoned":
                            EffectStatus = StatusType.Poison;
                            break;
                        case "bleeding":
                            EffectStatus = StatusType.Bleeding;
                            break;
                        case "stunned":
                            EffectStatus = StatusType.Stun;
                            break;
                        default:
                            Debug.LogError("Unknown key status in status skill desire: " + (string)dataSet["effect_key_status"]);
                            break;
                    }
                    break;
                default:
                    Debug.LogError("Unknown token in status skill desire: " + token.Key);
                    break;
            }
        }
    }
}

public class SkillSelectionAllyDead : SkillSelectionDesire
{
    public bool FirstInitiativeOnly { get; set; }
    public string CombatSkillId { get; set; }
    public string AllyBaseClassId { get; set; }

    public SkillSelectionAllyDead()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.AllyDead;
    }
    public SkillSelectionAllyDead(Dictionary<string, object> dataSet)
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);

        Type = SkillDesireType.AllyDead;

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
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
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