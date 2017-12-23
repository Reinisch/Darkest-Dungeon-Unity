using System.Collections.Generic;
using UnityEngine;

public class HeroGeneration
{
    public int NumberOfPositiveQuirksMin { get; private set; }
    public int NumberOfPositiveQuirksMax { get; private set; }
    public int NumberOfNegativeQuirksMin { get; private set; }
    public int NumberOfNegativeQuirksMax { get; private set; }
    public int NumberOfSpecificCampingSkills { get; private set; }
    public int NumberOfSharedCampingSkills { get; private set; }
    public int NumberOfRandomCombatSkills { get; private set; }

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