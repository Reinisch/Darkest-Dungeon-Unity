public class ImmobilizeEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Immobilize; } }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (!target.CombatInfo.IsImmobilized)
        {
            target.CombatInfo.IsImmobilized = true;
            target.SetDefendAnimation(true);
            return true;
        }
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}