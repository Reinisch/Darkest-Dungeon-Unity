using UnityEngine;
using System.Collections.Generic;

public class QuirkTreatmentActivity
{
    public string Id { get; set; }

    public float QuirkTreatmentChance { get; set; }
    public CurrencyCost BasePositiveQuirkCost { get; set; }
    public CurrencyCost BaseNegativeQuirkCost { get; set; }
    public CurrencyCost BasePermNegativeCost { get; set; }
    public int BaseQuirkSlots { get; set; }
    public CurrencyCost PositiveQuirkCost { get; set; }
    public CurrencyCost NegativeQuirkCost { get; set; }
    public CurrencyCost PermNegativeQuirkCost { get; set; }
    public int QuirkSlots { get; set; }

    public List<CostUpgrade> PositiveQuirkUpgrades { get; set; }
    public List<CostUpgrade> NegativeQuirkUpgrades { get; set; }
    public List<CostUpgrade> PermNegativeUpgrades { get; set; }
    public List<SlotUpgrade> SlotUpgrades { get; set; }

    public List<TreatmentSlot> TreatmentSlots { get; set; }

    public QuirkTreatmentActivity()
    {
        PositiveQuirkUpgrades = new List<CostUpgrade>();
        NegativeQuirkUpgrades = new List<CostUpgrade>();
        PermNegativeUpgrades = new List<CostUpgrade>();
        SlotUpgrades = new List<SlotUpgrade>();

        TreatmentSlots = new List<TreatmentSlot>();
        Id = "treatment";
    }

    public void Reset()
    {
        PositiveQuirkCost = BasePositiveQuirkCost;
        NegativeQuirkCost = BaseNegativeQuirkCost;
        PermNegativeQuirkCost = BasePermNegativeCost;
        QuirkSlots = BaseQuirkSlots;

        TreatmentSlots = new List<TreatmentSlot>();
    }

