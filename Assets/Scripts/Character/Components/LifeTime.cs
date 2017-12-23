using System.Collections.Generic;
using UnityEngine;

public class LifeTime
{
    public int AliveRoundLimit { get; private set; }

    public LifeTime(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".alive_round_limit":
                    AliveRoundLimit = int.Parse(data[++i]);
                    break;
                case ".does_check_for_loot":
                    ++i;
                    break;
                default:
                    Debug.LogError("Unexpected token in life time: " + data[i]);
                    break;
            }
        }
    }
}