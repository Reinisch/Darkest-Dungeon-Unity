using System.Collections.Generic;

public interface IQuestData
{
    string GetDataString(string goalType);
    bool IsQuestCompleted();
}

public class UpgradeTag
{
    public string Tag { get; set; }
    public int Amount { get; set; }

    public UpgradeTag(string tag, int amount)
    {
        Tag = tag;
        Amount = amount;
    }
}

public class Quest
{
    public virtual string Id
    {
        get
        {
            return Type;
        }
        set
        {

        }
    }
    public bool IsPlotQuest { get; set; }
    public string Type { get; set; }
    public string Dungeon { get; set; }
    public int Difficulty { get; set; }
    public int Length { get; set; }
    public QuestGoal Goal { get; set; }
    public CompletionReward Reward { get; set; }

    public bool IsProgression { get; set; }
    public bool HasStatueContents { get; set; }
    public bool CompletionDungeonXp { get; set; }
    public bool CanRetreat { get; set; }
    public bool AlwaysRetreatFromRaid { get; set; }
    public int RetreatKillCount { get; set; }
    public bool IsSurpriseEnabled { get; set; }
    public bool IsScoutingEnabled { get; set; }
    public bool IsStressClearedOnCompletion { get; set; }
    public int RosterBuffOnFailureMinimumPartyResolveLevel { get; set; }
    public List<Buff> RosterBuffsOnFailure { get; set; }
    public List<ItemDefinition> SuggestedTrinkets { get; set; }
    public List<UpgradeTag> UpgradeTagsRemovedOnIgnore { get; set; }

    public Quest(bool isPlot = false)
    {
        IsPlotQuest = isPlot;
        IsProgression = true;
        HasStatueContents = false;
        CompletionDungeonXp = true;
        CanRetreat = true;
        AlwaysRetreatFromRaid = true;
        RetreatKillCount = 0;
        IsSurpriseEnabled = true;
        IsScoutingEnabled = true;
        IsStressClearedOnCompletion = false;
        RosterBuffOnFailureMinimumPartyResolveLevel = 10;
        RosterBuffsOnFailure = new List<Buff>();
        SuggestedTrinkets = new List<ItemDefinition>();
        UpgradeTagsRemovedOnIgnore = new List<UpgradeTag>();
    }
}
