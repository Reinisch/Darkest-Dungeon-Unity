public class UpgradeTag
{
    public string Tag { get; private set; }
    public int Amount { get; private set; }

    public UpgradeTag(string tag, int amount)
    {
        Tag = tag;
        Amount = amount;
    }
}