using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class QuestGenerator
{
    private class DungeonQuestInfo
    {
        public string Dungeon { get; set; }
        public List<Quest> Quests { get; private set; }
        public List<GeneratedQuestType> GeneratedTypes { get; set; }

        public DungeonQuestInfo()
        {
            Quests = new List<Quest>();
            GeneratedTypes = new List<GeneratedQuestType>();
        }
    }

    private class QuestGenerationInfo
    {
        public int DungeonCount;
        public int QuestCount;
        public int MaxPerDungeon;

        public readonly List<DungeonQuestInfo> DungeonQuests;

        public QuestGenerationInfo()
        {
            DungeonQuests = new List<DungeonQuestInfo>();
        }
    }

    public static List<Quest> GenerateQuests(Campaign campaign, int seed = 0)
    {
        if (seed != 0)
            Random.InitState(seed);

        QuestDatabase questData = DarkestDungeonManager.Data.QuestDatabase;
        QuestGenerationData genData = questData.QuestGeneration;

        QuestGenerationInfo questGenerationInfo = GetQuestInfo(genData, campaign);

        DistributeQuests(questGenerationInfo, genData, campaign);
        DistributeQuestTypes(questGenerationInfo);
        DistributeQuestGoals(questGenerationInfo, questData);
        DistributeQuestRewards(questGenerationInfo, questData);

        List<Quest> generatedQuests = new List<Quest>();
        foreach (var info in questGenerationInfo.DungeonQuests)
            foreach (var quest in info.Quests)
                generatedQuests.Add(quest);

        return generatedQuests;
    }

    private static QuestGenerationInfo GetQuestInfo(QuestGenerationData genData, Campaign campaign)
    {
        QuestGenerationInfo questInfo = new QuestGenerationInfo();
        foreach (var dungeon in genData.Dungeons.Values)
        {
            if (dungeon.RequiredQuestsCompleted <= campaign.QuestsComleted)
            {
                DungeonQuestInfo dungeonInfo = new DungeonQuestInfo();
                dungeonInfo.Dungeon = dungeon.Id;
                dungeonInfo.GeneratedTypes = genData.QuestTypes[dungeonInfo.Dungeon].
                    QuestTypeSets[campaign.Dungeons[dungeon.Id].MasteryLevel];
                questInfo.DungeonQuests.Add(dungeonInfo);
            }
        }
        questInfo.DungeonCount = questInfo.DungeonQuests.Count;
        questInfo.MaxPerDungeon = genData.MaxQuestsPerDungeon;
        questInfo.QuestCount = GetQuestNumber(genData, campaign);
        return questInfo;
    }

    private static int GetQuestNumber(QuestGenerationData genData, Campaign campaign)
    {
        int questNumberState;
        if (campaign.QuestsComleted <= 2)
            questNumberState = 0;
        else if (campaign.QuestsComleted <= 3)
            questNumberState = 1;
        else if (campaign.QuestsComleted <= 4)
            questNumberState = 2;
        else if (campaign.QuestsComleted <= 6)
            questNumberState = 3;
        else if (campaign.QuestsComleted <= 10)
            questNumberState = 4;
        else if (campaign.QuestsComleted <= 16)
            questNumberState = 5;
        else if (campaign.QuestsComleted <= 20)
            questNumberState = 6;
        else
            questNumberState = 7;
        return genData.QuestsPerVisit[questNumberState];
    }

    private static void DistributeQuests(QuestGenerationInfo questInfo, QuestGenerationData genData, Campaign campaign)
    {
        if (questInfo.DungeonCount * questInfo.MaxPerDungeon < questInfo.QuestCount)
            questInfo.QuestCount = questInfo.DungeonCount * questInfo.MaxPerDungeon;
        if (questInfo.DungeonCount > questInfo.QuestCount)
            questInfo.QuestCount = questInfo.DungeonCount;

        int questsLeft = questInfo.QuestCount;

        float difOneAvailable = campaign.Heroes.FindAll(hero => 
            genData.Difficulties[0].ResolveLevels.Contains(hero.Resolve.Level)).Count;
        float difTwoAvailable = campaign.Heroes.FindAll(hero => 
            genData.Difficulties[1].ResolveLevels.Contains(hero.Resolve.Level)).Count;
        float difThreeAvailable = campaign.Heroes.FindAll(hero =>
            genData.Difficulties[2].ResolveLevels.Contains(hero.Resolve.Level)).Count;
        float allAvailable = difOneAvailable + difTwoAvailable + difThreeAvailable;
        if (allAvailable == 0)
        {
            difOneAvailable = 4;
            allAvailable = 4;
        }

        List<int> difficulties = new List<int>();
        int difOnes = Mathf.RoundToInt(difOneAvailable / allAvailable * questsLeft);
        int difTwos = Mathf.RoundToInt(difTwoAvailable / allAvailable * questsLeft);
        int difThrees = Mathf.RoundToInt(difThreeAvailable / allAvailable * questsLeft);

        difOnes += questsLeft - (difOnes + difTwos + difThrees);

        for (int i = 0; i < difOnes; i++)
            difficulties.Add(1);
        for (int i = 0; i < difTwos; i++)
            difficulties.Add(3);
        for (int i = 0; i < difThrees; i++)
            difficulties.Add(5);

        for (int i = 0; i < questInfo.DungeonQuests.Count; i++)
        {
            Quest quest = new Quest();
            int difIndex = Random.Range(0, difficulties.Count);
            quest.Dungeon = questInfo.DungeonQuests[i].Dungeon;
            quest.Difficulty = difficulties[difIndex];
            difficulties.RemoveAt(difIndex);
            questInfo.DungeonQuests[i].Quests.Add(quest);
            questsLeft--;

            if (questsLeft == 0)
                break;
        }

        while (questsLeft > 0)
        {
            foreach (var dungeonInfo in questInfo.DungeonQuests)
            {
                if (questsLeft == 0)
                    break;

                if (dungeonInfo.Quests.Count < questInfo.MaxPerDungeon)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        Quest quest = new Quest();
                        int difIndex = Random.Range(0, difficulties.Count);
                        quest.Dungeon = dungeonInfo.Dungeon;
                        quest.Difficulty = difficulties[difIndex];
                        difficulties.RemoveAt(difIndex);
                        dungeonInfo.Quests.Add(quest);
                        questsLeft--;
                    }
                }
            }
        }

        foreach (var dungeon in campaign.Dungeons)
        {
            var plotMastery = dungeon.Value.CurrentPlotQuest;
            if (plotMastery != null)
            {
                DungeonQuestInfo dungeonQuest = questInfo.DungeonQuests.Find(item => item.Dungeon == dungeon.Value.DungeonName);
                if (dungeonQuest != null)
                {
                    dungeonQuest.Quests.Add(plotMastery.Copy());
                }
                else
                {
                    dungeonQuest = new DungeonQuestInfo() { Dungeon = plotMastery.Dungeon };
                    dungeonQuest.Quests.Add(plotMastery.Copy());
                    questInfo.DungeonQuests.Add(dungeonQuest);
                }
            }
        }

        foreach (var townEventData in campaign.EventModifiers.EventData)
        {
            if (townEventData.Type == TownEventDataType.PlotQuest)
            {
                var plotQuest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.
                    Find(quest => quest.Id == townEventData.StringData);

                if(plotQuest != null && !campaign.CompletedPlot.Contains(plotQuest.Id))
                {
                    DungeonQuestInfo dungeonQuest = questInfo.DungeonQuests.Find(item => item.Dungeon == plotQuest.Dungeon);
                    if (dungeonQuest != null)
                    {
                        dungeonQuest.Quests.Add(plotQuest.Copy());
                    }
                    else
                    {
                        dungeonQuest = new DungeonQuestInfo() { Dungeon = plotQuest.Dungeon };
                        dungeonQuest.Quests.Add(plotQuest.Copy());
                        questInfo.DungeonQuests.Add(dungeonQuest);
                    }
                }
            }
        }
    }

    private static void DistributeQuestTypes(QuestGenerationInfo questInfo)
    {
        foreach(var dungeonInfo in questInfo.DungeonQuests)
        {
            foreach(var quest in dungeonInfo.Quests)
            {
                if (quest.IsPlotQuest)
                    continue;
                GeneratedQuestType questType = RandomSolver.ChooseByRandom(dungeonInfo.GeneratedTypes);
                quest.Type = questType.Type;
                quest.Length = questType.Length;
            }
        }
    }

    private static void DistributeQuestGoals(QuestGenerationInfo questInfo, QuestDatabase questData)
    {
        List<string> availableGoals = new List<string>();

        foreach (var dungeonInfo in questInfo.DungeonQuests)
        {
            foreach (var quest in dungeonInfo.Quests)
            {
                if (quest.IsPlotQuest)
                    continue;

                QuestType questType = questData.QuestTypes[quest.Type];

                foreach(var goalList in questType.GoalLists)
                {
                    if(goalList.Dungeon == "all")
                        availableGoals.AddRange(goalList.Goals);
                    else if (goalList.Dungeon == quest.Dungeon)
                        availableGoals.AddRange(goalList.Goals);
                }
                if (availableGoals.Count > 0)
                    quest.Goal = questData.QuestGoals[availableGoals[Random.Range(0, availableGoals.Count)]];
                else
                    Debug.LogError("No goal for quest " + quest.Dungeon + " " + quest.Difficulty);
                availableGoals.Clear();
            }
        }
    }

    private static void DistributeQuestRewards(QuestGenerationInfo questInfo, QuestDatabase questData)
    {
        var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.Cast<Trinket>().ToList();

        foreach (var dungeonInfo in questInfo.DungeonQuests)
        {
            foreach (var quest in dungeonInfo.Quests)
            {
                if (quest.IsPlotQuest)
                {
                    var plotQuest = (PlotQuest)quest;
                    if(plotQuest.PlotTrinket != null)
                    {
                        var rarityTrinketList = trinketList.FindAll(item => item.RarityId == plotQuest.PlotTrinket.Rarity);
                        ItemDefinition trinket = new ItemDefinition();
                        trinket.Type = "trinket";
                        trinket.Amount = 1;
                        trinket.Id = rarityTrinketList[Random.Range(0, rarityTrinketList.Count)].Id;
                        plotQuest.Reward.ItemDefinitions.Add(trinket);
                    }
                    continue;
                }

                CompletionReward reward = new CompletionReward();
                quest.Reward = reward;
                reward.ResolveXP = questData.QuestGeneration.ResolveXpReward[quest.Difficulty][quest.Length];
                ItemDefinition heirloomOne = new ItemDefinition();
                ItemDefinition heirloomTwo = new ItemDefinition();

                heirloomOne.Type = "heirloom";
                heirloomOne.Id = questData.QuestGeneration.HeirloomTypes[quest.Dungeon][0];
                heirloomOne.Amount = questData.QuestGeneration.HeirloomAmounts[heirloomOne.Id][quest.Difficulty][quest.Length];
                reward.ItemDefinitions.Add(heirloomOne);

                heirloomTwo.Type = "heirloom";
                heirloomTwo.Id = questData.QuestGeneration.HeirloomTypes[quest.Dungeon][1];
                heirloomTwo.Amount = questData.QuestGeneration.HeirloomAmounts[heirloomOne.Id][quest.Difficulty][quest.Length];
                reward.ItemDefinitions.Add(heirloomTwo);
                reward.ItemDefinitions.Add(questData.QuestGeneration.ItemTable[quest.Difficulty][quest.Length][0]);

                foreach (var trinketInfo in questData.QuestGeneration.TrinketChances)
                {
                    if (trinketInfo.Value[quest.Difficulty][quest.Length] != 1)
                        continue;

                    var rarityTrinketList = trinketList.FindAll(item => item.RarityId == trinketInfo.Key);
                    ItemDefinition trinket = new ItemDefinition();
                    trinket.Type = "trinket";
                    trinket.Amount = 1;
                    trinket.Id = rarityTrinketList[Random.Range(0, rarityTrinketList.Count)].Id;
                    reward.ItemDefinitions.Add(trinket);
                    break;
                }
            }
        }
    }
}