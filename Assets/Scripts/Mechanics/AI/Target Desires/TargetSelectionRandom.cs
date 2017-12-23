using System.Collections.Generic;

public sealed class TargetSelectionRandom : TargetSelectionDesire
{
    public TargetSelectionRandom(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.Random;

        GenerateFromDataSet(dataSet);
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "can_target_deaths_door":
                    Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)token.Value;
                    break;
                case "can_target_last_hero":
                    Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)token.Value;
                    break;
                case "can_target_not_overstressed":
                    Parameters[TargetSelectParameter.CanTargetNotOverstressed] = (bool)token.Value;
                    break;
                case "can_target_afflicted":
                    Parameters[TargetSelectParameter.CanTargetAfflicted] = (bool)token.Value;
                    break;
                case "can_target_virtued":
                    Parameters[TargetSelectParameter.CanTargetVirtued] = (bool)token.Value;
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}