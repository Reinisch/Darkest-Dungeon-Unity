using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void RaidPartyEvent();

public class RaidPartyPanel : MonoBehaviour
{
    const int slotNumber = 4;
    int partyMembersPrepared = 0;
    public HeroDiscardPanel heroDiscardPanel;
    public bool IsPartyPrepared { get { return partyMembersPrepared == 4; } }
    public List<RaidPartySlot> PartySlots { get; private set; }

    public event RaidPartyEvent onPartyAssembled;
    public event RaidPartyEvent onPartyDisassembled;

    void Awake()
    {
        PartySlots = new List<RaidPartySlot>(transform.FindChild("PartySlots").GetComponentsInChildren<RaidPartySlot>());
        for(int i = 0; i < slotNumber; i++)
        {
            PartySlots[i].SlotId = i + 1;
            PartySlots[i].onDropIn += RaidPartyPanel_onDropIn;
            PartySlots[i].onDropOut += RaidPartyPanel_onDropOut;
        }
    }

    void PartyAssembled()
    {
        if (onPartyAssembled != null)
            onPartyAssembled();
    }
    void PartyDisassembled()
    {
        if (onPartyDisassembled != null)
            onPartyDisassembled();
    }

    void RaidPartyPanel_onDropOut(HeroSlot heroSlot)
    {
        partyMembersPrepared--;
        if (partyMembersPrepared == 3)
        {
            PartyDisassembled();
        }
    }

    void RaidPartyPanel_onDropIn(HeroSlot heroSlot)
    {
        partyMembersPrepared++;
        if(partyMembersPrepared == 4)
        {
            PartyAssembled();
        }
    }

    void ActivateDropOutPanel(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        heroDiscardPanel.gameObject.SetActive(true);
    }

    void DeactivateDropOutPanel(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        heroDiscardPanel.gameObject.SetActive(false);
    }

    public void ActivateDragManagerBehaviour()
    {
        DragManager.Instanse.onStartDraggingPartyHero += ActivateDropOutPanel;
        DragManager.Instanse.onEndDraggingPartyHero += DeactivateDropOutPanel;

        for (int i = 0; i < slotNumber; i++)
        {
            if (PartySlots[i].SelectedHero != null)
                PartySlots[i].SlotAnimator.SetBool("empty", false);
            DragManager.Instanse.onStartDraggingPartyHero += PartySlots[i].MarkSlots;
            DragManager.Instanse.onEndDraggingPartyHero += PartySlots[i].UnmarkSlots;
        }
    }
    public void DeactivateDragManagerBehaviour()
    {
        DragManager.Instanse.onStartDraggingPartyHero -= ActivateDropOutPanel;
        DragManager.Instanse.onEndDraggingPartyHero -= DeactivateDropOutPanel;

        for (int i = 0; i < slotNumber; i++)
        {
            DragManager.Instanse.onStartDraggingPartyHero -= PartySlots[i].MarkSlots;
            DragManager.Instanse.onEndDraggingPartyHero -= PartySlots[i].UnmarkSlots;
        }
    }
}
