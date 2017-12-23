using System.Collections.Generic;

public sealed class TargetSelectionStress : TargetSelectionDesire
{
    public TargetSelectionStress(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.Stress;

        GenerateFromDataSet(dataSet);
    }

    protected override List<FormationUnit> FilterTargets(FormationUnit performer, List<FormationUnit> possibleTargets)
    {
        var availableTargets = base.FilterTargets(performer, possibleTargets);

        availableTargets.RemoveAll(target => !target.Character.IsStressed);
        return availableTargets;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "is_greater_comparison":
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