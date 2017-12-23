using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image buildingIcon;
    [SerializeField]
    private Animator iconAnimator;

    public bool IsOpened { get; private set; }

    public void SwitchUpgrades()
    {
        IsOpened = !IsOpened;
        iconAnimator.SetBool("IsOpened", IsOpened);
        DarkestSoundManager.PlayOneShot(IsOpened ? "event:/ui/town/page_open" : "event:/ui/town/page_close");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buildingIcon.material.SetFloat("_BrightnessAmount", 1.35f);
        DarkestSoundManager.PlayOneShot("event:/ui/town/button_mouse_over");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildingIcon.material.SetFloat("_BrightnessAmount", 1f);
    }
}
