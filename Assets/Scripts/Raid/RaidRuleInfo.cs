public class RaidRuleInfo
{
    public string Dungeon { get; private set; }
    public BattleGround BattleGround { get; private set; }
    public TorchMeter TorchMeter { get; private set; }
    public bool IsWalkingBack { get; private set; }
    public bool IsDoingCamping { get; private set; }

    public bool IsRiposting { get; private set; }
    public FormationUnit Unit { get; private set; }
    public FormationUnit Target { get; private set; }
    public CombatSkill Skill { get; private set; }

    public RaidRuleInfo(string dungeon, BattleGround battleGround, TorchMeter torch)
    {
        Dungeon = dungeon;
        BattleGround = battleGround;
        TorchMeter = torch;
    }

    public void SetWalkingBack(bool isWalkingBack)
    {
        IsWalkingBack = isWalkingBack;
        RaidSceneManager.Formations.Heroes.UpdateBuffRule(BuffRule.WalkBack);
    }

    public void SetCamping(bool isCamping)
    {
        IsDoingCamping = isCamping;
    }

    public RaidRuleInfo GetIdleUnitRules(FormationUnit unit)
    {
        IsRiposting = false;
        Unit = unit;
        Target = null;
        Skill = null;
        return this;
    }

    public RaidRuleInfo GetCombatUnitRules(FormationUnit unit, FormationUnit target, CombatSkill skill, bool riposte)
    {
        Unit = unit;
        Target = target;
        Skill = skill;
        IsRiposting = riposte;
        return this;
    }
}