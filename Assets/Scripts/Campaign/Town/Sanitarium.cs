using System.Collections.Generic;

public class Sanitarium : Building
{
    public QuirkTreatmentActivity QuirkActivity { get; set; }
    public DiseaseTreatmentActivity DiseaseActivity { get; set; }

    public void InitializeBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        QuirkActivity.InitializeActivity(purchases);
        DiseaseActivity.InitializeActivity(purchases);
    }
    public void ProvideActivity()
    {
        QuirkActivity.ProvideActivity();
        DiseaseActivity.ProvideActivity();
    }
    public void UpdateBuilding(Dictionary<string, UpgradePurchases> purchases)
    {
        QuirkActivity.UpdateActivity(purchases);
        DiseaseActivity.UpdateActivity(purchases);
    }

    public List<ITownUpgrade> GetUpgrades(string treeId, string code)
    {
        var upgrades = QuirkActivity.GetUpgrades(treeId,code);
        upgrades.AddRange(DiseaseActivity.GetUpgrades(treeId, code));
        return upgrades;
    }
}