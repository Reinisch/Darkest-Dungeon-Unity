using System.Collections.Generic;

public class SkillTargetInfo
{
    public List<FormationUnit> Targets { get; set; }
    public SkillTargetType Type { get; set; }

    public CharacterMode Mode { get; private set; }
    public CombatSkill Skill { get; private set; }
    public SkillArtInfo SkillArtInfo { get; private set; }

    public SkillTargetInfo UpdateSkillInfo(FormationUnit performer, CombatSkill skill)
    {
        Mode = performer.Character.Mode;
        Skill = skill;
        SkillArtInfo = performer.Character.SkillArtInfo.Find(info => info.SkillId == skill.Id);

        if (skill.LimitPerBattle.HasValue)
            performer.CombatInfo.SkillsUsedInBattle.Add(skill.Id);
        if (skill.LimitPerTurn.HasValue)
            performer.CombatInfo.SkillsUsedThisTurn.Add(skill.Id);

        return this;
    }

    public SkillTargetInfo(List<FormationUnit> targets, SkillTargetType type)
    {
        Targets = targets;
        Type = type;
    }

    public SkillTargetInfo(FormationUnit target, SkillTargetType type)
    {
        Targets = new List<FormationUnit>();
        Targets.Add(target);
        Type = type;
    }

    public SkillTargetInfo(SkillTargetType type)
    {
        Targets = new List<FormationUnit>();
        Type = type;
    }
}