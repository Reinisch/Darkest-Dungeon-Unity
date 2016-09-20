using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public enum InventoryConfiguration { RaidInventory, Equipment, LootInventory, TrinketInventory }
public enum InventoryState { Normal, Disabled, Peaceful, PeacefulLooting, Combat, Interaction, QuestInteraction, Obstacle, }

public class PartyInventory : MonoBehaviour, IInventory
{
    public List<InventorySlot> InventorySlots { get; set; }
    public InventoryConfiguration Configuration { get; set; }
    public InventoryState State { get; set; }
    public Hero CurrentHero
    {
        get { return null; }
    }

    public float PercentageFull
    {
        get
        {
            float fullness = 0;
            for (int i = 0; i < InventorySlots.Count; i++)
                if (InventorySlots[i].HasItem)
                    fullness += (float)InventorySlots[i].SlotItem.Item.Amount / InventorySlots[i].SlotItem.ItemData.StackLimit;
            return fullness / InventorySlots.Count;
        }
    }

    void Awake()
    {
        if(InventorySlots == null)
        {
            InventorySlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>(true));
            for (int i = 0; i < InventorySlots.Count; i++)
                InventorySlots[i].Initialize(this);
        }
        DragManager.Instanse.onStartDraggingInventorySlot += Instanse_onStartDraggingInventorySlot;
        DragManager.Instanse.onEndDraggingInventorySlot += Instanse_onEndDraggingInventorySlot;
    }

    void OnDestroy()
    {
        DragManager.Instanse.onStartDraggingInventorySlot -= Instanse_onStartDraggingInventorySlot;
        DragManager.Instanse.onEndDraggingInventorySlot -= Instanse_onEndDraggingInventorySlot;
    }

    void Instanse_onEndDraggingInventorySlot(InventorySlot slot)
    {
        UpdateState();
    }
    void Instanse_onStartDraggingInventorySlot(InventorySlot slot)
    {
        if (gameObject.activeSelf && slot.Inventory.Configuration == InventoryConfiguration.Equipment)
        {
            var charEquipmentPanel = slot.Inventory as CharEquipmentPanel;
            for (int i = 0; i < InventorySlots.Count; i++)
            {
                if (InventorySlots[i].HasItem)
                {
                    var trinket = InventorySlots[i].SlotItem.ItemData as Trinket;
                    if (trinket != null)
                    {
                        if (trinket.EquipLimit == 1 && charEquipmentPanel.ContainsItem(trinket))
                        {
                            InventorySlots[i].SetActiveState(false);
                        }
                        else if (trinket.ClassRequirements.Count > 0 && !trinket.ClassRequirements.Contains(charEquipmentPanel.CurrentHero.Class))
                        {
                            InventorySlots[i].SetActiveState(false);
                        }
                        else
                            InventorySlots[i].SetActiveState(true);
                    }
                }
            }
        }
    }

    public void Initialize()
    {
        InventorySlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>(true));
        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].Initialize(this);
    }

    InventorySlot SeekExtendableStack(ItemDefinition item)
    {
        for (int i = 0; i < InventorySlots.Count; i++)
            if (InventorySlots[i].HasFreeSpaceForItem(item))
                return InventorySlots[i];

        return null;
    }

    public void LoadInitialSetup(Quest quest, RaidParty party)
    {
        DiscardAll();

        for (int i = 0; i < DarkestDungeonManager.Data.Provision.StartingLengthInventories[quest.Length].Count; i++)
            DistributeItem(DarkestDungeonManager.Data.Provision.StartingLengthInventories[quest.Length][i]);

        for (int i = 0; i < party.HeroInfo.Count; i++)
            if (DarkestDungeonManager.Data.Provision.HeroClassItemList.ContainsKey(party.HeroInfo[i].Hero.Class))
                for (int j = 0; j < DarkestDungeonManager.Data.Provision.HeroClassItemList[party.HeroInfo[i].Hero.Class].Count; j++)
                    DistributeItem(DarkestDungeonManager.Data.Provision.HeroClassItemList[party.HeroInfo[i].Hero.Class][j]);

        for (int i = 0; i < quest.Goal.StartingItems.Count; i++)
            DistributeItem(quest.Goal.StartingItems[i]);
    }
    public void LoadInitialSetup(Quest quest, RaidPartyPanel partyPanel)
    {
        DiscardAll();

        for (int i = 0; i < DarkestDungeonManager.Data.Provision.StartingLengthInventories[quest.Length].Count; i++)
            DistributeItem(DarkestDungeonManager.Data.Provision.StartingLengthInventories[quest.Length][i]);

        for (int i = 0; i < partyPanel.PartySlots.Count; i++)
            if(DarkestDungeonManager.Data.Provision.HeroClassItemList.ContainsKey(partyPanel.PartySlots[i].SelectedHero.Hero.Class))
                for (int j = 0; j < DarkestDungeonManager.Data.Provision.HeroClassItemList[partyPanel.PartySlots[i].SelectedHero.Hero.Class].Count; j++)
                    DistributeItem(DarkestDungeonManager.Data.Provision.HeroClassItemList[partyPanel.PartySlots[i].SelectedHero.Hero.Class][j]);

        for (int i = 0; i < quest.Goal.StartingItems.Count; i++)
            DistributeItem(quest.Goal.StartingItems[i]);
    }
    public void LoadItems(List<InventorySlotData> items)
    {
        int inventorySlots = Mathf.Min(items.Count, InventorySlots.Count);
        for (int i = 0; i < inventorySlots; i++)
        {
            if (items[i] == null)
                InventorySlots[i].DeleteItem();
            else
                InventorySlots[i].CreateItem(items[i]);
        }
    }
    public void LoadItems(List<ItemDefinition> items)
    {
        int inventorySlots = Mathf.Min(InventorySlots.Count, items.Count);

        for (int i = 0; i < inventorySlots; i++)
        {
            if (items[i] == null)
                InventorySlots[i].DeleteItem();
            else
                InventorySlots[i].CreateItem(items[i]);
        }
    }
    public List<InventorySlotData> SaveInventorySlotData()
    {
        List<InventorySlotData> slotData = new List<InventorySlotData>();
        foreach(var slot in InventorySlots)
        {
            InventorySlotData newSlotData = new InventorySlotData();
            newSlotData.Item = slot.SlotItem.Item;
            newSlotData.ItemData = slot.SlotItem.ItemData;
            slotData.Add(newSlotData);
        }
        return slotData;
    }

    public bool HasSomething()
    {
        for (int i = 0; i < InventorySlots.Count; i++)
            if (InventorySlots[i].HasItem)
                return true;

        return false;
    }
    public bool CheckSingleInventorySpace(ItemDefinition item)
    {
        int stackLimit = DarkestDungeonManager.Data
            .Items[item.Type][item.Id].StackLimit;
        int spaceNeeded = 1;

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            if (InventorySlots[i].SlotItem.IsNotEmpty)
            {
                if (InventorySlots[i].SlotItem.Item.Type == item.Type
                    && InventorySlots[i].SlotItem.Item.Id == item.Id)
                {
                    if (InventorySlots[i].SlotItem.Item.Amount < stackLimit)
                    {
                        spaceNeeded -= stackLimit - InventorySlots[i].SlotItem.Item.Amount;
                    }
                }
            }
            else
            {
                spaceNeeded -= stackLimit;
            }
            if (spaceNeeded <= 0)
                return true;
        }
        return false;
    }
    public bool CheckInventorySpace(ItemDefinition item)
    {
        int stackLimit = DarkestDungeonManager.Data
            .Items[item.Type][item.Id].StackLimit;
        int spaceNeeded = item.Amount;

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            if (InventorySlots[i].SlotItem.IsNotEmpty)
            {
                if (InventorySlots[i].SlotItem.Item.Type == item.Type
                    && InventorySlots[i].SlotItem.Item.Id == item.Id)
                {
                    if (InventorySlots[i].SlotItem.Item.Amount < stackLimit)
                    {
                        spaceNeeded -= stackLimit - InventorySlots[i].SlotItem.Item.Amount;
                    }
                }
            }
            else
            {
                spaceNeeded -= stackLimit;
            }
            if (spaceNeeded <= 0)
                return true;
        }
        return false;
    }
    public bool ContainsItem(ItemData item)
    {
        for (int i = 0; i < InventorySlots.Count; i++)
            if (InventorySlots[i].HasItem && InventorySlots[i].SlotItem.ItemData == item)
                return true;

        return false;
    }
    public bool ContainsEnoughItems(ItemDefinition item)
    {
        int itemsFound = 0;
        for (int i = 0; i < InventorySlots.Count; i++)
            if (InventorySlots[i].HasItem && InventorySlots[i].SlotItem.ItemData.Id == item.Id)
                itemsFound += InventorySlots[i].SlotItem.Item.Amount;

        if (itemsFound >= item.Amount)
            return true;
        return false;
    }
    public bool ContainsEnoughItems(string itemType, int itemAmount)
    {
        int itemsFound = 0;
        for (int i = 0; i < InventorySlots.Count; i++)
            if (InventorySlots[i].HasItem && InventorySlots[i].SlotItem.ItemData.Type == itemType)
                itemsFound += InventorySlots[i].SlotItem.Item.Amount;

        if (itemsFound >= itemAmount)
            return true;
        return false;
    }
    public bool ContaintItemType(string itemType)
    {
        for (int i = 0; i < InventorySlots.Count; i++)
            if (InventorySlots[i].HasItem && InventorySlots[i].SlotItem.ItemType == itemType)
                return true;

        return false;
    }
    public bool UseItem(ItemData item)
    {
        for (int i = 0; i < InventorySlots.Count; i++)
            if (InventorySlots[i].HasItem && InventorySlots[i].SlotItem.ItemData == item)
            {
                InventorySlots[i].SlotItem.RemoveItems(1);
                return true;
            }

        return false;
    }
    public void DeactivateEmptySlots()
    {
        foreach (var slot in InventorySlots)
            if (slot.HasItem)
                slot.gameObject.SetActive(true);
            else
                slot.gameObject.SetActive(false);
    }
    public void DiscardAll()
    {
        foreach (var slot in InventorySlots)
            slot.SlotItem.Delete();
    }
    public void DistributeItem(ItemDefinition itemDefinition)
    {
        if(!DarkestDungeonManager.Data.Items.ContainsKey(itemDefinition.Type))
        {
            Debug.LogError("Missing item: Type - " + itemDefinition.Type + " Id - " + itemDefinition.Id);
            return;
        }

        if (!DarkestDungeonManager.Data.Items[itemDefinition.Type].ContainsKey(itemDefinition.Id))
        {
            Debug.LogError("Missing item: Type - " + itemDefinition.Type + " Id - " + itemDefinition.Id);
            return;
        }
        ItemData itemData = DarkestDungeonManager.Data
            .Items[itemDefinition.Type][itemDefinition.Id];
        int stackLimit = itemData.StackLimit;
        int spaceNeeded = itemDefinition.Amount;

        while (spaceNeeded > 0)
        {
            InventorySlot extendableSlot = SeekExtendableStack(itemDefinition);
            if (extendableSlot != null)
                spaceNeeded = extendableSlot.SlotItem.AddItems(spaceNeeded);
            else
                break;
        }

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            if (spaceNeeded == 0)
                return;

            if (InventorySlots[i].HasItem)
            {
                if (InventorySlots[i].HasFreeSpaceForItem(itemDefinition))
                    spaceNeeded = InventorySlots[i].SlotItem.AddItems(spaceNeeded);
            }
            else
            {
                if (spaceNeeded <= stackLimit)
                {
                    InventorySlots[i].CreateItem(itemDefinition.Type, itemDefinition.Id, spaceNeeded);
                    spaceNeeded = 0;
                }
                else
                {
                    InventorySlots[i].CreateItem(itemDefinition.Type, itemDefinition.Id, stackLimit);
                    spaceNeeded -= stackLimit;
                }
            }
        }
    }
    public void DistributeFromShopItem(ShopSlot slot, InventorySlot dropSlot)
    {
        int stackLimit = slot.InventoryItem.StackLimit;
        ItemDefinition item = slot.Item;
        int spaceNeeded = 1;

        if (dropSlot != null)
        {
            if (dropSlot.HasItem)
            {
                if (dropSlot.HasFreeSpaceForItem(slot.Item))
                    spaceNeeded = dropSlot.SlotItem.AddItems(spaceNeeded);
            }
            else
            {
                if (spaceNeeded <= stackLimit)
                {
                    dropSlot.CreateItem(item.Type, item.Id, spaceNeeded);
                    spaceNeeded = 0;
                }
                else
                {
                    dropSlot.CreateItem(item.Type, item.Id, stackLimit);
                    spaceNeeded -= stackLimit;
                }
            }
        }

        while (spaceNeeded > 0)
        {
            InventorySlot extendableSlot = SeekExtendableStack(slot.Item);
            if (extendableSlot != null)
                spaceNeeded = extendableSlot.SlotItem.AddItems(spaceNeeded);
            else
                break;
        }

        for (int i = 0; i < InventorySlots.Count; i++)
        {
            if (spaceNeeded == 0)
                return;

            if (InventorySlots[i].HasItem)
            {
                if (InventorySlots[i].HasFreeSpaceForItem(item))
                    spaceNeeded = InventorySlots[i].SlotItem.AddItems(spaceNeeded);
            }
            else
            {
                if (spaceNeeded <= stackLimit)
                {
                    InventorySlots[i].CreateItem(item.Type, item.Id, spaceNeeded);
                    spaceNeeded = 0;
                }
                else
                {
                    InventorySlots[i].CreateItem(item.Type, item.Id, stackLimit);
                    spaceNeeded -= stackLimit;
                }
            }       
        }
    }
    public void DiscardAllItems(InventorySlot discardSlot)
    {
        discardSlot.SlotItem.Delete();
    }
    public void DiscardSingleItem(InventorySlot discardSlot)
    {
        discardSlot.SlotItem.RemoveItems(1);
    }
    public void DiscardItemType(string itemType, int amount)
    {
        int needToDiscard = amount;
        for(int i = 0; i < InventorySlots.Count; i++)
        {
            if (needToDiscard == 0)
                return;
            if(InventorySlots[i].HasItem && InventorySlots[i].SlotItem.Item.Type == itemType)
            {
                if(InventorySlots[i].SlotItem.Item.Amount <= needToDiscard)
                {
                    needToDiscard -= InventorySlots[i].SlotItem.Item.Amount;
                    InventorySlots[i].SlotItem.Delete();
                }
                else if (InventorySlots[i].SlotItem.Item.Amount > needToDiscard)
                {
                    InventorySlots[i].SlotItem.RemoveItems(needToDiscard);
                    needToDiscard = 0;
                }
            }
        }
    }

    public void SetActivated()
    {
        State = InventoryState.Normal;
        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].SetActiveState(true);
    }
    public void SetDeactivated()
    {
        State = InventoryState.Disabled;
        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].SetActiveState(false);
    }
    public void SetPeacefulState(bool looting)
    {
        if(looting)
            State = InventoryState.PeacefulLooting;
        else
            State = InventoryState.Peaceful;

        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].SetPeacfulState(looting);
    }
    public void SetCombatState()
    {
        State = InventoryState.Combat;
        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].SetCombatState();
    }
    public void SetInteractionState(bool questInteraction)
    {
        if (questInteraction)
            State = InventoryState.QuestInteraction;
        else
            State = InventoryState.Interaction;

        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].SetInteractionState(questInteraction);
    }
    public void SetObstacleState()
    {
        State = InventoryState.Obstacle;
        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].SetObstacleState();
    }
    public void UpdateState()
    {
        for (int i = 0; i < InventorySlots.Count; i++)
            InventorySlots[i].UpdateState();
    }  
}