public enum ActivitySlotStatus
{
    Available,
    Caretaken,
    Crierd,
    Paid,
    Blocked,
    Checkout
}

public class ActivitySlot
{
    public bool IsUnlocked { get; set; }
    public int BaseCost { get; set; }

    public Hero Hero { get; set; }
    public ActivitySlotStatus Status { get; set; }

    public ActivitySlot()
    {
        IsUnlocked = false;
        Hero = null;
    }

    public ActivitySlot(bool isUnlocked, int baseCost)
    {
        IsUnlocked = isUnlocked;
        BaseCost = baseCost;
    }

    public void UpdateSlot(bool isUnlocked, int baseCost)
    {
        IsUnlocked = isUnlocked;
        BaseCost = baseCost;
    }
}
