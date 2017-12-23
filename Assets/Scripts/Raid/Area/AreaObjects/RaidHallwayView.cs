using UnityEngine;
using UnityEngine.UI;

public class RaidHallwayView : MonoBehaviour
{
    [SerializeField]
    private Image farBackground;
    [SerializeField]
    private Image midBackground;

    [SerializeField]
    private RaidBorderWall leftWall;
    [SerializeField]
    private RaidBorderWall rightWall;

    [SerializeField]
    private RaidHallway raidHallway;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private RaidHallwayPassage hallwayPassage;
    [SerializeField]
    private RectTransform startingPosition;

    public RaidHallSector CurrentSector { get; set; }

    public Hallway Hallway { get; private set; }
    public DungeonRoom StartingRoom { get; private set; }
    public DungeonRoom TargetRoom { get; private set; }
    public RaidHallway RaidHallway { get { return raidHallway; } }
    public RaidHallwayPassage HallwayPassage { get { return hallwayPassage; } }
    public RectTransform StartingPosition { get { return startingPosition; } }

    public void LoadHallway(Hallway hallway, Direction direction, DungeonRoom fromRoom, bool loadBattleSave = false)
    {
        Hallway = hallway;

        if (direction == Direction.Bot || direction == Direction.Left)
        {
            StartingRoom = hallway.RoomA;
            TargetRoom = hallway.RoomB;
        }
        else
        {
            StartingRoom = hallway.RoomB;
            TargetRoom = hallway.RoomA;
        }

        raidHallway.LoadHallway(hallway, direction, loadBattleSave);

        UpdateEnviroment();
        if (fromRoom != null)
        {
            if (!(fromRoom.Type == AreaType.BattleTresure || fromRoom.Type == AreaType.BattleCurio || fromRoom.Type == AreaType.Curio))
                fromRoom.Knowledge = Knowledge.Completed;
        }
        RaidSceneManager.MapPanel.OnHallwayEnter(Hallway, StartingRoom, TargetRoom);
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

    private void UpdateEnviroment()
    {
        var sprites = DarkestDungeonManager.Data.DungeonSprites;

        farBackground.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor.back"];
        midBackground.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor.mid"];

        leftWall.BorderWall.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".endhall.1"];
        leftWall.AdditionalWall.sprite = leftWall.BorderWall.sprite;
        rightWall.BorderWall.sprite = leftWall.BorderWall.sprite;
        rightWall.AdditionalWall.sprite = leftWall.BorderWall.sprite;

        leftWall.RectTransform.localPosition = new Vector3(-raidHallway.ActiveSectorCount * 360 + 80,
            leftWall.RectTransform.localPosition.y, leftWall.RectTransform.localPosition.z);
        rightWall.RectTransform.localPosition = new Vector3(raidHallway.ActiveSectorCount * 360 - 80,
            leftWall.RectTransform.localPosition.y, leftWall.RectTransform.localPosition.z);
    }
}