using UnityEngine;

public class NomadWagonWindow : UpgradableBuildingWindow
{
    [SerializeField]
    private WagonInventory wagonInventory;

    public WagonInventory Inventory { get { return wagonInventory; } }

    protected override BuildingType BuildingType { get { return BuildingType.NomadWagon; } }

    public override void Initialize()
    {
        base.Initialize();

        Inventory.NomadWagon = DarkestDungeonManager.Campaign.Estate.NomadWagon;
        Inventory.UpdateShop();
    }

    public override void UpdateUpgradeTrees(bool afterPurchase = false)
    {
        base.UpdateUpgradeTrees(afterPurchase);

        Inventory.UpdatePrices();
    }
}
