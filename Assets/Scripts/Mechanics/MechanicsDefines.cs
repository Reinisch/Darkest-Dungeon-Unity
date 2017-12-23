public enum AttributeType
{
    Undefined,
    HitPoints,
    Stress,
    HpHealAmount,
    HpHealPercent,
    DmgReceivedPercent,
    HpHealReceivedPercent,
    StressDmgReceivedPercent,
    StressDmgPercent,
    StressHealPercent,
    StressHealReceivedPercent,
    ResolveCheckPercent,
    ResolveXpPercent,
    StunChance,
    PoisonChance,
    BleedChance,
    MoveChance,
    DebuffChance,
    ScoutingChance,
    PartySurpriseChance,
    MonsterSurpirseChance,
    RemoveQuirkChance,
    FoodConsumption,
    StarvingDamagePercent,
    DefenseRating,
    ProtectionRating,
    SpeedRating,
    AttackRating,
    CritChance,
    DamageLow,
    DamageHigh,
    ArmorDiscount,
    WeaponDiscount,
    Stun,
    Poison,
    Disease,
    DeathBlow,
    Move,
    Bleed,
    Debuff,
    Trap,
}

public enum AttributeCategory
{
    Undefined,
    CombatStat,
    Modifier,
    Discount,
    Resistance
}

public enum EffectBoolParams
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

public enum EffectIntParams
{
    Chance,
    Duration,
    Torch,
    Summons,
    VirtueBlockChance,
    Curio,
    Item,
}

public enum EffectTargetType
{
    Target,
    Performer,
    Global,
    PerformersOther,
    TargetGroup,
}

public enum EffectSubType
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

public enum TrinketSlot
{
    Left = 0,
    Right
}

public enum Rarity
{
    Trophy,
    DarkestDungeon,
    AncestralShambler,
    Ancestral,
    Collector,
    Madman,
    VeryRare,
    Rare,
    Uncommon,
    Common,
    VeryCommon,
    KickStarter
}

public enum MonsterType
{
    None,
    Unholy,
    Man,
    Eldritch,
    Beast,
    Corpse,
    Carpentry,
    Ironwork,
    Cauldron,
    Unknown,
    Cosmic
}

public enum DeathClassType
{
    Replacement,
    Corpse
}

public enum StatusType
{
    None,
    Stun,
    Bleeding,
    Poison,
    Marked,
    Riposte,
    Guard,
    Guarded,
    DeathsDoor,
    DeathRecovery
}

public enum DurationType
{
    Round,
    Combat
}

public enum HeroEquipmentSlot
{
    Weapon,
    Armor
}

public enum CampingPhase
{
    None,
    Meal,
    Skill
}

public enum BuffDurationType
{
    Undefined,
    Permanent,
    Round,
    Combat,
    Camp,
    Raid,
    Activity,
    QuestComplete,
    IdleTownVisit
}

public enum BuffSourceType
{
    Estate,
    Quirk,
    Trinket,
    Adventure,
    Condition,
    Trait,
    DeathsDoor,
    Mortality,
    Light
}

public enum QuestVisitType
{
    Explore,
    Battle
}

public enum SkillResultType
{
    Hit,
    Miss,
    Crit,
    Dodge,
    Heal,
    CritHeal,
    Utility
}

public enum SkillTargetType
{
    Self,
    Party,
    Enemy
}

public enum HeroTurnAction
{
    Waiting,
    Skill,
    Move,
    Pass,
    Retreat
}

public enum SkillCategory
{
    Damage,
    Heal,
    Support
}