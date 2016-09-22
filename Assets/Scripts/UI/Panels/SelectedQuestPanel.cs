using UnityEngine;
using UnityEngine.UI;

public class SelectedQuestPanel : MonoBehaviour
{
    public Text questTitle;
    public Text questDescription;
    public Text questLength;
    public Text campfireAmount;
    public Text questDifficulty;
    public Text questGoals;

    public QuestRewardPanel questRewardPanel;

    public void SetSelectedQuest(Quest quest)
    {
        if (quest.IsPlotQuest)
        {
            PlotQuest plotQuest = (PlotQuest)quest;

            questTitle.text = LocalizationManager.GetString("town_quest_name_" + plotQuest.Id);
            questDescription.text = LocalizationManager.GetString("town_quest_description_" + plotQuest.Id);
            questLength.text = plotQuest.Length.ToString();
            campfireAmount.text = "x" + (quest.Length - 1).ToString();
            questLength.text = LocalizationManager.GetString("town_quest_length_" + plotQuest.Length.ToString());
            questDifficulty.text = LocalizationManager.GetString("town_quest_difficulty_" + plotQuest.Difficulty.ToString());
            questGoals.text = LocalizationManager.GetString("town_quest_goals") + "\n";
            questGoals.text += plotQuest.Goal.QuestData.GetDataString(plotQuest.Goal.Type);
            questRewardPanel.UpdateRewardSlots(quest);
        }
        else
        {
            questTitle.text = LocalizationManager.GetString("town_quest_name_" + quest.Type +
                "+" + quest.Length.ToString() + "+" + quest.Dungeon + "+" + quest.Goal.Id);
            questDescription.text = LocalizationManager.GetString("town_quest_description_" +
                quest.Type + "+" + quest.Length.ToString() + "+" + quest.Dungeon + "+" + quest.Goal.Id);
            questLength.text = quest.Length.ToString();
            campfireAmount.text = "x" + (quest.Length - 1).ToString();
            questLength.text = LocalizationManager.GetString("town_quest_length_" + quest.Length.ToString());
            questDifficulty.text = LocalizationManager.GetString("town_quest_difficulty_" + quest.Difficulty.ToString());
            questGoals.text = LocalizationManager.GetString("town_quest_goals") + "\n";
            questGoals.text += quest.Goal.QuestData.GetDataString(quest.Goal.Type);
            questRewardPanel.UpdateRewardSlots(quest);
        }
    }
}
