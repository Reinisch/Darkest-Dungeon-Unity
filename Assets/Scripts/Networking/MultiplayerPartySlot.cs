using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MultiplayerPartySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image slotFrame;
    public Image heroFrame;

    public int SlotId { get; set; }
    public Hero SelectedHero { get; set; }
    public RectTransform RectTransform { get; private set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SelectedHero != null)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SelectedHero != null)
            ToolTipManager.Instanse.Show(LocalizationManager.GetString("hero_class_name_" + SelectedHero.ClassStringId),
                eventData, RectTransform, ToolTipStyle.FromTop, ToolTipSize.Small);
    }
}