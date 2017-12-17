using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatStatBuffEffect : SubEffect
{
    public override EffectSubType Type { get { return EffectSubType.StatBuff; } }
    public StatusType TargetStatus { get; set; }
    public MonsterType TargetMonsterType { get; set; }
    public Dictionary<AttributeType, float> StatAddBuffs { get; private set; }
    public Dictionary<AttributeType, float> StatMultBuffs { get; private set; }

    public CombatStatBuffEffect()
    {
        StatAddBuffs = new Dictionary<AttributeType, float>();
        StatMultBuffs = new Dictionary<AttributeType, float>();
    }

    public bool IsPositive()
    {
        KeyValuePair<AttributeType, float> buff;

        if (StatAddBuffs.Count > 0)
            buff = StatAddBuffs.First();
        else if (StatMultBuffs.Count > 0)
            buff = StatMultBuffs.First();
        else
            return false;

        if (buff.Key == AttributeType.StressDmgPercent || buff.Key == AttributeType.StressDmgReceivedPercent)
        {
            if (buff.Value > 0)
                return false;
            return true;
        }
        if (buff.Value >= 0)
            return true;
        return false;
    }

    public override void Apply(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (TargetStatus == StatusType.None && TargetMonsterType == MonsterType.None)
        {
            if (effect.BooleanParams[EffectBoolParams.Queue].HasValue)
            {
                if (effect.BooleanParams[EffectBoolParams.Queue] == false)
                    ApplyInstant(performer, target, effect);
                else
                    target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
            }
            else
                target.EventQueue.Add(new EffectEvent(performer, target, effect, this));
        }
    }

    public override void ApplyTargetConditions(FormationUnit performer, FormationUnit target, FormationUnit primaryTarget, Effect effect)
    {
        if ((TargetStatus == StatusType.None && TargetMonsterType == MonsterType.None) == false)
        {
            if (primaryTarget == null)
                return;

            if (TargetMonsterType != MonsterType.None)
            {
                if (primaryTarget.Character.IsMonster == false)
                    return;
                if (!((Monster)primaryTarget.Character).Types.Contains(TargetMonsterType))
                    return;
            }

            if (TargetStatus != StatusType.None)
            {
                if (!primaryTarget.Character.GetStatusEffect(TargetStatus).IsApplied)
                    return;
            }
            ApplyConditional(target);
        }
    }

    public override bool ApplyInstant(FormationUnit performer, FormationUnit target, Effect effect)
    {
        if (target == null)
            return false;

        if (StatAddBuffs.Count == 0 && StatMultBuffs.Count == 0)
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


                debuffChance = Mathf.Clamp(debuffChance, 0, 0.95f);
                if (performer == target)
                    debuffChance = 1;

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
            if (IsPositive())
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

                debuffChance = Mathf.Clamp(debuffChance, 0, 0.95f);
                if (performer == target)
                    debuffChance = 1;

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

        if (StatAddBuffs.Count == 0 && StatMultBuffs.Count == 0)
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
            if (IsPositive())
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
    }

    public override string Tooltip(Effect effect)
    {
        string toolTip = "";
        foreach (var item in StatAddBuffs)
        {
            if (item.Key == AttributeType.DamageLow || Mathf.Approximately(item.Value, 0))
                continue;

            string newStat = toolTip.Length > 0 ?
                "\n" + string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsAddBonusString(item.Key)), item.Value) :
                string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsAddBonusString(item.Key)), item.Value);
            if (TargetStatus != StatusType.None)
            {
                string statusFormat = LocalizationManager.GetString("buff_rule_tooltip_actorStatus");
                toolTip += string.Format(statusFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.StatusString(TargetStatus)), newStat);
            }
            else if (!(TargetMonsterType == MonsterType.None || TargetMonsterType == MonsterType.Unknown))
            {
                string monsterFormat = LocalizationManager.GetString("buff_rule_tooltip_monsterType");
                toolTip += string.Format(monsterFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.MonsterTypeString(TargetMonsterType)), newStat);
            }
            else
                toolTip += newStat;
        }

        foreach (var item in StatMultBuffs)
        {
            if (item.Key == AttributeType.DamageLow || Mathf.Approximately(item.Value, 0))
                continue;
            string newStat = toolTip.Length > 0 ?
                "\n" + string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsMultBonusString(item.Key)), item.Value) :
                string.Format(LocalizationManager.GetString(
                CharacterLocalizationHelper.BaseStatsMultBonusString(item.Key)), item.Value);

            if (TargetStatus != StatusType.None)
            {
                string statusFormat = LocalizationManager.GetString("buff_rule_tooltip_actorStatus");
                toolTip += string.Format(statusFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.StatusString(TargetStatus)), newStat);
            }
            else if (!(TargetMonsterType == MonsterType.None || TargetMonsterType == MonsterType.Unknown))
            {
                string monsterFormat = LocalizationManager.GetString("buff_rule_tooltip_monsterType");
                toolTip += string.Format(monsterFormat, LocalizationManager.GetString(
                    CharacterLocalizationHelper.MonsterTypeString(TargetMonsterType)), newStat);
            }
            else
                toolTip += newStat;
        }

        return toolTip;
    }

    private void ApplyBuff(FormationUnit target, Effect effect)
    {
        if (effect.IntegerParams[EffectIntParams.Curio].HasValue)
        {
            foreach (var statInfo in StatAddBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                    BuffDurationType.Camp, BuffSourceType.Adventure));
            foreach (var statInfo in StatMultBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                    BuffDurationType.Camp, BuffSourceType.Adventure));
        }
        else if (effect.IntegerParams[EffectIntParams.Duration].HasValue)
        {
            if (effect.IntegerParams[EffectIntParams.Duration].Value == -1)
            {
                foreach (var statInfo in StatAddBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                        BuffDurationType.Camp, BuffSourceType.Adventure));
                foreach (var statInfo in StatMultBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                        BuffDurationType.Camp, BuffSourceType.Adventure));
            }
            else
            {
                foreach (var statInfo in StatAddBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                        BuffDurationType.Round, BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
                foreach (var statInfo in StatMultBuffs)
                    target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                        BuffDurationType.Round, BuffSourceType.Adventure, effect.IntegerParams[EffectIntParams.Duration].Value));
            }
        }
        else
        {
            foreach (var statInfo in StatAddBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, statInfo.Key, statInfo.Value),
                    BuffDurationType.Round, BuffSourceType.Adventure, 3));
            foreach (var statInfo in StatMultBuffs)
                target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, statInfo.Key, statInfo.Value),
                    BuffDurationType.Round, BuffSourceType.Adventure, 3));
        }
    }

    private void ApplyConditional(FormationUnit target)
    {
        foreach (var statInfo in StatAddBuffs)
            target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatAdd, BuffRule.Always, statInfo.Key, statInfo.Value),
                BuffDurationType.Round, BuffSourceType.Condition));
        foreach (var statInfo in StatMultBuffs)
            target.Character.AddBuff(new BuffInfo(new Buff(BuffType.StatMultiply, BuffRule.Always, statInfo.Key, statInfo.Value),
                BuffDurationType.Round, BuffSourceType.Condition));
    }
}