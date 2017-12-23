using UnityEngine;
using UnityEngine.UI;

public class BlacksmithHeroWindow : MonoBehaviour
{
    [SerializeField]
    private Text heroNameLabel;
    [SerializeField]
    private Text heroClassLabel;
    [SerializeField]
    private Text classEquipLabel;
    [SerializeField]
    private Image heroGuildHeader;
    [SerializeField]
    private EquipmentUpgradeTreeSlot[] equipmentTrees;

    private TownManager TownManager { get; set; }
    private Hero ViewedHero { get; set; }

    public void Initialize(TownManager townManager)
    {
        TownManager = townManager;
        for(int i = 0; i < equipmentTrees.Length; i++)
            for (int j = 0; j < equipmentTrees[i].Upgrades.Count; j++)
                equipmentTrees[i].Upgrades[j].EventClicked += EquipmentUpgradeSlotClicked;
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
                    TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountWep);
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
                    TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountArm);
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
                    TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountWep);
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
                    TownManager.UpdateUpgradeSlot(status, treeSlot.Upgrades[i], discountArm);
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
                TownManager.EstateSceneManager.CurrencyPanel.CurrencyDecreased("gold");
                TownManager.EstateSceneManager.CurrencyPanel.UpdateCurrency();
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