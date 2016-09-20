using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum QuestVisitType { Explore, Battle }

public class QuestTutorialData : IQuestData
{
    public string FinalRoomId { get; set; }

    public string GetDataString(string goalType)
    {
        string format = LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n";

        return string.Format(format, FinalRoomId);
    }

    public bool IsQuestCompleted()
    {
        if(RaidSceneManager.Raid.Dungeon.Rooms[FinalRoomId].BattleEncounter.Cleared)
            return true;
        return false;
    }
}

public class QuestKillMonsterData : IQuestData
{
    public List<string> MonsterNameIds { get; set; }
    public int Amount { get; set;}

    public string GetDataString(string goalType)
    {
        string format = LocalizationManager.GetString("town_quest_goal_start_plural_" + goalType) + "\n";

        return string.Format(format, Amount, LocalizationManager.GetString("str_monstername_" + MonsterNameIds[0]));
    }

    public bool IsQuestCompleted()
    {
        foreach (var monsterId in MonsterNameIds)
            if (!RaidSceneManager.Raid.KilledMonsters.Contains(monsterId))
                return false;

        return true;
    }
}

public class QuestVisitedData : IQuestData
{
    public QuestVisitType Type { get; set; }
    public int Amount { get; set; }
    public float PercenageExplored { get; set; }

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
        if (RaidSceneManager.Raid.InvestigatedCurios.FindAll(curioId => curioId == CurioName).Count >= Amount)
                return true;

        return false;
    }
}

public class QuestTraitData : IQuestData
{
    public bool IsAffliction { get; set; }
    public bool IsVirtue { get; set; }
    public int Amount { get; set; }

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

public class QuestDeathDoorData : IQuestData
{
    public int Amount { get; set; }

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
