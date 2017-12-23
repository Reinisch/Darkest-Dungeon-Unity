using UnityEngine;
using System.Collections.Generic;

public class CharCombatSkillPanel : MonoBehaviour
{
    [SerializeField]
    private RectTransform skillPanel;
    [SerializeField]
    private List<SkeletonAnimation> teamStrengthPips;
    [SerializeField]
    private List<SkeletonAnimation> targetStrengthPips;

    private List<SkillSlot> SkillSlots { get; set; }

    private Hero currentHero;
    private int[] teamStrCount = { 0, 0, 0, 0, 0 };
    private int[] targetStrCount = { 0, 0, 0, 0, 0 };
    private bool[] friendlyCheck = { false, false, false, false, false };
    private bool interactable;

    private void Awake()
    {
        SkillSlots = new List<SkillSlot>(skillPanel.GetComponentsInChildren<SkillSlot>());
        for (int i = 0; i < SkillSlots.Count; i++)
        {
            SkillSlots[i].EventSkillSelected += CharCombatSkillPanelSkillSelected;
            SkillSlots[i].EventSkillDeselected += CharCombatSkillPanelSkillDeselected;
        }

        var canvas = GetComponentInParent<Canvas>();
        for (int i = 0; i < teamStrengthPips.Count; i++)
        {
            teamStrengthPips[i].MeshRenderer.sortingOrder = canvas.sortingOrder + 1;
            targetStrengthPips[i].MeshRenderer.sortingOrder = canvas.sortingOrder + 1;
            teamStrengthPips[i].Reset();
            teamStrengthPips[i].Update();
            targetStrengthPips[i].Reset();
            targetStrengthPips[i].Update();
        }
    }

    public void UpdateCombatSkillPanel(Hero hero, bool allowedInteraction)
    {
        interactable = allowedInteraction && hero.HeroClass.CanSelectCombatSkills;
        currentHero = hero;

        for (int i = 0; i < SkillSlots.Count; i++)
        {
            SkillSlots[i].UpdateSkill(hero, i);
            SkillSlots[i].Interactable = interactable;
            SkillSlots[i].Available = true;
        }

        if (currentHero.SelectedCombatSkills.Count == 4)
            for (int i = 0; i < SkillSlots.Count; i++)
            {
                if (!SkillSlots[i].Selected)
                {
                    SkillSlots[i].Available = false;
                    SkillSlots[i].SkillIcon.material = SkillSlots[i].Highlighted ?
                        DarkestDungeonManager.GrayHighlightMaterial : DarkestDungeonManager.GrayMaterial;
                }
                else
                {
                    SkillSlots[i].Available = true;
                    SkillSlots[i].SkillIcon.material = SkillSlots[i].Highlighted ?
                        DarkestDungeonManager.HighlightMaterial : SkillSlots[i].SkillIcon.defaultMaterial;
                }
            }
        else
            for (int i = 0; i < SkillSlots.Count; i++)
            {
                if (!SkillSlots[i].Locked)
                {
                    SkillSlots[i].SkillIcon.material = SkillSlots[i].Highlighted ?
                        DarkestDungeonManager.HighlightMaterial : SkillSlots[i].SkillIcon.defaultMaterial;
                }
            }

        CalculateStrength();
    }

    private void CharCombatSkillPanelSkillDeselected(SkillSlot slot)
    {
        if (currentHero.SelectedCombatSkills.Count == 3)
            for (int i = 0; i < SkillSlots.Count; i++)
                if (!SkillSlots[i].Selected && !SkillSlots[i].Locked)
                {
                    SkillSlots[i].Available = true;
                    if (SkillSlots[i].Highlighted)
                        SkillSlots[i].SkillIcon.material = DarkestDungeonManager.HighlightMaterial;
                    else
                        SkillSlots[i].SkillIcon.material = SkillSlots[i].SkillIcon.defaultMaterial;
                }
                

        CalculateStrength();
    }

    private void CharCombatSkillPanelSkillSelected(SkillSlot slot)
    {
        if (currentHero.SelectedCombatSkills.Count == 4)
            for (int i = 0; i < SkillSlots.Count; i++)
                if (!SkillSlots[i].Selected)
                {
                    SkillSlots[i].Available = false;
                    if (SkillSlots[i].Highlighted)
                        SkillSlots[i].SkillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
                    else
                        SkillSlots[i].SkillIcon.material = DarkestDungeonManager.GrayMaterial;
                }

        CalculateStrength();
    }

    private void CalculateStrength()
    {
        teamStrCount = new [] { 0, 0, 0, 0, 0 };
        targetStrCount = new [] { 0, 0, 0, 0, 0 };
        friendlyCheck = new [] { false, false, false, false, false };

        for(int i = 0; i < currentHero.SelectedCombatSkills.Count; i++)
        {
            CombatSkill skill = currentHero.SelectedCombatSkills[i];
            for (int j = 0; j < skill.LaunchRanks.Ranks.Count; j++)
                teamStrCount[skill.LaunchRanks.Ranks[j]]++;
            for (int j = 0; j < skill.TargetRanks.Ranks.Count; j++)
            {
                if(skill.TargetRanks.IsSelfFormation)
                    friendlyCheck[skill.TargetRanks.Ranks[j]] = true;
                else
                    targetStrCount[skill.TargetRanks.Ranks[j]]++;
            }
        }

        if(currentHero.SelectedCombatSkills.Count > 4)
        {
            for (int i = 0; i < 4; i++)
            {
                teamStrCount[i + 1] = Mathf.RoundToInt((float)teamStrCount[i + 1] * 4 / currentHero.SelectedCombatSkills.Count);
                targetStrCount[i + 1] = Mathf.RoundToInt((float)targetStrCount[i + 1] * 4 / currentHero.SelectedCombatSkills.Count);
            }
        }

        for(int i = 0; i < 4; i++)
        {
            if (friendlyCheck[i + 1])
                teamStrengthPips[i].state.SetAnimation(0, "skill_strength_friendly_" +
                    Mathf.Clamp(teamStrCount[i + 1], 1, 4), false);
            else
                teamStrengthPips[i].state.SetAnimation(0, "skill_strength_hero_" + 
                    Mathf.Clamp(teamStrCount[i + 1], 0, 4), false);

            targetStrengthPips[i].state.SetAnimation(0, "skill_strength_target_" +
                Mathf.Clamp(targetStrCount[i + 1], 0, 4), false);
        }
    }
}
