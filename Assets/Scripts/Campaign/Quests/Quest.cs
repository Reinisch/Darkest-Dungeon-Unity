using System.Collections.Generic;
using System.IO;

public class Quest : IBinarySaveData
{
    public virtual string Id { get { return Type; } set { Type = value; } }

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

    public bool IsMeetingSaveCriteria { get { return true; } }

    public Quest()
    {
        RosterBuffsOnFailure = new List<Buff>();
        SuggestedTrinkets = new List<ItemDefinition>();
        UpgradeTagsRemovedOnIgnore = new List<UpgradeTag>();
    }

    public Quest(bool isPlot = false) : this()
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
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(IsPlotQuest ? ((PlotQuest)this).Id : "");
        bw.Write(Type);
        bw.Write(Dungeon);
        bw.Write(Difficulty);
        bw.Write(Length);
        bw.Write(Goal.Id);
        bw.Write(Reward.ResolveXP);
        bw.Write(Reward.ItemDefinitions.Count);
        for (int j = 0; j < Reward.ItemDefinitions.Count; j++)
        {
            bw.Write(Reward.ItemDefinitions[j].Id);
            bw.Write(Reward.ItemDefinitions[j].Type);
            bw.Write(Reward.ItemDefinitions[j].Amount);
        }

        bw.Write(IsProgression);
        bw.Write(HasStatueContents);
        bw.Write(CompletionDungeonXp);
        bw.Write(CanRetreat);
        bw.Write(AlwaysRetreatFromRaid);
        bw.Write(RetreatKillCount);
        bw.Write(IsSurpriseEnabled);
        bw.Write(IsScoutingEnabled);
        bw.Write(IsStressClearedOnCompletion);
        bw.Write(RosterBuffOnFailureMinimumPartyResolveLevel);
        bw.Write(RosterBuffsOnFailure.Count);
        for (int j = 0; j < RosterBuffsOnFailure.Count; j++)
            bw.Write(RosterBuffsOnFailure[j].Id);
        bw.Write(SuggestedTrinkets.Count);
        for (int j = 0; j < SuggestedTrinkets.Count; j++)
        {
            bw.Write(SuggestedTrinkets[j].Id);
            bw.Write(SuggestedTrinkets[j].Amount);
        }
        bw.Write(UpgradeTagsRemovedOnIgnore.Count);
        for (int j = 0; j < UpgradeTagsRemovedOnIgnore.Count; j++)
        {
            bw.Write(UpgradeTagsRemovedOnIgnore[j].Tag);
            bw.Write(UpgradeTagsRemovedOnIgnore[j].Amount);
        }
    }

    public void Read(BinaryReader br)
    {
        // Id = br.ReadString(); called in BinarySaveDataHelper.Create<Quest>()
        Type = br.ReadString();
        Dungeon = br.ReadString();
        Difficulty = br.ReadInt32();
        Length = br.ReadInt32();
        string goalId = br.ReadString();
        Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals[goalId];
        Reward = new CompletionReward();
        Reward.ResolveXP = br.ReadInt32();
        int questRewardGenCount = br.ReadInt32();
        Reward.ItemDefinitions.Clear();
        for (int j = 0; j < questRewardGenCount; j++)
        {
            Reward.ItemDefinitions.Add(new ItemDefinition()
            {
                Id = br.ReadString(),
                Type = br.ReadString(),
                Amount = br.ReadInt32(),
            });
        }

        IsProgression = br.ReadBoolean();
        HasStatueContents = br.ReadBoolean();
        CompletionDungeonXp = br.ReadBoolean();
        CanRetreat = br.ReadBoolean();
        AlwaysRetreatFromRaid = br.ReadBoolean();
        RetreatKillCount = br.ReadInt32();
        IsSurpriseEnabled = br.ReadBoolean();
        IsScoutingEnabled = br.ReadBoolean();
        IsStressClearedOnCompletion = br.ReadBoolean();
        RosterBuffOnFailureMinimumPartyResolveLevel = br.ReadInt32();

        int rostBuffsFailGenCount = br.ReadInt32();
        RosterBuffsOnFailure.Clear();
        for (int j = 0; j < rostBuffsFailGenCount; j++)
            RosterBuffsOnFailure.Add(DarkestDungeonManager.Data.Buffs[br.ReadString()]);

        int suggestedGenCount = br.ReadInt32();
        SuggestedTrinkets.Clear();
        for (int j = 0; j < suggestedGenCount; j++)
            SuggestedTrinkets.Add(new ItemDefinition("trinket", br.ReadString(), br.ReadInt32()));

        int tagGenCount = br.ReadInt32();
        UpgradeTagsRemovedOnIgnore.Clear();
        for (int j = 0; j < tagGenCount; j++)
            UpgradeTagsRemovedOnIgnore.Add(new UpgradeTag(br.ReadString(), br.ReadInt32()));
    }
}