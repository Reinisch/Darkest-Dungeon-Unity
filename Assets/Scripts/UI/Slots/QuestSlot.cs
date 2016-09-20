using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void QuestSelectionEvent(QuestSlot questSlot);

public class QuestSlot : MonoBehaviour
{
    bool isSelected = false;
    public bool Selected
    {
        get
        {
            return isSelected; 
        }
        set
        {
            isSelected = value;
            Animator.SetBool("selected", isSelected);
            selectedFrame.gameObject.SetActive(isSelected);
            selectedFrame.rectTransform.localPosition = RectTransform.localPosition;
        }
    }

    public Button questSelectButton;
    public Image selectedFrame;
    public Image lengthFrame;
    public Image typeFrame;

    public RectTransform RectTransform { get; private set; }
    public Animator Animator { get; private set; }
    public Quest Quest { get; set;}

    public event QuestSelectionEvent onQuestSelected;

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
        if (onQuestSelected != null)
            onQuestSelected(this);
    }
}
