using UnityEngine;
using UnityEngine.UI;

public enum LogGoalType { Plot, Roster }

public class LogQuestGoal : MonoBehaviour
{
    public Image checkBox;
    public Image checkIcon;
    public Text goalText;
    public LogGoalType goalType;
    public string goalInfo;

    public void UpdateInfo(PlotQuest plotQuest)
    {
        goalType = LogGoalType.Plot;
        goalInfo = plotQuest.Id;
        if (DarkestDungeonManager.Campaign.CompletedPlot.Contains(plotQuest.Id))
            checkIcon.enabled = true;
        else
            checkIcon.enabled = false;
        goalText.text = LocalizationManager.GetString("str_caretaker_goal_" + plotQuest.Id);
    }
    public void UpdateInfo(HeroClass heroClass)
    {
        goalType = LogGoalType.Roster;
        goalInfo = heroClass.StringId;
        if (DarkestDungeonManager.Campaign.CompletedPlot.Contains(heroClass.StringId))
            checkIcon.enabled = true;
        else
            checkIcon.enabled = false;
        goalText.text = string.Format(LocalizationManager.GetString("str_caretaker_goal_hero_resolve"),
            LocalizationManager.GetString("hero_class_name_" + heroClass.StringId));
    }
}
