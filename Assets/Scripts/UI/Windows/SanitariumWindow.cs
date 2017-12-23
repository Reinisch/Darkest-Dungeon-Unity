using UnityEngine;
using System.Collections.Generic;

public class SanitariumWindow : BuildingWindow
{
    [SerializeField]
    private UpgradeButton upgradeSwitch;
    [SerializeField]
    private UpgradeWindow upgradeWindow;
    [SerializeField]
    private List<TreatmentHeroSlot> quirkSlots;
    [SerializeField]
    private List<TreatmentHeroSlot> diseaseSlots;
    [SerializeField]
    private SanitariumQuirkWindow quirkWindow;
    [SerializeField]
    private SanitariumDiseaseWindow diseaseWindow;

    public override TownManager TownManager { get; set; }
    private Sanitarium Sanitarium { get; set; }

    public override void Initialize()
    {
        quirkWindow.Initialize(TownManager);
        diseaseWindow.Initialize(TownManager);
        Sanitarium = DarkestDungeonManager.Campaign.Estate.Sanitarium;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Sanitarium);
        upgradeWindow.UpgradedValue.text = Mathf.RoundToInt(ratio * 100) + "%";

        foreach (var tree in upgradeWindow.UpgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.TreeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.Upgrades.Count; i++)
            {
                tree.Upgrades[i].Tree = DarkestDungeonManager.Data.UpgradeTrees[tree.TreeId];
                tree.Upgrades[i].UpgradeInfo = currentUpgrades[i];
                tree.Upgrades[i].TownUpgrades = Sanitarium.GetUpgrades(tree.TreeId, currentUpgrades[i].Code);
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
            quirkSlots[i].Initialize(Sanitarium.QuirkActivity.TreatmentSlots[i]);
            quirkSlots[i].EventHeroDropped += TreatmentHeroSlotQuirkHeroDropped;
            quirkSlots[i].EventHeroRemoved += TreatmentHeroSlotQuirkHeroRemoved;
            quirkSlots[i].EventTreatmentButtonClick += TreatmentHeroSlotButtonClicked;
            diseaseSlots[i].Initialize(Sanitarium.DiseaseActivity.TreatmentSlots[i]);
            diseaseSlots[i].EventHeroDropped += TreatmentHeroSlotDiseaseHeroDropped;
            diseaseSlots[i].EventHeroRemoved += TreatmentHeroSlotDiseaseHeroRemoved;
            diseaseSlots[i].EventTreatmentButtonClick += TreatmentHeroSlotButtonClicked;
        }
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        Sanitarium.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Sanitarium);
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
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
        }
        if(quirkWindow.isActiveAndEnabled)
        {
            quirkWindow.UpdateHeroOverview();
        }
        if(diseaseWindow.isActiveAndEnabled)
        {
            diseaseWindow.UpdateHeroOverview();
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
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "sanitarium");
            DarkestSoundManager.PlayOneShot("event:/town/enter_sanitarium");
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
            if (quirkSlots[i].TreatmentSlot.Status == ActivitySlotStatus.Checkout)
            {
                quirkSlots[i].SetStatus(ActivitySlotStatus.Available);
                quirkWindow.ResetWindow();
            }
            if (diseaseSlots[i].TreatmentSlot.Status == ActivitySlotStatus.Checkout)
            {
                diseaseSlots[i].SetStatus(ActivitySlotStatus.Available);
                diseaseWindow.ResetWindow();
            }
        }
        TownManager.BuildingWindowActive = false;
        DarkestSoundManager.PlayOneShot("event:/ui/town/building_zoomout");
    }

    public void UpdateSlots()
    {
        Sanitarium.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);

        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
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

    private void TreatmentHeroSlotQuirkHeroDropped(TreatmentHeroSlot slot)
    {
        quirkWindow.LoadHeroOverview(slot);
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].ButtonIcon.gameObject.SetActive(false);
            diseaseSlots[i].ButtonIcon.gameObject.SetActive(false);
        }
    }

    private void TreatmentHeroSlotQuirkHeroRemoved(TreatmentHeroSlot slot)
    {
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
        }
        quirkWindow.ResetWindow();
    }

    private void TreatmentHeroSlotDiseaseHeroDropped(TreatmentHeroSlot slot)
    {
        diseaseWindow.LoadHeroOverview(slot);
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].ButtonIcon.gameObject.SetActive(false);
            diseaseSlots[i].ButtonIcon.gameObject.SetActive(false);
        }
    }

    private void TreatmentHeroSlotDiseaseHeroRemoved(TreatmentHeroSlot slot)
    {
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
        }
        diseaseWindow.ResetWindow();
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

    private void TreatmentHeroSlotButtonClicked(TreatmentHeroSlot slot)
    {
        if (slot.TreatmentSlot.Status == ActivitySlotStatus.Paid)
        {
            TownManager.GetHeroSlot(slot.TreatmentSlot.Hero).SetStatus(HeroStatus.Available);
            slot.SetStatus(ActivitySlotStatus.Available);
        }
    }

    private void RosterPanelHeroSlotBeginDragging(HeroSlot heroSlot)
    {
        if (quirkWindow.isActiveAndEnabled)
            quirkWindow.ResetWindow();
        if (diseaseWindow.isActiveAndEnabled)
            diseaseWindow.ResetWindow();

        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateHeroAvailable();
            diseaseSlots[i].UpdateHeroAvailable();
            quirkSlots[i].ButtonIcon.gameObject.SetActive(false);
            diseaseSlots[i].ButtonIcon.gameObject.SetActive(false);
        }
    }

    private void RosterPanelHeroSlotEndDragging(HeroSlot heroSlot)
    {
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
            if (quirkWindow.isActiveAndEnabled || diseaseWindow.isActiveAndEnabled)
            {
                quirkSlots[i].ButtonIcon.gameObject.SetActive(false);
                diseaseSlots[i].ButtonIcon.gameObject.SetActive(false);
            }
        }
    }
}