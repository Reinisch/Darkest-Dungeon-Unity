using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public delegate void CampingSkillSlotEvent(CampingSkillSlot slot);

public class CampingSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image skillIcon;
    public Image lockedIcon;
    public Image selectorIcon;

    public CampingSkill Skill { get; set; }
    public RectTransform RectTransform { get; set; }

    public bool Selected { get; set; }
    public bool Locked { get; set; }
    public bool Available { get; set; }
    public bool Interactable { get; set; }
    public bool Highlighted { get; set; }

    protected Hero currentHero;

    public event CampingSkillSlotEvent onSkillSelected;
    public event CampingSkillSlotEvent onSkillDeselected;

    void Awake()
    {
        lockedIcon.enabled = false;
        RectTransform = GetComponent<RectTransform>();
    }

    public void Lock()
    {
        Locked = true;
        lockedIcon.enabled = true;
    }
    public void Unlock()
    {
        Locked = false;
        lockedIcon.enabled = false;
    }

    public void Select()
    {
        Selected = true;
        selectorIcon.enabled = true;

        if (onSkillSelected != null)
            onSkillSelected(this);
    }
    public void Deselect()
    {
        Selected = false;
        selectorIcon.enabled = false;

        if (onSkillDeselected != null)
            onSkillDeselected(this);
    }

    public virtual void UpdateSkill(Hero hero, int skillIndex)
    {
        currentHero = hero;
        Skill = hero.HeroClass.CampingSkills[skillIndex];
        skillIcon.sprite = DarkestDungeonManager.Data.Sprites["camp_skill_" + Skill.Id];

        if (hero.CurrentCampingSkills[skillIndex] != null)
        {
            Unlock();

            if (hero.SelectedCampingSkills.Contains(Skill))
            {
                Selected = true;
                selectorIcon.enabled = true;
            }
            else
            {
                Selected = false;
                selectorIcon.enabled = false;
            }
        }
        else
        {
            Lock();
            Selected = false;
            selectorIcon.enabled = false;
        }
    }
    public virtual void ResetSkill()
    {
        currentHero = null;
        Skill = null;

        Selected = false;
        Locked = false;
        Available = false;
        Interactable = false;
        Highlighted = false;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable || Locked)
            return;

        if (Selected)
        {
            if (!currentHero.SelectedCampingSkills.Remove(Skill))
                Debug.LogError("Deselected camping skill not found.");
            Deselect();
        }
        else
        {
            if (currentHero.SelectedCampingSkills.Count == 4)
                return;
            else
            {
                currentHero.SelectedCampingSkills.Add(Skill);
                Select();
            }
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        if (!Available || Locked)
            skillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
        else
            skillIcon.material = DarkestDungeonManager.HighlightMaterial;

        if (Skill != null)
            ToolTipManager.Instanse.Show(Skill.Tooltip(), eventData, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        if (!Available || Locked)
            skillIcon.material = DarkestDungeonManager.GrayMaterial;
        else
            skillIcon.material = skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
