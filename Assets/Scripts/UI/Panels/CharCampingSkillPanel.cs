using UnityEngine;
using System.Collections.Generic;

public class CharCampingSkillPanel : MonoBehaviour
{
    [SerializeField]
    private List<CampingSkillSlot> skillSlots;

    private Hero currentHero;
    private bool interactable;

    private void Awake()
    {
        for (int i = 0; i < skillSlots.Count; i++)
        {
            skillSlots[i].EventSkillSelected += CharCampingSkillPanelSkillSelected;
            skillSlots[i].EventSkillDeselected += CharCampingSkillPanelSkillDeselected;
        }
    }

    private void CharCampingSkillPanelSkillDeselected(CampingSkillSlot slot)
    {
        if (currentHero.SelectedCampingSkills.Count == 3)
            for (int i = 0; i < skillSlots.Count; i++)
                if (!skillSlots[i].Selected && !skillSlots[i].Locked)
                {
                    skillSlots[i].Available = true;
                    skillSlots[i].SkillIcon.material = skillSlots[i].Highlighted ?
                        DarkestDungeonManager.HighlightMaterial : skillSlots[i].SkillIcon.defaultMaterial;
                }
    }

    private void CharCampingSkillPanelSkillSelected(CampingSkillSlot slot)
    {
        if (currentHero.SelectedCampingSkills.Count != 4)
            return;

        for (int i = 0; i < skillSlots.Count; i++)
            if (!skillSlots[i].Selected)
            {
                skillSlots[i].Available = false;
                skillSlots[i].SkillIcon.material = skillSlots[i].Highlighted ?
                    DarkestDungeonManager.GrayHighlightMaterial : DarkestDungeonManager.GrayMaterial;
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
            foreach (CampingSkillSlot slot in skillSlots)
            {
                if (!slot.Selected)
                {
                    slot.Available = false;
                    slot.SkillIcon.material = slot.Highlighted ? DarkestDungeonManager.GrayHighlightMaterial : DarkestDungeonManager.GrayMaterial;
                }
                else
                {
                    slot.Available = true;
                    slot.SkillIcon.material = slot.Highlighted ? DarkestDungeonManager.HighlightMaterial : slot.SkillIcon.defaultMaterial;
                }
            }
        else
            foreach (CampingSkillSlot slot in skillSlots)
            {
                if (!slot.Locked)
                    slot.SkillIcon.material = slot.Highlighted ? DarkestDungeonManager.HighlightMaterial : slot.SkillIcon.defaultMaterial;
                else
                    slot.SkillIcon.material = slot.Highlighted ? DarkestDungeonManager.GrayHighlightMaterial : DarkestDungeonManager.GrayMaterial;
            }
    }
}
