using System.Collections.Generic;
using System.Text;

public enum UpgradeStatus { Purchased, Available, Locked }

public class TownUpgrade
{
    public string Code { get; set; }
    public List<CurrencyCost> Cost { get; set; }
    public List<PrerequisiteReqirement> Prerequisites { get; set; }

    public TownUpgrade()
    {
        Cost = new List<CurrencyCost>();
        Prerequisites = new List<PrerequisiteReqirement>();
    }

    public string PrerequisitesTooltip()
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
        sb.Append(LocalizationManager.GetString("upgrade_prerequisite_tooltip_title"));
        sb.Append("</color>");

        string prereqFormat = LocalizationManager.GetString("upgrade_prerequisite_requirement_tooltip_body_format");
        for (int i = 0; i < Prerequisites.Count; i++)
        {
            if (DarkestDungeonManager.Campaign.Estate.IsRequirementMet(Prerequisites[i]))
            {
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.AppendFormat(prereqFormat, LocalizationManager.GetString("upgrade_tree_name_" + Prerequisites[i].TreeId),
                    DarkestDungeonManager.Campaign.Estate.GetUpgradeLevel(Prerequisites[i]));
                sb.Append("</color>");
            }
            else
            {
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.AppendFormat(prereqFormat, LocalizationManager.GetString("upgrade_tree_name_" + Prerequisites[i].TreeId),
                    DarkestDungeonManager.Campaign.Estate.GetUpgradeLevel(Prerequisites[i]));
                sb.Append("</color>");
            }
        }

        return sb.ToString();
    }
}

public class HeroUpgrade : TownUpgrade
{
    public int PrerequisiteResolveLevel { get; set; }

    public HeroUpgrade()
    {
        Cost = new List<CurrencyCost>();
        Prerequisites = new List<PrerequisiteReqirement>();
    }

    public string PrerequisitesTooltip(Hero hero, Estate estate)
    {
        StringBuilder sb = ToolTipManager.TipBody;
        sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["notable"]);
        sb.Append(LocalizationManager.GetString("upgrade_prerequisite_tooltip_title"));
        sb.Append("</color>");

        string prereqFormat = LocalizationManager.GetString("upgrade_prerequisite_requirement_tooltip_body_format");
        for (int i = 0; i < Prerequisites.Count; i++)
        {
            if (estate.IsRequirementMet(hero.RosterId, Prerequisites[i]))
            {
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.AppendFormat(prereqFormat, LocalizationManager.GetString(
                    "upgrade_tree_name_" + Prerequisites[i].TreeId), estate.GetUpgradeLevel(Prerequisites[i]));
                sb.Append("</color>");
            }
            else
            {
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.AppendFormat(prereqFormat, LocalizationManager.GetString(
                    "upgrade_tree_name_" + Prerequisites[i].TreeId), estate.GetUpgradeLevel(Prerequisites[i]));
                sb.Append("</color>");
            }
        }
        
        if (PrerequisiteResolveLevel != 0)
        {
            if(hero.Resolve.Level < PrerequisiteResolveLevel)
            {
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["harmful"]);
                sb.AppendFormat(LocalizationManager.GetString(
                    "upgrade_prerequisite_resolve_level_tooltip_body_format"), PrerequisiteResolveLevel);
                sb.Append("</color>");
            }
            else
            {
                sb.AppendFormat("\n<color={0}>", DarkestDungeonManager.Data.HexColors["neutral"]);
                sb.AppendFormat(LocalizationManager.GetString(
                    "upgrade_prerequisite_resolve_level_tooltip_body_format"), PrerequisiteResolveLevel);
                sb.Append("</color>");
            }
        }
        return sb.ToString();
    }
}

public interface ITownUpgrade
{
    string UpgradeCode { get; set; }
    string TreeId { get; set; }
    string ToolTip { get; }
}

public class CostUpgrade : ITownUpgrade
{
    public CurrencyCost Cost { get; set; }
    public string TreeId { get; set; }
    public string UpgradeCode { get; set; }

    public string ToolTip
    {
        get
        {
            StringBuilder sb = ToolTipManager.TipBody;

            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            switch (TreeId)
            {
                case "sanitarium.disease_quirk_cost":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_reduces_disease_quirk_treatment_cost_format"), 13);
                    break;
                default:
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_reduces_treatment_cost_format"), Cost.Amount);
                    break;
            }
            sb.Append("</color>");

            return sb.ToString();
        }
    }
}

