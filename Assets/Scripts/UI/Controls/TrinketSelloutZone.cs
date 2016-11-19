using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class TrinketSelloutZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        var trinketDrop = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (trinketDrop == null || trinketDrop.Slot.Inventory.Configuration != InventoryConfiguration.TrinketInventory)
            return;

        trinketDrop.Slot.ItemDroppedOut(trinketDrop.Slot, trinketDrop);
        DarkestDungeonManager.Campaign.Estate.AddGold(Mathf.RoundToInt(trinketDrop.ItemData.PurchasePrice * 0.15f));
        EstateSceneManager.Instanse.currencyPanel.CurrencyIncreased("gold");
        EstateSceneManager.Instanse.currencyPanel.UpdateCurrency();
        trinketDrop.Delete();
#endif
    }
}