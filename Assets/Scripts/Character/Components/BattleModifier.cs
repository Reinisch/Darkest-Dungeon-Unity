using System.Collections.Generic;
using UnityEngine;

public class BattleModifier
{
    public bool DisableStallPenalty { get; private set; }
    public bool CanSurprise { get; private set; }
    public bool CanBeSurprised { get; private set; }
    public bool AlwaysSurprise { get; private set; }
    public bool AlwaysBeSurprised { get; private set; }
    public bool IsValidFriendlyTarget { get; private set; }
    public bool CanRelieveStressFromKills { get; private set; }
    public bool CanRelieveStressFromCrit { get; private set; }
    public bool CanBeSummonRank { get; private set; }
    public bool CanBeMissed { get; private set; }

    public bool? CanBeHit { get; private set; }
    public bool? CanBeDamagedDirectly { get; private set; }

    public BattleModifier()
    {
        DisableStallPenalty = false;
        CanSurprise = true;
        CanBeSurprised = true;
        AlwaysSurprise = false;
        AlwaysBeSurprised = false;
        IsValidFriendlyTarget = true;
        CanRelieveStressFromKills = true;
        CanRelieveStressFromCrit = true;
        CanBeSummonRank = false;
        CanBeMissed = true;

        CanBeHit = true;
        CanBeDamagedDirectly = true;
    }

    public BattleModifier(List<string> data)
    {
        DisableStallPenalty = false;
        CanSurprise = true;
        CanBeSurprised = true;
        AlwaysSurprise = false;
        AlwaysBeSurprised = false;
        IsValidFriendlyTarget = true;
        CanRelieveStressFromKills = true;
        CanRelieveStressFromCrit = true;
        CanBeSummonRank = false;
        CanBeMissed = true;

        CanBeHit = true;
        CanBeDamagedDirectly = true;

        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".disable_stall_penalty":
                    DisableStallPenalty = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_surprise":
                    CanSurprise = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_surprised":
                    CanBeSurprised = bool.Parse(data[++i].ToLower());
                    break;
                case ".always_surprise":
                    AlwaysSurprise = bool.Parse(data[++i].ToLower());
                    break;
                case ".always_be_surprised":
                    AlwaysBeSurprised = bool.Parse(data[++i].ToLower());
                    break;
                case ".is_valid_friendly_target":
                    IsValidFriendlyTarget = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_relieve_stress_from_killing_blow":
                    CanRelieveStressFromKills = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_summon_rank":
                    CanBeSummonRank = bool.Parse(data[++i].ToLower());
                    break;
                case ".does_count_as_monster_size_for_monster_brain":
                    ++i;
                    break;
                case ".does_count_towards_stall_penalty":
                    ++i;
                    break;
                case ".can_relieve_stress_from_crit":
                    CanRelieveStressFromCrit = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_missed":
                    CanBeMissed = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_random_target":
                    ++i;
                    break;
                case ".can_be_hit":
                    CanBeHit = bool.Parse(data[++i].ToLower());
                    break;
                case ".can_be_damaged_directly":
                    CanBeDamagedDirectly = bool.Parse(data[++i].ToLower());
                    break;
                default:
                    Debug.LogError("Unexpected token in battle modifiers: " + data[i]);
                    break;
            }
        }
    }
}