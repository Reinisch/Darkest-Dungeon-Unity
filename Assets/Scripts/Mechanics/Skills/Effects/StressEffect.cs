using UnityEngine;

public class StressEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Stress; } }
    public override bool Fusable { get { return true; } }
    private int StressAmount { get; set; }

    public StressEffect(int amount)
    {
        StressAmount = amount;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character is Hero == false)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialDamage = StressAmount;
        if (performer != null)
            initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

        int damage = Mathf.RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
        if (damage < 1) damage = 1;

        target.Character.Stress.IncreaseValue(damage);
        if (target.Character.IsOverstressed)
        {
            if (target.Character.IsVirtued)
                target.Character.Stress.CurrentValue = Mathf.Clamp(target.Character.Stress.CurrentValue, 0, 100);
            else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                RaidSceneManager.Instanse.AddResolveCheck(target);

            if (Mathf.Approximately(target.Character.Stress.CurrentValue, 200))
                RaidSceneManager.Instanse.AddHeartAttackCheck(target);
        }
        target.OverlaySlot.UpdateOverlay();
        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (target.Character is Hero == false)
            return false;

        if (effect.IntegerParams[EffectIntParams.Chance].HasValue)
            if (!RandomSolver.CheckSuccess((float)effect.IntegerParams[EffectIntParams.Chance].Value / 100))
                return false;

        float initialDamage = StressAmount;
        if (performer != null)
            initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

        int damage = Mathf.RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
        if (damage < 1) damage = 1;

        target.Character.Stress.IncreaseValue(damage);
        if (target.Character.IsOverstressed)
        {
            if (target.Character.IsVirtued)
                target.Character.Stress.CurrentValue = Mathf.Clamp(target.Character.Stress.CurrentValue, 0, 100);
            else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                RaidSceneManager.Instanse.AddResolveCheck(target);

            if (Mathf.Approximately(target.Character.Stress.CurrentValue, 200))
                RaidSceneManager.Instanse.AddHeartAttackCheck(target);
        }

        target.OverlaySlot.UpdateOverlay();
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Stress, damage.ToString());
        target.SetHalo("afflicted");
        return true;
    }

    public override bool ApplyFused(FormationUnit performer, FormationUnit target, Effect effect, int fuseParameter)
    {
        if (target == null || fuseParameter <= 0)
            return false;

        if (target.Character is Hero == false)
            return false;

        target.Character.Stress.IncreaseValue(fuseParameter);
        if (target.Character.IsOverstressed)
        {
            if (target.Character.IsVirtued)
                target.Character.Stress.CurrentValue = Mathf.Clamp(target.Character.Stress.CurrentValue, 0, 100);
            else if (!target.Character.IsAfflicted && target.Character.IsOverstressed)
                RaidSceneManager.Instanse.AddResolveCheck(target);

            if (Mathf.Approximately(target.Character.Stress.CurrentValue, 200))
                RaidSceneManager.Instanse.AddHeartAttackCheck(target);
        }

        target.OverlaySlot.UpdateOverlay();
        RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Stress, fuseParameter.ToString());
        target.SetHalo("afflicted");
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

        float initialDamage = StressAmount;
        if (performer != null)
            initialDamage *= (1 + performer.Character.GetSingleAttribute(AttributeType.StressDmgPercent).ModifiedValue);

        int damage = Mathf.RoundToInt(initialDamage * (1 +
                target.Character.GetSingleAttribute(AttributeType.StressDmgReceivedPercent).ModifiedValue));
        if (damage < 1) damage = 1;

        return damage;
    }

    public override string Tooltip(Effect effect)
    {
        string toolTip = string.Format(LocalizationManager.GetString(
                        "effect_tooltip_stress_format"), StressAmount);
        return toolTip;
    }
}