using System.Collections.Generic;

public enum OverstressType
{
    Affliction,
    Virtue,
}

public enum StartTurnActType 
{
    Nothing, BarkStress,
    ChangePosition, IgnoreCommand,
    RandomCommand, RetreatFromCombat,
    AttackFriendly, AttackSelf,
    MarkSelf, StressHealSelf,
    StressHealParty, BuffAlly,
    BuffParty, HealSelf,
}

public enum ReactionType
{
    BlockMove, BlockHeal,
    BlockBuff, BlockItem,
    BlockRetreat, CommentSelfHit,
    CommentSelfMissed, CommentAllyHit,
    CommentAllyMissed, CommentAllyAttackHit,
    CommentAllyAttackMiss, CommentMove,
    CommentCurioInteraction, CommentTrapTriggered,
    BlockEffect,
}

public class Trait
{
    public string Id { get; set; }
    public OverstressType Type { get; set; }
    public string CurioTag { get; set; }
    public float TagChance { get; set; }
    public bool KeepLoot { get; set; }

    public List<Buff> Buffs { get; set; }
    public List<CombatStartTurnActOut> StartTurnActs { get; set; }
    public Dictionary<ReactionType, ReactionActOut> Reactions { get; set; }

    public Trait()
    {
        Buffs = new List<Buff>();
        StartTurnActs = new List<CombatStartTurnActOut>();
        Reactions = new Dictionary<ReactionType, ReactionActOut>();
    }
}

public class CombatStartTurnActOut : IProportionValue
{
    public StartTurnActType ActType { get; set; }
    public float NumberParameter { get; set; }
    public string StringParameter { get; set; }
    public int Chance { get; set; }
}

public class ReactionActOut
{
    public ReactionType ActType { get; set; }
    public Effect Effect { get; set; }
    public float Chance { get; set; }
}