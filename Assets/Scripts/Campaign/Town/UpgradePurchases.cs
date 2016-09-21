using System.Collections.Generic;

public class UpgradePurchases
{
    public string TreeId { get; set; }
    public List<string> PurchasedUpgrades { get; set; }

    public UpgradePurchases()
    {
        PurchasedUpgrades = new List<string>();
    }

    public UpgradePurchases(string treeId, IEnumerable<string> purchases)
    {
        TreeId = treeId;
        PurchasedUpgrades = new List<string>(purchases);
    }

    public UpgradePurchases(string treeId, string code)
    {
        TreeId = treeId;
        PurchasedUpgrades = new List<string>();
        PurchasedUpgrades.Add(code);
    }

    public UpgradePurchases(string treeId)
    {
        TreeId = treeId;
        PurchasedUpgrades = new List<string>();
    }
}
