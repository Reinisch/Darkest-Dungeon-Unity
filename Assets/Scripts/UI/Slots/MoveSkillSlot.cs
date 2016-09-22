using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void MoveSkillSlotEvent(MoveSkillSlot slot);

public class MoveSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image skillIcon;
    public Image selectorIcon;

    public Hero Hero { get; private set; }
    public MoveSkill Skill { get; private set; }
    public RectTransform RectTransform { get; private set; }

    public bool Selected { get; set; }
    public bool Available { get; set; }
    public bool Selectable { get; set; }
    public bool Deselectable { get; set; }
    public bool Highlighted { get; set; }

    public event MoveSkillSlotEvent onSkillSelected;
    public event MoveSkillSlotEvent onSkillDeselected;

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

        if (onSkillDeselected != null)
            onSkillDeselected(this);
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

    public virtual void UpdateSkill()
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
    public virtual void ResetSkill()
    {
        Hero = null;
        Skill = null;

        Selected = false;
        Available = false;
        Highlighted = false;
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
            ToolTipManager.Instanse.Show(Skill.HeroSkillTooltip(Hero), eventData, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
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
