public class QuestVisitedData : IQuestData
{
    public QuestVisitType Type { private get; set; }
    public float PercenageExplored { private get; set; }

    public string GetDataString(string goalType)
    {
        string format = LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n";

        return string.Format(format, PercenageExplored*100);
    }

    public bool IsQuestCompleted()
    {
        if(Type == QuestVisitType.Explore)
        {
            if ((float)RaidSceneManager.Raid.ExploredRoomCount / RaidSceneManager.Raid.Dungeon.Rooms.Count >= PercenageExplored)
                return true;
            return false;
        }
        else if (Type == QuestVisitType.Battle)
        {
            foreach (var room in RaidSceneManager.Raid.Dungeon.Rooms)
                if (room.Value.BattleEncounter != null && room.Value.BattleEncounter.Cleared == false)
                    return false;
            return true;
        }
        return true;
    }
}