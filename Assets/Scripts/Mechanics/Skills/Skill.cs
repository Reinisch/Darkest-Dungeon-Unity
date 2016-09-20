using UnityEngine;
using System.Collections;

public class SkillComponent
{

}

public class HealComponent : SkillComponent
{
    public int MinAmount { get; set; }
    public int MaxAmount { get; set; }

    public HealComponent(int min, int max)
    {
        MinAmount = min;
        MaxAmount = max;
    }
}

public class MoveComponent : SkillComponent
{
    public int Pushback { get; set; }
    public int Pullforward { get; set; }

     public MoveComponent(int push, int pull)
    {
        Pushback = push;
        Pullforward = pull;
    }
}

public class Skill
{

}
