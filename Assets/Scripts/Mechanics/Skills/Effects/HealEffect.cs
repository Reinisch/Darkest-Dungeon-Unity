public class HealEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Heal; } }
    private int HealAmount { get; set; }

    public HealEffect(int amount)
    {
        HealAmount = amount;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = HealAmount;
        if (performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

        if (performer != null && RandomSolver.CheckSuccess(performer.Character.Crit))
        {
            int critHeal = target.Character.Heal(initialHeal * 1.5f, true);
            target.OverlaySlot.UpdateOverlay();
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.CritHeal, critHeal.ToString());
        }
        else
        {
            int heal = target.Character.Heal(initialHeal, true);
            target.OverlaySlot.UpdateOverlay();
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Heal, heal.ToString());
        }
        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = HealAmount;
        if (performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.HpHealPercent).ModifiedValue);

        if (performer != null && RandomSolver.CheckSuccess(performer.Character.Crit))
        {
            int critHeal = target.Character.Heal(initialHeal * 1.5f, true);
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.CritHeal, critHeal.ToString());
            if (target.Character is Hero)
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally_crit");
            else
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_enemy_crit");

            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        else
        {
            int heal = target.Character.Heal(initialHeal, true);
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Heal, heal.ToString());
            if (target.Character is Hero)
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
            else
                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_enemy");

            target.OverlaySlot.UpdateOverlay();
            return true;
        }
    }

    public override string Tooltip(Effect effect)
    {
        return string.Format(LocalizationManager.GetString("effect_tooltip_heal_format"), HealAmount);
    }
}