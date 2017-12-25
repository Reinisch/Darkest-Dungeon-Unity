using System.Collections.Generic;
using System.Linq;

public sealed class SkillSelectionFillEmptyCaptor : SkillSelectionDesire
{
    private bool CanTargetDeathsDoor { get; set; }
    private bool CanTargetLastHero { get; set; }
    private bool FirstInitiativeOnly { get; set; }

    public SkillSelectionFillEmptyCaptor(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    protected override bool IsRestricted(FormationUnit performer)
    {
        if (base.IsRestricted(performer))
            return true;

        if (FirstInitiativeOnly && performer.CombatInfo.CurrentInitiative != 1)
            return true;

        if (!CanTargetDeathsDoor && RaidSceneManager.BattleGround.NonDeathsDoorHeroes == 0)
            return true;

        if (!CanTargetLastHero && RaidSceneManager.BattleGround.HeroNumber == 1)
            return true;

        if (performer.Party.Units.All(unit => unit.Character.EmptyCaptor == null))
            return true;

        return false;
    }

    protected override bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if (!base.IsValidSkill(performer, skill))
            return false;

        return skill.Effects.Any(effect => effect.SubEffects.Any(IsValidSubEffect));
    }

    protected override bool IsValidTarget(FormationUnit target)
    {
        if (!CanTargetDeathsDoor && target.Character.AtDeathsDoor)
            return false;

        return true;
    }

    protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
    {
        return desire.Type == TargetDesireType.FillEmptyCaptor;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "can_target_deaths_door":
                    CanTargetDeathsDoor = (bool)dataSet["can_target_deaths_door"];
                    break;
                case "can_target_last_hero":
                    CanTargetLastHero = (bool)dataSet["can_target_last_hero"];
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

    private bool IsValidSubEffect(SubEffect subEffect)
    {
        return subEffect.Type == EffectSubType.Capture;
    }
}