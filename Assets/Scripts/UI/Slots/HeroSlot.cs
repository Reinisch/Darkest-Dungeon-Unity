using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HeroSlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler
{
    [SerializeField]
    private Image portrait;
    [SerializeField]
    private Image statusIcon;
    [SerializeField]
    private Image diseaseIcon;
    [SerializeField]
    private Text heroName;
    [SerializeField]
    private Text weaponLevel;
    [SerializeField]
    private Text armorLevel;
    [SerializeField]
    private StressOverlayPanel stressPanel;
    [SerializeField]
    private ResolveBar resolveBar;
    [SerializeField]
    private Animator slotController;

    public Hero Hero { get; set; }
    public RaidPartySlot PartySlot { get; set; }
    public RectTransform RectTransform { get; private set; }
    public HeroRosterPanel HeroRoster { private get; set; }
    public Image Portrait { get { return portrait; } }
    public Animator SlotController { get { return slotController; } }

#if UNITY_ANDROID || UNITY_IOS
    float doubleTapTimer = 0f;
    float doubleTapTime = 0.2f;

    private void Update()
    {
        if (doubleTapTimer > 0)
            doubleTapTimer -= Time.deltaTime;
    }
#endif

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
    
    public void CopyToDragItem(DragItemHolder heroHolder)
    {
        heroHolder.ItemIcon.sprite = Portrait.sprite;
    }

    public void UpdateSlot()
    {
        Portrait.sprite = DarkestDungeonManager.HeroSprites[Hero.ClassStringId]["A"].Portrait;
        heroName.text = Hero.HeroName;
        weaponLevel.text = Hero.WeaponLevel.ToString();
        armorLevel.text = Hero.ArmorLevel.ToString();
        stressPanel.UpdateStress(Hero.GetPairedAttribute(AttributeType.Stress).ValueRatio);
        resolveBar.UpdateResolve(Hero);
        diseaseIcon.gameObject.SetActive(Hero.Diseases.Count > 0);

        SetStatus(Hero.Status);
    }

    public void SetStatus(HeroStatus status)
    {
        if (Hero == null)
            return;

        if (Hero.Status == HeroStatus.RaidParty && status != HeroStatus.RaidParty)
        {
            Hero.Status = status;
            if (PartySlot != null)
                PartySlot.ItemDroppedOut(this);
        }
        Hero.Status = status;

        switch (Hero.Status)
        {
            case HeroStatus.Abbey:
                Portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["abbey.icon_roster"];
                break;
            case HeroStatus.Sanitarium:
                Portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["sanitarium.icon_roster"];
                break;
            case HeroStatus.Tavern:
                Portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["tavern.icon_roster"];
                break;
            case HeroStatus.Missing:
                Portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["missing.icon_roster"];
                break;
            case HeroStatus.RaidParty:
                Portrait.material = Portrait.defaultMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["party.icon_roster"];
                break;
            case HeroStatus.Available:
                Portrait.material = Portrait.defaultMaterial;
                statusIcon.gameObject.SetActive(false);
                break;
            default:
                statusIcon.gameObject.SetActive(false);
                Portrait.material = Portrait.defaultMaterial;
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;

#if UNITY_ANDROID || UNITY_IOS
        if (doubleTapTimer > 0)
            HeroRoster.HeroSlotClicked(this);
        else
            doubleTapTimer = doubleTapTime;
#else
        if (eventData.button == PointerEventData.InputButton.Right)
            HeroRoster.HeroSlotClicked(this);
#endif
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragManager.Instanse.OnDrag(this, eventData);

        if (!HeroRoster.Hovered)
            return;

        if (!eventData.pointerCurrentRaycast.isValid)
            return;

        if (eventData.pointerCurrentRaycast.gameObject == HeroRoster.PlaceHolder.gameObject ||
            eventData.pointerCurrentRaycast.gameObject == gameObject)
            return;

        var hoveredSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<HeroSlot>();
        Vector3 globalMousePos;
        if (hoveredSlot == null)
        {
            if (HeroRoster.gameObject == eventData.pointerCurrentRaycast.gameObject)
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(DragManager.Instanse.OverlayRect,
                    eventData.position, eventData.pressEventCamera, out globalMousePos);

                int shifts = 10;
                while (shifts > 0)
                {
                    var shifted = false;
                    if (globalMousePos.y + HeroRoster.PlaceHolder.RectTransform.sizeDelta.y / 2 > 
                        HeroRoster.PlaceHolder.RectTransform.position.y)
                    {
                        HeroRoster.PlaceHolder.RectTransform.SetSiblingIndex(HeroRoster.PlaceHolder.RectTransform.GetSiblingIndex() - 1);
                        if (HeroRoster.PlaceHolder.RectTransform.GetSiblingIndex() == 0)
                            shifts = 0;
                        shifted = true;
                    }

                    if (globalMousePos.y - HeroRoster.PlaceHolder.RectTransform.sizeDelta.y / 2 <
                        HeroRoster.PlaceHolder.RectTransform.position.y)
                    {
                        HeroRoster.PlaceHolder.RectTransform.SetSiblingIndex(HeroRoster.PlaceHolder.RectTransform.GetSiblingIndex() + 1);
                        if (HeroRoster.PlaceHolder.RectTransform.GetSiblingIndex() == HeroRoster.HeroSlots.Count)
                            shifts = 0;
                        shifted = true;
                    }
                    if (shifted)
                        shifts--;
                    else
                        shifts = 0;
                }
            }
            return;
        }

        RectTransformUtility.ScreenPointToWorldPointInRectangle(DragManager.Instanse.OverlayRect,
            eventData.position, eventData.pressEventCamera, out globalMousePos);

        if (hoveredSlot.RectTransform.position.y > HeroRoster.PlaceHolder.RectTransform.position.y)
        {
            if (globalMousePos.y > hoveredSlot.RectTransform.position.y)
            {
                RectTransform.SetSiblingIndex(hoveredSlot.RectTransform.GetSiblingIndex());
                HeroRoster.PlaceHolder.RectTransform.SetSiblingIndex(RectTransform.GetSiblingIndex());
            }
        }
        else if (hoveredSlot.RectTransform.position.y < HeroRoster.PlaceHolder.RectTransform.position.y)
        {
            if (globalMousePos.y < hoveredSlot.RectTransform.position.y)
            {
                RectTransform.SetSiblingIndex(hoveredSlot.RectTransform.GetSiblingIndex());
                HeroRoster.PlaceHolder.RectTransform.SetSiblingIndex(RectTransform.GetSiblingIndex());
            }               
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left || Hero == null)
        {
            eventData.dragging = false;
            eventData.pointerDrag = null;
            return;
        }

        if (Hero.Status == HeroStatus.Tavern || Hero.Status == HeroStatus.Sanitarium 
            || Hero.Status == HeroStatus.Abbey || Hero.Status == HeroStatus.Missing)
        {
            eventData.dragging = false;
            eventData.pointerDrag = null;
            return;
        }

        HeroRoster.Dragging = true;
        HeroRoster.RosterLive.gameObject.SetActive(true);
        HeroRoster.HeroSlotBeginDragging(this);
        DragManager.Instanse.StartDragging(this, eventData);

        if (HeroRoster.Hovered)
        {
            SlotController.SetTrigger("hide");
            HeroRoster.PlaceHolder.RectTransform.SetSiblingIndex(RectTransform.GetSiblingIndex());
            HeroRoster.PlaceHolder.SlotController.SetTrigger("show");
            HeroRoster.PlaceHolder.SlotController.SetBool("isHidden", false);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HeroRoster.Dragging = false;
        HeroRoster.RosterLive.gameObject.SetActive(false);
        HeroRoster.HeroSlotEndDragging(this);
        DragManager.Instanse.EndDragging(this, eventData);

        HeroRoster.PlaceHolder.SlotController.SetBool("isHidden", true);
        HeroRoster.PlaceHolder.SlotController.SetTrigger("hide");
        SlotController.SetTrigger("show");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag.CompareTag("Recruit"))
        {
            var recruitSlot = eventData.pointerDrag.GetComponent<RecruitSlot>();
            if (HeroRoster.CreateSlot(recruitSlot, this) != null)
            {
                recruitSlot.OnEndDrag(eventData);
                recruitSlot.RemoveSlot();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over_3");
    }
}