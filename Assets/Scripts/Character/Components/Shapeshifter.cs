using System.Collections.Generic;
using UnityEngine;

public class Shapeshifter
{
    public List<string> MonsterClassIds { get; private set; }
    public List<int> MonsterClassChances { get; private set; }
    public List<FormationSet> MonsterClassValidRanks { get; private set; }

    public Shapeshifter(List<string> data)
    {
        MonsterClassIds = new List<string>();
        MonsterClassChances = new List<int>();
        MonsterClassValidRanks = new List<FormationSet>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".fx_name":
                    ++i;
                    break;
                case ".round_frequency":
                    ++i;
                    break;
                case ".monster_class_ids":
                    while (++i < data.Count && data[i--][0] != '.')
                        MonsterClassIds.Add(data[++i]);
                    break;
                case ".monster_class_chances":
                    while (++i < data.Count && data[i--][0] != '.')
                        MonsterClassChances.Add(int.Parse(data[++i]));
                    break;
                case ".monster_class_valid_ranks":
                    while (++i < data.Count && data[i--][0] != '.')
                        MonsterClassValidRanks.Add(new FormationSet(data[++i]));
                    break;
                default:
                    Debug.LogError("Unexpected token in shapeshifter: " + data[i]);
                    break;
            }
        }
    }
}