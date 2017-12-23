public class PoisonStatusEffect : DamageOverTimeStatusEffect
{
    public override StatusType Type { get { return StatusType.Bleeding; } }
}