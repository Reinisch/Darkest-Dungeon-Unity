using UnityEngine;
using System.Collections.Generic;

public class SanitariumWindow : UpgradableBuildingWindow
{
    [SerializeField]
    private List<TreatmentHeroSlot> quirkSlots;
    [SerializeField]
    private List<TreatmentHeroSlot> diseaseSlots;
    [SerializeField]
    private SanitariumQuirkWindow quirkWindow;
    [SerializeField]
    private SanitariumDiseaseWindow diseaseWindow;

    protected override BuildingType BuildingType { get { return BuildingType.Sanitarium; } }
    private Sanitarium Sanitarium { get; set; }

    public override void Initialize()
    {
        base.Initialize();

        Sanitarium = DarkestDungeonManager.Campaign.Estate.Sanitarium;

        quirkWindow.Initialize();
        diseaseWindow.Initialize();

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
        base.UpdateUpgradeTrees(afterPurchase);

        for (int i = 0; i < 3; i++)
        {
            quirkSlots[i].UpdateSlot();
            diseaseSlots[i].UpdateSlot();
        }

        if(quirkWindow.isActiveAndEnabled)
            quirkWindow.UpdateHeroOverview();
        if(diseaseWindow.isActiveAndEnabled)
            diseaseWindow.UpdateHeroOverview();
    }

    public override void WindowOpened()
    {
        if (!EstateSceneManager.Instanse.TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            EstateSceneManager.Instanse.TownManager.BuildingWindowActive = true;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "sanitarium");
            DarkestSoundManager.PlayOneShot("event:/town/enter_sanitarium");

            EstateSceneManager.Instanse.RosterPanel.EventHeroSlotBeginDragging += RosterPanelHeroSlotBeginDragging;
            EstateSceneManager.Instanse.RosterPanel.EventHeroSlotEndDragging += RosterPanelHeroSlotEndDragging;
        }
    }

    public override void WindowClosed()
    {
        EstateSceneManager.Instanse.RosterPanel.EventHeroSlotBeginDragging -= RosterPanelHeroSlotBeginDragging;
        EstateSceneManager.Instanse.RosterPanel.EventHeroSlotEndDragging -= RosterPanelHeroSlotEndDragging;

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

        base.WindowClosed();
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

    private void TreatmentHeroSlotButtonClicked(TreatmentHeroSlot slot)
    {
        if (slot.TreatmentSlot.Status == ActivitySlotStatus.Paid)
        {
            EstateSceneManager.Instanse.TownManager.GetHeroSlot(slot.TreatmentSlot.Hero).SetStatus(HeroStatus.Available);
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