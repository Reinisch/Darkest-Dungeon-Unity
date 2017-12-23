public enum BrainDecisionType { Pass, Perform }

public class MonsterBrainDecision
{
    public BrainDecisionType Decision { get; set; }
    public CombatSkill SelectedSkill { get; set; }
    public SkillTargetInfo TargetInfo { get; set; }

    public MonsterBrainDecision(BrainDecisionType decision)
    {
        Decision = decision;
        TargetInfo = new SkillTargetInfo(SkillTargetType.Self);
    }
    public MonsterBrainDecision(BrainDecisionType decision, CombatSkill skill, SkillTargetInfo targetInfo)
    {
        Decision = decision;
        SelectedSkill = skill;
        TargetInfo = targetInfo;
    }
}