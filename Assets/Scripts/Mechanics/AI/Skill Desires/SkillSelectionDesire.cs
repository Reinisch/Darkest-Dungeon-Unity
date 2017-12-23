using System.Collections.Generic;

public abstract class SkillSelectionDesire : IProportionValue
{
    public virtual int Chance { get; set; }

    protected Dictionary<SkillSelectRestriction, int?> Restrictions { get; set; }

    protected SkillSelectionDesire()
    {
        Restrictions = new Dictionary<SkillSelectRestriction, int?>();
        foreach (SkillSelectRestriction selectionAttribute
            in System.Enum.GetValues(typeof(SkillSelectRestriction)))
            Restrictions.Add(selectionAttribute, null);
    }

    public abstract bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision);
}