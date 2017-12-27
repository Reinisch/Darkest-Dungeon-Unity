public class Tavern : ActivityBuilding
{
    public override string Name { get { return "tavern"; } }
    public override BuildingType Type { get { return BuildingType.Tavern; } }

    public void UpdateActivitySlots(SaveCampaignData saveData)
    {
        UpdateActivitySlots(saveData.TavernActivitySlots);
    }
}