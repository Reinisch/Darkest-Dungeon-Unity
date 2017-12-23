public class QuestTutorialData : IQuestData
{
    public string FinalRoomId { private get; set; }

    public string GetDataString(string goalType)
    {
        string format = LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n";

        return string.Format(format, FinalRoomId);
    }

    public bool IsQuestCompleted()
    {
        return RaidSceneManager.Raid.Dungeon.Rooms[FinalRoomId].BattleEncounter.Cleared;
    }
}