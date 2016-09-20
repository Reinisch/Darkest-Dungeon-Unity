using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class QuirkSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text quirkText;
    public Image lockedIcon;
    public ReplacedQuirkIcon replacedIcon;

    public RectTransform RectTransform { get; set; }
    public bool HasQuirk { get; set; }
    public QuirkInfo QuirkInfo { get; set; }

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void UpdateQuirk(QuirkInfo quirkInfo)
    {
        QuirkInfo = quirkInfo;

        if (QuirkInfo != null)
        {
            HasQuirk = true;
            quirkText.text = LocalizationManager.GetString("str_quirk_name_" + QuirkInfo.Quirk.Id);
            if (quirkInfo.IsLocked)
                lockedIcon.enabled = true;
            else
                lockedIcon.enabled = false;

            if(replacedIcon != null)
            {
                if (QuirkInfo.IsReplaced)
                    replacedIcon.gameObject.SetActive(true);
                else
                    replacedIcon.gameObject.SetActive(false);
            }
            
            gameObject.SetActive(true);
        }
        else
            ResetSlot();    
    }
    public void ResetSlot()
    {
        HasQuirk = false;
        QuirkInfo = null;
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(QuirkInfo != null)
            ToolTipManager.Instanse.Show(QuirkInfo.Quirk.ToolTip(), eventData, RectTransform, ToolTipStyle.FromTop, ToolTipSize.Normal);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
