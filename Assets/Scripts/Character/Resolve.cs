using UnityEngine;

public class Resolve
{
    public int CurrentXP { get; set; }
    public int Level { get; private set; }
    public int NextLevelXP { get; private set; }

    public float Ratio
    {
        get
        {
            if (NextLevelXP == 0)
                return 1;

            return (float)CurrentXP / NextLevelXP;
        }
    }

    private const int MaxLevel = 6;

    public Resolve(int level, int currentXP)
    {
        Level = Mathf.Clamp(level, 0, MaxLevel);
        CurrentXP = currentXP;
        
        NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.HeroXpLevelThreshold[Mathf.Clamp(level + 1, 0, MaxLevel)];
    }

    public int AddExperience(int experience)
    {
        if (Level == MaxLevel)
            return 0;

        int initialLevel = Level;

        CurrentXP += experience;
        
        while(CurrentXP >= NextLevelXP)
        {
            if(Level < MaxLevel)
            {
                Level++;
                CurrentXP = CurrentXP - NextLevelXP;
                NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.HeroXpLevelThreshold[Mathf.Clamp(Level + 1, 0, MaxLevel)];
            }
            else
            {
                NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.HeroXpLevelThreshold[MaxLevel];
                CurrentXP = NextLevelXP;
                break;
            }
        }
        return Level - initialLevel;
    }
}