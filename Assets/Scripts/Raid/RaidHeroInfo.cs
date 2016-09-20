using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaidHeroInfo
{
    public bool IsAlive { get; set; }
    public Hero Hero { get; set; }
    public DeathRecord DeathRecord { get; set; }

    public RaidHeroInfo(Hero hero)
    {
        IsAlive = true;
        Hero = hero;
    }
}