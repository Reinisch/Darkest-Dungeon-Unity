using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public enum AnnouncmentPosition
{
    Top, Left, Right, Bottom
}

public class RaidAnnouncment : MonoBehaviour
{
    public Text title;
    public Animator animator;

    //public event ScrollEvent onScrollOpened;
    //public event ScrollEvent onScrollClosed;

    RectTransform rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform != null)
                return rectTransform;
            else
            {
                rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
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
        animator.SetTrigger("appear");

        ScrollOpened();
    }
    public void HideAnnouncment()
    {
        animator.SetTrigger("disappear");
        ScrollClosed();
    }

    public void HideCompleted()
    {
        gameObject.SetActive(false);
    }
}
