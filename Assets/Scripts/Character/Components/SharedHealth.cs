using System.Collections.Generic;
using UnityEngine;

public class SharedHealth
{
    public SharedHealth(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".id":
                    ++i;
                    break;
                default:
                    Debug.LogError("Unexpected token in shared health: " + data[i]);
                    break;
            }
        }
    }
}