public enum EffectBoolParams : byte
{
    OnHit,
    OnMiss,
    ApplyOnce,
    CanApplyAfterDeath,
    CritDoesntApplyToRoll,
    ApplyWithResult,
    Queue,
    CurioResult,
    SpawnsLoot,
}

public enum EffectIntParams : byte
{
    Chance,
    Duration,
    Torch,
    Summons,
    VirtueBlockChance,
    Curio,
    Item,
}

public enum EffectTargetType : byte
{
    Target,
    Performer,
    Global,
    PerformersOther,
    TargetGroup,
}

public enum EffectSubType : byte
{
    None,
    Kill,
    KillType,
    Control,
    Buff,
    Immobilize,
    Unimmobilize,
    Pull,
    Push,
    Stress,
    StressHeal,
    StatBuff,
    Debuff,
    Stun,
    Unstun,
    Poison,
    Bleeding,
    Heal,
    Cure,
    LifeDrain,
    Tag,
    Untag,
    Shuffle,
    Summon,
    Disease,
    Mode,
    Capture,
    Rank,
    ClearTargetRanks,
    GuardAlly,
    Riposte,
    ClearGuard
}