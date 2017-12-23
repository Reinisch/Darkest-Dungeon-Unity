using System.Collections.Generic;

public enum NarrationPlace { Raid, Town, Campaign }

public class NarrationAudioEvent
{
    public bool QueueOnlyOnEmpty { get; set; }
    public bool QueueWhilePlaying { get; set; }
    public string AudioEvent { get; set; }
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

    public bool IsPossible(NarrationPlace narrationPlace, params string[] tags)
    {
        switch(narrationPlace)
        {
            case NarrationPlace.Campaign:
                if (MaxCampaignOccurrences > 0)
                {
                    if (DarkestDungeonManager.Campaign.NarrationCampaignInfo.ContainsKey(AudioEvent))
                        if (DarkestDungeonManager.Campaign.NarrationCampaignInfo[AudioEvent] >= MaxCampaignOccurrences)
                            return false;
                }
                break;
            case NarrationPlace.Raid:
                if (MaxRaidOccurrences > 0)
                {
                    if (DarkestDungeonManager.Campaign.NarrationRaidInfo.ContainsKey(AudioEvent))
                        if (DarkestDungeonManager.Campaign.NarrationRaidInfo[AudioEvent] >= MaxRaidOccurrences)
                            return false;
                }
                goto case NarrationPlace.Campaign;
            case NarrationPlace.Town:
                if (MaxTownVisitOccurrences > 0)
                {
                    if (DarkestDungeonManager.Campaign.NarrationTownInfo.ContainsKey(AudioEvent))
                        if (DarkestDungeonManager.Campaign.NarrationTownInfo[AudioEvent] >= MaxTownVisitOccurrences)
                            return false;
                }
                goto case NarrationPlace.Campaign;
        }

        if (Tags.Count > 0)
        {
            if(CheckAllTags)
            {
                for(int i = 0; i < Tags.Count; i++)
                {
                    if (tags.Length == 0)
                        return false;

                    for(int j = 0; j < tags.Length; j++)
                    {
                        if (tags[j] == Tags[i])
                            break;

                        if (j == tags.Length - 1)
                            return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < tags.Length; i++)
                    if (Tags.Contains(tags[i]))
                        return true;

                return false;
            }
        }
        return true;
    }
}
