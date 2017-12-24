public class KillEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Kill; } }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        target.CombatInfo.MarkedForDeath = true;
        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return ApplyInstant(performer, target, effect);
    }
}