public class RecruitUpgrade : ITownUpgrade
{
    public int Level { get; set; }
    public float Chance { get; set; }
    public int ExtraPositiveQuirks { get; set; }
    public int ExtraNegativeQuirks { get; set; }
    public int ExtraCombatSkills { get; set; }
    public int ExtraCampingSkills { get; set; }
    public string TreeId { get; set; }
    public string UpgradeCode { get; set; }

    public string ToolTip
    {
        get
        {
            StringBuilder sb = ToolTipManager.TipBody;

            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            sb.Append(LocalizationManager.GetString("str_stage_coach_upgraded_recruits_upgrade_lvl_" + Level.ToString()));
            sb.Append("</color>");

            return sb.ToString();
        }
    }
}

public class SlotUpgrade : ITownUpgrade
{
    public int NumberOfSlots { get; set; }
    public string TreeId { get; set; }
    public string UpgradeCode { get; set; }

    public string ToolTip
    {
        get
        {
            StringBuilder sb = ToolTipManager.TipBody;

            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            switch (TreeId)
            {
                case "sanitarium.slots":
                    if(UpgradeCode == "a" || UpgradeCode == "c")
                        sb.AppendFormat(LocalizationManager.GetString(
                            "upgrade_tree_tooltip_description_increases_number_of_disease_slots_format"), NumberOfSlots);
                    else
                        sb.AppendFormat(LocalizationManager.GetString(
                            "upgrade_tree_tooltip_description_increases_number_of_quirk_slots_format"), NumberOfSlots);
                    break;
                case "nomad_wagon.numitems":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_increases_number_of_items_generated_format"), NumberOfSlots);
                    break;
                case "stage_coach.numrecruits":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_increases_number_of_heroes_generated_format"), NumberOfSlots);
                    break;
                case "stage_coach.rostersize":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_increases_size_of_roster_format"), NumberOfSlots);
                    break;
                default:
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_increases_number_of_slots_format"), NumberOfSlots);
                    break;
            }
            sb.Append("</color>");

            return sb.ToString();
        }
    }
}

public class StressUpgrade : ITownUpgrade
{
    public int StressHeal { get; set; }
    public string TreeId { get; set; }
    public string UpgradeCode { get; set; }

    public string ToolTip
    {
        get
        {
            StringBuilder sb = ToolTipManager.TipBody;

            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            switch (TreeId)
            {
                default:
                    sb.Append(LocalizationManager.GetString("upgrade_tree_tooltip_description_increases_stress_recovery"));
                    break;
            }
            sb.Append("</color>");

            return sb.ToString();
        }
    }
}

public class DiscountUpgrade : ITownUpgrade
{
    public float Percent { get; set; }
    public string TreeId { get; set; }
    public string UpgradeCode { get; set; }

    public string ToolTip
    {
        get
        {
            StringBuilder sb = ToolTipManager.TipBody;

            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            switch (TreeId)
            {
                case "blacksmith.cost":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_reduces_cost_of_weapon_and_armour_upgrades_format"), Percent * 100);
                    break;
                case "guild.cost":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_reduces_cost_of_combat_skill_upgrades_format"), Percent * 100);
                    break;
                case "camping_trainer.cost":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_reduces_cost_of_camping_skills_format"), Percent * 100);
                    break;
                case "nomad_wagon.cost":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_reduces_cost_of_items_format"), Percent * 100);
                    break;
            }
            sb.Append("</color>");

            return sb.ToString();
        }
    }
}

public class ChanceUpgrade : ITownUpgrade
{
    public float Chance { get; set; }
    public string TreeId { get; set; }
    public string UpgradeCode { get; set; }

    public string ToolTip
    {
        get
        {
            StringBuilder sb = ToolTipManager.TipBody;

            sb.AppendFormat("<color={0}>", DarkestDungeonManager.Data.HexColors["equipment_tooltip_body"]);
            switch (TreeId)
            {
                case "sanitarium.disease_quirk_cost":
                    sb.AppendFormat(LocalizationManager.GetString(
                        "upgrade_tree_tooltip_description_disease_cure_all_chance_format"), Chance*100);
                    break;
            }
            sb.Append("</color>");

            return sb.ToString();
        }
    }
}