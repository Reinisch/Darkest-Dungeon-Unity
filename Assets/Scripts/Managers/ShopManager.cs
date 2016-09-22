using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public ShopInventory shopInventory;
    public PartyInventory partyInventory;
    public ItemSellbackPanel sellBackPanel;

    public void ActivateShopBehaviour()
    {
        DragManager.Instanse.onStartDraggingInventorySlot += ActivateSellbackPanel;
        DragManager.Instanse.onEndDraggingInventorySlot += DeactivateSellbackPanel;
        foreach(var slot in partyInventory.InventorySlots)
            slot.onActivate += SellSingleItem;
    }
    public void DeactivateShopBehaviour()
    {
        DragManager.Instanse.onStartDraggingInventorySlot -= ActivateSellbackPanel;
        DragManager.Instanse.onEndDraggingInventorySlot -= DeactivateSellbackPanel;
        foreach (var slot in partyInventory.InventorySlots)
            slot.onActivate -= SellSingleItem;
    }

    void SellSingleItem(InventorySlot slot)
    {
        if (shopInventory.SellSingeItem(slot))
            partyInventory.DiscardSingleItem(slot);
    }

    void ActivateSellbackPanel(InventorySlot slot)
    {
        sellBackPanel.gameObject.SetActive(true);
    }
    void DeactivateSellbackPanel(InventorySlot slot)
    {
        sellBackPanel.gameObject.SetActive(false);
    }

    void ShopInventoryPurchase(ShopSlot slot, InventorySlot dropSlot)
    {
        if (shopInventory.BuyShopSlot(slot, partyInventory))
            partyInventory.DistributeFromShopItem(slot, dropSlot);
    }
    void ShopInventorySellBackSlot(InventorySlot slot)
    {
        int itemsSold = shopInventory.SellSlotBack(slot);

        if (itemsSold != 0)
            slot.SlotItem.RemoveItems(itemsSold);
    }

    public void SellOutEverything()
    {
        foreach (var slot in partyInventory.InventorySlots)
        {
            int itemsSold = shopInventory.SellSlotBack(slot);

            if (itemsSold != 0)
                slot.SlotItem.RemoveItems(itemsSold);
        }
    }

    void Awake()
    {
        shopInventory.onPurchase += ShopInventoryPurchase;
        shopInventory.onSellBackSlot += ShopInventorySellBackSlot;
    }

    public void LoadInitialSetup(Quest quest, RaidPartyPanel raidParty)
    {
        shopInventory.UpdateShop(quest);
        partyInventory.LoadInitialSetup(quest, raidParty);
    }
}