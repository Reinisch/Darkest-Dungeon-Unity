using System.Collections.Generic;
using System.Linq;

public sealed class TargetSelectionFillCaptor : TargetSelectionDesire
{
    public TargetSelectionFillCaptor(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.FillEmptyCaptor;

        GenerateFromDataSet(dataSet);
    }

    public override bool SelectTarget(FormationUnit performer, MonsterBrainDecision decision)
    {
        if (decision.SelectedSkill.Effects.All(effect => !effect.SubEffects.Any(subeffect => subeffect is CaptureEffect)))
            return false;

        return base.SelectTarget(performer, decision);
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "can_target_deaths_door":
                    Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)token.Value;
                    break;
                case "can_target_last_hero":
                    Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)token.Value;
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}