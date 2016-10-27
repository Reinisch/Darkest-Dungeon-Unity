using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public delegate void ShopSlotEvent(ShopSlot slot, InventorySlot dropSlot);

public class ShopSlot : BaseSlot, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon;
    public Text amountLabel;
    public Text costText;

    public int Cost { get; set; }
    public int InitialAmount { get; private set; }
    public ItemDefinition Item { get; set; }
    public ItemData InventoryItem { get; set; }
    public ShopInventory InventoryShop { get; set; }

    public event ShopSlotEvent onShopSlotPurchase;

    public void SetItem(ItemDefinition newItem)
    {
        int amount = Mathf.CeilToInt(newItem.Amount *
            DarkestDungeonManager.Campaign.EventModifiers.ProvisionAmountModifier(newItem.Type));

        Item = new ItemDefinition(newItem.Type, newItem.Id, amount);
        InitialAmount = amount;

        InventoryItem = DarkestDungeonManager.Data.Items[Item.Type][Item.Id];
        if (Item.Type == "gold" || Item.Type == "provision")
        {
            itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id + "_1"];
        }
        else
        {
            itemIcon.sprite = DarkestDungeonManager.Data.Sprites["inv_" + Item.Type + "+" + Item.Id];
        }
        Cost = (int) (InventoryItem.PurchasePrice *
            DarkestDungeonManager.Campaign.EventModifiers.ProvisionCostModifier(newItem.Type));
        costText.text = Cost.ToString();
        amountLabel.text = InitialAmount.ToString();
    }
    public void UpdateAmount()
    {
        amountLabel.text = Item.Amount.ToString();
    }

    public virtual void CopyToDragItem(DragItemHolder dragItem)
    {
        dragItem.backIcon.enabled = false;
        dragItem.itemIcon.sprite = itemIcon.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item != null)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
            ToolTipManager.Instanse.Show(Item.ToolTip, eventData, RectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }

    public void SlotClicked()
    {
        if (onShopSlotPurchase != null)
            onShopSlotPurchase(this, null);
    }
}
