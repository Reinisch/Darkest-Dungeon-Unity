using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HeroSlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    public Image portrait;
    public Image statusIcon;
    public Image diseaseIcon;
    public Text heroName;
    public Text weaponLevel;
    public Text armorLevel;
    public StressOverlayPanel stressPanel;
    public ResolveBar resolveBar;
    public Image resolveRect;
    public CanvasGroup canvasGroup;
    public Animator slotController;

    public int PartySlotId
    {
        get
        {
            if (PartySlot == null)
                return 0;
            else
                return PartySlot.SlotId;
        }
    }

    public Hero Hero { get; set; }
    public RaidPartySlot PartySlot { get; set; }

    public RectTransform RectTransform { get; set; }
    public HeroRosterPanel HeroRoster { get; set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void CopyToDragItem(DragItemHolder heroHolder)
    {
        heroHolder.itemIcon.sprite = portrait.sprite;
    }

    public void UpdateSlot()
    {
        portrait.sprite = DarkestDungeonManager.HeroSprites[Hero.ClassStringId]["A"].Portrait;
        heroName.text = Hero.HeroName;
        weaponLevel.text = Hero.WeaponLevel.ToString();
        armorLevel.text = Hero.ArmorLevel.ToString();
        stressPanel.UpdateStress(Hero.GetPairedAttribute(AttributeType.Stress).ValueRatio);
        resolveBar.UpdateResolve(Hero);
        if (Hero.Diseases.Count > 0)
            diseaseIcon.gameObject.SetActive(true);
        else
            diseaseIcon.gameObject.SetActive(false);
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
                portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["abbey.icon_roster"];
                break;
            case HeroStatus.Sanitarium:
                portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["sanitarium.icon_roster"];
                break;
            case HeroStatus.Tavern:
                portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["tavern.icon_roster"];
                break;
            case HeroStatus.Missing:
                portrait.material = DarkestDungeonManager.GrayDarkMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["missing.icon_roster"];
                break;
            case HeroStatus.RaidParty:
                portrait.material = portrait.defaultMaterial;
                statusIcon.gameObject.SetActive(true);
                statusIcon.sprite = DarkestDungeonManager.Data.Sprites["party.icon_roster"];
                break;
            case HeroStatus.Available:
                portrait.material = portrait.defaultMaterial;
                statusIcon.gameObject.SetActive(false);
                break;
            default:
                statusIcon.gameObject.SetActive(false);
                portrait.material = portrait.defaultMaterial;
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
            HeroRoster.HeroSlotClicked(this);
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 globalMousePos;

        DragManager.Instanse.OnDrag(this, eventData);

        if (!HeroRoster.Hovered)
            return;

        if (eventData.pointerCurrentRaycast.isValid)
        {
            if (eventData.pointerCurrentRaycast.gameObject == HeroRoster.placeHolder.gameObject ||
                eventData.pointerCurrentRaycast.gameObject == gameObject)
                return;

            var hoveredSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<HeroSlot>();
            if (hoveredSlot == null)
            {
                if (HeroRoster.gameObject == eventData.pointerCurrentRaycast.gameObject)
                {
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(DragManager.Instanse.OverlayRect,
                        eventData.position, eventData.pressEventCamera, out globalMousePos);

                    bool shifted;
                    int shifts = 10;
                    while (shifts > 0)
                    {
                        shifted = false;
                        if (globalMousePos.y + HeroRoster.placeHolder.RectTransform.sizeDelta.y / 2 > 
                            HeroRoster.placeHolder.RectTransform.position.y)
                        {
                            HeroRoster.placeHolder.RectTransform.SetSiblingIndex(HeroRoster.placeHolder.RectTransform.GetSiblingIndex() - 1);
                            if (HeroRoster.placeHolder.RectTransform.GetSiblingIndex() == 0)
                                shifts = 0;
                            shifted = true;
                        }

                        if (globalMousePos.y - HeroRoster.placeHolder.RectTransform.sizeDelta.y / 2 <
                            HeroRoster.placeHolder.RectTransform.position.y)
                        {
                            HeroRoster.placeHolder.RectTransform.SetSiblingIndex(HeroRoster.placeHolder.RectTransform.GetSiblingIndex() + 1);
                            if (HeroRoster.placeHolder.RectTransform.GetSiblingIndex() == HeroRoster.HeroSlots.Count)
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

            if (hoveredSlot.RectTransform.position.y > HeroRoster.placeHolder.RectTransform.position.y)
            {
                if (globalMousePos.y > hoveredSlot.RectTransform.position.y)
                {
                    RectTransform.SetSiblingIndex(hoveredSlot.RectTransform.GetSiblingIndex());
                    HeroRoster.placeHolder.RectTransform.SetSiblingIndex(RectTransform.GetSiblingIndex());
                }
            }
            else if (hoveredSlot.RectTransform.position.y < HeroRoster.placeHolder.RectTransform.position.y)
            {
                if (globalMousePos.y < hoveredSlot.RectTransform.position.y)
                {
                    RectTransform.SetSiblingIndex(hoveredSlot.RectTransform.GetSiblingIndex());
                    HeroRoster.placeHolder.RectTransform.SetSiblingIndex(RectTransform.GetSiblingIndex());
                }               
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
        HeroRoster.rosterLive.gameObject.SetActive(true);
        HeroRoster.HeroSlotBeginDragging(this);
        DragManager.Instanse.StartDragging(this, eventData);

        if (HeroRoster.Hovered)
        {
            slotController.SetTrigger("hide");
            HeroRoster.placeHolder.RectTransform.SetSiblingIndex(RectTransform.GetSiblingIndex());
            HeroRoster.placeHolder.slotController.SetTrigger("show");
            HeroRoster.placeHolder.slotController.SetBool("isHidden", false);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        HeroRoster.Dragging = false;
        HeroRoster.rosterLive.gameObject.SetActive(false);
        HeroRoster.HeroSlotEndDragging(this);
        DragManager.Instanse.EndDragging(this, eventData);

        HeroRoster.placeHolder.slotController.SetBool("isHidden", true);
        HeroRoster.placeHolder.slotController.SetTrigger("hide");
        slotController.SetTrigger("show");
    }
    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag.tag == "Recruit")
        {
            var recruitSlot = eventData.pointerDrag.GetComponent<RecruitSlot>();
            if (HeroRoster.CreateSlot(recruitSlot, this) != null)
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