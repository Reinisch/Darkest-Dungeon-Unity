using System.Collections.Generic;
using UnityEngine;

public class BonusInitiativeLastSkill : BonusInitiativeDesire
{
    private string LastCombatSkill { get; set; }
    private int? MonstersSizeLimit { get; set; }

    public BonusInitiativeLastSkill(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool CheckBonusInitiative(FormationUnit performer)
    {
        if (MonstersSizeLimit != null)
            if (MonstersSizeLimit.Value < RaidSceneManager.BattleGround.MonsterSize)
                return false;

        if (LastCombatSkill == null || RaidSceneManager.BattleGround.LastSkillUsed == null
            || RaidSceneManager.BattleGround.LastSkillUsed != LastCombatSkill)
            return false;

        RaidSceneManager.BattleGround.LastSkillUsed = null;

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