using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class QuickParameterTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform tipTarget;
    public string tipEntry;
    public ToolTipSize tipSize;
    public ToolTipStyle tipStyle;

    public int ParamCount { get; set; }
    public float ParamOne { get; set; }
    public float ParamTwo { get; set; }
    public float ParamThree { get; set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(ParamCount == 1)
            ToolTipManager.Instanse.Show(string.Format(LocalizationManager.GetString(tipEntry), ParamOne),
                eventData, tipTarget, tipStyle, tipSize);
        else if (ParamCount == 2)
            ToolTipManager.Instanse.Show(string.Format(LocalizationManager.GetString(tipEntry), ParamOne, ParamTwo),
                eventData, tipTarget, tipStyle, tipSize);
        else if (ParamCount == 3)
            ToolTipManager.Instanse.Show(string.Format(LocalizationManager.GetString(tipEntry), ParamOne, ParamTwo, ParamThree),
                eventData, tipTarget, tipStyle, tipSize);
        else
            ToolTipManager.Instanse.Show(LocalizationManager.GetString(tipEntry),eventData, tipTarget, tipStyle, tipSize);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
