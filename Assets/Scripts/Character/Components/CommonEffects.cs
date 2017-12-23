using UnityEngine;
using System.Collections.Generic;

public class CommonEffects
{
    public string DeathEffect { get; private set; }

    public CommonEffects(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".deathfx":
                    DeathEffect = data[++i];
                    break;
                default:
                    Debug.LogError("Unexpected token in common effects: " + data[i]);
                    break;
            }
        }
    }
}