using UnityEngine;
using UnityEngine.EventSystems;

public class TrinketSelloutZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        var trinketDrop = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (trinketDrop == null || trinketDrop.Slot.Inventory.Configuration != InventoryConfiguration.TrinketInventory)
            return;

        trinketDrop.Slot.ItemDroppedOut(trinketDrop);
        DarkestDungeonManager.Campaign.Estate.AddGold(Mathf.RoundToInt(trinketDrop.ItemData.PurchasePrice * 0.15f));
        EstateSceneManager.Instanse.CurrencyPanel.CurrencyIncreased("gold");
        EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
        trinketDrop.Delete();
#endif
    }
}