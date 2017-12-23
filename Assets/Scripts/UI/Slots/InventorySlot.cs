using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : BaseSlot, IDropHandler
{
    [SerializeField]
    private Image overlayIcon;

    public IInventory Inventory { get; private set; }
    public InventoryItem SlotItem { get; private set; }
    public bool InteractionDisabled { private get; set; }
    public Image OverlayIcon { get { return overlayIcon; } set { overlayIcon = value; } }
    public bool HasItem { get { return SlotItem.IsNotEmpty; } }

    public event Action<InventorySlot, InventoryItem> EventDropOut;
    public event Action<InventorySlot, InventoryItem> EventDropIn;
    public event Action<InventorySlot, InventoryItem> EventSwap;
    public event Action<InventorySlot> EventActivate;

    public void Initialize(IInventory inventory)
    {
        Inventory = inventory;
        SlotItem = GetComponentInChildren<InventoryItem>();
        SlotItem.Initialize(this);
    }

    public bool HasFreeSpaceForItem(ItemDefinition item)
    {
        return SlotItem.HasFreeSpaceForItem(item);
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

    public void DeleteItem()
    {
        SlotItem.gameObject.SetActive(false);
        SlotItem.Delete();
    }

    #region Inventory States

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
        }
    }

    #endregion

    public void SlotActivated()
    {
        if (EventActivate != null)
            EventActivate(this);
    }

    public void ItemDroppedIn(InventoryItem itemDroppedIn)
    {
        if (EventDropIn != null)
            EventDropIn(this, itemDroppedIn);
    }

    public void ItemDroppedOut(InventoryItem itemDroppedOut)
    {
        if (EventDropOut != null)
            EventDropOut(this, itemDroppedOut);
    }

    public void ItemSwapped(InventoryItem incomingItem)
    {
        if (EventSwap != null)
            EventSwap(this, incomingItem);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (InteractionDisabled)
            return;
        if (DragManager.Instanse.PartySlotItem != null)
        {
            CheckDrop(DragManager.Instanse.PartySlotItem);
        }
    }

    private void CheckDrop(InventoryItem movingItem)
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
            }
        }

        if (SlotItem.IsNotEmpty)
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

    private void RepositionItem(InventoryItem slotItem)
    {
        InventorySlot toSlot = this;
        InventorySlot fromSlot = slotItem.Slot;

        InventoryItem fromSlotItem = fromSlot.SlotItem;
        InventoryItem toSlotItem = toSlot.SlotItem;

        toSlot.SlotItem = fromSlotItem;
        fromSlotItem.Slot = toSlot;
        fromSlotItem.RectTransform.SetParent(toSlot.RectTransform, false);
        fromSlotItem.RectTransform.SetAsFirstSibling();
        toSlot.ItemDroppedIn(fromSlotItem);

        fromSlot.SlotItem = toSlotItem;
        toSlotItem.Slot = fromSlot;
        toSlotItem.RectTransform.SetParent(fromSlot.RectTransform, false);
        toSlotItem.RectTransform.SetAsFirstSibling();
        fromSlot.ItemDroppedOut(fromSlotItem);
    }

    private void SwapItems(InventoryItem slotItem)
    {
        InventorySlot toSlot = this;
        InventorySlot fromSlot = slotItem.Slot;

        InventoryItem fromSlotItem = fromSlot.SlotItem;
        InventoryItem toSlotItem = toSlot.SlotItem;

        toSlot.SlotItem = fromSlotItem;
        fromSlotItem.Slot = toSlot;
        fromSlotItem.RectTransform.SetParent(toSlot.RectTransform, false);
        fromSlotItem.RectTransform.SetAsFirstSibling();
        toSlot.ItemSwapped(fromSlotItem);

        fromSlot.SlotItem = toSlotItem;
        toSlotItem.Slot = fromSlot;
        toSlotItem.RectTransform.SetParent(fromSlot.RectTransform, false);
        toSlotItem.RectTransform.SetAsFirstSibling();
        fromSlot.ItemSwapped(toSlotItem);
    }

    private void MergeItems(InventoryItem slotItem)
    {
        SlotItem.MergeItems(slotItem);
    }
}