using System.Collections.Generic;
using UnityEngine;

public class Controller
{
    public int StressPerTurn { get; private set; }

    public List<string> UncontrollEffects { get; private set; }

    public Controller(List<string> data)
    {
        UncontrollEffects = new List<string>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".stress_per_controlled_turn":
                    StressPerTurn = int.Parse(data[++i]);
                    break;
                case ".uncontrol_effects":
                    while (++i < data.Count && data[i][0] != '.')
                        UncontrollEffects.Add(data[i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in controller captor: " + data[i]);
                    break;
            }
        }
    }
}