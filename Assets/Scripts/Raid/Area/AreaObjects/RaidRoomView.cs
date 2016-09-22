using UnityEngine;

public class RaidRoomView : MonoBehaviour
{
    public RaidHallwayPassage hallwayPassage;
    public RectTransform startingPosition;
    public RaidRoom raidRoom;
    public CanvasGroup canvasGroup;

    public Hallway LastHallway { get; set; }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void EnableInteraction()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    public void DisableInteraction()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void LoadRoom(Room room, HallSector fromSector, bool savedBattle = false)
    {
        raidRoom.LoadRoom(room, savedBattle);
        LastHallway = fromSector == null ? null : fromSector.Hallway;

        RaidSceneManager.MapPanel.OnRoomEnter(room, LastHallway);
    }
}