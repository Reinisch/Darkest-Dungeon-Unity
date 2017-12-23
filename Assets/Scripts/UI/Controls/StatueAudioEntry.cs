using UnityEngine;
using UnityEngine.UI;

public class StatueAudioEntry : MonoBehaviour
{
    [SerializeField]
    private Sprite lockIcon;
    [SerializeField]
    private Sprite playIcon;
    [SerializeField]
    private Image statusIcon;
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Text description;

    [SerializeField]
    private string audioEntryId;
    [SerializeField]
    private string plotCondition;

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
