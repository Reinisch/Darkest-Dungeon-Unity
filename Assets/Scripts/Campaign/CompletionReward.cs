using System.Collections.Generic;

public class CompletionReward
{
    public int ResolveXP { get; set; }

    public List<ItemDefinition> ItemDefinitions { get; set; }

    public CompletionReward()
    {
        ItemDefinitions = new List<ItemDefinition>();
    }
}
