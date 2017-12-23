using UnityEngine;
using UnityEngine.EventSystems;

public class PointerPartyMover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Direction direction;

    public void OnPointerEnter(PointerEventData eventData)
    {
        RaidSceneManager.PartyController.PointerMovementDirection = direction;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RaidSceneManager.PartyController.PointerMovementDirection = Direction.Bot;
    }
}