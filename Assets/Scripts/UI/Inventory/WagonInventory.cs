using UnityEngine;
using System.Collections.Generic;

public delegate void TrinketSellEvent(Trinket trinket);

public class WagonInventory : MonoBehaviour
{
    [SerializeField]
    private EstateCurrencyPanel currencyPanel;
    [SerializeField]
    private List<WagonSlot> wagonSlots;

    public NomadWagon NomadWagon { private get; set; }

    public event TrinketSellEvent EventTrinketSell;

    private void Awake()
    {
        for (int i = 0; i < wagonSlots.Count; i++)
            wagonSlots[i].EventSlotPurchase += WagonSlotPurchased;
    }

    public void UpdateShop()
    {
        float eventDiscount = DarkestDungeonManager.Campaign.EventModifiers.UpgradeTagDiscount("trinket");

        int slotsFilled = 0;
        for (int i = 0; i < NomadWagon.Trinkets.Count; i++)
        {
            wagonSlots[i].gameObject.SetActive(true);
            wagonSlots[i].SetTrinket(NomadWagon.Trinkets[i], NomadWagon.Discount + eventDiscount);
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

    private bool CheckPrice(WagonSlot slot)
    {
        return DarkestDungeonManager.Campaign.Estate.Currencies["gold"] >= slot.Cost;
    }

    private bool BuyShopSlot(WagonSlot slot)
    {
        if (!CheckPrice(slot))
            return false;
        DarkestDungeonManager.Campaign.Estate.Currencies["gold"] -= slot.Cost;
        currencyPanel.CurrencyDecreased("gold");
        currencyPanel.UpdateCurrency();
        NomadWagon.Trinkets.Remove(slot.Trinket);
        return true;
    }

    private void WagonSlotPurchased(WagonSlot slot)
    {
        if (BuyShopSlot(slot))
        {
            if (EventTrinketSell != null)
                EventTrinketSell(slot.Trinket);

            slot.EmptySlot();
        }
    }
}
