using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TreatmentHeroSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField]
    private string activityName;
    [SerializeField]
    private Text heroLabel;

    [SerializeField]
    private Image lockerIcon;
    [SerializeField]
    private Image activeIcon;
    [SerializeField]
    private Image buttonIcon;
    [SerializeField]
    private Image slotIcon;
    [SerializeField]
    private Image freeIcon;

    public Image ButtonIcon { get { return buttonIcon; } }
    public string ActivityName { get { return activityName; } }
    public TreatmentSlot TreatmentSlot { get; private set; }

    private RectTransform RectTransform { get; set; }

    public event Action<TreatmentHeroSlot> EventHeroDropped;
    public event Action<TreatmentHeroSlot> EventHeroRemoved;
    public event Action<TreatmentHeroSlot> EventTreatmentButtonClick;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(TreatmentSlot slot)
    {
        TreatmentSlot = slot;
        UpdateSlot();
    }

    public void SetStatus(ActivitySlotStatus status)
    {
        if (TreatmentSlot == null)
            return;

        TreatmentSlot.Status = status;
        switch (TreatmentSlot.Status)
        {
            case ActivitySlotStatus.Available:
                TreatmentSlot.Hero = null;
                break;
        }
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (!TreatmentSlot.IsUnlocked)
            SetActivitySlotClosed();
        else
        {
            switch (TreatmentSlot.Status)
            {
                case ActivitySlotStatus.Available:
                    SetActivitySlotOpened();
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

    public void PayoutSlot()
    {
        if (EventHeroRemoved != null)
            EventHeroRemoved(this);

        TreatmentSlot.Status = ActivitySlotStatus.Paid;

        UpdateSlot();
    }

    public void ClearSlot()
    {
        if (EventHeroRemoved != null)
            EventHeroRemoved(this);

        TreatmentSlot.Hero = null;
        TreatmentSlot.Status = ActivitySlotStatus.Available;
        
        UpdateSlot();
    }

    public void ResetTreatment()
    {
        if(TreatmentSlot != null)
        {
            TreatmentSlot.TargetDiseaseQuirk = null;
            TreatmentSlot.TargetNegativeQuirk = null;
            TreatmentSlot.TargetPositiveQuirk = null;
        }
    }

    public void UpdateHeroAvailable()
    {
        if (TreatmentSlot.IsUnlocked)
        {
            if (TreatmentSlot.Status == ActivitySlotStatus.Available)
                SetActivitySlotHighlighted();
            else if (TreatmentSlot.Status == ActivitySlotStatus.Checkout)
            {
                TreatmentSlot.Hero = null;
                TreatmentSlot.Status = ActivitySlotStatus.Available;
                SetActivitySlotHighlighted();
            }
        }
    }

    public void TreatmentButtonClicked()
    {
        if (EventTreatmentButtonClick != null)
            EventTreatmentButtonClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TreatmentSlot.Status == ActivitySlotStatus.Caretaken)
            ToolTipManager.Instanse.Show(LocalizationManager.GetString("str_cant_place_hero_here"), RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Small);

        if (TreatmentSlot.IsUnlocked)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over_3");
        else
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over_2");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.CompareTag("Roster Hero Slot"))
        {
            if (TreatmentSlot == null || !TreatmentSlot.IsUnlocked)
            {
                DarkestSoundManager.PlayOneShot("event:/ui/town/button_invalid");
                return;
            }

            if (TreatmentSlot.Status == ActivitySlotStatus.Available)
            {
                var heroSlot = eventData.pointerDrag.GetComponent<HeroSlot>();
                TreatmentSlot.Status = ActivitySlotStatus.Checkout;
                TreatmentSlot.Hero = heroSlot.Hero;
                if (EventHeroDropped != null)
                    EventHeroDropped(this);
                UpdateSlot();
                DarkestSoundManager.PlayOneShot("event:/ui/town/character_add");
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (TreatmentSlot != null && TreatmentSlot.Status == ActivitySlotStatus.Checkout)
            ClearSlot();
    }

    #region Slot States

    private void SetActivitySlotOccupiedPaid()
    {
        heroLabel.text = TreatmentSlot.Hero.Name;
        ButtonIcon.gameObject.SetActive(true);
        lockerIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(true);
        freeIcon.gameObject.SetActive(false);

        if (TreatmentSlot.Hero != null)
            slotIcon.sprite = DarkestDungeonManager.HeroSprites[TreatmentSlot.Hero.ClassStringId]["A"].Portrait;
    }

    private void SetActivitySlotOccupiedCheckout()
    {
        heroLabel.text = TreatmentSlot.Hero.Name;

        ButtonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);

        if (TreatmentSlot.Hero != null)
            slotIcon.sprite = DarkestDungeonManager.HeroSprites[TreatmentSlot.Hero.ClassStringId]["A"].Portrait;
    }

    private void SetActivitySlotClosed()
    {
        heroLabel.text = "";

        ButtonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(true);
        activeIcon.gameObject.SetActive(false);
        freeIcon.gameObject.SetActive(false);
    }

    private void SetActivitySlotOpened()
    {
        heroLabel.text = "";

        ButtonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
        activeIcon.gameObject.SetActive(false);
        if (DarkestDungeonManager.Campaign.EventModifiers.FreeActivities.ContainsKey(ActivityName) &&
            DarkestDungeonManager.Campaign.EventModifiers.FreeActivities[ActivityName])
            freeIcon.gameObject.SetActive(true);
        else
            freeIcon.gameObject.SetActive(false);
        slotIcon.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.background"];
    }

    private void SetActivitySlotHighlighted()
    {
        heroLabel.text = "";

        ButtonIcon.gameObject.SetActive(false);
        lockerIcon.gameObject.SetActive(false);
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
