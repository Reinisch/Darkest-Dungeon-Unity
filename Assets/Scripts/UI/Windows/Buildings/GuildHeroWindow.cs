using UnityEngine;

public class GuildHeroWindow : HeroOverviewWindow
{
    [SerializeField]
    private SkillUpgradeTreeSlot[] skillTrees;

    protected override BuildingType BuildingType { get {return BuildingType.Guild; } }

    public override void Initialize()
    {
        base.Initialize();

        foreach (SkillUpgradeTreeSlot skillTree in skillTrees)
        {
            skillTree.CurrentSkill.EventClicked += SkillPurchaseSlotClicked;
            foreach (SkillUpgradeSlot upgradeSlot in skillTree.Upgrades)
                upgradeSlot.EventClicked += SkillUpgradeSlotClicked;
        }
    }

    public override void LoadHeroOverview(Hero hero)
    {
        base.LoadHeroOverview(hero);

        for (int i = 0; i < skillTrees.Length; i++)
            InitializeTree(skillTrees[i], hero, i);
    }

    public override void UpdateHeroOverview()
    {
        base.UpdateHeroOverview();

        if (ViewedHero != null)
        {
            for (int i = 0; i < skillTrees.Length; i++)
                InitializeTree(skillTrees[i], ViewedHero, i);
        }
    }

    public override void ResetWindow()
    {
        base.ResetWindow();
        
        foreach (SkillUpgradeTreeSlot skillTree in skillTrees)
            ResetTree(skillTree);
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
            EstateSceneManager.Instanse.TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountWep);
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
                EstateSceneManager.Instanse.CurrencyPanel.CurrencyDecreased("gold");
                EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
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
                EstateSceneManager.Instanse.CurrencyPanel.CurrencyDecreased("gold");
                EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReskillCombatHero(slot.Hero);
                UpdateHeroOverview();
            }
        }
    }
}