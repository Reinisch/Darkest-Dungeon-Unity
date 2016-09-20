using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeekActivityLog
{
    public int WeekNumber { get; set; }

    public PartyActivityRecord EmbarkRecord { get; set; }
    public PartyActivityRecord ReturnRecord { get; set; }

    public List<ActorActivityRecord> HeroRecords { get; set; }

    public WeekActivityLog(int weekNumber)
    {
        WeekNumber = weekNumber;
        HeroRecords = new List<ActorActivityRecord>();
    }
}
