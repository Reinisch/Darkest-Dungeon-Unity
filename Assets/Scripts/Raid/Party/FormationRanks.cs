using UnityEngine;
using System.Collections.Generic;

public class FormationRanks : MonoBehaviour
{
    public const int SlotSize = 140;

    [SerializeField]
    private bool facingRight = true;

    public bool FacingRight { get { return facingRight; } }
    public RectTransform RectTransform { get; private set; }
    private List<FormationRanksSlot> Slots { get; set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        Slots = new List<FormationRanksSlot>(GetComponentsInChildren<FormationRanksSlot>());

        if (FacingRight)
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

        if (!FacingRight)
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
