using System.IO;
using UnityEngine;

public class DungeonProgress : IBinarySaveData
{
    public string DungeonName { get; private set; }
    public int MasteryLevel { get; private set; }
    public bool IsEvent { get; private set; }
    public bool IsMeetingSaveCriteria { get { return true; } }

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
                if (DarkestDungeonManager.Campaign.CompletedPlot.Contains(dungeonPlot[i].Id))
                    continue;

                var plotDependency = dungeonPlot[i].PlotDependency;
                if (plotDependency != null)
                {
                    if (DarkestDungeonManager.Campaign.CompletedPlot.Contains(plotDependency))
                        return dungeonPlot[i];
                }
                else
                    return dungeonPlot[i];
            }
            return null;
        }
    }

    private int CurrentXP { get; set; }
    private int NextLevelXP { get; set; }
    private bool IsUnlocked { get; set; }

    private const int MaxLevel = 7;

    public DungeonProgress()
    {
    }

    public DungeonProgress(string dungeonName, int masteryLevel, int currentXP, bool isUnlocked, bool isEvent)
    {
        DungeonName = dungeonName;
        IsUnlocked = isUnlocked;
        IsEvent = isEvent;
        MasteryLevel = Mathf.Clamp(masteryLevel, 0, MaxLevel);
        CurrentXP = currentXP;

        UpdateNextLevelXP();
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(DungeonName);
        bw.Write(MasteryLevel);
        bw.Write(CurrentXP);
        bw.Write(IsUnlocked);
        bw.Write(IsEvent);
    }

    public void Read(BinaryReader br)
    {
        DungeonName = br.ReadString();
        MasteryLevel = br.ReadInt32();
        CurrentXP = br.ReadInt32();
        IsUnlocked = br.ReadBoolean();
        IsEvent = br.ReadBoolean();

        UpdateNextLevelXP();
    }

    public void AddExperience(int expAmount)
    {
        if (MasteryLevel == MaxLevel)
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

            if (MasteryLevel < MaxLevel)
            {
                MasteryLevel++;
                CurrentXP -= NextLevelXP;
                UpdateNextLevelXP();
            }
            else
            {
                UpdateNextLevelXP();
                CurrentXP = NextLevelXP;
                break;
            }
        }
    }

    private void UpdateNextLevelXP()
    {
        NextLevelXP = DarkestDungeonManager.Data.CampaignGeneration.DungeonXpLevelThreshold[Mathf.Clamp(MasteryLevel + 1, 0, MaxLevel)];
    }
}