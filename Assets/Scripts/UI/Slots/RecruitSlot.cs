using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecruitSlot : MonoBehaviour, IBeginDragHandler, IEndDragHandler,
    IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image heroPortrait;
    public Text nameLabel;
    public Text classLabel;
    public Image resolveIcon;
    public Text resolveLabel;

    public Hero Hero { get; set; }
    public HeroRosterPanel HeroRoster { get; set; }

    public void UpdateSlot(Hero hero)
    {
        if(hero == null)
            RemoveSlot();
        else
        {
            Hero = hero;
            if(nameLabel != null)
            {
                nameLabel.text = Hero.HeroName;
                classLabel.text = LocalizationManager.GetString("str_resolve_" + Hero.Resolve.Level.ToString())
                    + " " + LocalizationManager.GetString("hero_class_name_" + Hero.ClassStringId);

                resolveLabel.text = hero.Resolve.Level.ToString();
                if (hero.Resolve.Level > 0)
                    resolveIcon.sprite = DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background_lvl" +
                        hero.Resolve.Level.ToString()];
                else
                    resolveIcon.sprite = DarkestDungeonManager.Data.Sprites["resolve_level_bar_number_background"];
            }
            heroPortrait.sprite = DarkestDungeonManager.HeroSprites[Hero.ClassStringId]["A"].Portrait;

            transform.parent.gameObject.SetActive(true);
        }
        
    }
    public void RemoveSlot()
    {
        Hero = null;
        transform.parent.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;

        if (eventData.button == PointerEventData.InputButton.Right)
            HeroRoster.RecruitSlotClicked(this);
    }
    public void OnDrag(PointerEventData eventData)
    {
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
                return;
            Vector3 globalMousePos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(DragManager.Instanse.OverlayRect,
                eventData.position, eventData.pressEventCamera, out globalMousePos);

            if (hoveredSlot.RectTransform.position.y > HeroRoster.placeHolder.RectTransform.position.y)
            {
                if (globalMousePos.y > hoveredSlot.RectTransform.position.y)
                {
                    HeroRoster.placeHolder.RectTransform.SetSiblingIndex(hoveredSlot.RectTransform.GetSiblingIndex());
                }
            }
            else if (hoveredSlot.RectTransform.position.y < HeroRoster.placeHolder.RectTransform.position.y)
            {
                if (globalMousePos.y < hoveredSlot.RectTransform.position.y)
                {
                    HeroRoster.placeHolder.RectTransform.SetSiblingIndex(hoveredSlot.RectTransform.GetSiblingIndex());
                }
            }
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {      
        DragManager.Instanse.StartDragging(this, eventData);

        HeroRoster.Dragging = true;
        HeroRoster.rosterLive.gameObject.SetActive(true);
        heroPortrait.sprite = DarkestDungeonManager.Data.Sprites["hero_slot.background"];
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        DragManager.Instanse.EndDragging(this, eventData);

        HeroRoster.Dragging = false;
        HeroRoster.rosterLive.gameObject.SetActive(false);
        if(Hero != null)
            heroPortrait.sprite = DarkestDungeonManager.HeroSprites[Hero.ClassStringId]["A"].Portrait;

        HeroRoster.placeHolder.slotController.SetBool("isHidden", true);
        HeroRoster.placeHolder.slotController.SetTrigger("hide");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Show(LocalizationManager.GetString("str_hero_slot_unlocked_stagecoach_tt"),
            eventData, (RectTransform)transform, ToolTipStyle.FromBottom, ToolTipSize.Small);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    } 
}
