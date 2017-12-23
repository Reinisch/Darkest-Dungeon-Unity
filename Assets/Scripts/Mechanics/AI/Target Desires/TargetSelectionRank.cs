using System.Collections.Generic;

public sealed class TargetSelectionRank : TargetSelectionDesire
{
    public TargetSelectionRank(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.Rank;

        GenerateFromDataSet(dataSet);
    }

    protected override List<FormationUnit> FilterTargets(FormationUnit performer, List<FormationUnit> possibleTargets)
    {
        var availableTargets = base.FilterTargets(performer, possibleTargets);

        availableTargets.RemoveAll(target => !target.Formation.RankHolder.MarkedRanks.Contains(target.Rank));
        return availableTargets;
    }
}