using UnityEngine;

public class PoisonEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Poison; } }
    private int DotPoison { get; set; }

    public PoisonEffect(int dotAmount)
    {
        DotPoison = dotAmount;
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        float poisonChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        poisonChance -= target.Character.GetSingleAttribute(AttributeType.Poison).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            poisonChance += performer.Character.GetSingleAttribute(AttributeType.PoisonChance).ModifiedValue;

        poisonChance = Mathf.Clamp(poisonChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(poisonChance))
        {
            var poisonStatus = (PoisonStatusEffect)target.Character.GetStatusEffect(StatusType.Poison);
            var newDot = new DamageOverTimeInstanse
            {
                TickDamage = DotPoison,
                TicksAmount = effect.IntegerParams[EffectIntParams.Duration] ?? 3
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
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Poison);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.PoisonResist);
            return false;
        }
    }

    public override string Tooltip(Effect effect)
    {
        string poisonString = LocalizationManager.GetString("effect_tooltip_dot_poison");
        string chanceFormat = string.Format(LocalizationManager.GetString("effect_base_chance_format"),
            effect.IntegerParams[EffectIntParams.Chance] ?? 0);
        string toolTip = poisonString + " " + chanceFormat;
        toolTip += "\n" + string.Format(LocalizationManager.GetString("effect_tooltip_dot_format"),
            DotPoison, effect.IntegerParams[EffectIntParams.Duration] ?? 0);
        return toolTip;
    }
}