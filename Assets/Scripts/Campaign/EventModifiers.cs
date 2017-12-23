using System.Collections.Generic;
using System.IO;

public class EventModifiers : IBinarySaveData
{
    public bool NoLevelRestrictions { get; set; }
    public Dictionary<string, bool> ActivityLocks { get; set; }
    public Dictionary<string, bool> FreeActivities { get; set; }
    public Dictionary<string, float> ActivityCostModifiers { get; set; }
    public Dictionary<string, float> ProvisionCostModifiers { get; set; }
    public Dictionary<string, float> ProvisionAmountModifiers { get; set; }
    public Dictionary<string, float> UpgradeTagCostModifiers { get; set; }
    public Dictionary<string, int> FreeUpgradeTags { get; set; }
    public List<TownEventData> EventData { get; set; }
    public bool IsMeetingSaveCriteria { get { return true; } }

    public EventModifiers()
    {
        ActivityLocks = new Dictionary<string, bool>();
        FreeActivities = new Dictionary<string, bool>();
        ActivityCostModifiers = new Dictionary<string, float>();
        ProvisionCostModifiers = new Dictionary<string, float>();
        ProvisionAmountModifiers = new Dictionary<string, float>();
        UpgradeTagCostModifiers = new Dictionary<string, float>();
        FreeUpgradeTags = new Dictionary<string, int>();

        EventData = new List<TownEventData>();
    }

    public void Reset()
    {
        NoLevelRestrictions = false;

        ActivityLocks.Clear();
        FreeActivities.Clear();
        ActivityCostModifiers.Clear();
        ProvisionCostModifiers.Clear();
        ProvisionAmountModifiers.Clear();
        UpgradeTagCostModifiers.Clear();
        FreeUpgradeTags.Clear();

        EventData.Clear();
    }

    public void IncludeEvent(TownEvent townEvent)
    {
        for (int i = 0; i < townEvent.Data.Count; i++)
        {
            EventData.AddRange(townEvent.Data);

            switch (townEvent.Data[i].Type)
            {
                case TownEventDataType.ActivityCostChange:
                    if (!ActivityCostModifiers.ContainsKey(townEvent.Data[i].StringData))
                        ActivityCostModifiers.Add(townEvent.Data[i].StringData, 0);

                    ActivityCostModifiers[townEvent.Data[i].StringData] += townEvent.Data[i].NumberData;
                    break;
                case TownEventDataType.ActivityLock:
                    if (!ActivityLocks.ContainsKey(townEvent.Data[i].StringData))
                        ActivityLocks.Add(townEvent.Data[i].StringData, true);
                    break;
                case TownEventDataType.FreeActivity:
                    if (!FreeActivities.ContainsKey(townEvent.Data[i].StringData))
                        FreeActivities.Add(townEvent.Data[i].StringData, true);
                    break;
                case TownEventDataType.NoLevelRestriction:
                    NoLevelRestrictions = true;
                    break;
                case TownEventDataType.ProvisionTypeAmountChange:
                    if (!ProvisionAmountModifiers.ContainsKey(townEvent.Data[i].StringData))
                        ProvisionAmountModifiers.Add(townEvent.Data[i].StringData, 0);

                    ProvisionAmountModifiers[townEvent.Data[i].StringData] += townEvent.Data[i].NumberData;
                    break;
                case TownEventDataType.ProvisionTypeCostChange:
                    if (!ProvisionCostModifiers.ContainsKey(townEvent.Data[i].StringData))
                        ProvisionCostModifiers.Add(townEvent.Data[i].StringData, 0);

                    ProvisionCostModifiers[townEvent.Data[i].StringData] += townEvent.Data[i].NumberData;
                    break;
                case TownEventDataType.UpgradeTagDiscount:
                    if (!UpgradeTagCostModifiers.ContainsKey(townEvent.Data[i].StringData))
                        UpgradeTagCostModifiers.Add(townEvent.Data[i].StringData, 0);

                    UpgradeTagCostModifiers[townEvent.Data[i].StringData] += townEvent.Data[i].NumberData;
                    break;
                case TownEventDataType.UpgradeTagFree:
                    if (!FreeUpgradeTags.ContainsKey(townEvent.Data[i].StringData))
                        FreeUpgradeTags.Add(townEvent.Data[i].StringData, 0);

                    FreeUpgradeTags[townEvent.Data[i].StringData] += (int)townEvent.Data[i].NumberData;
                    break;
                case TownEventDataType.BonusRecruit:
                    DarkestDungeonManager.Campaign.Estate.RestockBonus(townEvent.Data[i].StringData,
                        (int)townEvent.Data[i].NumberData);
                    break;
                case TownEventDataType.DeadRecruit:
                    DarkestDungeonManager.Campaign.Estate.RestockFromGrave((int)townEvent.Data[i].NumberData);
                    break;
            }
        }
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(NoLevelRestrictions);

        bw.Write(ActivityLocks.Count);
        foreach (var entry in ActivityLocks)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
        bw.Write(FreeActivities.Count);
        foreach (var entry in FreeActivities)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
        bw.Write(ActivityCostModifiers.Count);
        foreach (var entry in ActivityCostModifiers)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
        bw.Write(ProvisionCostModifiers.Count);
        foreach (var entry in ProvisionCostModifiers)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
        bw.Write(ProvisionAmountModifiers.Count);
        foreach (var entry in ProvisionAmountModifiers)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
        bw.Write(UpgradeTagCostModifiers.Count);
        foreach (var entry in UpgradeTagCostModifiers)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
        bw.Write(FreeUpgradeTags.Count);
        foreach (var entry in FreeUpgradeTags)
        {
            bw.Write(entry.Key);
            bw.Write(entry.Value);
        }
    }

