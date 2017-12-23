using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private BuildCostFrame costFrame;
    [SerializeField]
    private Image background;
    [SerializeField]
    private Image icon;

    public BuildCostFrame CostFrame { get { return costFrame; } }
    public Image Background { get { return background; } }
    public Image Icon { get { return icon; } }

    public UpgradeTree Tree { get; private set; }
    public HeroUpgrade Upgrade { get; private set; }
    public Hero Hero { get; private set; }

    private CombatSkill Skill { get; set; }
    private RectTransform rectTransform;

    public event Action<SkillUpgradeSlot> EventClicked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Hero hero, UpgradeTree tree, HeroUpgrade upgrade, int skillIndex)
    {
        Hero = hero;
        Tree = tree;
        Upgrade = upgrade;
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

        Icon.material = DarkestDungeonManager.HighlightMaterial;
        if (DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(Tree.Id, Hero, Upgrade) == UpgradeStatus.Locked)
            ToolTipManager.Instanse.ShowSkillTooltip(Skill.HeroSkillTooltip(Hero) + "\n" +
                Upgrade.PrerequisitesTooltip(Hero, DarkestDungeonManager.Campaign.Estate),
                Skill, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
        else
            ToolTipManager.Instanse.ShowSkillTooltip(Hero, Skill, rectTransform, ToolTipStyle.FromRight, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Icon.material = Icon.defaultMaterial;
        ToolTipManager.Instanse.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EventClicked != null)
            EventClicked(this);
    }
}