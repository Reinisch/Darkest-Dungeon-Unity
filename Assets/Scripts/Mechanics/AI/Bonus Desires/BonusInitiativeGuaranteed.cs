using System.Collections.Generic;

public sealed class BonusInitiativeGuaranteed : BonusInitiativeDesire
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

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
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
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}