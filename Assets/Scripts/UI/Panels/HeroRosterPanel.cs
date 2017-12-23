using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public delegate void HeroInspectEvent(Hero hero, bool interactable);
public delegate void HeroSlotEvent(HeroSlot heroSlot);
public delegate void HeroResurrectionEvent(DeathRecord record);

public class HeroRosterPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [SerializeField]
    private GameObject heroSlotTemplate;
    [SerializeField]
    private HeroSlot placeHolder;
    [SerializeField]
    private Text capacityLabel;
    [SerializeField]
    private RectTransform rosterLive;

    public HeroSlot PlaceHolder { get { return placeHolder; } }
    public RectTransform RosterLive { get { return rosterLive; } }
    public List<HeroSlot> HeroSlots { get; private set; }
    public bool Hovered { get; private set; }
    public bool Dragging { private get; set; }

    private Transform rosterSlots;

    public event HeroInspectEvent EventHeroInspected;
    public event HeroSlotEvent EventHeroSlotBeginDragging;
    public event HeroSlotEvent EventHeroSlotEndDragging;
    public event HeroResurrectionEvent EventHeroResurrected;

    private void Awake()
    {
        rosterSlots = transform.Find("RosterScroll").Find("Viewport").Find("RosterSlots");
#if !(UNITY_ANDROID || UNITY_IOS)
        var scrollRect = GetComponentInChildren<ScrollRect>();
        scrollRect.verticalScrollbar.gameObject.SetActive(false);
#endif
    }

    private void Start()
    {
        PlaceHolder.SlotController.SetTrigger("hide");
        PlaceHolder.SlotController.SetBool("isHidden", true);
        PlaceHolder.HeroRoster = this;
        PlaceHolder.RectTransform.SetAsLastSibling();
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

        if (deathRecord != null && EventHeroResurrected != null)
            EventHeroResurrected(deathRecord);
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
        if (EventHeroSlotBeginDragging != null)
            EventHeroSlotBeginDragging(slot);
    }

    public void HeroSlotEndDragging(HeroSlot slot)
    {
        if (EventHeroSlotEndDragging != null)
            EventHeroSlotEndDragging(slot);
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

    #region Roster Panel Actions

    public void SortByBuilding()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        Comparison<Hero> sorting = (x, y) =>
        {
            int result = -x.Status.CompareTo(y.Status);
            return result == 0 ? string.Compare(x.Name, y.Name, StringComparison.Ordinal) : result;
        };

        DarkestDungeonManager.Campaign.Heroes.Sort(sorting);
        HeroSlots.Sort((x, y) => sorting(x.Hero, y.Hero));

        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }

    public void SortByClass()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        Comparison<Hero> sorting = (x, y) =>
        {
            int result = string.Compare(x.Class, y.Class, StringComparison.Ordinal);
            return result == 0 ? string.Compare(x.Name, y.Name, StringComparison.Ordinal) : result;
        };

        DarkestDungeonManager.Campaign.Heroes.Sort(sorting);
        HeroSlots.Sort((x, y) => sorting(x.Hero, y.Hero));

        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }

    public void SortByStress()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        Comparison<Hero> sorting = (x, y) =>
        {
            int result = x.Stress.CurrentValue.CompareTo(y.Stress.CurrentValue);
            return result == 0 ? string.Compare(x.Name, y.Name, StringComparison.Ordinal) : result;
        };

        DarkestDungeonManager.Campaign.Heroes.Sort(sorting);
        HeroSlots.Sort((x, y) => sorting(x.Hero, y.Hero));

        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }

    public void SortByLevel()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        Comparison<Hero> sorting = (x, y) =>
        {
            int result = -x.Resolve.Level.CompareTo(y.Resolve.Level);
            return result == 0 ? string.Compare(x.Name, y.Name, StringComparison.Ordinal) : result;
        };

        DarkestDungeonManager.Campaign.Heroes.Sort(sorting);
        HeroSlots.Sort((x, y) => sorting(x.Hero, y.Hero));

        for (int i = 0; i < HeroSlots.Count; i++)
            HeroSlots[i].RectTransform.SetSiblingIndex(i);
    }

    #endregion

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
        if (EventHeroInspected != null)
            EventHeroInspected(heroSlot.Hero, true);
    }

    public void RecruitSlotClicked(RecruitSlot recruitSlot)
    {
        if (EventHeroInspected != null)
            EventHeroInspected(recruitSlot.Hero, false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovered = true;
        if(Dragging)
            PlaceHolder.SlotController.SetBool("isHidden", false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hovered = false;
        if(Dragging)
            PlaceHolder.SlotController.SetBool("isHidden", true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!eventData.pointerDrag.CompareTag("Recruit"))
            return;

        var recruitSlot = eventData.pointerDrag.GetComponent<RecruitSlot>();
        if (CreateSlot(recruitSlot, null) != null)
        {
            recruitSlot.OnEndDrag(eventData);
            recruitSlot.RemoveSlot();
        }
    }

    private HeroSlot CreateSlot(Hero hero)
    {
        GameObject newSlotObject = Instantiate(heroSlotTemplate);
        newSlotObject.transform.SetParent(rosterSlots, false);

        HeroSlot heroSlot = newSlotObject.GetComponent<HeroSlot>();
        heroSlot.HeroRoster = this;
        heroSlot.Hero = hero;
        heroSlot.UpdateSlot();
        HeroSlots.Add(heroSlot);
        return heroSlot;
    }
}
