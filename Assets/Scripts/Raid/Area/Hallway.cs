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

    public Room RoomA { get; set; }
    public Room RoomB { get; set; }

    public Hallway(string id)
    {
        Id = id;
        Halls = new List<HallSector>();
    }

    public bool Connects(Room room)
    {
        return RoomA == room || RoomB == room;
    }
    public bool Connects(Room roomOne, Room roomTwo)
    {
        return (RoomA == roomOne && RoomB == roomTwo) || (RoomB == roomOne && RoomA == roomTwo);
    }
    public Room OppositeRoom(Room room)
    {
        if (RoomA.Id == room.Id)
            return RoomB;
        if (RoomB.Id == room.Id)
            return RoomA;
        return null;
    }
}