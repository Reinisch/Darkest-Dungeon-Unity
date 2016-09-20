using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public delegate void InventorySlotEvent(InventorySlot slot, InventoryItem item);
public delegate void InventorySlotActivationEvent(InventorySlot slot);

public class InventorySlot : BaseSlot, IDropHandler
{
    public Image overlayIcon;

    public IInventory Inventory { get; set; }
    public InventoryItem SlotItem { get; set; }

    public bool InteractionDisabled
    { 
        get;
        set;
    }
    public bool HasItem
    {
        get
        {
            return SlotItem.IsNotEmpty;
        }
    }
    public bool IsFullyStacked
    {
        get
        {
            return SlotItem.IsFull;
        }
    }
    public bool HasFreeSpaceForItem(ItemDefinition item)
    {
        return SlotItem.HasFreeSpaceForItem(item);
    }

    public void Initialize(IInventory inventory)
    {
        Inventory = inventory;
        SlotItem = GetComponentInChildren<InventoryItem>();
        SlotItem.Initialize(this);
    }

    public event InventorySlotEvent onDropOut;
    public event InventorySlotEvent onDropIn;
    public event InventorySlotEvent onSwap;

    public event InventorySlotActivationEvent onActivate;
    public event InventorySlotActivationEvent onAlternativeActivate;

    public void CreateItem(ItemDefinition itemDefinition)
    {
        SlotItem.gameObject.SetActive(true);
        SlotItem.Create(itemDefinition.Type, itemDefinition.Id, itemDefinition.Amount);
        if (Inventory != null && Inventory.Configuration == InventoryConfiguration.RaidInventory)
            UpdateState();
    }
    public void CreateItem(string itemType, string itemId, int amount)
    {
        SlotItem.gameObject.SetActive(true);
        SlotItem.Create(itemType, itemId, amount);
        if (Inventory != null && Inventory.Configuration == InventoryConfiguration.RaidInventory)
            UpdateState();
    }
    public void CreateItem(Trinket trinket)
    {
        SlotItem.gameObject.SetActive(true);
        SlotItem.Create(trinket);
        if (Inventory != null && Inventory.Configuration == InventoryConfiguration.RaidInventory)
            UpdateState();
    }
    public void CreateItem(InventorySlotData slotData)
    {
        SlotItem.Create(slotData);
        if (Inventory != null && Inventory.Configuration == InventoryConfiguration.RaidInventory)
            UpdateState();
    }

    public void SetActiveState(bool active)
    {
        SlotItem.SetActive(active);
    }
    public void SetPeacfulState(bool looting)
    {
        SlotItem.SetPeacefulState(looting);
    }
    public void SetCombatState()
    {
        SlotItem.SetCombatState();
    }
    public void SetObstacleState()
    {
        SlotItem.SetObstacleState();
    }
    public void SetInteractionState(bool questInteraction)
    {
        SlotItem.SetInteractionState(questInteraction);
    }
    public void UpdateState()
    {
        switch(Inventory.State)
        {
            case InventoryState.Normal:
                SlotItem.SetActive(true);
                break;
            case InventoryState.Disabled:
                SlotItem.SetActive(false);
                break;
            case InventoryState.Combat:
                SlotItem.SetCombatState();
                break;
            case InventoryState.Peaceful:
                SlotItem.SetPeacefulState(false);
                break;
            case InventoryState.PeacefulLooting:
                SlotItem.SetPeacefulState(true);
                break;
            case InventoryState.Interaction:
                SlotItem.SetInteractionState(false);
                break;
            case InventoryState.QuestInteraction:
                SlotItem.SetInteractionState(true);
                break;
            case InventoryState.Obstacle:
                SlotItem.SetObstacleState();
                break;
            default:
                break;
        }
    }
    public void DeleteItem()
    {
        SlotItem.gameObject.SetActive(false);
        SlotItem.Delete();
    }

