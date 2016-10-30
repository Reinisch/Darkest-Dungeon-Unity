using UnityEngine;
using UnityEngine.EventSystems;

public class PointerPartyMover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Direction direction;

    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_ANDROID || UNITY_IOS
        RaidSceneManager.Instanse.partyController.PointerMovementDirection = direction;
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if UNITY_ANDROID || UNITY_IOS
        RaidSceneManager.Instanse.partyController.PointerMovementDirection = Direction.Bot;
#endif
    }
}