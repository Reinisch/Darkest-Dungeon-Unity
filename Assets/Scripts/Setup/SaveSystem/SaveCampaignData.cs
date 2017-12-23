using System.Collections.Generic;

public class SaveCampaignData
{
    public int SaveId;
    public bool InBattle;
    public readonly BattleGroundSaveData BattleGroundSaveData = new BattleGroundSaveData();

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

    public void SetEstateInfo(bool firstStart, string locationName, int questsCompleted, int currentWeek, int gold, int busts, int deeds, int portraits, int crests)
    {
        IsFirstStart = firstStart;
        LocationName = locationName;
        QuestsCompleted = questsCompleted;
        CurrentWeek = currentWeek;

        GoldAmount = gold;
        BustsAmount = busts;
        DeedsAmount = deeds;
        PortraitsAmount = portraits;
        CrestsAmount = crests;
    }

    public void SetTavernSlots(int activityCount, int activitySlots)
    {
        SetEmptySlots(TavernActivitySlots, activityCount, activitySlots);
    }

    public void SetAbbeySlots(int activityCount, int activitySlots)
    {
        SetEmptySlots(AbbeyActivitySlots, activityCount, activitySlots);
    }

    public void SetSanitariumSlots(int activityCount, int activitySlots)
    {
        SetEmptySlots(SanitariumActivitySlots, activityCount, activitySlots);
    }

    public void PopulateStartingEstateData()
    {
        SetEstateInfo(true, "Raid", 0, 1, 1000, 10, 10, 10, 10);

        RosterHeroes = new List<SaveHeroData>
        {
            new SaveHeroData(1, "Reynald", "crusader", 0, 0, 10, 1, 1, "", "", "warrior_of_light", "kleptomaniac", "god_fearing"),
            new SaveHeroData(2, "Dismas", "highwayman", 0, 0, 10, 1, 1, "", "", "hard_noggin", "known_cheat", "quick_reflexes"),
        };

        StageCoachHeroes = new List<SaveHeroData>();

        InstancedPurchases.Clear();
        foreach (SaveHeroData heroData in RosterHeroes)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            InstancedPurchases.Add(heroData.RosterId, newHeroPurchases);
            newHeroPurchases.Add(heroData.HeroClass + ".weapon", new UpgradePurchases(heroData.HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(heroData.HeroClass + ".armour", new UpgradePurchases(heroData.HeroClass + ".armour", new string[0]));

            foreach (CombatSkill skill in DarkestDungeonManager.Data.HeroClasses[heroData.HeroClass].CombatSkills)
                newHeroPurchases.Add(heroData.HeroClass + "." + skill.Id, new UpgradePurchases(heroData.HeroClass + "." + skill.Id, new string[0]));
            foreach (CampingSkill skill in DarkestDungeonManager.Data.HeroClasses[heroData.HeroClass].CampingSkills)
                newHeroPurchases.Add(skill.Id, new UpgradePurchases(skill.Id, new string[0]));
        }

        InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        RosterHeroes[0].SelectedCombatSkillIndexes.AddRange(new[] { 0, 1, 2, 3 });
        InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        RosterHeroes[0].SelectedCampingSkillIndexes.AddRange(new[] { 0, 4, 5 });

        InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        RosterHeroes[1].SelectedCombatSkillIndexes.AddRange(new[] { 6, 1, 3, 4 });
        InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        RosterHeroes[1].SelectedCampingSkillIndexes.AddRange(new[] { 1, 5, 6 });

        ActivityLog = new List<WeekActivityLog>();
        CompletedPlot = new List<string>();
        GeneratedQuests = new List<Quest>();
        DeathRecords = new List<DeathRecord>();
        InventoryTrinkets.Clear();
        WagonTrinkets.Clear();

        DungeonProgress.Clear();
        DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        BuildingUpgrades.Clear();
        BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));

        SetAbbeySlots(3, 3);
        SetTavernSlots(3, 3);
        SetSanitariumSlots(2, 3);

