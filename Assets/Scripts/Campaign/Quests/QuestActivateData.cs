public class QuestActivateData : IQuestData
{
    public string CurioName { get; set; }
    public int Amount { get; set; }

    public string GetDataString(string goalType)
    {
        string format = Amount > 1 ? LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n" :
            LocalizationManager.GetString("town_quest_goal_start_single_" + goalType) + "\n";

        return string.Format(format, Amount, LocalizationManager.GetString("str_curio_title_" + CurioName));
    }

    public bool IsQuestCompleted()
    {
        return RaidSceneManager.Raid.InvestigatedCurios.FindAll(curioId => curioId == CurioName).Count >= Amount;
    }
}