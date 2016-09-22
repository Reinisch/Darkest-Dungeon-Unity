using UnityEngine;
using System.Collections.Generic;

public class CharCombatSkillPanel : MonoBehaviour
{
    public RectTransform skillPanel;

    public List<SkeletonAnimation> teamStrengthPips;
    public List<SkeletonAnimation> targetStrengthPips;

    public List<SkillSlot> SkillSlots { get; set; }

    Hero currentHero;
    int[] teamStrCount = new int[5] { 0, 0, 0, 0, 0 };
    int[] targetStrCount = new int[5] { 0, 0, 0, 0, 0 };
    bool[] friendlyCheck = new bool[5] { false, false, false, false, false };
    bool interactable;

    void Awake()
    {
        SkillSlots = new List<SkillSlot>(skillPanel.GetComponentsInChildren<SkillSlot>());
        for (int i = 0; i < SkillSlots.Count; i++)
        {
            SkillSlots[i].onSkillSelected += CharCombatSkillPanel_onSkillSelected;
            SkillSlots[i].onSkillDeselected += CharCombatSkillPanel_onSkillDeselected;
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

    void CharCombatSkillPanel_onSkillDeselected(SkillSlot slot)
    {
        if (currentHero.SelectedCombatSkills.Count == 3)
            for (int i = 0; i < SkillSlots.Count; i++)
                if (!SkillSlots[i].Selected && !SkillSlots[i].Locked)
                {
                    SkillSlots[i].Available = true;
                    if (SkillSlots[i].Highlighted)
                        SkillSlots[i].skillIcon.material = DarkestDungeonManager.HighlightMaterial;
                    else
                        SkillSlots[i].skillIcon.material = SkillSlots[i].skillIcon.defaultMaterial;
                }
                

        CalculateStrength();
    }
    void CharCombatSkillPanel_onSkillSelected(SkillSlot slot)
    {
        if (currentHero.SelectedCombatSkills.Count == 4)
            for (int i = 0; i < SkillSlots.Count; i++)
                if (!SkillSlots[i].Selected)
                {
                    SkillSlots[i].Available = false;
                    if (SkillSlots[i].Highlighted)
                        SkillSlots[i].skillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
                    else
                        SkillSlots[i].skillIcon.material = DarkestDungeonManager.GrayMaterial;
                }

        CalculateStrength();
    }

    void CalculateStrength()
    {
        teamStrCount = new int[5] { 0, 0, 0, 0, 0 };
        targetStrCount = new int[5] { 0, 0, 0, 0, 0 };
        friendlyCheck = new bool[5] { false, false, false, false, false };

        for(int i = 0; i < currentHero.SelectedCombatSkills.Count; i++)
        {
            CombatSkill skill = currentHero.SelectedCombatSkills[i];
            for (int j = 0; j < skill.LaunchRanks.Ranks.Count; j++)
                teamStrCount[skill.LaunchRanks.Ranks[j]]++;
            for (int j = 0; j < skill.TargetRanks.Ranks.Count; j++)
            {
                if(skill.TargetRanks.IsSelfFormation)
                {
                    friendlyCheck[skill.TargetRanks.Ranks[j]] = true;
                }
                else
                {
                    targetStrCount[skill.TargetRanks.Ranks[j]]++;
                }
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
                    Mathf.Clamp(teamStrCount[i + 1], 1, 4).ToString(), false);
            else
                teamStrengthPips[i].state.SetAnimation(0, "skill_strength_hero_" + 
                    Mathf.Clamp(teamStrCount[i + 1], 0, 4).ToString(), false);

            targetStrengthPips[i].state.SetAnimation(0, "skill_strength_target_" +
                Mathf.Clamp(targetStrCount[i + 1], 0, 4).ToString(), false);
        }
    }

    public void UpdateCombatSkillPanel(Hero hero, bool allowedInteraction)
    {
        interactable = allowedInteraction ? hero.HeroClass.CanSelectCombatSkills : false;
        currentHero = hero;

        for(int i = 0; i < SkillSlots.Count; i++)
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
                    if(SkillSlots[i].Highlighted)
                        SkillSlots[i].skillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
                    else
                        SkillSlots[i].skillIcon.material = DarkestDungeonManager.GrayMaterial;
                }
                else
                {
                    SkillSlots[i].Available = true;
                    if (SkillSlots[i].Highlighted)
                        SkillSlots[i].skillIcon.material = DarkestDungeonManager.HighlightMaterial;
                    else
                        SkillSlots[i].skillIcon.material = SkillSlots[i].skillIcon.defaultMaterial;
                }

            }
        else
            for (int i = 0; i < SkillSlots.Count; i++)
            {
                if (!SkillSlots[i].Locked)
                {
                    if (SkillSlots[i].Highlighted)
                        SkillSlots[i].skillIcon.material = DarkestDungeonManager.HighlightMaterial;
                    else
                        SkillSlots[i].skillIcon.material = SkillSlots[i].skillIcon.defaultMaterial;
                }
            }

        CalculateStrength();
    }
}
