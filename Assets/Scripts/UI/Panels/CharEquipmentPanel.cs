using System;
using UnityEngine;

public class CharEquipmentPanel : MonoBehaviour, IInventory
{
    [SerializeField]
    private EquipmentSlot weaponSlot;
    [SerializeField]
    private EquipmentSlot armorSlot;
    [SerializeField]
    private InventorySlot leftSlot;
    [SerializeField]
    private InventorySlot rightSlot;

    public Hero CurrentHero { get; private set; }
    public InventoryState State { get; private set; }
    public InventoryConfiguration Configuration { get { return InventoryConfiguration.Equipment; } }

    public event Action EventPanelChanged;

    private void Awake()
    {
        leftSlot.Initialize(this);
        rightSlot.Initialize(this);

        leftSlot.EventDropOut += LeftTrinketSlotDropOut;
        rightSlot.EventDropOut += RightTrinketSlotDropOut;
        leftSlot.EventDropIn += LeftTrinketSlotDropIn;
        rightSlot.EventDropIn += RightTrinketSlotDropIn;
        leftSlot.EventSwap += LeftTrinketSlotSwap;
        rightSlot.EventSwap += RightTrinketSlotSwap;
        DragManager.Instanse.EventStartDraggingInventorySlot += DragManagerStartDraggingInventorySlot;
        DragManager.Instanse.EventEndDraggingInventorySlot += DragManagerEndDraggingInventorySlot;
    }

    private void OnDestroy()
    {
        DragManager.Instanse.EventStartDraggingInventorySlot -= DragManagerStartDraggingInventorySlot;
        DragManager.Instanse.EventEndDraggingInventorySlot -= DragManagerEndDraggingInventorySlot;
    }

    public void UpdateEquipmentPanel(Hero hero, bool allowedInteraction)
    {
        CurrentHero = hero;

        weaponSlot.UpdateEquipment(CurrentHero.Weapon, hero);
        armorSlot.UpdateEquipment(CurrentHero.Armor, hero);
        leftSlot.InteractionDisabled = !allowedInteraction;
        rightSlot.InteractionDisabled = !allowedInteraction;

        if (hero.LeftTrinket != null)
            leftSlot.CreateItem(hero.LeftTrinket);
        else
            leftSlot.DeleteItem();

        if (hero.RightTrinket != null)
            rightSlot.CreateItem(hero.RightTrinket);
        else
            rightSlot.DeleteItem();
    }

    public void SetDisabled()
    {
        State = InventoryState.Disabled;
    }

    public void SetActive()
    {
        State = InventoryState.Peaceful;
    }

    public bool ContainsItem(ItemData itemData)
    {
        return leftSlot.SlotItem.ItemData == itemData || rightSlot.SlotItem.ItemData == itemData;
    }

    public bool CheckSingleInventorySpace(ItemDefinition item)
    {
        return false;
    }

    private void DragManagerEndDraggingInventorySlot(InventorySlot slot)
    {
        rightSlot.SetActiveState(true);
        leftSlot.SetActiveState(true);
        leftSlot.SlotItem.SetOverlayDefault();
        rightSlot.SlotItem.SetOverlayDefault();
    }

    private void DragManagerStartDraggingInventorySlot(InventorySlot slot)
    {
        if (CurrentHero != null && DarkestDungeonManager.Campaign.Heroes.Contains(CurrentHero))
        {
            if (slot.HasItem && slot.SlotItem.Item.Type == "trinket")
            {
                var trinket = slot.SlotItem.ItemData as Trinket;
                if (trinket != null)
                {
                    if (trinket.EquipLimit == 1)
                    {
                        if (leftSlot.SlotItem.ItemData == trinket)
                        {
                            rightSlot.SetActiveState(false);
                            rightSlot.SlotItem.SetDropUnavailable();
                        }
                        else if (rightSlot.SlotItem.ItemData == trinket)
                        {
                            leftSlot.SetActiveState(false);
                            leftSlot.SlotItem.SetDropUnavailable();
                        }
                    }

                    if (trinket.ClassRequirements.Count > 0)
                    {
                        if (!trinket.ClassRequirements.Contains(CurrentHero.Class))
                        {
                            leftSlot.SetActiveState(false);
                            leftSlot.SlotItem.SetDropUnavailable();
                            rightSlot.SetActiveState(false);
                            rightSlot.SlotItem.SetDropUnavailable();
                        }
                    }
                }
            }
        }
    }

    private void RightTrinketSlotSwap(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Right);
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_equip");
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Right);
        }
        PanelChanged();
    }

    private void LeftTrinketSlotSwap(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Left);
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_equip");
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Left);
        }
        PanelChanged();
    }

    private void RightTrinketSlotDropIn(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Right);
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_equip");
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Right);
        }
        PanelChanged();
    }

    private void LeftTrinketSlotDropIn(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Left);
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_equip");
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Left);
        }
        PanelChanged();
    }

    private void RightTrinketSlotDropOut(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Unequip(TrinketSlot.Right);
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_unequip");
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Right);
        }
        PanelChanged();
    }

    private void LeftTrinketSlotDropOut(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Unequip(TrinketSlot.Left);
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_unequip");
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Left);
        }
        PanelChanged();
    }

    private void PanelChanged()
    {
        if (EventPanelChanged != null)
            EventPanelChanged();
    }
}
