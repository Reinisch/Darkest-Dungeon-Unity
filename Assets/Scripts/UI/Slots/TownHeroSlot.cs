using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void TownHeroSlotEvent(TownHeroSlot slot);

public class TownHeroSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    public string activityName;

    public Text heroLabel;
    public Text costLabel;
    public Image costIcon;

    public Image eventLockerIcon;
    public Image lockerIcon;
    public Image activeIcon;
    public Image buttonIcon;
    public Image freeIcon;
    public Image slotIcon;

    public event TownHeroSlotEvent onTreatmentButtonClick;

    public ActivitySlot ActivitySlot { get; set; }
    public RectTransform RectTransform { get; set; }
    public bool AllowedToDrop { get; set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(ActivitySlot slot)
    {
        ActivitySlot = slot;
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (!ActivitySlot.IsUnlocked)
            SetActivitySlotClosed();
        else
        {
            switch(ActivitySlot.Status)
            {
                case ActivitySlotStatus.Available:
                    SetActivitySlotOpened();
                    break;
                case ActivitySlotStatus.Blocked:
                    SetActivitySlotOccupiedBlocked();
                    break;
                case ActivitySlotStatus.Caretaken:
                    SetActivitySlotOccupiedCaretaken();
                    break;
                case ActivitySlotStatus.Checkout:
                    SetActivitySlotOccupiedCheckout();
                    break;
                case ActivitySlotStatus.Paid:
                    SetActivitySlotOccupiedPaid();
                    break;
                default:
                    SetActivitySlotClosed();
                    break;
            }
        }
    }
    public void UpdateAvailable()
    {
        if(ActivitySlot.IsUnlocked)
            if (ActivitySlot.Status == ActivitySlotStatus.Available)
                SetActivitySlotOpened();
    }
    public void UpdateHeroAvailable(Hero hero, bool isAllowed)
    {
        if (ActivitySlot.IsUnlocked)
        {
            if (ActivitySlot.Status == ActivitySlotStatus.Available)
            {
                if (isAllowed)
                    SetActivitySlotHighlighted();
                else
                    SetActivitySlotHeroLocked();
                AllowedToDrop = isAllowed;
            }
            else if (ActivitySlot.Status == ActivitySlotStatus.Checkout)
            {
                if (ActivitySlot.Hero == hero)
                {
                    ActivitySlot.Hero = null;
                    ActivitySlot.Status = ActivitySlotStatus.Available;
                    if (isAllowed)
                        SetActivitySlotHighlighted();
                    else
                        SetActivitySlotHeroLocked();
                    AllowedToDrop = isAllowed;
                }
            }
        }
    }

    public void SetStatus(ActivitySlotStatus status)
    {
        if (ActivitySlot == null)
            return;

        ActivitySlot.Status = status;
        switch (ActivitySlot.Status)
        {
            case ActivitySlotStatus.Available:
                ActivitySlot.Hero = null;
                break;
            case ActivitySlotStatus.Blocked:
            case ActivitySlotStatus.Caretaken:
            case ActivitySlotStatus.Checkout:
            case ActivitySlotStatus.Paid:
            default:
                break;
        }
        UpdateSlot();
    }

    public void TreatmentButtonClicked()
    {
        if (onTreatmentButtonClick != null)
            onTreatmentButtonClick(this);
    }

    public void SetActivitySlotOccupiedPaid()
    {
        heroLabel.text = ActivitySlot.Hero.Name;
        costLabel.gameObject.SetActive(false);
        buttonIcon.gameObject.SetActive(true);
        buttonIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_activity.cancel_button"];

        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(true);

        if (ActivitySlot.Hero != null)
            slotIcon.sprite = DarkestDungeonManager.HeroSprites[ActivitySlot.Hero.ClassStringId]["A"].Portrait;
    }
    public void SetActivitySlotOccupiedCheckout()
    {
        heroLabel.text = ActivitySlot.Hero.Name;

        if (ActivitySlot.BaseCost != 0)
        {
            costLabel.gameObject.SetActive(true);
            costLabel.text = ActivitySlot.BaseCost.ToString();
        }
        else
        {
            costLabel.gameObject.SetActive(true);
            costLabel.text = LocalizationManager.GetString("town_free");
        }

        buttonIcon.gameObject.SetActive(true);
        buttonIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_activity.confirm_button"];

        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);

        if (ActivitySlot.Hero != null)
            slotIcon.sprite = DarkestDungeonManager.HeroSprites[ActivitySlot.Hero.ClassStringId]["A"].Portrait;
    }
    public void SetActivitySlotOccupiedBlocked()
    {
        heroLabel.text = ActivitySlot.Hero.Name;
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(true);

        if (ActivitySlot.Hero != null)
            slotIcon.sprite = DarkestDungeonManager.HeroSprites[ActivitySlot.Hero.ClassStringId]["A"].Portrait;
    }
    public void SetActivitySlotOccupiedCaretaken()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);

        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["caretaker_portrait"];
    }

    public void SetActivitySlotClosed()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(true);
        if(DarkestDungeonManager.Campaign.EventModifiers.ActivityLocks.ContainsKey(activityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.ActivityLocks[activityName] == true)
            eventLockerIcon.gameObject.SetActive(true);
        else
            eventLockerIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);
    }
    public void SetActivitySlotOpened()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);

        if (DarkestDungeonManager.Campaign.EventModifiers.FreeActivities.ContainsKey(activityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.FreeActivities[activityName] == true)
            freeIcon.gameObject.SetActive(true);
        else
            freeIcon.gameObject.SetActive(false);

        activeIcon.gameObject.SetActive(false);
        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.background"];
    }

    public void SetActivitySlotHeroLocked()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);
        if (DarkestDungeonManager.Campaign.EventModifiers.FreeActivities.ContainsKey(activityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.FreeActivities[activityName] == true)
            freeIcon.gameObject.SetActive(true);
        else
            freeIcon.gameObject.SetActive(false);

        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.locked_for_hero"];
    }
    public void SetActivitySlotHighlighted()
    {
        heroLabel.text = "";
        if(ActivitySlot.BaseCost != 0)
        {
            costLabel.gameObject.SetActive(true);
            costLabel.text = ActivitySlot.BaseCost.ToString();
        }
        else
        {
            costLabel.gameObject.SetActive(false);
            costLabel.text = "";
        }

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);
        if (DarkestDungeonManager.Campaign.EventModifiers.FreeActivities.ContainsKey(activityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.FreeActivities[activityName] == true)
            freeIcon.gameObject.SetActive(true);
        else
            freeIcon.gameObject.SetActive(false);
        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.backgroundhightlight"];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ActivitySlot.Status == ActivitySlotStatus.Caretaken)
            ToolTipManager.Instanse.Show(LocalizationManager.GetString("str_cant_place_hero_here_caretaker"),
                eventData, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Small);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.tag == "Roster Hero Slot")
        {
            if (ActivitySlot == null || !ActivitySlot.IsUnlocked || !AllowedToDrop)
                return;

            if (ActivitySlot.Status == ActivitySlotStatus.Available)
            {
                var heroSlot = eventData.pointerDrag.GetComponent<HeroSlot>();
                ActivitySlot.Status = ActivitySlotStatus.Checkout;
                ActivitySlot.Hero = heroSlot.Hero;
                UpdateSlot();
            }
        }
    }
}
