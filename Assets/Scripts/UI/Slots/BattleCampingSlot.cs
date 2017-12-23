using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleCampingSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image skillIcon;
    [SerializeField]
    private Image selectorIcon;

    public CampingSkill Skill { get; private set; }
    public bool Selected { get; private set; }
    public bool IsEmpty { get; private set; }

    private RectTransform RectTransform { get; set; }
    private bool Available { get; set; }
    private bool Selectable { get; set; }
    private bool Deselectable { get; set; }

    public event Action<BattleCampingSlot> EventSkillSelected;
    public event Action<BattleCampingSlot> EventSkillDeselected;

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

    public void Deselect(bool clearDeselect = false)
    {
        Selected = false;
        selectorIcon.enabled = false;

        if(!clearDeselect)
            if (EventSkillDeselected != null)
                EventSkillDeselected(this);
    }

    public void SetCombatState()
    {
        skillIcon.enabled = true;
        skillIcon.material = skillIcon.defaultMaterial;

        Available = true;
        Selectable = true;
        Deselectable = true;

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
        Skill = null;

        selectorIcon.enabled = false;
        skillIcon.enabled = false;

        Selectable = false;
        Deselectable = false;
        Selected = false;
        Available = false;
        IsEmpty = true;
    }

    public void UpdateSkill(Hero hero, CampingSkill campingSkill)
    {
        Skill = campingSkill;

        if (hero.SelectedCampingSkills.Contains(campingSkill))
        {
            skillIcon.sprite = DarkestDungeonManager.Data.Sprites["camp_skill_" + Skill.Id];

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
        if (RaidSceneManager.RaidEvents.CampEvent.ActionType != CampUsageResultType.Wait)
            return;

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
            ToolTipManager.Instanse.Show(Skill.Tooltip(), RectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillIcon.material = !Available ? DarkestDungeonManager.FullGrayDarkMaterial : skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}