using UnityEngine;
using System.Collections.Generic;

public delegate void InventoryRowEvent(int rowNumber);

public class InventoryRow : MonoBehaviour
{
    public RectTransform RectTransform { get; private set; }
    public List<InventorySlot> InventorySlots { get; private set; }
    public bool HasItems {  get { return FilledSlots > 0; } }
    public bool HasEmptySlot { get { return FilledSlots < InventorySlots.Count; } }
    public int SlotCount { get { return InventorySlots.Count;  } }
    public int RowNumber { private get; set; }

    private int FilledSlots { get; set; }

    public event InventoryRowEvent EventRowEmptied;

    public void Initialize(RealmInventoryWindow realmInventory)
    {
        RectTransform = GetComponent<RectTransform>();
        InventorySlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>());
        for (int i = 0; i < InventorySlots.Count; i++)
        {
            InventorySlots[i].Initialize(realmInventory);
            InventorySlots[i].EventDropIn += InventoryRowDropIn;
            InventorySlots[i].EventDropOut += InventoryRowDropOut;
        }
    }

    public void ItemAdded()
    {
        FilledSlots++;
    }

    private void InventoryRowDropOut(InventorySlot slot, InventoryItem item)
    {
        FilledSlots--;
        if (FilledSlots == 0)
            RowEmptied();
    }

    private void InventoryRowDropIn(InventorySlot slot, InventoryItem item)
    {
        FilledSlots++;
    }

    private void RowEmptied()
    {
        if (EventRowEmptied != null)
            EventRowEmptied(RowNumber);
    }
}
