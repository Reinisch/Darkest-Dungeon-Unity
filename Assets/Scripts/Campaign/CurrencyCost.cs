using UnityEngine;
using System.Collections;

public class CurrencyCost
{
    public string Type { get; set; }
    public int Amount { get; set; }

    public CurrencyCost()
    {

    }

    public CurrencyCost(CurrencyCost clone)
    {
        Type = clone.Type;
        Amount = clone.Amount;
    }

    public CurrencyCost(string type, int amount)
    {
        Type = type;
        Amount = amount;
    }
}
