using UnityEngine;

public enum CompletionAction
{
    Waiting,
    Return,
    Continue
}

public class QuestCompletionWindow : MonoBehaviour
{
    [SerializeField]
    private Animator completionAnimator;
    [SerializeField]
    private SkeletonAnimation completionCrest;

    public CompletionAction Action { get; private set; }

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
