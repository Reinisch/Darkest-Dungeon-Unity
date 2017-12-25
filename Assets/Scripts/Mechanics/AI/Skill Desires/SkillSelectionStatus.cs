using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class SkillSelectionStatus : SkillSelectionDesire
{
    private StatusType EffectStatus { get; set; }
    private string CombatSkillId { get; set; }

    public SkillSelectionStatus(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    protected override bool IsValidSkill(FormationUnit performer, CombatSkill skill)
    {
        if (!base.IsValidSkill(performer, skill))
            return false;

        if (CombatSkillId != null)
            return skill.Id == CombatSkillId;

        return skill.Effects.Any(effect => effect.SubEffects.Any(IsValidSubEffect));
    }

    protected override bool IsValidTarget(FormationUnit target)
    {
        return target.Character.GetStatusEffect(EffectStatus).IsApplied;
    }

    protected override bool IsValidTargetDesire(TargetSelectionDesire desire)
    {
        return desire.Type == TargetDesireType.Marked;
    }

    protected override void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "effect_key_status":
                    switch ((string)dataSet["effect_key_status"])
                    {
                        case "tagged":
                            EffectStatus = StatusType.Marked;
                            break;
                        case "poisoned":
                            EffectStatus = StatusType.Poison;
                            break;
                        case "bleeding":
                            EffectStatus = StatusType.Bleeding;
                            break;
                        case "stunned":
                            EffectStatus = StatusType.Stun;
                            break;
                        default:
                            Debug.LogError("Unknown key status in status skill desire: " + (string)dataSet["effect_key_status"]);
                            break;
                    }
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

    private bool IsValidSubEffect(SubEffect subEffect)
    {
        if (subEffect.Type != EffectSubType.StatBuff)
            return false;

        return ((CombatStatBuffEffect)subEffect).TargetStatus == EffectStatus;
    }
}