    public void ItemDroppedIn(InventorySlot slot, InventoryItem itemDroppedIn)
    {
        if (onDropIn != null)
            onDropIn(this, itemDroppedIn);
    }
    public void ItemDroppedOut(InventorySlot slot, InventoryItem itemDroppedOut)
    {
        if (onDropOut != null)
            onDropOut(this, itemDroppedOut);
    }
    public void ItemSwapped(InventorySlot slot, InventoryItem incomingItem)
    {
        if (onSwap != null)
            onSwap(this, incomingItem);
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (InteractionDisabled)
            return;
        if (DragManager.Instanse.PartySlotItem != null)
        {
            CheckDrop(DragManager.Instanse.PartySlotItem);
        }
    }
    public virtual void CheckDrop(InventoryItem movingItem)
    {
        InventorySlot fromSlot = movingItem.Slot;

        if(Inventory != null)
        {
            switch(Inventory.Configuration)
            {
                case InventoryConfiguration.Equipment:
                    if (movingItem.ItemType != "trinket")
                        return;
                    if (Inventory.State == InventoryState.Disabled)
                        return;
                    if (SlotItem.Deactivated)
                        return;
                    break;
                case InventoryConfiguration.LootInventory:
                    if (movingItem.ItemType == "quest_item")
                        return;
                    if (Inventory.State == InventoryState.Disabled)
                        return;
                    break;
                case InventoryConfiguration.RaidInventory:
                    if (fromSlot.Inventory.Configuration == InventoryConfiguration.Equipment)
                        if (HasItem && SlotItem.Deactivated)
                            return;
                    break;
                case InventoryConfiguration.TrinketInventory:
                    if (fromSlot.Inventory.Configuration == InventoryConfiguration.Equipment)
                        if (HasItem && SlotItem.Deactivated)
                            return;
                    break;
                default:
                    break;
            }
        }

        if (SlotItem.IsNotEmpty == true)
        {         
            if (movingItem.Slot != null)
            {
                if (SlotItem.IsFull || !SlotItem.IsSameItem(movingItem.Item))
                {
                    if(movingItem.Slot.Inventory != null)
                    {
                        switch (movingItem.Slot.Inventory.Configuration)
                        {
                            case InventoryConfiguration.Equipment:
                                if (SlotItem.ItemType != "trinket")
                                    return;
                                break;
                            case InventoryConfiguration.LootInventory:
                                if (SlotItem.ItemType == "quest_item")
                                    return;
                                break;
                            case InventoryConfiguration.RaidInventory:
                                break;
                        }
                    }

                    SwapItems(movingItem);
                }
                else
                    MergeItems(movingItem);
            }
        }
        else
        {
            if (movingItem.Slot != null)
                RepositionItem(movingItem);
        }

        UpdateState();
        fromSlot.UpdateState();
    }

    void RepositionItem(InventoryItem slotItem)
    {
        InventorySlot toSlot = this;
        InventorySlot fromSlot = slotItem.Slot;

        InventoryItem fromSlotItem = fromSlot.SlotItem;
        InventoryItem toSlotItem = toSlot.SlotItem;

        toSlot.SlotItem = fromSlotItem;
        fromSlotItem.Slot = toSlot;
        fromSlotItem.RectTransform.SetParent(toSlot.RectTransform, false);
        fromSlotItem.RectTransform.SetAsFirstSibling();
        toSlot.ItemDroppedIn(toSlot, fromSlotItem);

        fromSlot.SlotItem = toSlotItem;
        toSlotItem.Slot = fromSlot;
        toSlotItem.RectTransform.SetParent(fromSlot.RectTransform, false);
        toSlotItem.RectTransform.SetAsFirstSibling();
        fromSlot.ItemDroppedOut(fromSlot, fromSlotItem);
    }
    void SwapItems(InventoryItem slotItem)
    {
        InventorySlot toSlot = this;
        InventorySlot fromSlot = slotItem.Slot;

        InventoryItem fromSlotItem = fromSlot.SlotItem;
        InventoryItem toSlotItem = toSlot.SlotItem;

        toSlot.SlotItem = fromSlotItem;
        fromSlotItem.Slot = toSlot;
        fromSlotItem.RectTransform.SetParent(toSlot.RectTransform, false);
        fromSlotItem.RectTransform.SetAsFirstSibling();
        toSlot.ItemSwapped(toSlot, fromSlotItem);

        fromSlot.SlotItem = toSlotItem;
        toSlotItem.Slot = fromSlot;
        toSlotItem.RectTransform.SetParent(fromSlot.RectTransform, false);
        toSlotItem.RectTransform.SetAsFirstSibling();
        fromSlot.ItemSwapped(fromSlot, toSlotItem);
    }
    void MergeItems(InventoryItem slotItem)
    {
        SlotItem.MergeItems(slotItem);
    }

    public void SlotActivated()
    {
        if (onActivate != null)
            onActivate(this);
        else if (onAlternativeActivate != null)
            onAlternativeActivate(this);
    }
}