using UnityEngine;
using UnityEngine.UI;

public class LogQuestGoal : MonoBehaviour
{
    [SerializeField]
    private Image checkIcon;
    [SerializeField]
    private Text goalText;

    public void UpdateInfo(PlotQuest plotQuest)
    {
        checkIcon.enabled = DarkestDungeonManager.Campaign.CompletedPlot.Contains(plotQuest.Id);
        goalText.text = LocalizationManager.GetString("str_caretaker_goal_" + plotQuest.Id);
    }

    public void UpdateInfo(HeroClass heroClass)
    {
        checkIcon.enabled = DarkestDungeonManager.Campaign.CompletedPlot.Contains(heroClass.StringId);
        goalText.text = string.Format(LocalizationManager.GetString("str_caretaker_goal_hero_resolve"),
            LocalizationManager.GetString("hero_class_name_" + heroClass.StringId));
    }
}
