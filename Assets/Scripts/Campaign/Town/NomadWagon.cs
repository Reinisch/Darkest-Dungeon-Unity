using System.Collections.Generic;
using System.Linq;

public class NomadWagon : Building
{
    public override string Name { get { return "nomad_wagon"; } }
    public override BuildingType Type { get { return BuildingType.NomadWagon; } }
    public float Discount { get; private set; }
    public int BaseTrinketSlots { get; set; }
    public int TrinketSlots { private get; set; }

    public List<SlotUpgrade> TrinketSlotUpgrades { get; private set; }
    public List<DiscountUpgrade> DiscountUpgrades { get; private set; }
    public List<Trinket> Trinkets { get; private set; }

    private List<GeneratedRarity> RarityTable { get; set; }

    public NomadWagon()
    {
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

    public void RestockTrinkets()
    {
        Trinkets.Clear();
        var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.Cast<Trinket>().ToList();
        for(int i = 0; i < TrinketSlots; i++)
        {
            var rarity = RandomSolver.ChooseByRandom(RarityTable).RarityId;
            var rarityList = trinketList.FindAll(item => item.RarityId == rarity);
            Trinkets.Add(rarityList[UnityEngine.Random.Range(0, rarityList.Count)]);
        }
        Trinkets.Sort((x, y) => y.PurchasePrice.CompareTo(x.PurchasePrice));
    }

    public Trinket GenerateTrinket()
    {
        var trinketList = DarkestDungeonManager.Data.Items["trinket"].Values.Cast<Trinket>().ToList();
        var rarity = RandomSolver.ChooseByRandom(RarityTable).RarityId;
        var rarityList = trinketList.FindAll(item => item.RarityId == rarity);
        return rarityList[UnityEngine.Random.Range(0, rarityList.Count)];
    }

    public override void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        base.InitializeBuilding(purchases);

        Trinkets.Clear();

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

        for (int i = TrinketSlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[TrinketSlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(TrinketSlotUpgrades[i].UpgradeCode))
            {
                TrinketSlots = TrinketSlotUpgrades[i].NumberOfSlots;
                break;
            }
        }
    }

    public override List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        List<ITownUpgrade> townUpgrades = new List<ITownUpgrade>();
        townUpgrades.AddRange(DiscountUpgrades.FindAll(item => item.UpgradeCode == code && item.TreeId == treeId).Cast<ITownUpgrade>());
        townUpgrades.AddRange(TrinketSlotUpgrades.FindAll(item => item.UpgradeCode == code && item.TreeId == treeId).Cast<ITownUpgrade>());
        return townUpgrades;
    }

    protected override void Reset()
    {
        Discount = 0.0f;
        TrinketSlots = BaseTrinketSlots;
    }
}