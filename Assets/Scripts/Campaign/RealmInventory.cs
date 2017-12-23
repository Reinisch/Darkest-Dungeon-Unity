using System;
using System.Collections.Generic;

public class RealmInventory
{
    public int MaxCapacity { get; set; }

    public List<Trinket> Trinkets { get; set; }

    public Action<Trinket> TrinketAddAction { get; set; }

    public void AddTrinket(Trinket trinket)
    {
        if (TrinketAddAction != null)
            TrinketAddAction(trinket);
        else
            Trinkets.Add(trinket);
    }

    public RealmInventory(SaveCampaignData saveData)
    {
        Trinkets = new List<Trinket>();
        for(int i = 0; i < saveData.InventoryTrinkets.Count; i++)
            Trinkets.Add((Trinket)DarkestDungeonManager.Data.Items["trinket"][saveData.InventoryTrinkets[i]]);
    }
}