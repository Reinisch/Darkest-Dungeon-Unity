using System.Collections.Generic;
using UnityEngine;

public class DeathClass
{
    public string CorpseClass { get; private set; }
    public DeathClassType Type { get; private set; }
    public bool? CanDieFromDamage { get; private set; }
    public List<Effect> DeathChangeEffects { get; private set; }

    public DeathClass(List<string> data)
    {
        DeathChangeEffects = new List<Effect>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".monster_class_id":
                    CorpseClass = data[++i];
                    break;
                case ".is_valid_on_crit":
                    ++i;
                    break;
                case ".is_valid_on_bleed_dot":
                    ++i;
                    break;
                case ".is_valid_on_blight_dot":
                    ++i;
                    break;
                case ".can_die_from_damage":
                    CanDieFromDamage = bool.Parse(data[++i].ToLower());
                    break;
                case ".carry_over_hp_min_percent":
                    ++i;
                    break;
                case ".change_class_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            DeathChangeEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in death class");
                    }
                    break;
                case ".type":
                    if (data[++i] == "corpse")
                        Type = DeathClassType.Corpse;
                    break;
                default:
                    Debug.LogError("Unexpected token in death class: " + data[i]);
                    break;
            }
        }
    }
}