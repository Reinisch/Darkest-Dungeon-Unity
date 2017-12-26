using System.Collections.Generic;
using System.Linq;

public class Blacksmith : Building
{
    public override string Name { get { return "blacksmith"; } }
    public override BuildingType Type { get { return BuildingType.Blacksmith; } }
    public List<DiscountUpgrade> DiscountUpgrades { get; private set; }
    public float Discount { get; private set; }

    private float baseDiscount = 0.0f;

    public Blacksmith()
    {
        DiscountUpgrades = new List<DiscountUpgrade>();
    }

    public override void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        base.InitializeBuilding(purchases);

        for (int i = DiscountUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[DiscountUpgrades[i].TreeId].PurchasedUpgrades.Contains(DiscountUpgrades[i].UpgradeCode))
            {
                Discount += DiscountUpgrades[i].Percent;
            }
        }
    }

    public override void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        base.UpdateBuilding(purchases);

        for (int i = DiscountUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[DiscountUpgrades[i].TreeId].PurchasedUpgrades.Contains(DiscountUpgrades[i].UpgradeCode))
            {
                Discount += DiscountUpgrades[i].Percent;
            }
        }
    }

    public override List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        return DiscountUpgrades.FindAll(item => item.UpgradeCode == code).Cast<ITownUpgrade>().ToList();
    }

    protected override void Reset()
    {
        Discount = baseDiscount;
    }
}
