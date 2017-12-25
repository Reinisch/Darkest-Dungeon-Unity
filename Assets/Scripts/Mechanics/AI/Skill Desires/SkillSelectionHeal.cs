using System.Collections.Generic;

public sealed class SkillSelectionHeal : SkillSelectionDesire
{
    private string CombatSkillId { get; set; }
    private float HpRatioThreshold { get; set; }
    private bool FirstInitiativeOnly { get; set; }

    public SkillSelectionHeal(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    protected override bool IsRestricted(FormationUnit performer)
    {
        if (base.IsRestricted(performer))
            return true;

        if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
            return true;

        return false;
    }

    protected override bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if (!base.IsValidSkill(performer, skill))
            return false;

        if(string.IsNullOrEmpty(CombatSkillId))
            return skill.Id == CombatSkillId;

        return skill.Heal != null;
    }

    protected override bool IsValidTarget(FormationUnit target)
    {
        return target.Character.HealthRatio < HpRatioThreshold;
    }

    protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
    {
        return desire.Type == TargetDesireType.Health;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "hp_ratio_treshold":
                    HpRatioThreshold = (float)(double)dataSet["hp_ratio_treshold"];
                    break;
                case "first_initiative_only":
                    FirstInitiativeOnly = (bool)dataSet[token.Key];
                    break;
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet[token.Key];
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}