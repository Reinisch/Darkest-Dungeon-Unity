using System.Collections.Generic;

public enum TownEffectType
{
    ActivityLock,
    GoMissing,
    AddQuirk,
    ApplyBuff,
    ChangeCurrency,
    AddTrinket,
    RemoveTrinket
}

public abstract class TownEffect : IProportionValue
{
    public TownEffectType Type { get; set; }
    public int Chance { get; set; }
}

public class ActivityLockTownEffect : TownEffect
{
    public ActivityLockTownEffect()
    {
        Type = TownEffectType.ActivityLock;
    }
}

public class MissingTownEffect : TownEffect
{
    public List<MissingDuration> Durations { get; set; }

    public MissingTownEffect()
    {
        Type = TownEffectType.GoMissing;
        Durations = new List<MissingDuration>();
    }
}

public class MissingDuration : IProportionValue
{
    public int Chance { get; set; }
    public int Duration { get; set; }
}

public class AddQuirkTownEffect : TownEffect
{
    public List<TownActivityQuirk> QuirkSet { get; set; }

    public AddQuirkTownEffect()
    {
        Type = TownEffectType.AddQuirk;
        QuirkSet = new List<TownActivityQuirk>();
    }
}

public class TownActivityQuirk : IProportionValue
{
    public int Chance { get; set; }
    public string QuirkName { get; set; }
}

public class AddBuffTownEffect : TownEffect
{
    public List<TownActivityBuff> BuffSets { get; set; }

    public AddBuffTownEffect()
    {
        Type = TownEffectType.ApplyBuff;
        BuffSets = new List<TownActivityBuff>();
    }
}

public class TownActivityBuff : IProportionValue
{
    public int Chance { get; set; }
    public List<string> BuffNames { get; set; }

    public TownActivityBuff()
    {
        BuffNames = new List<string>();
    }
}

public class CurrencyTownEffect : TownEffect
{
    public List<TownActivityCurrency> Changes { get; set; }

    public CurrencyTownEffect()
    {
        Type = TownEffectType.ChangeCurrency;
        Changes = new List<TownActivityCurrency>();
    }
}

public class TownActivityCurrency : IProportionValue
{
    public int Chance { get; set; }
    public string Type { get; set; }
    public int Amount { get; set; }
}

public class AddTrinketTownEffect : TownEffect
{
    public AddTrinketTownEffect()
    {
        Type = TownEffectType.AddTrinket;
    }
}

public class RemoveTrinketTownEffect : TownEffect
{
    public RemoveTrinketTownEffect()
    {
        Type = TownEffectType.RemoveTrinket;
    }
}