using UnityEngine;
using System.Collections.Generic;

public class RaidHallway : MonoBehaviour
{
    private const int BorderRoomNumber = 2;

    public List<RaidHallSector> HallSectors { get; private set; }

    public int SectorCount { get { return HallSectors.Count; } }
    public int ActiveSectorCount { get { return HallSectors.Count - BorderRoomNumber * 2; } }
    public int StartDoorSector { get { return BorderRoomNumber; } }
    public int EndDoorSector { get { return ActiveSectorCount + BorderRoomNumber - 1; } }

    private void Awake()
    {
        HallSectors = new List<RaidHallSector>(GetComponentsInChildren<RaidHallSector>());
    }

    public void LoadHallway(Hallway hallway, Direction direction, bool loadBattleSave)
    {
        while(hallway.HallCount > ActiveSectorCount)
        {
            var newSector = Instantiate(HallSectors[StartDoorSector - 1].gameObject).GetComponent<RaidHallSector>();
            newSector.RectTransform.SetParent(HallSectors[StartDoorSector].RectTransform.parent, false);
            newSector.RectTransform.SetSiblingIndex(HallSectors[StartDoorSector].RectTransform.GetSiblingIndex() + 1);
            HallSectors.Insert(StartDoorSector + 1, newSector);
        }
        while(hallway.HallCount < ActiveSectorCount)
        {
            var removedSector = HallSectors[StartDoorSector + 1];
            HallSectors.RemoveAt(StartDoorSector + 1);
            Destroy(removedSector.gameObject);
        }

        if (direction == Direction.Bot || direction == Direction.Left )
        {
            RaidSceneManager.Raid.RaidParty.IsMovingLeft = true;

            for (int i = StartDoorSector; i <= EndDoorSector; i++)
                HallSectors[i].LoadSector(hallway.Halls[i - StartDoorSector]);
        }
        else if (direction == Direction.Top || direction == Direction.Right)
        {
            RaidSceneManager.Raid.RaidParty.IsMovingLeft = false;

            for (int i = EndDoorSector; i >= StartDoorSector; i--)
                HallSectors[i].LoadSector(hallway.Halls[EndDoorSector - i]);
        }

        for (int i = 0; i < StartDoorSector; i++)
            HallSectors[i].UpdateBorder();
        for (int i = EndDoorSector + 1; i < SectorCount; i++)
            HallSectors[i].UpdateBorder();
    }
}