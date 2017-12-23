using System.Collections.Generic;
using UnityEngine;

public class Initiative
{
    public int NumberOfTurns { get; private set; }

    public Initiative(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".number_of_turns_per_round":
                    NumberOfTurns = int.Parse(data[++i]);
                    break;
                case ".hide_indicator":
                    ++i;
                    break;
                default:
                    Debug.LogError("Unexpected token in initiative: " + data[i]);
                    break;
            }
        }
    }
}