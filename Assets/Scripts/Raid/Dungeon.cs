using System.Collections.Generic;

public class Dungeon
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

    public bool IsRandomlyGenerated { get; set; }

    public Room StartingRoom
    {
        get
        {
            return Rooms[StartingRoomId];
        }
    }

    public Dictionary<string, Room> Rooms { get; set; }
    public Dictionary<string, Hallway> Hallways { get; set; }

    public DungeonBattleMash SharedMash { get; set; }
    public DungeonBattleMash DungeonMash { get; set; }

    public List<int> SharedMashExecutionIds { get; set; }

    public Dungeon()
    {
        Rooms = new Dictionary<string, Room>();
        Hallways = new Dictionary<string, Hallway>();
        SharedMashExecutionIds = new List<int>();
    }
}