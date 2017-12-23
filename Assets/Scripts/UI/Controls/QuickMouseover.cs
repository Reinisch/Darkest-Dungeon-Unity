using UnityEngine;
using UnityEngine.EventSystems;

public class QuickMouseover : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private string mouseoverEvent = "event:/ui/town/button_mouse_over";

    public void OnPointerEnter(PointerEventData eventData)
    {
        DarkestSoundManager.PlayOneShot(mouseoverEvent);
    }
}
