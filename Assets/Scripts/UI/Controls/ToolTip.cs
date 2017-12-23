using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolTip : MonoBehaviour
{
    [SerializeField]
    private RectTransform bounds;
    [SerializeField]
    private LayoutElement layoutElement;
    [SerializeField]
    private VerticalLayoutGroup layoutGroup;
    [SerializeField]
    private Text text;

    public VerticalLayoutGroup LayoutGroup { get { return layoutGroup; } }
    public RectTransform RectTransform { get; private set; }
    public RectTransform SenderRect { get; private set; }
    public string CurrentTip { private get; set; }
    public float TipDelay { private get; set; }
    private bool IsAlreadyTipping { get; set; }

    private IEnumerator tipCoroutine;

    private void FixedUpdate()
    {
        if (bounds.gameObject.activeSelf && (SenderRect == null || !SenderRect.gameObject.activeSelf))
        {
            ToolTipManager.Instanse.Hide();
        }
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

    private IEnumerator TipDelayer(float delay)
    {
        bounds.gameObject.SetActive(false);
        yield return new WaitForSeconds(delay);
        text.text = CurrentTip;
        yield return new WaitForEndOfFrame();
        bounds.gameObject.SetActive(true);
    }
}
