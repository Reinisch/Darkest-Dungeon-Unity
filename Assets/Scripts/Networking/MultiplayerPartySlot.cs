using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MultiplayerPartySlot : MonoBehaviour, IPointerEnterHandler
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

    public void UpdateHero(Hero hero)
    {
        SelectedHero = hero;
        heroFrame.sprite = DarkestDungeonManager.HeroSprites[hero.ClassStringId]["A"].Portrait;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SelectedHero != null)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
    }

    public void HeroSelected()
    {
        if (SelectedHero != null)
        {
            if (!DarkestPhotonLauncher.CharacterWindow.gameObject.activeSelf)
                DarkestPhotonLauncher.CharacterWindow.WindowOpened();

            DarkestPhotonLauncher.CharacterWindow.UpdateCharacterInfo(SelectedHero, true, true);
        }
    }
}