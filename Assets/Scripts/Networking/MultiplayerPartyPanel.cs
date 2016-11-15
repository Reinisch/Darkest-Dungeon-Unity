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
    }
}
