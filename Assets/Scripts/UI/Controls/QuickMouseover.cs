using UnityEngine;
using UnityEngine.EventSystems;

public class QuickMouseover : MonoBehaviour, IPointerEnterHandler
{
    public RectTransform mouseOverTarget;
    public string mouseoverEvent = "event:/ui/town/button_mouse_over";

    public void OnPointerEnter(PointerEventData eventData)
    {
        DarkestSoundManager.PlayOneShot(mouseoverEvent);
    }
}
