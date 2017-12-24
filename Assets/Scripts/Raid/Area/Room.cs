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

        TextureId = "";
        Doors = new List<Door>();
    }

    public DungeonRoom(string id, int gridX, int gridY) : this()
    {
        Id = id;

        GridX = gridX;
        GridY = gridY;
    }

    public DungeonRoom(string id, int gridX, int gridY, Knowledge knowledge, AreaType areaType, int mashId, string textureId)
    {
        Id = id;

        GridX = gridX;
        GridY = gridY;

        Knowledge = knowledge;
        Type = areaType;
        MashId = mashId;
        TextureId = textureId;

        Prop = null;
        BattleEncounter = null;
        Doors = new List<Door>();
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

    public override void Scout()
    {
        if (Knowledge == Knowledge.Hidden)
        {
            Knowledge = Knowledge.Scouted;
            RaidSceneManager.MapPanel.UpdateArea(this);
            FMODUnity.RuntimeManager.PlayOneShot("event:/general/map/scout_room");
        }
    }
}