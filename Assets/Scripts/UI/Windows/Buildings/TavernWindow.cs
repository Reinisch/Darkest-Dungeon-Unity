using UnityEngine;
using System.Collections.Generic;

public class TavernWindow : UpgradableBuildingWindow
{
    [SerializeField]
    private List<TownHeroSlot> barSlots;
    [SerializeField]
    private List<TownHeroSlot> gamblingSlots;
    [SerializeField]
    private List<TownHeroSlot> brothelSlots;

    protected override BuildingType BuildingType { get { return BuildingType.Tavern; } }
    private Tavern Tavern { get; set; }

    public override void Initialize()
    {
        base.Initialize();

        Tavern = DarkestDungeonManager.Campaign.Estate.Tavern;

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
        base.UpdateUpgradeTrees(afterPurchase);

        for (int i = 0; i < 3; i++)
        {
            barSlots[i].UpdateSlot();
            gamblingSlots[i].UpdateSlot();
            brothelSlots[i].UpdateSlot();
        }
    }

    public override void WindowOpened()
    {
        if (!EstateSceneManager.Instanse.TownManager.AnyWindowsOpened)
        {
            gameObject.SetActive(true);
            EstateSceneManager.Instanse.TownManager.BuildingWindowActive = true;
            DarkestSoundManager.ExecuteNarration("enter_building", NarrationPlace.Town, "tavern");
            DarkestSoundManager.PlayOneShot("event:/town/enter_tavern");

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
            if(barSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                barSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (gamblingSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                gamblingSlots[i].SetStatus(ActivitySlotStatus.Available);
            if (brothelSlots[i].ActivitySlot.Status == ActivitySlotStatus.Checkout)
                brothelSlots[i].SetStatus(ActivitySlotStatus.Available);
        }

        base.WindowClosed();
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

    private void TownHeroSlotTreatmentButtonClicked(TownHeroSlot slot)
    {
        if (slot.ActivitySlot.Status == ActivitySlotStatus.Checkout)
        {
            if (DarkestDungeonManager.Campaign.Estate.CanPayGold(slot.ActivitySlot.BaseCost))
            {
                DarkestDungeonManager.Campaign.Estate.RemoveGold(slot.ActivitySlot.BaseCost);
                EstateSceneManager.Instanse.CurrencyPanel.UpdateCurrency();
                EstateSceneManager.Instanse.CurrencyPanel.CurrencyDecreased("gold");
                EstateSceneManager.Instanse.TownManager.GetHeroSlot(slot.ActivitySlot.Hero).SetStatus(HeroStatus.Tavern);
                slot.SetStatus(ActivitySlotStatus.Paid);
                DarkestSoundManager.PlayOneShot("event:/town/tavern_" + slot.ActivityName);
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
