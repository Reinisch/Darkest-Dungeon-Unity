using UnityEngine;
using System.Collections.Generic;

public enum SkillPanelMode { Combat, Camping }

public class RaidCombatSkillsPanel : MonoBehaviour
{
    [SerializeField]
    private List<BattleSkillSlot> skillSlots;
    [SerializeField]
    private List<BattleCampingSlot> campingSlots;
    [SerializeField]
    private MoveSkillSlot moveSlot;
    [SerializeField]
    private PassSkillSlot passSlot;

    public List<BattleSkillSlot> SkillSlots { get { return skillSlots; } }
    public MoveSkillSlot MoveSlot { get { return moveSlot; } }
    public Skill SelectedSkill { get; set; }

    private SkillPanelMode Mode { get; set; }

    private void Awake()
    {
        for (int i = 0; i < SkillSlots.Count; i++)
        {
            SkillSlots[i].EventSkillSelected += SkillSlotCombatSkillSelected;
            SkillSlots[i].EventSkillSelected += RaidSceneManager.Instanse.HeroSkillSelected;
        }
        for (int i = 0; i < campingSlots.Count; i++)
        {
            campingSlots[i].EventSkillSelected += SkillSlotCampSkillSelected;
            campingSlots[i].EventSkillSelected += RaidSceneManager.Instanse.HeroCampingSkillSelected;
            campingSlots[i].EventSkillDeselected += RaidSceneManager.Instanse.HeroCampingSkillDeselected;
        }

        MoveSlot.EventSkillSelected += SkillSlotMoveSelected;
        MoveSlot.EventSkillSelected += RaidSceneManager.Instanse.HeroMoveSelected;
        MoveSlot.EventSkillDeselected += RaidSceneManager.Instanse.HeroMoveDeselected;
        passSlot.EventPassPressed += RaidSceneManager.Instanse.HeroPassButtonClicked;
    }

    public void UpdateSkillPanel()
    {
        if (Mode == SkillPanelMode.Combat)
            UpdateCombatSkills();
        else if (Mode == SkillPanelMode.Camping)
            UpdateCampingSkills();
    }

    #region Panel States

    public void SetMode(SkillPanelMode mode)
    {
        Mode = mode;
        if(Mode == SkillPanelMode.Combat)
        {
            for(int i = 0; i < SkillSlots.Count; i++)
                SkillSlots[i].gameObject.SetActive(true);
            for (int i = 0; i < campingSlots.Count; i++)
                campingSlots[i].gameObject.SetActive(false);
            MoveSlot.gameObject.SetActive(true);
            passSlot.gameObject.SetActive(true);
        }
        else if (Mode == SkillPanelMode.Camping)
        {
            for (int i = 0; i < SkillSlots.Count; i++)
                SkillSlots[i].gameObject.SetActive(false);
            for (int i = 0; i < campingSlots.Count; i++)
                campingSlots[i].gameObject.SetActive(true);
            MoveSlot.gameObject.SetActive(false);
            passSlot.gameObject.SetActive(false);
        }
    }

    public void SetUsable()
    {
        if (Mode == SkillPanelMode.Combat)
            SetUsableCombatSkills();
        else if (Mode == SkillPanelMode.Camping)
            SetUsableCampingSkills();
    }

    public void SetDisabled()
    {
        if (Mode == SkillPanelMode.Combat)
            SetCombatDisabled();
        else if (Mode == SkillPanelMode.Camping)
            SetCampingDisabled();
    }

    public void SetPeaceful()
    {
        if (Mode == SkillPanelMode.Combat)
            SetCombatPeaceful();
        else if (Mode == SkillPanelMode.Camping)
            SetCampingPeaceful();
    }

    private void SetUsableCombatSkills()
    {
        BattleFormation allies = RaidSceneManager.RaidPanel.SelectedUnit.Team == Team.Heroes ?
            RaidSceneManager.BattleGround.HeroFormation : RaidSceneManager.BattleGround.MonsterFormation;
        BattleFormation enemies = RaidSceneManager.RaidPanel.SelectedUnit.Team == Team.Heroes ?
            RaidSceneManager.BattleGround.MonsterFormation : RaidSceneManager.BattleGround.HeroFormation;

        for (int i = 0; i < SkillSlots.Count; i++)
        {
            if (SkillSlots[i].IsEmpty)
                continue;

            if (SkillSlots[i].Skill.LimitPerBattle.HasValue)
            {
                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.SkillsUsedInBattle.
                    FindAll(skillId => skillId == SkillSlots[i].Skill.Id).Count >= SkillSlots[i].Skill.LimitPerBattle.Value)
                {
                    SkillSlots[i].SetDisabledState();
                    continue;
                }
            }

            if (SkillSlots[i].Skill.LimitPerTurn.HasValue)
            {
                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.SkillsUsedThisTurn.
                    FindAll(skillId => skillId == SkillSlots[i].Skill.Id).Count >= SkillSlots[i].Skill.LimitPerTurn.Value)
                {
                    SkillSlots[i].SetDisabledState();
                    continue;
                }
            }

