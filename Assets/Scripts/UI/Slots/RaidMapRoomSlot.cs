using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaidMapRoomSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private Image roomIcon;
    [SerializeField]
    private Image marker;
    [SerializeField]
    private RectTransform container;
    [SerializeField]
    private Animator animator;

    public RectTransform SlotRect { get { return slotRect ?? (slotRect = GetComponent<RectTransform>()); } }
    public RectTransform Container { get { return container; } }
    public Animator Animator { get { return animator; } }

    private DungeonRoom Room { get; set; }
    private bool MarkedForMove { get; set; }

    private RectTransform slotRect;

    public void SetRoom(DungeonRoom room)
    {
        Room = room;
        UpdateRoom();
    }

    public void SetEmpty()
    {
        Room = null;
        roomIcon.enabled = false;
        marker.enabled = false;
    }

    public void UpdateRoom()
    {
        if (Room.Type != AreaType.Entrance)
        {
            if(Room.Knowledge == Knowledge.Hidden)
            {
                roomIcon.enabled = true;
                roomIcon.sprite = DarkestDungeonManager.Data.MapRoomKnowledgeSet[Knowledge.Hidden];
                marker.enabled = false;
            }
            else if (Room.Knowledge == Knowledge.Scouted || Room.Knowledge == Knowledge.Visited)
            {
                roomIcon.enabled = true;
                roomIcon.sprite = DarkestDungeonManager.Data.MapRoomIconSet[Room.Type];
                marker.enabled = false;
            }
            else if (Room.Knowledge == Knowledge.Completed)
            {
                roomIcon.enabled = true;
                roomIcon.sprite = DarkestDungeonManager.Data.MapRoomIconSet[Room.Type];
                marker.enabled = true;
                marker.sprite = DarkestDungeonManager.Data.MapRoomKnowledgeSet[Knowledge.Completed];
            }
        }
        else
        {
            roomIcon.sprite = DarkestDungeonManager.Data.MapRoomIconSet[Room.Type];
            marker.enabled = false;
        }
    }

    public void SetMovable(bool movable)
    {
        Animator.SetBool("IsAvailable", movable);
        MarkedForMove = movable;
    }

    public void MarkRoom()
    {
        marker.enabled = true;
        marker.sprite = DarkestDungeonManager.Data.Sprites["moving_room"];
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(MarkedForMove)
            ToolTipManager.Instanse.Show(LocalizationManager.GetString("str_move_to_this_room"), SlotRect, ToolTipStyle.FromBottom, ToolTipSize.Small);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(MarkedForMove)
            RaidSceneManager.Instanse.TargetRoomSelected(Room);
    }
}
