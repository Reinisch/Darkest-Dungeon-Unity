using System.Collections.Generic;

public class Guild : Building
{
    public float BaseDiscount { get; set; }
    public float Discount { get; set; }
    public int SkillRank { get; set; }

    public List<DiscountUpgrade> DiscountUpgrades { get; set; }

    public Guild()
    {
        DiscountUpgrades = new List<DiscountUpgrade>();
        SkillRank = 1;
    }

    public void Reset()
    {
        Discount = BaseDiscount;
        SkillRank = 1;
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

        SkillRank = purchases["guild.skill_levels"].PurchasedUpgrades.Count + 1;
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

        SkillRank = purchases["guild.skill_levels"].PurchasedUpgrades.Count + 1;
    }

    public ITownUpgrade GetUpgradeByCode(string code)
    {
        return DiscountUpgrades.Find(item => item.UpgradeCode == code);
    }
}
