using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public delegate void RaidPartyEvent();

public class RaidPartyPanel : MonoBehaviour
{
    const int slotNumber = 4;
    int partyMembersPrepared = 0;
    public HeroDiscardPanel heroDiscardPanel;
    public Image eventOverlay;
    public List<Sprite> availableOverlays;

    public bool IsPartyPrepared { get { return partyMembersPrepared == 4; } }
    public List<RaidPartySlot> PartySlots { get; private set; }

    public event RaidPartyEvent onPartyAssembled;
    public event RaidPartyEvent onPartyDisassembled;

    public static bool IsResolveEligible(Hero hero)
    {
        if (DarkestDungeonManager.Campaign.EventModifiers.NoLevelRestrictions)
            return true;

        int maxLevel = DarkestDungeonManager.Data.QuestDatabase.
                LevelRestrictions[DarkestDungeonManager.RaidManager.Quest.Difficulty];

        if (hero.Resolve.Level > maxLevel)
            return false;

        return true;
    }
    public bool IsCompatible(Hero hero)
    {
        for (int i = 0; i < PartySlots.Count; i++)
        {
            if (PartySlots[i].SelectedHero == null)
                continue;

            if(hero.HeroClass.IncompatiablePartyTag != null)
                if (PartySlots[i].SelectedHero.Hero.HeroClass.Tags.Contains(hero.HeroClass.IncompatiablePartyTag))
                    return false;

            if (PartySlots[i].SelectedHero.Hero.HeroClass.IncompatiablePartyTag != null)
                if (hero.HeroClass.Tags.Contains(PartySlots[i].SelectedHero.Hero.HeroClass.IncompatiablePartyTag))
                    return false;
        }

        return true;
    }

    void Awake()
    {
        PartySlots = new List<RaidPartySlot>(transform.FindChild("PartySlots").GetComponentsInChildren<RaidPartySlot>());
        for(int i = 0; i < slotNumber; i++)
        {
            PartySlots[i].SlotId = i + 1;
            PartySlots[i].onDropIn += RaidPartyPanel_onDropIn;
            PartySlots[i].onDropOut += RaidPartyPanel_onDropOut;
            PartySlots[i].compatibilityCheck = RaidPartyPanel_compatibilityCheck;
        }
    }

    void PartyAssembled()
    {
        if (onPartyAssembled != null)
            onPartyAssembled();

        DarkestSoundManager.PlayOneShot("event:/ui/town/party_comp");
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
    bool RaidPartyPanel_compatibilityCheck(HeroSlot heroSlot)
    {
        return IsCompatible(heroSlot.Hero);
    }

    void ActivateDropOutPanel(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        heroDiscardPanel.gameObject.SetActive(true);
    }

    void DeactivateDropOutPanel(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        heroDiscardPanel.gameObject.SetActive(false);
    }

    public void CheckUniqueEventOverlay()
    {
        if (DarkestDungeonManager.Campaign.TriggeredEvent != null)
        {
            eventOverlay.sprite = availableOverlays.Find(overlay =>
                overlay.name == DarkestDungeonManager.Campaign.TriggeredEvent.Id);
            if (eventOverlay.sprite != null)
                eventOverlay.SetNativeSize();
        }
        else
            eventOverlay.sprite = null;

        if (eventOverlay.sprite == null)
            eventOverlay.enabled = false;
        else
            eventOverlay.enabled = true;
    }
    public void CheckRestrictions()
    {
        for (int i = 0; i < PartySlots.Count; i++)
        {
            if (PartySlots[i].SelectedHero != null && !IsResolveEligible(PartySlots[i].SelectedHero.Hero))
            {
                // Implement bark
                PartySlots[i].ItemDroppedOut(PartySlots[i].SelectedHero);
            }
        }
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
        CheckUniqueEventOverlay();
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
