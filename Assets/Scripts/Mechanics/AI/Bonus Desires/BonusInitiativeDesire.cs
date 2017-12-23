public abstract class BonusInitiativeDesire
{
    public string CombatSkillOverride { get; protected set; }
    public bool IsRoundStart { get; protected set; }
    public bool IsRoundInProgress { get; protected set; }
    public bool IsRoundFinish { get; protected set; }
    public bool IsPostTurn { get; protected set; }

    public abstract bool CheckBonusInitiative(FormationUnit performer);
}