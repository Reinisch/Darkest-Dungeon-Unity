using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RaidStatus { Preparation, Success, Abandon, Defeat }

public class RaidManager : MonoBehaviour
{
    public Quest Quest { get; set; }
    public RaidParty RaidParty { get; set; }
    public List<InventorySlotData> InventorySlotData { get; set; }

    public RaidStatus Status { get; set; }

    public void DeployFromPreparation(RaidPreparationManager preparationManager, ShopManager shopManager)
    {
        Quest = preparationManager.SelectedQuestSlot.Quest;
        RaidParty = new RaidParty(preparationManager.raidPartyPanel);
        InventorySlotData = shopManager.partyInventory.SaveInventorySlotData();
    }

    public void QuickStart(PartyInventory inventory)
    {
        Quest = DarkestDungeonManager.Campaign.Quests.Find(quest => quest.Difficulty == 1);
        if (Quest == null)
            Quest = DarkestDungeonManager.Campaign.Quests[0];

        RaidParty = new RaidParty(DarkestDungeonManager.Campaign.Heroes.GetRange(0, 4));
        inventory.LoadInitialSetup(Quest, RaidParty);
        InventorySlotData = inventory.SaveInventorySlotData();
    }
}
