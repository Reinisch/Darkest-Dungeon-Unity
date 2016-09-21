using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum PartyActionType
{
    Embark, Result, Tutorial
}
public enum ActivityType
{
    LevelUp, StillMissing, RemoveQuirk, RemoveDisease,
    RemoveAllDiseases, LockQuirk, LockRemoveQuirk,
    BarStressHeal, GambleStressHeal, BrothelStressHeal,
    MeditationStressHeal, PrayerStressHeal, FlagellationStressHeal
}
public enum ActivityEffectType
{
    Nothing, Missing, Found,
    Lock, AddQuirk, AddBuff,
    CurrencyLost, CurrencyGained,
    AddTrinket, RemoveTrinket
}
public enum LogType
{ 
    Activity, Raid
}

public abstract class ActivityRecord
{
    public LogType LogType { get; private set; }

    public ActivityRecord(LogType logType)
    {
        LogType = logType;
    }
}

public class ActorActivityRecord : ActivityRecord
{
    string cachedDescription;

    public string Description
    {
        get
        {
            if (cachedDescription == null)
                return GenerateDescription();
            else
                return cachedDescription;
        }
        set
        {
            cachedDescription = value;
        }
    }

    public string Actor { get; set; }
    public string HeroClass { get; private set; }
    public string Affliction { get; set; }
    public string EffectInfo { get; set; }
    public List<string> Variables { get; set; }

    public ActivityType ActivityType { get; set; }
    public ActivityEffectType ActivityEffectType { get; set; }

