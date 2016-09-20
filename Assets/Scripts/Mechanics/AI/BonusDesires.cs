using UnityEngine;
using System.Collections.Generic;

public enum BonusDesireType { AllyLastDamaged, Guaranteed, LastSkill, AllyClassCount, Death, HpRationThreshold }

public abstract class BonusInitiativeDesire
{
    public string CombatSkillOverride { get; set; }
    public bool IsRoundStart { get; set; }
    public bool IsRoundInProgress { get; set; }
    public bool IsRoundFinish { get; set; }
    public bool IsPreTurn { get; set; }
    public bool IsPostTurn { get; set; }

    public BonusDesireType Type { get; set; }

    public abstract bool CheckBonusInitiative(FormationUnit performer);
    public abstract void GenerateFromDataSet(Dictionary<string, object> dataSet);
}

public class BonusInitiativeHpRatio : BonusInitiativeDesire
{
    public float Threshold { get; set; }
    public bool IsUnderThreshold { get; set; }
    public int? HeroesMin { get; set; }
    public int? HeroesMax { get; set; }

    public BonusInitiativeHpRatio()
    {
        Type = BonusDesireType.HpRationThreshold;

        HeroesMin = 0;
        HeroesMax = 4;
    }
    public BonusInitiativeHpRatio(Dictionary<string, object> dataSet)
    {
        Type = BonusDesireType.HpRationThreshold;

        HeroesMin = 0;
        HeroesMax = 4;

        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if (HeroesMin.HasValue)
            if (HeroesMin.Value > RaidSceneManager.BattleGround.HeroNumber)
                return false;
        if (HeroesMax.HasValue)
            if (HeroesMax.Value < RaidSceneManager.BattleGround.HeroNumber)
                return false;

        if (IsUnderThreshold && performer.Character.Health.ValueRatio <= Threshold)
            return true;
        if (!IsUnderThreshold && performer.Character.Health.ValueRatio >= Threshold)
            return true;

        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id_override":
                    CombatSkillOverride = (string)dataSet["combat_skill_id_override"];
                    break;
                case "is_round_start":
                    IsRoundStart = (bool)dataSet["is_round_start"];
                    break;
                case "is_round_in_progress":
                    IsRoundInProgress = (bool)dataSet["is_round_in_progress"];
                    break;
                case "is_round_finish":
                    IsRoundFinish = (bool)dataSet["is_round_finish"];
                    break;
                case "is_pre_turn":
                    IsPreTurn = (bool)dataSet["is_pre_turn"];
                    break;
                case "is_post_turn":
                    IsPostTurn = (bool)dataSet["is_post_turn"];
                    break;
                case "heroes_min":
                    HeroesMin = (int)(long)dataSet[token.Key];
                    break;
                case "heroes_max":
                    HeroesMax = (int)(long)dataSet[token.Key];
                    break;
                case "health_ratio_threshold":
                    Threshold = (float)(double)dataSet[token.Key];
                    break;
                case "is_under_threshold":
                    IsUnderThreshold = (bool)dataSet[token.Key];
                    break;
                default:
                    Debug.LogError("Unknown token in guaranteed bonus initiative: " + token.Key);
                    break;
            }
        }
    }
}
public class BonusInitiativeDeath : BonusInitiativeDesire
{
    public BonusInitiativeDeath()
    {
        Type = BonusDesireType.Death;
    }
    public BonusInitiativeDeath(Dictionary<string, object> dataSet)
    {
        Type = BonusDesireType.Death;

        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if (Mathf.RoundToInt(performer.Character.Health.CurrentValue) == 0)
            return true;

        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id_override":
                    CombatSkillOverride = (string)dataSet["combat_skill_id_override"];
                    break;
                case "is_round_start":
                    IsRoundStart = (bool)dataSet["is_round_start"];
                    break;
                case "is_round_in_progress":
                    IsRoundInProgress = (bool)dataSet["is_round_in_progress"];
                    break;
                case "is_round_finish":
                    IsRoundFinish = (bool)dataSet["is_round_finish"];
                    break;
                case "is_pre_turn":
                    IsPreTurn = (bool)dataSet["is_pre_turn"];
                    break;
                case "is_post_turn":
                    IsPostTurn = (bool)dataSet["is_post_turn"];
                    break;
                default:
                    Debug.LogError("Unknown token in death bonus initiative: " + token.Key);
                    break;
            }
        }
    }
}
public class BonusInitiativeGuaranteed : BonusInitiativeDesire
{
    public int? MonstersMin { get; set; }
    public int? MonstersMax { get; set; }
    public int? MonstersSizeLimit { get; set; }

