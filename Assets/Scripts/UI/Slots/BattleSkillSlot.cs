using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image skillIcon;
    [SerializeField]
    private Image selectorIcon;

    public CombatSkill Skill { get; private set; }
    public bool IsEmpty { get; private set; }
    public bool Selected { get; private set; }

    private Hero Hero { get; set; }
    private RectTransform RectTransform { get; set; }
    private bool Available { get; set; }
    private bool Selectable { get; set; }
    private bool Deselectable { get; set; }

    public event Action<BattleSkillSlot> EventSkillSelected;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Select()
    {
        Selected = true;
        selectorIcon.enabled = true;

        if (EventSkillSelected != null)
            EventSkillSelected(this);
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
        IsEmpty = true;
    }

    public void UpdateSkill(Hero hero, CombatSkill combatSkill)
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

    public void ResetSkill()
    {
        SetEmptyState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Available)
            return;

        if (Selected && Deselectable)
            Deselect();
        else if (!Selected && Selectable)
            Select();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        skillIcon.material = !Available ? DarkestDungeonManager.GrayMaterial : DarkestDungeonManager.HighlightMaterial;

        if (Skill != null)
            ToolTipManager.Instanse.ShowSkillTooltip(Hero, Skill, RectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillIcon.material = !Available ? DarkestDungeonManager.FullGrayDarkMaterial : skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
