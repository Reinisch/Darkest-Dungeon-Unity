using System.Collections.Generic;

public class QuestType
{
    public string Id { get; set; }
    public List<QuestGoalList> GoalLists { get; set; }

    public QuestType()
    {
        GoalLists = new List<QuestGoalList>();
    }
}

public class QuestGoalList
{
    public string Dungeon { get; set; }
    public List<string> Goals { get; set; }

    public QuestGoalList()
    {
        Goals = new List<string>();
    }
}