using System.Collections.Generic;
using UnityEngine;

public class Campaign
{
    public int SaveSlotId { get; private set; }
    public int CurrentWeek { get; set; }
    public int QuestsComleted { get; set; }

    public Estate Estate { get; set; }
    public RealmInventory RealmInventory { get; set; }
    public List<WeekActivityLog> Logs { get; set; }
    public List<Hero> Heroes { get; set; }
    public List<Quest> Quests { get; set; }
    public List<string> CompletedPlot { get; set; }

    public bool AreQuestsReady { get; set; }
    public TownEvent TriggeredEvent { get; set; }
    public TownEvent GuaranteedEvent { get; set; }
    public EventModifiers EventModifiers { get; set; }
    public TownEventOption EventsOption { get; set; }

    public Dictionary<string, DungeonProgress> Dungeons { get; set; }

    public void SearchMissingHeroes()
    {
        for (int i = 0; i < Heroes.Count; i++)
            if (Heroes[i].Status == HeroStatus.Missing)
            {
                Heroes[i].MissingDuration--;
                if (Heroes[i].MissingDuration == 0)
                    Heroes[i].Status = HeroStatus.Available;
            }
    }
    public void ExecuteProgress()
    {
        foreach(var eventEntry in EventModifiers.EventData)
        {
            switch(eventEntry.Type)
            {
                case TownEventDataType.IdleBuff:
                    for (int i = 0; i < Heroes.Count; i++)
                        if (Heroes[i].Status == HeroStatus.Available)
                            Heroes[i].AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs[eventEntry.StringData],
                               BuffDurationType.IdleTownVisit, BuffSourceType.Estate));
                    break;
                case TownEventDataType.IdleResolve:
                    for (int i = 0; i < Heroes.Count; i++)
                        if (Heroes[i].Class == eventEntry.StringData)
                        {
                            for (int j = 0; j < eventEntry.NumberData; j++)
                                Heroes[i].Resolve.AddExperience(Heroes[i].Resolve.NextLevelXP - Heroes[i].Resolve.CurrentXP);

                            Heroes[i].UpdateResolve();
                        }
                    break;
                case TownEventDataType.InActivityBuff:
                    for (int i = 0; i < Heroes.Count; i++)
                        if (Heroes[i].Status == HeroStatus.Abbey || Heroes[i].Status == HeroStatus.Tavern)
                            for (int j = 0; j < eventEntry.NumberData; j++)
                                Heroes[i].AddBuff(new BuffInfo(DarkestDungeonManager.Data.Buffs[eventEntry.StringData],
                                    BuffDurationType.IdleTownVisit, BuffSourceType.Estate));
                    break;
                default:
                    break;
            }
        }

        TriggeredEvent = null;
        GuaranteedEvent = null;
        EventModifiers.Reset();
        Estate.ExecuteProgress();

        var possibleEvents = DarkestDungeonManager.Data.EventDatabase.Events.FindAll(townEvent => townEvent.IsPossible);
        if (possibleEvents.Count > 0 && EventsOption.Frequency.Count > 3 && RandomSolver.CheckSuccess(EventsOption.Frequency[3]))
        {
            TriggeredEvent = RandomSolver.ChooseBySingleRandom(possibleEvents);
            for (int i = 0; i < DarkestDungeonManager.Data.EventDatabase.Events.Count; i++)
                DarkestDungeonManager.Data.EventDatabase.Events[i].EventTriggered(false);

            TriggeredEvent.EventTriggered(true);
            EventModifiers.IncludeEvent(TriggeredEvent);
        }
        
        GenerateQuests();
        SearchMissingHeroes();
    }
    public void AdvanceNextWeek()
    {
        CurrentWeek++;
        Logs.Add(new WeekActivityLog(CurrentWeek));

        if (DarkestDungeonManager.RaidManager.Status == RaidStatus.Success)
            CheckGuarantees(DarkestDungeonManager.RaidManager.Quest);

        if (TriggeredEvent != null || GuaranteedEvent != null)
            Estate.RedeployCrier();
        else
            Estate.KickCrier();

        for (int i = 0; i < Heroes.Count; i++)
            Heroes[i].RemoveAllBuffsWithSource(BuffSourceType.Estate);
    }
    public void CheckGuarantees(Quest completedQuest)
    {
        if(EventsOption.Frequency[0] > 0)
        {
            var eventGuarantee = DarkestDungeonManager.Data.EventDatabase.Guarantees.Find(guarantee =>
                guarantee.Dungeon == completedQuest.Dungeon && guarantee.QuestType == completedQuest.Type);

            if(eventGuarantee != null)
            {
                GuaranteedEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(guarantEvent =>
                    guarantEvent.Id == eventGuarantee.EventId);

                EventModifiers.IncludeEvent(GuaranteedEvent);
            }
        }
    }
    public void CheckEmbarkBuffs(RaidParty raidParty)
    {
        if (TriggeredEvent == null)
            return;

        foreach(var eventEntry in EventModifiers.EventData)
            if (eventEntry.Type == TownEventDataType.EmbarkPartyBuff)
                for (int j = 0; j < raidParty.HeroInfo.Count; j++)
                {
                    var embarkBuff = new BuffInfo(DarkestDungeonManager.Data.Buffs[eventEntry.StringData],
                        BuffDurationType.IdleTownVisit, BuffSourceType.Estate);
                    raidParty.HeroInfo[j].Hero.AddBuff(embarkBuff);
                }
    }

    public Campaign()
    {
    }

    public void Load(SaveCampaignData saveData)
    {
        EventModifiers = saveData.eventModifers;

        foreach (var townEvent in DarkestDungeonManager.Data.EventDatabase.Events)
            townEvent.SetDefaultState();

        SaveSlotId = saveData.saveId;
        CurrentWeek = saveData.currentWeek;

        QuestsComleted = saveData.questsCompleted;

        Estate = new Estate(saveData);
        RealmInventory = new RealmInventory(saveData);
        CompletedPlot = saveData.completedPlot;

        Heroes = new List<Hero>();
        for (int i = 0; i < saveData.saveHeroData.Length; i++)
            Heroes.Add(new Hero(Estate, saveData.saveHeroData[i]));
        for (int i = 0; i < saveData.stageCoachData.Length; i++)
            Estate.StageCoach.Heroes.Add(new Hero(Estate, saveData.stageCoachData[i]));
        for (int i = 0; i < saveData.stageEventData.Length; i++)
            Estate.StageCoach.EventHeroes.Add(new Hero(Estate, saveData.stageEventData[i]));
        for (int i = 0; i < saveData.wagonData.Count; i++)
            Estate.NomadWagon.Trinkets.Add(DarkestDungeonManager.Data.Items["trinket"][saveData.wagonData[i]] as Trinket);
        Estate.StageCoach.GraveIndexes = saveData.deathEventData;

        Estate.Abbey.UpdateActivitySlots(saveData);
        Estate.Tavern.UpdateActivitySlots(saveData);
        Estate.Sanitarium.QuirkActivity.UpdateActivitySlots(saveData);
        Estate.Sanitarium.DiseaseActivity.UpdateActivitySlots(saveData);

        Dungeons = saveData.saveDungeonData;
        Quests = saveData.generatedQuests;
        if (Quests.Count == 0 && saveData.InRaid == false)
            GenerateQuests();

        Logs = new List<WeekActivityLog>(saveData.activityLog);
        if (Logs.Count == 0)
            Logs.Add(new WeekActivityLog(CurrentWeek));

        EventsOption = DarkestDungeonManager.Data.EventDatabase.Settings[2];

        if(saveData.currentEvent != "")
        {
            TriggeredEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(townEvent => townEvent.Id == saveData.currentEvent);
            if (TriggeredEvent != null)
                EventModifiers.EventData.AddRange(TriggeredEvent.Data);
        }

        if(saveData.guaranteedEvent != "")
        {
            GuaranteedEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(townEvent => townEvent.Id == saveData.guaranteedEvent);
            if (GuaranteedEvent != null)
                EventModifiers.EventData.AddRange(GuaranteedEvent.Data);
        }

        foreach(var saveEventEntry in saveData.eventData)
        {
            var targetEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(townEvent => townEvent.Id == saveEventEntry.EventId);
            if(targetEvent != null)
                targetEvent.UpdateFromSave(saveEventEntry);
        }
    }

    public WeekActivityLog CurrentLog()
    {
        return Logs.Count > 0 ? Logs[Logs.Count - 1] : null;
    }
    public WeekActivityLog PreviousLog()
    {
        return Logs.Count > 1 ? Logs[Logs.Count - 2] : null;
    }
    public void GenerateQuests()
    {
        Quests = QuestGenerator.GenerateQuests(DarkestDungeonManager.Campaign);
        AreQuestsReady = false;
    }
}

public class EventModifiers
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

    public bool IsActivityFree(string activityName)
    {
        if (FreeActivities.ContainsKey(activityName) && FreeActivities[activityName] == true)
            return true;

        return false;
    }
    public bool IsActivityLocked(string activityName)
    {
        if (ActivityLocks.ContainsKey(activityName) && ActivityLocks[activityName] == true)
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
        for(int i = 0; i < townEvent.Data.Count; i++)
        {
            EventData.AddRange(townEvent.Data);

            switch(townEvent.Data[i].Type)
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
                default:
                    break;
            }
        }
    }
}