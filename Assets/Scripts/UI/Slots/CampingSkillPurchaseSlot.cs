using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public delegate void CampingSkillPurchaseSlotEvent(CampingSkillPurchaseSlot slot);

public class CampingSkillPurchaseSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    RectTransform rectTransform;

    public BuildCostFrame costFrame;
    public Image icon;
    public Image locker;

    public Hero Hero { get; set; }
    public CampingSkill Skill { get; set; }
    public bool Unlocked { get; set; }
    public bool Highlighted { get; set; }

    public event CampingSkillPurchaseSlotEvent onClick;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Hero hero, int skillIndex, float discount)
    {
        Hero = hero;
        Skill = hero.HeroClass.CampingSkills[skillIndex];
        icon.sprite = DarkestDungeonManager.Data.Sprites["camp_skill_" + Skill.Id];
        costFrame.heirloomOneAmount.text = Mathf.RoundToInt(Skill.CurrencyCost.Amount * discount).ToString();

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

        costFrame.heirloomOneAmount.text = Mathf.RoundToInt(Skill.CurrencyCost.Amount * discount).ToString();
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

        if (Highlighted)
            icon.material = DarkestDungeonManager.GrayHighlightMaterial;
        else
            icon.material = DarkestDungeonManager.GrayMaterial;

        costFrame.gameObject.SetActive(true);
    }

    public void Unlock()
    {
        Unlocked = true;
        locker.enabled = false;

        if (Highlighted)
            icon.material = DarkestDungeonManager.HighlightMaterial;
        else
            icon.material = icon.defaultMaterial;

        costFrame.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Hero == null)
            return;

        if(Unlocked)
            icon.material = DarkestDungeonManager.HighlightMaterial;
        else
            icon.material = DarkestDungeonManager.GrayHighlightMaterial;

        if(Skill != null)
            ToolTipManager.Instanse.Show(Skill.Tooltip(), eventData, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(Unlocked)
            icon.material = icon.material = icon.defaultMaterial;
        else
            icon.material = DarkestDungeonManager.GrayMaterial;
        
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick(this);
    }
}