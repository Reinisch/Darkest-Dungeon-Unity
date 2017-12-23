using UnityEngine;
using System.Collections.Generic;

public class TavernWindow : BuildingWindow
{
    [SerializeField]
    private UpgradeButton upgradeSwitch;
    [SerializeField]
    private UpgradeWindow upgradeWindow;
    [SerializeField]
    private List<TownHeroSlot> barSlots;
    [SerializeField]
    private List<TownHeroSlot> gamblingSlots;
    [SerializeField]
    private List<TownHeroSlot> brothelSlots;

    public override TownManager TownManager { get; set; }
    private Tavern Tavern { get; set; }

    public override void Initialize()
    {
        Tavern = DarkestDungeonManager.Campaign.Estate.Tavern;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Tavern);
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
                    Tavern.Activities.Find(activity => activity.TreeId == tree.TreeId).GetUpgradeByCode(currentUpgrades[i].Code) });
                tree.Upgrades[i].EventClicked += BuildingUpgradeSlotClicked;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.TreeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.Upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }

        for (int i = 0; i < 3; i++)
        {
            barSlots[i].Initialize(Tavern.Activities[0].ActivitySlots[i]);
            barSlots[i].EventTreatmentButtonClicked += TownHeroSlotTreatmentButtonClicked;
            gamblingSlots[i].Initialize(Tavern.Activities[1].ActivitySlots[i]);
            gamblingSlots[i].EventTreatmentButtonClicked += TownHeroSlotTreatmentButtonClicked;
            brothelSlots[i].Initialize(Tavern.Activities[2].ActivitySlots[i]);
            brothelSlots[i].EventTreatmentButtonClicked += TownHeroSlotTreatmentButtonClicked;
        }
    }
    
    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        Tavern.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Tavern);
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
            barSlots[i].UpdateSlot();
            gamblingSlots[i].UpdateSlot();
            brothelSlots[i].UpdateSlot();
        }
    }

    public override void WindowOpened()
    {
        if (!TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            TownManager.BuildingWindowActive = true;
            TownManager.EstateSceneManager.RosterPanel.EventHeroSlotBeginDragging += RosterPanelHeroSlotBeginDragging;
            TownManager.EstateSceneManager.RosterPanel.EventHeroSlotEndDragging += RosterPanelHeroSlotEndDragging;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "tavern");
            DarkestSoundManager.PlayOneShot("event:/town/enter_tavern");
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
            if(barSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                barSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (gamblingSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                gamblingSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (brothelSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                brothelSlots[i].SetStatus(ActivitySlotStatus.Available);
        }
        TownManager.BuildingWindowActive = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/building_zoomout");
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

    private void TownHeroSlotTreatmentButtonClicked(TownHeroSlot slot)
    {
        if (slot.ActivitySlot.Status == ActivitySlotStatus.Checkout)
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayGold(slot.ActivitySlot.BaseCost))
            {
                DarkestDungeonManager.Campaign.Estate.RemoveGold(slot.ActivitySlot.BaseCost);
                TownManager.EstateSceneManager.CurrencyPanel.UpdateCurrency();
                TownManager.EstateSceneManager.CurrencyPanel.CurrencyDecreased("gold");
                TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Tavern);
                slot.SetStatus(ActivitySlotStatus.Paid);
                DarkestSoundManager.PlayOneShot("event:/town/tavern_" + slot.ActivityName);
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

    private void RosterPanelHeroSlotEndDragging(HeroSlot heroSlot)
    {
        for (int i = 0; i < 3; i++)
        {
            barSlots[i].UpdateAvailable();
            gamblingSlots[i].UpdateAvailable();
            brothelSlots[i].UpdateAvailable();
        }
    }
}
