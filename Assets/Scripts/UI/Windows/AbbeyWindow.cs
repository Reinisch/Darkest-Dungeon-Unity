using UnityEngine;
using System.Collections.Generic;

public class AbbeyWindow : BuildingWindow
{
    [SerializeField]
    private UpgradeButton upgradeSwitch;
    [SerializeField]
    private UpgradeWindow upgradeWindow;

    [SerializeField]
    private List<TownHeroSlot> cloisterSlots;
    [SerializeField]
    private List<TownHeroSlot> transeptSlots;
    [SerializeField]
    private List<TownHeroSlot> penanceSlots;

    public override TownManager TownManager { get; set; }
    private Abbey Abbey { get; set; }

    public override void Initialize()
    {
        Abbey = DarkestDungeonManager.Campaign.Estate.Abbey;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Abbey);
        upgradeWindow.UpgradedValue.text = Mathf.RoundToInt(ratio * 100) + "%";

        foreach (var tree in upgradeWindow.UpgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.TreeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.Upgrades.Count; i++)
            {
                tree.Upgrades[i].Tree = DarkestDungeonManager.Data.UpgradeTrees[tree.TreeId];
                tree.Upgrades[i].UpgradeInfo = currentUpgrades[i];
                tree.Upgrades[i].TownUpgrades = new List<ITownUpgrade>(new[] {
                    Abbey.Activities.Find(activity => activity.TreeId == tree.TreeId).GetUpgradeByCode(currentUpgrades[i].Code) });
                tree.Upgrades[i].EventClicked += BuildingUpgradeSlotClicked;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.TreeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.Upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }

        for(int i = 0; i < 3; i++)
        {
            cloisterSlots[i].Initialize(Abbey.Activities[0].ActivitySlots[i]);
            cloisterSlots[i].EventTreatmentButtonClicked += TownHeroSlotTreatmentButtonClicked;
            transeptSlots[i].Initialize(Abbey.Activities[1].ActivitySlots[i]);
            transeptSlots[i].EventTreatmentButtonClicked += TownHeroSlotTreatmentButtonClicked;
            penanceSlots[i].Initialize(Abbey.Activities[2].ActivitySlots[i]);
            penanceSlots[i].EventTreatmentButtonClicked += TownHeroSlotTreatmentButtonClicked;
        }
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        Abbey.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Abbey);
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

        for (int i = 0; i < 3; i++)
        {
            cloisterSlots[i].UpdateSlot();
            transeptSlots[i].UpdateSlot();
            penanceSlots[i].UpdateSlot();
        }
    }

    public override void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "abbey");
            DarkestSoundManager.PlayOneShot("event:/town/enter_abbey");
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
            TownManager.EstateSceneManager.RosterPanel.EventHeroSlotBeginDragging += RosterPanelHeroSlotBeginDragging;
            TownManager.EstateSceneManager.RosterPanel.EventHeroSlotEndDragging += RosterPanelHeroSlotEndDragging;
        }
    }

    public override void WindowClosed()
    {
        if (upgradeSwitch.IsOpened)
        {
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(false);
        }
        TownManager.EstateSceneManager.RosterPanel.EventHeroSlotBeginDragging -= RosterPanelHeroSlotBeginDragging;
        TownManager.EstateSceneManager.RosterPanel.EventHeroSlotEndDragging -= RosterPanelHeroSlotEndDragging;
        gameObject.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            if (cloisterSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                cloisterSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (transeptSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                transeptSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (penanceSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                penanceSlots[i].SetStatus(ActivitySlotStatus.Available);
        }
        TownManager.BuildingWindowActive = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/building_zoomout");
    }

    public void UpdateSlots()
    {
        Abbey.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);

        for (int i = 0; i < 3; i++)
        {
            cloisterSlots[i].UpdateSlot();
            transeptSlots[i].UpdateSlot();
            penanceSlots[i].UpdateSlot();
        }
    }

    public void UpgradeSwitchClicked()
    {
        if(upgradeSwitch.IsOpened)
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

    private void TownHeroSlotTreatmentButtonClicked(TownHeroSlot slot)
    {
        if (slot.ActivitySlot.Status == ActivitySlotStatus.Checkout)
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayGold(slot.ActivitySlot.BaseCost))
            {
                DarkestDungeonManager.Campaign.Estate.RemoveGold(slot.ActivitySlot.BaseCost);
                TownManager.EstateSceneManager.CurrencyPanel.UpdateCurrency();
                TownManager.EstateSceneManager.CurrencyPanel.CurrencyDecreased("gold");
                TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Abbey);
                slot.SetStatus(ActivitySlotStatus.Paid);
                DarkestSoundManager.PlayOneShot("event:/town/abbey_" + slot.ActivityName);
            }
        }
        else if (slot.ActivitySlot.Status == ActivitySlotStatus.Paid)
        {
            TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Available);
            slot.SetStatus(ActivitySlotStatus.Available);
        }
    }

    private void RosterPanelHeroSlotBeginDragging(HeroSlot heroSlot)
    {
        var quirks = heroSlot.Hero.Quirks;
        bool cloisterAllowed = true;
        bool transeptAllowed = true;
        bool penanceAllowed = true;

        foreach (var quirkInfo in quirks)
        {
            if (Abbey.Activities[0].IncompatiableQuirks.Contains(quirkInfo.Quirk.Id))
                cloisterAllowed = false;
            if (Abbey.Activities[1].IncompatiableQuirks.Contains(quirkInfo.Quirk.Id))
                transeptAllowed = false;
            if (Abbey.Activities[2].IncompatiableQuirks.Contains(quirkInfo.Quirk.Id))
                penanceAllowed = false;
        }

        for (int i = 0; i < 3; i++)
        {
            cloisterSlots[i].UpdateHeroAvailable(heroSlot.Hero, cloisterAllowed);
            transeptSlots[i].UpdateHeroAvailable(heroSlot.Hero, transeptAllowed);
            penanceSlots[i].UpdateHeroAvailable(heroSlot.Hero, penanceAllowed);
        }
    }

    private void RosterPanelHeroSlotEndDragging(HeroSlot heroSlot)
    {
        for (int i = 0; i < 3; i++)
        {
            cloisterSlots[i].UpdateAvailable();
            transeptSlots[i].UpdateAvailable();
            penanceSlots[i].UpdateAvailable();
        }
    }
}
