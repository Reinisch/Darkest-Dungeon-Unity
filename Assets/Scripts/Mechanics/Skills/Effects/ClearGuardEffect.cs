public class ClearGuardEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.ClearGuard; } }
    public bool ClearGuarding { private get; set; }
    public bool ClearGuarded { private get; set; }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            ApplyInstant(performer, target, effect);
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ClearGuarding)
            target.Character[StatusType.Guard].ResetStatus();
        if (ClearGuarded)
            target.Character[StatusType.Guarded].ResetStatus();

        target.OverlaySlot.UpdateOverlay();

        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
            return true;
        return false;
    }
}