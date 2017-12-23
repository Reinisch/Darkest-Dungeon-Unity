using System.Collections.Generic;

public sealed class TargetSelectionResistance : TargetSelectionDesire
{
    private AttributeType ResistanceType { get; set; }

    public TargetSelectionResistance(Dictionary<string, object> dataSet)
    {
        Type = TargetDesireType.Resistance;

        GenerateFromDataSet(dataSet);
    }

    protected override bool ChooseTargets(List<FormationUnit> availableTargets, MonsterBrainDecision decision)
    {
        if (availableTargets.Count > 0)
        {
            decision.TargetInfo.Targets.Clear();

            if (decision.SelectedSkill.TargetRanks.IsMultitarget)
            {
                decision.TargetInfo.Targets.AddRange(availableTargets);
                return true;
            }
            else
            {
                float lowestRes = float.MaxValue;
                FormationUnit lowestResTarget = null;
                foreach (var target in availableTargets)
                {
                    if (target.Character[ResistanceType].ModifiedValue < lowestRes)
                    {
                        lowestRes = target.Character[ResistanceType].ModifiedValue;
                        lowestResTarget = target;
                    }
                }
                decision.TargetInfo.Targets.Add(lowestResTarget);
                availableTargets.Remove(lowestResTarget);

                if (decision.SelectedSkill.ExtraTargetsChance > 0 && availableTargets.Count > 0 &&
                    RandomSolver.CheckSuccess(decision.SelectedSkill.ExtraTargetsChance))
                {
                    lowestRes = 500f;
                    lowestResTarget = null;
                    foreach (var target in availableTargets)
                    {
                        if (target.Character[ResistanceType].ModifiedValue < lowestRes)
                        {
                            lowestRes = target.Character[ResistanceType].ModifiedValue;
                            lowestResTarget = target;
                        }
                    }
                    if (lowestResTarget != null)
                        decision.TargetInfo.Targets.Add(lowestResTarget);
                    return true;
                }
                return true;
            }
        }
        return false;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "is_greater_comparison":
                    break;
                case "can_target_deaths_door":
                    Parameters[TargetSelectParameter.CanTargetDeathsDoor] = (bool)token.Value;
                    break;
                case "can_target_last_hero":
                    Parameters[TargetSelectParameter.CanTargetLastHero] = (bool)token.Value;
                    break;
                case "resistance_type_id":
                    ResistanceType = CharacterHelper.StringToAttributeType((string)token.Value);
                    break;
                default:
                    ProcessBaseDataToken(token);
                    break;
            }
        }
    }
}