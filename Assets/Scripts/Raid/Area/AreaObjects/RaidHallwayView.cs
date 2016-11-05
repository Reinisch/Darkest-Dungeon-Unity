using UnityEngine;
using UnityEngine.UI;

public class RaidHallwayView : MonoBehaviour
{
    public Image farBackground;
    public Image midBackground;

    public RaidBorderWall leftWall;
    public RaidBorderWall rightWall;

    public RaidHallway raidHallway;
    public CanvasGroup canvasGroup;
    public RaidHallwayPassage hallwayPassage;

    public RectTransform startingPosition;

    public Hallway Hallway { get; set; }
    public RaidHallSector CurrentSector { get; set; }
    public DungeonRoom StartingRoom { get; set; }
    public DungeonRoom TargetRoom { get; set; }

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

    public void UpdateEnviroment()
    {
        var sprites = DarkestDungeonManager.Data.DungeonSprites;

        farBackground.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor.back"];
        midBackground.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".corridor.mid"];

        leftWall.borderWall.sprite = sprites[RaidSceneManager.Raid.Quest.Dungeon + ".endhall.1"];
        leftWall.additionalWall.sprite = leftWall.borderWall.sprite;
        rightWall.borderWall.sprite = leftWall.borderWall.sprite;
        rightWall.additionalWall.sprite = leftWall.borderWall.sprite;

        leftWall.rectTransform.localPosition = new Vector3(-raidHallway.ActiveSectorCount * 360 + 80,
            leftWall.rectTransform.localPosition.y, leftWall.rectTransform.localPosition.z);
        rightWall.rectTransform.localPosition = new Vector3(raidHallway.ActiveSectorCount * 360 - 80,
            leftWall.rectTransform.localPosition.y, leftWall.rectTransform.localPosition.z);
    }
}