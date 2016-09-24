using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SanitariumWindow : BuildingWindow
{
    public Text buildingLabel;
    public Text treatmentLabel;
    public Text treatmentDescription;
    public Text medicalLabel;
    public Text medicalDescription;

    public UpgradeButton upgradeSwitch;
    public UpgradeWindow upgradeWindow;

    public Button closeButton;

    public List<TreatmentHeroSlot> quirkSlots;
    public List<TreatmentHeroSlot> diseaseSlots;

    public SanitariumQuirkWindow quirkWindow;
    public SanitariumDiseaseWindow diseaseWindow;

    public override TownManager TownManager { get; set; }
    public Sanitarium Sanitarium { get; private set; }

    void SanitariumWindow_onQuirkHeroDropped(TreatmentHeroSlot slot)
    {
        quirkWindow.LoadHeroOverview(slot);
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].buttonIcon.gameObject.SetActive(false);
            diseaseSlots[i].buttonIcon.gameObject.SetActive(false);
        }
    }
    void SanitariumWindow_onQuirkHeroRemoved(TreatmentHeroSlot slot)
    {
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
        }
        quirkWindow.ResetWindow();
    }
    void SanitariumWindow_onDiseaseHeroDropped(TreatmentHeroSlot slot)
    {
        diseaseWindow.LoadHeroOverview(slot);
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].buttonIcon.gameObject.SetActive(false);
            diseaseSlots[i].buttonIcon.gameObject.SetActive(false);
        }
    }
    void SanitariumWindow_onDiseaseHeroRemoved(TreatmentHeroSlot slot)
    {
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
        }
        diseaseWindow.ResetWindow();
    }
    void SanitariumWindow_onUpgradeClick(BuildingUpgradeSlot slot)
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
    void SanitariumWindow_onTreatmentButtonClick(TreatmentHeroSlot slot)
    {
        if (slot.TreatmentSlot.Status == ActivitySlotStatus.Paid)
        {
            TownManager.GetHeroSlot(slot.TreatmentSlot.Hero).SetStatus(HeroStatus.Available);
            slot.SetStatus(ActivitySlotStatus.Available);
        }
    }

    void rosterPanel_onHeroSlotBeginDragging(HeroSlot heroSlot)
    {
        if (quirkWindow.isActiveAndEnabled)
            quirkWindow.ResetWindow();
        if (diseaseWindow.isActiveAndEnabled)
            diseaseWindow.ResetWindow();

        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateHeroAvailable(heroSlot.Hero);
            diseaseSlots[i].UpdateHeroAvailable(heroSlot.Hero);
            quirkSlots[i].buttonIcon.gameObject.SetActive(false);
            diseaseSlots[i].buttonIcon.gameObject.SetActive(false);
        }
    }
    void rosterPanel_onHeroSlotEndDragging(HeroSlot heroSlot)
    {
        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
            if(quirkWindow.isActiveAndEnabled || diseaseWindow.isActiveAndEnabled)
            {
                quirkSlots[i].buttonIcon.gameObject.SetActive(false);
                diseaseSlots[i].buttonIcon.gameObject.SetActive(false);
            }
        }
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
    public override void Initialize()
    {
        quirkWindow.Initialize(TownManager);
        diseaseWindow.Initialize(TownManager);
        Sanitarium = DarkestDungeonManager.Campaign.Estate.Sanitarium;
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Sanitarium);
        upgradeWindow.upgradedValue.text = Mathf.RoundToInt(ratio * 100).ToString() + "%";

        foreach (var tree in upgradeWindow.upgradeTrees)
        {
            var currentUpgrades = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId].Upgrades;
            int lastPurchaseIndex = -1;
            for (int i = 0; i < tree.upgrades.Count; i++)
            {
                tree.upgrades[i].Tree = DarkestDungeonManager.Data.UpgradeTrees[tree.treeId];
                tree.upgrades[i].UpgradeInfo = currentUpgrades[i];
                tree.upgrades[i].TownUpgrades = Sanitarium.GetUpgrades(tree.treeId, currentUpgrades[i].Code);
                tree.upgrades[i].onClick += SanitariumWindow_onUpgradeClick;
                var status = DarkestDungeonManager.Campaign.Estate.GetUpgradeStatus(tree.treeId, currentUpgrades[i]);
                TownManager.UpdateUpgradeSlot(status, tree.upgrades[i]);
                if (status == UpgradeStatus.Purchased)
                    lastPurchaseIndex = i;
            }
            tree.UpdateConnector(lastPurchaseIndex);
        }

        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].Initialize(Sanitarium.QuirkActivity.TreatmentSlots[i]);
            quirkSlots[i].onHeroDropped += SanitariumWindow_onQuirkHeroDropped;
            quirkSlots[i].onHeroRemoved += SanitariumWindow_onQuirkHeroRemoved;
            quirkSlots[i].onTreatmentButtonClick += SanitariumWindow_onTreatmentButtonClick;
            diseaseSlots[i].Initialize(Sanitarium.DiseaseActivity.TreatmentSlots[i]);
            diseaseSlots[i].onHeroDropped += SanitariumWindow_onDiseaseHeroDropped;
            diseaseSlots[i].onHeroRemoved += SanitariumWindow_onDiseaseHeroRemoved;
            diseaseSlots[i].onTreatmentButtonClick += SanitariumWindow_onTreatmentButtonClick;
        }
    }

    public void UpdateUpgradeTrees()
    {
        Sanitarium.UpdateBuilding(DarkestDungeonManager.Campaign.Estate.TownPurchases);
        float ratio = DarkestDungeonManager.Campaign.Estate.GetBuildingUpgradeRatio(BuildingType.Sanitarium);
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
    }
}