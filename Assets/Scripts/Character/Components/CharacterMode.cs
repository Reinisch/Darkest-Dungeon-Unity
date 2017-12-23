using UnityEngine;
using System.Collections.Generic;

public class CharacterMode
{
    public string Id { get; private set; }
    public bool IsRaidDefault { get; private set; }
    public string AfflictionSkillId { get; private set; }
    public string BattleCompleteSkillId { get; private set; }
    public int StressPerTurn { get; private set; }

    public CharacterMode(List<string> data)
    {
        LoadData(data);
    }

    private void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".id":
                    Id = data[++i];
                    break;
                case ".bark_override_id":
                    ++i;
                    break;
                case ".affliction_combat_skill_id":
                    AfflictionSkillId = data[++i];
                    break;
                case ".battle_complete_combat_skill_id":
                    BattleCompleteSkillId = data[++i];
                    break;
                case ".stress_damage_per_turn":
                    StressPerTurn = int.Parse(data[++i]);
                    break;
                case ".is_raid_default":
                    IsRaidDefault = bool.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in mode: " + data[i]);
                    break;
            }
        }
    }
}