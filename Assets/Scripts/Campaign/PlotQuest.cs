using System.Collections.Generic;

public class PlotQuest : Quest
{
    public string Id { get; set; }
    public int DungeonLevel { get; set; }

    public PlotTrinketReward PlotTrinket { get; set; }

    public PlotQuest():base(true)
    {
    }

    public PlotQuest Copy()
    {
        var newQuest = new PlotQuest();
        newQuest.Id = Id;
        newQuest.DungeonLevel = DungeonLevel;
        newQuest.PlotTrinket = PlotTrinket;
        newQuest.IsPlotQuest = true;
        newQuest.Type = Type;
        newQuest.Dungeon = Dungeon;
        newQuest.Difficulty = Difficulty;
        newQuest.Length = Length;
        newQuest.Goal = Goal;

        newQuest.Reward = new CompletionReward();
        newQuest.Reward.ResolveXP = Reward.ResolveXP;
        newQuest.Reward.ItemDefinitions = new List<ItemDefinition>(Reward.ItemDefinitions);
        return newQuest;
    }
}

public class PlotTrinketReward
{
    public string Rarity { get; set; }
    public int Amount { get; set;}
}