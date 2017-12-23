using System.Collections.Generic;
using UnityEngine;

public class BonusInitiativeHpRatio : BonusInitiativeDesire
{
    private float Threshold { get; set; }
    private bool IsUnderThreshold { get; set; }
    private int? HeroesMin { get; set; }
    private int? HeroesMax { get; set; }

    public BonusInitiativeHpRatio(Dictionary<string, object> dataSet)
    {
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

        if (IsUnderThreshold && performer.Character.HealthRatio <= Threshold)
            return true;
        if (!IsUnderThreshold && performer.Character.HealthRatio >= Threshold)
            return true;

        return false;
    }

    private void GenerateFromDataSet(Dictionary<string, object> dataSet)
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