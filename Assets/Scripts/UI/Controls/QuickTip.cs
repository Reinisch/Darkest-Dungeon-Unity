using UnityEngine;
using UnityEngine.EventSystems;

public class QuickTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform tipTarget;
    public string tipEntry;
    public ToolTipSize tipSize;
    public ToolTipStyle tipStyle;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Show(LocalizationManager.GetString(tipEntry), eventData, tipTarget, tipStyle, tipSize);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
