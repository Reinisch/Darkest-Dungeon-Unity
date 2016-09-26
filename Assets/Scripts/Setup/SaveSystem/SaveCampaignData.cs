using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveCampaignData
{
    public int saveId;

    #region Estate
    public bool isFirstStart;
    public string gameVersion;
    public string hamletTitle;
    public string locationName;
    public int questsCompleted;
    public int currentWeek;

    public int goldAmount;
    public int bustsAmount;
    public int deedsAmount;
    public int portraitsAmount;
    public int crestsAmount;

    public List<string> trinketData;
    public List<string> wagonData;
    public List<WeekActivityLog> activityLog;
    public List<Quest> generatedQuests;
    public List<string> completedPlot;

    public SaveHeroData[] saveHeroData;
    public SaveHeroData[] stageCoachData;
    public SaveHeroData[] stageEventData;
    public List<int> deathEventData;

    public Dictionary<string, DungeonProgress> saveDungeonData;
    public List<DeathRecord> deathRecords;

    public Dictionary<string, UpgradePurchases> buildingUpgrades;
    public Dictionary<int, Dictionary<string, UpgradePurchases>> instancedPurchases;

    public List<List<SaveActivitySlot>> tavernActivitySlots;
    public List<List<SaveActivitySlot>> abbeyActivitySlots;
    public List<List<SaveActivitySlot>> sanitariumActivitySlots;

    public string currentEvent;
    public string guaranteedEvent;

    public List<SaveEventData> eventData;
    public EventModifiers eventModifers;
    #endregion
    
    #region Raid
    public bool InRaid { get; set; }
    public bool QuestCompleted { get; set; }
    public Quest Quest { get; set; }
    public Dungeon Dungeon { get; set; }
    public RaidPartySaveData RaidParty { get; set; }

    public CampingPhase CampingPhase { get; set; }
    public int CampingTimeLeft { get; set; }
    public float NightAmbushReduced { get; set; }
    public int HungerCooldown { get; set; }

    public int ExploredRoomCount { get; set; }
    public string CurrentLocation { get; set; }
    public string LastRoom { get; set; }
    public string PreviousLastSector { get; set; }
    public string LastSector { get; set; }

    public List<string> KilledMonsters { get; set; }
    public List<string> InvestigatedCurios { get; set; }

    public int TorchAmount { get; set; }
    public int MaxTorchAmount { get; set; }
    public int ModifiedMinTorch { get; set; }
    public int ModifiedMaxTorch { get; set; }

    public BattleFormationSaveData HeroFormationData { get; set; }
    public List<InventorySlotData> InventoryItems { get; set; }
    #endregion

    #region Battle
    public bool inBattle;
    public BattleGroundSaveData battleGroundSaveData = new BattleGroundSaveData();
    #endregion

    public SaveCampaignData()
    {
        saveId = 1;
        trinketData = new List<string>();
        wagonData = new List<string>();
        KilledMonsters = new List<string>();
        InvestigatedCurios = new List<string>();
        deathRecords = new List<DeathRecord>();
        activityLog = new List<WeekActivityLog>();
        generatedQuests = new List<Quest>();
        completedPlot = new List<string>();
        InventoryItems = new List<InventorySlotData>();
        saveDungeonData = new Dictionary<string, DungeonProgress>();
        eventData = new List<SaveEventData>();
        eventModifers = new EventModifiers();
        deathEventData = new List<int>();
        stageEventData = new SaveHeroData[0];

        tavernActivitySlots = new List<List<SaveActivitySlot>>();
        abbeyActivitySlots = new List<List<SaveActivitySlot>>();
        sanitariumActivitySlots = new List<List<SaveActivitySlot>>();

        HeroFormationData = new BattleFormationSaveData();
    }
    public SaveCampaignData(int newSaveId)
    {
        saveId = newSaveId;
        trinketData = new List<string>();
        wagonData = new List<string>();
        KilledMonsters = new List<string>();
        InvestigatedCurios = new List<string>();
        deathRecords = new List<DeathRecord>();
        activityLog = new List<WeekActivityLog>();
        generatedQuests = new List<Quest>();
        completedPlot = new List<string>();
        InventoryItems = new List<InventorySlotData>();
        saveDungeonData = new Dictionary<string, DungeonProgress>();
        eventData = new List<SaveEventData>();
        eventModifers = new EventModifiers();
        deathEventData = new List<int>();
        stageEventData = new SaveHeroData[0];

        tavernActivitySlots = new List<List<SaveActivitySlot>>();
        abbeyActivitySlots = new List<List<SaveActivitySlot>>();
        sanitariumActivitySlots = new List<List<SaveActivitySlot>>();

        HeroFormationData = new BattleFormationSaveData();
    }
    public SaveCampaignData(int newSaveId, string newEstateName)
    {
        saveId = newSaveId;
        hamletTitle = newEstateName;
        trinketData = new List<string>();
        wagonData = new List<string>();
        KilledMonsters = new List<string>();
        InvestigatedCurios = new List<string>();
        deathRecords = new List<DeathRecord>();
        activityLog = new List<WeekActivityLog>();
        generatedQuests = new List<Quest>();
        completedPlot = new List<string>();
        InventoryItems = new List<InventorySlotData>();
        saveDungeonData = new Dictionary<string, DungeonProgress>();
        eventData = new List<SaveEventData>();
        eventModifers = new EventModifiers();
        deathEventData = new List<int>();
        stageEventData = new SaveHeroData[0];

        tavernActivitySlots = new List<List<SaveActivitySlot>>();
        abbeyActivitySlots = new List<List<SaveActivitySlot>>();
        sanitariumActivitySlots = new List<List<SaveActivitySlot>>();

        HeroFormationData = new BattleFormationSaveData();
    }

    public void UpdateFromEstate()
    {
        var campaign = DarkestDungeonManager.Campaign;

        isFirstStart = false;
        gameVersion = Application.version;
        locationName = "Hamlet";
        questsCompleted = campaign.QuestsComleted;
        currentWeek = campaign.CurrentWeek;

        goldAmount = campaign.Estate.Currencies["gold"].amount;
        bustsAmount = campaign.Estate.Currencies["bust"].amount;
        deedsAmount = campaign.Estate.Currencies["deed"].amount;
        portraitsAmount = campaign.Estate.Currencies["portrait"].amount;
        crestsAmount = campaign.Estate.Currencies["crest"].amount;

        #region Estate Misc
        #region Trinkets
        trinketData.Clear();
        for (int i = 0; i < campaign.RealmInventory.Trinkets.Count; i++)
            trinketData.Add(campaign.RealmInventory.Trinkets[i].Id);
        #endregion
        #region Wagon Trinkets
        wagonData.Clear();
        for (int i = 0; i < campaign.Estate.NomadWagon.Trinkets.Count; i++)
            wagonData.Add(campaign.Estate.NomadWagon.Trinkets[i].Id);
        #endregion

        activityLog = campaign.Logs;
        completedPlot = campaign.CompletedPlot;
        generatedQuests = campaign.Quests;

        #region Initial Heroes
        saveHeroData = new SaveHeroData[campaign.Heroes.Count];
        for (int i = 0; i < campaign.Heroes.Count; i++)
        {
            saveHeroData[i] = new SaveHeroData();
            campaign.Heroes[i].UpdateSaveData(saveHeroData[i]);
        };
        #endregion
        #region StageCoach Heroes
        stageCoachData = new SaveHeroData[campaign.Estate.StageCoach.Heroes.Count];
        for (int i = 0; i < campaign.Estate.StageCoach.Heroes.Count; i++)
        {
            stageCoachData[i] = new SaveHeroData();
            campaign.Estate.StageCoach.Heroes[i].UpdateSaveData(stageCoachData[i]);
        }
        stageEventData = new SaveHeroData[campaign.Estate.StageCoach.EventHeroes.Count];
        for (int i = 0; i < campaign.Estate.StageCoach.EventHeroes.Count; i++)
        {
            stageEventData[i] = new SaveHeroData();
            campaign.Estate.StageCoach.EventHeroes[i].UpdateSaveData(stageEventData[i]);
        }
        deathEventData = campaign.Estate.StageCoach.GraveIndexes;
        #endregion

        deathRecords = campaign.Estate.Graveyard.Records;
        saveDungeonData = campaign.Dungeons;

        buildingUpgrades = campaign.Estate.TownPurchases;
        instancedPurchases = campaign.Estate.HeroPurchases;

        abbeyActivitySlots.Clear();
        for (int i = 0; i < campaign.Estate.Abbey.Activities.Count; i++)
        {
            if (abbeyActivitySlots.Count < i + 1)
                abbeyActivitySlots.Add(new List<SaveActivitySlot>());

            for (int j = 0; j < campaign.Estate.Abbey.Activities[i].ActivitySlots.Count; j++)
            {
                if (abbeyActivitySlots[i].Count < j + 1)
                    abbeyActivitySlots[i].Add(new SaveActivitySlot());

                abbeyActivitySlots[i][j].UpdateFromActivity(campaign.Estate.Abbey.Activities[i].ActivitySlots[j]);
            }
        }

        tavernActivitySlots.Clear();
        for (int i = 0; i < campaign.Estate.Tavern.Activities.Count; i++)
        {
            if (tavernActivitySlots.Count < i + 1)
                tavernActivitySlots.Add(new List<SaveActivitySlot>());

            for (int j = 0; j < campaign.Estate.Tavern.Activities[i].ActivitySlots.Count; j++)
            {
                if (tavernActivitySlots[i].Count < j + 1)
                    tavernActivitySlots[i].Add(new SaveActivitySlot());

                tavernActivitySlots[i][j].UpdateFromActivity(campaign.Estate.Tavern.Activities[i].ActivitySlots[j]);
            }
        }

        sanitariumActivitySlots.Clear();
        if (sanitariumActivitySlots.Count < 1)
            sanitariumActivitySlots.Add(new List<SaveActivitySlot>());

        for (int i = 0; i < campaign.Estate.Sanitarium.QuirkActivity.TreatmentSlots.Count; i++)
        {
            if (sanitariumActivitySlots[0].Count < i + 1)
                sanitariumActivitySlots[0].Add(new SaveActivitySlot());

            sanitariumActivitySlots[0][i].UpdateFromTreatment(campaign.Estate.Sanitarium.QuirkActivity.TreatmentSlots[i]);
        }

        if (sanitariumActivitySlots.Count < 2)
            sanitariumActivitySlots.Add(new List<SaveActivitySlot>());
        for (int i = 0; i < campaign.Estate.Sanitarium.DiseaseActivity.TreatmentSlots.Count; i++)
        {
            if (sanitariumActivitySlots[1].Count < i + 1)
                sanitariumActivitySlots[1].Add(new SaveActivitySlot());

            sanitariumActivitySlots[1][i].UpdateFromTreatment(campaign.Estate.Sanitarium.DiseaseActivity.TreatmentSlots[i]);
        }

        currentEvent = campaign.TriggeredEvent == null ? "" : campaign.TriggeredEvent.Id;
        guaranteedEvent = campaign.GuaranteedEvent == null ? "" : campaign.GuaranteedEvent.Id;

        eventData.Clear();
        for (int i = 0; i < DarkestDungeonManager.Data.EventDatabase.Events.Count; i++)
            if (!DarkestDungeonManager.Data.EventDatabase.Events[i].IsDefault)
                eventData.Add(DarkestDungeonManager.Data.EventDatabase.Events[i].GetSaveData());
        eventModifers = campaign.EventModifiers;

        InRaid = false;
        #endregion
    }
    public void UpdateFromRaid()
    {
        UpdateFromEstate();

        InRaid = true;

        QuestCompleted = RaidSceneManager.Raid.QuestCompleted;
        Quest = RaidSceneManager.Raid.Quest;
        Dungeon = RaidSceneManager.Raid.Dungeon;
        if (RaidParty == null)
            RaidParty = new RaidPartySaveData();
        RaidParty.UpdateFromRaidParty(RaidSceneManager.Raid.RaidParty);
        CampingPhase = RaidSceneManager.Raid.CampingPhase;
        CampingTimeLeft = RaidSceneManager.Raid.CampingTimeLeft;
        NightAmbushReduced = RaidSceneManager.Raid.NightAmbushReduced;
        HungerCooldown = RaidSceneManager.Raid.HungerCooldown;

        CurrentLocation = RaidSceneManager.Raid.CurrentLocation == null ?
            RaidSceneManager.Raid.Dungeon.StartingRoom.Id :
            RaidSceneManager.Raid.CurrentLocation.Id;

        ExploredRoomCount = RaidSceneManager.Raid.ExploredRoomCount;
        LastRoom = RaidSceneManager.Raid.LastRoom == null ? "" : RaidSceneManager.Raid.LastRoom.Id;
        LastSector = RaidSceneManager.Raid.LastSector == null ? "" : RaidSceneManager.Raid.LastSector.Id;
        PreviousLastSector = RaidSceneManager.Raid.PreviousLastSector == null ? LastSector : RaidSceneManager.Raid.PreviousLastSector.Id;


        KilledMonsters = RaidSceneManager.Raid.KilledMonsters;
        InvestigatedCurios = RaidSceneManager.Raid.InvestigatedCurios;

        TorchAmount = RaidSceneManager.TorchMeter.TorchAmount;
        MaxTorchAmount = RaidSceneManager.TorchMeter.MaxAmount;

        ModifiedMinTorch = RaidSceneManager.TorchMeter.Modifier == null ? -1 : RaidSceneManager.TorchMeter.Modifier.Min;
        ModifiedMaxTorch = RaidSceneManager.TorchMeter.Modifier == null ? -1 : RaidSceneManager.TorchMeter.Modifier.Max;

        HeroFormationData.UpdateFormation(RaidSceneManager.Formations.heroes);
        InventoryItems = RaidSceneManager.Inventory.SaveInventorySlotData();

        if (RaidSceneManager.BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            inBattle = true;
            battleGroundSaveData.UpdateFromBattleGround(RaidSceneManager.BattleGround);
        }
        else
            inBattle = false;
    }
}