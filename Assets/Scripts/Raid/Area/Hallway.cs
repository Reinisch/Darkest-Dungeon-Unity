using System.Collections.Generic;
using System.IO;

public class Hallway : IBinarySaveData
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
    public bool IsMeetingSaveCriteria { get { return true; } }

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

    private string roomAId;
    private string roomBId;

    public Hallway()
    {
        Halls = new List<HallSector>();
    }

    public Hallway(string id) : this()
    {
        Id = id;
    }

    public Hallway(string id, DungeonRoom fromRoomB, DungeonRoom toRoomA, Direction directionFromB, Direction directionFromA) : this(id)
    {
        RoomA = toRoomA;
        RoomB = fromRoomB;
        RoomA.Doors.Add(new Door(toRoomA.Id, Id, directionFromA));
        RoomB.Doors.Add(new Door(fromRoomB.Id, Id, directionFromB));
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

    public void AddHall(HallSector hallSector)
    {
        Halls.Add(hallSector);
    }

    public void Initialize(Dungeon dungeon)
    {
        RoomA = dungeon.Rooms[roomAId];
        RoomB = dungeon.Rooms[roomBId];

        foreach (var hall in Halls)
            hall.Hallway = this;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(Id);
        bw.Write(RoomA.Id);
        bw.Write(RoomB.Id);
        Halls.Write(bw);
    }

    public void Read(BinaryReader br)
    {
        Id = br.ReadString();
        roomAId = br.ReadString();
        roomBId = br.ReadString();
        Halls.Read(br);
    }
}