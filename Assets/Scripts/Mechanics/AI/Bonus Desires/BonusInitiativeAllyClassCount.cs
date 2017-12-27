using System.Collections.Generic;

public sealed class BonusInitiativeAllyClassCount : BonusInitiativeDesire
{
    private string AllyBaseClass { get; set; }
    private int? AllyCountMin { get; set; }
    private int? AllyCountMax { get; set; }
    private int? MonstersMin { get; set; }
    private int? MonstersMax { get; set; }

    public BonusInitiativeAllyClassCount(Dictionary<string, object> dataSet)
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

        int allyCount = RaidSceneManager.BattleGround.MonsterParty.Units.FindAll(unit => unit.Character.Class == AllyBaseClass).Count;
        if (allyCount == 0) return false;

        if (AllyCountMin > allyCount)
            return false;
        if (AllyCountMax < allyCount)
            return false;

        return true;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
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
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}