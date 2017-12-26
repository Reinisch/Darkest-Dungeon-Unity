using UnityEngine;
using System.Collections.Generic;

public class AbbeyWindow : UpgradableBuildingWindow
{
    [SerializeField]
    private List<TownHeroSlot> cloisterSlots;
    [SerializeField]
    private List<TownHeroSlot> transeptSlots;
    [SerializeField]
    private List<TownHeroSlot> penanceSlots;

    protected override BuildingType BuildingType { get { return BuildingType.Abbey; } }
    private Abbey Abbey { get; set; }

    public override void Initialize()
    {
        base.Initialize();

        Abbey = DarkestDungeonManager.Campaign.Estate.Abbey;

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
        base.UpdateUpgradeTrees(afterPurchase);

        for (int i = 0; i < 3; i++)
        {
            cloisterSlots[i].UpdateSlot();
            transeptSlots[i].UpdateSlot();
            penanceSlots[i].UpdateSlot();
        }
    }

    public override void WindowOpened()
    {
        if (!EstateSceneManager.Instanse.TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            EstateSceneManager.Instanse.TownManager.BuildingWindowActive = true;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "abbey");
            DarkestSoundManager.PlayOneShot("event:/town/enter_abbey");

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
            if (cloisterSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                cloisterSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (transeptSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                transeptSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (penanceSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                penanceSlots[i].SetStatus(ActivitySlotStatus.Available);
        }

        base.WindowClosed();
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

    private void TownHeroSlotTreatmentButtonClicked(TownHeroSlot slot)
    {
        if (slot.ActivitySlot.Status == ActivitySlotStatus.Checkout)
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayGold(slot.ActivitySlot.BaseCost))
            {
                DarkestDungeonManager.Campaign.Estate.RemoveGold(slot.ActivitySlot.BaseCost);
                EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
                EstateSceneManager.Instanse.CurrencyPanel.CurrencyDecreased("gold");
                EstateSceneManager.Instanse.TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Abbey);
                slot.SetStatus(ActivitySlotStatus.Paid);
                DarkestSoundManager.PlayOneShot("event:/town/abbey_" + slot.ActivityName);
            }
        }
        else if (slot.ActivitySlot.Status == ActivitySlotStatus.Paid)
        {
            EstateSceneManager.Instanse.TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Available);
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
