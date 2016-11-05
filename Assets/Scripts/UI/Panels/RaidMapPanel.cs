using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class RaidMapPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject roomSlotTemplate;
    public RaidMapHallwaySlot predefinedHallSlotTemplate;
    public RaidMapHallSectorSlot hallSectorSlotTemplate;

    public RectTransform roomContainer;
    public RectTransform hallContainer;
    public RectTransform hallIndicator;

    public RectTransform mapContent;
    public ScrollRect scrollRect;
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
    int baseHallSize = 24;

    public Dictionary<string, RaidMapRoomSlot> RoomSlots { get; set; }
    public Dictionary<string, RaidMapHallwaySlot> HallSlots { get; set; }

    public RaidMapRoomSlot LastCurrentRoom { get; set; }
    public RaidMapHallSectorSlot LastHallwaySector { get; set; }

    Vector2 GetGridOffset(DungeonRoom room)
    {
        return new Vector2(baseHallSize * room.GridX, baseHallSize * room.GridY);
    }
    Vector2 GetGridOffset(HallSector sector)
    {
        return new Vector2(baseHallSize * sector.GridX, baseHallSize * sector.GridY);
    }
    public void LoadDungeon(Dungeon dungeon)
    {
        RoomSlots = new Dictionary<string, RaidMapRoomSlot>();
        HallSlots = new Dictionary<string, RaidMapHallwaySlot>();

        Dungeon = dungeon;

        #region Map UI Instantiation
        int horizontalSize = baseHallSize * dungeon.GridSizeX;
        int verticalSize = baseHallSize * dungeon.GridSizeY;
        Vector2 botLeft = new Vector2(-horizontalSize / 2, -verticalSize / 2);
        foreach (var generatedRoom in Dungeon.Rooms)
        {
            RaidMapRoomSlot roomSlot = Instantiate(roomSlotTemplate).GetComponentInChildren<RaidMapRoomSlot>();
            roomSlot.container.SetParent(roomContainer, false);
            roomSlot.container.localPosition = botLeft + GetGridOffset(generatedRoom.Value);
            roomSlot.SetRoom(generatedRoom.Value);
            RoomSlots.Add(generatedRoom.Value.Id, roomSlot);
        }

        foreach (var generatedHallway in Dungeon.Hallways)
        {
            Vector2 hallLocation = botLeft + GetGridOffset(generatedHallway.Value.RoomA);

            var hallSlot = Instantiate(predefinedHallSlotTemplate);
            hallSlot.RectTransform.SetParent(hallContainer, false);
            hallSlot.RectTransform.localPosition = hallLocation;

            foreach (var generatedSector in generatedHallway.Value.Halls)
            {
                if (generatedSector.Type == AreaType.Door)
                    continue;

                hallLocation = botLeft + GetGridOffset(generatedSector);
                var hallSectorSlot = Instantiate(hallSectorSlotTemplate);
                hallSectorSlot.rectTransform.SetParent(hallContainer, false);
                hallSectorSlot.rectTransform.localPosition = hallLocation;
                hallSectorSlot.rectTransform.SetParent(hallSlot.RectTransform, true);
                hallSlot.RaidMapHallSectorSlots.Add(hallSectorSlot);
            }
            hallSlot.SetHall(generatedHallway.Value);
            HallSlots.Add(generatedHallway.Value.Id, hallSlot);
        }
        #endregion
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
    public void ShowAvailableRooms(DungeonRoom room)
    {
        foreach(var door in room.Doors)
        {
            Hallway hallway = Dungeon.Hallways[door.TargetArea];
            RoomSlots[hallway.OppositeRoom(room).Id].SetMovable(true);
        }
    }
    public void HideAvailableRooms(DungeonRoom room)
    {
        foreach (var door in room.Doors)
        {
            Hallway hallway = Dungeon.Hallways[door.TargetArea];
            RoomSlots[hallway.OppositeRoom(room).Id].SetMovable(false);
        }
    }

    public void SetCurrentIndicator(DungeonRoom room)
    {
        if (LastHallwaySector != null)
        {
            LastHallwaySector.RemoveIndicator(hallIndicator);   
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
            LastHallwaySector.RemoveIndicator(hallIndicator);       


        LastHallwaySector = HallSlots[hallsector.Hallway.Id].RaidMapHallSectorSlots.Find(item => item.Sector.Id == hallsector.Id);
        if(LastHallwaySector != null)
            LastHallwaySector.SetIndicator(hallIndicator);
    }
    public void SetMovingRoom(DungeonRoom room)
    {
        RoomSlots[room.Id].MarkRoom();
    }

    public void OnHallwayEnter(Hallway hallway, DungeonRoom fromRoom, DungeonRoom targetRoom)
    {
        UpdateArea(fromRoom);
        HideAvailableRooms(fromRoom);
        SetMovingRoom(targetRoom);
    }
    public void OnRoomEnter(DungeonRoom room, Hallway fromHallway)
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
        if(area is DungeonRoom)
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
