public class EffectEvent
{
    public SubEffect SubEffect { get; private set; }

    private FormationUnit Performer { get; set; }
    private FormationUnit Target { get; set; }
    private Effect Effect { get; set; }
    private int StackParameter { get; set; }

    public EffectEvent(FormationUnit performer, FormationUnit target, Effect effect, SubEffect subEffect)
    {
        Performer = performer;
        Target = target;
        Effect = effect;
        SubEffect = subEffect;
    }

    public void Fuse(EffectEvent nextEvent)
    {
        StackParameter += nextEvent.SubEffect.Fuse(nextEvent.Performer, nextEvent.Target, nextEvent.Effect);
    }

    public void FuseSelf()
    {
        StackParameter = SubEffect.Fuse(Performer, Target, Effect);
    }

    public void Execute()
    {
        if (StackParameter > 0)
            SubEffect.ApplyFused(Performer, Target, Effect, StackParameter);
        else
            SubEffect.ApplyQueued(Performer, Target, Effect);
    }
}