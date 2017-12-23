using System.Collections.Generic;

public class RaidPartySaveData
{
    public bool IsMovingLeft { get; set; }
    public List<RaidPartyHeroInfoSaveData> HeroInfo { get; set; }

    public RaidPartySaveData()
    {
        HeroInfo = new List<RaidPartyHeroInfoSaveData>();
    }

    public void UpdateFromRaidParty(RaidParty raidParty)
    {
        IsMovingLeft = raidParty.IsMovingLeft;
        HeroInfo.Clear();

        for(int i = 0; i < raidParty.HeroInfo.Count; i++)
        {
            var newHeroInfo = new RaidPartyHeroInfoSaveData();
            newHeroInfo.IsAlive = raidParty.HeroInfo[i].IsAlive;
            newHeroInfo.HeroRosterId = raidParty.HeroInfo[i].Hero.RosterId;
            newHeroInfo.Factor = raidParty.HeroInfo[i].DeathRecord == null ? DeathFactor.Unknown:
                raidParty.HeroInfo[i].DeathRecord.Factor;
            newHeroInfo.Killer = raidParty.HeroInfo[i].DeathRecord == null || raidParty.HeroInfo[i].DeathRecord.KillerName == null ?
                "" : raidParty.HeroInfo[i].DeathRecord.KillerName;
            HeroInfo.Add(newHeroInfo);
        }
    }
}