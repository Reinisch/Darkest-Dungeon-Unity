using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public delegate void HeroObserveEvent(Hero hero);

public class HeroObserverSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public Image slotIcon;

    public event HeroObserveEvent OnHeroDropped;
    public event HeroObserveEvent OnHeroRemoved;

    public Hero ObservedHero { get; set; }

    public void ClearSlot()
    {
        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.background"];
        if (OnHeroRemoved != null)
            OnHeroRemoved(ObservedHero);
        ObservedHero = null;
    }
    public void OnDrop(PointerEventData eventData)
    {
        var heroSlot = eventData.pointerDrag.GetComponent<HeroSlot>();
        if(heroSlot == null)
            return;

        slotIcon.sprite = DarkestDungeonManager.HeroSprites[heroSlot.Hero.ClassStringId]["A"].Portrait;
        ObservedHero = heroSlot.Hero;
        if (OnHeroDropped != null)
            OnHeroDropped(heroSlot.Hero);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;

        ClearSlot();
    }
}
