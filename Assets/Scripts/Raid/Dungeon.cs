using System.Collections.Generic;
using System.IO;
using UnityEngine.Assertions;

public class Dungeon : IBinarySaveData
{
    public string Name { get; set; }

    public int GridSizeX { get; set; }
    public int GridSizeY { get; set; }

    public string StartingRoomId { get; set; }

    public int TotalRoomBattles { get; set; }
    public int RoomGuardedCurio { get; set; }
    public int RoomGuardedTresure { get; set; }

    public int HallwayBattles { get; set; }
    public int HallwayTraps { get; set; }
    public int HallwayObstacles { get; set; }
    public int HallwayCurios { get; set; }
    public int HallwayHunger { get; set; }

    public DungeonRoom StartingRoom
    {
        get
        {
            return Rooms[StartingRoomId];
        }
    }

    public Dictionary<string, DungeonRoom> Rooms { get; set; }
    public Dictionary<string, Hallway> Hallways { get; set; }

    public DungeonBattleMash SharedMash { get; set; }
    public DungeonBattleMash DungeonMash { get; set; }
    public List<int> SharedMashExecutionIds { get; private set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

    public Dungeon()
    {
        Rooms = new Dictionary<string, DungeonRoom>();
        Hallways = new Dictionary<string, Hallway>();
        SharedMashExecutionIds = new List<int>();
    }

    public Dungeon(string name, int gridSizeX, int gridSizeY, string startingRoomId) : this()
    {
        Name = name;
        GridSizeX = gridSizeX;
        GridSizeY = gridSizeY;
        StartingRoomId = startingRoomId;
    }

    public void Initialize(Quest quest)
    {
        foreach (var hallway in Hallways)
        {
            hallway.Value.Initialize(this);
            foreach (var sector in hallway.Value.Halls)
                InitializeQuestCurios(sector, quest);
        }

        foreach (var entry in Rooms)
            InitializeQuestCurios(entry.Value, quest);

        SharedMash = DarkestDungeonManager.Data.DungeonEnviromentData["shared"].BattleMashes.Find(mash => mash.MashId == quest.Difficulty);
        DungeonMash = DarkestDungeonManager.Data.DungeonEnviromentData[quest.Dungeon].BattleMashes.Find(mash => mash.MashId == quest.Difficulty);
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(Name);
        bw.Write(GridSizeX);
        bw.Write(GridSizeY);
        bw.Write(StartingRoomId);

        Rooms.Write(bw);
        Hallways.Write(bw);
        SharedMashExecutionIds.Write(bw);
    }

    public void Read(BinaryReader br)
    {
        Name = br.ReadString();
        GridSizeX = br.ReadInt32();
        GridSizeY = br.ReadInt32();
        StartingRoomId = br.ReadString();

        Rooms.Read(room => room.Id, br);
        Hallways.Read(hallway => hallway.Id, br);
        SharedMashExecutionIds.Read(br);
    }

    private void InitializeQuestCurios(Area area, Quest quest)
    {
        var curio = area.Prop as Curio;

        if (curio == null || !curio.IsQuestCurio)
            return;

        if (quest.Goal.Type == "activate")
        {
            Assert.IsTrue(quest.Goal.QuestData is QuestActivateData);

            if (quest.Goal.StartingItems.Count > 0)
                curio.ItemInteractions.Add(new ItemInteraction(1, quest.Goal.StartingItems[0].Id, "loot"));
            else
                curio.Results.Add(new CurioInteraction(1, "loot"));
        }
        else if (quest.Goal.Type == "gather")
        {
            Assert.IsTrue(quest.Goal.QuestData is QuestGatherData);

            var gatherData = (QuestGatherData)quest.Goal.QuestData;
            var curioInteraction = new CurioInteraction(1, "loot");
            curioInteraction.Results.Add(new CurioResult(1, 1, gatherData.Item.Id));
            curio.Results.Add(curioInteraction);
        }
    }
}