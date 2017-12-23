using System;
using UnityEngine.EventSystems;

public class SkillPurchaseSlot : SkillSlot
{
    public UpgradeTree Tree { get; private set; }
    public HeroUpgrade Upgrade { get; private set; }
    public Hero Hero { get { return CurrentHero; } }

    public event Action<SkillPurchaseSlot> EventClicked;

    public void UpdatePurchaseSlot(Hero hero, UpgradeTree upgradeTree, HeroUpgrade upgrade, int skillIndex)
    {
        Tree = upgradeTree;
        Upgrade = upgrade;
        UpdateSkill(hero, skillIndex);
    }

    public override void UpdateSkill(Hero hero, int skillIndex)
    {
        CurrentHero = hero;
        Skill = hero.CurrentCombatSkills[skillIndex];

        if (Skill != null)
        {
            SkillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
            Unlock();
            Deselect();
        }
        else
        {
            Skill = hero.HeroClass.CombatSkills[skillIndex];
            SkillIcon.sprite = DarkestDungeonManager.Data.HeroSprites.GetCombatSkillIcon(hero, Skill);
            Lock();
            Deselect();
        }

        SkillIcon.material = Locked ? DarkestDungeonManager.GrayMaterial : SkillIcon.defaultMaterial;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (EventClicked != null)
            EventClicked(this);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        Highlighted = true;
        SkillIcon.material = Locked ? DarkestDungeonManager.GrayHighlightMaterial : DarkestDungeonManager.HighlightMaterial;

        if (Skill != null)
            ToolTipManager.Instanse.ShowSkillTooltip(CurrentHero, Skill, RectTransform, ToolTipStyle.FromBottom, ToolTipSize.Normal);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Highlighted = false;
        SkillIcon.material = Locked ? DarkestDungeonManager.GrayMaterial : SkillIcon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }
}
