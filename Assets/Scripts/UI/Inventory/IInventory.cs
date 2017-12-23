public interface IInventory
{
    InventoryConfiguration Configuration { get; }
    InventoryState State { get; }

    bool CheckSingleInventorySpace(ItemDefinition item);
}
