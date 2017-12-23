using System.Collections.Generic;

public class QuestType
{
    public string Id { get; set; }
    public List<QuestGoalList> GoalLists { get; private set; }

    public QuestType()
    {
        GoalLists = new List<QuestGoalList>();
    }
}