using System.Collections.Generic;

public class Campaign
{
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
    public Dictionary<string, DungeonProgress> Dungeons { get; set; }
    public Dictionary<string, int> NarrationRaidInfo { get; set; }
    public Dictionary<string, int> NarrationTownInfo { get; set; }
    public Dictionary<string, int> NarrationCampaignInfo { get; set; }

    private TownEventOption EventsOption { get; set; }

    public Campaign()
    {
        NarrationRaidInfo = new Dictionary<string, int>();
        NarrationTownInfo = new Dictionary<string, int>();
        NarrationCampaignInfo = new Dictionary<string, int>();
    }

    public WeekActivityLog CurrentLog()
    {
        return Logs.Count > 0 ? Logs[Logs.Count - 1] : null;
    }

    public void Load(SaveCampaignData saveData)
    {
        EventModifiers = saveData.EventModifers;

        foreach (var townEvent in DarkestDungeonManager.Data.EventDatabase.Events)
            townEvent.SetDefaultState();

        CurrentWeek = saveData.CurrentWeek;
        QuestsComleted = saveData.QuestsCompleted;

        Estate = new Estate(saveData);
        RealmInventory = new RealmInventory(saveData);
        CompletedPlot = saveData.CompletedPlot;

        Heroes = new List<Hero>();
        saveData.RosterHeroes.ForEach(hero => Heroes.Add(new Hero(Estate, hero)));
        saveData.StageCoachHeroes.ForEach(hero => Estate.StageCoach.Heroes.Add(new Hero(Estate, hero)));
        saveData.StageEventHeroes.ForEach(hero => Estate.StageCoach.EventHeroes.Add(new Hero(Estate, hero)));
        saveData.WagonTrinkets.ForEach(trinketName => Estate.NomadWagon.Trinkets.Add(DarkestDungeonManager.Data.Items["trinket"][trinketName] as Trinket));

        Estate.StageCoach.GraveIndexes = saveData.DeathEventData;
        Estate.Abbey.UpdateActivitySlots(saveData);
        Estate.Tavern.UpdateActivitySlots(saveData);
        Estate.Sanitarium.QuirkActivity.UpdateActivitySlots(saveData);
        Estate.Sanitarium.DiseaseActivity.UpdateActivitySlots(saveData);

        Dungeons = saveData.DungeonProgress;
        Quests = saveData.GeneratedQuests;
        if (Quests.Count == 0 && saveData.InRaid == false)
            GenerateQuests();

        Logs = new List<WeekActivityLog>(saveData.ActivityLog);
        if (Logs.Count == 0)
            Logs.Add(new WeekActivityLog(CurrentWeek));

        EventsOption = DarkestDungeonManager.Data.EventDatabase.Settings[2];

        if(saveData.CurrentEvent != "")
        {
            TriggeredEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(townEvent => townEvent.Id == saveData.CurrentEvent);
            if (TriggeredEvent != null)
                EventModifiers.EventData.AddRange(TriggeredEvent.Data);
        }

        if(saveData.GuaranteedEvent != "")
        {
            GuaranteedEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(townEvent => townEvent.Id == saveData.GuaranteedEvent);
            if (GuaranteedEvent != null)
                EventModifiers.EventData.AddRange(GuaranteedEvent.Data);
        }

        foreach(var saveEventEntry in saveData.EventData)
        {
            var targetEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(townEvent => townEvent.Id == saveEventEntry.EventId);
            if(targetEvent != null)
                targetEvent.UpdateFromSave(saveEventEntry);
        }

        NarrationCampaignInfo = saveData.CampaignNarrations;
        NarrationRaidInfo = saveData.RaidNarrations;
        NarrationTownInfo = saveData.TownNarrations;
    }

    public void ExecuteProgress()
    {
        foreach (var eventEntry in EventModifiers.EventData)
        {
            switch (eventEntry.Type)
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

            if (TriggeredEvent.Id == "plot_quest_town_invasion_0")
                DarkestSoundManager.PlayOneShot("event:/town/town_event_display_bandit_incursion");
            else if (TriggeredEvent.Tone == TownEventTone.Bad)
                DarkestSoundManager.PlayOneShot("event:/town/town_event_display_bad");
            else if (TriggeredEvent.Tone == TownEventTone.Good)
                DarkestSoundManager.PlayOneShot("event:/town/town_event_display_good");
            else
                DarkestSoundManager.PlayOneShot("event:/town/town_event_display_neutral");
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

    public void CheckEmbarkBuffs(RaidParty raidParty)
    {
        if (TriggeredEvent == null && GuaranteedEvent == null)
            return;

        foreach (var eventEntry in EventModifiers.EventData)
            if (eventEntry.Type == TownEventDataType.EmbarkPartyBuff)
                for (int j = 0; j < raidParty.HeroInfo.Count; j++)
                {
                    var embarkBuff = new BuffInfo(DarkestDungeonManager.Data.Buffs[eventEntry.StringData], BuffSourceType.Estate);
                    raidParty.HeroInfo[j].Hero.AddBuff(embarkBuff);
                }
    }

    public void DismissHero(Hero dismissedHero)
    {
        Heroes.Remove(dismissedHero);
        Estate.ReturnRosterId(dismissedHero.RosterId);
        DarkestSoundManager.PlayOneShot("event:/ui/town/let_go");
    }

    private void CheckGuarantees(Quest completedQuest)
    {
        if (EventsOption.Frequency[0] > 0)
        {
            var eventGuarantee = DarkestDungeonManager.Data.EventDatabase.Guarantees.Find(guarantee =>
                guarantee.Dungeon == completedQuest.Dungeon && guarantee.QuestType == completedQuest.Type);

            if (eventGuarantee != null)
            {
                GuaranteedEvent = DarkestDungeonManager.Data.EventDatabase.Events.Find(guarantEvent =>
                    guarantEvent.Id == eventGuarantee.EventId);

                EventModifiers.IncludeEvent(GuaranteedEvent);
            }
        }
    }

    private void SearchMissingHeroes()
    {
        for (int i = 0; i < Heroes.Count; i++)
            if (Heroes[i].Status == HeroStatus.Missing)
            {
                Heroes[i].MissingDuration--;
                if (Heroes[i].MissingDuration == 0)
                    Heroes[i].Status = HeroStatus.Available;
            }
    }

    private void GenerateQuests()
    {
        Quests = QuestGenerator.GenerateQuests(DarkestDungeonManager.Campaign);
        AreQuestsReady = false;
    }
}