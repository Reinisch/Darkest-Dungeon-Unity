using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollMapUnfocus : MonoBehaviour, IBeginDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        RaidSceneManager.RaidPanel.MapPanel.Unfocus();
    }
}
