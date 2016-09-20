using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Curio : Prop
{
    public bool IsFullCurio { get; set; }
    public bool IsQuestCurio { get; set; }
    public string ResultTypes { get; set; }
    public string RegionFound { get; set; }

    public List<string> Tags { get; set; }

    public List<CurioInteraction> Results { get; set; }
    public List<ItemInteraction> ItemInteractions { get; set; }

    public Curio(string id)
    {
        StringId = id;
        Type = AreaType.Curio;
        Tags = new List<string>();
        Results = new List<CurioInteraction>();
        ItemInteractions = new List<ItemInteraction>();
    }
}