using System.Collections.Generic;

public sealed class BonusInitiativeLastSkill : BonusInitiativeDesire
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

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "last_combat_skill_id":
                    LastCombatSkill = (string)dataSet["last_combat_skill_id"];
                    break;
                case "monsters_size_limit":
                    MonstersSizeLimit = (int)(long)dataSet["monsters_size_limit"];
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}