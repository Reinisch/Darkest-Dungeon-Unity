using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void TriketDelegate(Trinket trinket);

public class RealmInventory
{
    public int MaxCapacity { get; set; }

    public List<Trinket> Trinkets { get; set; }

    public TriketDelegate TrinketAddAction { get; set; }

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
        for(int i = 0; i < saveData.trinketData.Count; i++)
            Trinkets.Add((Trinket)DarkestDungeonManager.Data.Items["trinket"][saveData.trinketData[i]]);
    }
}
