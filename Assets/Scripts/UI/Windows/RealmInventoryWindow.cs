using System;
using UnityEngine;
using System.Collections.Generic;

public class RealmInventoryWindow : MonoBehaviour, IInventory
{
    [SerializeField]
    private RectTransform trinketBox;
    [SerializeField]
    private InventoryRow rowTemplate;

    public InventoryConfiguration Configuration { get { return InventoryConfiguration.TrinketInventory; } }
    public InventoryState State { get { return InventoryState.Normal; } }

    private RealmInventory RealmInventory { get; set; }
    private List<InventorySlot> InventorySlots { get { return trinketSlots; } }
    private List<InventoryRow> InventoryRows { get; set; }
    private int CurrentCapacity { get; set; }
    private int MaxCapacity { get; set; }

    private readonly List<InventorySlot> trinketSlots = new List<InventorySlot>();

    public event Action EventWindowClosed;

    private void Awake()
    {
        DragManager.Instanse.EventStartDraggingInventorySlot += DragManagerStartDraggingInventorySlot;
        DragManager.Instanse.EventEndDraggingInventorySlot += DragManagerEndDraggingInventorySlot;
    }

    private void OnDestroy()
    {
        DragManager.Instanse.EventStartDraggingInventorySlot -= DragManagerStartDraggingInventorySlot;
        DragManager.Instanse.EventEndDraggingInventorySlot -= DragManagerEndDraggingInventorySlot;
    }

    #region Inventory Buttons

    public void UnequipAllHeroes()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");
        bool hasUnequipped = false;

        for (int i = 0; i < DarkestDungeonManager.Campaign.Heroes.Count; i++)
        {
            var hero = DarkestDungeonManager.Campaign.Heroes[i];

            if (!(hero.Status == HeroStatus.Available || hero.Status == HeroStatus.RaidParty))
                continue;

            if(hero.LeftTrinket != null)
            {
                AddTrinket(hero.LeftTrinket);
                hero.Unequip(TrinketSlot.Left);
                hasUnequipped = true;
            }
            if(hero.RightTrinket != null)
            {
                AddTrinket(hero.RightTrinket);
                hero.Unequip(TrinketSlot.Right);
                hasUnequipped = true;
            }

            if (EstateSceneManager.Instanse.CharacterWindow.IsOpened)
                EstateSceneManager.Instanse.CharacterWindow.UpdateCharacterInfo();
        }