    private string GenerateDescription()
    {
        StringBuilder sb = ToolTipManager.TipBody;
        switch (ActivityType)
        {
            case ActivityType.LevelUp:
                #region Lvl up
                if (Variables.Count < 1)
                    return "Not enough record variables.";

                sb.AppendFormat(LocalizationManager.GetString("str_is_now_a"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("hero_class_name_" + HeroClass),
                            LocalizationManager.GetString("str_resolve_" + Variables[0]), Variables[0]);
                #endregion
                break;
            case ActivityType.StillMissing:
                #region Still missing
                if (Variables.Count < 1)
                    return "Not enough record variables.";

                sb.AppendFormat(LocalizationManager.GetString("str_is_now_a"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_" + Variables[0]));
                #endregion
                break;
            case ActivityType.RemoveQuirk:
                #region Remove quirk
                if (Variables.Count < 1)
                    return "Not enough record variables.";

                sb.AppendFormat(LocalizationManager.GetString("str_treatment_remove_negative_quirk"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_sanitarium"),
                            LocalizationManager.GetString("str_quirk_name_" + Variables[0]));
                #endregion
                break;
            case ActivityType.RemoveDisease:
                #region Remove disease
                if (Variables.Count < 1)
                    return "Not enough record variables.";

                sb.AppendFormat(LocalizationManager.GetString("str_disease_treatment_remove_negative_quirk"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_sanitarium"),
                            LocalizationManager.GetString("str_quirk_name_" + Variables[0]));
                #endregion
                break;
            case ActivityType.RemoveAllDiseases:
                #region Remove all diseases
                if (Variables.Count < 1)
                    return "Not enough record variables.";

                string diseasesString = "";
                for (int i = 0; i < Variables.Count; i++)
                    if(i == 0)
                        diseasesString += LocalizationManager.GetString("str_quirk_name_" + Variables[i]);
                    else
                        diseasesString += ", " + LocalizationManager.GetString("str_quirk_name_" + Variables[i]);

                sb.AppendFormat(LocalizationManager.GetString("str_disease_treatment_removed_diseases_crit_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_sanitarium"),
                            diseasesString);
                #endregion
                break;
            case ActivityType.LockQuirk:
                #region Lock quirk
                if (Variables.Count < 1)
                    return "Not enough record variables.";

                sb.AppendFormat(LocalizationManager.GetString("str_lock_quirk_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_sanitarium"),
                            LocalizationManager.GetString("str_quirk_name_" + Variables[0]));
                #endregion
                break;
            case ActivityType.LockRemoveQuirk:
                #region Lock and remove quirk
                if (Variables.Count < 2)
                    return "Not enough record variables.";

                sb.AppendFormat(LocalizationManager.GetString("str_treatment_remove_negative_quirk"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_sanitarium"),
                            LocalizationManager.GetString("str_quirk_name_" + Variables[0]));

                sb.AppendFormat("\n" + LocalizationManager.GetString("str_lock_quirk_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_sanitarium"),
                            LocalizationManager.GetString("str_quirk_name_" + Variables[1]));
                #endregion
                break;
            case ActivityType.BarStressHeal:
                if (ActivityEffectType != ActivityEffectType.Found)
                {
                    #region Bar stress heal
                    if (Variables.Count < 1)
                        return "Not enough record variables.";

                    if (Affliction != null)
                        sb.AppendFormat(LocalizationManager.GetString("str_bar_stress_relief_affliction_cured_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            Variables[0], LocalizationManager.GetString("str_affliction_name_" + Affliction));
                    else
                        sb.AppendFormat(LocalizationManager.GetString("str_bar_stress_relief_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            Variables[0]);
                    #endregion
                }
                #region Bar side effect
                switch(ActivityEffectType)
                {
                    case ActivityEffectType.Missing:
                        #region Missing
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_go_missing_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.Found:
                        #region Found
                        sb.AppendFormat(LocalizationManager.GetString("str_bar_go_missing_found_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.Lock:
                        #region Lock
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_activity_lock_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.AddQuirk:
                        #region Add Quirk
                        switch (EffectInfo)
                        {
                            case "alcoholism":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_add_quirk_alcoholism_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "resolution":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_add_quirk_resolution_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            default:
                                sb.Append("Surprising quirk: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.AddBuff:
                        #region Add Buff
                        switch (EffectInfo)
                        {
                            case "townHungoverAccDebuff":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_add_buff_townHungoverAccDebuff_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString("str_bar_add_buff_townHungoverAccDebuff_story_desc"));
                                break;
                            case "townHungoverDEFDebuff":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_add_buff_townHungoverDEFDebuff_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString("str_bar_add_buff_townHungoverDEFDebuff_story_desc"));
                                break;
                            default:
                                sb.Append("Surprising quirk: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.RemoveTrinket:
                        #region Remove Trinket
                        Trinket trinket = DarkestDungeonManager.Data.Items["trinket"][EffectInfo] as Trinket;

                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_remove_trinket_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    DarkestDungeonManager.Data.HexColors[trinket.Rarity],
                                    trinket.Rarity == "kickstarter" ?
                                    LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", EffectInfo)) :
                                    LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", EffectInfo)));
                        break;
                        #endregion
                    case ActivityEffectType.AddTrinket:
                        #region Add Trinket
                        sb.Append("\n Not expected to get trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyLost:
                        #region Currency Lost
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_currency_lost_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    DarkestDungeonManager.Data.HexColors["harmful"],
                                    EffectInfo);
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyGained:
                        #region Currency Gained
                        sb.Append("\n Not expected to gain gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.Nothing:
                        break;
                }
                #endregion
                break;
            case ActivityType.GambleStressHeal:
                if (ActivityEffectType != ActivityEffectType.Found)
                {
                    #region Gamble stress heal
                    if (Variables.Count < 1)
                        return "Not enough record variables.";

                    if (Affliction != null)
                        sb.AppendFormat(LocalizationManager.GetString("str_gambling_stress_relief_affliction_cured_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            Variables[0], LocalizationManager.GetString("str_affliction_name_" + Affliction));
                    else
                        sb.AppendFormat(LocalizationManager.GetString("str_gambling_stress_relief_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            Variables[0]);
                    #endregion
                }
                #region Gamble side effect
                switch (ActivityEffectType)
                {
                    case ActivityEffectType.Missing:
                        #region Missing
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_go_missing_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.Found:
                        #region Found
                        sb.AppendFormat(LocalizationManager.GetString("str_gambling_go_missing_found_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.Lock:
                        #region Lock
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_activity_lock_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.AddQuirk:
                        #region Add Quirk
                        switch (EffectInfo)
                        {
                            case "gambler":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_add_quirk_gambler_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "bad_gambler":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_add_quirk_bad_gambler_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "known_cheat":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_add_quirk_known_cheat_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            default:
                                sb.Append("Surprising quirk: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.AddBuff:
                        #region Add Buff
                        sb.Append("\n Not expected to get buffed here.");
                        break;
                        #endregion
                    case ActivityEffectType.RemoveTrinket:
                        #region Remove Trinket
                        Trinket trinket = DarkestDungeonManager.Data.Items["trinket"][EffectInfo] as Trinket;

                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_remove_trinket_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    DarkestDungeonManager.Data.HexColors[trinket.Rarity],
                                    trinket.Rarity == "kickstarter" ?
                                    LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", EffectInfo)) :
                                    LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", EffectInfo)));
                        break;
                        #endregion
                    case ActivityEffectType.AddTrinket:
                        #region Add Trinket
                        Trinket addTrinket = DarkestDungeonManager.Data.Items["trinket"][EffectInfo] as Trinket;

                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_add_trinket_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    DarkestDungeonManager.Data.HexColors[addTrinket.Rarity],
                                    addTrinket.Rarity == "kickstarter" ?
                                    LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", EffectInfo)) :
                                    LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", EffectInfo)));
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyLost:
                        #region Currency Lost
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_currency_lost_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    DarkestDungeonManager.Data.HexColors["harmful"],
                                    EffectInfo);
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyGained:
                        #region Currency Gained
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_currency_gained_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    DarkestDungeonManager.Data.HexColors["notable"],
                                    EffectInfo);
                        break;
                        #endregion
                    case ActivityEffectType.Nothing:
                        break;
                }
                #endregion
                break;
            case ActivityType.BrothelStressHeal:
                if (ActivityEffectType != ActivityEffectType.Found)
                {
                    #region Brothel stress heal
                    if (Variables.Count < 1)
                        return "Not enough record variables.";

                    if (Affliction != null)
                        sb.AppendFormat(LocalizationManager.GetString("str_brothel_stress_relief_affliction_cured_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            Variables[0], LocalizationManager.GetString("str_affliction_name_" + Affliction));
                    else
                        sb.AppendFormat(LocalizationManager.GetString("str_brothel_stress_relief_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            Variables[0]);
                    #endregion
                }
                #region Brothel side effect
                switch (ActivityEffectType)
                {
                    case ActivityEffectType.Missing:
                        #region Missing
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_brothel_go_missing_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.Found:
                        #region Found
                        sb.AppendFormat(LocalizationManager.GetString("str_brothel_go_missing_found_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.Lock:
                        #region Lock
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_brothel_activity_lock_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"));
                        break;
                        #endregion
                    case ActivityEffectType.AddQuirk:
                        #region Add Quirk
                        switch (EffectInfo)
                        {
                            case "love_interest":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_brothel_add_quirk_love_interest_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "syphilis":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_brothel_add_quirk_syphilis_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "deviant_tastes":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_brothel_add_quirk_deviant_tastes_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            default:
                                sb.Append("Surprising quirk: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.AddBuff:
                        #region Add Buff
                        switch (EffectInfo)
                        {
                            case "townBrothelSPDBuff":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_brothel_add_buff_townBrothelSPDBuff_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString("str_brothel_add_buff_townBrothelSPDBuff_story_desc"));
                                break;
                            case "townBrothelSPDDebuff":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_brothel_add_buff_townBrothelSPDDebuff_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_tavern"),
                                    LocalizationManager.GetString("str_brothel_add_buff_townBrothelSPDDebuff_story_desc"));
                                break;
                            default:
                                sb.Append("Surprising buff: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.RemoveTrinket:
                        #region Remove Trinket
                        sb.Append("\n Not expected to lose trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.AddTrinket:
                        #region Add Trinket
                        sb.Append("\n Not expected to get trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyLost:
                        #region Currency Lost
                        sb.Append("\n Not expected to lose gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyGained:
                        #region Currency Gained
                        sb.Append("\n Not expected to gain gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.Nothing:
                        break;
                }
                #endregion
                break;
            case ActivityType.MeditationStressHeal:
                if (ActivityEffectType != ActivityEffectType.Found)
                {
                    #region Meditation stress heal
                    if (Variables.Count < 1)
                        return "Not enough record variables.";

                    if (Affliction != null)
                        sb.AppendFormat(LocalizationManager.GetString("str_meditation_stress_relief_affliction_cured_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"),
                            Variables[0], LocalizationManager.GetString("str_affliction_name_" + Affliction));
                    else
                        sb.AppendFormat(LocalizationManager.GetString("str_meditation_stress_relief_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"),
                            Variables[0]);
                    #endregion
                }
                #region Meditation side effect
                switch (ActivityEffectType)
                {
                    case ActivityEffectType.Missing:
                        #region Missing
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_meditation_go_missing_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.Found:
                        #region Found
                        sb.AppendFormat(LocalizationManager.GetString("str_meditation_go_missing_found_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.Lock:
                        #region Lock
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_meditation_activity_lock_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.AddQuirk:
                        #region Add Quirk
                        switch (EffectInfo)
                        {
                            case "enlightened":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_meditation_add_quirk_enlightened_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "improved_balance":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_meditation_add_quirk_improved_balance_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "meditator":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_meditation_add_quirk_meditator_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "calm":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_meditation_add_quirk_calm_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "unquiet_mind":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_meditation_add_quirk_unquiet_mind_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            default:
                                sb.Append("Surprising quirk: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.AddBuff:
                        #region Add Buff
                        sb.Append("\n Not expected to get buffed here.");
                        break;
                        #endregion
                    case ActivityEffectType.RemoveTrinket:
                        #region Remove Trinket
                        sb.Append("\n Not expected to lose trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.AddTrinket:
                        #region Add Trinket
                        sb.Append("\n Not expected to get trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyLost:
                        #region Currency Lost
                        sb.Append("\n Not expected to lose gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyGained:
                        #region Currency Gained
                        sb.Append("\n Not expected to gain gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.Nothing:
                        break;
                }
                #endregion
                break;
            case ActivityType.PrayerStressHeal:
                if (ActivityEffectType != ActivityEffectType.Found)
                {
                    #region Prayer stress heal
                    if (Variables.Count < 1)
                        return "Not enough record variables.";

                    if (Affliction != null)
                        sb.AppendFormat(LocalizationManager.GetString("str_prayer_stress_relief_affliction_cured_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"),
                            Variables[0], LocalizationManager.GetString("str_affliction_name_" + Affliction));
                    else
                        sb.AppendFormat(LocalizationManager.GetString("str_prayer_stress_relief_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"),
                            Variables[0]);
                    #endregion
                }
                #region Prayer side effect
                switch (ActivityEffectType)
                {
                    case ActivityEffectType.Missing:
                        #region Missing
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_prayer_go_missing_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.Found:
                        #region Found
                        sb.AppendFormat(LocalizationManager.GetString("str_prayer_go_missing_found_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.Lock:
                        #region Lock
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_prayer_activity_lock_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.AddQuirk:
                        #region Add Quirk
                        switch (EffectInfo)
                        {
                            case "god_fearing":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_prayer_add_quirk_god_fearing_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "witness":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_prayer_add_quirk_witness_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            default:
                                sb.Append("Surprising quirk: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.AddBuff:
                        #region Add Buff
                        sb.Append("\n Not expected to get buffed here.");
                        break;
                        #endregion
                    case ActivityEffectType.RemoveTrinket:
                        #region Remove Trinket
                        sb.Append("\n Not expected to lose trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.AddTrinket:
                        #region Add Trinket
                        sb.Append("\n Not expected to get trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyLost:
                        #region Currency Lost
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_prayer_currency_lost_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    DarkestDungeonManager.Data.HexColors["harmful"],
                                    EffectInfo);
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyGained:
                        #region Currency Gained
                        sb.Append("\n Not expected to gain gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.Nothing:
                        break;
                }
                #endregion
                break;
            case ActivityType.FlagellationStressHeal:
                if (ActivityEffectType != ActivityEffectType.Found)
                {
                    #region Flagellation stress heal
                    if (Variables.Count < 1)
                        return "Not enough record variables.";

                    if (Affliction != null)
                        sb.AppendFormat(LocalizationManager.GetString("str_flagellation_stress_relief_affliction_cured_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"),
                            Variables[0], LocalizationManager.GetString("str_affliction_name_" + Affliction));
                    else
                        sb.AppendFormat(LocalizationManager.GetString("str_flagellation_stress_relief_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"),
                            Variables[0]);
                    #endregion
                }
                #region Flagellation side effect
                switch (ActivityEffectType)
                {
                    case ActivityEffectType.Missing:
                        #region Missing
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_flagellation_go_missing_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.Found:
                        #region Found
                        sb.AppendFormat(LocalizationManager.GetString("str_flagellation_go_missing_found_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.Lock:
                        #region Lock
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_flagellation_activity_lock_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_abbey"));
                        break;
                        #endregion
                    case ActivityEffectType.AddQuirk:
                        #region Add Quirk
                        switch (EffectInfo)
                        {
                            case "flagellant":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_flagellation_add_quirk_flagellant_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            case "faithless":
                                sb.AppendFormat("\n" + LocalizationManager.GetString("str_flagellation_add_quirk_faithless_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString(EffectInfo));
                                break;
                            default:
                                sb.Append("Surprising quirk: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.AddBuff:
                        #region Add Buff
                        switch (EffectInfo)
                        {
                            case "townFlagellationDMGLowBuff":
                                sb.AppendFormat("\n" +
                                    LocalizationManager.GetString("str_flagellation_add_buff_townFlagellationDMGLowBuff_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString("str_flagellation_add_buff_townFlagellationDMGLowBuff_story_desc"));
                                break;
                            case "townFlagellationDMGLowDebuff":
                                sb.AppendFormat("\n" + 
                                    LocalizationManager.GetString("str_flagellation_add_buff_townFlagellationDMGLowDebuff_story"),
                                    DarkestDungeonManager.Data.HexColors["notable"], Actor,
                                    LocalizationManager.GetString("town_name_abbey"),
                                    LocalizationManager.GetString("str_flagellation_add_buff_townFlagellationDMGLowDebuff_story_desc"));
                                break;
                            default:
                                sb.Append("Surprising buff: " + EffectInfo);
                                break;
                        }
                        break;
                        #endregion
                    case ActivityEffectType.RemoveTrinket:
                        #region Remove Trinket
                        sb.Append("\n Not expected to lose trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.AddTrinket:
                        #region Add Trinket
                        sb.Append("\n Not expected to get trinket here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyLost:
                        #region Currency Lost
                        sb.Append("\n Not expected to lose gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.CurrencyGained:
                        #region Currency Gained
                        sb.Append("\n Not expected to gain gold here.");
                        break;
                        #endregion
                    case ActivityEffectType.Nothing:
                        break;
                }
                #endregion
                break;
        }
        cachedDescription = sb.ToString();
        return cachedDescription;
    }

    public ActorActivityRecord(ActivityType activityType, Hero hero, int stressHeal) 
        : base(LogType.Activity)
    {
        ActivityType = activityType;
        Actor = hero.HeroName;
        HeroClass = hero.ClassStringId;
        Variables = new List<string>();
        Variables.Add(stressHeal.ToString());
    }

    public ActorActivityRecord(ActivityType activityType, Hero hero, string[] quirks)
        : base(LogType.Activity)
    {
        ActivityType = activityType;
        Actor = hero.HeroName;
        HeroClass = hero.ClassStringId;
        Variables = new List<string>(quirks);
    }

    public ActorActivityRecord(ActivityType activityType, ActivityEffectType effectType, Hero hero, string effectInfo, int stressHeal)
        : base(LogType.Activity)
    {
        ActivityType = activityType;
        Actor = hero.HeroName;
        HeroClass = hero.ClassStringId;
        ActivityEffectType = effectType;
        EffectInfo = effectInfo;
        Variables = new List<string>();
        Variables.Add(stressHeal.ToString());
    }

    public ActorActivityRecord(ActivityType activityType, ActivityEffectType effectType, Hero hero, int stressHeal)
        : base(LogType.Activity)
    {
        ActivityType = activityType;
        Actor = hero.HeroName;
        HeroClass = hero.ClassStringId;
        ActivityEffectType = effectType;
        Variables = new List<string>();
        Variables.Add(stressHeal.ToString());
    }
}

public class PartyActivityRecord : ActivityRecord
{
    string cachedDescription;

    public string Description
    {
        get
        {
            if (cachedDescription == null)
                return GenerateDescription();
            else
                return cachedDescription;
        }
        set
        {
            cachedDescription = value;
        }
    }

    public PartyActionType PartyActionType { get; set; }
    public List<string> Names { get; set; }
    public List<string> Classes { get; set; }
    public List<bool> Alive { get; set; }

    public bool IsSuccessfull { get; set; }

    public string QuestType { get; set; }
    public string QuestDifficulty { get; set; }
    public string QuestLength { get; set; }
    public string Dungeon { get; set; }

    private string GenerateDescription()
    {
        StringBuilder sb = ToolTipManager.TipBody;
        switch (PartyActionType)
        {
            case PartyActionType.Tutorial:
                #region Tutorial
                if (!Alive[0] || !Alive[1])
                {
                    if (!Alive[0] && !Alive[1])
                    {
                        string tutAllPerish = string.Format(LocalizationManager.GetString("str_party_members_2"),
                            DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1]);
                        sb.AppendFormat(LocalizationManager.GetString("str_tutorial_all_perished"), tutAllPerish);
                    }
                    else
                    {
                        string tutSurv = string.Format(LocalizationManager.GetString("str_party_members_1"),
                            DarkestDungeonManager.Data.HexColors["notable"], Alive[0] ? Names[0] : Names[1]);
                        string tutPerish = string.Format(LocalizationManager.GetString("str_party_members_1"),
                            DarkestDungeonManager.Data.HexColors["harmful"], Alive[1] ? Names[0] : Names[1]);
                        sb.AppendFormat(LocalizationManager.GetString("str_tutorial_alive_perished_mixed"), tutSurv, tutPerish);
                    }
                }
                else
                {
                    string tutWinners = string.Format(LocalizationManager.GetString("str_party_members_2"),
                            DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1]);
                    sb.AppendFormat(LocalizationManager.GetString("str_tutorial_all_alive"), tutWinners);
                }
                #endregion
                break;
            case PartyActionType.Embark:
                #region Embark
                string strEmbarkers = string.Format(LocalizationManager.GetString("str_party_members_4"),
                            DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1], Names[2], Names[3]);
                string embarkId = "str_embarked_on_";
                switch(QuestType)
                {
                    case "explore":
                    case "cleanse":
                    case "kill_boss":
                    case "gather":
                    case "inventory_activate":
                        embarkId += QuestType + "_" + Dungeon;
                        break;
                    default:
                        embarkId += QuestType;
                        break;
                }
                sb.AppendFormat(LocalizationManager.GetString(embarkId), strEmbarkers,
                    LocalizationManager.GetString("str_difficulty_" + QuestDifficulty),
                    LocalizationManager.GetString("town_quest_length_" + QuestLength));
                #endregion
                break;
            case PartyActionType.Result:
                #region Result
                string strRaiders = string.Format(LocalizationManager.GetString("str_party_members_4"),
                            DarkestDungeonManager.Data.HexColors["notable"], Names[0], Names[1], Names[2], Names[3]);
                string returnId = "str_returned_from_";
                switch(QuestType)
                {
                    case "explore":
                    case "cleanse":
                    case "kill_boss":
                    case "gather":
                    case "inventory_activate":
                        returnId += QuestType + "_" + Dungeon;
                        break;
                    default:
                        returnId += QuestType;
                        break;
                }
                if (IsSuccessfull)
                    returnId += "_success";
                else
                    returnId += "_failure";

                sb.AppendFormat(LocalizationManager.GetString(returnId), strRaiders,
                    LocalizationManager.GetString("str_difficulty_" + QuestDifficulty),
                    LocalizationManager.GetString("town_quest_length_" + QuestLength));

                if(Alive.Contains(false))
                {
                    if(Alive.Contains(true))
                    {
                        string deadHeroes = "";
                        switch(Alive.FindAll(x => !x).Count)
                        {
                            case 1:
                                deadHeroes = string.Format(LocalizationManager.GetString("str_party_members_1"),
                                    DarkestDungeonManager.Data.HexColors["harmful"],
                                    Alive[0] ? Alive[1] ? Alive[2] ? Names[3] : Names[2] : Names[1] : Names[0]);
                                break;
                            case 2:
                                int deadOne = 0; int deadTwo = 0;
                                for(int i = 0; i < Alive.Count; i++)
                                    if(!Alive[i]) deadTwo = i;
                                for (int i = Alive.Count - 1; i >= 0; i--)
                                    if(!Alive[i]) deadOne = i;
                                deadHeroes = string.Format(LocalizationManager.GetString("str_party_members_2"),
                                    DarkestDungeonManager.Data.HexColors["harmful"], Names[deadOne], Names[deadTwo]);
                                break;
                            case 3:
                                int dead = 0; int live = Alive.IndexOf(true);
                                deadHeroes = string.Format(LocalizationManager.GetString("str_party_members_3"),
                                    DarkestDungeonManager.Data.HexColors["harmful"],
                                    dead++ == live ? Names[dead++] : Names[dead - 1],
                                    dead++ == live ? Names[dead++] : Names[dead - 1],
                                    dead++ == live ? Names[dead++] : Names[dead - 1]);
                                break;
                        }
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_perished"), deadHeroes);
                    }
                    else
                    {
                        string deadParty = string.Format(LocalizationManager.GetString("str_party_members_4"),
                            DarkestDungeonManager.Data.HexColors["harmful"], Names[0], Names[1], Names[2], Names[3]);
                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_allperished"), deadParty);
                    }
                }
                #endregion
                break;
        }
        cachedDescription = sb.ToString();
        return cachedDescription;
    }

    public PartyActivityRecord() : base(LogType.Raid)
    {
        Names = new List<string>();
        Classes = new List<string>();
        Alive = new List<bool>();
    }

    public PartyActivityRecord(PartyActionType actionType, string questType, string questDifficulty, 
        string questLength, string dungeon, List<Hero> heroes) : base (LogType.Raid)
    {
        PartyActionType = actionType;
        QuestType = questType;
        QuestDifficulty = questDifficulty;
        QuestLength = questLength;
        Dungeon = dungeon;

        Names = new List<string>(heroes.Select(item => item.HeroName));
        Classes = new List<string>(heroes.Select(item => item.Class));
        Alive = new List<bool>();
        for (int i = 0; i < Names.Count; i++)
            Alive.Add(true);
    }

    public PartyActivityRecord(PartyActionType actionType, RaidManager raidManager)
        : base(LogType.Raid)
    {
        PartyActionType = actionType;
        QuestType = raidManager.Quest.Type;
        QuestDifficulty = raidManager.Quest.Difficulty.ToString();
        QuestLength = raidManager.Quest.Length.ToString();
        Dungeon = raidManager.Quest.Dungeon;
        IsSuccessfull = raidManager.Status == RaidStatus.Success;

        Names = new List<string>(raidManager.RaidParty.HeroInfo.Select(item => item.Hero.HeroName));
        Classes = new List<string>(raidManager.RaidParty.HeroInfo.Select(item => item.Hero.Class));
        if (actionType == global::PartyActionType.Result)
            Alive = new List<bool>(raidManager.RaidParty.HeroInfo.Select(item => item.IsAlive));
        else
        {
            Alive = new List<bool>();
            for (int i = 0; i < Names.Count; i++)
                Alive.Add(true);
        }
    }

    public PartyActivityRecord(PartyActionType actionType, string questType, string questDifficulty,
        string questLength, string dungeon, List<Hero> heroes, bool[] aliveStatus, bool isSuccessfull)
        : base(LogType.Raid)
    {
        PartyActionType = actionType;
        QuestType = questType;
        QuestDifficulty = questDifficulty;
        QuestLength = questLength;
        Dungeon = dungeon;
        IsSuccessfull = isSuccessfull;

        Names = new List<string>(heroes.Select(item => item.HeroName));
        Classes = new List<string>(heroes.Select(item => item.Class));
        Alive = new List<bool>(aliveStatus);
    }

    public PartyActivityRecord(PartyActivityRecord embarkRecord, bool[] aliveStatus, bool isSuccessfull)
        : base(LogType.Raid)
    {
        PartyActionType = PartyActionType.Result;
        QuestType = embarkRecord.QuestType;
        QuestDifficulty = embarkRecord.QuestDifficulty;
        QuestLength = embarkRecord.QuestLength;
        Dungeon = embarkRecord.Dungeon;
        IsSuccessfull = isSuccessfull;

        Names = embarkRecord.Names.ToList();
        Classes = embarkRecord.Classes.ToList();
        Alive = new List<bool>(aliveStatus);
    }
}