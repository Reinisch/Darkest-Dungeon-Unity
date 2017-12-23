using System.Collections.Generic;
using UnityEngine;

public class LootDefinition
{
    public string Code { get; set; }
    public int Count { get; set; }

    public LootDefinition()
    {
    }

    public LootDefinition(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".code":
                    Code = data[++i];
                    break;
                case ".count":
                    Count = int.Parse(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in loot: " + data[i]);
                    break;
            }
        }
    }
}