public class HallSector : Area
{
    public Hallway Hallway { get; private set; }

    public HallSector(string id, Hallway parentHallway)
    {
        Id = id;
        Hallway = parentHallway;

        Knowledge = Knowledge.Hidden;
        Type = AreaType.Empty;
        Prop = null;

        TextureId = "0";
        MashId = 0;
    }

    public HallSector(string id, Hallway parentHallway, Door door)
    {
        Id = id;
        Hallway = parentHallway;

        Knowledge = Knowledge.Hidden;
        Type = AreaType.Door;
        Prop = door;

        TextureId = "0";
        MashId = 0;
    }
}
