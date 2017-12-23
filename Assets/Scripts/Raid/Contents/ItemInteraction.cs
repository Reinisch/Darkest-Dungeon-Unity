public class ItemInteraction : CurioInteraction
{
    public string ItemId { get; set; }

    public ItemInteraction()
    {
        Chance = 1;
    }

    public ItemInteraction(int chance, string itemId, string resultType)
    {
        Chance = chance;
        ItemId = itemId;
        ResultType = resultType;
    }

    public override string ResultString()
    {
        return ItemId + "_" + ResultType;
    }
}