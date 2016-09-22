public enum BuffDurationType { Undefined, Permanent, Round, Combat, Camp, Raid, Activity, QuestComplete, IdleTownVisit }
public enum BuffSourceType { Estate, Quirk, Trinket, Adventure, Condition, Trait, DeathsDoor, Mortality, Light }

public class BuffInfo
{
    public BuffDurationType DurationType { get; private set; }
    public BuffSourceType SourceType { get; private set; }
    public int Duration { get; set; }
    public bool IsApplied { get; set; }

    public float ModifierValue
    {
        get
        {
            return OverridenValue == 0 ? Buff.ModifierValue : OverridenValue;
        }
    }
    public float OverridenValue
    {
        get;
        set;
    }

    public Buff Buff { get; set; }

    public BuffInfo(BuffDurationType durationType, BuffSourceType sourceType)
    {
        DurationType = durationType;
        SourceType = sourceType;
    }
    public BuffInfo(Buff buff, BuffDurationType durationType, BuffSourceType sourceType, int duration = 1)
    {
        Buff = buff;
        DurationType = durationType;
        SourceType = sourceType;
        Duration = duration;
    }
    public BuffInfo(Buff buff, float overridenValue, BuffSourceType sourceType)
    {
        Buff = buff;
        OverridenValue = overridenValue;
        DurationType = buff.DurationType;
        SourceType = sourceType;
        Duration = buff.DurationAmount;
    }
}