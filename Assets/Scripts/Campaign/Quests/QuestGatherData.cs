public class QuestGatherData : IQuestData
{
    public string CurioName { get; set; }
    public ItemDefinition Item { get; set; }

    public string GetDataString(string goalType)
    {
        string format = Item.Amount > 1 ? LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n" :
            LocalizationManager.GetString("town_quest_goal_start_single_" + goalType) + "\n";

        return string.Format(format, Item.Amount, LocalizationManager.GetString("str_inventory_title_quest_item" + Item.Id));
    }

    public bool IsQuestCompleted()
    {
        return RaidSceneManager.Inventory.ContainsEnoughItems(Item);
    }
}