    public BonusInitiativeGuaranteed()
    {
        Type = BonusDesireType.Guaranteed;
    }
    public BonusInitiativeGuaranteed(Dictionary<string, object> dataSet)
    {
        Type = BonusDesireType.Guaranteed;

        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if (MonstersMin != null)
            if (MonstersMin.Value > RaidSceneManager.BattleGround.MonsterNumber)
                return false;
        if (MonstersMax != null)
            if (MonstersMax.Value < RaidSceneManager.BattleGround.MonsterNumber)
                return false;
        if (MonstersSizeLimit != null)
            if (MonstersSizeLimit.Value < RaidSceneManager.BattleGround.MonsterSize)
                return false;

        return true;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id_override":
                    CombatSkillOverride = (string)dataSet["combat_skill_id_override"];
                    break;
                case "is_round_start":
                    IsRoundStart = (bool)dataSet["is_round_start"];
                    break;
                case "is_round_in_progress":
                    IsRoundInProgress = (bool)dataSet["is_round_in_progress"];
                    break;
                case "is_round_finish":
                    IsRoundFinish = (bool)dataSet["is_round_finish"];
                    break;
                case "is_pre_turn":
                    IsPreTurn = (bool)dataSet["is_pre_turn"];
                    break;
                case "is_post_turn":
                    IsPostTurn = (bool)dataSet["is_post_turn"];
                    break;
                case "monsters_min":
                    MonstersMin = (int)(long)dataSet["monsters_min"];
                    break;
                case "monsters_max":
                    MonstersMax = (int)(long)dataSet["monsters_max"];
                    break;
                case "monsters_size_limit":
                    MonstersSizeLimit = (int)(long)dataSet["monsters_size_limit"];
                    break;
                default:
                    Debug.LogError("Unknown token in guaranteed bonus initiative: " + token.Key);
                    break;
            }
        }
    }
}
public class BonusInitiativeLastSkill : BonusInitiativeDesire
{
    public string LastCombatSkill { get; set; }
    public int? MonstersSizeLimit { get; set; }

    public BonusInitiativeLastSkill()
    {
        Type = BonusDesireType.LastSkill;
    }
    public BonusInitiativeLastSkill(Dictionary<string, object> dataSet)
    {
        Type = BonusDesireType.LastSkill;

        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if (MonstersSizeLimit != null)
            if (MonstersSizeLimit.Value < RaidSceneManager.BattleGround.MonsterSize)
                return false;

        if (LastCombatSkill == null || RaidSceneManager.BattleGround.LastSkillUsed == null || RaidSceneManager.BattleGround.LastSkillUsed != LastCombatSkill)
            return false;

        RaidSceneManager.BattleGround.LastSkillUsed = null;

        return true;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id_override":
                    CombatSkillOverride = (string)dataSet["combat_skill_id_override"];
                    break;
                case "is_round_start":
                    IsRoundStart = (bool)dataSet["is_round_start"];
                    break;
                case "is_round_in_progress":
                    IsRoundInProgress = (bool)dataSet["is_round_in_progress"];
                    break;
                case "is_round_finish":
                    IsRoundFinish = (bool)dataSet["is_round_finish"];
                    break;
                case "is_pre_turn":
                    IsPreTurn = (bool)dataSet["is_pre_turn"];
                    break;
                case "is_post_turn":
                    IsPostTurn = (bool)dataSet["is_post_turn"];
                    break;
                case "last_combat_skill_id":
                    LastCombatSkill = (string)dataSet["last_combat_skill_id"];
                    break;
                case "monsters_size_limit":
                    MonstersSizeLimit = (int)(long)dataSet["monsters_size_limit"];
                    break;
                default:
                    Debug.LogError("Unknown token in last skill bonus initiative: " + token.Key);
                    break;
            }
        }
    }
}
public class BonusInitiativeAllyClassCount : BonusInitiativeDesire
{
    public string AllyBaseClass { get; set; }
    public int? AllyCountMin { get; set; }
    public int? AllyCountMax { get; set; }
    public int? MonstersMin { get; set; }
    public int? MonstersMax { get; set; }

