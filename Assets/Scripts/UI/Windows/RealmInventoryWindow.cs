using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class RealmInventoryWindow : MonoBehaviour, IInventory
{
    public Button closeButton;
    public RectTransform trinketBox;
    public InventoryRow rowTemplate;

    public RealmInventory RealmInventory
    {
        get;
        set;
    }
    public List<InventoryRow> InventoryRows
    {
        get;
        private set;
    }
    public List<InventorySlot> InventorySlots
    {
        get
        {
            return trinketSlots;
        }
    }
    public Hero CurrentHero
    {
        get
        {
            return null;
        }
    }
    public InventoryConfiguration Configuration
    {
        get
        {
            return InventoryConfiguration.TrinketInventory;
        }
    }
    public InventoryState State
    {
        get
        {
            return InventoryState.Normal;
        }
    }
    public int CurrentCapacity
    { 
        get;
        private set;
    }
    public int MaxCapacity
    {
        get;
        private set;
    }

    public event WindowEvent onWindowClose;

    private List<InventorySlot> trinketSlots = new List<InventorySlot>();

    void Awake()
    {
        DragManager.Instanse.onStartDraggingInventorySlot += Instanse_onStartDraggingInventorySlot;
        DragManager.Instanse.onEndDraggingInventorySlot += Instanse_onEndDraggingInventorySlot;
    }

    void OnDestroy()
    {
        DragManager.Instanse.onStartDraggingInventorySlot -= Instanse_onStartDraggingInventorySlot;
        DragManager.Instanse.onEndDraggingInventorySlot -= Instanse_onEndDraggingInventorySlot;
    }

    void Instanse_onEndDraggingInventorySlot(InventorySlot slot)
    {
        if (gameObject.activeSelf)
        {
            for (int i = 0; i < InventorySlots.Count; i++)
            {
                InventorySlots[i].SetActiveState(true);
            }
        }
    }

    void Instanse_onStartDraggingInventorySlot(InventorySlot slot)
    {
        if(gameObject.activeSelf && slot.Inventory.Configuration == InventoryConfiguration.Equipment)
        {
            var charEquipmentPanel = slot.Inventory as CharEquipmentPanel;
            for (int i = 0; i < InventorySlots.Count; i++)
            {
                if (InventorySlots[i].HasItem)
                {
                    var trinket = InventorySlots[i].SlotItem.ItemData as Trinket;
                    if (trinket != null)
                    {
                        if (trinket.EquipLimit == 1 && charEquipmentPanel.ContainsItem(trinket))
                        {
                            InventorySlots[i].SetActiveState(false);
                        }

                        if (trinket.ClassRequirements.Count > 0)
                        {
                            if (!trinket.ClassRequirements.Contains(charEquipmentPanel.CurrentHero.Class))
                            {
                                InventorySlots[i].SetActiveState(false);
                            }
                        }
                    }
                }
            }
        }
    }

    public void Populate()
    {
        RealmInventory = DarkestDungeonManager.Campaign.RealmInventory;
        InventoryRows = new List<InventoryRow>();

        int trinketCount = RealmInventory.Trinkets.Count;
        int rowCount = Mathf.Max(3, trinketCount / 7 + 1);
        int trinketsLoaded = 0;

        CurrentCapacity = trinketCount;
        MaxCapacity = rowCount * 7;

        for(int i = 0; i < rowCount; i++)
        {
            InventoryRow newRow = Instantiate(rowTemplate);
            newRow.Initialize(this);
            newRow.onRowEmptied += RealmInventoryWindow_onRowEmptied;
            newRow.RectTransform.SetParent(trinketBox, false);
            InventoryRows.Add(newRow);
            InventorySlots.AddRange(newRow.InventorySlots);

            for (int j = 0; j < 7; j++ )
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
            InventoryRows[i].onRowEmptied += RealmInventoryWindow_onRowEmptied;
            for (int j = 0; j < InventoryRows[i].SlotCount; j++)
            {
                InventoryRows[i].InventorySlots[j].onDropIn += RealmInventoryWindow_onDropIn;
                InventoryRows[i].InventorySlots[j].onDropOut += RealmInventoryWindow_onDropOut;
            }
        }
        UpdateWindow();
    }
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

            if (EstateSceneManager.Instanse.characterWindow.IsOpened)
                EstateSceneManager.Instanse.characterWindow.UpdateCharacterInfo();
        }

        if (hasUnequipped)
            DarkestSoundManager.PlayOneShot("event:/ui/dungeon/trink_unequip");
    }
    public void SortByName()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/sort_by");

        RealmInventory.Trinkets.Sort((x, y) =>
            LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", x.Id)).
            CompareTo(LocalizationManager.GetString(ToolTipManager.GetConcat("str_inventory_title_trinket", y.Id))));

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
            return result == 0 ? x.Id.CompareTo(y.Id) : result;
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
                    int result = x.ClassRequirements[0].CompareTo(y.ClassRequirements[0]);
                    return result == 0 ? x.Id.CompareTo(y.Id) : result;
                }
                else
                    return -1;
            }
            else if (y.ClassRequirements.Count > 0)
                return 1;
            else
                return x.Id.CompareTo(y.Id);
        });

        int trinketCount = RealmInventory.Trinkets.Count;
        int trinketsLoaded = Mathf.Min(trinketCount, InventorySlots.Count);

        for (int i = 0; i < trinketsLoaded; i++)
            InventorySlots[i].CreateItem(RealmInventory.Trinkets[i]);

        for (int i = trinketsLoaded; i < InventorySlots.Count; i++)
            InventorySlots[i].DeleteItem();
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
        UpdateWindow();
    }

    void RealmInventoryWindow_onDropOut(InventorySlot slot, InventoryItem itemDrop)
    {
        CurrentCapacity--;
        DarkestDungeonManager.Campaign.RealmInventory.Trinkets.Remove(itemDrop.ItemData as Trinket);
        UpdateWindow();
    }
    void RealmInventoryWindow_onDropIn(InventorySlot slot, InventoryItem itemDrop)
    {
        CurrentCapacity++;
        DarkestDungeonManager.Campaign.RealmInventory.Trinkets.Add(itemDrop.ItemData as Trinket);
        if(CurrentCapacity == MaxCapacity)
            AddRow();
        UpdateWindow();
    }
    void RealmInventoryWindow_onRowEmptied(int rowNumber)
    {
        if(rowNumber > 3 && rowNumber == InventoryRows.Count)
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
            UpdateWindow();
        }    
    }

    public InventoryRow AddRow()
    {
        InventoryRow newRow = Instantiate(rowTemplate);
        newRow.Initialize(this);
        newRow.RectTransform.SetParent(trinketBox,false);
        InventoryRows.Add(newRow);
        InventorySlots.AddRange(newRow.InventorySlots);

        newRow.RowNumber = InventoryRows.Count;
        MaxCapacity += newRow.SlotCount;
        newRow.onRowEmptied += RealmInventoryWindow_onRowEmptied;

        for (int j = 0; j < newRow.SlotCount; j++)
        {
            newRow.InventorySlots[j].onDropIn += RealmInventoryWindow_onDropIn;
            newRow.InventorySlots[j].onDropOut += RealmInventoryWindow_onDropOut;
        }
        UpdateWindow();
        return newRow;
    }
    public void UpdateWindow()
    {
        //Debug.Log("Capacity: " + CurrentCapacity + " Trinket count: " + DarkestDungeonManager.Campaign.RealmInventory.Trinkets.Count);
    }
    public void WindowClosed()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/trinket_close");
        
        if (onWindowClose != null)
            onWindowClose();
    }

    public bool CheckSingleInventorySpace(ItemDefinition item)
    {
        return false;
    }
    public void DistributeFromShopItem(ShopSlot slot, InventorySlot dropSlot)
    {
        return;
    }
}
