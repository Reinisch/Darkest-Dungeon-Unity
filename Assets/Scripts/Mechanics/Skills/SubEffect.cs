public abstract class SubEffect
{
    public abstract EffectSubType Type { get; }
    public virtual bool Fusable { get { return false; } }

    public virtual void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                ApplyInstant(performer, target, effect);
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
        else
            target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
    }

    public abstract bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect);

    public abstract bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect);

    public virtual void ApplyTargetConditions(FormationUnit performer, FormationUnit target, FormationUnit primaryTarget, Effect effect)
    {
    }

    public virtual bool ApplyFused(FormationUnit performer, FormationUnit target, Effect effect, int fuseParameter)
    {
        return false;
    }

    public virtual int Fuse(FormationUnit performer, FormationUnit target, Effect effect)
    {
        return 0;
    }

    public virtual string Tooltip(Effect effect)
    {
        return "";
    }
}