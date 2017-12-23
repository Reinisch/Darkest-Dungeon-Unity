using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MultiplayerPartySlot : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private Image heroFrame;

    public Hero SelectedHero { get; private set; }

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