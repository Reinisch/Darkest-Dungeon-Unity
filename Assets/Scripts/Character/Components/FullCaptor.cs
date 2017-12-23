using System.Collections.Generic;
using UnityEngine;

public class FullCaptor
{
    public string EmptyMonsterClass { get; private set; }
    public bool ReleasePrisonerAtDeathDoor { get; private set; }
    public bool ReleaseOnPrisonerAffliction { get; private set; }
    public float PerTurnDamagePercent { get; private set; }
    public float PerTurnStress { get; private set; }

    public List<string> ReleaseEffects { get; private set; }

    public FullCaptor(List<string> data)
    {
        ReleaseEffects = new List<string>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".switch_class_on_death":
                    ++i;
                    break;
                case ".captor_empty_monster_class":
                    EmptyMonsterClass = data[++i];
                    break;
                case ".release_on_death":
                    ++i;
                    break;
                case ".release_on_prisoner_at_deaths_door":
                    ReleasePrisonerAtDeathDoor = bool.Parse(data[++i].ToLower());
                    break;
                case ".release_on_prisoner_affliction":
                    ReleaseOnPrisonerAffliction = bool.Parse(data[++i].ToLower());
                    break;
                case ".per_turn_damage_percent":
                    PerTurnDamagePercent = float.Parse(data[++i]);
                    break;
                case ".per_turn_stress_damage":
                    PerTurnStress = float.Parse(data[++i]);
                    break;
                case ".has_prisoner_overlay":
                    ++i;
                    break;
                case ".release_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                        ReleaseEffects.Add(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in full captor: " + data[i]);
                    break;
            }
        }
    }
}