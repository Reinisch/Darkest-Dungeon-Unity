using System.Collections.Generic;
using UnityEngine;

public class LifeLink
{
    public string LinkBaseClass { get; private set; }

    public LifeLink(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".base_class":
                    LinkBaseClass = data[++i];
                    break;
                case ".does_spawn_loot":
                    ++i;
                    break;
                default:
                    Debug.LogError("Unexpected token in life link: " + data[i]);
                    break;
            }
        }
    }
}