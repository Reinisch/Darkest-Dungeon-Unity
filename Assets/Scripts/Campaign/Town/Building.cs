using System.Collections.Generic;

public enum BuildingType { Abbey, Tavern, Sanitarium, Blacksmith, Guild, CampingTrainer, NomadWagon, StageCoach, Graveyard, Statue }

public abstract class Building
{
    public abstract string Name { get; }
    public abstract BuildingType Type { get; }

    public virtual void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        Reset();
    }

    public virtual void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        Reset();
    }

    public virtual List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        return null;
    }

    protected virtual void Reset()
    {
    }
}
