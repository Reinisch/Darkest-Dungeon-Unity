using System.Collections.Generic;

public class NarrationEntry
{
    public string Id { get; set; }
    public string Tone { get; set; }
    public float Chance { get; set; }
    public List<NarrationAudioEvent> AudioEvents { get; set; }

    public NarrationEntry()
    {
        AudioEvents = new List<NarrationAudioEvent>();
    }
}