    public BonusInitiativeAllyClassCount()
    {
        Type = BonusDesireType.AllyClassCount;
    }
    public BonusInitiativeAllyClassCount(Dictionary<string, object> dataSet)
    {
        Type = BonusDesireType.AllyClassCount;

        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if (MonstersMin != null)
            if (MonstersMin.Value > RaidSceneManager.BattleGround.MonsterNumber)
                return false;
        if (MonstersMax != null)
            if (MonstersMax.Value < RaidSceneManager.BattleGround.MonsterNumber)
                return false;

        int allyCount = RaidSceneManager.BattleGround.monsterFormation.party.Units.FindAll(unit => unit.Character.Class == AllyBaseClass).Count;
        if (allyCount == 0) return false;

        if (AllyCountMin != null)
            if (AllyCountMin.Value > allyCount)
                return false;
        if (AllyCountMax != null)
            if (AllyCountMax.Value < allyCount)
                return false;

        return true;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id_override":
                    CombatSkillOverride = (string)dataSet["combat_skill_id_override"];
                    break;
                case "is_round_start":
                    IsRoundStart = (bool)dataSet["is_round_start"];
                    break;
                case "is_round_in_progress":
                    IsRoundInProgress = (bool)dataSet["is_round_in_progress"];
                    break;
                case "is_round_finish":
                    IsRoundFinish = (bool)dataSet["is_round_finish"];
                    break;
                case "is_pre_turn":
                    IsPreTurn = (bool)dataSet["is_pre_turn"];
                    break;
                case "is_post_turn":
                    IsPostTurn = (bool)dataSet["is_post_turn"];
                    break;
                case "ally_base_class_id":
                    AllyBaseClass = (string)dataSet["ally_base_class_id"];
                    break;
                case "ally_count_min":
                    AllyCountMin = (int)(long)dataSet["ally_count_min"];
                    break;
                case "ally_count_max":
                    AllyCountMax = (int)(long)dataSet["ally_count_max"];
                    break;
                case "monsters_min":
                    MonstersMin = (int)(long)dataSet["monsters_min"];
                    break;
                case "monsters_max":
                    MonstersMax = (int)(long)dataSet["monsters_max"];
                    break;
                default:
                    Debug.LogError("Unknown token in ally class count bonus initiative: " + token.Key);
                    break;
            }
        }
    }
}
public class BonusInitiativeAllyLastDamaged : BonusInitiativeDesire
{
    public string AllyBaseClass { get; set; }
    public bool IgnoreIfStun { get; set; }

    public BonusInitiativeAllyLastDamaged()
    {
        Type = BonusDesireType.AllyLastDamaged;
    }
    public BonusInitiativeAllyLastDamaged(Dictionary<string, object> dataSet)
    {
        Type = BonusDesireType.AllyLastDamaged;

        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if(IgnoreIfStun && performer.Character[StatusType.Stun].IsApplied)
            return false;

        if (AllyBaseClass != null && RaidSceneManager.BattleGround.LastDamaged.Contains(AllyBaseClass))
        {
            RaidSceneManager.BattleGround.LastDamaged.Clear();
            return true;
        }

        return false;
    }
    public override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id_override":
                    CombatSkillOverride = (string)dataSet["combat_skill_id_override"];
                    break;
                case "is_round_start":
                    IsRoundStart = (bool)dataSet["is_round_start"];
                    break;
                case "is_round_in_progress":
                    IsRoundInProgress = (bool)dataSet["is_round_in_progress"];
                    break;
                case "is_round_finish":
                    IsRoundFinish = (bool)dataSet["is_round_finish"];
                    break;
                case "is_pre_turn":
                    IsPreTurn = (bool)dataSet["is_pre_turn"];
                    break;
                case "is_post_turn":
                    IsPostTurn = (bool)dataSet["is_post_turn"];
                    break;
                case "ally_base_class_id":
                    AllyBaseClass = (string)dataSet["ally_base_class_id"];
                    break;
                case "ignore_if_stun":
                    IgnoreIfStun = (bool)dataSet["ignore_if_stun"];
                    break;
                default:
                    Debug.LogError("Unknown token in ally last damaged bonus initiative: " + token.Key);
                    break;
            }
        }
    }
}
