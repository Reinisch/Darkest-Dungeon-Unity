using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public delegate void BattleSkillSlotEvent(BattleSkillSlot slot);


public class BattleSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image skillIcon;
    public Image selectorIcon;

    public Hero Hero { get; private set; }
    public CombatSkill Skill { get; private set; }
    public RectTransform RectTransform { get; private set; }

    public bool Selected { get; set; }
    public bool Available { get; set; }
    public bool Highlighted { get; set; }
    public bool Selectable { get; set; }
    public bool Deselectable { get; set; }
    public bool IsEmpty { get; set; }

    public event BattleSkillSlotEvent onSkillSelected;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
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
    }

    public void SetCombatState()
    {
        skillIcon.enabled = true;
        skillIcon.material = skillIcon.defaultMaterial;

        Available = true;
        Selectable = true;
        Deselectable = false;

        IsEmpty = false;
    }
    public void SetDisabledState()
    {
        selectorIcon.enabled = false;
        skillIcon.enabled = true;
        skillIcon.material = DarkestDungeonManager.FullGrayDarkMaterial;

        Available = false;
        Selected = false;
        Selectable = false;
        Deselectable = false;

        IsEmpty = false;
    }
    public void SetEmptyState()
    {
        Hero = null;
        Skill = null;

        selectorIcon.enabled = false;
        skillIcon.enabled = false;

        Selectable = false;
        Deselectable = false;
        Selected = false;
        Available = false;
        Highlighted = false;
        IsEmpty = true;
    }

    public virtual void UpdateSkill(Hero hero, CombatSkill combatSkill)
    {
        Hero = hero;
        Skill = combatSkill;

        if(hero.SelectedCombatSkills.Contains(combatSkill))
        {
            skillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, combatSkill);

            SetDisabledState();
        }
        else
            ResetSkill();
    }
    public virtual void ResetSkill()
    {
        SetEmptyState();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!Available)
            return;

        if (Selected && Deselectable)
            Deselect();
        else if (!Selected && Selectable)
            Select();
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        if (!Available)
            skillIcon.material = DarkestDungeonManager.GrayMaterial;
        else
            skillIcon.material = DarkestDungeonManager.HighlightMaterial;

        if (Skill != null)
            ToolTipManager.Instanse.ShowSkillTooltip(Hero, Skill, eventData, RectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        if (!Available)
            skillIcon.material = DarkestDungeonManager.FullGrayDarkMaterial;
        else
            skillIcon.material = skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
