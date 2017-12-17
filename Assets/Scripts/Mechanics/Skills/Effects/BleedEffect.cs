using UnityEngine;

public class BleedEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Bleeding; } }
    private int DotBleed { get; set; }

    public BleedEffect(int dotAmount)
    {
        DotBleed = dotAmount;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        float bleedChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        bleedChance -= target.Character.GetSingleAttribute(AttributeType.Bleed).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            bleedChance += performer.Character.GetSingleAttribute(AttributeType.BleedChance).ModifiedValue;

        bleedChance = Mathf.Clamp(bleedChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(bleedChance))
        {
            var poisonStatus = (BleedingStatusEffect)target.Character.GetStatusEffect(StatusType.Bleeding);
            var newDot = new DamageOverTimeInstanse
            {
                TickDamage = DotBleed,
                TicksAmount = effect.IntegerParams[EffectIntParams.Duration] ?? 3,
            };
            newDot.TicksLeft = newDot.TicksAmount;
            poisonStatus.AddInstanse(newDot);
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
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Bleed);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.BleedResist);
            return false;
        }
    }

    public override string Tooltip(Effect effect)
    {
        string poisonString = LocalizationManager.GetString("effect_tooltip_dot_bleed");
        string chanceFormat = string.Format(LocalizationManager.GetString("effect_base_chance_format"),
            effect.IntegerParams[EffectIntParams.Chance] ?? 0);
        string toolTip = poisonString + " " + chanceFormat;
        toolTip += "\n" + string.Format(LocalizationManager.GetString("effect_tooltip_dot_format"),
            DotBleed, effect.IntegerParams[EffectIntParams.Duration] ?? 0);
        return toolTip;
    }
}