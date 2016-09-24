using UnityEngine;
using System.Collections.Generic;

public delegate void WagonSellEvent(WagonSlot slot);
public delegate void TrinketSellEvent(Trinket trinket);

public class WagonInventory : MonoBehaviour
{
    public EstateCurrencyPanel currencyPanel;

    public List<WagonSlot> wagonSlots;

    public event TrinketSellEvent onTrinketSell;

    public NomadWagon NomadWagon { get; set; }

    void Awake()
    {
        for (int i = 0; i < wagonSlots.Count; i++)
        {
            wagonSlots[i].onSlotPurchase += WagonSlotPurchased;
        }
    }
    bool CheckPrice(WagonSlot slot)
    {
        return DarkestDungeonManager.Campaign.Estate.Currencies["gold"].amount >= slot.Cost;
    }
    bool BuyShopSlot(WagonSlot slot)
    {
        if (!CheckPrice(slot))
            return false;
        DarkestDungeonManager.Campaign.Estate.Currencies["gold"].amount -= slot.Cost;
        currencyPanel.CurrencyDecreased("gold");
        currencyPanel.UpdateCurrency();
        NomadWagon.Trinkets.Remove(slot.Trinket);
        return true;
    }

    public void WagonSlotPurchased(WagonSlot slot)
    {
        if (BuyShopSlot(slot))
        {
            if (onTrinketSell != null)
                onTrinketSell(slot.Trinket);

            slot.EmptySlot();
        }
    }

    public void UpdateShop()
    {
        int slotsFilled = 0;
        for (int i = 0; i < NomadWagon.Trinkets.Count; i++)
        {
            wagonSlots[i].gameObject.SetActive(true);
            wagonSlots[i].SetTrinket(NomadWagon.Trinkets[i], NomadWagon.Discount);
            slotsFilled++;
        }

        for (int i = slotsFilled; i < wagonSlots.Count; i++)
        {
            wagonSlots[i].gameObject.SetActive(false);
        }
    }
    public void UpdatePrices()
    {
        float eventDiscount = DarkestDungeonManager.Campaign.EventModifiers.UpgradeTagDiscount("trinket");

        for (int i = 0; i < NomadWagon.Trinkets.Count; i++)
        {
            wagonSlots[i].UpdatePrice(NomadWagon.Discount + eventDiscount);
        }
    }
}
