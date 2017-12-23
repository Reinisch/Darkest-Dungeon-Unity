public class PerformerRankTargetEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Rank; } }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null || performer == null)
            return false;

        target.Formation.RankHolder.MarkRank(target.Rank);
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}