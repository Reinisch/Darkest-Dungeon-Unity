using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class MultiplayerPartyPanel : MonoBehaviour
{
    public Image eventOverlay;
    public PartyCompositionPanel compositionPanel;

    [SerializeField]
    private List<MultiplayerPartySlot> partySlots;

    public List<MultiplayerPartySlot> PartySlots
    {
        get
        {
            return partySlots;
        }
    }

    void Awake()
    {
        for (int i = 0; i < PartySlots.Count; i++)
            PartySlots[i].SlotId = i + 1;
    }

    public void LoadInitialComposition(List<Hero> heroParty)
    {
        if (heroParty.Count != PartySlots.Count)
            return;

        for(int i = 0; i < PartySlots.Count; i++)
            PartySlots[i].UpdateHero(heroParty[i]);

        compositionPanel.UpdateComposition(heroParty.Select(hero => hero.ClassStringId).ToList());
    }

    public void SwapNextHero(int slotIndex)
    {
        var heroPool = DarkestPhotonLauncher.Instanse.HeroPool;
        int targetIndex = heroPool.IndexOf(PartySlots[slotIndex].SelectedHero);

        while(true)
        {
            targetIndex++;
            if (targetIndex >= heroPool.Count)
                targetIndex = 0;

            if (PartySlots.Find(slot => slot.SelectedHero == heroPool[targetIndex]))
                continue;

            if (PartySlots[slotIndex].SelectedHero.Class == heroPool[targetIndex].Class)
                continue;

            break;
        }

        if (DarkestPhotonLauncher.CharacterWindow.gameObject.activeSelf)
            if (DarkestPhotonLauncher.CharacterWindow.CurrentHero == PartySlots[slotIndex].SelectedHero)
                DarkestPhotonLauncher.CharacterWindow.UpdateCharacterInfo(heroPool[targetIndex], true, true);

        PartySlots[slotIndex].UpdateHero(heroPool[targetIndex]);

        compositionPanel.UpdateComposition(PartySlots.Select(slot => slot.SelectedHero.ClassStringId).ToList());
    }

    public void RefreshQuirks()
    {
        var currentHero = DarkestPhotonLauncher.CharacterWindow.CurrentHero;
        var currentSlot = PartySlots.Find(slot => slot.SelectedHero == currentHero);
        int currentIndex = DarkestPhotonLauncher.Instanse.HeroPool.IndexOf(currentHero);

        int heroSeed = GetInstanceID() + System.DateTime.Now.Millisecond + (int)System.DateTime.Now.Ticks;
        RandomSolver.SetRandomSeed(heroSeed);

        DarkestPhotonLauncher.Instanse.HeroSeeds[currentIndex] = heroSeed;
        DarkestPhotonLauncher.Instanse.HeroPool[currentIndex] = new Hero(currentHero.ClassStringId, currentHero.Name);

        DarkestPhotonLauncher.CharacterWindow.UpdateCharacterInfo(DarkestPhotonLauncher.Instanse.HeroPool[currentIndex], true, true);
        currentSlot.UpdateHero(DarkestPhotonLauncher.Instanse.HeroPool[currentIndex]);
    }
}
