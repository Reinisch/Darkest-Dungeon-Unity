public class SkillResultEntry
{
    public int Amount { get; set; }
    public bool IsZeroed { get; set; }
    public bool IsTargetHit { get; set; }
    public bool IsHarmful { get; set; }
    public bool CanCritReleaf { get; set; }
    public bool CanKillReleaf { get; set; }
    public SkillResultType Type { get; set; }
    public FormationUnit Target { get; set; }

    public SkillResultEntry(FormationUnit target, SkillResultType result)
    {
        Type = result;
        Target = target;
        IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
        IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        CanCritReleaf = target.Character.BattleModifiers != null && target.Character.BattleModifiers.CanRelieveStressFromCrit;
        CanKillReleaf = target.Character.BattleModifiers != null && target.Character.BattleModifiers.CanRelieveStressFromKills;
    }

    public SkillResultEntry(FormationUnit target, int skillDamage, SkillResultType result)
    {
        Amount = skillDamage;
        Type = result;
        Target = target;
        IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
        IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        CanCritReleaf = target.Character.BattleModifiers != null && target.Character.BattleModifiers.CanRelieveStressFromCrit;
        CanKillReleaf = target.Character.BattleModifiers != null && target.Character.BattleModifiers.CanRelieveStressFromKills;
    }

    public SkillResultEntry(FormationUnit target, int skillDamage, bool isTargetZeroed, SkillResultType result)
    {
        Amount = skillDamage;
        Type = result;
        Target = target;
        IsZeroed = isTargetZeroed;
        IsTargetHit = Type != SkillResultType.Miss && Type != SkillResultType.Dodge;
        IsHarmful = Type == SkillResultType.Hit || Type == SkillResultType.Crit;
        CanCritReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromCrit;
        CanKillReleaf = target.Character.BattleModifiers == null ? false : target.Character.BattleModifiers.CanRelieveStressFromKills;
    }
}