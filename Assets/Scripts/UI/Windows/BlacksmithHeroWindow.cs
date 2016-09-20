using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BlacksmithHeroWindow : MonoBehaviour
{
    public Text heroNameLabel;
    public Text heroClassLabel;
    public Text classEquipLabel;
    public Image heroGuildHeader;

    public EquipmentUpgradeTreeSlot[] equipmentTrees;

    public TownManager TownManager { get; set; }
    public Hero ViewedHero { get; set; }

    void InitializeTree(EquipmentUpgradeTreeSlot treeSlot, Hero hero, HeroEquipmentSlot slot)
    {
        switch (slot)
        {
            case HeroEquipmentSlot.Weapon:
                float discountWep = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var weaponTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".weapon"];
                treeSlot.currentEquipment.UpdateEquipment(hero.Weapon, hero);
                int lastWepIndex = -1;
                for (int i = 0; i < treeSlot.upgrades.Count; i++)
                {
                    treeSlot.upgrades[i].Initialize(hero, weaponTree, weaponTree.Upgrades[i] as HeroUpgrade, hero.HeroClass.Weapons.Find(wep => wep.UpgradeLevel == i + 2));
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(weaponTree.Id, hero, treeSlot.upgrades[i].Upgrade);
                    TownManager.UpdateUpgradeSlot(status, treeSlot.upgrades[i], discountWep);
                    if (status == UpgradeStatus.Purchased)
                        lastWepIndex = i;
                }
                treeSlot.UpdateConnector(lastWepIndex);
                break;
            case HeroEquipmentSlot.Armor:
                float discountArm = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var armorTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".armour"];
                treeSlot.currentEquipment.UpdateEquipment(hero.Armor, hero);
                int lastArmIndex = -1;
                for (int i = 0; i < treeSlot.upgrades.Count; i++)
                {
                    treeSlot.upgrades[i].Initialize(hero, armorTree, armorTree.Upgrades[i] as HeroUpgrade, hero.HeroClass.Armors.Find(arm => arm.UpgradeLevel == i + 2));
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(armorTree.Id, hero, treeSlot.upgrades[i].Upgrade);
                    TownManager.UpdateUpgradeSlot(status, treeSlot.upgrades[i], discountArm);
                    if (status == UpgradeStatus.Purchased)
                        lastArmIndex = i;
                }
                treeSlot.UpdateConnector(lastArmIndex);
                break;
        }
    }
    void UpdateTree(EquipmentUpgradeTreeSlot treeSlot, Hero hero, HeroEquipmentSlot slot)
    {
        switch (slot)
        {
            case HeroEquipmentSlot.Weapon:
                float discountWep = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var weaponTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".weapon"];
                treeSlot.currentEquipment.UpdateEquipment(hero.Weapon, hero);
                int lastWepIndex = -1;
                for (int i = 0; i < treeSlot.upgrades.Count; i++)
                {
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(weaponTree.Id, hero, treeSlot.upgrades[i].Upgrade);
                    TownManager.UpdateUpgradeSlot(status, treeSlot.upgrades[i], discountWep);
                    if (status == UpgradeStatus.Purchased)
                        lastWepIndex = i;
                }
                treeSlot.UpdateConnector(lastWepIndex);
                break;
            case HeroEquipmentSlot.Armor:
                float discountArm = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;
                var armorTree = DarkestDungeonManager.Data.UpgradeTrees[hero.ClassStringId + ".armour"];
                treeSlot.currentEquipment.UpdateEquipment(hero.Armor, hero);
                int lastArmIndex = -1;
                for (int i = 0; i < treeSlot.upgrades.Count; i++)
                {
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(armorTree.Id, hero, treeSlot.upgrades[i].Upgrade);
                    TownManager.UpdateUpgradeSlot(status, treeSlot.upgrades[i], discountArm);
                    if (status == UpgradeStatus.Purchased)
                        lastArmIndex = i;
                }
                treeSlot.UpdateConnector(lastArmIndex);
                break;
        }
    }
    void ResetTree(EquipmentUpgradeTreeSlot treeSlot)
    {
        treeSlot.currentEquipment.ResetEquipment();
        for (int i = 0; i < treeSlot.upgrades.Count; i++)
            treeSlot.upgrades[i].Reset();
    }

    void BlacksmithHeroWindow_onUpgradeClick(EquipmentUpgradeSlot slot)
    {
        var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(slot.Tree.Id, slot.Hero, slot.Upgrade);
        if (status == UpgradeStatus.Available)
        {
            float discount = 1 - DarkestDungeonManager.Campaign.Estate.Blacksmith.Discount;

            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Tree.Id, slot.Hero, slot.Upgrade, discount))
            {
                TownManager.EstateSceneManager.currencyPanel.CurrencyDecreased("gold");
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                DarkestDungeonManager.Campaign.Estate.ReequipHero(slot.Hero);
                UpdateHeroOverview();
            }
        }
    }

    public void Initialize(TownManager townManager)
    {
        TownManager = townManager;
        for(int i = 0; i < equipmentTrees.Length; i++)
        {
            for (int j = 0; j < equipmentTrees[i].upgrades.Count; j++)
                equipmentTrees[i].upgrades[j].onClick += BlacksmithHeroWindow_onUpgradeClick;
        }
    }

    public void LoadHeroOverview(Hero hero)
    {
        heroNameLabel.text = hero.HeroName;
        heroClassLabel.text = LocalizationManager.GetString("hero_class_name_" + hero.ClassStringId);
        classEquipLabel.text = LocalizationManager.GetString("action_verbose_body_blacksmith_" + hero.ClassStringId);
        heroGuildHeader.sprite = DarkestDungeonManager.HeroSprites[hero.ClassStringId].Header;
        ViewedHero = hero;
        InitializeTree(equipmentTrees[0], hero, HeroEquipmentSlot.Weapon);
        InitializeTree(equipmentTrees[1], hero, HeroEquipmentSlot.Armor);
    }

    public void UpdateHeroOverview()
    {
        if(ViewedHero != null)
        {
            UpdateTree(equipmentTrees[0], ViewedHero, HeroEquipmentSlot.Weapon);
            UpdateTree(equipmentTrees[1], ViewedHero, HeroEquipmentSlot.Armor);
        }
    }

    public void ResetWindow()
    {
        ViewedHero = null;
        ResetTree(equipmentTrees[0]);
        ResetTree(equipmentTrees[1]);
    }
}