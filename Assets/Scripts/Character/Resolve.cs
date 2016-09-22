using UnityEngine;

public class Resolve
{
    const int maxLevel = 6;

    public int Level { get; set; }

    public int CurrentXP { get; set; }
    public int NextLevelXP { get; set; }
    public float Ratio
    {
        get
        {
            if (NextLevelXP == 0)
                return 1;

            return (float)CurrentXP / NextLevelXP;
        }
    }

    public Resolve(int level, int currentXP)
    {
        Level = Mathf.Clamp(level, 0, maxLevel);
        CurrentXP = currentXP;
        
        NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.HeroXpLevelThreshold[Mathf.Clamp(level + 1, 0, maxLevel)];
    }

    public int AddExperience(int experience)
    {
        if (Level == maxLevel)
            return 0;

        int initialLevel = Level;

        CurrentXP += experience;
        
        while(CurrentXP >= NextLevelXP)
        {
            if(Level < maxLevel)
            {
                Level++;
                CurrentXP = CurrentXP - NextLevelXP;
                NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.HeroXpLevelThreshold[Mathf.Clamp(Level + 1, 0, maxLevel)];
            }
            else
            {
                NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.HeroXpLevelThreshold[maxLevel];
                CurrentXP = NextLevelXP;
                break;
            }
        }
        return Level - initialLevel;
    }
}