using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TownHeroSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [SerializeField]
    private string activityName;
    [SerializeField]
    private Text heroLabel;
    [SerializeField]
    private Text costLabel;

    [SerializeField]
    private Image eventLockerIcon;
    [SerializeField]
    private Image lockerIcon;
    [SerializeField]
    private Image activeIcon;
    [SerializeField]
    private Image buttonIcon;
    [SerializeField]
    private Image freeIcon;
    [SerializeField]
    private Image slotIcon;

    public event Action<TownHeroSlot> EventTreatmentButtonClicked;

    public string ActivityName { get { return activityName; } }
    public ActivitySlot ActivitySlot { get; private set; }

    private RectTransform RectTransform { get; set; }
    private bool AllowedToDrop { get; set; }

    private void Awake()
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
                case ActivitySlotStatus.Crierd:
                    SetActivitySlotOccupiedCrierd();
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
        if(ActivitySlot.IsUnlocked && ActivitySlot.Status == ActivitySlotStatus.Available)
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
        if (ActivitySlot.Status == ActivitySlotStatus.Available)
            ActivitySlot.Hero = null;
        UpdateSlot();
    }

    public void TreatmentButtonClicked()
    {
        if (EventTreatmentButtonClicked != null)
            EventTreatmentButtonClicked(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ActivitySlot.Status == ActivitySlotStatus.Caretaken)
            ToolTipManager.Instanse.Show(LocalizationManager.GetString("str_cant_place_hero_here_caretaker"),
                RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Small);
        else if (ActivitySlot.Status == ActivitySlotStatus.Crierd)
            ToolTipManager.Instanse.Show(LocalizationManager.GetString("str_cant_place_hero_here_crier"),
                RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Small);

        DarkestSoundManager.PlayOneShot(ActivitySlot.IsUnlocked ?
            "event:/ui/town/button_mouse_over_3" : "event:/ui/town/button_mouse_over_2");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.CompareTag("Roster Hero Slot"))
        {
            if (ActivitySlot == null || !ActivitySlot.IsUnlocked || !AllowedToDrop)
            {
                DarkestSoundManager.PlayOneShot("event:/ui/town/button_invalid");
                return;
            }

            if (ActivitySlot.Status == ActivitySlotStatus.Available)
            {
                var heroSlot = eventData.pointerDrag.GetComponent<HeroSlot>();
                ActivitySlot.Status = ActivitySlotStatus.Checkout;
                ActivitySlot.Hero = heroSlot.Hero;
                UpdateSlot();
                DarkestSoundManager.PlayOneShot("event:/ui/town/character_add");
            }
        }
    }

    #region Slots States

    private void SetActivitySlotOccupiedPaid()
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

    private void SetActivitySlotOccupiedCheckout()
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

    private void SetActivitySlotOccupiedBlocked()
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

    private void SetActivitySlotOccupiedCaretaken()
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

    private void SetActivitySlotOccupiedCrierd()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);

        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["crier_portrait"];
    }

    private void SetActivitySlotClosed()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(true);
        if (DarkestDungeonManager.Campaign.EventModifiers.ActivityLocks.ContainsKey(ActivityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.ActivityLocks[ActivityName])
            eventLockerIcon.gameObject.SetActive(true);
        else
            eventLockerIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);
    }

    private void SetActivitySlotOpened()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);

        if (DarkestDungeonManager.Campaign.EventModifiers.FreeActivities.ContainsKey(ActivityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.FreeActivities[ActivityName])
            freeIcon.gameObject.SetActive(true);
        else
            freeIcon.gameObject.SetActive(false);

        activeIcon.gameObject.SetActive(false);
        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.background"];
    }

    private void SetActivitySlotHeroLocked()
    {
        heroLabel.text = "";
        costLabel.gameObject.SetActive(false);

        buttonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        eventLockerIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);
        if (DarkestDungeonManager.Campaign.EventModifiers.FreeActivities.ContainsKey(ActivityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.FreeActivities[ActivityName])
            freeIcon.gameObject.SetActive(true);
        else
            freeIcon.gameObject.SetActive(false);

        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.locked_for_hero"];
    }

    private void SetActivitySlotHighlighted()
    {
        heroLabel.text = "";
        if (ActivitySlot.BaseCost != 0)
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
        if (DarkestDungeonManager.Campaign.EventModifiers.FreeActivities.ContainsKey(ActivityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.FreeActivities[ActivityName])
            freeIcon.gameObject.SetActive(true);
        else
            freeIcon.gameObject.SetActive(false);
        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.backgroundhightlight"];
    }

    #endregion
}