using System.Collections.Generic;
using UnityEngine;

public class BonusInitiativeGuaranteed : BonusInitiativeDesire
{
    private int? MonstersMin { get; set; }
    private int? MonstersMax { get; set; }
    private int? MonstersSizeLimit { get; set; }

    public BonusInitiativeGuaranteed(Dictionary<string, object> dataSet)
    {
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