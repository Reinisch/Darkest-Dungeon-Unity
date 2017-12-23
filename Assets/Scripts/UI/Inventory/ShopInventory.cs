using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopInventory : MonoBehaviour
{
    [SerializeField]
    private EstateCurrencyPanel currencyPanel;
    [SerializeField]
    private Text raidLocation;
    [SerializeField]
    private Text raidLongetivity;
    [SerializeField]
    private Text raidDifficulty;

    private List<ShopSlot> ShopSlots { get; set; }

    public event Action<ShopSlot, InventorySlot> EventPurchase;
    public event Action<InventorySlot> EventSellBackSlot;

    private void Awake()
    {
        ShopSlots = new List<ShopSlot>(GetComponentsInChildren<ShopSlot>());
        for (int i = 0; i < ShopSlots.Count; i++)
            ShopSlots[i].EventShopSlotPurchased += ShopSlotPurchased;
    }

    public void PartySlotSoldBack(InventorySlot sellSlot)
    {
        if (EventSellBackSlot != null)
            EventSellBackSlot(sellSlot);
    }

    public bool BuyShopSlot(ShopSlot slot, IInventory inventory)
    {
        if (slot.Item.Amount <= 0 || !CheckPrice(slot) || !CheckInventorySpace(slot, inventory))
            return false;
        DarkestDungeonManager.Campaign.Estate.Currencies["gold"] -= slot.Cost;
        slot.Item.Amount--;
        slot.UpdateAmount();
        currencyPanel.CurrencyDecreased("gold");
        currencyPanel.UpdateCurrency();
        return true;
    }

    public void UpdateShop(Quest currentQuest)
    {
        raidLocation.text = LocalizationManager.GetString("dungeon_name_" + currentQuest.Dungeon);
        raidLongetivity.text = LocalizationManager.GetString("town_quest_length_" + currentQuest.Length.ToString()) + " | ";
        raidDifficulty.text = LocalizationManager.GetString("town_quest_difficulty_" + currentQuest.Difficulty.ToString());
        Color difficultyColor;
        if (ColorUtility.TryParseHtmlString(DarkestDungeonManager.Data.HexColors["town_quest_difficulty_" +
            currentQuest.Difficulty.ToString()], out difficultyColor))
            raidDifficulty.color = difficultyColor;

        int slotsFilled = 0;
        List<ItemDefinition> shopItems = DarkestDungeonManager.Data.Provision.ShopLengthInventories[currentQuest.Length];
        for (int i = 0; i < shopItems.Count; i++)
        {
            ShopSlots[i].gameObject.SetActive(true);
            ShopSlots[i].SetItem(shopItems[i]);
            slotsFilled++;
        }

        for (int i = slotsFilled; i < ShopSlots.Count; i++)
        {
            ShopSlots[i].gameObject.SetActive(false);
        }
    }

    public int SellSlotBack(InventorySlot slot)
    {
        if (slot.SlotItem.Amount <= 0)
            return 0;

        var thisItemShopSlot = ShopSlots.Find(shopSlot =>
            shopSlot.gameObject.activeSelf && shopSlot.InventoryItem == slot.SlotItem.ItemData);
        if (thisItemShopSlot == null || thisItemShopSlot.InitialAmount == thisItemShopSlot.Item.Amount)
            return 0;

        var itemsSold = Mathf.Min(thisItemShopSlot.InitialAmount - thisItemShopSlot.Item.Amount, slot.SlotItem.Item.Amount);
        thisItemShopSlot.Item.Amount += itemsSold;
        thisItemShopSlot.UpdateAmount();

        DarkestDungeonManager.Campaign.Estate.Currencies["gold"] += itemsSold * thisItemShopSlot.Cost;
        currencyPanel.CurrencyIncreased("gold");
        currencyPanel.UpdateCurrency();
        return itemsSold;
    }

    public bool SellSingeItem(InventorySlot slot)
    {
        if (slot.SlotItem.Amount > 0)
        {
            var thisItemShopSlot = ShopSlots.Find(shopSlot =>
                shopSlot.gameObject.activeSelf && shopSlot.InventoryItem == slot.SlotItem.ItemData);
            if (thisItemShopSlot == null || thisItemShopSlot.InitialAmount == thisItemShopSlot.Item.Amount)
                return false;
            else
            {
                thisItemShopSlot.Item.Amount++;
                thisItemShopSlot.UpdateAmount();
            }
            DarkestDungeonManager.Campaign.Estate.Currencies["gold"] += thisItemShopSlot.Cost;
            currencyPanel.CurrencyIncreased("gold");
            currencyPanel.UpdateCurrency();
            return true;
        }
        return false;
    }

    private void ShopSlotPurchased(ShopSlot slot, InventorySlot dropSlot)
    {
        if (EventPurchase != null)
            EventPurchase(slot, dropSlot);
    }

    private bool CheckPrice(ShopSlot slot)
    {
        return DarkestDungeonManager.Campaign.Estate.Currencies["gold"] >= slot.Cost;
    }

    private bool CheckInventorySpace(ShopSlot slot, IInventory inventory)
    {
        return inventory.CheckSingleInventorySpace(slot.Item);
    }
}