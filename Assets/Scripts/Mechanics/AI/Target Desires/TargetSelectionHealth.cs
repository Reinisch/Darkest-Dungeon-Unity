using System.Collections.Generic;

public sealed class TargetSelectionHealth : TargetSelectionDesire
{
    private string AllyBaseClassId { get; set; }
    
    public TargetSelectionHealth(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.Health;

        GenerateFromDataSet(dataSet);
    }

    protected override List<FormationUnit> FilterTargets(FormationUnit performer, List<FormationUnit> possibleTargets)
    {
        var availableTargets = base.FilterTargets(performer, possibleTargets);

        if (!string.IsNullOrEmpty(AllyBaseClassId))
            availableTargets.RemoveAll(target => target.Character.Class != AllyBaseClassId);

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
                case "ally_base_class_id":
                    AllyBaseClassId = (string)token.Value;
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}