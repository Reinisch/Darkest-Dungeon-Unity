using UnityEngine;
using System.Collections.Generic;

public class StageCoachWindow : BuildingWindow
{
    [SerializeField]
    private RecruitPanel recruitPanel;
    [SerializeField]
    private HeroRosterPanel rosterPanel;
    [SerializeField]
    private UpgradeButton upgradeSwitch;
    [SerializeField]
    private UpgradeWindow upgradeWindow;

    public override TownManager TownManager { get; set; }
    private StageCoach StageCoach { get; set; }
    public RecruitPanel Panel { get { return recruitPanel; } }

    public override void Initialize()
    {
        StageCoach = DarkestDungeonManager.Campaign.Estate.StageCoach;
        Panel.UpdateRecruitPanel(DarkestDungeonManager.Campaign.Estate.StageCoach.Heroes);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.StageCoach);
        upgradeWindow.UpgradedValue.text = Mathf.RoundToInt(ratio * 100) + "%";

        foreach (var tree in upgradeWindow.UpgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.TreeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.Upgrades.Count; i++)
            {
                tree.Upgrades[i].Tree = DarkestDungeonManager.Data.UpgradeTrees[tree.TreeId];
                tree.Upgrades[i].UpgradeInfo = currentUpgrades[i];
                tree.Upgrades[i].TownUpgrades = new List<ITownUpgrade>(new [] {
                    StageCoach.GetUpgradeByCode(tree.TreeId, currentUpgrades[i].Code) });
                tree.Upgrades[i].EventClicked += BuildingUpgradeSlotClicked;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.TreeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.Upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        StageCoach.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.StageCoach);
        upgradeWindow.UpgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

        if (afterPurchase && Mathf.Approximately(ratio, 1))
            DarkestSoundManager.PlayOneShot("event:/town/purchase_upgrade_last");

        foreach (var tree in upgradeWindow.UpgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.TreeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.Upgrades.Count; i++)
            {
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.TreeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.Upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }
        rosterPanel.UpdateCapacity();
    }

    public override void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "stage_coach");
            DarkestSoundManager.PlayOneShot("event:/town/enter_stage_coach");
        }
    }

    public override void WindowClosed()
    {
        if (upgradeSwitch.IsOpened)
        {
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(false);
        }
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
            foreach (var tree in upgradeWindow.UpgradeTrees)
            {
                for (int i = 0; i < tree.Upgrades.Count; i++)
                {
                    var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.TreeId, tree.Upgrades[i].UpgradeInfo);
                    if (status == UpgradeStatus.Available)
                        TownManager.UpdateUpgradeSlot(status, tree.Upgrades[i]);
                }
            }
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(true);
        }
    }

    private void BuildingUpgradeSlotClicked(BuildingUpgradeSlot slot)
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
                TownManager.EstateSceneManager.CurrencyPanel.UpdateCurrency();
                UpdateUpgradeTrees(true);
            }
        }
        else if (status == UpgradeStatus.Locked)
            DarkestSoundManager.PlayOneShot("event:/ui/town/button_click_locked");
    }
}