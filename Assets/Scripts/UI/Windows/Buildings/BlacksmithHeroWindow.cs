using UnityEngine;

public class BlacksmithHeroWindow : HeroOverviewWindow
{
    [SerializeField]
    private EquipmentUpgradeTreeSlot[] equipmentTrees;

    protected override BuildingType BuildingType { get {return BuildingType.Blacksmith; } }

    public override void Initialize()
    {
        base.Initialize();
        
        foreach (EquipmentUpgradeTreeSlot equipmentTree in equipmentTrees)
            foreach (EquipmentUpgradeSlot upgradeSlot in equipmentTree.Upgrades)
                upgradeSlot.EventClicked += EquipmentUpgradeSlotClicked;
    }

    public override void LoadHeroOverview(Hero hero)
    {
        base.LoadHeroOverview(hero);

        InitializeTree(equipmentTrees[0], hero, HeroEquipmentSlot.Weapon);
        InitializeTree(equipmentTrees[1], hero, HeroEquipmentSlot.Armor);
    }

    public override void UpdateHeroOverview()
    {
        base.UpdateHeroOverview();

        if(ViewedHero != null)
        {
            UpdateTree(equipmentTrees[0], ViewedHero, HeroEquipmentSlot.Weapon);
            UpdateTree(equipmentTrees[1], ViewedHero, HeroEquipmentSlot.Armor);
        }
    }

    public override void ResetWindow()
    {
        base.ResetWindow();

        ResetTree(equipmentTrees[0]);
        ResetTree(equipmentTrees[1]);
    }

    private void InitializeTree(EquipmentUpgradeTreeSlot treeSlot, Hero hero, HeroEquipmentSlot slot)
    {
        switch (slot)
        {
            case HeroEquipmentSlot.Weapon:
                float discountWep = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var weaponTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".weapon"];
                treeSlot.CurrentEquipment.UpdateEquipment(hero.Weapon, hero);
                int lastWepIndex = -1;
                for (int i = 0; i < treeSlot.Upgrades.Count; i++)
                {
                    treeSlot.Upgrades[i].Initialize(hero, weaponTree, weaponTree.Upgrades[i] as HeroUpgrade,
                        hero.HeroClass.Weapons.Find(wep => wep.UpgradeLevel == i + 2));
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(weaponTree.Id, hero, treeSlot.Upgrades[i].Upgrade);
                    EstateSceneManager.Instanse.TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountWep);
                    if (status == UpgradeStatus.Purchased)
                        lastWepIndex = i;
                }
                treeSlot.UpdateConnector(lastWepIndex);
                break;
            case HeroEquipmentSlot.Armor:
                float discountArm = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var armorTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".armour"];
                treeSlot.CurrentEquipment.UpdateEquipment(hero.Armor, hero);
                int lastArmIndex = -1;
                for (int i = 0; i < treeSlot.Upgrades.Count; i++)
                {
                    treeSlot.Upgrades[i].Initialize(hero, armorTree, armorTree.Upgrades[i] as HeroUpgrade,
                        hero.HeroClass.Armors.Find(arm => arm.UpgradeLevel == i + 2));
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(armorTree.Id, hero, treeSlot.Upgrades[i].Upgrade);
                    EstateSceneManager.Instanse.TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountArm);
                    if (status == UpgradeStatus.Purchased)
                        lastArmIndex = i;
                }
                treeSlot.UpdateConnector(lastArmIndex);
                break;
        }
    }

    private void UpdateTree(EquipmentUpgradeTreeSlot treeSlot, Hero hero, HeroEquipmentSlot slot)
    {
        switch (slot)
        {
            case HeroEquipmentSlot.Weapon:
                float discountWep = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var weaponTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".weapon"];
                treeSlot.CurrentEquipment.UpdateEquipment(hero.Weapon, hero);

                int lastWepIndex = -1;
                for (int i = 0; i < treeSlot.Upgrades.Count; i++)
                {
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(weaponTree.Id, hero, treeSlot.Upgrades[i].Upgrade);
                    EstateSceneManager.Instanse.TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountWep);
                    if (status == UpgradeStatus.Purchased)
                        lastWepIndex = i;
                }
                treeSlot.UpdateConnector(lastWepIndex);
                break;
            case HeroEquipmentSlot.Armor:
                float discountArm = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var armorTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".armour"];
                treeSlot.CurrentEquipment.UpdateEquipment(hero.Armor, hero);
                int lastArmIndex = -1;
                for (int i = 0; i < treeSlot.Upgrades.Count; i++)
                {
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(armorTree.Id, hero, treeSlot.Upgrades[i].Upgrade);
                    EstateSceneManager.Instanse.TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountArm);
                    if (status == UpgradeStatus.Purchased)
                        lastArmIndex = i;
                }
                treeSlot.UpdateConnector(lastArmIndex);
                break;
        }
    }

    private void ResetTree(EquipmentUpgradeTreeSlot treeSlot)
    {
        treeSlot.CurrentEquipment.ResetEquipment();
        for (int i = 0; i < treeSlot.Upgrades.Count; i++)
            treeSlot.Upgrades[i].Reset();
    }

    private void EquipmentUpgradeSlotClicked(EquipmentUpgradeSlot slot)
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

            float discount = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;

            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Tree.Id, slot.Hero, slot.Upgrade, discount, isFree))
            {
                EstateSceneManager.Instanse.CurrencyPanel.CurrencyDecreased("gold");
                EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReequipHero(slot.Hero);
                UpdateHeroOverview();
                if (slot.Tree.Tags.Contains("weapon"))
                    DarkestSoundManager.PlayOneShot("event:/town/blacksmith_purchase_weapon");
                else
                    DarkestSoundManager.PlayOneShot("event:/town/blacksmith_purchase_armor");
            }
        }
        else if (status == UpgradeStatus.Locked)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_click_locked");
    }
}