using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IInventory
{
    List<InventorySlot> InventorySlots { get; }
    InventoryConfiguration Configuration { get; }
    InventoryState State { get; }
    Hero CurrentHero { get; }

    bool CheckSingleInventorySpace(ItemDefinition item);
    void DistributeFromShopItem(ShopSlot slot, InventorySlot dropSlot);
}
