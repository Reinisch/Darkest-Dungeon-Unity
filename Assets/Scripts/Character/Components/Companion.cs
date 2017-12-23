using System.Collections.Generic;
using UnityEngine;

public class Companion
{
    public string MonsterClass { get; private set; }
    public float HealPerTurn { get; private set; }
    public List<Buff> Buffs { get; private set; }

    public Companion(List<string> data)
    {
        Buffs = new List<Buff>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".monster_class":
                    MonsterClass = data[++i];
                    break;
                case ".heal_per_turn_percent":
                    HealPerTurn = float.Parse(data[++i]);
                    break;
                case ".buffs":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        i++;
                        if (DarkestDungeonManager.Data.Buffs.ContainsKey(data[i]))
                            Buffs.Add(DarkestDungeonManager.Data.Buffs[data[i]]);
                        else
                            Debug.LogError("Missing buff " + data[i] + " in Companion " + MonsterClass);
                    }
                    break;
                default:
                    Debug.LogError("Unexpected token in companion: " + data[i]);
                    break;
            }
        }
    }
}