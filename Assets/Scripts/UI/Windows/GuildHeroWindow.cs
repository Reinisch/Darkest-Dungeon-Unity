using UnityEngine;
using UnityEngine.UI;

public class GuildHeroWindow : MonoBehaviour
{
    [SerializeField]
    private Text heroNameLabel;
    [SerializeField]
    private Text heroClassLabel;
    [SerializeField]
    private Text classSkillLabel;
    [SerializeField]
    private Image heroGuildHeader;
    [SerializeField]
    private SkillUpgradeTreeSlot[] skillTrees;

    private TownManager TownManager { get; set; }
    private Hero ViewedHero { get; set; }

    public void Initialize(TownManager townManager)
    {
        TownManager = townManager;
        for (int i = 0; i < skillTrees.Length; i++)
        {
            skillTrees[i].CurrentSkill.EventClicked += SkillPurchaseSlotClicked;
            for (int j = 0; j < skillTrees[i].Upgrades.Count; j++)
                skillTrees[i].Upgrades[j].EventClicked += SkillUpgradeSlotClicked;
        }
    }

    public void LoadHeroOverview(Hero hero)
    {
        heroNameLabel.text = hero.HeroName;
        heroClassLabel.text = LocalizationManager.GetString("hero_class_name_" + hero.ClassStringId);
        classSkillLabel.text = LocalizationManager.GetString("action_verbose_body_guild_" + hero.ClassStringId);
        heroGuildHeader.sprite = DarkestDungeonManager.HeroSprites[hero.ClassStringId].Header;
        ViewedHero = hero;
        for (int i = 0; i < skillTrees.Length; i++)
            InitializeTree(skillTrees[i], hero, i);
    }

    public void UpdateHeroOverview()
    {
        if (ViewedHero != null)
        {
            for (int i = 0; i < skillTrees.Length; i++)
                InitializeTree(skillTrees[i], ViewedHero, i);
        }
    }

    public void ResetWindow()
    {
        ViewedHero = null;
        for (int i = 0; i < skillTrees.Length; i++)
            ResetTree(skillTrees[i]);
    }

    private void InitializeTree(SkillUpgradeTreeSlot treeSlot, Hero hero, int index)
    {
        float discountWep = 1 - DarkestDungeonManager.Campaign.Estate.Guild.Discount;
        var skillTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + "." + hero.HeroClass.CombatSkills[index].Id];
        treeSlot.CurrentSkill.UpdatePurchaseSlot(hero, skillTree, skillTree.Upgrades[0] as HeroUpgrade, index);
        int lastWepIndex = -1;
        for (int i = 0; i < treeSlot.Upgrades.Count; i++)
        {
            treeSlot.Upgrades[i].Initialize(hero, skillTree, skillTree.Upgrades[i + 1] as HeroUpgrade, index);
            var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(skillTree.Id, hero, treeSlot.Upgrades[i].Upgrade);
            TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountWep);
            if (status == UpgradeStatus.Purchased)
                lastWepIndex = i;
        }
        treeSlot.UpdateConnector(lastWepIndex);
    }

    private void ResetTree(SkillUpgradeTreeSlot treeSlot)
    {
        treeSlot.CurrentSkill.ResetSkill();
        for (int i = 0; i < treeSlot.Upgrades.Count; i++)
            treeSlot.Upgrades[i].Reset();
    }

    private void SkillUpgradeSlotClicked(SkillUpgradeSlot slot)
    {
        var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(slot.Tree.Id, slot.Hero, slot.Upgrade);
        if (status == UpgradeStatus.Available)
        {
            bool isFree = false;
            for (int i = 0; i < slot.Tree.Tags.Count; i++)
                if (DarkestDungeonManager.Campaign.EventModifiers.HasFreeUpgrade(slot.Tree.Tags[i]))
                {
                    isFree = true;
                    DarkestDungeonManager.Campaign.EventModifiers.RemoveUpgradeTag(slot.Tree.Tags[i]);
                    break;
                }

            float discount = 1 - DarkestDungeonManager.Campaign.Estate.Guild.Discount;

            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Tree.Id, slot.Hero, slot.Upgrade, discount, isFree))
            {
                TownManager.EstateSceneManager.CurrencyPanel.CurrencyDecreased("gold");
                TownManager.EstateSceneManager.CurrencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReskillCombatHero(slot.Hero);
                UpdateHeroOverview();
            }
        }
        else if (status == UpgradeStatus.Locked)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_click_locked");
    }

    private void SkillPurchaseSlotClicked(SkillPurchaseSlot slot)
    {
        var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(slot.Tree.Id, slot.Hero, slot.Upgrade);
        if (status == UpgradeStatus.Available)
        {
            bool isFree = false;
            for (int i = 0; i < slot.Tree.Tags.Count; i++)
                if (DarkestDungeonManager.Campaign.EventModifiers.HasFreeUpgrade(slot.Tree.Tags[i]))
                {
                    isFree = true;
                    DarkestDungeonManager.Campaign.EventModifiers.RemoveUpgradeTag(slot.Tree.Tags[i]);
                    break;
                }

            float discount = 1 - DarkestDungeonManager.Campaign.Estate.Guild.Discount;

            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Tree.Id, slot.Hero, slot.Upgrade, discount, isFree))
            {
                TownManager.EstateSceneManager.CurrencyPanel.CurrencyDecreased("gold");
                TownManager.EstateSceneManager.CurrencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReskillCombatHero(slot.Hero);
                UpdateHeroOverview();
            }
        }
    }
}