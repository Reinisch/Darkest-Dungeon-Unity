using UnityEngine;

public class StressHealEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.StressHeal; } }
    public override bool Fusable { get { return true; } }
    private int StressHealAmount { get; set; }

    public StressHealEffect(int amount)
    {
        StressHealAmount = amount;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var hero = target.Character as Hero;

        if (hero == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = StressHealAmount;
        if (performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;

        target.Character.Stress.DecreaseValue(heal);
        if (Mathf.RoundToInt(hero.Stress.CurrentValue) == 0 && hero.IsAfflicted)
            hero.RevertTrait();
        target.OverlaySlot.UpdateOverlay();
        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var hero = target.Character as Hero;

        if (hero == null)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialHeal = StressHealAmount;
        if (performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;

        target.Character.Stress.DecreaseValue(heal);
        if (Mathf.RoundToInt(hero.Stress.CurrentValue) == 0 && hero.IsAfflicted)
            hero.RevertTrait();
        target.OverlaySlot.UpdateOverlay();
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.StressHeal, heal.ToString());
        target.SetHalo("heroic");
        return true;
    }

    public override bool ApplyFused(FormationUnit performer, FormationUnit target, Effect effect, int fuseParameter)
    {
        if (target == null || fuseParameter <= 0)
            return false;

        var hero = target.Character as Hero;

        if (hero == null)
            return false;

        target.Character.Stress.DecreaseValue(fuseParameter);
        if (Mathf.RoundToInt(hero.Stress.CurrentValue) == 0 && hero.IsAfflicted)
            hero.RevertTrait();
        target.OverlaySlot.UpdateOverlay();
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.StressHeal, fuseParameter.ToString());
        target.SetHalo("heroic");
        return true;
    }

    public override int Fuse(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return 0;

        if (target.Character is Hero == false)
            return 0;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return 0;

        float initialHeal = StressHealAmount;
        if (performer != null)
            initialHeal *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressHealPercent).ModifiedValue);

        int heal = Mathf.RoundToInt(initialHeal * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressHealReceivedPercent).ModifiedValue));
        if (heal < 1) heal = 1;

        return heal;
    }

    public override string Tooltip(Effect effect)
    {
        return string.Format(LocalizationManager.GetString("effect_tooltip_stress_heal_format"), StressHealAmount);
    }
}