public class UntagEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Untag; } }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var markStatus = (MarkStatusEffect)target.Character.GetStatusEffect(StatusType.Marked);
        if (markStatus.IsApplied)
        {
            markStatus.MarkDuration = 0;
            return true;
        }
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Untagged);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        return false;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_untag");
    }
}