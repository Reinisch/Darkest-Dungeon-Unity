using System.Collections.Generic;
using UnityEngine;

public class DeathDamage
{
    public string TargetBaseClass { get; private set; }
    public int TargetDamage { get; private set; }

    public DeathDamage(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".target_base_class_id":
                    TargetBaseClass = data[++i];
                    break;
                case ".target_damage":
                    TargetDamage = int.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in death damage: " + data[i]);
                    break;
            }
        }
    }
}