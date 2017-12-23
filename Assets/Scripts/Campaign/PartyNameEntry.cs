using System.Collections.Generic;

public class PartyNameEntry
{
    public string Id { get; set; }
    public List<string> ClassIds { get; set; }

    public PartyNameEntry()
    {
        ClassIds = new List<string>();
    }
}
