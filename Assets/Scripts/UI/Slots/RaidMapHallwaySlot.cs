using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RaidMapHallwaySlot : MonoBehaviour
{
    public RectTransform hallSet;
    public RectTransform rectTransform;
    public RectTransform slotGroup;

    public List<RaidMapHallSectorSlot> RaidMapHallSectorSlots { get; set; }

    public Hallway Hallway { get; set; }
    public bool HasHall { get; set; }

    public void SetHall(Hallway hallway)
    {
        RaidMapHallSectorSlots = new List<RaidMapHallSectorSlot>(slotGroup.GetComponentsInChildren<RaidMapHallSectorSlot>());

        Hallway = hallway;
        HasHall = true;
        hallSet.gameObject.SetActive(true);
        for (int i = 1; i < hallway.Halls.Count - 1; i++)
            RaidMapHallSectorSlots[i - 1].SetSector(hallway.Halls[i]);
    }

    public void SetEmpty()
    {
        Hallway = null;
        HasHall = false;
        hallSet.gameObject.SetActive(false);
    }
}