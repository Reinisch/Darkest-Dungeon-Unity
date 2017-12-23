using UnityEngine;
using UnityEngine.EventSystems;

public class HeroDiscardPanel : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        DragManager.Instanse.DropOutDraggedPartyHero();
    }
}
