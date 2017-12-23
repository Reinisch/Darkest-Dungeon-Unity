using System.Collections.Generic;
using System.Linq;

public sealed class TargetSelectionMarked : TargetSelectionDesire
{
    public TargetSelectionMarked(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.Marked;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (decision.TargetInfo.Targets.All(target => !target.Character.GetStatusEffect(StatusType.Marked).IsApplied))
            return false;

        return base.SelectTarget(performer, decision);
    }

    protected override List<FormationUnit> FilterTargets(FormationUnit performer, List<FormationUnit> possibleTargets)
    {
        var availableTargets = base.FilterTargets(performer, possibleTargets);

        availableTargets.RemoveAll(target => !target.Character[StatusType.Marked].IsApplied);
        return availableTargets;
    }
}