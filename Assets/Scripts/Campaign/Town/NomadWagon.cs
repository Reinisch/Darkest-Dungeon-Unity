using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class NomadWagon : Building
{
    public float TrinketSellValue { get; set; }

    public float BaseDiscount { get; set; }
    public float Discount { get; set; }

    public int BaseTrinketSlots { get; set; }
    public int TrinketSlots { get; set; }

    public List<SlotUpgrade> TrinketSlotUpgrades { get; set; }
    public List<DiscountUpgrade> DiscountUpgrades { get; set; }
    public List<GeneratedRarity> RarityTable { get; set; }

    public List<Trinket> Trinkets { get; set; }

    public NomadWagon()
    {
        TrinketSellValue = 0.85f;
        TrinketSlotUpgrades = new List<SlotUpgrade>();
        DiscountUpgrades = new List<DiscountUpgrade>();
        Trinkets = new List<Trinket>();
        RarityTable = new List<GeneratedRarity>()
        {
            new GeneratedRarity()
            {
                 Chance = 1,
                 RarityId = "very_common",
            },
            new GeneratedRarity()
            {
                 Chance = 1,
                 RarityId = "common",
            },
            new GeneratedRarity()
            {
                 Chance = 1,
                 RarityId = "uncommon",
            },
            new GeneratedRarity()
            {
                 Chance = 1,
                 RarityId = "rare",
            },
            new GeneratedRarity()
            {
                 Chance = 1,
                 RarityId = "very_rare",
            },
        };
    }

    public void Reset()
    {
        Discount = BaseDiscount;
        TrinketSlots = BaseTrinketSlots;
    }

    public void RestockTrinkets()
    {
        Trinkets = new List<Trinket>();
        var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.Cast<Trinket>().ToList();
        for(int i = 0; i < TrinketSlots; i++)
        {
            var rarity = RandomSolver.ChooseByRandom<GeneratedRarity>(RarityTable).RarityId;
            var rarityList = trinketList.FindAll(item => item.Rarity == rarity);
            Trinkets.Add(rarityList[UnityEngine.Random.Range(0, rarityList.Count)]);
        }
        Trinkets.Sort((x, y) => y.PurchasePrice.CompareTo(x.PurchasePrice));
    }

    public Trinket GenerateTrinket()
    {
        var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.Cast<Trinket>().ToList();
        var rarity = RandomSolver.ChooseByRandom<GeneratedRarity>(RarityTable).RarityId;
        var rarityList = trinketList.FindAll(item => item.Rarity == rarity);
        return rarityList[UnityEngine.Random.Range(0, rarityList.Count)];
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

        for (int i = TrinketSlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[TrinketSlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(TrinketSlotUpgrades[i].UpgradeCode))
            {
                TrinketSlots = TrinketSlotUpgrades[i].NumberOfSlots;
                break;
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

        for (int i = TrinketSlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[TrinketSlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(TrinketSlotUpgrades[i].UpgradeCode))
            {
                TrinketSlots = TrinketSlotUpgrades[i].NumberOfSlots;
                break;
            }
        }
    }

    public ITownUpgrade GetUpgradeByCode(string treeId, string code)
    {
        ITownUpgrade upgrade = DiscountUpgrades.Find(item => item.UpgradeCode == code && item.TreeId == treeId);
        if (upgrade == null)
            return TrinketSlotUpgrades.Find(item => item.UpgradeCode == code && item.TreeId == treeId);
        return upgrade;
    }
}

public class GeneratedRarity : IProportionValue
{
    public string RarityId { get; set; }
    public int Chance { get; set; }
}