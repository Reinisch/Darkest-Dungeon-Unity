using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum CompletionAction { Waiting, Return, Continue }

public class QuestCompletionWindow : MonoBehaviour
{
    public Animator completionAnimator;
    public SkeletonAnimation completionCrest;
    public Text completionText;
    public Button returnButton;
    public Button continueButton;

    public CompletionAction Action { get; set; }

    public void Appear()
    {
        completionCrest.gameObject.SetActive(true);
        completionCrest.state.SetAnimation(0, "appear", false);
        completionCrest.state.AddAnimation(0, "idle", true, 0.4f);
    }
    public void Disappear()
    {
        completionAnimator.SetBool("IsClosed", true);
        completionCrest.state.SetAnimation(0, "disappear", false);
    }
    public void Disappeared()
    {
        ToolTipManager.Instanse.Hide();
        completionCrest.gameObject.SetActive(false);
    }

    public void ReturnButtonClick()
    {
        Action = CompletionAction.Return;
        Disappear();
    }
    public void ContinueButtonClick()
    {
        Action = CompletionAction.Continue;
        Disappear();
    }
}
