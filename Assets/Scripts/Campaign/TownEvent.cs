using System.Collections.Generic;

public enum TownEventTone
{
    Good, Bad, Neutral
}
public enum TownEventDataType
{
    EmbarkPartyBuff, IdleResolve, BonusRecruit, InActivityBuff,
    ActivityLock, ActivityCostChange, ProvisionTypeCostChange,
    ProvisionTypeAmountChange, UpgradeTagDiscount, FreeActivity,
    DeadRecruit, NoLevelRestriction, UpgradeTagFree, IdleBuff,
    PlotQuest
}

public class TownEvent : ISingleProportion
{
    private float baseChance;
    private int notRolledAmount;
    private int activeCooldown;

    public string Id { get; set; }
    public TownEventTone Tone { get; set; }
    public int Cooldown { get; set; }
    public float ChancePerNotRolled { get; set; }
    public float Chance
    {
        get
        {
            return baseChance + ChancePerNotRolled * notRolledAmount;
        }
        set
        {
            baseChance = value;
        }
    }

    public int MinimumWeek { get; set; }
    public int DeadHeroes { get; set; }
    public Dictionary<int, int> LevelHeroes { get; set; }
    public Dictionary<string, string> Purchases { get; set; }

    public List<string> AmbienceParameters { get; set; }
    public string Sprite { get; set; }
    public string SpriteAttachment { get; set; }

    public List<TownEventData> Data { get; set; }

    public TownEvent()
    {
        LevelHeroes = new Dictionary<int, int>();
        Purchases = new Dictionary<string, string>();
        Data = new List<TownEventData>();
    }
}

public class TownEventGuarantee
{
    public string Dungeon { get; set; }
    public string QuestType { get; set; }
    public string EventId { get; set; }
}

public class TownEventData
{
    public TownEventDataType Type { get; set; }

    public string StringData { get; set; }
    public float NumberData { get; set; }
}