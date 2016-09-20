using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampingTrainer : Building
{
    public float BaseDiscount { get; set; }
    public float Discount { get; set; }

    public List<DiscountUpgrade> DiscountUpgrades { get; set; }

    public CampingTrainer()
    {
        DiscountUpgrades = new List<DiscountUpgrade>();
    }

    public void Reset()
    {
        Discount = BaseDiscount;
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
    }

    public ITownUpgrade GetUpgradeByCode(string code)
    {
        return DiscountUpgrades.Find(item => item.UpgradeCode == code);
    }
}