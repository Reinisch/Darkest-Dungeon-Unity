public class EstateCurrency
{
    public int amount;
    public bool isHeirloom;

    public EstateCurrency(int startAmount, bool isHeirloomCurrency)
    {
        amount = startAmount;
        isHeirloom = isHeirloomCurrency;
    }
}