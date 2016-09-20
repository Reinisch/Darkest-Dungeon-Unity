using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buildingIcon;
    public Image infoIcon;
    public SkeletonAnimation upgradePulse;
    public Animator iconAnimator;

    public bool IsOpened { get; set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buildingIcon.material.SetFloat("_BrightnessAmount", 1.35f);
    }

    public void SwitchUpgrades()
    {
        IsOpened = !IsOpened;
        iconAnimator.SetBool("IsOpened", IsOpened);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildingIcon.material.SetFloat("_BrightnessAmount", 1f);
    }
}
