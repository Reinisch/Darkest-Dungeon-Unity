public class RaidPartyHeroInfoSaveData
{
    public bool IsAlive { get; set; }
    public int HeroRosterId { get; set; }
    public DeathFactor Factor { get; set; }
    public string Killer { get; set; }

    public RaidPartyHeroInfoSaveData()
    {
        Killer = "";
    }
}