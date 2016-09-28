public class HallSector : Area
{
    public Hallway Hallway { get; private set; }

    public int GridX { get; set; }
    public int GridY { get; set; }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway)
    {
        Id = id;
        Hallway = parentHallway;
        GridX = gridX;
        GridY = gridY;

        Knowledge = Knowledge.Hidden;
        Type = AreaType.Empty;
        Prop = null;

        TextureId = "0";
        MashId = 0;
    }

    public HallSector(string id, int gridX, int gridY, Hallway parentHallway, Door door)
    {
        Id = id;
        Hallway = parentHallway;
        GridX = gridX;
        GridY = gridY;

        Knowledge = Knowledge.Hidden;
        Type = AreaType.Door;
        Prop = door;

        TextureId = "0";
        MashId = 0;
    }
}
