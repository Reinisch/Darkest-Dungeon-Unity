using System.Collections.Generic;

public class ProvisionDatabase
{
    public List<List<ItemDefinition>> StartingLengthInventories { get; private set; }
    public Dictionary<string, List<ItemDefinition>> HeroClassItemList { get; private set; }
    public List<List<ItemDefinition>> ShopLengthInventories { get; private set; }

    public ProvisionDatabase()
    {
        StartingLengthInventories = new List<List<ItemDefinition>>();
        HeroClassItemList = new Dictionary<string, List<ItemDefinition>>();
        ShopLengthInventories = new List<List<ItemDefinition>>();
    }
}