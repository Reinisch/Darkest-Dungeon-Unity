using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public delegate void InventoryRowEvent(int rowNumber);

public class InventoryRow : MonoBehaviour
{
    public int FilledSlots { get; set; }

    public RectTransform RectTransform { get; private set; }
    public List<InventorySlot> InventorySlots { get; private set; }

    public bool HasItems
    { 
        get { return FilledSlots > 0; }
    }
    public bool HasEmptySlot
    {
        get { return FilledSlots < InventorySlots.Count; }
    }
    public int SlotCount
    {
        get { return InventorySlots.Count;  }
    }
    public int RowNumber { get; set; }

    public event InventoryRowEvent onRowEmptied;

    public void ItemAdded()
    {
        FilledSlots++;
    }

    void InventoryRow_onDropOut(InventorySlot slot, InventoryItem item)
    {
        FilledSlots--;
        if (FilledSlots == 0)
            RowEmptied();
    }
    void InventoryRow_onDropIn(InventorySlot slot, InventoryItem item)
    {
        FilledSlots++;
    }

    public void Initialize(RealmInventoryWindow realmInventory)
    {
        RectTransform = GetComponent<RectTransform>();
        InventorySlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>());
        for (int i = 0; i < InventorySlots.Count; i++)
        {
            InventorySlots[i].Initialize(realmInventory);
            InventorySlots[i].onDropIn += InventoryRow_onDropIn;
            InventorySlots[i].onDropOut += InventoryRow_onDropOut;
        }
    }

    public void RowEmptied()
    {
        if (onRowEmptied != null)
            onRowEmptied(RowNumber);
    }
}
