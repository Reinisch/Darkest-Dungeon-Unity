using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaidMapRoomSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image roomIcon;
    public Image marker;
    public Image indicator;

    public RectTransform container;
    public Animator animator;

    private RectTransform slotRect;

    public Room Room { get; set; }
    public bool HasRoom { get; set; }
    public bool MarkedForMove { get; set; }
    public RectTransform SlotRect
    {
        get
        {
            if (slotRect == null)
                slotRect = GetComponent<RectTransform>();
            return slotRect;
        }
    }

    public void SetRoom(Room room)
    {
        Room = room;
        HasRoom = true;
        UpdateRoom();
    }
    public void SetEmpty()
    {
        Room = null;
        HasRoom = false;
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
        animator.SetBool("IsAvailable", movable);
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
            ToolTipManager.Instanse.Show(LocalizationManager.GetString("str_move_to_this_room"),
                eventData, SlotRect, ToolTipStyle.FromBottom, ToolTipSize.Small);
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
