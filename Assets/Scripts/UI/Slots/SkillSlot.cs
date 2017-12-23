using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image skillIcon;
    [SerializeField]
    private Image lockedIcon;
    [SerializeField]
    private Image selectorIcon;
    [SerializeField]
    private Text levelLabel;

    public Image SkillIcon { get { return skillIcon; } }
    public bool Selected { get; private set; }
    public bool Locked { get; private set; }
    public bool Highlighted { get; protected set; }
    public bool Available { private get; set; }
    public bool Interactable { private get; set; }

    protected CombatSkill Skill { get; set; }
    protected RectTransform RectTransform { get; private set; }
    protected Hero CurrentHero;

    public event Action<SkillSlot> EventSkillSelected;
    public event Action<SkillSlot> EventSkillDeselected;

    private void Awake()
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

    public virtual void UpdateSkill(Hero hero, int skillIndex)
    {
        CurrentHero = hero;
        Skill = hero.CurrentCombatSkills[skillIndex];

        if(Skill != null)
        {
            SkillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
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
            SkillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
        }
    }

    public virtual void ResetSkill()
    {
        CurrentHero = null;
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
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_invalid");
            return;
        }

        if (Selected)
        {
            if (!CurrentHero.SelectedCombatSkills.Remove(Skill))
                Debug.LogError("Deselected skill not found.");
            Deselect();
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_unequip");
        }
        else
        {
            if (CurrentHero.SelectedCombatSkills.Count == 4)
                DarkestSoundManager.PlayOneShot("event:/ui/town/button_invalid");
            else
            {
                CurrentHero.SelectedCombatSkills.Add(Skill);
                Select();
                DarkestSoundManager.PlayOneShot("event:/ui/town/character_equip");
            }
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        if (!Available || Locked)
            SkillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
        else
            SkillIcon.material = DarkestDungeonManager.HighlightMaterial;

        if (Skill != null)
            ToolTipManager.Instanse.ShowSkillTooltip(CurrentHero, Skill, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        if (!Available || Locked)
            SkillIcon.material = DarkestDungeonManager.GrayMaterial;
        else
            SkillIcon.material = SkillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}