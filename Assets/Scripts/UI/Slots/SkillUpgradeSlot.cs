using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public delegate void SkillUpgradeSlotEvent(SkillUpgradeSlot slot);

public class SkillUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    RectTransform rectTransform;

    public BuildCostFrame costFrame;

    public Image background;
    public Image icon;

    public UpgradeTree Tree { get; set; }
    public HeroUpgrade Upgrade { get; set; }
    public Hero Hero { get; set; }
    public CombatSkill Skill { get; set; }

    public event SkillUpgradeSlotEvent onClick;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Hero hero, UpgradeTree tree, HeroUpgrade upgrade, int skillIndex)
    {
        Hero = hero;
        Tree = tree;
        Upgrade = upgrade; ;
        Skill = hero.HeroClass.CombatSkillVariants.Find(skill =>
            skill.Id == hero.HeroClass.CombatSkills[skillIndex].Id
            && skill.Level == tree.Upgrades.IndexOf(upgrade));
    }

    public void Reset()
    {
        Tree = null;
        Upgrade = null;
        Hero = null;

        Skill = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Hero == null)
            return;

        icon.material = DarkestDungeonManager.HighlightMaterial;
        if (DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(Tree.Id, Hero, Upgrade) == UpgradeStatus.Locked)
            ToolTipManager.Instanse.ShowSkillTooltip(Skill.HeroSkillTooltip(Hero) + "\n" + Upgrade.PrerequisitesTooltip(Hero, DarkestDungeonManager.Campaign.Estate),
                Skill, eventData, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        else
            ToolTipManager.Instanse.ShowSkillTooltip(Hero, Skill, eventData, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        icon.material = icon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick(this);
    }
}