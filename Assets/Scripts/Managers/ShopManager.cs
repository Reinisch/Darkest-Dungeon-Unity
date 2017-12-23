using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private ShopInventory shopInventory;
    [SerializeField]
    private PartyInventory partyInventory;
    [SerializeField]
    private ItemSellbackPanel sellBackPanel;

    public PartyInventory PartyInventory { get { return partyInventory; } }

    private void Awake()
    {
        shopInventory.EventPurchase += ShopInventoryPurchase;
        shopInventory.EventSellBackSlot += ShopInventorySellBackSlot;
    }

    public void ActivateShopBehaviour()
    {
        DragManager.Instanse.EventStartDraggingInventorySlot += ActivateSellbackPanel;
        DragManager.Instanse.EventEndDraggingInventorySlot += DeactivateSellbackPanel;
        foreach(var slot in partyInventory.InventorySlots)
            slot.EventActivate += SellSingleItem;
    }

    public void DeactivateShopBehaviour()
    {
        DragManager.Instanse.EventStartDraggingInventorySlot -= ActivateSellbackPanel;
        DragManager.Instanse.EventEndDraggingInventorySlot -= DeactivateSellbackPanel;
        foreach (var slot in partyInventory.InventorySlots)
            slot.EventActivate -= SellSingleItem;
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

    public void LoadInitialSetup(Quest quest, RaidPartyPanel raidParty)
    {
        shopInventory.UpdateShop(quest);
        partyInventory.LoadInitialSetup(quest, raidParty);
    }

    private void SellSingleItem(InventorySlot slot)
    {
        if (shopInventory.SellSingeItem(slot))
            partyInventory.DiscardSingleItem(slot);
    }

    private void ActivateSellbackPanel(InventorySlot slot)
    {
        sellBackPanel.gameObject.SetActive(true);
    }

    private void DeactivateSellbackPanel(InventorySlot slot)
    {
        sellBackPanel.gameObject.SetActive(false);
    }

    private void ShopInventoryPurchase(ShopSlot slot, InventorySlot dropSlot)
    {
        if (shopInventory.BuyShopSlot(slot, partyInventory))
            partyInventory.DistributeFromShopItem(slot, dropSlot);
    }

    private void ShopInventorySellBackSlot(InventorySlot slot)
    {
        int itemsSold = shopInventory.SellSlotBack(slot);

        if (itemsSold != 0)
            slot.SlotItem.RemoveItems(itemsSold);
    }
}