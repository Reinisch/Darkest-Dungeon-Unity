using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatueAudioEntry : MonoBehaviour
{
    public Sprite lockIcon;
    public Sprite playIcon;

    public Image statusIcon;
    public Button playButton;
    public Text description;

    public string audioEntryId;
    public string plotCondition;

    public void UpdateCondition()
    {
        if(DarkestDungeonManager.Campaign.CompletedPlot.Contains(plotCondition))
        {
            playButton.interactable = true;
            statusIcon.sprite = playIcon;
            description.text = LocalizationManager.GetString("str_" + plotCondition + "_audio_line");
        }
        else
        {
            playButton.interactable = false;
            statusIcon.sprite = lockIcon;
            description.text = LocalizationManager.GetString("str_caretaker_goal_" + plotCondition);
        }
    }
    public void PlayButtonClicked()
    {
        DarkestSoundManager.PlayStatueAudioEntry(audioEntryId);
    }
}
