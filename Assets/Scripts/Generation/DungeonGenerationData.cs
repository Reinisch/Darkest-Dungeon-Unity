public class DungeonGenerationData
{
    public string Length { get; set; }
    public string QuestType { get; set; }
    public string Dungeon { get; set; }

    public int BaseRoomNumber { get; set; }
    public int BaseCorridorNumber { get; set; }

    public int GridSizeX { get; set; }
    public int GridSizeY { get; set; }

    public int Spacing { get; set; }
    public int GoalRoomNumber { get; set; }
    public float Connectivity { get; set; }
    public int MinFinalDistance { get; set; }

    public int HallwayBattleMin { get; set; }
    public int HallwayBattleMax { get; set; }

    public int HallwayTrapMin { get; set; }
    public int HallwayTrapMax { get; set; }

    public int HallwayObstacleMin { get; set; }
    public int HallwayObstacleMax { get; set; }

    public int HallwayCurioMin { get; set; }
    public int HallwayCurioMax { get; set; }

    public int HallwayHungerMin { get; set; }
    public int HallwayHungerMax { get; set; }

    public int TotalRoomBattleMin { get; set; }
    public int TotalRoomBattleMax { get; set; }

    public int RoomBattleMin { get; set; }
    public int RoomBattleMax { get; set; }

    public int RoomGuardedCurioMin { get; set; }
    public int RoomGuardedCurioMax { get; set; }

    public int RoomCurioMin { get; set; }
    public int RoomCurioMax { get; set; }

    public int RoomGuardedTresureMin { get; set; }
    public int RoomGuardedTresureMax { get; set; }

    public int RoomTresureMin { get; set; }
    public int RoomTresureMax { get; set; }
}