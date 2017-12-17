using System.Collections.Generic;
using UnityEngine;

public class RiposteEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Riposte; } }
    public Dictionary<AttributeType, float> StatAddBuffs { get; private set; }
    public Dictionary<AttributeType, float> StatMultBuffs { get; private set; }

    public RiposteEffect()
    {
        StatAddBuffs = new Dictionary<AttributeType, float>();
        StatMultBuffs = new Dictionary<AttributeType, float>();
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        var riposteStatus = (RiposteStatusEffect)target.Character.GetStatusEffect(StatusType.Riposte);
        int duration = effect.IntegerParams[EffectIntParams.Duration] ?? 1;

        if (duration == -1)
        {
            riposteStatus.DurationType = DurationType.Combat;
            duration = 1;
        }

        riposteStatus.RiposteDuration = duration;

        foreach (var statInfo in StatAddBuffs)
            if (!Mathf.Approximately(statInfo.Value, 0))
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, BuffRule.Riposting, statInfo.Key, statInfo.Value),
                    BuffDurationType.Round, BuffSourceType.Adventure, duration));
        foreach (var statInfo in StatMultBuffs)
            if (!Mathf.Approximately(statInfo.Value, 0))
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, BuffRule.Riposting, statInfo.Key, statInfo.Value),
                    BuffDurationType.Round, BuffSourceType.Adventure, duration));

        return true;
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (ApplyInstant(performer, target, effect))
        {
            RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Riposte);
            target.OverlaySlot.UpdateOverlay();
            return true;
        }
        return false;
    }

    public override string Tooltip(Effect effect)
    {
        return LocalizationManager.GetString("effect_tooltip_riposte");
    }
}