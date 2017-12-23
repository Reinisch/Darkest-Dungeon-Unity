using UnityEngine;
using UnityEngine.UI;

public class RoundIndicator : MonoBehaviour
{
    [SerializeField]
    private SkeletonAnimation indicator;
    [SerializeField]
    private Text roundNumber;
    [SerializeField]
    private CanvasGroup canvasGroup;

    public SkeletonAnimation Indicator { get { return indicator; } }
    public CanvasGroup CanvasGroup { get { return canvasGroup; } }

    private void Awake()
    {
        Indicator.Reset();
        gameObject.SetActive(false);
    }

    public void Appear()
    {
        gameObject.SetActive(true);
        roundNumber.text = "1";
        Indicator.state.SetAnimation(0, "start", false);
        Indicator.state.AddAnimation(0, "idle", true, 1.333f);
    }

    public void UpdateRound(int number)
    {
        roundNumber.text = number.ToString();
        Indicator.state.SetAnimation(0, "update", false);
        Indicator.state.AddAnimation(0, "idle", true, 0.933f);
    }

    public void End()
    {
        roundNumber.text = "";
        Indicator.state.SetAnimation(0, "end", false);
    }

    public void Disappear()
    {
        gameObject.SetActive(false);
    }
}
