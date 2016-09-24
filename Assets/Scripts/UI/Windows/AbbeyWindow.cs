using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AbbeyWindow : BuildingWindow
{
    public Text buildingLabel;
    public Text cloisterLabel;
    public Text cloisterDescription;
    public Text transeptLabel;
    public Text transeptDescription;
    public Text penanceLabel;
    public Text pananceDescription;

    public UpgradeButton upgradeSwitch;
    public UpgradeWindow upgradeWindow;

    public Button closeButton;   

    public List<TownHeroSlot> cloisterSlots;
    public List<TownHeroSlot> transeptSlots;
    public List<TownHeroSlot> penanceSlots;

    public override TownManager TownManager { get; set; }
    public Abbey Abbey { get; private set; }

    void AbbeyWindow_onUpgradeClick(BuildingUpgradeSlot slot)
    {
        var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(slot.Tree.Id, slot.UpgradeInfo);
        if (status == UpgradeStatus.Available)
        {
            if (DarkestDungeonManager.Campaign.Estate.BuyUpgrade(slot.Tree.Id, slot.UpgradeInfo))
            {
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                UpdateUpgradeTrees();
            }
        }
    }
    void AbbeyWindow_onTreatmentButtonClick(TownHeroSlot slot)
    {
        if (slot.ActivitySlot.Status == ActivitySlotStatus.Checkout)
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayGold(slot.ActivitySlot.BaseCost))
            {
                DarkestDungeonManager.Campaign.Estate.RemoveGold(slot.ActivitySlot.BaseCost);
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                TownManager.EstateSceneManager.currencyPanel.CurrencyDecreased("gold");
                TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Abbey);
                slot.SetStatus(ActivitySlotStatus.Paid);
            }
        }
        else if (slot.ActivitySlot.Status == ActivitySlotStatus.Paid)
        {
            TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Available);
            slot.SetStatus(ActivitySlotStatus.Available);
        }
    }
    void rosterPanel_onHeroSlotBeginDragging(HeroSlot heroSlot)
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
    void rosterPanel_onHeroSlotEndDragging(HeroSlot heroSlot)
    {
        for (int i = 0; i < 3; i++)
        {
            cloisterSlots[i].UpdateAvailable();
            transeptSlots[i].UpdateAvailable();
            penanceSlots[i].UpdateAvailable();
        }
    }

    public override void Initialize()
    {
        Abbey = DarkestDungeonManager.Campaign.Estate.Abbey;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Abbey);
        upgradeWindow.upgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

        foreach (var tree in upgradeWindow.upgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.upgrades.Count; i++)
            {
                tree.upgrades[i].Tree = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId];
                tree.upgrades[i].UpgradeInfo = currentUpgrades[i];
                tree.upgrades[i].TownUpgrades = new List<ITownUpgrade>(new ITownUpgrade[] {
                    Abbey.Activities.Find(activity => activity.TreeId == tree.treeId).GetUpgradeByCode(currentUpgrades[i].Code) });
                tree.upgrades[i].onClick += AbbeyWindow_onUpgradeClick;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }

        for(int i = 0; i < 3; i++)
        {
            cloisterSlots[i].Initialize(Abbey.Activities[0].ActivitySlots[i]);
            cloisterSlots[i].onTreatmentButtonClick += AbbeyWindow_onTreatmentButtonClick;
            transeptSlots[i].Initialize(Abbey.Activities[1].ActivitySlots[i]);
            transeptSlots[i].onTreatmentButtonClick += AbbeyWindow_onTreatmentButtonClick;
            penanceSlots[i].Initialize(Abbey.Activities[2].ActivitySlots[i]);
            penanceSlots[i].onTreatmentButtonClick += AbbeyWindow_onTreatmentButtonClick;
        }
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
    public void UpdateUpgradeTrees()
    {
        Abbey.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Abbey);
        upgradeWindow.upgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

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

        for (int i = 0; i < 3; i++)
        {
            cloisterSlots[i].UpdateSlot();
            transeptSlots[i].UpdateSlot();
            penanceSlots[i].UpdateSlot();
        }
    }

    public void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
            TownManager.EstateSceneManager.rosterPanel.onHeroSlotBeginDragging += rosterPanel_onHeroSlotBeginDragging;
            TownManager.EstateSceneManager.rosterPanel.onHeroSlotEndDragging += rosterPanel_onHeroSlotEndDragging;
        }
    }
    public override void WindowClosed()
    {
        if (upgradeSwitch.IsOpened)
        {
            upgradeSwitch.SwitchUpgrades();
            upgradeWindow.gameObject.SetActive(false);
        }
        TownManager.EstateSceneManager.rosterPanel.onHeroSlotBeginDragging -= rosterPanel_onHeroSlotBeginDragging;
        TownManager.EstateSceneManager.rosterPanel.onHeroSlotEndDragging -= rosterPanel_onHeroSlotEndDragging;
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
