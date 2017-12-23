using System.Collections.Generic;

public enum TownEventTone
{
    Good, Bad, Neutral
}

public enum TownEventDataType
{
    EmbarkPartyBuff, IdleResolve, BonusRecruit, InActivityBuff,
    ActivityLock, ActivityCostChange, ProvisionTypeCostChange,
    ProvisionTypeAmountChange, UpgradeTagDiscount, FreeActivity,
    DeadRecruit, NoLevelRestriction, UpgradeTagFree, IdleBuff,
    PlotQuest
}

public class TownEvent : ISingleProportion
{
    private float baseChance;
    private int notRolledAmount;
    private int activeCooldown;

    public string Id { get; set; }
    public TownEventTone Tone { get; set; }
    public int Cooldown { get; set; }
    public float ChancePerNotRolled { get; set; }
    public float Chance
    {
        get
        {
            return baseChance + ChancePerNotRolled * notRolledAmount;
        }
        set
        {
            baseChance = value;
        }
    }

    public int MinimumWeek { get; set; }
    public int DeadHeroes { get; set; }
    public Dictionary<int, int> LevelHeroes { get; set; }
    public Dictionary<string, string> Purchases { get; set; }

    public string Sprite { get; set; }
    public string SpriteAttachment { get; set; }
    public List<string> AmbienceParameters { get; set; }
    public List<TownEventData> Data { get; set; }

