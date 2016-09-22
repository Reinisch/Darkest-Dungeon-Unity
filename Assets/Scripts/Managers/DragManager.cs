using UnityEngine;
using UnityEngine.EventSystems;

public delegate void DragHeroRaidPartyEvent(RaidPartySlot partySlot, HeroSlot heroSlot);
public delegate void DragPartySlotEvent(InventorySlot slot);

public class DragManager : MonoBehaviour
{
    public static DragManager Instanse { get; private set; }

    public RectTransform OverlayRect { get; set; }
    public Camera OverlayCamera { get; set; }

    public DragItemHolder heroHolder;
    public DragItemHolder inventoryItemHolder;

    public InventoryItem PartySlotItem { get; set; }
    public HeroSlot HeroItem { get; set; }

    public event DragHeroRaidPartyEvent onStartDraggingPartyHero;
    public event DragHeroRaidPartyEvent onEndDraggingPartyHero;

    public event DragPartySlotEvent onStartDraggingInventorySlot;
    public event DragPartySlotEvent onEndDraggingInventorySlot;

    Vector2 position;

    void Awake()
    {
        if (Instanse == null)
            Instanse = this;
    }

    void UpdateHeroItemPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(OverlayRect, eventData.position, OverlayCamera, out position);
        heroHolder.rectTransform.localPosition = position;
    }
    void UpdateSlotItemPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(OverlayRect, eventData.position, OverlayCamera, out position);
        inventoryItemHolder.rectTransform.localPosition = position;
    }

    void LoadHeroItem(HeroSlot heroSlot)
    {
        HeroItem = heroSlot;
        HeroItem.CopyToDragItem(heroHolder);
        heroHolder.backIcon.enabled = false;
        heroHolder.gameObject.SetActive(true);
    }
    void LoadHeroItem(RecruitSlot recruitSlot)
    {
        heroHolder.itemIcon.sprite = recruitSlot.heroPortrait.sprite;
        heroHolder.backIcon.enabled = false;
        heroHolder.gameObject.SetActive(true);
    }
    void LoadPartySlotItem(InventoryItem slotItem)
    {
        PartySlotItem = slotItem;
        PartySlotItem.CopyToDragItem(inventoryItemHolder);
        inventoryItemHolder.gameObject.SetActive(true);
    }

    public void OnDrag(HeroSlot heroSlot, PointerEventData eventData)
    {
        UpdateHeroItemPosition(eventData);
    }
    public void OnDrag(RecruitSlot recruitSlot, PointerEventData eventData)
    {
        UpdateHeroItemPosition(eventData);
    }
    public void OnDrag(InventoryItem slotItem, PointerEventData eventData)
    {
        UpdateSlotItemPosition(eventData);
    }

    public void StartDragging(HeroSlot heroSlot, PointerEventData eventData, bool fromRaidPanel = false)
    {
        UpdateHeroItemPosition(eventData);
        LoadHeroItem(heroSlot);

        if (onStartDraggingPartyHero != null)
            onStartDraggingPartyHero(null, heroSlot);

        HeroItem.slotController.SetBool("isHidden", true);

        Cursor.visible = false;    
    }
    public void StartDragging(RecruitSlot recruitSlot, PointerEventData eventData)
    {
        UpdateHeroItemPosition(eventData);
        LoadHeroItem(recruitSlot);
        Cursor.visible = false;
    }
    public void StartDragging(InventoryItem slotItem, PointerEventData eventData)
    {
        LoadPartySlotItem(slotItem);
        UpdateSlotItemPosition(eventData);

        if (onStartDraggingInventorySlot != null)
            onStartDraggingInventorySlot(slotItem.Slot);
    }

    public void EndDragging(HeroSlot heroSlot, PointerEventData eventData, bool fromRaidPanel = false)
    {
        if (onEndDraggingPartyHero != null)
            onEndDraggingPartyHero(null, null);
        HeroItem.slotController.SetBool("isHidden", false);

        heroHolder.gameObject.SetActive(false);
        Cursor.visible = true;
        HeroItem = null;
    }
    public void EndDragging(RecruitSlot recruitSlot, PointerEventData eventData)
    {
        heroHolder.gameObject.SetActive(false);
        Cursor.visible = true;
    }
    public void EndDragging(InventoryItem slotItem, PointerEventData eventData)
    {
        PartySlotItem = null;
        inventoryItemHolder.gameObject.SetActive(false);

        if (onEndDraggingInventorySlot != null)
            onEndDraggingInventorySlot(slotItem.Slot);
    }

    public void DropOutDraggedPartyHero()
    {
        if (HeroItem.PartySlot != null)
            HeroItem.PartySlot.ItemDroppedOut(HeroItem);
    }
    public void SellBackSlotItem(ShopInventory shop)
    {
        shop.PartySlotSoldBack(PartySlotItem.Slot);
    }
}
