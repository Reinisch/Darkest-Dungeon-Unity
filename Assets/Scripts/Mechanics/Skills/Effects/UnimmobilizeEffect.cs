public class UnimmobilizeEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Unimmobilize; } }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        ApplyInstant(performer, target, effect);
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.CombatInfo.IsImmobilized)
        {
            target.CombatInfo.IsImmobilized = false;
            target.SetDefendAnimation(false);
            return true;
        }
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}