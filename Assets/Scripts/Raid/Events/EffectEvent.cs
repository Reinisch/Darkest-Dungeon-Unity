public class EffectEvent
{
    public FormationUnit Performer { get; private set; }
    public FormationUnit Target { get; private set; }
    public Effect Effect { get; private set; }
    public SubEffect SubEffect { get; private set; }

    public int StackParameter { get; set; }

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