using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graveyard : Building
{
    public List<DeathRecord> Records { get; set; }

    public Graveyard()
    {
        Name = "graveyard";
        VisitPriority = 1;
        QuestsRequired = 0;
        HighestDungeonLevelRequired = 0;
        Records = new List<DeathRecord>();
    }
}
