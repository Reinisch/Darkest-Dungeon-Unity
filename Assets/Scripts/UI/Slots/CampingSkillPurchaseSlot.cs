using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CampingSkillPurchaseSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private BuildCostFrame costFrame;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Image locker;

    public Hero Hero { get; private set; }
    public CampingSkill Skill { get; private set; }
    public bool Unlocked { get; private set; }

    private bool Highlighted { get; set; }

    private RectTransform rectTransform;

    public event Action<CampingSkillPurchaseSlot> EventClicked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Hero hero, int skillIndex, float discount)
    {
        Hero = hero;
        Skill = hero.HeroClass.CampingSkills[skillIndex];
        icon.sprite = DarkestDungeonManager.Data.Sprites["camp_skill_" + Skill.Id];
        costFrame.HeirloomOneAmount.text = Mathf.RoundToInt(Skill.CurrencyCost.Amount * discount).ToString();

        if (hero.CurrentCampingSkills[skillIndex] == null)
            Lock();
        else
            Unlock();
    }

    public void UpdateSkill(float discount)
    {
        if (Hero == null || Skill == null)
            return;

        if (Hero.CurrentCampingSkills[Hero.HeroClass.CampingSkills.IndexOf(Skill)] == null)
            Lock();
        else
            Unlock();

        costFrame.HeirloomOneAmount.text = Mathf.RoundToInt(Skill.CurrencyCost.Amount * discount).ToString();
    }

    public void Reset()
    {
        Hero = null;
        Skill = null;
    }

    public void Lock()
    {
        Unlocked = false;
        locker.enabled = true;

        icon.material = Highlighted ? DarkestDungeonManager.GrayHighlightMaterial : DarkestDungeonManager.GrayMaterial;

        costFrame.gameObject.SetActive(true);
    }

    public void Unlock()
    {
        Unlocked = true;
        locker.enabled = false;

        icon.material = Highlighted ? DarkestDungeonManager.HighlightMaterial : icon.defaultMaterial;

        costFrame.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;

        if (Hero == null)
            return;

        icon.material = Unlocked ? DarkestDungeonManager.HighlightMaterial : DarkestDungeonManager.GrayHighlightMaterial;

        if(Skill != null)
            ToolTipManager.Instanse.Show(Skill.Tooltip(), rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;

        icon.material = Unlocked ? icon.defaultMaterial : DarkestDungeonManager.GrayMaterial;
        
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EventClicked != null)
            EventClicked(this);
    }
}