using UnityEngine.EventSystems;

public delegate void SkillPurchaseSlotEvent(SkillPurchaseSlot slot);

public class SkillPurchaseSlot : SkillSlot
{
    public UpgradeTree Tree { get; set; }
    public HeroUpgrade Upgrade { get; set; }
    public Hero Hero
    {
        get
        {
            return currentHero;
        }
    }

    public event SkillPurchaseSlotEvent onClick;

    public void UpdatePurchaseSlot(Hero hero, UpgradeTree upgradeTree, HeroUpgrade upgrade, int skillIndex)
    {
        Tree = upgradeTree;
        Upgrade = upgrade;
        UpdateSkill(hero, skillIndex);
    }
    public override void UpdateSkill(Hero hero, int skillIndex)
    {
        currentHero = hero;
        Skill = hero.CurrentCombatSkills[skillIndex];

        if (Skill != null)
        {
            skillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
            Unlock();
            Deselect();
        }
        else
        {
            Skill = hero.HeroClass.CombatSkills[skillIndex];
            skillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
            Lock();
            Deselect();
        }

        if (Locked)
            skillIcon.material = DarkestDungeonManager.GrayMaterial;
        else
            skillIcon.material = skillIcon.defaultMaterial;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick(this);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        if (Locked)
            skillIcon.material = DarkestDungeonManager.GrayHighlightMaterial;
        else
            skillIcon.material = DarkestDungeonManager.HighlightMaterial;

        if (Skill != null)
            ToolTipManager.Instanse.ShowSkillTooltip(currentHero, Skill,
                eventData, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        if (Locked)
            skillIcon.material = DarkestDungeonManager.GrayMaterial;
        else
            skillIcon.material = skillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
