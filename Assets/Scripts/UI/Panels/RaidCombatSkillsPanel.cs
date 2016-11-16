using UnityEngine;
using System.Collections.Generic;

public enum SkillPanelMode { Combat, Camping }

public class RaidCombatSkillsPanel : MonoBehaviour
{
    public List<BattleSkillSlot> skillSlots;
    public List<BattleCampingSlot> campingSlots;
    public MoveSkillSlot moveSlot;
    public PassSkillSlot passSlot;

    public Skill SelectedSkill
    {
        get;
        set;
    }
    public SkillPanelMode Mode
    {
        get;
        private set;
    }

    void SkillPanel_onMoveSelected(MoveSkillSlot slot)
    {
        SelectedSkill = slot.Skill;
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (skillSlots[i].IsEmpty || !skillSlots[i].Selected)
                continue;

            skillSlots[i].Deselect();
        }
    }
    public void SkillPanel_onSkillSelected(BattleSkillSlot slot)
    {
        SelectedSkill = slot.Skill;
        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (skillSlots[i].IsEmpty)
                continue;

            if (skillSlots[i] != slot)
                skillSlots[i].Deselect();
        }
        moveSlot.Deselect();
    }
    void SkillPanel_onSkillSelected(BattleCampingSlot slot)
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

    void Awake()
    {
        for (int i = 0; i < skillSlots.Count; i++)
        {
            skillSlots[i].onSkillSelected += SkillPanel_onSkillSelected;
            skillSlots[i].onSkillSelected += RaidSceneManager.Instanse.HeroSkillSelected;
        }
        for (int i = 0; i < campingSlots.Count; i++)
        {
            campingSlots[i].onSkillSelected += SkillPanel_onSkillSelected;
            campingSlots[i].onSkillSelected += RaidSceneManager.Instanse.HeroCampingSkillSelected;
            campingSlots[i].onSkillDeselected += RaidSceneManager.Instanse.HeroCampingSkillDeselected;
        }

        moveSlot.onSkillSelected += SkillPanel_onMoveSelected;
        moveSlot.onSkillSelected += RaidSceneManager.Instanse.HeroMoveSelected;
        moveSlot.onSkillDeselected += RaidSceneManager.Instanse.HeroMoveDeselected;
        passSlot.onPassPressed += RaidSceneManager.Instanse.HeroPassButtonClicked;
    }
    void SetUsableCombatSkills()
    {
        BattleFormation allies = RaidSceneManager.RaidPanel.SelectedUnit.Team == Team.Heroes ?
            RaidSceneManager.BattleGround.HeroFormation : RaidSceneManager.BattleGround.MonsterFormation;
        BattleFormation enemies = RaidSceneManager.RaidPanel.SelectedUnit.Team == Team.Heroes ?
            RaidSceneManager.BattleGround.MonsterFormation : RaidSceneManager.BattleGround.HeroFormation;

        for (int i = 0; i < skillSlots.Count; i++)
        {
            if (skillSlots[i].IsEmpty)
                continue;

            if (skillSlots[i].Skill.LimitPerBattle.HasValue)
            {
                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.SkillsUsedInBattle.
                    FindAll(skillId => skillId == skillSlots[i].Skill.Id).Count >= skillSlots[i].Skill.LimitPerBattle.Value)
                {
                    skillSlots[i].SetDisabledState();
                    continue;
                }
            }

            if (skillSlots[i].Skill.LimitPerTurn.HasValue)
            {
                if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.SkillsUsedThisTurn.
                    FindAll(skillId => skillId == skillSlots[i].Skill.Id).Count >= skillSlots[i].Skill.LimitPerTurn.Value)
                {
                    skillSlots[i].SetDisabledState();
                    continue;
                }
            }

            if (skillSlots[i].Skill.LaunchRanks.IsLaunchableFrom(RaidSceneManager.RaidPanel.SelectedUnit.Rank,
                RaidSceneManager.RaidPanel.SelectedUnit.Size))
            {
                if (BattleSolver.IsPerformerSkillTargetable(skillSlots[i].Skill,
                    allies, enemies, RaidSceneManager.RaidPanel.SelectedUnit))
                {
                    skillSlots[i].SetCombatState();

                    if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.LastCombatSkillUsed == skillSlots[i].Skill.Id)
                        skillSlots[i].Select();

                    continue;
                }
            }
            skillSlots[i].SetDisabledState();
        }
        if (RaidSceneManager.RaidPanel.SelectedUnit.CombatInfo.IsImmobilized)
            moveSlot.SetDisabledState();
        else
            moveSlot.SetCombatState();

        passSlot.SetCombatState();
    }
    void SetUsableCampingSkills()
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

    void SetCombatDisabled()
    {
        for (int i = 0; i < skillSlots.Count; i++)
            if (!skillSlots[i].IsEmpty)
                skillSlots[i].SetDisabledState();

        moveSlot.SetDisabledState();
        passSlot.SetDisabledState();
    }
    void SetCampingDisabled()
    {
        for (int i = 0; i < campingSlots.Count; i++)
            if (!campingSlots[i].IsEmpty)
                campingSlots[i].SetDisabledState();
    }

    void SetCombatPeaceful()
    {
        for (int i = 0; i < skillSlots.Count; i++)
            if (!skillSlots[i].IsEmpty)
                skillSlots[i].SetDisabledState();

        moveSlot.SetMovableState();
        passSlot.SetDisabledState();
    }
    void SetCampingPeaceful()
    {
        for (int i = 0; i < campingSlots.Count; i++)
            if (!campingSlots[i].IsEmpty)
                campingSlots[i].SetDisabledState();
    }

    void UpdateCombatSkills()
    {
        if (RaidSceneManager.RaidPanel.SelectedHero.Mode == null)
        {
            int selectedSkills = Mathf.Min(RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills.Count, skillSlots.Count);

            for (int i = 0; i < selectedSkills; i++)
                skillSlots[i].UpdateSkill(RaidSceneManager.RaidPanel.SelectedHero,
                    RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills[i]);

            for (int i = selectedSkills; i < skillSlots.Count; i++)
                skillSlots[i].ResetSkill();
        }
        else
        {
            var modeSkills = RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills.FindAll(skill =>
                skill.ValidModes.Contains(RaidSceneManager.RaidPanel.SelectedHero.Mode.Id));
            int selectedSkills = Mathf.Min(RaidSceneManager.RaidPanel.SelectedHero.SelectedCombatSkills.Count, modeSkills.Count);

            for (int i = 0; i < selectedSkills; i++)
                skillSlots[i].UpdateSkill(RaidSceneManager.RaidPanel.SelectedHero, modeSkills[i]);

            for (int i = selectedSkills; i < skillSlots.Count; i++)
                skillSlots[i].ResetSkill();
        }

        moveSlot.UpdateSkill();
    }
    void UpdateCampingSkills()
    {
        int selectedSkills = Mathf.Min(RaidSceneManager.RaidPanel.SelectedHero.SelectedCampingSkills.Count, campingSlots.Count);

        for (int i = 0; i < selectedSkills; i++)
            campingSlots[i].UpdateSkill(RaidSceneManager.RaidPanel.SelectedHero,
                RaidSceneManager.RaidPanel.SelectedHero.SelectedCampingSkills[i]);

        for (int i = selectedSkills; i < campingSlots.Count; i++)
            campingSlots[i].ResetSkill();
    }

    public void SetMode(SkillPanelMode mode)
    {
        Mode = mode;
        if(Mode == SkillPanelMode.Combat)
        {
            for(int i = 0; i < skillSlots.Count; i++)
                skillSlots[i].gameObject.SetActive(true);
            for (int i = 0; i < campingSlots.Count; i++)
                campingSlots[i].gameObject.SetActive(false);
            moveSlot.gameObject.SetActive(true);
            passSlot.gameObject.SetActive(true);
        }
        else if (Mode == SkillPanelMode.Camping)
        {
            for (int i = 0; i < skillSlots.Count; i++)
                skillSlots[i].gameObject.SetActive(false);
            for (int i = 0; i < campingSlots.Count; i++)
                campingSlots[i].gameObject.SetActive(true);
            moveSlot.gameObject.SetActive(false);
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
    public void UpdateSkillPanel()
    {
        if (Mode == SkillPanelMode.Combat)
            UpdateCombatSkills();
        else if (Mode == SkillPanelMode.Camping)
            UpdateCampingSkills();
    }
}
