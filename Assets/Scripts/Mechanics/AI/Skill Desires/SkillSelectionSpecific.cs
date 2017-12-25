using System.Collections.Generic;

public sealed class SkillSelectionSpecific : SkillSelectionDesire
{
    public override int Chance
    {
        get
        {
            if (RaidSceneManager.Instanse != null)
                return base.Chance + PerRoundChance * (RaidSceneManager.BattleGround.Round.RoundNumber - 1);
            return base.Chance;
        }
        set
        {
            base.Chance = value;
        }
    }

    private string CombatSkillId { get; set; }
    private bool FirstInitiativeOnly { get; set; }
    private int PerRoundChance { get; set; }

    public SkillSelectionSpecific(Dictionary<string, object> dataSet)
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

        return skill.Id == CombatSkillId;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "hp_ratio_treshold":
                    break;
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet["combat_skill_id"];
                    break;
                case "per_round_chance":
                    PerRoundChance = Chance = (int)((double)dataSet[token.Key] * 100);
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