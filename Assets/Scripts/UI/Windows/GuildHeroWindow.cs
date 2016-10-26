using UnityEngine;
using UnityEngine.UI;

public class GuildHeroWindow : MonoBehaviour
{
    public Text heroNameLabel;
    public Text heroClassLabel;
    public Text classSkillLabel;
    public Image heroGuildHeader;

    public SkillUpgradeTreeSlot[] skillTrees;

    public TownManager TownManager { get; set; }
    public Hero ViewedHero { get; set; }

    void InitializeTree(SkillUpgradeTreeSlot treeSlot, Hero hero, int index)
    {
        float discountWep = 1 - DarkestDungeonManager.Campaign.Estate.Guild.Discount;
        var skillTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + "." + hero.HeroClass.CombatSkills[index].Id];
        treeSlot.currentSkill.UpdatePurchaseSlot(hero, skillTree, skillTree.Upgrades[0] as HeroUpgrade, index);
        int lastWepIndex = -1;
        for (int i = 0; i < treeSlot.upgrades.Count; i++)
        {
            treeSlot.upgrades[i].Initialize(hero, skillTree, skillTree.Upgrades[i + 1] as HeroUpgrade, index);
            var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(skillTree.Id, hero, treeSlot.upgrades[i].Upgrade);
            TownManager.UpdateUpgradeSlot(status, treeSlot.upgrades[i], discountWep);
            if (status == UpgradeStatus.Purchased)
                lastWepIndex = i;
        }
        treeSlot.UpdateConnector(lastWepIndex);
    }
    void UpdateTree(SkillUpgradeTreeSlot treeSlot, Hero hero, int index)
    {
        float discountSkill = 1 - DarkestDungeonManager.Campaign.Estate.Guild.Discount;
        var skillTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + "." + hero.HeroClass.CombatSkills[index].Id];
        treeSlot.currentSkill.UpdatePurchaseSlot(hero, skillTree, skillTree.Upgrades[0] as HeroUpgrade, index);
        int lastWepIndex = -1;
        for (int i = 0; i < treeSlot.upgrades.Count; i++)
        {
            var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(skillTree.Id, hero, treeSlot.upgrades[i].Upgrade);
            TownManager.UpdateUpgradeSlot(status, treeSlot.upgrades[i], discountSkill);
            if (status == UpgradeStatus.Purchased)
                lastWepIndex = i;
        }
        treeSlot.UpdateConnector(lastWepIndex);
    }
    void ResetTree(SkillUpgradeTreeSlot treeSlot)
    {
        treeSlot.currentSkill.ResetSkill();
        for (int i = 0; i < treeSlot.upgrades.Count; i++)
            treeSlot.upgrades[i].Reset();
    }

    void GuildHeroWindow_onUpgradeClick(SkillUpgradeSlot slot)
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
                TownManager.EstateSceneManager.currencyPanel.CurrencyDecreased("gold");
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReskillCombatHero(slot.Hero);
                UpdateHeroOverview();
            }
        }
        else if (status == UpgradeStatus.Locked)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_click_locked");
    }
    void GuildHeroWindow_onSkillClick(SkillPurchaseSlot slot)
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
                TownManager.EstateSceneManager.currencyPanel.CurrencyDecreased("gold");
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReskillCombatHero(slot.Hero);
                UpdateHeroOverview();
            }
        }
    }

    public void Initialize(TownManager townManager)
    {
        TownManager = townManager;
        for (int i = 0; i < skillTrees.Length; i++)
        {
            skillTrees[i].currentSkill.onClick += GuildHeroWindow_onSkillClick;
            for (int j = 0; j < skillTrees[i].upgrades.Count; j++)
                skillTrees[i].upgrades[j].onClick += GuildHeroWindow_onUpgradeClick;
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
}