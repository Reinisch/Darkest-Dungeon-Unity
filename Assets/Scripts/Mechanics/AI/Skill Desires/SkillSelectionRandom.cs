using System.Collections.Generic;

public sealed class SkillSelectionRandom : SkillSelectionDesire
{
    public SkillSelectionRandom(Dictionary<string, object> dataSet)
    {
        GenerateFromDataSet(dataSet);
    }
}