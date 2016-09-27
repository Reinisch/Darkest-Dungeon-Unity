using UnityEngine;
using System.Collections.Generic;

public class RaidMapHallwaySlot : MonoBehaviour
{
    private RectTransform rectTransform;
    private List<RaidMapHallSectorSlot> hallSectorSlots = new List<RaidMapHallSectorSlot>();

    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }
    public List<RaidMapHallSectorSlot> RaidMapHallSectorSlots
    {
        get
        {
            return hallSectorSlots;
        }
    }

    public void SetHall(Hallway hallway)
    {
        for (int i = 1; i < hallway.Halls.Count - 1; i++)
            RaidMapHallSectorSlots[i - 1].SetSector(hallway.Halls[i]);
    }
}