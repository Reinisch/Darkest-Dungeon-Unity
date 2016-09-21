using System.Collections.Generic;

public class Blacksmith : Building
{
    public float BaseDiscount { get; set; }
    public float Discount { get; set; }
    public int WeaponRank { get; set; }
    public int ArmorRank { get; set; }

    public List<DiscountUpgrade> DiscountUpgrades { get; set; }

    public Blacksmith()
    {
        DiscountUpgrades = new List<DiscountUpgrade>();
        WeaponRank = 1;
        ArmorRank = 1;
    }

    public void Reset()
    {
        Discount = BaseDiscount;
        WeaponRank = 1;
        ArmorRank = 1;
    }

    public void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        Reset();

        for (int i = DiscountUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[DiscountUpgrades[i].TreeId].PurchasedUpgrades.Contains(DiscountUpgrades[i].UpgradeCode))
            {
                Discount += DiscountUpgrades[i].Percent;
            }
        }

        WeaponRank = purchases["blacksmith.weapon"].PurchasedUpgrades.Count + 1;
        ArmorRank = purchases["blacksmith.armour"].PurchasedUpgrades.Count + 1;
    }

    public void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        Reset();

        for (int i = DiscountUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[DiscountUpgrades[i].TreeId].PurchasedUpgrades.Contains(DiscountUpgrades[i].UpgradeCode))
            {
                Discount += DiscountUpgrades[i].Percent;
            }
        }

        WeaponRank = purchases["blacksmith.weapon"].PurchasedUpgrades.Count + 1;
        ArmorRank = purchases["blacksmith.armour"].PurchasedUpgrades.Count + 1;
    }

    public ITownUpgrade GetUpgradeByCode(string code)
    {
        return DiscountUpgrades.Find(item => item.UpgradeCode == code);
    }
}
