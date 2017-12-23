using System.Collections.Generic;
using UnityEngine;

public class SkillReaction
{
    public List<Effect> WasHitPerformerEffects { get; private set; }
    public List<Effect> WasKilledOtherMonstersEffects { get; private set; }

    public SkillReaction(List<string> data)
    {
        WasHitPerformerEffects = new List<Effect>();
        WasKilledOtherMonstersEffects = new List<Effect>();

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".was_hit_performer_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            WasHitPerformerEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in skill reaction");
                    }
                    break;
                case ".was_killed_other_monsters_effects":
                    while (++i < data.Count && data[i--][0] != '.')
                    {
                        if (data[++i].Length < 2)
                            continue;

                        if (DarkestDungeonManager.Data.Effects.ContainsKey(data[i]))
                            WasKilledOtherMonstersEffects.Add(DarkestDungeonManager.Data.Effects[data[i]]);
                        else
                            Debug.LogError("Missing effect " + data[i] + " in skill reaction");
                    }
                    break;
                default:
                    Debug.LogError("Unexpected token in skill reaction: " + data[i]);
                    break;
            }
        }
    }
}