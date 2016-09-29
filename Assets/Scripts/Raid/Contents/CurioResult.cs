public class CurioResult : IProportionValue
{
    public string Item { get; set; }
    public int Draws { get; set; }
    public bool IsCombined { get; set; }
    public int Chance
    {
        get;
        set;
    }
}