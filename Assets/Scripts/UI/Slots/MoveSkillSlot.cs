using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MoveSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image skillIcon;
    [SerializeField]
    private Image selectorIcon;

    public MoveSkill Skill { get; private set; }

    private RectTransform RectTransform { get; set; }
    private Hero Hero { get; set; }
    private bool Selected { get; set; }
    private bool Available { get; set; }
    private bool Selectable { get; set; }
    private bool Deselectable { get; set; }

    public event Action<MoveSkillSlot> EventSkillSelected;
    public event Action<MoveSkillSlot> EventSkillDeselected;

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

        if (EventSkillDeselected != null)
            EventSkillDeselected(this);
    }

    public void SetCombatState()
    {
        skillIcon.enabled = true;
        skillIcon.material = skillIcon.defaultMaterial;

        Available = true;
        Selectable = true;
        Deselectable = false;
    }

    public void SetMovableState()
    {
        skillIcon.enabled = true;
        skillIcon.material = skillIcon.defaultMaterial;

        Available = true;
        Selectable = true;
        Deselectable = true;
    }

    public void SetDisabledState()
    {
        skillIcon.enabled = true;
        skillIcon.material = DarkestDungeonManager.FullGrayDarkMaterial;

        Available = false;
        Selectable = false;
        Deselectable = false;

        Selected = false;
        selectorIcon.enabled = false;
    }

    public void UpdateSkill()
    {
        if (RaidSceneManager.RaidPanel.SelectedHero.HeroClass.MoveSkill != null)
        {
            Hero = RaidSceneManager.RaidPanel.SelectedHero;
            Skill = RaidSceneManager.RaidPanel.SelectedHero.HeroClass.MoveSkill;

            Selected = false;
            selectorIcon.enabled = false;
        }
        else
            ResetSkill();
    }

    public void ResetSkill()
    {
        Hero = null;
        Skill = null;

        Selected = false;
        Available = false;
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
            ToolTipManager.Instanse.Show(Skill.HeroSkillTooltip(Hero), RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillIcon.material = !Available ? DarkestDungeonManager.FullGrayDarkMaterial : skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
