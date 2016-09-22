using UnityEngine;
using System.Collections.Generic;

public class FormationRanks : MonoBehaviour
{
    public const int SlotNumber = 4;
    public const int SlotSize = 140;

    public bool facingRight = true;

    public RectTransform RectTransform { get; set; }
    public List<FormationRanksSlot> Slots { get; set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        Slots = new List<FormationRanksSlot>(GetComponentsInChildren<FormationRanksSlot>());

        if (facingRight)
            Slots.Reverse();
    }

    public void AddUnit(FormationUnit unit, int targetRank)
    {
        var freeSlot = Slots.Find(slot => !slot.HasUnit);
        if (freeSlot == null)
            Debug.LogError("No free rank for " + unit.name);
        else
            freeSlot.PutInSlot(unit);

        foreach (var slot in Slots)
            slot.UpdateSlot();
    }
    public void RedistributeParty(FormationParty party)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (i < party.Units.Count)
                Slots[i].PutInSlot(party.Units[party.Units.Count - i - 1]);
            else
                Slots[i].ClearSlot();
        }
    }
    public void DistributeParty(FormationParty party)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (i < party.Units.Count)
                Slots[i].PutInSlot(party.Units[party.Units.Count - i - 1]);
            else
                Slots[i].ClearSlot();
        }

        if (!facingRight)
            for (int i = 0; i < party.Units.Count; i++)
                party.Units[i].InstantFlip();

    }

    public void InstantRelocation()
    {
        foreach (var slot in Slots)
        {
            slot.Teleport();
        }
    }
}
