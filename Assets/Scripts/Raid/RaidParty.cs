using System.Collections.Generic;

public class RaidParty
{
    public bool IsMovingLeft { get; set; }
    public List<RaidHeroInfo> HeroInfo { get; private set; }

    public RaidParty(PhotonPlayer photonPlayer)
    {
        List<Hero> multiplayerHeroes = new List<Hero>
        {
            new Hero(1, photonPlayer),
            new Hero(2, photonPlayer),
            new Hero(3, photonPlayer),
            new Hero(4, photonPlayer)
        };

        HeroInfo = new List<RaidHeroInfo>();
        foreach (var hero in multiplayerHeroes)
            HeroInfo.Add(new RaidHeroInfo(hero));
    }

    public RaidParty(RaidPartySaveData saveData)
    {
        IsMovingLeft = saveData.IsMovingLeft;

        HeroInfo = new List<RaidHeroInfo>();
        foreach (RaidPartyHeroInfoSaveData saveInfo in saveData.HeroInfo)
        {
            var hero = DarkestDungeonManager.Campaign.Heroes.Find(campaignHero => 
                campaignHero.RosterId == saveInfo.HeroRosterId);
            RaidHeroInfo newInfo = new RaidHeroInfo(hero) {IsAlive = saveInfo.IsAlive};

            if(saveInfo.IsAlive == false)
            {
                newInfo.DeathRecord = new DeathRecord()
                {
                    Factor = saveInfo.Factor,
                    HeroClassIndex = hero.ClassIndexId,
                    HeroName = hero.HeroName,
                    KillerName = saveInfo.Killer,
                    ResolveLevel = hero.Resolve.Level,
                };
            }
            HeroInfo.Add(newInfo);
        }
    }

    public RaidParty(RaidPartyPanel partyPanel)
    {
        HeroInfo = new List<RaidHeroInfo>();
        foreach (var partySlot in partyPanel.PartySlots)
            HeroInfo.Add(new RaidHeroInfo(partySlot.SelectedHero.Hero));
    }

    public RaidParty(List<Hero> heroes)
    {
        HeroInfo = new List<RaidHeroInfo>();
        foreach (var hero in heroes)
            HeroInfo.Add(new RaidHeroInfo(hero));
    }
}
