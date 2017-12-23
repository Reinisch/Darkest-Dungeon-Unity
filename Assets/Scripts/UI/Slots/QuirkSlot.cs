using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuirkSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Text quirkText;
    [SerializeField]
    private Image lockedIcon;
    [SerializeField]
    private ReplacedQuirkIcon replacedIcon;

    public QuirkInfo QuirkInfo { get; private set; }
    private RectTransform RectTransform { get; set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void UpdateQuirk(QuirkInfo quirkInfo)
    {
        QuirkInfo = quirkInfo;

        if (QuirkInfo != null)
        {
            quirkText.text = LocalizationManager.GetString("str_quirk_name_" + QuirkInfo.Quirk.Id);
            lockedIcon.enabled = quirkInfo.IsLocked;

            if(replacedIcon != null)
                replacedIcon.gameObject.SetActive(QuirkInfo.IsReplaced);
            
            gameObject.SetActive(true);
        }
        else
            ResetSlot();    
    }

    public void ResetSlot()
    {
        QuirkInfo = null;
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(QuirkInfo != null)
            ToolTipManager.Instanse.Show(QuirkInfo.Quirk.ToolTip(), RectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
