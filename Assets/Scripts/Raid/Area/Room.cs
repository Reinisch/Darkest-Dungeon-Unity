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
    public int GridX { get; set; }
    public int GridY { get; set; }
    public List<Door> Doors { get; set; }

    public Room(string id, int gridX, int gridY)
    {
        Id = id;

        GridX = gridX;
        GridY = gridY;

        Type = AreaType.Empty;
        Knowledge = Knowledge.Hidden;
        Prop = null;

        TextureId = "";
        MashId = 0;
        Doors = new List<Door>();
    }
}
