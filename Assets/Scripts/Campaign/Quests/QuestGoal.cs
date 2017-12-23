using System.Collections.Generic;

public class QuestGoal
{
    public string Id { get; set; }
    public string Type { get; set; }

    public List<ItemDefinition> StartingItems { get; private set; }
    public IQuestData QuestData { get; set; }

    public QuestGoal()
    {
        StartingItems = new List<ItemDefinition>();
    }
}