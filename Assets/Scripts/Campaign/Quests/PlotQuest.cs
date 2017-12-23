using System.Collections.Generic;

public sealed class PlotQuest : Quest
{
    public override string Id { get; set; }
    public int DungeonLevel { get; set; }
    public string RaidMap { get; set; }
    public string PlotDependency { get; set; }

    public PlotTrinketReward PlotTrinket { get; set; }

    public PlotQuest():base(true)
    {
    }

    public PlotQuest(string id, PlotTrinketReward trinketReward) : this()
    {
        Id = id;
        PlotTrinket = trinketReward;
    }

    public PlotQuest Copy()
    {
        var newQuest = new PlotQuest();
        newQuest.Id = Id;
        newQuest.PlotDependency = PlotDependency;
        newQuest.DungeonLevel = DungeonLevel;
        newQuest.PlotTrinket = PlotTrinket;
        newQuest.IsPlotQuest = true;
        newQuest.Type = Type;
        newQuest.Dungeon = Dungeon;
        newQuest.Difficulty = Difficulty;
        newQuest.Length = Length;
        newQuest.RaidMap = RaidMap;
        newQuest.Goal = Goal;

        newQuest.IsProgression = IsProgression;
        newQuest.HasStatueContents = HasStatueContents;
        newQuest.CompletionDungeonXp = CompletionDungeonXp;
        newQuest.CanRetreat = CanRetreat;
        newQuest.AlwaysRetreatFromRaid = AlwaysRetreatFromRaid;
        newQuest.RetreatKillCount = RetreatKillCount;
        newQuest.IsSurpriseEnabled = IsSurpriseEnabled;
        newQuest.IsScoutingEnabled = IsScoutingEnabled;
        newQuest.IsStressClearedOnCompletion = IsStressClearedOnCompletion;
        newQuest.RosterBuffOnFailureMinimumPartyResolveLevel = RosterBuffOnFailureMinimumPartyResolveLevel;
        newQuest.RosterBuffsOnFailure = RosterBuffsOnFailure;
        newQuest.SuggestedTrinkets = SuggestedTrinkets;
        newQuest.UpgradeTagsRemovedOnIgnore = UpgradeTagsRemovedOnIgnore;

        newQuest.Reward = new CompletionReward();
        newQuest.Reward.ResolveXP = Reward.ResolveXP;
        newQuest.Reward.ItemDefinitions = new List<ItemDefinition>(Reward.ItemDefinitions);
        return newQuest;
    }
}