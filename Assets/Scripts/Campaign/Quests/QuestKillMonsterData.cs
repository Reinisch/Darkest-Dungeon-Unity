using System.Collections.Generic;

public class QuestKillMonsterData : IQuestData
{
    public List<string> MonsterNameIds { get; set; }
    public int Amount { private get; set; }

    public string GetDataString(string goalType)
    {
        string format = LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n";

        return string.Format(format, Amount, LocalizationManager.GetString("str_monstername_" + MonsterNameIds[0]));
    }

    public bool IsQuestCompleted()
    {
        foreach (var monsterId in MonsterNameIds)
            if (RaidSceneManager.Raid.KilledMonsters.Contains(monsterId))
                return true;

        return false;
    }
}