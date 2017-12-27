using System.Collections.Generic;

public sealed class BonusInitiativeHpRatio : BonusInitiativeDesire
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

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
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
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}