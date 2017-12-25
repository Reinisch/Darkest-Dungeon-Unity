using System.Collections.Generic;

public sealed class SkillSelectionPreferred : SkillSelectionDesire
{
    public SkillSelectionPreferred(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    protected override bool IsRestricted(FormationUnit performer)
    {
        if (base.IsRestricted(performer))
            return true;

        var monster = (Monster)performer.Character;
        if (monster.Data.PreferableSkill < 0)
            return true;

        if (monster.Data.PreferableSkill >= monster.Data.CombatSkills.Count)
            return true;

        return false;
    }

    protected override bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if (!base.IsValidSkill(performer, skill))
            return false;

        var monster = (Monster)performer.Character;
        return monster.Data.CombatSkills.IndexOf(skill) == monster.Data.PreferableSkill;
    }
}