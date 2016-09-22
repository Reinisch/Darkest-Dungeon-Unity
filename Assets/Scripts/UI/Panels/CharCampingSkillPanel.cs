using UnityEngine;
using System.Collections.Generic;

public class CharCampingSkillPanel : MonoBehaviour
{
    public List<CampingSkillSlot> skillSlots;

    Hero currentHero;
    bool interactable;

    void Awake()
    {
        for (int i = 0; i < skillSlots.Count; i++)
        {
            skillSlots[i].onSkillSelected += CharCampingSkillPanel_onSkillSelected;
            skillSlots[i].onSkillDeselected += CharCampingSkillPanel_onSkillDeselected;
        }
    }

    void CharCampingSkillPanel_onSkillDeselected(CampingSkillSlot slot)
    {
        if (currentHero.SelectedCampingSkills.Count == 3)
            for (int i = 0; i < skillSlots.Count; i++)
                if (!skillSlots[i].Selected && !skillSlots[i].Locked)
                {
                    skillSlots[i].Available = true;
                    if (skillSlots[i].Highlighted)
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.HighlightMaterial;
                    else
                        skillSlots[i].skillIcon.material = skillSlots[i].skillIcon.defaultMaterial;
                }
    }
    void CharCampingSkillPanel_onSkillSelected(CampingSkillSlot slot)
    {
        if (currentHero.SelectedCampingSkills.Count == 4)
            for (int i = 0; i < skillSlots.Count; i++)
                if (!skillSlots[i].Selected)
                {
                    skillSlots[i].Available = false;
                    if (skillSlots[i].Highlighted)
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
                    else
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.GrayMaterial;
                }
    }

    public void UpdateCampingSkillPanel(Hero hero, bool allowedInteraction)
    {
        interactable = allowedInteraction;
        currentHero = hero;

        for (int i = 0; i < skillSlots.Count; i++)
        {
            skillSlots[i].UpdateSkill(hero, i);
            skillSlots[i].Interactable = interactable;
            skillSlots[i].Available = true;
        }

        if (currentHero.SelectedCampingSkills.Count == 4)
            for (int i = 0; i < skillSlots.Count; i++)
            {
                if (!skillSlots[i].Selected)
                {
                    skillSlots[i].Available = false;
                    if (skillSlots[i].Highlighted)
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
                    else
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.GrayMaterial;
                }
                else
                {
                    skillSlots[i].Available = true;
                    if (skillSlots[i].Highlighted)
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.HighlightMaterial;
                    else
                        skillSlots[i].skillIcon.material = skillSlots[i].skillIcon.defaultMaterial;
                }

            }
        else
            for (int i = 0; i < skillSlots.Count; i++)
            {
                if (!skillSlots[i].Locked)
                {
                    if (skillSlots[i].Highlighted)
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.HighlightMaterial;
                    else
                        skillSlots[i].skillIcon.material = skillSlots[i].skillIcon.defaultMaterial;
                }
                else
                    if (skillSlots[i].Highlighted)
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
                    else
                        skillSlots[i].skillIcon.material = DarkestDungeonManager.GrayMaterial;

            }
    }
}
