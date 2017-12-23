using System.Collections.Generic;
using System.Text;

public enum CampTargetType
{
    None, Individual, Self, PartyOther
}
public enum CampEffectRequirement
{
    None, Religious, Nonreligious, Afflicted, DeathRecovery
}
public enum CampEffectType
{
    None, StressHealAmount, HealthHealMaxHealthPercent, RemoveBleed, RemovePoison, Buff,
    RemoveDeathRecovery, ReduceAmbushChance, RemoveDisease, StressDamageAmount, Loot,
    ReduceTorch, HealthDamageMaxHealthPercent
}

public class CampingSkill : Skill
{
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
                    sb.Append("\n" + LocalizationManager.GetString(
                        CampingSkillHelper.CampTargetTypeToStringId(Effects[i].Selection)));
            }
            else
                sb.Append("\n" + LocalizationManager.GetString(
                    CampingSkillHelper.CampTargetTypeToStringId(Effects[i].Selection)));

            string effectString = "";

            switch(Effects[i].Type)
            {
                case CampEffectType.Buff:
                    effectString = DarkestDungeonManager.Data.Buffs[Effects[i].Subtype].TooltipOverrided(Effects[i].Amount);
                    break;
                case CampEffectType.HealthDamageMaxHealthPercent:
                    effectString = string.Format(LocalizationManager.GetString(
                        "camping_skill_effect_health_damage_max_health_percent"), Effects[i].Amount);
                    break;
                case CampEffectType.HealthHealMaxHealthPercent:
                    effectString = string.Format(LocalizationManager.GetString(
                        "camping_skill_effect_health_heal_max_health_percent"), Effects[i].Amount);
                    break;
                case CampEffectType.Loot:
                    effectString = LocalizationManager.GetString("camping_skill_effect_loot_" + Effects[i].Subtype);
                    break;
                case CampEffectType.ReduceAmbushChance:
                    effectString = LocalizationManager.GetString("camping_skill_effect_reduce_ambush_chance");
                    break;
                case CampEffectType.ReduceTorch:
                    effectString = string.Format(LocalizationManager.GetString(
                        "camping_skill_effect_reduce_torch"), Effects[i].Amount);
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
                    effectString = string.Format(LocalizationManager.GetString(
                        "camping_skill_effect_stress_damage_amount"), Effects[i].Amount);
                    break;
                case CampEffectType.StressHealAmount:
                    effectString = string.Format(LocalizationManager.GetString(
                        "camping_skill_effect_stress_heal_amount"), Effects[i].Amount);
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