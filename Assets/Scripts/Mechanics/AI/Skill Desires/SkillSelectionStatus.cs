using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionStatus : SkillSelectionDesire
{
    private StatusType EffectStatus { get; set; }
    private string CombatSkillId { get; set; }

    public SkillSelectionStatus(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }

    public override bool SelectSkill(FormationUnit performer, MonsterBrainDecision decision)
    {
        var monster = performer.Character as Monster;
        var availableSkills = CombatSkillId != null ? monster.Data.CombatSkills.FindAll(skill => skill.Id == CombatSkillId
                                                                                                 && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null) :
            monster.Data.CombatSkills.FindAll(skill => BattleSolver.IsSkillUsable(performer, skill)
                                                       && skill.Effects.Count > 0 && skill.Effects.Find(effect =>
                                                           effect.SubEffects.Find(subEffect => subEffect.Type == EffectSubType.StatBuff
                                                                                               && (subEffect as CombatStatBuffEffect).TargetStatus == EffectStatus) != null) != null
                                                       && performer.CombatInfo.SkillCooldowns.Find(cooldown => cooldown.SkillId == skill.Id) == null);

        if (availableSkills.Count > 0)
        {
            decision.Decision = BrainDecisionType.Perform;
            decision.SelectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
            decision.TargetInfo.Targets = BattleSolver.GetSkillAvailableTargets(performer, decision.SelectedSkill).
                FindAll(unit => unit.Character.GetStatusEffect(EffectStatus).IsApplied);
            if (decision.TargetInfo.Targets.Count == 0)
                return false;

            decision.TargetInfo.Type = decision.SelectedSkill.TargetRanks.IsSelfTarget ?
                SkillTargetType.Self : decision.SelectedSkill.TargetRanks.IsSelfFormation ?
                    SkillTargetType.Party : SkillTargetType.Enemy;

            var availableTargetDesires = new List<TargetSelectionDesire>(monster.Brain.TargetDesireSet)
                .FindAll(desire => desire.Type == TargetDesireType.Marked);

            while (availableTargetDesires.Count > 0)
            {
                TargetSelectionDesire desire = RandomSolver.ChooseByRandom(availableTargetDesires);
                if (desire.SelectTarget(performer, decision))
                    return true;
                else
                    availableTargetDesires.Remove(desire);
            }
            return false;
        }
        return false;
    }

    private void GenerateFromDataSet(Dictionary<string, object> dataSet)
    {
        foreach (var token in dataSet)
        {
            switch (token.Key)
            {
                case "base_chance":
                    Chance = (int)(double)dataSet["base_chance"];
                    break;
                case "combat_skill_id":
                    CombatSkillId = (string)dataSet["combat_skill_id"];
                    break;
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
                default:
                    Debug.LogError("Unknown token in status skill desire: " + token.Key);
                    break;
            }
        }
    }
}