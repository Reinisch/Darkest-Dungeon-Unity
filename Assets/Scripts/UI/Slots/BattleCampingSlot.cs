using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void BattleCampingSlotEvent(BattleCampingSlot slot);

public class BattleCampingSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image skillIcon;
    public Image selectorIcon;

    public Hero Hero { get; private set; }
    public CampingSkill Skill { get; private set; }
    public RectTransform RectTransform { get; private set; }

    public bool Selected { get; set; }
    public bool Available { get; set; }
    public bool Highlighted { get; set; }
    public bool Selectable { get; set; }
    public bool Deselectable { get; set; }
    public bool IsEmpty { get; set; }

    public event BattleCampingSlotEvent onSkillSelected;
    public event BattleCampingSlotEvent onSkillDeselected;

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
    public void Deselect(bool clearDeselect = false)
    {
        Selected = false;
        selectorIcon.enabled = false;

        if(!clearDeselect)
            if (onSkillDeselected != null)
                onSkillDeselected(this);
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

    public virtual void UpdateSkill(Hero hero, CampingSkill campingSkill)
    {
        Hero = hero;
        Skill = campingSkill;

        if (hero.SelectedCampingSkills.Contains(campingSkill))
        {
            skillIcon.sprite = DarkestDungeonManager.Data.Sprites["camp_skill_" + Skill.Id];

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
        if (RaidSceneManager.RaidEvents.CampEvent.ActionType != CampUsageResultType.Wait)
            return;

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
            ToolTipManager.Instanse.Show(Skill.Tooltip(), eventData, RectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
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