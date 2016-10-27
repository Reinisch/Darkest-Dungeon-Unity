using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void ItemInteractionEvent(InventoryItem item);
public delegate void InteractionActivationEvent(ItemData item);

public class InteractionSlot : BaseSlot, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public RectTransform rectTransform;
    public Image slotBackground;
    public Image itemIcon;

    public ItemData Item { get; set; }
    public bool IsItemAllowed { get; set; }
    public bool IsItemFixed { get; set; }

    public event ItemInteractionEvent onDropIn;
    public event InteractionActivationEvent onActivate;

    public void Reset()
    {
        Item = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (DragManager.Instanse.PartySlotItem != null)
        {
            if (IsItemFixed)
            {
                if(Item != null && Item == DragManager.Instanse.PartySlotItem.ItemData)
                    if (onDropIn != null)
                        onDropIn(DragManager.Instanse.PartySlotItem);
            }
            else if (IsItemAllowed)
            {
                if (DragManager.Instanse.PartySlotItem.ItemData.Type == "supply")
                {
                    Item = DragManager.Instanse.PartySlotItem.ItemData;
                    if (onDropIn != null)
                        onDropIn(DragManager.Instanse.PartySlotItem);
                }
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item != null)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
            ToolTipManager.Instanse.Show(Item.ToolTip(), eventData, rectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsItemFixed)
        {
            if (onActivate != null && Item != null)
                onActivate(Item);
        }
    }
}