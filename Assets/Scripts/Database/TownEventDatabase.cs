using System.Collections.Generic;

public class TownEventDatabase
{
    public List<TownEventOption> Settings { get; set; }
    public List<TownEventGuarantee> Guarantees { get; set; }
    public List<TownEvent> Events { get; set; }

    public TownEventDatabase()
    {
        Settings = new List<TownEventOption>();
        Events = new List<TownEvent>();
        Guarantees = new List<TownEventGuarantee>();
    }
}

public class TownEventOption
{
    public string Id { get; set; }
    public List<float> Frequency { get; set; }
}