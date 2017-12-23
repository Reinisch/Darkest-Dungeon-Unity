using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CampingSkillSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image skillIcon;
    [SerializeField]
    private Image lockedIcon;
    [SerializeField]
    private Image selectorIcon;

    public Image SkillIcon { get { return skillIcon; } }
    public bool Selected { get; private set; }
    public bool Locked { get; private set; }
    public bool Highlighted { get; private set; }
    public bool Available { private get; set; }
    public bool Interactable { private get; set; }

    private CampingSkill Skill { get; set; }
    private RectTransform RectTransform { get; set; }

    private Hero currentHero;

    public event Action<CampingSkillSlot> EventSkillSelected;
    public event Action<CampingSkillSlot> EventSkillDeselected;

    private void Awake()
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

    public void UpdateSkill(Hero hero, int skillIndex)
    {
        currentHero = hero;
        Skill = hero.HeroClass.CampingSkills[skillIndex];
        SkillIcon.sprite = DarkestDungeonManager.Data.Sprites["camp_skill_" + Skill.Id];

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

    public void ResetSkill()
    {
        currentHero = null;
        Skill = null;

        Selected = false;
        Locked = false;
        Available = false;
        Interactable = false;
        Highlighted = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable || Locked)
        {
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_invalid");
            return;
        }

        if (Selected)
        {
            if (!currentHero.SelectedCampingSkills.Remove(Skill))
                Debug.LogError("Deselected camping skill not found.");

            Deselect();
            DarkestSoundManager.PlayOneShot("event:/ui/town/character_unequip");
        }
        else
        {
            if (currentHero.SelectedCampingSkills.Count == 4)
                DarkestSoundManager.PlayOneShot("event:/ui/town/button_invalid");
            else
            {
                currentHero.SelectedCampingSkills.Add(Skill);
                Select();
                DarkestSoundManager.PlayOneShot("event:/ui/town/character_equip");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;

        if (!Available || Locked)
            SkillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
        else
            SkillIcon.material = DarkestDungeonManager.HighlightMaterial;

        if (Skill != null)
            ToolTipManager.Instanse.Show(Skill.Tooltip(), RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        if (!Available || Locked)
            SkillIcon.material = DarkestDungeonManager.GrayMaterial;
        else
            SkillIcon.material = SkillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
