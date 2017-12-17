public class TagEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Tag; } }
    public DurationType DurationType { private get; set; }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var markStatus = (MarkStatusEffect)target.Character.GetStatusEffect(StatusType.Marked);
        markStatus.MarkDuration = effect.IntegerParams[EffectIntParams.Duration] ?? 3;
        markStatus.DurationType = DurationType;
        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Tagged);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        return false;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_tag");
    }
}