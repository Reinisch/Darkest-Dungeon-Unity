using UnityEngine;
using System.Collections;

public class Statue : Building
{
    public Statue()
    {
        Name = "statue";
        VisitPriority = 1;
        QuestsRequired = 0;
        HighestDungeonLevelRequired = 0;
    }
}
