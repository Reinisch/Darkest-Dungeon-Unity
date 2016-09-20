using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public delegate void SkillSlotEvent(SkillSlot slot);

public class SkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image skillIcon;
    public Image lockedIcon;
    public Image selectorIcon;
    public Text levelLabel;

    public CombatSkill Skill { get; set; }
    public RectTransform RectTransform { get; set; }

    public bool Selected { get; set; }
    public bool Locked { get; set; }
    public bool Available { get; set; }
    public bool Interactable { get; set; }
    public bool Highlighted { get; set; }

    protected Hero currentHero;

    public event SkillSlotEvent onSkillSelected;
    public event SkillSlotEvent onSkillDeselected;

    void Awake()
    {
        lockedIcon.enabled = false;
        RectTransform = GetComponent<RectTransform>();
    }

    public void Lock()
    {
        Locked = true;
        lockedIcon.enabled = true;
        levelLabel.text = "";
    }
    public void Unlock()
    {
        Locked = false;
        lockedIcon.enabled = false;
        levelLabel.text = (Skill.Level + 1).ToString();
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
        Skill = hero.CurrentCombatSkills[skillIndex];

        if(Skill != null)
        {
            skillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
            Unlock();

            if (hero.SelectedCombatSkills.Contains(Skill))
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
            Skill = hero.HeroClass.CombatSkills[skillIndex];
            skillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
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
            if (!currentHero.SelectedCombatSkills.Remove(Skill))
                Debug.LogError("Deselected skill not found.");
            Deselect();           
        }
        else
        {
            if (currentHero.SelectedCombatSkills.Count == 4)
                return;
            else
            {
                currentHero.SelectedCombatSkills.Add(Skill);
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
            ToolTipManager.Instanse.ShowSkillTooltip(currentHero, Skill, eventData, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
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
