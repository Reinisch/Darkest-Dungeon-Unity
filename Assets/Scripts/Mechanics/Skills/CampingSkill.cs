using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public enum CampTargetType { None, Individual, Self, PartyOther }
public enum CampEffectRequirement { None, Religious, Nonreligious, Afflicted, DeathRecovery }
public enum CampEffectType { None, StressHealAmount, HealthHealMaxHealthPercent, RemoveBleed, RemovePoison, Buff,
                                RemoveDeathRecovery, ReduceAmbushChance, RemoveDisease, StressDamageAmount, Loot,
                                ReduceTorch, HealthDamageMaxHealthPercent }

public static class CampingSkillHelper
{
    public static string CampTargetTypeToStringId(CampTargetType targetType)
    {
        switch(targetType)
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
        switch(targetType)
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
}

public class CampingSkill : Skill
{
    public string Id { get; set; }
    public int TimeCost { get; set; }
    public int Limit { get; set; }

    public bool HasIndividualTarget
    {
        get
        {
            return Effects.Find(effect => effect.Selection == CampTargetType.Individual) != null;
        }
    }

    public List<string> Classes { get; set; }
    public List<CampEffect> Effects { get; set; }
    public CurrencyCost CurrencyCost { get; set; }

    public CampingSkill()
    {
        Effects = new List<CampEffect>();
        Classes = new List<string>();
    }

    public string Tooltip()
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
        sb.Append(LocalizationManager.GetString(ToolTipManager.GetConcat("camping_skill_name_", Id)));
        sb.AppendFormat("\n" + LocalizationManager.GetString("camping_skill_cost"), TimeCost);
        sb.AppendFormat("</color><color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);


        for (int i = 0; i < Effects.Count; i++)
        {
            if(i > 0)
            {
                if(Effects[i - 1].Selection != Effects[i].Selection)
                    sb.Append("\n" + LocalizationManager.GetString(CampingSkillHelper.CampTargetTypeToStringId(Effects[i].Selection)));
            }
            else
                sb.Append("\n" + LocalizationManager.GetString(CampingSkillHelper.CampTargetTypeToStringId(Effects[i].Selection)));

            string effectString = "";

            switch(Effects[i].Type)
            {
                case CampEffectType.Buff:
                    effectString = DarkestDungeonManager.Data.Buffs[Effects[i].Subtype].TooltipOverrided(Effects[i].Amount);
                    break;
                case CampEffectType.HealthDamageMaxHealthPercent:
                    effectString = string.Format(LocalizationManager.GetString("camping_skill_effect_health_damage_max_health_percent"), Effects[i].Amount);
                    break;
                case CampEffectType.HealthHealMaxHealthPercent:
                    effectString = string.Format(LocalizationManager.GetString("camping_skill_effect_health_heal_max_health_percent"), Effects[i].Amount);
                    break;
                case CampEffectType.Loot:
                    effectString = LocalizationManager.GetString("camping_skill_effect_loot_" + Effects[i].Subtype);
                    break;
                case CampEffectType.ReduceAmbushChance:
                    effectString = LocalizationManager.GetString("camping_skill_effect_reduce_ambush_chance");
                    break;
                case CampEffectType.ReduceTorch:
                    effectString = string.Format(LocalizationManager.GetString("camping_skill_effect_reduce_torch"), Effects[i].Amount);
                    break;
                case CampEffectType.RemoveBleed:
                    effectString = LocalizationManager.GetString("camping_skill_effect_remove_bleeding");
                    break;
                case CampEffectType.RemoveDeathRecovery:
                    effectString = LocalizationManager.GetString("camping_skill_effect_remove_deaths_door_recovery_buffs");
                    break;
                case CampEffectType.RemoveDisease:
                    effectString = LocalizationManager.GetString("camping_skill_effect_remove_disease");
                    break;
                case CampEffectType.RemovePoison:
                    effectString = LocalizationManager.GetString("camping_skill_effect_remove_poison");
                    break;
                case CampEffectType.StressDamageAmount:
                    effectString = string.Format(LocalizationManager.GetString("camping_skill_effect_stress_damage_amount"), Effects[i].Amount);
                    break;
                case CampEffectType.StressHealAmount:
                    effectString = string.Format(LocalizationManager.GetString("camping_skill_effect_stress_heal_amount"), Effects[i].Amount);
                    break;
            }

            if(Effects[i].Chance != 1)
            {
                string formatChance = LocalizationManager.GetString("camping_skill_chance_effect_format");
                effectString = string.Format(formatChance, Effects[i].Chance * 100, effectString);
            }

            switch(Effects[i].Requirement)
            {
                case CampEffectRequirement.Afflicted:
                case CampEffectRequirement.DeathRecovery:
                case CampEffectRequirement.Nonreligious:
                case CampEffectRequirement.Religious:
                    if (effectString.Length > 0)
                    {
                        string reqFormat = LocalizationManager.GetString("camping_skill_requirement_effect_format");
                        sb.AppendFormat("\n" + reqFormat, LocalizationManager.GetString(
                            CampingSkillHelper.CampRequirementToStringId(Effects[i].Requirement)), effectString);
                    }
                    break;
                case CampEffectRequirement.None:
                    if(effectString.Length > 0)
                    {
                        sb.Append("\n" + effectString);
                    }
                    break;
            }

        }

        sb.AppendFormat("</color>");
        return sb.ToString();
    }
}

public class CampEffect : ISingleProportion
{
    public CampTargetType Selection { get; set; }
    public CampEffectRequirement Requirement { get; set; }
    public CampEffectType Type { get; set; }
    public string Subtype { get; set; }
    public float Amount { get; set; }
    public float Chance { get; set; }
    public string Code { get; set; }
}
