using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InteractionSlot : BaseSlot, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Image itemIcon;

    public ItemData Item { get; set; }
    public Image ItemIcon { get { return itemIcon; } }
    public bool IsItemAllowed { private get; set; }
    public bool IsItemFixed { private get; set; }

    public event Action<InventoryItem> EventDropIn;
    public event Action<ItemData> EventActivate;

    public void Reset()
    {
        Item = null;
        ItemIcon.sprite = null;
        ItemIcon.enabled = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (DragManager.Instanse.PartySlotItem != null)
        {
            if (IsItemFixed)
            {
                if(Item != null && Item == DragManager.Instanse.PartySlotItem.ItemData)
                    if (EventDropIn != null)
                        EventDropIn(DragManager.Instanse.PartySlotItem);
            }
            else if (IsItemAllowed)
            {
                if (DragManager.Instanse.PartySlotItem.ItemData.Type == "supply")
                {
                    Item = DragManager.Instanse.PartySlotItem.ItemData;
                    if (EventDropIn != null)
                        EventDropIn(DragManager.Instanse.PartySlotItem);
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item != null)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
            ToolTipManager.Instanse.Show(Item.ToolTip(), rectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
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
            if (EventActivate != null && Item != null)
                EventActivate(Item);
        }
    }
}