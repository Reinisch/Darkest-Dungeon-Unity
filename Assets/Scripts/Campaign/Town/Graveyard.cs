using System.Collections.Generic;

public class Graveyard : Building
{
    public override string Name { get { return "graveyard"; } }
    public override BuildingType Type { get { return BuildingType.Graveyard; } }
    public List<DeathRecord> Records { get; private set; }

    public Graveyard()
    {
        Records = new List<DeathRecord>();
    }
}
