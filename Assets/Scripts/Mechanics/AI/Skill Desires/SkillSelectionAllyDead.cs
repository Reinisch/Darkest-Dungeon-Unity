using System.Collections.Generic;
using System.Linq;

public sealed class SkillSelectionAllyDead : SkillSelectionDesire
{
    private bool FirstInitiativeOnly { get; set; }
    private string CombatSkillId { get; set; }
    private string AllyBaseClassId { get; set; }

    public SkillSelectionAllyDead(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    protected override bool IsRestricted(FormationUnit performer)
    {
        if (base.IsRestricted(performer))
            return true;

        if (performer.Party.Units.Any(unit => unit.Character.Class == AllyBaseClassId))
            return true;

        if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
            return true;

        return false;
    }

    protected override bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if (!base.IsValidSkill(performer, skill))
            return false;

        return skill.Id == CombatSkillId;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet["combat_skill_id"];
                    break;
                case "ally_base_class_id":
                    AllyBaseClassId = (string)dataSet["ally_base_class_id"];
                    break;
                case "first_initiative_only":
                    FirstInitiativeOnly = (bool)dataSet[token.Key];
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}