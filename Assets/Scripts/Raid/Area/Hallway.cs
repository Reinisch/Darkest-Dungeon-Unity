using System.Collections.Generic;

public class Hallway
{
    public string Id { get; set; }

    public List<HallSector> Halls { get; set; }

    public int HallCount
    {
        get
        {
            return Halls.Count;
        }
    }

    public DungeonRoom RoomA { get; set; }
    public DungeonRoom RoomB { get; set; }

    public Direction DirectionFromA
    {
        get
        {
            var targetDoor = RoomA.Doors.Find(door => door.TargetArea == Id);
            if (targetDoor != null)
                return targetDoor.Direction;
            else
                return Direction.Right;
        }
    }
    public Direction DirectionFromB
    {
        get
        {
            var targetDoor = RoomB.Doors.Find(door => door.TargetArea == Id);
            if (targetDoor != null)
                return targetDoor.Direction;
            else
                return Direction.Right;
        }
    }

    public Hallway(string id)
    {
        Id = id;
        Halls = new List<HallSector>();
    }

    public bool Connects(DungeonRoom room)
    {
        return RoomA == room || RoomB == room;
    }
    public bool Connects(DungeonRoom roomOne, DungeonRoom roomTwo)
    {
        return (RoomA == roomOne && RoomB == roomTwo) || (RoomB == roomOne && RoomA == roomTwo);
    }
    public DungeonRoom OppositeRoom(DungeonRoom room)
    {
        if (RoomA.Id == room.Id)
            return RoomB;
        if (RoomB.Id == room.Id)
            return RoomA;
        return null;
    }
}