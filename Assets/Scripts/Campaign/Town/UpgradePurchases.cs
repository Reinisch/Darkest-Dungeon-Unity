using System.Collections.Generic;
using System.IO;

public class UpgradePurchases : IBinarySaveData
{
    public string TreeId { get; private set; }
    public List<string> PurchasedUpgrades { get; private set; }

    public bool IsMeetingSaveCriteria { get { return true; } }

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
        PurchasedUpgrades = new List<string> {code};
    }

    public UpgradePurchases(string treeId)
    {
        TreeId = treeId;
        PurchasedUpgrades = new List<string>();
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(TreeId);
        PurchasedUpgrades.Write(bw);
    }

    public void Read(BinaryReader br)
    {
        TreeId = br.ReadString();
        PurchasedUpgrades.Read(br);
    }
}
