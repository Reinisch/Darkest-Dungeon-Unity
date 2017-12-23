using UnityEngine;
using System.Collections.Generic;

public enum RaidStatus { Preparation, Success, Abandon, Defeat }

public class RaidManager : MonoBehaviour
{
    public Quest Quest { get; set; }
    public RaidParty RaidParty { get; set; }
    public List<InventorySlotData> InventorySlotData { get; private set; }
    public RaidStatus Status { get; set; }

    public void DeployFromPreparation(RaidPreparationManager preparationManager, ShopManager shopManager)
    {
        Quest = preparationManager.SelectedQuestSlot.Quest;
        RaidParty = new RaidParty(preparationManager.RaidPartyPanel);
        InventorySlotData = shopManager.PartyInventory.SaveInventorySlotData();
    }

    public void QuickStart(PartyInventory inventory)
    {
        Quest = DarkestDungeonManager.Campaign.Quests.Find(quest => quest.Difficulty == 1) ?? DarkestDungeonManager.Campaign.Quests[0];

        RaidParty = new RaidParty(DarkestDungeonManager.Campaign.Heroes.GetRange(0, 4));
        inventory.LoadInitialSetup(Quest, RaidParty);
        InventorySlotData = inventory.SaveInventorySlotData();
    }
}
