using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSellbackPanel : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private ShopInventory shop;

    public void OnDrop(PointerEventData eventData)
    {
        if(DragManager.Instanse.PartySlotItem != null)
            DragManager.Instanse.SellBackSlotItem(shop);
    }
}
