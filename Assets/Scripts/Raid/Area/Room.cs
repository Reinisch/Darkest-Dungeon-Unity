using System.Collections.Generic;
using System.IO;

public class DungeonRoom : Area
{
    public int Connections
    {
        get
        {
            return Doors.Count;
        }
    }
   
    public List<Door> Doors { get; set; }


    public DungeonRoom()
    {
        Type = AreaType.Empty;
        Knowledge = Knowledge.Hidden;
        Prop = null;

        TextureId = "";
        MashId = 0;
        Doors = new List<Door>();
    }

    public DungeonRoom(string id, int gridX, int gridY) : this()
    {
        Id = id;

        GridX = gridX;
        GridY = gridY;
    }


    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);

        Doors.Write(bw);
    }

    public override void Read(BinaryReader br)
    {
        base.Read(br);

        Doors.Read(br);
    }
}