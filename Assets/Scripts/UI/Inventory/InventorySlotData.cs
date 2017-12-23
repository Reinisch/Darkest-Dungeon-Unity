public class InventorySlotData
{
    public ItemDefinition Item { get; set; }
    public ItemData ItemData { get; set; }

    public InventorySlotData()
    {
    }

    public InventorySlotData(ItemDefinition itemDefinition)
    {
        Item = itemDefinition;

        if (DarkestDungeonManager.Data.ItemExists(Item))
            ItemData = DarkestDungeonManager.Data.Items[itemDefinition.Type][itemDefinition.Id];
    }
}
