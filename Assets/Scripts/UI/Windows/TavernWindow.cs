using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TavernWindow : BuildingWindow
{
    public Text buildingLabel;
    public Text barLabel;
    public Text barDescription;
    public Text gamblingLabel;
    public Text gamblingDescription;
    public Text brothelLabel;
    public Text brothelDescription;

    public UpgradeButton upgradeSwitch;
    public UpgradeWindow upgradeWindow;

    public Button closeButton;

    public List<TownHeroSlot> barSlots;
    public List<TownHeroSlot> gamblingSlots;
    public List<TownHeroSlot> brothelSlots;

    public override TownManager TownManager { get; set; }
    public Tavern Tavern { get; private set; }

    void TavernWindow_onUpgradeClick(BuildingUpgradeSlot slot)
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
    void TavernWindow_onTreatmentButtonClick(TownHeroSlot slot)
    {
        if (slot.ActivitySlot.Status == ActivitySlotStatus.Checkout)
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayGold(slot.ActivitySlot.BaseCost))
            {
                DarkestDungeonManager.Campaign.Estate.RemoveGold(slot.ActivitySlot.BaseCost);
                TownManager.EstateSceneManager.currencyPanel.UpdateCurrency();
                TownManager.EstateSceneManager.currencyPanel.CurrencyDecreased("gold");
                TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Tavern);
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
        bool barAllowed = true;
        bool gamblingAllowed = true;
        bool brothelAllowed = true;

        foreach (var quirkInfo in quirks)
        {
            if (Tavern.Activities[0].IncompatiableQuirks.Contains(quirkInfo.Quirk.Id))
                barAllowed = false;
            if (Tavern.Activities[1].IncompatiableQuirks.Contains(quirkInfo.Quirk.Id))
                gamblingAllowed = false;
            if (Tavern.Activities[2].IncompatiableQuirks.Contains(quirkInfo.Quirk.Id))
                brothelAllowed = false;
        }
        for (int i = 0; i < 3; i++)
        {
            barSlots[i].UpdateHeroAvailable(heroSlot.Hero, barAllowed);
            gamblingSlots[i].UpdateHeroAvailable(heroSlot.Hero, gamblingAllowed);
            brothelSlots[i].UpdateHeroAvailable(heroSlot.Hero, brothelAllowed);
        }
    }
    void rosterPanel_onHeroSlotEndDragging(HeroSlot heroSlot)
    {
        for (int i = 0; i < 3; i++)
        {
            barSlots[i].UpdateAvailable();
            gamblingSlots[i].UpdateAvailable();
            brothelSlots[i].UpdateAvailable();
        }
    }

    public override void Initialize()
    {
        Tavern = DarkestDungeonManager.Campaign.Estate.Tavern;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Tavern);
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
                    Tavern.Activities.Find(activity => activity.TreeId == tree.treeId).GetUpgradeByCode(currentUpgrades[i].Code) });
                tree.upgrades[i].onClick += TavernWindow_onUpgradeClick;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }

        for (int i = 0; i < 3; i++)
        {
            barSlots[i].Initialize(Tavern.Activities[0].ActivitySlots[i]);
            barSlots[i].onTreatmentButtonClick += TavernWindow_onTreatmentButtonClick;
            gamblingSlots[i].Initialize(Tavern.Activities[1].ActivitySlots[i]);
            gamblingSlots[i].onTreatmentButtonClick += TavernWindow_onTreatmentButtonClick;
            brothelSlots[i].Initialize(Tavern.Activities[2].ActivitySlots[i]);
            brothelSlots[i].onTreatmentButtonClick += TavernWindow_onTreatmentButtonClick;
        }
    }
    
    public void UpdateSlots()
    {
        Tavern.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);

        for (int i = 0; i < 3; i++)
        {
            barSlots[i].UpdateSlot();
            gamblingSlots[i].UpdateSlot();
            brothelSlots[i].UpdateSlot();
        }
    }
    public void UpdateUpgradeTrees()
    {
        Tavern.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Tavern);
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
            barSlots[i].UpdateSlot();
            gamblingSlots[i].UpdateSlot();
            brothelSlots[i].UpdateSlot();
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
            if(barSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                barSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (gamblingSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                gamblingSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (brothelSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                brothelSlots[i].SetStatus(ActivitySlotStatus.Available);
        }
        TownManager.BuildingWindowActive = false;
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
