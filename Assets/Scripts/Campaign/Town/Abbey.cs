public class Abbey : ActivityBuilding
{
    public override string Name { get { return "abbey"; } }
    public override BuildingType Type { get { return BuildingType.Abbey; } }

    public void UpdateActivitySlots(SaveCampaignData saveData)
    {
        UpdateActivitySlots(saveData.AbbeyActivitySlots);
    }
}