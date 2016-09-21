public enum DeathFactor
{
    Hunger, Trap, Obstacle, AttackMonster, BleedMonster, PoisonMonster,
    AttackFriend, BleedFriend, PoisonFriend, PoisonUnknown, BleedUnknown,
    Unknown, CaptorMonster, HeartAttack
}

public class DeathRecord
{
    public string HeroName { get; set; }
    public int HeroClassIndex { get; set; }
    public string KillerName { get; set; }
    public int ResolveLevel { get; set; }
    public DeathFactor Factor { get; set; }
}