    public bool IsPossible
    {
        get
        {
            if (Chance == 0 || activeCooldown > 0)
                return false;
            if (MinimumWeek > DarkestDungeonManager.Campaign.CurrentWeek)
                return false;
            if (DeadHeroes > DarkestDungeonManager.Campaign.Estate.Graveyard.Records.Count)
                return false;

            foreach (var levelHero in LevelHeroes)
                if (DarkestDungeonManager.Campaign.Heroes.FindAll(hero => hero.Resolve.Level >= levelHero.Key).Count < levelHero.Value)
                    return false;

            foreach (var purchase in Purchases)
                if (!DarkestDungeonManager.Campaign.Estate.TownPurchases.ContainsKey(purchase.Key) ||
                    !DarkestDungeonManager.Campaign.Estate.TownPurchases[purchase.Key].PurchasedUpgrades.Contains(purchase.Value))
                        return false;

            for (int i = 0; i < Data.Count; i++)
                if (Data[i].Type == TownEventDataType.PlotQuest)
                    if (DarkestDungeonManager.Campaign.CompletedPlot.Contains(Data[i].StringData))
                        return false;

            return true;
        }
    }
    public bool IsDefault
    {
        get
        {
            return activeCooldown == 0 && notRolledAmount == 0;
        }
    }
    public string EffectTooltip
    {
        get
        {
            var sb = ToolTipManager.TipBody;
            for(int i = 0; i < Data.Count; i++)
            {
                switch(Data[i].Type)
                {
                    case TownEventDataType.ActivityCostChange:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_activity_cost_change"),
                            LocalizationManager.GetString("town_activity_name_" + Data[i].StringData),
                            Data[i].NumberData);
                        sb.AppendLine();
                        break;
                    case TownEventDataType.ActivityLock:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_activity_lock"),
                            LocalizationManager.GetString("town_activity_name_" + Data[i].StringData));
                        sb.AppendLine();
                        break;
                    case TownEventDataType.BonusRecruit:
                    case TownEventDataType.DeadRecruit:
                        break;
                    case TownEventDataType.EmbarkPartyBuff:
                        var buffTooltip = DarkestDungeonManager.Data.Buffs[Data[i].StringData].ToolTip;
                        if (buffTooltip.Length > 0)
                        {
                            sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_embark_party_buff"), buffTooltip);
                            sb.AppendLine();
                        }
                        break;
                    case TownEventDataType.FreeActivity:
                        if(Data.FindAll(data => data.Type == TownEventDataType.FreeActivity).Count > 3)
                        {
                            sb.Append(LocalizationManager.GetString("town_event_info_free_all_activities"));
                            return sb.ToString();
                        }
                        else
                        {
                            sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_free_activity"),
                            LocalizationManager.GetString("town_activity_name_" + Data[i].StringData));
                            sb.AppendLine();
                        }
                        break;
                    case TownEventDataType.IdleBuff:
                        var idleBuffTooltip = DarkestDungeonManager.Data.Buffs[Data[i].StringData].ToolTip;
                        if (idleBuffTooltip.Length > 0)
                        {
                            sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_idle_buff"), idleBuffTooltip);
                            sb.AppendLine();
                        }
                        break;
                    case TownEventDataType.IdleResolve:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_idle_resolve_level"),
                            LocalizationManager.GetString("hero_class_name_" + Data[i].StringData), Data[i].NumberData);
                        sb.AppendLine();
                        break;
                    case TownEventDataType.InActivityBuff:
                        if (Tone == TownEventTone.Good)
                        {
                            sb.Append(LocalizationManager.GetString("town_event_info_in_activity_buff_stress_heal_buff"));
                            return sb.ToString();
                        }
                        else if (Tone == TownEventTone.Bad)
                        {
                            sb.Append(LocalizationManager.GetString("town_event_info_in_activity_buff_stress_heal_debuff"));
                            return sb.ToString();
                        }
                        break; 
                    case TownEventDataType.NoLevelRestriction:
                        sb.Append(LocalizationManager.GetString("town_event_info_remove_quest_hero_level_restriction"));
                        sb.AppendLine();
                        break;
                    case TownEventDataType.PlotQuest:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_plot_quest"),
                            LocalizationManager.GetString("town_quest_name_" + Data[i].StringData));
                        sb.AppendLine();
                        break;
                    case TownEventDataType.ProvisionTypeAmountChange:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_provision_item_type_amount_change"),
                            LocalizationManager.GetString("str_inventory_type_name_" + Data[i].StringData), Data[i].NumberData);
                        sb.AppendLine();
                        break;
                    case TownEventDataType.ProvisionTypeCostChange:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_provision_item_type_cost_change"),
                            LocalizationManager.GetString("str_inventory_type_name_" + Data[i].StringData), Data[i].NumberData);
                        sb.AppendLine();
                        break;
                    case TownEventDataType.UpgradeTagDiscount:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_upgrade_tag_discount"),
                            LocalizationManager.GetString("upgrade_tag_name_" + Data[i].StringData), Data[i].NumberData);
                        sb.AppendLine();
                        break;
                    case TownEventDataType.UpgradeTagFree:
                        sb.AppendFormat(LocalizationManager.GetString("town_event_info_format_upgrade_tag_free"),
                            LocalizationManager.GetString("upgrade_tag_name_" + Data[i].StringData), Data[i].NumberData);
                        sb.AppendLine();
                        break;
                }
            }
            return sb.ToString().TrimEnd('\n');
        }
    }

    public TownEvent()
    {
        LevelHeroes = new Dictionary<int, int>();
        Purchases = new Dictionary<string, string>();
        Data = new List<TownEventData>();
    }

    public void EventTriggered(bool isRolled)
    {
        if(isRolled)
        {
            if (Cooldown != 0)
                activeCooldown = Cooldown;
            notRolledAmount = 0;
        }
        else
        {
            if (ChancePerNotRolled != 0)
                notRolledAmount++;
            if (activeCooldown > 0)
                activeCooldown--;
        }
    }

    public void SetDefaultState()
    {
        activeCooldown = 0;
        notRolledAmount = 0;
    }

    public void UpdateFromSave(SaveEventData eventData)
    {
        activeCooldown = eventData.ActiveCooldown;
        notRolledAmount = eventData.NotRolledAmount;
    }

    public SaveEventData GetSaveData()
    {
        SaveEventData eventData = new SaveEventData();
        eventData.ActiveCooldown = activeCooldown;
        eventData.NotRolledAmount = notRolledAmount;
        eventData.EventId = Id;
        return eventData;
    }
}

public class TownEventGuarantee
{
    public string Dungeon { get; set; }
    public string QuestType { get; set; }
    public string EventId { get; set; }
}

public class TownEventData
{
    public TownEventDataType Type { get; set; }

    public string StringData { get; set; }
    public float NumberData { get; set; }
}