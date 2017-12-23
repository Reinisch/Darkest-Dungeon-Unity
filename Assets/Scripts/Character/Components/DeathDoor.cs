using System.Collections.Generic;
using UnityEngine;

public class DeathDoor
{
    public List<string> Buffs { get; private set; }
    public List<string> RecoveryBuffs { get; private set; }

    public DeathDoor(List<string> data)
    {
        Buffs = new List<string>();
        RecoveryBuffs = new List<string>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".buffs":
                    while (++i < data.Count && data[i--][0] != '.')
                        Buffs.Add(data[++i]);
                    break;
                case ".recovery_buffs":
                    while (++i < data.Count && data[i--][0] != '.')
                        RecoveryBuffs.Add(data[++i]);
                    break;
                case ".recovery_heart_attack_buffs":
                    while (++i < data.Count && data[i--][0] != '.')
                        ++i;
                    break;
                default:
                    Debug.LogError("Unexpected token in death door: " + data[i]);
                    break;
            }
        }
    }
}