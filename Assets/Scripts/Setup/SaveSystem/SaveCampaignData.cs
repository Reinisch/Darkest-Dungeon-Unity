using System.Collections.Generic;

public class SaveCampaignData
{
    public int SaveId;

    #region Estate
    public bool IsFirstStart;
    public string HamletTitle;
    public string LocationName;
    public int QuestsCompleted;
    public int CurrentWeek;

    public int GoldAmount;
    public int BustsAmount;
    public int DeedsAmount;
    public int PortraitsAmount;
    public int CrestsAmount;

    public List<string> InventoryTrinkets;
    public List<string> WagonTrinkets;
    public List<WeekActivityLog> ActivityLog;
    public List<Quest> GeneratedQuests;
    public List<string> CompletedPlot;

    public List<SaveHeroData> RosterHeroes;
    public List<SaveHeroData> StageCoachHeroes;
    public List<SaveHeroData> StageEventHeroes;
    public List<int> DeathEventData;

    public Dictionary<string, DungeonProgress> DungeonProgress;
    public List<DeathRecord> DeathRecords;

    public Dictionary<string, UpgradePurchases> BuildingUpgrades { get; private set; }
    public Dictionary<int, Dictionary<string, UpgradePurchases>> InstancedPurchases { get; private set; }

    public List<List<SaveActivitySlot>> TavernActivitySlots;
    public List<List<SaveActivitySlot>> AbbeyActivitySlots;
    public List<List<SaveActivitySlot>> SanitariumActivitySlots;

    public string CurrentEvent;
    public string GuaranteedEvent;

    public List<SaveEventData> EventData;
    public EventModifiers EventModifers;

    public Dictionary<string, int> TownNarrations;
    public Dictionary<string, int> RaidNarrations;
    public Dictionary<string, int> CampaignNarrations;
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
    public int AncestorTalk { get; set; }

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
    public bool InBattle;
    public BattleGroundSaveData BattleGroundSaveData = new BattleGroundSaveData();
    #endregion


    public SaveCampaignData()
    {
        InventoryTrinkets = new List<string>();
        WagonTrinkets = new List<string>();
        KilledMonsters = new List<string>();
        InvestigatedCurios = new List<string>();
        DeathRecords = new List<DeathRecord>();
        ActivityLog = new List<WeekActivityLog>();
        GeneratedQuests = new List<Quest>();
        CompletedPlot = new List<string>();
        InventoryItems = new List<InventorySlotData>();
        DungeonProgress = new Dictionary<string, DungeonProgress>();
        EventData = new List<SaveEventData>();
        EventModifers = new EventModifiers();
        RosterHeroes = new List<SaveHeroData>();
        StageCoachHeroes = new List<SaveHeroData>();
        StageEventHeroes = new List<SaveHeroData>();
        DeathEventData = new List<int>();

        TavernActivitySlots = new List<List<SaveActivitySlot>>();
        AbbeyActivitySlots = new List<List<SaveActivitySlot>>();
        SanitariumActivitySlots = new List<List<SaveActivitySlot>>();

        BuildingUpgrades = new Dictionary<string, UpgradePurchases>();
        InstancedPurchases = new Dictionary<int, Dictionary<string, UpgradePurchases>>();

        TownNarrations = new Dictionary<string, int>();
        RaidNarrations = new Dictionary<string, int>();
        CampaignNarrations = new Dictionary<string, int>();

        HeroFormationData = new BattleFormationSaveData();
    }

    public SaveCampaignData(int newSaveId) : this()
    {
        SaveId = newSaveId;
    }

    public SaveCampaignData(int newSaveId, string newEstateName) : this()
    {
        SaveId = newSaveId;
        HamletTitle = newEstateName;
    }


