using System.Collections.Generic;
using UnityEngine;

public class EmptyCaptor
{
    public string FullMonsterClass { get; private set; }
    public string PerformerBaseClass { get; private set; }
    public List<string> CaptureEffects { get; private set; }

    public EmptyCaptor(List<string> data)
    {
        CaptureEffects = new List<string>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".performing_monster_captor_base_class":
                    PerformerBaseClass = data[++i];
                    break;
                case ".captor_full_monster_class":
                    FullMonsterClass = data[++i];
                    break;
                case ".capture_effects":
                    while (++i < data.Count && data[i][0] != '.')
                        CaptureEffects.Add(data[i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in empty captor: " + data[i]);
                    break;
            }
        }
    }
}