    public void InitializeActivity(Dictionary<string, UpgradePurchases> purchases)
    {
        Reset();

        for (int i = PositiveQuirkUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[PositiveQuirkUpgrades[i].TreeId].PurchasedUpgrades.Contains(PositiveQuirkUpgrades[i].UpgradeCode))
            {
                PositiveQuirkCost = PositiveQuirkUpgrades[i].Cost;
                break;
            }
        }
        for (int i = NegativeQuirkUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[NegativeQuirkUpgrades[i].TreeId].PurchasedUpgrades.Contains(NegativeQuirkUpgrades[i].UpgradeCode))
            {
                NegativeQuirkCost = NegativeQuirkUpgrades[i].Cost;
                break;
            }
        }

        for (int i = PermNegativeUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[PermNegativeUpgrades[i].TreeId].PurchasedUpgrades.Contains(PermNegativeUpgrades[i].UpgradeCode))
            {
                PermNegativeQuirkCost = PermNegativeUpgrades[i].Cost;
                break;
            }
        }

        for (int i = SlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[SlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(SlotUpgrades[i].UpgradeCode))
            {
                QuirkSlots = SlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        bool isActivityFree = DarkestDungeonManager.Campaign.EventModifiers.IsActivityFree(Id);
        float costModifier = DarkestDungeonManager.Campaign.EventModifiers.ActivityCostModifier(Id);

        for (int i = 1; i <= 3; i++)
        {
            if (i <= QuirkSlots)
            {
                if (isActivityFree)
                    TreatmentSlots.Add(new TreatmentSlot(true, 0, 0, 0, 0));
                else
                    TreatmentSlots.Add(new TreatmentSlot(true, (int) (NegativeQuirkCost.Amount * costModifier),
                        (int)(PositiveQuirkCost.Amount * costModifier), (int)(PermNegativeQuirkCost.Amount * costModifier), 0));
            }
            else
                TreatmentSlots.Add(new TreatmentSlot(false, (int)(NegativeQuirkCost.Amount * costModifier),
                        (int)(PositiveQuirkCost.Amount * costModifier), (int)(PermNegativeQuirkCost.Amount * costModifier), 0));
        }
    }

    public void ProvideActivity()
    {
        foreach (var treatmentSlot in TreatmentSlots)
        {
            switch (treatmentSlot.Status)
            {
                case ActivitySlotStatus.Caretaken:
                    treatmentSlot.Status = ActivitySlotStatus.Available;
                    treatmentSlot.Hero = null;
                    break;
                case ActivitySlotStatus.Blocked:
                case ActivitySlotStatus.Checkout:
                    treatmentSlot.Hero.Status = HeroStatus.Available;
                    treatmentSlot.Status = ActivitySlotStatus.Available;
                    treatmentSlot.Hero = null;
                    break;
                case ActivitySlotStatus.Paid:
                    if(treatmentSlot.TargetNegativeQuirk != null)
                    {
                        if(treatmentSlot.Hero.RemoveQuirk(treatmentSlot.TargetNegativeQuirk) == null)
                            Debug.LogError("Sanitarium treament negative quirk not found.");
                    }
                    if(treatmentSlot.TargetPositiveQuirk != null)
                    {
                        if (!treatmentSlot.Hero.LockQuirk(treatmentSlot.TargetPositiveQuirk))
                            Debug.LogError("Sanitarium treatment positive quirk not found.");
                    }

                    if (treatmentSlot.TargetNegativeQuirk != null && treatmentSlot.TargetPositiveQuirk != null)
                        LogActivity(ActivityType.LockRemoveQuirk, treatmentSlot.Hero,
                            treatmentSlot.TargetNegativeQuirk, treatmentSlot.TargetPositiveQuirk);
                    else if (treatmentSlot.TargetNegativeQuirk != null)
                        LogActivity(ActivityType.RemoveQuirk, treatmentSlot.Hero, treatmentSlot.TargetNegativeQuirk);
                    else if (treatmentSlot.TargetPositiveQuirk != null)
                        LogActivity(ActivityType.LockQuirk, treatmentSlot.Hero, treatmentSlot.TargetPositiveQuirk);

                    treatmentSlot.Hero.Status = HeroStatus.Available;
                    treatmentSlot.Status = ActivitySlotStatus.Available;
                    treatmentSlot.Hero = null;
                    break;
            }
        }
    }

    public void UpdateActivity(Dictionary<string, UpgradePurchases> purchases)
    {
        for (int i = PositiveQuirkUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[PositiveQuirkUpgrades[i].TreeId].PurchasedUpgrades.Contains(PositiveQuirkUpgrades[i].UpgradeCode))
            {
                PositiveQuirkCost = PositiveQuirkUpgrades[i].Cost;
                break;
            }
        }
        for (int i = NegativeQuirkUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[NegativeQuirkUpgrades[i].TreeId].PurchasedUpgrades.Contains(NegativeQuirkUpgrades[i].UpgradeCode))
            {
                NegativeQuirkCost = NegativeQuirkUpgrades[i].Cost;
                break;
            }
        }

        for (int i = PermNegativeUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[PermNegativeUpgrades[i].TreeId].PurchasedUpgrades.Contains(PermNegativeUpgrades[i].UpgradeCode))
            {
                PermNegativeQuirkCost = PermNegativeUpgrades[i].Cost;
                break;
            }
        }

        for (int i = SlotUpgrades.Count - 1; i >= 0; i--)
        {
            if (purchases[SlotUpgrades[i].TreeId].PurchasedUpgrades.Contains(SlotUpgrades[i].UpgradeCode))
            {
                QuirkSlots = SlotUpgrades[i].NumberOfSlots;
                break;
            }
        }

        bool isActivityFree = DarkestDungeonManager.Campaign.EventModifiers.IsActivityFree(Id);
        float costModifier = DarkestDungeonManager.Campaign.EventModifiers.ActivityCostModifier(Id);

        for (int i = 1; i <= 3; i++)
        {
            if (i <= QuirkSlots)
            {
                if(isActivityFree)
                    TreatmentSlots[i - 1].UpdateTreatmentSlot(true, 0, 0, 0, 0);
                else
                    TreatmentSlots[i - 1].UpdateTreatmentSlot(true, (int)(NegativeQuirkCost.Amount * costModifier),
                        (int)(PositiveQuirkCost.Amount * costModifier), (int)(PermNegativeQuirkCost.Amount * costModifier), 0);
            }
            else
                TreatmentSlots[i - 1].UpdateTreatmentSlot(false, (int)(NegativeQuirkCost.Amount * costModifier),
                        (int)(PositiveQuirkCost.Amount * costModifier), (int)(PermNegativeQuirkCost.Amount * costModifier), 0);
        }
    }

    public void UpdateActivitySlots(SaveCampaignData saveData)
    {
        bool isActivityFree = DarkestDungeonManager.Campaign.EventModifiers.IsActivityFree(Id);
        float costModifier = DarkestDungeonManager.Campaign.EventModifiers.ActivityCostModifier(Id);

        for (int i = 0; i < 3; i++)
        {
            if (i + 1 <= QuirkSlots)
            {
                if (saveData.SanitariumActivitySlots.Count > 0 && saveData.SanitariumActivitySlots[0].Count >= i)
                {
                    var activityHero = DarkestDungeonManager.Campaign.Heroes.Find(hero =>
                        hero.RosterId == saveData.SanitariumActivitySlots[0][i].HeroRosterId);

                    TreatmentSlots[i].Hero = activityHero;
                    if (activityHero != null)
                        TreatmentSlots[i].Status = saveData.SanitariumActivitySlots[0][i].Status;
                    else
                        TreatmentSlots[i].Status = ActivitySlotStatus.Available;

                    TreatmentSlots[i].TargetDiseaseQuirk = saveData.SanitariumActivitySlots[0][i].TargetDiseaseQuirk;
                    TreatmentSlots[i].TargetPositiveQuirk = saveData.SanitariumActivitySlots[0][i].TargetPositiveQuirk;
                    TreatmentSlots[i].TargetNegativeQuirk = saveData.SanitariumActivitySlots[0][i].TargetNegativeQuirk;
                }

                if (isActivityFree)
                    TreatmentSlots[i].UpdateTreatmentSlot(true, 0, 0, 0, 0);
                else
                    TreatmentSlots[i].UpdateTreatmentSlot(true, (int)(NegativeQuirkCost.Amount * costModifier),
                        (int)(PositiveQuirkCost.Amount * costModifier), (int)(PermNegativeQuirkCost.Amount * costModifier), 0);
            }
            else
                TreatmentSlots[i].UpdateTreatmentSlot(false, (int)(NegativeQuirkCost.Amount * costModifier),
                       (int)(PositiveQuirkCost.Amount * costModifier), (int)(PermNegativeQuirkCost.Amount * costModifier), 0);
        }
    }

    public List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        List<ITownUpgrade> foundUpgrades = new List<ITownUpgrade>();
        ITownUpgrade upgrade = PositiveQuirkUpgrades.Find(item => item.TreeId == treeId && item.UpgradeCode == code);
        if (upgrade != null)
            foundUpgrades.Add(upgrade);
        upgrade = NegativeQuirkUpgrades.Find(item => item.TreeId == treeId && item.UpgradeCode == code);
        if (upgrade != null)
            foundUpgrades.Add(upgrade);
        upgrade = PermNegativeUpgrades.Find(item => item.TreeId == treeId && item.UpgradeCode == code);
        if (upgrade != null)
            foundUpgrades.Add(upgrade);
        upgrade = SlotUpgrades.Find(item => item.TreeId == treeId && item.UpgradeCode == code);
        if (upgrade != null)
            foundUpgrades.Add(upgrade);
        return foundUpgrades;
    }

    private void LogActivity(ActivityType type, Hero hero, params string[] quirks)
    {
        var log = DarkestDungeonManager.Campaign.CurrentLog();
        if (log != null)
            log.HeroRecords.Add(new ActorActivityRecord(type, hero, quirks));
    }
}