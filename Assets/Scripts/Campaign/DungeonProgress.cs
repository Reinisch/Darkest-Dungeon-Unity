using UnityEngine;

public class DungeonProgress
{
    const int maxLevel = 7;

    public string DungeonName { get; set; }
    public int MasteryLevel { get; set; }
    
    public int CurrentXP { get; set; }
    public int NextLevelXP { get; set; }

    public bool PlotQuestCompleted { get; set; }
    public bool IsUnlocked { get; set; }

    public float XpRatio
    {
        get
        {
            if (NextLevelXP == 0)
                return 1;

            return (float)CurrentXP / NextLevelXP;
        }
    }
    public PlotQuest CurrentPlotQuest
    {
        get
        {
            var dungeonPlot = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.FindAll(item =>
                item.Dungeon == DungeonName && item.DungeonLevel <= MasteryLevel);
            for(int i = 0; i < dungeonPlot.Count; i++)
            {
                if (!DarkestDungeonManager.Campaign.CompletedPlot.Contains(dungeonPlot[i].Id))
                {
                    var plotDependency = (dungeonPlot[i] as PlotQuest).PlotDependency;
                    if (plotDependency != null)
                    {
                        if (DarkestDungeonManager.Campaign.CompletedPlot.Contains(plotDependency))
                            return dungeonPlot[i];
                    }
                    else
                        return dungeonPlot[i];
                }
            }
            return null;
        }
    }

    public DungeonProgress(string dungeonName, int masteryLevel, int currentXP, bool isUnlocked, bool plotCompleted)
    {
        DungeonName = dungeonName;

        IsUnlocked = isUnlocked;
        PlotQuestCompleted = plotCompleted;

        MasteryLevel = Mathf.Clamp(masteryLevel, 0, maxLevel);
        CurrentXP = currentXP;
        NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.DungeonXpLevelThreshold[Mathf.Clamp(MasteryLevel + 1, 0, maxLevel)];
    }

    public void AddExperience(int expAmount)
    {
        if (MasteryLevel == maxLevel)
            return;

        CurrentXP += expAmount;

        while (CurrentXP >= NextLevelXP)
        {
            PlotQuest masteryQuest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(item =>
                item.Dungeon == DungeonName && item.DungeonLevel == MasteryLevel);
            if(masteryQuest != null)
            {
                if (!DarkestDungeonManager.Campaign.CompletedPlot.Contains(masteryQuest.Id))
                {
                    CurrentXP = NextLevelXP;
                    break;
                }
            }

            if (MasteryLevel < maxLevel)
            {
                MasteryLevel++;
                CurrentXP = CurrentXP - NextLevelXP;
                NextLevelXP = NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.
                    DungeonXpLevelThreshold[Mathf.Clamp(MasteryLevel + 1, 0, maxLevel)];
            }
            else
            {
                NextLevelXP = NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.DungeonXpLevelThreshold[maxLevel];
                CurrentXP = NextLevelXP;
                break;
            }
        }
    }
}