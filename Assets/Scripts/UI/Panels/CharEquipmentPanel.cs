using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public delegate void PanelChangedEvent();

public class CharEquipmentPanel : MonoBehaviour, IInventory
{
    public EquipmentSlot weaponSlot;
    public EquipmentSlot armorSlot;
    public InventorySlot leftSlot;
    public InventorySlot rightSlot;

    public Hero CurrentHero { get; set; }
    public InventoryConfiguration Configuration
    {
        get
        {
            return InventoryConfiguration.Equipment;
        }
    }
    public InventoryState State
    {
        get;
        set;
    }

    public event PanelChangedEvent onPanelChanged;

    void Awake()
    {
        leftSlot.Initialize(this);
        rightSlot.Initialize(this);

        leftSlot.onDropOut += leftTrinketSlot_onDropOut;
        rightSlot.onDropOut += rightTrinketSlot_onDropOut;
        leftSlot.onDropIn += leftTrinketSlot_onDropIn;
        rightSlot.onDropIn += rightTrinketSlot_onDropIn;
        leftSlot.onSwap += leftTrinketSlot_onSwap;
        rightSlot.onSwap += rightTrinketSlot_onSwap;
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
        rightSlot.SetActiveState(true);
        leftSlot.SetActiveState(true);
        leftSlot.SlotItem.SetOverlayDefault();
        rightSlot.SlotItem.SetOverlayDefault();
    }
    void Instanse_onStartDraggingInventorySlot(InventorySlot slot)
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

    void rightTrinketSlot_onSwap(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Right);
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Right);
        }
        PanelChanged();
    }
    void leftTrinketSlot_onSwap(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Left);
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Left);
        }
        PanelChanged();
    }

    public bool ContainsItem(ItemData itemData)
    {
        if (leftSlot.SlotItem.ItemData == itemData || rightSlot.SlotItem.ItemData == itemData)
            return true;

        return false;
    }

    private void rightTrinketSlot_onDropIn(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Right);
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Right);
        }
        PanelChanged();
    }
    private void leftTrinketSlot_onDropIn(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Equip((Trinket)item.ItemData, TrinketSlot.Left);
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Left);
        }
        PanelChanged();
    }
    private void rightTrinketSlot_onDropOut(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Unequip(TrinketSlot.Right);
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Right);
        }
        PanelChanged();
    }
    private void leftTrinketSlot_onDropOut(InventorySlot slot, InventoryItem item)
    {
        if (item.ItemType == "trinket")
        {
            CurrentHero.Unequip(TrinketSlot.Left);
            PanelChanged();
        }
        else
        {
            CurrentHero.Unequip(TrinketSlot.Left);
        }
        PanelChanged();
    }

    public void PanelChanged()
    {
        if (onPanelChanged != null)
            onPanelChanged();
    }

    public void SetDisabled()
    {
        State = InventoryState.Disabled;
    }
    public void SetActive()
    {
        State = InventoryState.Peaceful;
    }

    public void UpdateEquipmentPanel(Hero hero, bool allowedInteraction)
    {
        CurrentHero = hero;

        weaponSlot.UpdateEquipment(CurrentHero.Weapon, hero);
        armorSlot.UpdateEquipment(CurrentHero.Armor, hero);
        leftSlot.InteractionDisabled = !allowedInteraction;
        rightSlot.InteractionDisabled = !allowedInteraction;

        if(hero.LeftTrinket != null)
            leftSlot.CreateItem(hero.LeftTrinket);
        else
            leftSlot.DeleteItem();

        if (hero.RightTrinket != null)
            rightSlot.CreateItem(hero.RightTrinket);
        else
            rightSlot.DeleteItem();
    }

    public List<InventorySlot> InventorySlots
    {
        get;
        set;
    }
    public bool CheckSingleInventorySpace(ItemDefinition item)
    {
        return false;
    }
    public void DistributeFromShopItem(ShopSlot slot, InventorySlot dropSlot)
    {
        throw new System.NotImplementedException();
    }
}
