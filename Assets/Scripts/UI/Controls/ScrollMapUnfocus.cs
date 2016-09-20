using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ScrollMapUnfocus : MonoBehaviour, IBeginDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        RaidSceneManager.RaidPanel.mapPanel.Unfocus();
    }
}
