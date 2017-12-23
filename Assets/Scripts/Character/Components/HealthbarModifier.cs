using System.Collections.Generic;
using UnityEngine;

public class HealthbarModifier
{
    public string Type { get; private set; }

    public HealthbarModifier(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".type":
                    Type = data[++i];
                    break;
                default:
                    Debug.LogError("Unexpected token in health bar: " + data[i]);
                    break;
            }
        }
    }
}