        InRaid = true;
        QuestCompleted = false;
    }

    public void PopulateStartingRaidInfo(string startingRoom)
    {
        RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };

        ExploredRoomCount = 1;
        CurrentLocation = startingRoom;
        LastRoom = startingRoom;
        PreviousLastSector = "";
        LastSector = "";
        KilledMonsters = new List<string>();
        InvestigatedCurios = new List<string>();

        TorchAmount = 100;
        MaxTorchAmount = 100;
        ModifiedMinTorch = -1;
        ModifiedMaxTorch = -1;

        HeroFormationData = new BattleFormationSaveData();
        HeroFormationData.UnitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        HeroFormationData.UnitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });

        InventoryItems = new List<InventorySlotData>();
    }

    public void PopulateStartingDungeonInfo(bool isNewGamePlus)
    {
        Quest = new PlotQuest()
        {
            IsPlotQuest = true,
            Id = "tutorial",
            Difficulty = 1,
            Type = "tutorial_room",
            Dungeon = "weald",
            DungeonLevel = 1,
            Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals["tutorial_final_room"],
            Length = 1,
            PlotTrinket = new PlotTrinketReward() { Amount = 0, Rarity = "very_common" },
            Reward = new CompletionReward()
            {
                ResolveXP = 2,
                ItemDefinitions = new List<ItemDefinition>()
                 {
                     new ItemDefinition("gold", "", 5000),
                 }
            },

            CanRetreat = false,
            CompletionDungeonXp = false,
        };

        Dungeon = new Dungeon("weald", 9, 1, "room1_1");
        Dungeon.Rooms["room1_1"] = new DungeonRoom("room1_1", 1, 1, Knowledge.Completed, AreaType.Entrance, 1, "effigy_0");
        Dungeon.Rooms["room2_1"] = new DungeonRoom("room2_1", 8, 1, Knowledge.Hidden, AreaType.BattleTresure, 1, "effigy_1");
        Dungeon.Rooms["room2_1"].SetNamedEncounter("weald", "tutorial_2", 0, 1);
        Dungeon.Rooms["room2_1"].SetCurio("bandits_trapped_chest");

        Hallway hallway = Dungeon.Hallways["hallroom2_1_room1_1"] = new Hallway("hallroom2_1_room1_1", Dungeon.Rooms["room1_1"], Dungeon.Rooms["room2_1"], Direction.Right, Direction.Left);
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", 7, 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", 6, 1, hallway, Knowledge.Hidden, AreaType.Curio, "7", isNewGamePlus ? "open_grave" : "travellers_tent_tutorial"),
            new HallSector("2", 5, 1, hallway, Knowledge.Hidden, AreaType.Empty, "8"),
            new HallSector("3", 4, 1, hallway, Knowledge.Hidden, AreaType.Battle, "2", "weald", "tutorial_1", 1, 0),
            new HallSector("4", 3, 1, hallway, Knowledge.Hidden, AreaType.Empty, "1"),
            new HallSector("5", 2, 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
    }

    public void UpdateFromEstate()
    {
        var campaign = DarkestDungeonManager.Campaign;

        IsFirstStart = false;
        LocationName = "Hamlet";
        QuestsCompleted = campaign.QuestsComleted;
        CurrentWeek = campaign.CurrentWeek;

        GoldAmount = campaign.Estate.Currencies["gold"];
        BustsAmount = campaign.Estate.Currencies["bust"];
        DeedsAmount = campaign.Estate.Currencies["deed"];
        PortraitsAmount = campaign.Estate.Currencies["portrait"];
        CrestsAmount = campaign.Estate.Currencies["crest"];

        InventoryTrinkets.Clear();
        for (int i = 0; i < campaign.RealmInventory.Trinkets.Count; i++)
            InventoryTrinkets.Add(campaign.RealmInventory.Trinkets[i].Id);
        WagonTrinkets.Clear();
        for (int i = 0; i < campaign.Estate.NomadWagon.Trinkets.Count; i++)
            WagonTrinkets.Add(campaign.Estate.NomadWagon.Trinkets[i].Id);

        ActivityLog = campaign.Logs;
        CompletedPlot = campaign.CompletedPlot;
        GeneratedQuests = campaign.Quests;

        RosterHeroes = new List<SaveHeroData>();
        for (int i = 0; i < campaign.Heroes.Count; i++)
        {
            var newHeroData = new SaveHeroData();
            campaign.Heroes[i].UpdateSaveData(newHeroData);
            RosterHeroes.Add(newHeroData);
        }
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

        HeroFormationData.UpdateFormation(RaidSceneManager.Formations.Heroes);
        InventoryItems = RaidSceneManager.Inventory.SaveInventorySlotData();

        if (RaidSceneManager.BattleGround.BattleStatus == BattleStatus.Fighting)
        {
            InBattle = true;
            BattleGroundSaveData.UpdateFromBattleGround(RaidSceneManager.BattleGround);
        }
        else
            InBattle = false;
    }

    private static void SetEmptySlots(List<List<SaveActivitySlot>> activities, int activityCount, int activitySlots)
    {
        activities.Clear();
        for (int i = 0; i < activityCount; i++)
        {
            activities.Add(new List<SaveActivitySlot>());
            for (int j = 0; j < activitySlots; j++)
                activities[i].Add(new SaveActivitySlot());
        }
    }
}