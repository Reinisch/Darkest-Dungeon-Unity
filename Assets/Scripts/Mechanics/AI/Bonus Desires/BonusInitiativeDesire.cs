using System.Collections.Generic;
using UnityEngine;

public abstract class BonusInitiativeDesire
{
    public string CombatSkillOverride { get; private set; }
    public bool IsRoundStart { get; private set; }
    public bool IsRoundInProgress { get; private set; }
    public bool IsRoundFinish { get; private set; }
    public bool IsPostTurn { get; private set; }

    public abstract bool CheckBonusInitiative(FormationUnit performer);

    protected virtual void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
            ProcessBaseDataToken(token);
    }

    protected void ProcessBaseDataToken(KeyValuePair<string, object> token)
    {
        switch (token.Key)
        {
            case "combat_skill_id_override":
                CombatSkillOverride = (string)token.Value;
                break;
            case "is_round_start":
                IsRoundStart = (bool)token.Value;
                break;
            case "is_round_in_progress":
                IsRoundInProgress = (bool)token.Value;
                break;
            case "is_round_finish":
                IsRoundFinish = (bool)token.Value;
                break;
            case "is_pre_turn":
                break;
            case "is_post_turn":
                IsPostTurn = (bool)token.Value;
                break;
            default:
                Debug.LogError("Unknown token in bonus initiative desire: " + token.Key + " Type: " + this);
                break;
        }
    }
}