        if (hasUnequipped)
            DarkestSoundManager.PlayOneShot("event:/ui/dungeon/trink_unequip");
    }

    public void SortByName()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        RealmInventory.Trinkets.Sort((x, y) => 
            string.Compare(LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", x.Id)),
            LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", y.Id)), StringComparison.Ordinal));

        int trinketCount = RealmInventory.Trinkets.Count;
        int trinketsLoaded = Mathf.Min(trinketCount, InventorySlots.Count);

        for(int i = 0; i < trinketsLoaded; i++)
            InventorySlots[i].CreateItem(RealmInventory.Trinkets[i]);

        for (int i = trinketsLoaded; i < InventorySlots.Count; i++)
            InventorySlots[i].DeleteItem();
    }

    public void SortByRarity()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        RealmInventory.Trinkets.Sort((x, y) =>
        {
            int result = x.Rarity.CompareTo(y.Rarity);
            return result == 0 ? string.Compare(x.Id, y.Id, StringComparison.Ordinal) : result;
        });
        
        int trinketCount = RealmInventory.Trinkets.Count;
        int trinketsLoaded = Mathf.Min(trinketCount, InventorySlots.Count);

        for (int i = 0; i < trinketsLoaded; i++)
            InventorySlots[i].CreateItem(RealmInventory.Trinkets[i]);

        for (int i = trinketsLoaded; i < InventorySlots.Count; i++)
            InventorySlots[i].DeleteItem();
    }

    public void SortByRestriction()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        RealmInventory.Trinkets.Sort((x, y) =>
        {
            if (x.ClassRequirements.Count > 0)
            {
                if (y.ClassRequirements.Count > 0)
                {
                    int result = string.Compare(x.ClassRequirements[0], y.ClassRequirements[0], StringComparison.Ordinal);
                    return result == 0 ? string.Compare(x.Id, y.Id, StringComparison.Ordinal) : result;
                }
                return -1;
            }
            return y.ClassRequirements.Count > 0 ? 1 : string.Compare(x.Id, y.Id, StringComparison.Ordinal);
        });

        int trinketCount = RealmInventory.Trinkets.Count;
        int trinketsLoaded = Mathf.Min(trinketCount, InventorySlots.Count);

        for (int i = 0; i < trinketsLoaded; i++)
            InventorySlots[i].CreateItem(RealmInventory.Trinkets[i]);

        for (int i = trinketsLoaded; i < InventorySlots.Count; i++)
            InventorySlots[i].DeleteItem();
    }

    #endregion

    public void Populate()
    {
        RealmInventory = DarkestDungeonManager.Campaign.RealmInventory;
        InventoryRows = new List<InventoryRow>();

        int trinketCount = RealmInventory.Trinkets.Count;
        int rowCount = Mathf.Max(3, trinketCount / 7 + 1);
        int trinketsLoaded = 0;

        CurrentCapacity = trinketCount;
        MaxCapacity = rowCount * 7;

        for (int i = 0; i < rowCount; i++)
        {
            InventoryRow newRow = Instantiate(rowTemplate);
            newRow.Initialize(this);
            newRow.EventRowEmptied += RealmInventoryRowEmptied;
            newRow.RectTransform.SetParent(trinketBox, false);
            InventoryRows.Add(newRow);
            InventorySlots.AddRange(newRow.InventorySlots);

            for (int j = 0; j < 7; j++)
            {
                if (trinketsLoaded != trinketCount)
                {
                    Trinket trinket = RealmInventory.Trinkets[trinketsLoaded];
                    newRow.InventorySlots[j].CreateItem(trinket);
                    newRow.ItemAdded();
                    trinketsLoaded++;
                }
                else
                    break;
            }
        }

        for (int i = 0; i < InventoryRows.Count; i++)
        {
            InventoryRows[i].RowNumber = i + 1;
            InventoryRows[i].EventRowEmptied += RealmInventoryRowEmptied;
            for (int j = 0; j < InventoryRows[i].SlotCount; j++)
            {
                InventoryRows[i].InventorySlots[j].EventDropIn += RealmInventorySlotDropIn;
                InventoryRows[i].InventorySlots[j].EventDropOut += RealmInventorySlotDropOut;
            }
        }
    }

    public void AddTrinket(Trinket trinket)
    {
        if(CurrentCapacity < MaxCapacity)
        {
            var freeRow = InventoryRows.Find(row => row.HasEmptySlot);
            var emptySlot = freeRow.InventorySlots.Find(slot => slot.HasItem == false);
            freeRow.ItemAdded();
            emptySlot.CreateItem(trinket);
            CurrentCapacity++;
            RealmInventory.Trinkets.Add(trinket);
        }
        else
        {
            var newRow = AddRow();
            newRow.InventorySlots[0].CreateItem(trinket);
            newRow.ItemAdded();
            CurrentCapacity++;
            RealmInventory.Trinkets.Add(trinket);
        }

        if (CurrentCapacity == MaxCapacity)
            AddRow();
    }

    public void WindowClosed()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/trinket_close");
        
        if (EventWindowClosed != null)
            EventWindowClosed();
    }

    public bool CheckSingleInventorySpace(ItemDefinition item)
    {
        return false;
    }

    private void DragManagerEndDraggingInventorySlot(InventorySlot slot)
    {
        if (gameObject.activeSelf)
        {
            for (int i = 0; i < InventorySlots.Count; i++)
            {
                InventorySlots[i].SetActiveState(true);
            }
        }
    }

    private void DragManagerStartDraggingInventorySlot(InventorySlot slot)
    {
        if (gameObject.activeSelf && slot.Inventory.Configuration == InventoryConfiguration.Equipment)
        {
            var charEquipmentPanel = (CharEquipmentPanel)slot.Inventory;

            foreach (InventorySlot realmSlot in InventorySlots)
            {
                if (!realmSlot.HasItem)
                    continue;

                var trinket = realmSlot.SlotItem.ItemData as Trinket;
                if (trinket == null)
                    continue;

                if (trinket.EquipLimit == 1 && charEquipmentPanel.ContainsItem(trinket))
                {
                    realmSlot.SetActiveState(false);
                }

                if (trinket.ClassRequirements.Count > 0)
                    if (!trinket.ClassRequirements.Contains(charEquipmentPanel.CurrentHero.Class))
                        realmSlot.SetActiveState(false);
            }
        }
    }

    private void RealmInventorySlotDropOut(InventorySlot slot, InventoryItem itemDrop)
    {
        CurrentCapacity--;
        DarkestDungeonManager.Campaign.RealmInventory.Trinkets.Remove(itemDrop.ItemData as Trinket);
    }

    private void RealmInventorySlotDropIn(InventorySlot slot, InventoryItem itemDrop)
    {
        CurrentCapacity++;
        DarkestDungeonManager.Campaign.RealmInventory.Trinkets.Add(itemDrop.ItemData as Trinket);
        if (CurrentCapacity == MaxCapacity)
            AddRow();
    }

    private void RealmInventoryRowEmptied(int rowNumber)
    {
        if (rowNumber > 3 && rowNumber == InventoryRows.Count)
        {
            for (int i = rowNumber; i > 3; i--)
            {
                if (InventoryRows[i - 1].HasItems)
                    break;

                InventorySlots.RemoveAll(slot => InventoryRows[i - 1].InventorySlots.Contains(slot));
                Destroy(InventoryRows[i - 1].gameObject);
                MaxCapacity -= InventoryRows[i - 1].SlotCount;
                InventoryRows.RemoveAt(i - 1);
            }
        }
    }

    private InventoryRow AddRow()
    {
        InventoryRow newRow = Instantiate(rowTemplate);
        newRow.Initialize(this);
        newRow.RectTransform.SetParent(trinketBox, false);
        InventoryRows.Add(newRow);
        InventorySlots.AddRange(newRow.InventorySlots);

        newRow.RowNumber = InventoryRows.Count;
        MaxCapacity += newRow.SlotCount;
        newRow.EventRowEmptied += RealmInventoryRowEmptied;

        for (int j = 0; j < newRow.SlotCount; j++)
        {
            newRow.InventorySlots[j].EventDropIn += RealmInventorySlotDropIn;
            newRow.InventorySlots[j].EventDropOut += RealmInventorySlotDropOut;
        }
        return newRow;
    }
}
