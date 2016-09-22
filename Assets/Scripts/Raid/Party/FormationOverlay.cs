using UnityEngine;
using System.Collections.Generic;

public class FormationOverlay : MonoBehaviour
{
    public Sprite moveSprite;
    public List<Sprite> selectionSizeSprites;
    public List<Sprite> friendSizeSprites;
    public List<Sprite> enemySizeSprites;

    public List<FormationOverlaySlot> OverlaySlots { get; set; }
    
    void Awake()
    {
        OverlaySlots = new List<FormationOverlaySlot>(GetComponentsInChildren<FormationOverlaySlot>(true));
        for (int i = 0; i < OverlaySlots.Count; i++)
        {
            OverlaySlots[i].onHeroSelected += UnitSelected;
            OverlaySlots[i].onSkillTargetSelected += RaidSceneManager.Instanse.HeroSkillTargetSelected;
            OverlaySlots[i].Overlay = this;
        }
    }
    void UnitSelected(FormationOverlaySlot slot)
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
            if(OverlaySlots[i] != slot && OverlaySlots[i].TargetUnit != null)
                OverlaySlots[i].TargetUnit.SetDeactivatedStatus();

        if(slot.TargetUnit.OverlaySlot.IsSelectionLocked && RaidSceneManager.Raid.CampingPhase != CampingPhase.None)
            slot.TargetUnit.SetDeactivatedStatus();
        else
            slot.TargetUnit.SetPerformerStatus();
    }

    public bool HasFreeSlot
    {
        get
        {
            return OverlaySlots.Find(slot => slot.TargetUnit == null) == null;
        }
    }
    public FormationOverlaySlot FreeSlot
    {
        get
        {
            return OverlaySlots.Find(slot => slot.TargetUnit == null);
        }
    }

    public void LockOnUnits(FormationParty party)
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
        {
            if(i < party.Units.Count)
                OverlaySlots[i].LockOnUnit(party.Units[i]);
            else
                OverlaySlots[i].ClearTarget();
        }
            
    }
    public void UpdateOverlay()
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
        {
            if (OverlaySlots[i].isActiveAndEnabled)
                OverlaySlots[i].UpdateOverlay();
        }
    }

    public void ResetOverlay()
    {
        for(int i = 0; i < OverlaySlots.Count; i++)
            OverlaySlots[i].ClearTarget();
    }

    public void ResetSelectionsExcept(FormationUnit unit)
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
            if (OverlaySlots[i].TargetUnit != null && OverlaySlots[i].TargetUnit != unit)
                OverlaySlots[i].TargetUnit.SetDeactivatedStatus();
    }
    public void ResetSelections()
    {
        for(int i = 0; i < OverlaySlots.Count; i++)
            if(OverlaySlots[i].TargetUnit != null)
                OverlaySlots[i].TargetUnit.SetDeactivatedStatus();
    }

    public void Show()
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
            OverlaySlots[i].Show();
    }
    public void Hide()
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
            OverlaySlots[i].Hide();
    }

    public void LockSelection()
    {
        for(int i = 0; i < OverlaySlots.Count; i++)
            OverlaySlots[i].LockSelection();
    }
    public void UnlockSelection()
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
            OverlaySlots[i].UnlockSelection();
    }
}