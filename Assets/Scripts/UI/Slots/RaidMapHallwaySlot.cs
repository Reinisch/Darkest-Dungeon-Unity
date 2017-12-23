using UnityEngine;
using System.Collections.Generic;

public class RaidMapHallwaySlot : MonoBehaviour
{
    public RectTransform RectTransform { get { return rectTransform ?? (rectTransform = GetComponent<RectTransform>()); } }
    public List<RaidMapHallSectorSlot> RaidMapHallSectorSlots { get { return hallSectorSlots; } }

    private RectTransform rectTransform;
    private readonly List<RaidMapHallSectorSlot> hallSectorSlots = new List<RaidMapHallSectorSlot>();

    public void SetHall(Hallway hallway)
    {
        for (int i = 1; i < hallway.Halls.Count - 1; i++)
            RaidMapHallSectorSlots[i - 1].SetSector(hallway.Halls[i]);
    }
}