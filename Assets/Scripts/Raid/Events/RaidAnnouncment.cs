using UnityEngine;
using UnityEngine.UI;

public enum AnnouncmentPosition
{
    Top, Left, Right, Bottom
}

public class RaidAnnouncment : MonoBehaviour
{
    [SerializeField]
    private Text title;
    [SerializeField]
    private Animator animator;

    public Animator Animator { get { return animator; } }

    private RectTransform rectTransform;
    private RectTransform RectTransform
    {
        get
        {
            if (rectTransform != null)
                return rectTransform;

            rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    public void ShowAnnouncment(string message, AnnouncmentPosition position = AnnouncmentPosition.Top)
    {
        if (position == AnnouncmentPosition.Top)
            RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0.5f, 0.8f);
        else if (position == AnnouncmentPosition.Bottom)
            RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0.5f, 0.4f);
        else if (position == AnnouncmentPosition.Right)
            RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0.725f, 0.8f);
        else
            RectTransform.anchorMin = RectTransform.anchorMax = new Vector2(0.275f, 0.8f);

        title.text = message;
        gameObject.SetActive(true);
        Animator.SetTrigger("appear");

        ScrollOpened();
    }

    public void HideAnnouncment()
    {
        Animator.SetTrigger("disappear");
        ScrollClosed();
    }

    public void ScrollOpened()
    {
        //if (onScrollOpened != null)
        //    onScrollOpened();
    }

    public void ScrollClosed()
    {
        //if (onScrollClosed != null)
        //    onScrollClosed();
    }

    public void HideCompleted()
    {
        gameObject.SetActive(false);
    }
}