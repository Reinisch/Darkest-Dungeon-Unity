using System.Collections.Generic;
using UnityEngine;

public class BuffEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.Buff; } }
    public List<Buff> Buffs { get; private set; }

    public BuffEffect()
    {
        Buffs = new List<Buff>();
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (Buffs.Count == 0)
            return false;

        if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
            {
                ApplyBuff(target, effect);
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                return false;
            }
        }
        else
        {
            if (Buffs[0].IsPositive())
            {
                ApplyBuff(target, effect);
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    return true;
                }
                return false;
            }
        }
    }

    public override bool ApplyQueued(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;
        if (Buffs.Count == 0)
            return false;

        if (effect.BooleanParams[EffectBoolParams.CurioResult].HasValue)
        {
            if (effect.BooleanParams[EffectBoolParams.CurioResult].Value)
            {
                ApplyBuff(target, effect);
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Buff);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Debuff).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.DebuffChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Debuff);
                    target.OverlaySlot.UpdateOverlay();
                    return true;
                }
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
                return false;
            }
        }
        else
        {
            if (Buffs[0].IsPositive())
            {
                ApplyBuff(target, effect);
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Buff);
                target.OverlaySlot.UpdateOverlay();
                return true;
            }
            else
            {
                float debuffChance = effect.IntegerParams[EffectIntParams.Chance].HasValue ?
                    (float)effect.IntegerParams[EffectIntParams.Chance].Value / 100 : 1;

                debuffChance -= target.Character.GetSingleAttribute(AttributeType.Move).ModifiedValue;
                if (performer != null && performer.Character is Hero)
                    debuffChance += performer.Character.GetSingleAttribute(AttributeType.MoveChance).ModifiedValue;

                debuffChance = performer == target ? 1 : Mathf.Clamp(debuffChance, 0, 0.95f);

                if (RandomSolver.CheckSuccess(debuffChance))
                {
                    ApplyBuff(target, effect);
                    RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.Debuff);
                    target.OverlaySlot.UpdateOverlay();
                    return true;
                }
                RaidSceneManager.RaidEvents.ShowPopupMessage(target, PopupMessageType.DebuffResist);
                return false;
            }
        }
    }

    public override string Tooltip(Effect effect)
    {
        string toolTip = "";
        foreach (var buff in Buffs)
            if (toolTip.Length > 0)
                toolTip += "\n" + buff.ToolTip;
            else
                toolTip += buff.ToolTip;

        return toolTip;
    }

    private void ApplyBuff(FormationUnit target, Effect effect)
    {
        if (effect.IntegerParams[EffectIntParams.Curio].HasValue)
            foreach (var buff in Buffs)
                target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Camp, BuffSourceType.Adventure));
        else if (effect.IntegerParams[EffectIntParams.Duration].HasValue)
        {
            if (effect.IntegerParams[EffectIntParams.Duration].Value == -1)
                foreach (var buff in Buffs)
                    target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Camp, BuffSourceType.Adventure));
            else
                foreach (var buff in Buffs)
                    target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Round,
                        BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
        }
        else
        {
            foreach (var buff in Buffs)
                target.Character.AddBuff(new BuffInfo(buff, BuffDurationType.Round,
                    BuffSourceType.Adventure, 3));
        }
    }
}