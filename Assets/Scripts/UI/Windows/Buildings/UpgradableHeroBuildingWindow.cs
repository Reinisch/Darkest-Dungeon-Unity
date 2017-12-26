using UnityEngine;
using UnityEngine.UI;

public class UpgradableHeroBuildingWindow : UpgradableBuildingWindow
{
    [SerializeField]
    private BuildingType buildingType;
    [SerializeField]
    private Text dragHeroLabel;
    [SerializeField]
    private HeroOverviewWindow heroOverview;
    [SerializeField]
    private HeroObserverSlot heroSlot;

    protected override BuildingType BuildingType { get { return buildingType; } }

    private void Awake()
    {
        heroSlot.EventHeroDropped += HeroObserverSlotHeroDropped;
        heroSlot.EventHeroRemoved += HeroObserverSlotHeroRemoved;
    }

    public override void Initialize()
    {
        base.Initialize();

        heroOverview.Initialize();
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        base.UpdateUpgradeTrees(afterPurchase);
        
        heroOverview.UpdateHeroOverview();
    }

    public override void WindowOpened()
    {
        base.WindowOpened();

        heroSlot.ClearSlot();
    }

    public override void WindowClosed()
    {
        heroSlot.ClearSlot();

        base.WindowClosed();
    }

    private void HeroObserverSlotHeroDropped(Hero hero)
    {
        heroOverview.gameObject.SetActive(true);
        heroOverview.LoadHeroOverview(hero);
        dragHeroLabel.enabled = false;
    }

    private void HeroObserverSlotHeroRemoved(Hero hero)
    {
        heroOverview.gameObject.SetActive(false);
        heroOverview.ResetWindow();
        dragHeroLabel.enabled = true;
    }
}