    public void Read(BinaryReader br)
    {
        NoLevelRestrictions = br.ReadBoolean();

        int dictionaryCount = br.ReadInt32();
        for (int i = 0; i < dictionaryCount; i++)
            ActivityLocks.Add(br.ReadString(), br.ReadBoolean());

        dictionaryCount = br.ReadInt32();
        for (int i = 0; i < dictionaryCount; i++)
            FreeActivities.Add(br.ReadString(), br.ReadBoolean());

        dictionaryCount = br.ReadInt32();
        for (int i = 0; i < dictionaryCount; i++)
            ActivityCostModifiers.Add(br.ReadString(), br.ReadSingle());

        dictionaryCount = br.ReadInt32();
        for (int i = 0; i < dictionaryCount; i++)
            ProvisionCostModifiers.Add(br.ReadString(), br.ReadSingle());

        dictionaryCount = br.ReadInt32();
        for (int i = 0; i < dictionaryCount; i++)
            ProvisionAmountModifiers.Add(br.ReadString(), br.ReadSingle());

        dictionaryCount = br.ReadInt32();
        for (int i = 0; i < dictionaryCount; i++)
            UpgradeTagCostModifiers.Add(br.ReadString(), br.ReadSingle());

        dictionaryCount = br.ReadInt32();
        for (int i = 0; i < dictionaryCount; i++)
            FreeUpgradeTags.Add(br.ReadString(), br.ReadInt32());
    }

    #region Event Helpers

    public bool IsActivityFree(string activityName)
    {
        if (FreeActivities.ContainsKey(activityName) && FreeActivities[activityName])
            return true;

        return false;
    }

    public bool IsActivityLocked(string activityName)
    {
        if (ActivityLocks.ContainsKey(activityName) && ActivityLocks[activityName])
            return true;

        return false;
    }

    public float ActivityCostModifier(string activityName)
    {
        if (ActivityCostModifiers.ContainsKey(activityName))
            return 1 + ActivityCostModifiers[activityName];
        else
            return 1;
    }

    public float ProvisionCostModifier(string itemType)
    {
        if (ProvisionCostModifiers.ContainsKey(itemType))
            return 1 + ProvisionCostModifiers[itemType];
        else
            return 1;
    }

    public float ProvisionAmountModifier(string itemType)
    {
        if (ProvisionAmountModifiers.ContainsKey(itemType))
            return 1 + ProvisionAmountModifiers[itemType];
        else
            return 1;
    }

    public float UpgradeTagDiscount(string tag)
    {
        if (UpgradeTagCostModifiers.ContainsKey(tag))
            return UpgradeTagCostModifiers[tag];
        else
            return 0;
    }

    public bool HasFreeUpgrade(string tag)
    {
        if (FreeUpgradeTags.ContainsKey(tag) && FreeUpgradeTags[tag] > 0)
            return true;

        return false;
    }

    public void RemoveUpgradeTag(string tag)
    {
        if (FreeUpgradeTags.ContainsKey(tag) && FreeUpgradeTags[tag] > 0)
            FreeUpgradeTags[tag]--;
    }

    #endregion
}