using System.Collections.Generic;

public class UpgradeTree
{
    public string Id { get; set; }
    public bool IsInstanced { get; set; }
    public List<string> Tags { get; set; }
    public List<TownUpgrade> Upgrades { get; set; }

    public UpgradeTree()
    {
        Tags = new List<string>();
        Upgrades = new List<TownUpgrade>();
    }
}