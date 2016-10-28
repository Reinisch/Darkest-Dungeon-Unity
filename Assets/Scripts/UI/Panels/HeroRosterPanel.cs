using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public delegate void HeroInspectEvent(Hero hero, bool interactable);
public delegate void HeroSlotEvent(HeroSlot heroSlot);
public delegate void HeroResurrectionEvent(DeathRecord record);

public class HeroRosterPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    Transform rosterSlots;

    public GameObject heroSlotTemplate;
    public HeroSlot placeHolder;
    public Text capacityLabel;
    public RectTransform rosterLive;

    public int CurrentRosterCount { get; set; }
    public List<HeroSlot> HeroSlots { get; private set; }
    public bool Hovered { get; set; }
    public bool Dragging { get; set; }

    public event HeroInspectEvent onHeroInspect;
    public event HeroSlotEvent onHeroSlotBeginDragging;
    public event HeroSlotEvent onHeroSlotEndDragging;
    public event HeroResurrectionEvent onHeroResurrection;

    void Awake()
    {
        rosterSlots = transform.FindChild("RosterScroll").FindChild("Viewport").FindChild("RosterSlots");
    }
    void Start()
    {
        placeHolder.slotController.SetTrigger("hide");
        placeHolder.slotController.SetBool("isHidden", true);
        placeHolder.HeroRoster = this;
        placeHolder.RectTransform.SetAsLastSibling();
    }

    HeroSlot CreateSlot(Hero hero)
    {
        GameObject gameObject = Instantiate(heroSlotTemplate);
        gameObject.transform.SetParent(rosterSlots, false);
        HeroSlot heroSlot = gameObject.GetComponent<HeroSlot>();
        heroSlot.HeroRoster = this;
        heroSlot.Hero = hero;
        heroSlot.UpdateSlot();
        HeroSlots.Add(heroSlot);
        return heroSlot;
    }
    public HeroSlot CreateSlot(RecruitSlot recruitSlot, HeroSlot heroSlot)
    {
        if (DarkestDungeonManager.Campaign.Heroes.Count >= DarkestDungeonManager.Campaign.Estate.StageCoach.RosterSlots)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_add_full");
            return null;
        }

        var newSlot = CreateSlot(recruitSlot.Hero);
        DarkestDungeonManager.Campaign.Heroes.Add(recruitSlot.Hero);
        var deathRecord = DarkestDungeonManager.Campaign.Estate.RecruitHero(recruitSlot.Hero);

        DarkestSoundManager.ExecuteNarration("recruit_hero", NarrationPlace.Town, recruitSlot.Hero.Class);

        if (deathRecord != null && onHeroResurrection != null)
            onHeroResurrection(deathRecord);
        else
        {
            DarkestSoundManager.PlayOneShot("event:/town/stage_coach_purchase");
        }

        if (heroSlot != null)
            newSlot.RectTransform.SetSiblingIndex(heroSlot.RectTransform.GetSiblingIndex());
        else
            newSlot.RectTransform.SetAsLastSibling();
        UpdateCapacity();
        return newSlot;
    }
    public void HeroSlotBeginDragging(HeroSlot slot)
    {
        if (onHeroSlotBeginDragging != null)
            onHeroSlotBeginDragging(slot);
    }
    public void HeroSlotEndDragging(HeroSlot slot)
    {
        if (onHeroSlotEndDragging != null)
            onHeroSlotEndDragging(slot);
    }
    public void DestroySlot(Hero hero)
    {
        var targetSlot = HeroSlots.Find(slot => slot.Hero == hero);
        if (targetSlot != null)
        {
            HeroSlots.Remove(targetSlot);
            Destroy(targetSlot.gameObject);
        }
        UpdateCapacity();
    }

    public void SortByBuilding()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        DarkestDungeonManager.Campaign.Heroes.Sort((x, y) => x.Status.CompareTo(y.Status));
        HeroSlots.Sort((x, y) => x.Hero.Status.CompareTo(y.Hero.Status));
        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }
    public void SortByClass()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        DarkestDungeonManager.Campaign.Heroes.Sort((x, y) => x.Class.CompareTo(y.Class));
        HeroSlots.Sort((x, y) => x.Hero.Class.CompareTo(y.Hero.Class));
        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }
    public void SortByStress()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        DarkestDungeonManager.Campaign.Heroes.Sort((x, y) => x.Stress.CurrentValue.CompareTo(y.Stress.CurrentValue));
        HeroSlots.Sort((x, y) => x.Hero.Stress.CurrentValue.CompareTo(y.Hero.Stress.CurrentValue));
        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }
    public void SortByLevel()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        DarkestDungeonManager.Campaign.Heroes.Sort((x, y) => x.Resolve.Level.CompareTo(y.Resolve.Level));
        HeroSlots.Sort((x, y) => x.Hero.Resolve.Level.CompareTo(y.Hero.Resolve.Level));
        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }

    public void UpdateCapacity()
    {
        if (DarkestDungeonManager.Campaign.Heroes.Count >= DarkestDungeonManager.Campaign.Estate.StageCoach.RosterSlots)
            capacityLabel.text = LocalizationManager.GetString("str_roster_list_full");
        else
            capacityLabel.text = DarkestDungeonManager.Campaign.Heroes.Count.ToString() + 
                "/" + DarkestDungeonManager.Campaign.Estate.StageCoach.RosterSlots.ToString();
    }
    public void InitializeRoster()
    {
        HeroSlots = new List<HeroSlot>();
        for (int i = 0; i < DarkestDungeonManager.Campaign.Heroes.Count; i++ )
            CreateSlot(DarkestDungeonManager.Campaign.Heroes[i]);
        UpdateCapacity();
    }
    public void UpdateRoster()
    {
        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].UpdateSlot();
    }
    public void HeroSlotClicked(HeroSlot heroSlot)
    {
        if (onHeroInspect != null)
            onHeroInspect(heroSlot.Hero, true);
    }
    public void RecruitSlotClicked(RecruitSlot recruitSlot)
    {
        if (onHeroInspect != null)
            onHeroInspect(recruitSlot.Hero, false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovered = true;
        if(Dragging)
            placeHolder.slotController.SetBool("isHidden", false);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Hovered = false;
        if(Dragging)
            placeHolder.slotController.SetBool("isHidden", true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.tag == "Recruit")
        {
            var recruitSlot = eventData.pointerDrag.GetComponent<RecruitSlot>();
            if (CreateSlot(recruitSlot, null) != null)
            {
                recruitSlot.OnEndDrag(eventData);
                recruitSlot.RemoveSlot();
            }
            else
            {
                //recruitSlot.OnEndDrag(eventData);
            }
        }
    }
}
