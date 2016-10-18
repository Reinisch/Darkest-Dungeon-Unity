using UnityEngine;
using System.Collections.Generic;

public class NarrationAudioEvent
{
    public bool QueueOnlyOnEmpty { get; set; }
    public bool QueueWhilePlaying { get; set; }
    public float Chance { get; set; }
    public float Priority { get; set; }
    public int MaxRaidOccurrences { get; set; }
    public int MaxTownVisitOccurrences { get; set; }
    public int MaxCampaignOccurrences { get; set; }
    public string Filter { get; set; }
    public bool CheckAllTags { get; set; }
    public List<string> Tags { get; set; }

    public NarrationAudioEvent()
    {
        Tags = new List<string>();
    }
}
