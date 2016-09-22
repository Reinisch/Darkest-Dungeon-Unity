public class ItemInteraction : CurioInteraction
{
    public string ItemId { get; set; }

    public ItemInteraction() : base()
    {
        Chance = 1;
    }

    public override string ResultString()
    {
        return ItemId + "_" + ResultType;
    }
}