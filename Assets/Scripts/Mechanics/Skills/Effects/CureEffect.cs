public class CureEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Cure; } }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        ((DamageOverTimeStatusEffect)target.Character[StatusType.Poison]).RemoveDoT();
        ((DamageOverTimeStatusEffect)target.Character[StatusType.Bleeding]).RemoveDoT();
        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        bool cureEffective = false;
        var poisonStatus = (PoisonStatusEffect)target.Character.GetStatusEffect(StatusType.Poison);
        var bleedStatus = (BleedingStatusEffect)target.Character.GetStatusEffect(StatusType.Bleeding);
        if (poisonStatus.IsApplied)
        {
            poisonStatus.RemoveDoT();
            cureEffective = true;
        }
        if (bleedStatus.IsApplied)
        {
            bleedStatus.RemoveDoT();
            cureEffective = true;
        }
        if (cureEffective)
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Cured);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        return false;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_cure");
    }
}