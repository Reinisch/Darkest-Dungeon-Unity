using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragManager : MonoBehaviour
{
    public static DragManager Instanse { get; private set; }

    [SerializeField]
    private DragItemHolder heroHolder;
    [SerializeField]
    private DragItemHolder inventoryItemHolder;

    public RectTransform OverlayRect { get; set; }
    public HeroSlot HeroItem { get; private set; }
    public Camera OverlayCamera { private get; set; }
    public InventoryItem PartySlotItem { get; private set; }

    public event Action<RaidPartySlot, HeroSlot> EventStartDraggingPartyHero;
    public event Action<RaidPartySlot, HeroSlot> EventEndDraggingPartyHero;

    public event Action<InventorySlot> EventStartDraggingInventorySlot;
    public event Action<InventorySlot> EventEndDraggingInventorySlot;

    private Vector2 currentDrahPosition;

    private void Awake()
    {
        if (Instanse == null)
            Instanse = this;
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
        DarkestSoundManager.PlayOneShot("event:/ui/town/character_pickup");

        UpdateHeroItemPosition(eventData);
        LoadHeroItem(heroSlot);

        if (EventStartDraggingPartyHero != null)
            EventStartDraggingPartyHero(null, heroSlot);

        HeroItem.SlotController.SetBool("isHidden", true);

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

        if (EventStartDraggingInventorySlot != null)
            EventStartDraggingInventorySlot(slotItem.Slot);
    }

    public void EndDragging(HeroSlot heroSlot, PointerEventData eventData, bool fromRaidPanel = false)
    {
        if (EventEndDraggingPartyHero != null)
            EventEndDraggingPartyHero(null, null);
        HeroItem.SlotController.SetBool("isHidden", false);

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

        if (EventEndDraggingInventorySlot != null)
            EventEndDraggingInventorySlot(slotItem.Slot);
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

    private void UpdateHeroItemPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(OverlayRect, eventData.position, OverlayCamera, out currentDrahPosition);
        heroHolder.RectTransform.localPosition = currentDrahPosition;
    }

    private void UpdateSlotItemPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(OverlayRect, eventData.position, OverlayCamera, out currentDrahPosition);
        inventoryItemHolder.RectTransform.localPosition = currentDrahPosition;
    }

    private void LoadHeroItem(HeroSlot heroSlot)
    {
        HeroItem = heroSlot;
        HeroItem.CopyToDragItem(heroHolder);
        heroHolder.BackIcon.enabled = false;
        heroHolder.gameObject.SetActive(true);
    }

    private void LoadHeroItem(RecruitSlot recruitSlot)
    {
        heroHolder.ItemIcon.sprite = recruitSlot.HeroPortrait.sprite;
        heroHolder.BackIcon.enabled = false;
        heroHolder.gameObject.SetActive(true);
    }

    private void LoadPartySlotItem(InventoryItem slotItem)
    {
        PartySlotItem = slotItem;
        PartySlotItem.CopyToDragItem(inventoryItemHolder);
        inventoryItemHolder.gameObject.SetActive(true);
    }
}
