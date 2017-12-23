public class QuestDeathDoorData : IQuestData
{
    public int Amount { private get; set; }

    public string GetDataString(string goalType)
    {
        string format = LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n";

        return string.Format(format, Amount);
    }

    public bool IsQuestCompleted()
    {
        return true;
    }
}