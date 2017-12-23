public class CurrencyCost
{
    public string Type { get; set; }
    public int Amount { get; set; }

    public CurrencyCost()
    {
    }

    public CurrencyCost(string type, int amount)
    {
        Type = type;
        Amount = amount;
    }
}
