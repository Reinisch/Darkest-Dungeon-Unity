using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HeroObserverSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [SerializeField]
    private Image slotIcon;

    private Hero ObservedHero { get; set; }

    public event Action<Hero> EventHeroDropped;
    public event Action<Hero> EventHeroRemoved;

    public void ClearSlot()
    {
        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.background"];
        if (EventHeroRemoved != null)
            EventHeroRemoved(ObservedHero);
        ObservedHero = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var heroSlot = eventData.pointerDrag.GetComponent<HeroSlot>();
        if(heroSlot == null)
            return;

        slotIcon.sprite = DarkestDungeonManager.HeroSprites[heroSlot.Hero.ClassStringId]["A"].Portrait;
        ObservedHero = heroSlot.Hero;
        if (EventHeroDropped != null)
            EventHeroDropped(heroSlot.Hero);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;

        ClearSlot();
    }
}
