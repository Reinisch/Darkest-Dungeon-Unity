using System.Collections.Generic;
using System.Text;

public class ActorActivityRecord : ActivityRecord
{
    public string Description { get { return cachedDescription ?? GenerateDescription(); } }
    public string HeroClass { get; private set; }

    private string Actor { get; set; }
    private string EffectInfo { get; set; }
    private List<string> Variables { get; set; }
    private ActivityType ActivityType { get; set; }
    private ActivityEffectType ActivityEffectType { get; set; }

    private string cachedDescription;

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

                    sb.AppendFormat(LocalizationManager.GetString("str_bar_stress_relief_story"),
                        DarkestDungeonManager.Data.HexColors["notable"], Actor,
                        LocalizationManager.GetString("town_name_tavern"), Variables[0]);
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
                        Trinket trinket = (Trinket)DarkestDungeonManager.Data.Items["trinket"][EffectInfo];

                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_bar_remove_trinket_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            DarkestDungeonManager.Data.HexColors[trinket.RarityId],
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

                    sb.AppendFormat(LocalizationManager.GetString("str_gambling_stress_relief_story"),
                        DarkestDungeonManager.Data.HexColors["notable"], Actor,
                        LocalizationManager.GetString("town_name_tavern"), Variables[0]);
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
                        Trinket trinket = (Trinket)DarkestDungeonManager.Data.Items["trinket"][EffectInfo];

                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_remove_trinket_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            DarkestDungeonManager.Data.HexColors[trinket.RarityId],
                            LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", EffectInfo)));
                        break;
                        #endregion
                    case ActivityEffectType.AddTrinket:
                        #region Add Trinket
                        Trinket addTrinket = (Trinket)DarkestDungeonManager.Data.Items["trinket"][EffectInfo];

                        sb.AppendFormat("\n" + LocalizationManager.GetString("str_gambling_add_trinket_story"),
                            DarkestDungeonManager.Data.HexColors["notable"], Actor,
                            LocalizationManager.GetString("town_name_tavern"),
                            DarkestDungeonManager.Data.HexColors[addTrinket.RarityId],
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

                    sb.AppendFormat(LocalizationManager.GetString("str_brothel_stress_relief_story"),
                        DarkestDungeonManager.Data.HexColors["notable"], Actor,
                        LocalizationManager.GetString("town_name_tavern"), Variables[0]);
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

                    sb.AppendFormat(LocalizationManager.GetString("str_meditation_stress_relief_story"),
                        DarkestDungeonManager.Data.HexColors["notable"], Actor,
                        LocalizationManager.GetString("town_name_abbey"), Variables[0]);
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

                    sb.AppendFormat(LocalizationManager.GetString("str_prayer_stress_relief_story"),
                        DarkestDungeonManager.Data.HexColors["notable"], Actor,
                        LocalizationManager.GetString("town_name_abbey"), Variables[0]);
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

                    sb.AppendFormat(LocalizationManager.GetString("str_flagellation_stress_relief_story"),
                        DarkestDungeonManager.Data.HexColors["notable"], Actor,
                        LocalizationManager.GetString("town_name_abbey"), Variables[0]);
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

    public ActorActivityRecord(ActivityType activityType, Hero hero, string[] quirks)
    {
        ActivityType = activityType;
        Actor = hero.HeroName;
        HeroClass = hero.ClassStringId;
        Variables = new List<string>(quirks);
    }

    public ActorActivityRecord(ActivityType activityType, ActivityEffectType effectType, Hero hero, string effectInfo, int stressHeal)
    {
        ActivityType = activityType;
        Actor = hero.HeroName;
        HeroClass = hero.ClassStringId;
        ActivityEffectType = effectType;
        EffectInfo = effectInfo;
        Variables = new List<string> {stressHeal.ToString()};
    }
}