public class ItemDefinition
{
    public string Type { get; set; }
    public string Id { get; set; }
    public int Amount { get; set; }

    public string ToolTip { get { return DarkestDungeonManager.Data.Items[Type][Id].ToolTip(Amount); } }
    public string ToolTipDiscard { get { return DarkestDungeonManager.Data.Items[Type][Id].ToolTip(Amount, true); } }

    public ItemDefinition()
    {
    }

    public ItemDefinition(string type, string id, int amount)
    {
        Type = type;
        Id = id;
        Amount = amount;
    }

    public bool IsSameItem(ItemDefinition compareItem)
    {
        return Type == compareItem.Type && Id == compareItem.Id;
    }
}
