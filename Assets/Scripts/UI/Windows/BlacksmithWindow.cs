using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BlacksmithWindow : BuildingWindow
{
    public Text dragHeroLabel;
    public BlacksmithHeroWindow heroOverview;
    public HeroObserverSlot heroSlot;

    public UpgradeButton upgradeSwitch;
    public UpgradeWindow upgradeWindow;

    public Button closeButton;

    public override TownManager TownManager { get; set; }
    public Blacksmith Blacksmith { get; private set; }

    void Awake()
    {
        heroSlot.OnHeroDropped += heroSlot_OnHeroDropped;
        heroSlot.OnHeroRemoved += heroSlot_OnHeroRemoved;
    }

    void heroSlot_OnHeroDropped(Hero hero)
    {
        heroOverview.gameObject.SetActive(true);
        heroOverview.LoadHeroOverview(hero);
        dragHeroLabel.enabled = false;
    }

    void heroSlot_OnHeroRemoved(Hero hero)
    {
        heroOverview.gameObject.SetActive(false);
        heroOverview.ResetWindow();
        dragHeroLabel.enabled = true;
    }

    void BlacksmithWindow_onUpgradeClick(BuildingUpgradeSlot slot)
    {
        var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(slot.Tree.Id, slot.UpgradeInfo);
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

            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Tree.Id, slot.UpgradeInfo, isFree))
            {
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                UpdateUpgradeTrees(true);
            }
        }
        else if (status == UpgradeStatus.Locked)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_click_locked");

    }

    public override void Initialize()
    {
        heroOverview.Initialize(TownManager);

        Blacksmith = DarkestDungeonManager.Campaign.Estate.Blacksmith;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Blacksmith);
        upgradeWindow.upgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

        foreach (var tree in upgradeWindow.upgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.upgrades.Count; i++)
            {
                tree.upgrades[i].Tree = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId];
                tree.upgrades[i].UpgradeInfo = currentUpgrades[i];
                if (tree.treeId == "blacksmith.cost")
                {
                    tree.upgrades[i].TownUpgrades = new List<ITownUpgrade>(new ITownUpgrade[] {
                        Blacksmith.GetUpgradeByCode(currentUpgrades[i].Code) });
                }
                tree.upgrades[i].onClick += BlacksmithWindow_onUpgradeClick;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        Blacksmith.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Blacksmith);
        upgradeWindow.upgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

        if (afterPurchase && Mathf.Approximately(ratio, 1))
            DarkestSoundManager.PlayOneShot("event:/town/purchase_upgrade_last");

        foreach (var tree in upgradeWindow.upgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.upgrades.Count; i++)
            {
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }
        heroOverview.UpdateHeroOverview();
    }

    public override void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "blacksmith");
            DarkestSoundManager.PlayOneShot("event:/town/enter_blacksmith");
        }
    }

    public override void WindowClosed()
    {
        if (upgradeSwitch.IsOpened)
        {
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(false);
        }
        heroSlot.ClearSlot();
        gameObject.SetActive(false);
        TownManager.BuildingWindowActive = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/building_zoomout");
    }

    public void UpgradeSwitchClicked()
    {
        if (upgradeSwitch.IsOpened)
        {
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(false);
        }
        else
        {
            foreach (var tree in upgradeWindow.upgradeTrees)
            {
                for (int i = 0; i < tree.upgrades.Count; i++)
                {
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, tree.upgrades[i].UpgradeInfo);
                    if (status == UpgradeStatus.Available)
                        TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                }
            }
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(true);
        }
    }
}
