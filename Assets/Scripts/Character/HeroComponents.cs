using UnityEngine;
using System.Collections.Generic;

public class CharacterMode
{
    public string Id { get; set; }
    public bool IsRaidDefault { get; set; }
    public string BarkOverrideId { get; set; }
    public string AfflictionSkillId { get; set; }
    public string BattleCompleteSkillId { get; set; }
    public int StressPerTurn { get; set; }

    public CharacterMode(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".id":
                    Id = data[++i];
                    break;
                case ".bark_override_id":
                    BarkOverrideId = data[++i];
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
public class DeathDoor
{
    public List<string> Buffs { get; set; }
    public List<string> RecoveryBuffs { get; set; }
    public List<string> RecoveryHeartAttackBuffs { get; set; }

    public DeathDoor()
    {
        Buffs = new List<string>();
        RecoveryBuffs = new List<string>();
        RecoveryHeartAttackBuffs = new List<string>();
    }
    public DeathDoor(List<string> data)
    {
        Buffs = new List<string>();
        RecoveryBuffs = new List<string>();
        RecoveryHeartAttackBuffs = new List<string>();

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
                        RecoveryHeartAttackBuffs.Add(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in death door: " + data[i]);
                    break;
            }
        }
    }
}
public class HeroGeneration
{
    public int NumberOfPositiveQuirksMin { get; set; }
    public int NumberOfPositiveQuirksMax { get; set; }
    public int NumberOfNegativeQuirksMin { get; set; }
    public int NumberOfNegativeQuirksMax { get; set; }
    public int NumberOfSpecificCampingSkills { get; set; }
    public int NumberOfSharedCampingSkills { get; set; }
    public int NumberOfRandomCombatSkills { get; set; }

    public HeroGeneration()
    {
        NumberOfPositiveQuirksMin = 1;
        NumberOfPositiveQuirksMax = 2;
        NumberOfNegativeQuirksMin = 1;
        NumberOfNegativeQuirksMax = 2;
        NumberOfSpecificCampingSkills = 2;
        NumberOfSharedCampingSkills = 1;
        NumberOfRandomCombatSkills = 4;
    }
    public HeroGeneration(List<string> data)
    {
        LoadData(data);
    }

    public void LoadData(List<string> data)
    {
        for (int i = 1; i < data.Count; i++)
        {
            switch (data[i])
            {
                case ".number_of_positive_quirks_min":
                    NumberOfPositiveQuirksMin = int.Parse(data[++i]);
                    break;
                case ".number_of_positive_quirks_max":
                    NumberOfPositiveQuirksMax = int.Parse(data[++i]);
                    break;
                case ".number_of_negative_quirks_min":
                    NumberOfNegativeQuirksMin = int.Parse(data[++i]);
                    break;
                case ".number_of_negative_quirks_max":
                    NumberOfNegativeQuirksMax = int.Parse(data[++i]);
                    break;
                case ".number_of_class_specific_camping_skills":
                    NumberOfSpecificCampingSkills = int.Parse(data[++i]);
                    break;
                case ".number_of_shared_camping_skills":
                    NumberOfSharedCampingSkills = int.Parse(data[++i]);
                    break;
                case ".number_of_random_combat_skills":
                    NumberOfRandomCombatSkills = int.Parse(data[++i]);
                    break;
                default:
                    Debug.LogError("Unexpected token in hero generation: " + data[i]);
                    break;
            }
        }
    }
}