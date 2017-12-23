using System.Collections.Generic;
using UnityEngine;

public class Spawn
{
    public List<Effect> Effects { get; private set; }

    public Spawn(List<string> data)
    {
        Effects = new List<Effect>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            Effects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in spawn");
                    }
                    break;
                default:
                    Debug.LogError("Unexpected token in spawn: " + data[i]);
                    break;
            }
        }
    }
}