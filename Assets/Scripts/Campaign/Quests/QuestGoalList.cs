using System.Collections.Generic;

public class QuestGoalList
{
    public string Dungeon { get; set; }
    public List<string> Goals { get; private set; }

    public QuestGoalList()
    {
        Goals = new List<string>();
    }
}