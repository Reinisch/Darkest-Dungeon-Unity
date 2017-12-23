using UnityEngine;
using UnityEngine.EventSystems;

public class QuickParameterTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private RectTransform tipTarget;
    [SerializeField]
    private string tipEntry;
    [SerializeField]
    private ToolTipSize tipSize;
    [SerializeField]
    private ToolTipStyle tipStyle;

    private int ParamCount { get; set; }
    private float ParamOne { get; set; }
    private float ParamTwo { get; set; }

    public void SetParams(float paramOne)
    {
        ParamCount = 1;
        ParamOne = paramOne;
    }

    public void SetParams(float paramOne, float paramTwo)
    {
        ParamCount = 2;
        ParamOne = paramOne;
        ParamTwo = paramTwo;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(ParamCount == 1)
            ToolTipManager.Instanse.Show(string.Format(LocalizationManager.GetString(tipEntry), ParamOne), tipTarget, tipStyle, tipSize);
        else if (ParamCount == 2)
            ToolTipManager.Instanse.Show(string.Format(LocalizationManager.GetString(tipEntry), ParamOne, ParamTwo), tipTarget, tipStyle, tipSize);
        else
            ToolTipManager.Instanse.Show(LocalizationManager.GetString(tipEntry), tipTarget, tipStyle, tipSize);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
