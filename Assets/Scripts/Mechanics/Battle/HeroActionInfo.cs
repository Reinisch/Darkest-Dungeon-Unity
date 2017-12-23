public class HeroActionInfo
{
    public bool IsValid { get; private set; }
    public float ChanceToHit { get; private set; }
    public float ChanceToCrit { get; private set; }
    public int MinDamage { get; private set; }
    public int MaxDamage { get; private set; }

    public void UpdateInfo(bool valid, float hit, float crit, int minDamage, int maxDamage)
    {
        IsValid = valid;
        ChanceToCrit = crit;
        ChanceToHit = hit;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
    }
}