using UnityEngine;
using System.Collections.Generic;

public class FormationOverlay : MonoBehaviour
{
    [SerializeField]
    private Sprite moveSprite;
    [SerializeField]
    private List<Sprite> selectionSizeSprites;
    [SerializeField]
    private List<Sprite> friendSizeSprites;
    [SerializeField]
    private List<Sprite> enemySizeSprites;

    public List<FormationOverlaySlot> OverlaySlots { get; private set; }
    public FormationOverlaySlot FreeSlot { get { return OverlaySlots.Find(slot => slot.TargetUnit == null); } }
    public List<Sprite> SelectionSizeSprites { get { return selectionSizeSprites; } }
    public List<Sprite> FriendSizeSprites { get { return friendSizeSprites; } }
    public List<Sprite> EnemySizeSprites { get { return enemySizeSprites; } }
    public Sprite MoveSprite { get { return moveSprite; } }

    private void Awake()
    {
        OverlaySlots = new List<FormationOverlaySlot>(GetComponentsInChildren<FormationOverlaySlot>(true));
        for (int i = 0; i < OverlaySlots.Count; i++)
        {
            OverlaySlots[i].HeroSelected += UnitSelected;
            OverlaySlots[i].SkillTargetSelected += RaidSceneManager.Instanse.HeroSkillTargetSelected;
            OverlaySlots[i].Overlay = this;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
        {
            OverlaySlots[i].HeroSelected -= UnitSelected;
            OverlaySlots[i].SkillTargetSelected -= RaidSceneManager.Instanse.HeroSkillTargetSelected;
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

    private void UnitSelected(FormationOverlaySlot slot)
    {
        for (int i = 0; i < OverlaySlots.Count; i++)
            if (OverlaySlots[i] != slot && OverlaySlots[i].TargetUnit != null)
                OverlaySlots[i].TargetUnit.SetDeactivatedStatus();

        if (slot.TargetUnit.OverlaySlot.IsSelectionLocked && RaidSceneManager.Raid.CampingPhase != CampingPhase.None)
            slot.TargetUnit.SetDeactivatedStatus();
        else
            slot.TargetUnit.SetPerformerStatus();
    }
}