using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestGoal
{
    public string Id { get; set; }
    public string Type { get; set; }

    public List<ItemDefinition> StartingItems { get; set; }
    public IQuestData QuestData { get; set; }

    public QuestGoal()
    {
        StartingItems = new List<ItemDefinition>();
    }
}
