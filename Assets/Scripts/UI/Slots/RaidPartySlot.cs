using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaidPartySlot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler
{
    [SerializeField]
    private Image heroFrame;

    public Animator SlotAnimator { get; private set; }
    public HeroSlot SelectedHero { get; private set; }

    private bool isDragging;

    public event Action<HeroSlot> EventDropOut;
    public event Action<HeroSlot> EventDropIn;
    public Func<HeroSlot, bool> CompatibilityCheck;

    private void Awake()
    {
        SlotAnimator = GetComponent<Animator>();
    }

    public void MarkSlots(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        if(SelectedHero == null || SelectedHero.Hero.RosterId == heroSlot.Hero.RosterId)
        {
            if(RaidPartyPanel.IsResolveEligible(heroSlot.Hero))
                SlotAnimator.SetBool("marked", true);
        }
    }

    public void UnmarkSlots(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        SlotAnimator.SetBool("marked", false);
    }

    public void ItemDroppedIn(HeroSlot heroSlot)
    {
        SelectedHero = heroSlot;
        heroSlot.PartySlot = this;
        SlotAnimator.SetBool("empty", false);
        SlotAnimator.SetBool("marked", false);
        SlotAnimator.SetBool("locked", false);
        heroFrame.sprite = heroSlot.Portrait.sprite;
        heroSlot.SetStatus(HeroStatus.RaidParty);
        if (EventDropIn != null)
            EventDropIn(heroSlot);
    }

    public void ItemDroppedOut(HeroSlot heroSlot)
    {
        SelectedHero = null;
        heroSlot.PartySlot = null;
        SlotAnimator.SetBool("empty", true);
        SlotAnimator.SetBool("marked", false);
        SlotAnimator.SetBool("locked",false);
        heroSlot.SetStatus(HeroStatus.Available);
        if (EventDropOut != null)
            EventDropOut(heroSlot);

        DarkestSoundManager.PlayOneShot("event:/ui/town/character_remove");
    }

    public void ItemSwapped(HeroSlot heroSlot)
    {
        SelectedHero = heroSlot;
        heroSlot.PartySlot = this;
        heroFrame.sprite = heroSlot.Portrait.sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SelectedHero == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
            return;

        isDragging = true;
        Debug.Log("Begin Drag from panel.");
        DragManager.Instanse.StartDragging(SelectedHero, eventData, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
            return;
        DragManager.Instanse.OnDrag(SelectedHero, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
            return;
        isDragging = false;
        Debug.Log("End Drag from panel.");
        DragManager.Instanse.EndDragging(SelectedHero, eventData,true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var droppedItem = DragManager.Instanse.HeroItem;
        if (droppedItem == null || DarkestDungeonManager.RaidManager.Quest == null)
            return;

        if (!RaidPartyPanel.IsResolveEligible(DragManager.Instanse.HeroItem.Hero))
        {
            // Implement bark
            return;
        }

        if (!CompatibilityCheck(DragManager.Instanse.HeroItem))
        {
            // Implement bark
            return;
        }

        DarkestSoundManager.PlayOneShot("event:/ui/town/character_add");

        if (SelectedHero == null)
        {
            if (droppedItem.PartySlot != null)
            {
                droppedItem.PartySlot.ItemDroppedOut(droppedItem);
                ItemDroppedIn(droppedItem);
            }
            else
                ItemDroppedIn(droppedItem);
        }
        else
        {
            if (droppedItem.PartySlot != null)
            {
                droppedItem.PartySlot.ItemSwapped(SelectedHero);
                ItemSwapped(droppedItem);
            }
            else
            {
                ItemDroppedOut(SelectedHero);
                ItemDroppedIn(droppedItem);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(SelectedHero != null)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
    }
}