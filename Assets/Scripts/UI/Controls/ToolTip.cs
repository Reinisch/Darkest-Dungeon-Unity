using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ToolTip : MonoBehaviour
{
    public RectTransform bounds;
    public RectTransform textRectTransform;
    public LayoutElement layoutElement;
    public VerticalLayoutGroup layoutGroup;
    public RectTransform SenderRect { get; set; }
    public Text text;

    IEnumerator tipCoroutine;

    void FixedUpdate()
    {
        if (bounds.gameObject.activeSelf && (SenderRect == null || !SenderRect.gameObject.activeSelf))
        {
            ToolTipManager.Instanse.Hide();
        }
    }

    public bool IsAlreadyTipping { get; set; }
    public string CurrentTip { get; set; }
    public float TipDelay { get; set; }
    public RectTransform RectTransform { get; set; }

    IEnumerator TipDelayer(float delay)
    {
        bounds.gameObject.SetActive(false);
        yield return new WaitForSeconds(delay);
        text.text = CurrentTip;
        yield return new WaitForEndOfFrame();
        bounds.gameObject.SetActive(true);
        yield break;
    }

    public virtual void Initialize()
    {
        IsAlreadyTipping = false;
        RectTransform = GetComponent<RectTransform>();
    }
    public void Show(RectTransform senderRect)
    {
        if (bounds.gameObject.activeSelf && SenderRect == senderRect)
        {
            text.text = CurrentTip;
            return;
        }
        else
            SenderRect = senderRect;

        if (IsAlreadyTipping)
        {
            StopCoroutine(tipCoroutine);
            tipCoroutine = TipDelayer(TipDelay);
            StartCoroutine(tipCoroutine);
        }
        else
        {
            gameObject.SetActive(true);
            IsAlreadyTipping = true;
            tipCoroutine = TipDelayer(TipDelay);
            StartCoroutine(tipCoroutine);
        }
    }
    public void Hide()
    {
        if (tipCoroutine != null)
        {
            StopCoroutine(tipCoroutine);
            bounds.gameObject.SetActive(false);
            IsAlreadyTipping = false;
            gameObject.SetActive(false);
        }
    }
    public void UpdateSize(ToolTipSize size)
    {
        switch(size)
        {
            case ToolTipSize.Normal:
                layoutElement.flexibleWidth = 320;
                layoutElement.preferredWidth = 320;
                break;
            case ToolTipSize.Small:
                layoutElement.flexibleWidth = 220;
                layoutElement.preferredWidth = 220;
                break;
        }
    }
}
