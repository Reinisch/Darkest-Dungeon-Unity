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
                if (purchases[activity.TreeId].PurchasedUpgrades.Contains(activity.CostUpgrades[i].UpgradeCode))
                {
                    activity.ActivityCost = activity.CostUpgrades[i].Cost;
                    break;
                }
            }
            for (int i = activity.SlotUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.TreeId].PurchasedUpgrades.Contains(activity.SlotUpgrades[i].UpgradeCode))
                {
                    activity.NumberOfSlots = activity.SlotUpgrades[i].NumberOfSlots;
                    break;
                }
            }
            for (int i = activity.StressUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.TreeId].PurchasedUpgrades.Contains(activity.StressUpgrades[i].UpgradeCode))
                {
                    activity.StressHealAmount = activity.StressUpgrades[i].StressHeal;
                    break;
                }
            }
        }

        foreach (var activity in Activities)
        {
            bool isActivityLocked = DarkestDungeonManager.Campaign.EventModifiers.IsActivityLocked(activity.Id);
            bool isActivityFree = DarkestDungeonManager.Campaign.EventModifiers.IsActivityFree(activity.Id);
            float costModifier = DarkestDungeonManager.Campaign.EventModifiers.ActivityCostModifier(activity.Id);

            for (int i = 1; i <= 3; i++)
            {
                if (i <= activity.NumberOfSlots)
                    activity.ActivitySlots.Add(new ActivitySlot(isActivityLocked ? false : true,
                        isActivityFree ? 0 : (int)(activity.ActivityCost.Amount * costModifier)));
                else
                    activity.ActivitySlots.Add(new ActivitySlot(false, isActivityFree ?
                        0 : (int)(activity.ActivityCost.Amount * costModifier)));
            }
        }
    }

    public void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        foreach (var activity in Activities)
        {
            for (int i = activity.CostUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.TreeId].PurchasedUpgrades.Contains(activity.CostUpgrades[i].UpgradeCode))
                {
                    activity.ActivityCost = activity.CostUpgrades[i].Cost;
                    break;
                }
            }
            for (int i = activity.SlotUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.TreeId].PurchasedUpgrades.Contains(activity.SlotUpgrades[i].UpgradeCode))
                {
                    activity.NumberOfSlots = activity.SlotUpgrades[i].NumberOfSlots;
                    break;
                }
            }
            for (int i = activity.StressUpgrades.Count - 1; i >= 0; i--)
            {
                if (purchases[activity.TreeId].PurchasedUpgrades.Contains(activity.StressUpgrades[i].UpgradeCode))
                {
                    activity.StressHealAmount = activity.StressUpgrades[i].StressHeal;
                    break;
                }
            }
        }

        foreach (var activity in Activities)
        {
            bool isActivityLocked = DarkestDungeonManager.Campaign.EventModifiers.IsActivityLocked(activity.Id);
            bool isActivityFree = DarkestDungeonManager.Campaign.EventModifiers.IsActivityFree(activity.Id);
            float costModifier = DarkestDungeonManager.Campaign.EventModifiers.ActivityCostModifier(activity.Id);

            for (int i = 1; i <= 3; i++)
            {
                if (i <= activity.NumberOfSlots)
                    activity.ActivitySlots[i - 1].UpdateSlot(isActivityLocked ? false : true,
                        isActivityFree ? 0 : (int)(activity.ActivityCost.Amount * costModifier));
                else
                    activity.ActivitySlots[i - 1].UpdateSlot(false, isActivityFree ?
                        0 : (int)(activity.ActivityCost.Amount * costModifier));
            }
        }
    }

    public void UpdateActivitySlots(SaveCampaignData saveData)
    {
        for (int activityIndex = 0; activityIndex < Activities.Count; activityIndex++)
        {
            bool isActivityLocked = DarkestDungeonManager.Campaign.EventModifiers.IsActivityLocked(Activities[activityIndex].Id);
            bool isActivityFree = DarkestDungeonManager.Campaign.EventModifiers.IsActivityFree(Activities[activityIndex].Id);
            float costModifier = DarkestDungeonManager.Campaign.EventModifiers.ActivityCostModifier(Activities[activityIndex].Id);

            for (int i = 0; i < 3; i++)
            {
                if (i + 1 <= Activities[activityIndex].NumberOfSlots)
                {
                    if (saveData.abbeyActivitySlots.Count > activityIndex && saveData.abbeyActivitySlots[activityIndex].Count > i)
                    {
                        var activityHero = DarkestDungeonManager.Campaign.Heroes.Find(hero =>
                            hero.RosterId == saveData.abbeyActivitySlots[activityIndex][i].HeroRosterId);

                        Activities[activityIndex].ActivitySlots[i].Hero = activityHero;
                        if (activityHero != null)
                            Activities[activityIndex].ActivitySlots[i].Status = saveData.abbeyActivitySlots[activityIndex][i].Status;
                        else
                            Activities[activityIndex].ActivitySlots[i].Status = ActivitySlotStatus.Available;

                        Activities[activityIndex].ActivitySlots[i].UpdateSlot(isActivityLocked ? false : true,
                            isActivityFree ? 0 : (int)(Activities[activityIndex].ActivityCost.Amount * costModifier));
                    }
                    else
                        Activities[activityIndex].ActivitySlots[i].UpdateSlot(isActivityLocked ? false : true,
                            isActivityFree ? 0 : (int)(Activities[activityIndex].ActivityCost.Amount * costModifier));
                }
                else
                    Activities[activityIndex].ActivitySlots[i].UpdateSlot(false,
                        isActivityFree ? 0 : (int)(Activities[activityIndex].ActivityCost.Amount * costModifier));
            }
        }
    }
}