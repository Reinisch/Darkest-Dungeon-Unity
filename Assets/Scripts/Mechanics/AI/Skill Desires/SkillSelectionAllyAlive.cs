using System.Collections.Generic;
using System.Linq;

public sealed class SkillSelectionAllyAlive : SkillSelectionDesire
{
    private string CombatSkillId { get; set; }
    private string AllyBaseClassId { get; set; }

    public SkillSelectionAllyAlive(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    protected override bool IsRestricted(FormationUnit performer)
    {
        if (base.IsRestricted(performer))
            return true;

        if (performer.Party.Units.All(unit => unit.Character.Class != AllyBaseClassId))
            return true;

        return false;
    }

    protected override bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if (!base.IsValidSkill(performer, skill))
            return false;

        return skill.Id == CombatSkillId;
    }

    protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
    {
        return desire.Type == TargetDesireType.AllyClass;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "ally_base_class_id":
                    AllyBaseClassId = (string)dataSet["ally_base_class_id"];
                    break;
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet["combat_skill_id"];
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}