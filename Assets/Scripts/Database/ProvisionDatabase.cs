using UnityEngine;
using System.Collections.Generic;

public class ProvisionDatabase
{
    public List<List<ItemDefinition>> StartingLengthInventories { get; set; }
    public Dictionary<string, List<ItemDefinition>> HeroClassItemList { get; set; }
    public List<List<ItemDefinition>> ShopLengthInventories { get; set; }

    public ProvisionDatabase()
    {
        StartingLengthInventories = new List<List<ItemDefinition>>();
        HeroClassItemList = new Dictionary<string, List<ItemDefinition>>();
        ShopLengthInventories = new List<List<ItemDefinition>>();
    }
}
