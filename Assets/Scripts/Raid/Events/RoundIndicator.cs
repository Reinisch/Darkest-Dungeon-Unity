using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundIndicator : MonoBehaviour
{
    public SkeletonAnimation indicator;
    public Text roundNumber;
    public CanvasGroup canvasGroup;

    void Awake()
    {
        indicator.Reset();
        gameObject.SetActive(false);
    }

    public void Appear()
    {
        gameObject.SetActive(true);
        roundNumber.text = "1";
        indicator.state.SetAnimation(0, "start", false);
        indicator.state.AddAnimation(0, "idle", true, 1.333f);
    }

    public void UpdateRound(int number)
    {
        roundNumber.text = number.ToString();
        indicator.state.SetAnimation(0, "update", false);
        indicator.state.AddAnimation(0, "idle", true, 0.933f);
    }

    public void End()
    {
        roundNumber.text = "";
        indicator.state.SetAnimation(0, "end", false);
    }

    public void Disappear()
    {
        gameObject.SetActive(false);
    }
}
