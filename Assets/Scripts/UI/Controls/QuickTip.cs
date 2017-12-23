using UnityEngine;
using UnityEngine.EventSystems;

public class QuickTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private RectTransform tipTarget;
    [SerializeField]
    private string tipEntry;
    [SerializeField]
    private ToolTipSize tipSize;
    [SerializeField]
    private ToolTipStyle tipStyle;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Show(LocalizationManager.GetString(tipEntry), tipTarget, tipStyle, tipSize);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipManager.Instanse.Hide();
    }
}
