using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tavern : Building
{
    public List<TownActivity> Activities { get; set; }

    public Tavern()
    {
        Activities = new List<TownActivity>();
    }

    public void ProvideActivity()
    {
        for (int i = 0; i < Activities.Count; i++)
            Activities[i].ProvideActivity();
    }

    public void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        foreach (var activity in Activities)
        {
            activity.Reset();

            for (int i = activity.CostUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.Id].PurchasedUpgrades.Contains(activity.CostUpgrades[i].UpgradeCode))
                {
                    activity.ActivityCost = activity.CostUpgrades[i].Cost;
                    break;
                }
            }
            for (int i = activity.SlotUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.Id].PurchasedUpgrades.Contains(activity.SlotUpgrades[i].UpgradeCode))
                {
                    activity.NumberOfSlots = activity.SlotUpgrades[i].NumberOfSlots;
                    break;
                }
            }
            for (int i = activity.StressUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.Id].PurchasedUpgrades.Contains(activity.StressUpgrades[i].UpgradeCode))
                {
                    activity.StressHealAmount = activity.StressUpgrades[i].StressHeal;
                    break;
                }
            }
        }

        foreach (var activity in Activities)
        {
            for (int i = 1; i <= 3; i++)
            {
                if (i <= activity.NumberOfSlots)
                    activity.ActivitySlots.Add(new ActivitySlot(true, activity.ActivityCost.Amount));
                else
                    activity.ActivitySlots.Add(new ActivitySlot(false, activity.ActivityCost.Amount));
            }
        }
    }

    public void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        foreach (var activity in Activities)
        {
            for (int i = activity.CostUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.Id].PurchasedUpgrades.Contains(activity.CostUpgrades[i].UpgradeCode))
                {
                    activity.ActivityCost = activity.CostUpgrades[i].Cost;
                    break;
                }
            }
            for (int i = activity.SlotUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.Id].PurchasedUpgrades.Contains(activity.SlotUpgrades[i].UpgradeCode))
                {
                    activity.NumberOfSlots = activity.SlotUpgrades[i].NumberOfSlots;
                    break;
                }
            }
            for (int i = activity.StressUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.Id].PurchasedUpgrades.Contains(activity.StressUpgrades[i].UpgradeCode))
                {
                    activity.StressHealAmount = activity.StressUpgrades[i].StressHeal;
                    break;
                }
            }
        }

        foreach (var activity in Activities)
        {
            for (int i = 1; i <= 3; i++)
            {
                if (i <= activity.NumberOfSlots)
                    activity.ActivitySlots[i - 1].UpdateSlot(true, activity.ActivityCost.Amount);
                else
                    activity.ActivitySlots[i - 1].UpdateSlot(false, activity.ActivityCost.Amount);
            }
        }
    }

    public void UpdateActivitySlots(SaveCampaignData saveData)
    {
        for (int activityIndex = 0; activityIndex < Activities.Count; activityIndex++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i + 1 <= Activities[activityIndex].NumberOfSlots)
                {
                    if (saveData.tavernActivitySlots.Count > activityIndex && saveData.tavernActivitySlots[activityIndex].Count > i)
                    {
                        var activityHero = DarkestDungeonManager.Campaign.Heroes.Find(hero =>
                            hero.RosterId == saveData.tavernActivitySlots[activityIndex][i].HeroRosterId);

                        Activities[activityIndex].ActivitySlots[i].Hero = activityHero;
                        if (activityHero != null)
                            Activities[activityIndex].ActivitySlots[i].Status = saveData.tavernActivitySlots[activityIndex][i].Status;
                        else
                            Activities[activityIndex].ActivitySlots[i].Status = ActivitySlotStatus.Available;

                        Activities[activityIndex].ActivitySlots[i].UpdateSlot(true, Activities[activityIndex].ActivityCost.Amount);
                    }
                    else
                        Activities[activityIndex].ActivitySlots[i].UpdateSlot(true, Activities[activityIndex].ActivityCost.Amount);
                }
                else
                    Activities[activityIndex].ActivitySlots[i].UpdateSlot(false, Activities[activityIndex].ActivityCost.Amount);
            }
        }
    }
}