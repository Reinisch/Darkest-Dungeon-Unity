public enum BuffType : byte
{
    StatAdd,
    StatMultiply
}
public enum BuffRule : byte
{
    Always, Size, LightBelow, LightAbove,
    HpBelow, HpAbove, InRank,
    StressAbove, StressBelow,
    Skill, Afflicted, Virtued,
    Melee, Ranged, FirstRound,
    Status, EnemyType, DeathsDoor,
    InCamp, InDungeon, WalkBack,
    InActivity, InCorridor,
    Riposting, InMode,
}

public class Buff
{
    public string Id { get; set; }
    public float ModifierValue { get; set; }
    public BuffType Type { get; set; }
    public AttributeType AttributeType { get; set; }

    public BuffDurationType DurationType { get; set; }
    public int DurationAmount { get; set; }

    public BuffRule RuleType { get; set; }
    public bool IsFalseRule { get; set; }
    public float SingleParam { get; set; }
    public string StringParam { get; set; }

    public Buff()
    {
    }

    public Buff(BuffType buffType, BuffRule rule, AttributeType attributeType, float modifierValue)
    {
        Id = "";
        Type = buffType;
        RuleType = rule;
        AttributeType = attributeType;
        ModifierValue = modifierValue;
    }

    public Buff(BuffType buffType, AttributeType attributeType, float modifierValue) :
        this(buffType, BuffRule.Always, attributeType, modifierValue)
    {
    }

    public bool IsPositive()
    {
        if (AttributeType == AttributeType.StressDmgPercent || AttributeType == AttributeType.StressDmgReceivedPercent)
        {
            if (ModifierValue > 0)
                return false;
            return true;
        }
        else if (ModifierValue >= 0)
            return true;
        return false;
    }

    public bool IsSameBuff(Buff buff)
    {
        return AttributeType == buff.AttributeType && RuleType == buff.RuleType && IsFalseRule == buff.IsFalseRule;
    }

    public string ToolTip
    {
        get
        {
            if (AttributeType == AttributeType.DamageLow || ModifierValue == 0)
                return "";

            var sb = ToolTipManager.SubTipBody;
            var attributeCategory = CharacterHelper.GetAttributeCategory(AttributeType);

            switch(attributeCategory)
            {
                case AttributeCategory.Modifier:
                    sb.AppendFormat(LocalizationManager.GetString(
                        ToolTipManager.GetConcat("buff_stat_tooltip_"
                            + CharacterHelper.AttributeTypeToString(AttributeType))), ModifierValue);
                    break;
                case AttributeCategory.CombatStat:
                    sb.AppendFormat(LocalizationManager.GetString(
                        ToolTipManager.GetConcat("buff_stat_tooltip_", CharacterHelper.BuffTypeToString(Type), "_",
                        CharacterHelper.AttributeTypeToString(AttributeType))), ModifierValue);
                    break;
                default:
                    sb.AppendFormat(LocalizationManager.GetString(
                        ToolTipManager.GetConcat("buff_stat_tooltip_",
                            CharacterHelper.AttributeCategoryToString(attributeCategory), "_",
                            CharacterHelper.AttributeTypeToString(AttributeType))), ModifierValue);
                    break;
            }

            string body = sb.ToString();
            sb = ToolTipManager.SubTipBody;

            switch (RuleType)
            {
                case BuffRule.InRank:
                    sb.AppendFormat(LocalizationManager.GetString(
                        CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), SingleParam + 1, body);
                    break;
                case BuffRule.Size:
                    sb.AppendFormat(LocalizationManager.GetString(
                        CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), SingleParam, body);
                    break;
                case BuffRule.EnemyType:
                    sb.AppendFormat(LocalizationManager.GetString(
                        CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), 
                        LocalizationManager.GetString("buff_rule_data_tooltip_" + StringParam), body);
                    break;
                case BuffRule.Skill:
                    sb.AppendFormat(LocalizationManager.GetString(
                        CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)),
                        LocalizationManager.GetString("buff_rule_data_tooltip_" + StringParam), body);
                    break;
                case BuffRule.Status:
                    sb.AppendFormat(LocalizationManager.GetString(
                        CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)),
                        LocalizationManager.GetString("buff_rule_data_tooltip_" + StringParam), body);
                    break;
                case BuffRule.LightBelow:
                case BuffRule.LightAbove:
                case BuffRule.HpBelow:
                case BuffRule.HpAbove:
                case BuffRule.StressAbove:
                case BuffRule.StressBelow:
                    sb.AppendFormat(LocalizationManager.GetString(
                        CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), SingleParam, body);
                    break;
                default:
                    sb.AppendFormat(LocalizationManager.GetString(
                        CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), StringParam, body);
                    break;
            }
          
            return sb.ToString();
        }
    }

    public string TooltipOverrided(float amount)
    {
        if (AttributeType == AttributeType.DamageLow || amount == 0)
            return "";

        var sb = ToolTipManager.SubTipBody;
        var attributeCategory = CharacterHelper.GetAttributeCategory(AttributeType);

        switch (attributeCategory)
        {
            case AttributeCategory.Modifier:
                sb.AppendFormat(LocalizationManager.GetString(
                    ToolTipManager.GetConcat("buff_stat_tooltip_" +
                        CharacterHelper.AttributeTypeToString(AttributeType))), amount);
                break;
            case AttributeCategory.CombatStat:
                sb.AppendFormat(LocalizationManager.GetString(
                    ToolTipManager.GetConcat("buff_stat_tooltip_",
                    CharacterHelper.BuffTypeToString(Type), "_",
                    CharacterHelper.AttributeTypeToString(AttributeType))), amount);
                break;
            default:
                sb.AppendFormat(LocalizationManager.GetString(
                    ToolTipManager.GetConcat("buff_stat_tooltip_",
                    CharacterHelper.AttributeCategoryToString(attributeCategory), "_",
                    CharacterHelper.AttributeTypeToString(AttributeType))), amount);
                break;
        }

        string body = sb.ToString();
        sb = ToolTipManager.SubTipBody;

        switch (RuleType)
        {
            case BuffRule.EnemyType:
                sb.AppendFormat(LocalizationManager.GetString(
                    CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)),
                    LocalizationManager.GetString("buff_rule_data_tooltip_" + StringParam), body);
                break;
            case BuffRule.InRank:
                sb.AppendFormat(LocalizationManager.GetString(
                    CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), SingleParam + 1, body);
                break;
            case BuffRule.Size:
                sb.AppendFormat(LocalizationManager.GetString(
                    CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), SingleParam + 1, body);
                break;
            case BuffRule.Skill:
                sb.AppendFormat(LocalizationManager.GetString(
                    CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)),
                    LocalizationManager.GetString("buff_rule_data_tooltip_" + StringParam), body);
                break;
            case BuffRule.Status:
                sb.AppendFormat(LocalizationManager.GetString(
                    CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)),
                    LocalizationManager.GetString("buff_rule_data_tooltip_" + StringParam), body);
                break;
            case BuffRule.LightBelow:
            case BuffRule.LightAbove:
            case BuffRule.HpBelow:
            case BuffRule.HpAbove:
            case BuffRule.StressAbove:
            case BuffRule.StressBelow:
                sb.AppendFormat(LocalizationManager.GetString(
                    CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), SingleParam, body);
                break;
            default:
                sb.AppendFormat(LocalizationManager.GetString(
                    CharacterLocalizationHelper.BuffRuleTooltipString(RuleType, IsFalseRule)), StringParam, body);
                break;
        }

        return sb.ToString();
    }
}