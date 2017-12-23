using System.Collections.Generic;
using UnityEngine;

public class TorchlightModifier
{
    public int Min { get; private set; }
    public int Max { get; private set; }

    public TorchlightModifier(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public TorchlightModifier(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".min":
                    Min = int.Parse(data[++i]);
                    break;
                case ".max":
                    Max = int.Parse(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in torchlight modifier: " + data[i]);
                    break;
            }
        }
    }
}