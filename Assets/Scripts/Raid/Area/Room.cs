using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : Area
{
    public int Connections
    {
        get
        {
            return Doors.Count;
        }
    }

    public List<Door> Doors { get; set; }

    public Room(string id)
    {
        Id = id;

        Type = AreaType.Empty;
        Knowledge = Knowledge.Hidden;
        Prop = null;

        TextureId = "";
        MashId = 0;
        Doors = new List<Door>();
    }
}
