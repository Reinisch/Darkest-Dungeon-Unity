using System.Collections.Generic;

public class QuestDatabase
{
    public int FailStressPenalty { get; set; }

    public Dictionary<string, QuestGoal> QuestGoals { get; private set; }
    public Dictionary<string, QuestType> QuestTypes { get; private set; }
    public List<PlotQuest> PlotQuests { get; private set; }
    public List<int> LevelRestrictions { get; private set; }
    public QuestGenerationData QuestGeneration { get; set; }

    public List<string> TownProgressionGoalIds { get; set; }

    public QuestDatabase()
    {
        QuestGoals = new Dictionary<string, QuestGoal>();
        QuestTypes = new Dictionary<string, QuestType>();
        PlotQuests = new List<PlotQuest>();
        LevelRestrictions = new List<int>();
    }
}