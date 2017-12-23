using System.Collections.Generic;
using UnityEngine;

public class BonusInitiativeAllyLastDamaged : BonusInitiativeDesire
{
    private string AllyBaseClass { get; set; }
    private bool IgnoreIfStun { get; set; }

    public BonusInitiativeAllyLastDamaged(Dictionary<string, object> dataSet)
    {
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