    public void UpdateFromEstate()
    {
        var campaign = DarkestDungeonManager.Campaign;

        IsFirstStart = false;
        LocationName = "Hamlet";
        QuestsCompleted = campaign.QuestsComleted;
        CurrentWeek = campaign.CurrentWeek;

        GoldAmount = campaign.Estate.Currencies["gold"].amount;
        BustsAmount = campaign.Estate.Currencies["bust"].amount;
        DeedsAmount = campaign.Estate.Currencies["deed"].amount;
        PortraitsAmount = campaign.Estate.Currencies["portrait"].amount;
        CrestsAmount = campaign.Estate.Currencies["crest"].amount;

        #region Estate Misc
        #region Trinkets
        InventoryTrinkets.Clear();
        for (int i = 0; i < campaign.RealmInventory.Trinkets.Count; i++)
            InventoryTrinkets.Add(campaign.RealmInventory.Trinkets[i].Id);
        #endregion
        #region Wagon Trinkets
        WagonTrinkets.Clear();
        for (int i = 0; i < campaign.Estate.NomadWagon.Trinkets.Count; i++)
            WagonTrinkets.Add(campaign.Estate.NomadWagon.Trinkets[i].Id);
        #endregion

        ActivityLog = campaign.Logs;
        CompletedPlot = campaign.CompletedPlot;
        GeneratedQuests = campaign.Quests;

        #region Initial Heroes
        RosterHeroes = new List<SaveHeroData>();
        for (int i = 0; i < campaign.Heroes.Count; i++)
        {
            var newHeroData = new SaveHeroData();
            campaign.Heroes[i].UpdateSaveData(newHeroData);
            RosterHeroes.Add(newHeroData);
        };
        #endregion
        #region StageCoach Heroes
        StageCoachHeroes = new List<SaveHeroData>();
        for (int i = 0; i < campaign.Estate.StageCoach.Heroes.Count; i++)
        {
            var newHeroData = new SaveHeroData();
            campaign.Estate.StageCoach.Heroes[i].UpdateSaveData(newHeroData);
            StageCoachHeroes.Add(newHeroData);
        }
        StageEventHeroes = new List<SaveHeroData>();
        for (int i = 0; i < campaign.Estate.StageCoach.EventHeroes.Count; i++)
        {
            var newHeroData = new SaveHeroData();
            campaign.Estate.StageCoach.EventHeroes[i].UpdateSaveData(newHeroData);
            StageEventHeroes.Add(newHeroData);
        }
        DeathEventData = campaign.Estate.StageCoach.GraveIndexes;
        #endregion

        DeathRecords = campaign.Estate.Graveyard.Records;
        DungeonProgress = campaign.Dungeons;

        BuildingUpgrades = campaign.Estate.TownPurchases;
        InstancedPurchases = campaign.Estate.HeroPurchases;

        AbbeyActivitySlots.Clear();
        for (int i = 0; i < campaign.Estate.Abbey.Activities.Count; i++)
        {
            if (AbbeyActivitySlots.Count < i + 1)
                AbbeyActivitySlots.Add(new List<SaveActivitySlot>());

            for (int j = 0; j < campaign.Estate.Abbey.Activities[i].ActivitySlots.Count; j++)
            {
                if (AbbeyActivitySlots[i].Count < j + 1)
                    AbbeyActivitySlots[i].Add(new SaveActivitySlot());

                AbbeyActivitySlots[i][j].UpdateFromActivity(campaign.Estate.Abbey.Activities[i].ActivitySlots[j]);
            }
        }

        TavernActivitySlots.Clear();
        for (int i = 0; i < campaign.Estate.Tavern.Activities.Count; i++)
        {
            if (TavernActivitySlots.Count < i + 1)
                TavernActivitySlots.Add(new List<SaveActivitySlot>());

            for (int j = 0; j < campaign.Estate.Tavern.Activities[i].ActivitySlots.Count; j++)
            {
                if (TavernActivitySlots[i].Count < j + 1)
                    TavernActivitySlots[i].Add(new SaveActivitySlot());

                TavernActivitySlots[i][j].UpdateFromActivity(campaign.Estate.Tavern.Activities[i].ActivitySlots[j]);
            }
        }

        SanitariumActivitySlots.Clear();
        if (SanitariumActivitySlots.Count < 1)
            SanitariumActivitySlots.Add(new List<SaveActivitySlot>());

        for (int i = 0; i < campaign.Estate.Sanitarium.QuirkActivity.TreatmentSlots.Count; i++)
        {
            if (SanitariumActivitySlots[0].Count < i + 1)
                SanitariumActivitySlots[0].Add(new SaveActivitySlot());

            SanitariumActivitySlots[0][i].UpdateFromTreatment(campaign.Estate.Sanitarium.QuirkActivity.TreatmentSlots[i]);
        }

        if (SanitariumActivitySlots.Count < 2)
            SanitariumActivitySlots.Add(new List<SaveActivitySlot>());
        for (int i = 0; i < campaign.Estate.Sanitarium.DiseaseActivity.TreatmentSlots.Count; i++)
        {
            if (SanitariumActivitySlots[1].Count < i + 1)
                SanitariumActivitySlots[1].Add(new SaveActivitySlot());

            SanitariumActivitySlots[1][i].UpdateFromTreatment(campaign.Estate.Sanitarium.DiseaseActivity.TreatmentSlots[i]);
        }

        CurrentEvent = campaign.TriggeredEvent == null ? "" : campaign.TriggeredEvent.Id;
        GuaranteedEvent = campaign.GuaranteedEvent == null ? "" : campaign.GuaranteedEvent.Id;

        EventData.Clear();
        for (int i = 0; i < DarkestDungeonManager.Data.EventDatabase.Events.Count; i++)
            if (!DarkestDungeonManager.Data.EventDatabase.Events[i].IsDefault)
                EventData.Add(DarkestDungeonManager.Data.EventDatabase.Events[i].GetSaveData());
        EventModifers = campaign.EventModifiers;

        TownNarrations = campaign.NarrationCampaignInfo;
        RaidNarrations = campaign.NarrationRaidInfo;
        CampaignNarrations = campaign.NarrationCampaignInfo;

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
        AncestorTalk = RaidSceneManager.Raid.AncestorTalk;

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
            InBattle = true;
            BattleGroundSaveData.UpdateFromBattleGround(RaidSceneManager.BattleGround);
        }
        else
            InBattle = false;
    }
}