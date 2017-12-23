using System.Collections.Generic;

public class QuestGenerationData
{
    public int MaxQuestsPerDungeon { get; set; }
    public List<int> QuestsPerVisit { get; set; }
    public List<GeneratedResolveDifficulty> Difficulties { get; set; }
    public Dictionary<string, GeneratedDungeon> Dungeons { get; set; }
    public Dictionary<string, GeneratedDungeonQuestTypes> QuestTypes { get; set; }
    public Dictionary<string, List<string>> HeirloomTypes { get; set; }
    public Dictionary<string, int[][]> HeirloomAmounts { get; set; }
    public ItemDefinition[][][] ItemTable { get; set; }
    public int[][] ResolveXpReward { get; set; }
    public Dictionary<string, int[][]> TrinketChances { get; set; }
    public List<int> ResolveThreshold { get; set; }

    public QuestGenerationData()
    {
        QuestsPerVisit = new List<int>();
        Dungeons = new Dictionary<string, GeneratedDungeon>();
        Difficulties = new List<GeneratedResolveDifficulty>();
        QuestTypes = new Dictionary<string, GeneratedDungeonQuestTypes>();
        HeirloomTypes = new Dictionary<string, List<string>>();
        HeirloomAmounts = new Dictionary<string, int[][]>();
        ResolveThreshold = new List<int>();
        TrinketChances = new Dictionary<string, int[][]>();
    }
}

public class GeneratedDungeonQuestTypes
{
    public string Dungeon { get; set; }
    public List<List<GeneratedQuestType>> QuestTypeSets { get; set; }

    public GeneratedDungeonQuestTypes()
    {
        QuestTypeSets = new List<List<GeneratedQuestType>>();
    }
}

public class GeneratedQuestType : IProportionValue
{
    public string Type { get; set; }
    public int Chance { get; set; }
    public int Length { get; set; }
}


public class GeneratedResolveDifficulty
{
    public List<int> ResolveLevels { get; set; }
    public int Difficulty { get; set; }

    public GeneratedResolveDifficulty()
    {
        ResolveLevels = new List<int>();
    }
}

public class GeneratedDungeon
{
    public string Id { get; set; }
    public int RequiredQuestsCompleted { get; set; }
}