using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class RaidPartyPanel : MonoBehaviour
{
    [SerializeField]
    private HeroDiscardPanel heroDiscardPanel;
    [SerializeField]
    private Image eventOverlay;
    [SerializeField]
    private List<Sprite> availableOverlays;
    [SerializeField]
    private PartyCompositionPanel compositionPanel;

    public bool IsPartyPrepared { get { return partyMembersPrepared == 4; } }
    public List<RaidPartySlot> PartySlots { get; private set; }

    private const int SlotNumber = 4;
    private int partyMembersPrepared;

    public event Action EventPartyAssembled;
    public event Action EventPartyDisassembled;

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

    private void Awake()
    {
        PartySlots = new List<RaidPartySlot>(transform.Find("PartySlots").GetComponentsInChildren<RaidPartySlot>());
        for (int i = 0; i < SlotNumber; i++)
        {
            PartySlots[i].EventDropIn += RaidPartySlotDropIn;
            PartySlots[i].EventDropOut += RaidPartySlotDropOut;
            PartySlots[i].CompatibilityCheck = PartyCompatibilityCheck;
        }
    }

    public void CheckComposition()
    {
        compositionPanel.UpdateComposition(PartySlots.Select(slot =>
            slot.SelectedHero == null ? null : slot.SelectedHero.Hero.Class).ToList());
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
        DragManager.Instanse.EventStartDraggingPartyHero += ActivateDropOutPanel;
        DragManager.Instanse.EventEndDraggingPartyHero += DeactivateDropOutPanel;

        for (int i = 0; i < SlotNumber; i++)
        {
            if (PartySlots[i].SelectedHero != null)
                PartySlots[i].SlotAnimator.SetBool("empty", false);
            DragManager.Instanse.EventStartDraggingPartyHero += PartySlots[i].MarkSlots;
            DragManager.Instanse.EventEndDraggingPartyHero += PartySlots[i].UnmarkSlots;
        }
        CheckUniqueEventOverlay();
    }

    public void DeactivateDragManagerBehaviour()
    {
        DragManager.Instanse.EventStartDraggingPartyHero -= ActivateDropOutPanel;
        DragManager.Instanse.EventEndDraggingPartyHero -= DeactivateDropOutPanel;

        for (int i = 0; i < SlotNumber; i++)
        {
            DragManager.Instanse.EventStartDraggingPartyHero -= PartySlots[i].MarkSlots;
            DragManager.Instanse.EventEndDraggingPartyHero -= PartySlots[i].UnmarkSlots;
        }
    }

    private void PartyAssembled()
    {
        if (EventPartyAssembled != null)
            EventPartyAssembled();

        CheckComposition();
    }

    private void PartyDisassembled()
    {
        if (EventPartyDisassembled != null)
            EventPartyDisassembled();

        CheckComposition();
    }

    private bool IsCompatible(Hero hero)
    {
        for (int i = 0; i < PartySlots.Count; i++)
        {
            if (PartySlots[i].SelectedHero == null)
                continue;

            if (hero.HeroClass.IncompatiablePartyTag != null)
                if (PartySlots[i].SelectedHero.Hero.HeroClass.Tags.Contains(hero.HeroClass.IncompatiablePartyTag))
                    return false;

            if (PartySlots[i].SelectedHero.Hero.HeroClass.IncompatiablePartyTag != null)
                if (hero.HeroClass.Tags.Contains(PartySlots[i].SelectedHero.Hero.HeroClass.IncompatiablePartyTag))
                    return false;
        }

        return true;
    }

    private void CheckUniqueEventOverlay()
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

    private void RaidPartySlotDropOut(HeroSlot heroSlot)
    {
        partyMembersPrepared--;
        if (partyMembersPrepared == 3)
        {
            PartyDisassembled();
        }
    }

    private void RaidPartySlotDropIn(HeroSlot heroSlot)
    {
        partyMembersPrepared++;
        if (partyMembersPrepared == 4)
        {
            PartyAssembled();
        }
    }

    private bool PartyCompatibilityCheck(HeroSlot heroSlot)
    {
        return IsCompatible(heroSlot.Hero);
    }

    private void ActivateDropOutPanel(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        heroDiscardPanel.gameObject.SetActive(true);
    }

    private void DeactivateDropOutPanel(RaidPartySlot partySlot, HeroSlot heroSlot)
    {
        heroDiscardPanel.gameObject.SetActive(false);
    }
}
