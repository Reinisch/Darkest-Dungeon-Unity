using System.Collections.Generic;

public class Sanitarium : Building
{
    public override string Name { get { return "sanitarium"; } }
    public override BuildingType Type { get { return BuildingType.Sanitarium; } }
    public QuirkTreatmentActivity QuirkActivity { get; set; }
    public DiseaseTreatmentActivity DiseaseActivity { get; set; }

    public void ProvideActivity()
    {
        QuirkActivity.ProvideActivity();
        DiseaseActivity.ProvideActivity();
    }

    public override void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        QuirkActivity.InitializeActivity(purchases);
        DiseaseActivity.InitializeActivity(purchases);
    }

    public override void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        QuirkActivity.UpdateActivity(purchases);
        DiseaseActivity.UpdateActivity(purchases);
    }

    public override List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        var upgrades = QuirkActivity.GetUpgrades(treeId,code);
        upgrades.AddRange(DiseaseActivity.GetUpgrades(treeId, code));
        return upgrades;
    }
}