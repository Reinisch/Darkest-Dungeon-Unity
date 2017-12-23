using System.Collections.Generic;

public sealed class TargetSelectionAllyClass : TargetSelectionDesire
{
    private string AllyBaseClass { get; set; }

    public TargetSelectionAllyClass(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.AllyClass;

        GenerateFromDataSet(dataSet);
    }

    protected override List<FormationUnit> FilterTargets(FormationUnit performer, List<FormationUnit> possibleTargets)
    {
        var availableTargets = base.FilterTargets(performer, possibleTargets);

        availableTargets.RemoveAll(target => target.Character.Class != AllyBaseClass);
        return availableTargets;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "ally_base_class_id":
                    AllyBaseClass = (string)token.Value;
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}