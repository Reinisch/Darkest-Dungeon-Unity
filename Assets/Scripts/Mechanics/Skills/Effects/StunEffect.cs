using UnityEngine;

public class StunEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Stun; } }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var stunStatus = (StunStatusEffect)target.Character.GetStatusEffect(StatusType.Stun);
        if (stunStatus.IsApplied)
            return true;

        float stunChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
            (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

        stunChance -= target.Character.GetSingleAttribute(AttributeType.Stun).ModifiedValue;
        if (performer != null && performer.Character is Hero)
            stunChance += performer.Character.GetSingleAttribute(AttributeType.StunChance).ModifiedValue;

        stunChance = Mathf.Clamp(stunChance, 0, 0.95f);
        if (RandomSolver.CheckSuccess(stunChance))
        {
            stunStatus.StunApplied = true;
            target.SetHalo("stunned");
            target.Character[StatusType.Guard].ResetStatus();
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
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Stunned);
            return true;
        }
        else
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.StunResist);
            return false;
        }
    }

    public override string Tooltip(Effect effect)
    {
        string stunString = LocalizationManager.GetString("effect_tooltip_stun");
        string chanceFormat = string.Format(LocalizationManager.GetString("effect_base_chance_format"),
            effect.IntegerParams[EffectIntParams.Chance] ?? 1);
        string toolTip = stunString + " " + chanceFormat;
        return toolTip;
    }
}