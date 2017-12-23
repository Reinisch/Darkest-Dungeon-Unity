public class DamageOverTimeInstanse
{
    public int TickDamage { get; set; }
    public int TicksLeft { get; set; }
    public int TicksAmount { get; set; }

    public bool CheckExpiration()
    {
        return --TicksLeft <= 0;
    }
}