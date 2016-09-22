using UnityEngine;
using UnityEngine.UI;

public enum CampUsageResultType { Wait, Skill, Rest }

public class ScrollCampEvent : MonoBehaviour
{
    public Text title;
    public Text description;
    public Text timeAmount;
    public Animator campAnimator;
    public Button restButton;

    public CampUsageResultType ActionType { get; set; }
    public FormationUnit SelectedTarget { get; set; }

    public void Initialize()
    {
        
    }
    public void SpendTime(int time)
    {
        RaidSceneManager.Raid.CampingTimeLeft -= time;
        timeAmount.text = RaidSceneManager.Raid.CampingTimeLeft.ToString();
    }
    public void SkillExecuted()
    {
        ActionType = CampUsageResultType.Wait;
        SelectedTarget = null;
    }
    public void RestSelected()
    {
        if (ActionType == CampUsageResultType.Wait)
        {
            ActionType = CampUsageResultType.Rest;
        }
    }

    public void Show()
    {
        campAnimator.SetBool("IsShown", true);
    }
    public void Hide()
    {
        campAnimator.SetBool("IsShown", false);
    }

    public void PrepareCamping()
    {
        ActionType = CampUsageResultType.Wait;
        RaidSceneManager.Raid.CampingTimeLeft = 12;

        timeAmount.text = RaidSceneManager.Raid.CampingTimeLeft.ToString();
        ScrollOpened();
    }
    public void ScrollOpened()
    {
        gameObject.SetActive(true);
    }
    public void ScrollClosed()
    {
        SelectedTarget = null;
        gameObject.SetActive(false);
    }
}