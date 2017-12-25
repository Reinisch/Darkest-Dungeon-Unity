using UnityEngine;

public static class CampingSkillHelper
{
    public static string CampTargetTypeToStringId(CampTargetType targetType)
    {
        switch (targetType)
        {
            case CampTargetType.Individual:
                return "camping_skill_selection_individual";
            case CampTargetType.PartyOther:
                return "camping_skill_selection_party_other";
            case CampTargetType.Self:
                return "camping_skill_selection_self";
            default:
                return "camping_skill_selection_self";
        }
    }

    public static string CampRequirementToStringId(CampEffectRequirement requirement)
    {
        switch (requirement)
        {
            case CampEffectRequirement.Afflicted:
                return "camping_skill_requirement_afflicted";
            case CampEffectRequirement.DeathRecovery:
                return "camping_skill_requirement_has_deaths_door_recovery_buffs";
            case CampEffectRequirement.Nonreligious:
                return "camping_skill_requirement_not_religious";
            case CampEffectRequirement.Religious:
                return "camping_skill_requirement_religious";
            default:
                return "";
        }
    }

    public static CampEffectType StringToCampEffectType(string effectType)
    {
        switch (effectType)
        {
            case "stress_heal_amount":
                return CampEffectType.StressHealAmount;
            case "stress_damage_amount":
                return CampEffectType.StressDamageAmount;
            case "health_heal_max_health_percent":
                return CampEffectType.HealthHealMaxHealthPercent;
            case "health_damage_max_health_percent":
                return CampEffectType.HealthDamageMaxHealthPercent;
            case "remove_bleeding":
                return CampEffectType.RemoveBleed;
            case "remove_poison":
                return CampEffectType.RemovePoison;
            case "buff":
                return CampEffectType.Buff;
            case "remove_deaths_door_recovery_buffs":
                return CampEffectType.RemoveDeathRecovery;
            case "reduce_ambush_chance":
                return CampEffectType.ReduceAmbushChance;
            case "remove_disease":
                return CampEffectType.RemoveDisease;
            case "loot":
                return CampEffectType.Loot;
            case "reduce_torch":
                return CampEffectType.ReduceTorch;
            default:
                Debug.LogError("Unknown camp effect type: " + effectType);
                return CampEffectType.None;
        }
    }

    public static CampTargetType StringToCampTargetType(string targetType)
    {
        switch (targetType)
        {
            case "individual":
                return CampTargetType.Individual;
            case "self":
                return CampTargetType.Self;
            case "party_other":
                return CampTargetType.PartyOther;
            default:
                Debug.LogError("Unknown camp effect target type: " + targetType);
                return CampTargetType.None;
        }
    }

    public static CampEffectRequirement StringToCampEffectRequirement(string requirement)
    {
        switch (requirement)
        {
            case "afflicted":
                return CampEffectRequirement.Afflicted;
            case "not_religious":
                return CampEffectRequirement.Nonreligious;
            case "religious":
                return CampEffectRequirement.Religious;
            case "has_deaths_door_recovery_buffs":
                return CampEffectRequirement.DeathRecovery;
            default:
                Debug.LogError("Unknown camp effect requirement: " + requirement);
                return CampEffectRequirement.None;
        }
    }

    public static bool IsWaitingForNext(this CampTargetType targetType, CampTargetType nextTargetType)
    {
        switch (targetType)
        {
            case CampTargetType.Individual:
            case CampTargetType.PartyOther:
                if (nextTargetType != CampTargetType.Self)
                    return true;
                break;
            case CampTargetType.Self:
                if (nextTargetType == CampTargetType.Self)
                    return true;
                break;
        }

        return false;
    }
}