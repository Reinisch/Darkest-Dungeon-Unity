public abstract class Skill
{
    public string Id { get; set; }
}

public class HealComponent
{
    public int MinAmount { get; set; }
    public int MaxAmount { get; set; }

    public HealComponent(int min, int max)
    {
        MinAmount = min;
        MaxAmount = max;
    }
}

public class MoveComponent
{
    public int Pushback { get; set; }
    public int Pullforward { get; set; }

     public MoveComponent(int push, int pull)
    {
        Pushback = push;
        Pullforward = pull;
    }
}