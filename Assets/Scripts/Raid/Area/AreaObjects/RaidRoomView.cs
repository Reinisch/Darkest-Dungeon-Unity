using UnityEngine;

public class RaidRoomView : MonoBehaviour
{
    [SerializeField]
    private RaidHallwayPassage hallwayPassage;
    [SerializeField]
    private RectTransform startingPosition;
    [SerializeField]
    private RaidRoom raidRoom;
    [SerializeField]
    private CanvasGroup canvasGroup;

    private Hallway LastHallway { get; set; }

    public RaidRoom RaidRoom { get { return raidRoom; } }
    public RectTransform StartingPosition { get { return startingPosition; } }
    public RaidHallwayPassage HallwayPassage { get { return hallwayPassage; } }

    public void LoadRoom(DungeonRoom room, HallSector fromSector, bool savedBattle = false)
    {
        RaidRoom.LoadRoom(room, savedBattle);
        LastHallway = fromSector == null ? null : fromSector.Hallway;

        RaidSceneManager.MapPanel.OnRoomEnter(room, LastHallway);
    }

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
}