using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class RaidMapPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GridLayoutGroup roomGrid;
    public GridLayoutGroup verticalHallGrid;
    public GridLayoutGroup horizontalHallGrid;

    public GameObject roomSlotTemplate;
    public RaidMapHallwaySlot verticalHallSlotTemplate;
    public RaidMapHallwaySlot horizontalHallSlotTemplate;

    public RectTransform mapContent;
    public ScrollRect scrollRect;
    public RectTransform scrollArea;
    public RectTransform leftBot;
    public RectTransform rightTop;

    public SkeletonAnimation scoutRadar;

    public Dungeon Dungeon { get; set; }

    bool isMouseOver = false;
    float maxScrollScale = 3.2f;
    float minScrollScale = 1;
    float scrollSpeed = 200f;
    float velocity = 0;
    Vector2 focusVelocity = Vector2.zero;
    Vector2 normalizedTarget = Vector2.zero;
    float targetScale = 1;
    bool focus = false;
    float focusTimeMax = 3f;

    public Dictionary<string, RaidMapRoomSlot> RoomSlots { get; set; }
    public Dictionary<string, RaidMapHallwaySlot> HallSlots { get; set; }

    public RaidMapRoomSlot LastCurrentRoom { get; set; }
    public RaidMapHallSectorSlot LastHallwaySector { get; set; }

    public void LoadDungeon(Dungeon dungeon)
    {
        RoomSlots = new Dictionary<string, RaidMapRoomSlot>();
        HallSlots = new Dictionary<string, RaidMapHallwaySlot>();

        Dungeon = dungeon;
        roomGrid.constraintCount = dungeon.GridSizeX;
        verticalHallGrid.constraintCount = dungeon.GridSizeX;
        horizontalHallGrid.constraintCount = dungeon.GridSizeX - 1;

        for (int j = 1; j <= dungeon.GridSizeY; j++)
        {
            for (int i = 1; i <= dungeon.GridSizeX; i++)
            {
                RaidMapRoomSlot slot = Instantiate(roomSlotTemplate).GetComponentInChildren<RaidMapRoomSlot>();
                slot.container.SetParent(roomGrid.transform, false);
                Room room;
                if (dungeon.Rooms.TryGetValue("room" + i.ToString() + "_" + j.ToString(), out room))
                {
                    slot.SetRoom(room);
                    RoomSlots.Add(room.Id, slot);
                }
                else
                    slot.SetEmpty();
            }
        }

        for (int j = 1; j <= dungeon.GridSizeY; j++)
        {
            for (int i = 2; i <= dungeon.GridSizeX; i++)
            {
                RaidMapHallwaySlot slot = Instantiate(horizontalHallSlotTemplate);
                slot.rectTransform.SetParent(horizontalHallGrid.transform, false);
                Hallway hallway;
                if (dungeon.Hallways.TryGetValue("hallroom" + (i).ToString() + "_" + j.ToString()
                    + "_room" + (i-1).ToString() + "_" + j.ToString(), out hallway))
                {
                    slot.SetHall(hallway);
                    HallSlots.Add(hallway.Id, slot);
                }
                else
                    slot.SetEmpty();
            }
        }

        for (int j = 2; j <= dungeon.GridSizeY; j++)
        {
            for (int i = 1; i <= dungeon.GridSizeX; i++)
            {
                RaidMapHallwaySlot slot = Instantiate(verticalHallSlotTemplate);
                slot.rectTransform.SetParent(verticalHallGrid.transform, false);
                Hallway hallway;
                if (dungeon.Hallways.TryGetValue("hallroom" + (i).ToString() + "_" + (j).ToString()
                    + "_room" + (i).ToString() + "_" + (j - 1).ToString(), out hallway))
                {
                    slot.SetHall(hallway);
                    HallSlots.Add(hallway.Id, slot);
                }
                else
                    slot.SetEmpty();
            }
        }
    }

    public void SetScoutingRadar()
    {
        scoutRadar.gameObject.SetActive(true);
        scoutRadar.Reset();
        scoutRadar.state.SetAnimation(0, "pulse", false);
        if (LastCurrentRoom != null)
        {
            scoutRadar.transform.SetParent(LastCurrentRoom.SlotRect, false);
            scoutRadar.transform.position = LastCurrentRoom.SlotRect.position;
        }
        else if (LastHallwaySector != null)
        {
            scoutRadar.transform.SetParent(LastHallwaySector.rectTransform, false);
            scoutRadar.transform.position = LastHallwaySector.rectTransform.position;
        }
    }
    public void ShowAvailableRooms(Room room)
    {
        foreach(var door in room.Doors)
        {
            Hallway hallway = Dungeon.Hallways[door.TargetArea];
            RoomSlots[hallway.OppositeRoom(room).Id].SetMovable(true);
        }
    }
    public void HideAvailableRooms(Room room)
    {
        foreach (var door in room.Doors)
        {
            Hallway hallway = Dungeon.Hallways[door.TargetArea];
            RoomSlots[hallway.OppositeRoom(room).Id].SetMovable(false);
        }
    }

    public void SetCurrentIndicator(Room room)
    {
        if (LastHallwaySector != null)
        {
            LastHallwaySector.RemoveIndicator();   
            LastHallwaySector = null;
        }

        if (LastCurrentRoom != null)
            LastCurrentRoom.animator.SetBool("IsCurrent", false);

        LastCurrentRoom = RoomSlots[room.Id];
        LastCurrentRoom.animator.SetBool("IsCurrent", true);
    }
    public void SetCurrentIndicator(HallSector hallsector)
    {
        if (LastCurrentRoom != null)
        {
            LastCurrentRoom.animator.SetBool("IsCurrent", false);
            LastCurrentRoom = null;
        }

        if (LastHallwaySector != null)
            LastHallwaySector.RemoveIndicator();       


        LastHallwaySector = HallSlots[hallsector.Hallway.Id].RaidMapHallSectorSlots.Find(item => item.Sector.Id == hallsector.Id);
        if(LastHallwaySector != null)
            LastHallwaySector.SetIndicator();
    }
    public void SetMovingRoom(Room room)
    {
        RoomSlots[room.Id].MarkRoom();
    }

    public void OnHallwayEnter(Hallway hallway, Room fromRoom, Room targetRoom)
    {
        UpdateArea(fromRoom);
        HideAvailableRooms(fromRoom);
        SetMovingRoom(targetRoom);
    }
    public void OnRoomEnter(Room room, Hallway fromHallway)
    {
        UpdateArea(room);
        SetCurrentIndicator(room);

        if (fromHallway != null)
            UpdateArea(fromHallway.OppositeRoom(room));
    }

    public void FocusTarget(float focusScaleTarget = 2.2f)
    {
        focus = true;
        focusTimeMax = 3f;
        targetScale = focusScaleTarget;

        RectTransform currentMarker = LastHallwaySector == null ? LastCurrentRoom.SlotRect : LastHallwaySector.rectTransform;
        float width = rightTop.position.x - leftBot.position.x;
        float height = rightTop.position.y - leftBot.position.y;
        float markerX = currentMarker.position.x - leftBot.position.x;
        float markerY = currentMarker.position.y - leftBot.position.y;
        normalizedTarget = new Vector2(markerX / width, markerY / height);
    }
    public void InstantScaleTarget(float focusScaleTarget = 2.2f)
    {
        targetScale = focusScaleTarget;
        mapContent.localScale = new Vector3(focusScaleTarget, focusScaleTarget, 0);
    }
    public void InstantFocusTarget()
    {
        RectTransform currentMarker = LastHallwaySector == null ? LastCurrentRoom.SlotRect : LastHallwaySector.rectTransform;
        float width = rightTop.position.x - leftBot.position.x;
        float height = rightTop.position.y - leftBot.position.y;
        float markerX = currentMarker.position.x - leftBot.position.x;
        float markerY = currentMarker.position.y - leftBot.position.y;
        normalizedTarget = new Vector2(markerX / width, markerY / height);
        scrollRect.normalizedPosition =  normalizedTarget;
    }
    public void Unfocus()
    {
        if(focus)
        {
            targetScale = mapContent.localScale.x;
            focus = false;
        }
    }

    public void UpdateArea(Area area)
    {
        if(area is Room)
        {
            RoomSlots[area.Id].UpdateRoom();
            if (RaidSceneManager.SceneState == DungeonSceneState.Hall)
                if (RaidSceneManager.HallwayView.TargetRoom == area)
                    SetMovingRoom(RaidSceneManager.HallwayView.TargetRoom);
        }
        else if (area is HallSector)
        {
            var hallSector = area as HallSector;
            HallSlots[hallSector.Hallway.Id].RaidMapHallSectorSlots.Find(item => item.Sector.Id == area.Id).UpdateSector();
        }
    }
    public void OnHallSectorEnter(HallSector sector)
    {
        RaidMapHallSectorSlot slot = HallSlots[sector.Hallway.Id].RaidMapHallSectorSlots.Find(item => item.Sector.Id == sector.Id);
        slot.UpdateSector();
    }
    public void OnHallSectorExit(HallSector sector)
    {
        RaidMapHallSectorSlot slot = HallSlots[sector.Hallway.Id].RaidMapHallSectorSlots.Find(item => item.Sector.Id == sector.Id);
        slot.UpdateSector();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }

    void Update()
    {
        if (focus)
        {
            focusTimeMax -= Time.deltaTime;
            scrollRect.normalizedPosition = Vector2.SmoothDamp(scrollRect.normalizedPosition, normalizedTarget, ref focusVelocity, 0.8f);
            if (scrollRect.normalizedPosition == normalizedTarget || focusTimeMax < 0)
                focus = false;
        }
        if (isMouseOver)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                float scrollRange = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
                targetScale = Mathf.Clamp(mapContent.localScale.x + scrollRange, minScrollScale, maxScrollScale);
                focus = false;
            }
        }
        Vector2 normalized = scrollRect.normalizedPosition;
        float finalScale = Mathf.SmoothDamp(mapContent.localScale.x, targetScale, ref velocity, focus ? 0.6f : 0.3f);
        mapContent.localScale = new Vector3(finalScale, finalScale, 0);
        if (focus == false)
            scrollRect.normalizedPosition = normalized;
    }
}
