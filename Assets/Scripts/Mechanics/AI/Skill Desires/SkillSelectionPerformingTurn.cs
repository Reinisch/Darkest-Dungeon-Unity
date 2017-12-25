using System.Collections.Generic;

public sealed class SkillSelectionPerformingTurn : SkillSelectionDesire
{
    private string CombatSkillId { get; set; }
    private int PerformingTurn { get; set; }

    public SkillSelectionPerformingTurn(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    protected override bool IsRestricted(FormationUnit performer)
    {
        if (base.IsRestricted(performer))
            return true;

        if (PerformingTurn != RaidSceneManager.BattleGround.Round.RoundNumber)
            return true;

        return false;
    }

    protected override bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if (!base.IsValidSkill(performer, skill))
            return false;

        return skill.Id == CombatSkillId;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet["combat_skill_id"];
                    break;
                case "performing_turn":
                    PerformingTurn = (int)(long)dataSet["performing_turn"];
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}