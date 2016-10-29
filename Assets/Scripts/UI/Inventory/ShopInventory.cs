using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public delegate void ShopSellEvent(InventorySlot slot);

public class ShopInventory : MonoBehaviour
{
    public EstateCurrencyPanel currencyPanel;
    public Text raidLocation;
    public Text raidLongetivity;
    public Text raidDifficulty;

    public List<ShopSlot> ShopSlots { get; set; }

    public event ShopSlotEvent onPurchase;
    public event ShopSellEvent onSellBackSlot;

    public void ShopSlotPurchased(ShopSlot slot, InventorySlot dropSlot)
    {
        if (onPurchase != null)
            onPurchase(slot, dropSlot);
    }
    public void PartySlotSoldBack(InventorySlot sellSlot)
    {
        if (onSellBackSlot != null)
            onSellBackSlot(sellSlot);
    }

    void Awake()
    {
        ShopSlots = new List<ShopSlot>(GetComponentsInChildren<ShopSlot>());
        for (int i = 0; i < ShopSlots.Count; i++)
        {
            ShopSlots[i].InventoryShop = this;
            ShopSlots[i].onShopSlotPurchase += ShopSlotPurchased;
        }
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

    public bool CheckPrice(ShopSlot slot)
    {
        return DarkestDungeonManager.Campaign.Estate.Currencies["gold"].amount >= slot.Cost;
    }

    public bool CheckInventorySpace(ShopSlot slot, IInventory inventory)
    {
        return inventory.CheckSingleInventorySpace(slot.Item);
    }

    public bool BuyShopSlot(ShopSlot slot, IInventory inventory)
    {
        if (slot.Item.Amount <= 0 || !CheckPrice(slot) || !CheckInventorySpace(slot, inventory))
            return false;
        DarkestDungeonManager.Campaign.Estate.Currencies["gold"].amount -= slot.Cost;
        slot.Item.Amount--;
        slot.UpdateAmount();
        currencyPanel.CurrencyDecreased("gold");
        currencyPanel.UpdateCurrency();
        return true;
    }

    public int SellSlotBack(InventorySlot slot)
    {
        if (slot.SlotItem.Amount > 0)
        {
            int itemsSold = 0;

            var thisItemShopSlot = ShopSlots.Find(shopSlot =>
                shopSlot.gameObject.activeSelf && shopSlot.InventoryItem == slot.SlotItem.ItemData);
            if (thisItemShopSlot == null || thisItemShopSlot.InitialAmount == thisItemShopSlot.Item.Amount)
                return 0;
            else
            {
                itemsSold = Mathf.Min(thisItemShopSlot.InitialAmount - thisItemShopSlot.Item.Amount, slot.SlotItem.Item.Amount);
                thisItemShopSlot.Item.Amount += itemsSold;
                thisItemShopSlot.UpdateAmount();
            }

            DarkestDungeonManager.Campaign.Estate.Currencies["gold"].amount += itemsSold * thisItemShopSlot.Cost;
            currencyPanel.CurrencyIncreased("gold");
            currencyPanel.UpdateCurrency();
            return itemsSold;
        }
        return 0;
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
            DarkestDungeonManager.Campaign.Estate.Currencies["gold"].amount += thisItemShopSlot.Cost;
            currencyPanel.CurrencyIncreased("gold");
            currencyPanel.UpdateCurrency();
            return true;
        }
        return false;
    }
}