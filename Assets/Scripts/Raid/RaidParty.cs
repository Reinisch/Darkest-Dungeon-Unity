using System.Collections.Generic;

public class RaidParty
{
    public bool IsMovingLeft { get; set; }

    public List<RaidHeroInfo> HeroInfo { get; set; }

    public RaidParty(PhotonPlayer photonPlayer)
    {
        List<Hero> MultiplayerHeroes = new List<Hero>()
        {
            new Hero("abomination", "Abom #4"),
            new Hero("abomination", "Abom #3"),
            new Hero("abomination", "Abom #2"),
            new Hero("abomination", "Abom #1"),
        };
        HeroInfo = new List<RaidHeroInfo>();
        foreach (var hero in MultiplayerHeroes)
            HeroInfo.Add(new RaidHeroInfo(hero));
    }

    public RaidParty(RaidPartySaveData saveData)
    {
        IsMovingLeft = saveData.IsMovingLeft;

        HeroInfo = new List<RaidHeroInfo>();
        for(int i = 0; i < saveData.HeroInfo.Count; i++)
        {
            var hero = DarkestDungeonManager.Campaign.Heroes.Find(campaignHero => 
                campaignHero.RosterId == saveData.HeroInfo[i].HeroRosterId);
            RaidHeroInfo newInfo = new RaidHeroInfo(hero);
            newInfo.IsAlive = saveData.HeroInfo[i].IsAlive;
            if(saveData.HeroInfo[i].IsAlive == false)
            {
                newInfo.DeathRecord = new DeathRecord()
                {
                    Factor = saveData.HeroInfo[i].Factor,
                    HeroClassIndex = hero.ClassIndexId,
                    HeroName = hero.HeroName,
                    KillerName = saveData.HeroInfo[i].Killer,
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
