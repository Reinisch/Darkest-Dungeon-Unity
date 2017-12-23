public class FlatModifier
{
    public AttributeType TargetAttribute { get; set; }
    public float ModifierValue { get; set; }
    public bool IsPaired { get; set; }

    public FlatModifier(AttributeType attributeType, float value, bool isPaired)
    {
        TargetAttribute = attributeType;
        ModifierValue = value;
        IsPaired = isPaired;
    }

    public void ApplyModifier(Character character)
    {
        if(IsPaired)
            character.GetPairedAttribute(TargetAttribute).FlatAddition += ModifierValue;
        else
            character.GetSingleAttribute(TargetAttribute).FlatAddition += ModifierValue;
    }

    public void RevertModifier(Character character)
    {
        if(IsPaired)
            character.GetPairedAttribute(TargetAttribute).FlatAddition -= ModifierValue;
        else
            character.GetSingleAttribute(TargetAttribute).FlatAddition -= ModifierValue;
    }

    public string Tooltip
    {
        get
        {
            switch (CharacterHelper.GetAttributeCategory(TargetAttribute))
            {
                case AttributeCategory.CombatStat:
                    return string.Format(LocalizationManager.GetString("buff_stat_tooltip_combat_stat_add_"
                        + CharacterHelper.AttributeTypeToString(TargetAttribute)), ModifierValue);
                case AttributeCategory.Resistance:
                    return string.Format(LocalizationManager.GetString("buff_stat_tooltip_resistance_"
                        + CharacterHelper.AttributeTypeToString(TargetAttribute)), ModifierValue);
                case AttributeCategory.Modifier:
                    return string.Format(LocalizationManager.GetString("buff_stat_tooltip_"
                        + CharacterHelper.AttributeTypeToString(TargetAttribute)), ModifierValue);
                case AttributeCategory.Discount:
                    return string.Format(LocalizationManager.GetString("buff_stat_tooltip_upgrade_discount_"
                        + CharacterHelper.AttributeTypeToString(TargetAttribute)), ModifierValue);
                default:
                    return "";
            }
        }
    }
}