            if (SkillSlots[i].Skill.LaunchRanks.IsLaunchableFrom(RaidSceneManager.RaidPanel.SelectedUnit.Rank,
                RaidSceneManager.RaidPanel.SelectedUnit.Size))
            {
                if (BattleSolver.IsPerformerSkillTargetable(SkillSlots[i].Skill,
                    allies, enemies, RaidSceneManager.RaidPanel.SelectedUnit))
                {
                    SkillSlots[i].SetCombatState();

                    if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.LastCombatSkillUsed == SkillSlots[i].Skill.Id)
                        SkillSlots[i].Select();

                    continue;
                }
            }
            SkillSlots[i].SetDisabledState();
        }
        if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.IsImmobilized)
            MoveSlot.SetDisabledState();
        else
            MoveSlot.SetCombatState();

        passSlot.SetCombatState();
    }

    private void SetUsableCampingSkills()
    {
        for (int i = 0; i < campingSlots.Count; i++)
        {
            if (campingSlots[i].IsEmpty)
                continue;

            if (BattleSolver.IsCampingSkillUsable(RaidSceneManager.RaidPanel.SelectedUnit, campingSlots[i].Skill))
                campingSlots[i].SetCombatState();
            else
                campingSlots[i].SetDisabledState();
        }
    }

    private void SetCombatDisabled()
    {
        for (int i = 0; i < SkillSlots.Count; i++)
            if (!SkillSlots[i].IsEmpty)
                SkillSlots[i].SetDisabledState();

        MoveSlot.SetDisabledState();
        passSlot.SetDisabledState();
    }

    private void SetCampingDisabled()
    {
        for (int i = 0; i < campingSlots.Count; i++)
            if (!campingSlots[i].IsEmpty)
                campingSlots[i].SetDisabledState();
    }

    private void SetCombatPeaceful()
    {
        for (int i = 0; i < SkillSlots.Count; i++)
            if (!SkillSlots[i].IsEmpty)
                SkillSlots[i].SetDisabledState();

        MoveSlot.SetMovableState();
        passSlot.SetDisabledState();
    }

    private void SetCampingPeaceful()
    {
        for (int i = 0; i < campingSlots.Count; i++)
            if (!campingSlots[i].IsEmpty)
                campingSlots[i].SetDisabledState();
    }

    #endregion

    private void UpdateCombatSkills()
    {
        if (RaidSceneManager.RaidPanel.SelectedHero.Mode == null)
        {
            int selectedSkills = Mathf.Min(RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills.Count, SkillSlots.Count);

            for (int i = 0; i < selectedSkills; i++)
                SkillSlots[i].UpdateSkill(RaidSceneManager.RaidPanel.SelectedHero,
                    RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills[i]);

            for (int i = selectedSkills; i < SkillSlots.Count; i++)
                SkillSlots[i].ResetSkill();
        }
        else
        {
            var modeSkills = RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills.FindAll(skill =>
                skill.ValidModes.Contains(RaidSceneManager.RaidPanel.SelectedHero.Mode.Id));
            int selectedSkills = Mathf.Min(RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills.Count, modeSkills.Count);

            for (int i = 0; i < selectedSkills; i++)
                SkillSlots[i].UpdateSkill(RaidSceneManager.RaidPanel.SelectedHero, modeSkills[i]);

            for (int i = selectedSkills; i < SkillSlots.Count; i++)
                SkillSlots[i].ResetSkill();
        }

        MoveSlot.UpdateSkill();
    }

    private void UpdateCampingSkills()
    {
        int selectedSkills = Mathf.Min(RaidSceneManager.RaidPanel.SelectedHero.SelectedCampingSkills.Count, campingSlots.Count);

        for (int i = 0; i < selectedSkills; i++)
            campingSlots[i].UpdateSkill(RaidSceneManager.RaidPanel.SelectedHero,
                RaidSceneManager.RaidPanel.SelectedHero.SelectedCampingSkills[i]);

        for (int i = selectedSkills; i < campingSlots.Count; i++)
            campingSlots[i].ResetSkill();
    }

    private void SkillSlotMoveSelected(MoveSkillSlot slot)
    {
        SelectedSkill = slot.Skill;
        for (int i = 0; i < SkillSlots.Count; i++)
        {
            if (SkillSlots[i].IsEmpty || !SkillSlots[i].Selected)
                continue;

            SkillSlots[i].Deselect();
        }
    }

    private void SkillSlotCombatSkillSelected(BattleSkillSlot slot)
    {
        SelectedSkill = slot.Skill;
        for (int i = 0; i < SkillSlots.Count; i++)
        {
            if (SkillSlots[i].IsEmpty)
                continue;

            if (SkillSlots[i] != slot)
                SkillSlots[i].Deselect();
        }
        MoveSlot.Deselect();
    }

    private void SkillSlotCampSkillSelected(BattleCampingSlot slot)
    {
        SelectedSkill = slot.Skill;
        for (int i = 0; i < campingSlots.Count; i++)
        {
            if (campingSlots[i].IsEmpty)
                continue;

            if (campingSlots[i] != slot && campingSlots[i].Selected)
                campingSlots[i].Deselect(true);
        }
    }
}
