using System.IO;

public class HallSector : Area
{
    public Hallway Hallway { get; set; }


    public HallSector()
    {
        Knowledge = Knowledge.Hidden;
        TextureId = "0";
        MashId = 0;
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway) : this()
    {
        Id = id;
        Hallway = parentHallway;

        GridX = gridX;
        GridY = gridY;

        Type = AreaType.Empty;
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway, Door door) : this()
    {
        Id = id;
        Hallway = parentHallway;

        GridX = gridX;
        GridY = gridY;

        Prop = door;
        Type = AreaType.Door;
    }
}
