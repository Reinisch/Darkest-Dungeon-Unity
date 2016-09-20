using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum ResultWindowState { Items, Heroes }

public class RaidResultWindow : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image background;
    public Image resultFrame;
    public Text resultLabel;
    public Text goalLabel;
    public Text nextButtonLabel;
    public ResultItemWindow itemWindow;
    public ResultHeroWindow heroWindow;

    public Sprite completedFrame;
    public Sprite escapeFrame;
    public Sprite defeatFrame;

    public ResultWindowState State { get; set; }

    public void ProceedToHeroes()
    {
        itemWindow.gameObject.SetActive(false);
        heroWindow.gameObject.SetActive(true);
        heroWindow.PreparePropotions();
        State = ResultWindowState.Heroes;
        nextButtonLabel.text = LocalizationManager.GetString("raid_results_progression_return_to_town");
    }

    public void ProceedToItems()
    {
        if (RaidSceneManager.Raid.Quest.IsPlotQuest)
        {
            PlotQuest plotQuest = (PlotQuest)RaidSceneManager.Raid.Quest;
            goalLabel.text = LocalizationManager.GetString("town_quest_name_" + plotQuest.Id);
        }
        else
        {
            goalLabel.text  = LocalizationManager.GetString("town_quest_name_" 
                + RaidSceneManager.Raid.Quest.Type + "+" + RaidSceneManager.Raid.Quest.Length.ToString()
                + "+" + RaidSceneManager.Raid.Quest.Dungeon + "+" + RaidSceneManager.Raid.Quest.Goal.Id);
        }

        if (DarkestDungeonManager.RaidManager.Status == RaidStatus.Success)
        {
            resultLabel.text = LocalizationManager.GetString("raid_results_quest_result_was_completed");
            resultFrame.sprite = completedFrame;
        }
        else if (DarkestDungeonManager.RaidManager.Status == RaidStatus.Abandon)
        {
            resultLabel.text = LocalizationManager.GetString("raid_results_quest_result_was_not_completed_escape");
            resultFrame.sprite = escapeFrame;
        }
        else if (DarkestDungeonManager.RaidManager.Status == RaidStatus.Defeat)
        {
            resultLabel.text = LocalizationManager.GetString("raid_results_quest_result_was_not_completed_defeat");
            resultFrame.sprite = defeatFrame;
        }

        itemWindow.gameObject.SetActive(true);
        heroWindow.gameObject.SetActive(false);
        itemWindow.PrepareRewards();
        State = ResultWindowState.Items;

        nextButtonLabel.text = LocalizationManager.GetString("raid_results_progression_heroes");
    }

    public void DisableInteraction()
    {
        canvasGroup.blocksRaycasts = false;
    }
    public void EnableInteraction()
    {
        canvasGroup.blocksRaycasts = true;
    }
}
