using System;
using UnityEngine;
using UnityEngine.UI;

public class QuestSlot : MonoBehaviour
{
    [SerializeField]
    private Image selectedFrame;
    [SerializeField]
    private Image lengthFrame;
    [SerializeField]
    private Image typeFrame;

    public Quest Quest { get; private set; }

    public bool Selected
    {
        set
        {
            isSelected = value;
            Animator.SetBool("selected", isSelected);
            selectedFrame.gameObject.SetActive(isSelected);
            selectedFrame.rectTransform.localPosition = RectTransform.localPosition;
        }
    }

    private RectTransform RectTransform { get; set; }
    private Animator Animator { get; set; }

    private bool isSelected;

    public event Action<QuestSlot> EventQuestSelected;

    public void Initialize()
    {
        Animator = GetComponent<Animator>();
        RectTransform = GetComponent<RectTransform>();
    }

    public void UpdateQuest(Quest quest)
    {
        DarkestDatabase database = DarkestDungeonManager.Data;
        Quest = quest;
        if(Quest.IsPlotQuest)
        {
            lengthFrame.sprite = database.Sprites["quest_select_length_plot_" + quest.Length.ToString()];
            typeFrame.sprite = database.Sprites["quest_select_" + quest.Type + "_" + quest.Difficulty.ToString()];
        }
        else
        {
            lengthFrame.sprite = database.Sprites["quest_select_length_generated_" + quest.Length.ToString()];
            typeFrame.sprite = database.Sprites["quest_select_" + quest.Type + "_" + quest.Difficulty.ToString()];
        }
    }

    public void QuestButtonClicked()
    {
        DarkestSoundManager.PlayOneShot("event:/ui/town/dungeon_select");

        if (EventQuestSelected != null)
            EventQuestSelected(this);
    }
}
