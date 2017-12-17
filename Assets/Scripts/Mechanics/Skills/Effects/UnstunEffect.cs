public class UnstunEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Unstun; } }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var markStatus = (StunStatusEffect)target.Character.GetStatusEffect(StatusType.Stun);
        if (markStatus.IsApplied)
        {
            markStatus.StunApplied = false;
            target.ResetHalo();
            return true;
        }
        return false;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Unstun);
            return true;
        }
        return false;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_unstun");
    }
}